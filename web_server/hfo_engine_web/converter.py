import os
import mne
from flask import (
    Blueprint, request, send_from_directory,
    current_app, jsonify
)
from flask_api import status
from hfo_engine_web.db import get_db
from trcio import write_raw_trc
from werkzeug.utils import secure_filename
from pathlib import Path
from .engine import file_extension

TRC_EXTENSION = 'TRC'
EDF_EXTENSION = 'edf'
TRC_MAX_CH_NAME_LEN = 5

converter_bp = Blueprint('converter', __name__, url_prefix='/converter')


#            API and validations         #

@converter_bp.route('/upload_edf', methods=['GET', 'POST'])
def upload_edf():
    from .engine import upload_file
    return upload_file(request)


@converter_bp.route('/suggested_ch_name_mapping/<path:edf_fname>')
def suggested_ch_name_mapping(edf_fname):
    edf_fname = secure_filename(edf_fname)
    abs_edf_fname = os.path.join(current_app.config['EDF_FOLDER'], edf_fname)
    file_exists = os.path.isfile(abs_edf_fname)

    if file_extension(edf_fname) == EDF_EXTENSION and file_exists:
        return jsonify(suggested_mapping=suggested_mapping(abs_edf_fname))
    else:
        return jsonify(error_msg="That file does not exist."), status.HTTP_404_NOT_FOUND


@converter_bp.route('/convert', methods=['POST'])
def convert():
    content = request.get_json(silent=True)

    # Validate file
    edf_fname = secure_filename(content['edf_fname'])
    abs_edf_fname = os.path.join(current_app.config['EDF_FOLDER'], edf_fname)
    file_exists = os.path.isfile(abs_edf_fname)
    if file_extension(edf_fname) == EDF_EXTENSION and file_exists:
        pass
    else:
        return jsonify(error_msg="You must upload the edf prior to do the conversion.",
                       status_code=status.HTTP_404_NOT_FOUND)

    # Validate ch_names_mapping
    user_ch_names_mapping = content['ch_names_mapping']
    raw_edf = mne.io.read_raw_edf(abs_edf_fname, preload=True)
    raw_edf.pick_types(eeg=True, stim=False)

    # Check the ch_name list to be renamed is valid
    if set(user_ch_names_mapping.keys()) != set(raw_edf.ch_names):
        return jsonify(error_msg=("The provided ch_names_mapping must have a definition"
                                  " for (and only for) every channel name in the edf.")), \
               status.HTTP_409_CONFLICT

    # Check the final ch_name list is valid
    if len(set(user_ch_names_mapping.values())) != len(list(user_ch_names_mapping.values())):
        return jsonify(error_msg="The final ch_name list has repetead values."), \
               status.HTTP_409_CONFLICT

    for long_name, short_name in user_ch_names_mapping.items():
        if len(short_name) > TRC_MAX_CH_NAME_LEN:
            return jsonify(error_msg=("TRC channel names length must be less or"
                                      " equal to {}.".format(TRC_MAX_CH_NAME_LEN))), \
                   status.HTTP_409_CONFLICT

    # Create new conversion job
    job_id = current_app.config['JOB_MANAGER'].create_conversion_job(abs_edf_fname,
                                                                     content['ch_names_mapping'],
                                                                     conversion_procedure)

    return jsonify(task_id=job_id)


@converter_bp.route('/download_trc/<path:filename>')
def download_trc(trc_fname):
    trc_fname = secure_filename(trc_fname)
    abs_trc_fname = os.path.join(current_app.config['TRC_FOLDER'], trc_fname)
    file_exists = os.path.isfile(abs_trc_fname)
    if file_extension(trc_fname) == TRC_EXTENSION and file_exists:
        return send_from_directory(current_app.config['TRC_FOLDER'], trc_fname)
    else:
        return jsonify(error_msg="That file does not exist."), status.HTTP_404_NOT_FOUND


# Main Logic

def suggestion_for(ch_name, known_translation):
    if ch_name in known_translation.keys():
        return known_translation[ch_name]
    else:
        return ch_name[-5:]


def suggested_mapping(edf_fname):

    raw_edf = mne.io.read_raw_edf(edf_fname, preload=True)
    raw_edf.pick_types(eeg=True, stim=False)
    translation = dict()
    db = get_db()
    rows = db.execute('SELECT * FROM ch_name_translation').fetchall()
    known_translation = {translation['long_name']: translation['short_name'] for translation in rows}
    for ch_name in raw_edf.ch_names:
        translation[ch_name] = suggestion_for(ch_name, known_translation)
    return translation


def conversion_procedure(edf_fname, ch_names_translation, job_state):
    try:
        job_state.progress.update(1)
        raw_edf = mne.io.read_raw_edf(edf_fname, preload=True)
        job_state.progress.update(10)
        raw_edf.pick_types(eeg=True, stim=False)

        # WARNING
        # Converting unit of data, this is needed because Shennan edfs are not well formed
        # If data comes in micro volts, this information is stored
        # in the info dict in the unit_mul entry as -6. If it is volt, then it is stored as 1.
        # info['chs'][0]['unit'] info['chs'][0]['unit_mul']
        # to_rename = {x: x.split(' ')[1].split('-')[0] for x in raw_edf.ch_names}
        raw_edf._data *= 1e-06  # From micro volts to volts
        job_state.progress.update(30)

        # Channel renaming and persistence of latest mapping
        to_rename = {long: short for long, short in ch_names_translation.items() if long != short}
        raw_edf.rename_channels(to_rename)
        db = get_db()
        for long, short in to_rename.items():
            db.execute(
                'REPLACE INTO ch_name_translation'
                ' VALUES (?,?)',
                (long, short)
            )
        db.commit()
        job_state.progress.update(50)

        # Write the TRC
        trc_fname = Path(edf_fname).stem + '.TRC'
        abs_trc_fname = os.path.join(current_app.config['TRC_FOLDER'], trc_fname)
        write_raw_trc(raw_edf, abs_trc_fname)
        job_state.progress.update(100)
        with job_state.status_code.get_lock():
            job_state.status_code.value = status.HTTP_201_CREATED

    except Exception:
        with job_state.error_msg.get_lock():
            job_state.error_msg.value = "There was an exception running conversion procedure".encode('utf-8')
        with job_state.status_code.get_lock():
            job_state.status_code.value = status.HTTP_500_INTERNAL_SERVER_ERROR
