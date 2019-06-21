import os
from pathlib import Path

from ez_detect import config, hfo_annotate
from flask import (
    Blueprint, request, send_from_directory,
    current_app, jsonify
)
from flask_api import status
from trcio import read_raw_trc
from werkzeug.utils import secure_filename

from .engine import file_extension

TRC_EXTENSION = 'TRC'
EVT_EXTENSION = 'evt'

analyzer_bp = Blueprint('analyzer', __name__, url_prefix='/analyzer')


#            ANALYZER ENDPOINTS             #

@analyzer_bp.route('/upload_trc', methods=['GET', 'POST'])
def upload_trc():
    from .engine import upload_file
    return upload_file(request)


@analyzer_bp.route('/trc_info/<path:trc_fname>', methods=['GET'])
def trc_info(trc_fname):
    trc_fname = secure_filename(trc_fname)
    abs_trc_fname = os.path.join(current_app.config['TRC_FOLDER'], trc_fname)
    file_exists = os.path.isfile(abs_trc_fname)
    if file_extension(trc_fname) == TRC_EXTENSION and file_exists:

        raw_trc = read_raw_trc(abs_trc_fname, preload=False)
        return jsonify(montage_names=montage_names(raw_trc),
                       recording_len_snds=str(duration_snds(raw_trc)))
    else:
        return jsonify(error_msg="That file does not exist."), status.HTTP_404_NOT_FOUND


@analyzer_bp.route('/analyze', methods=['POST'])
def analyze():
    content = request.get_json()
    # Validate file exists
    trc_fname = secure_filename(content['trc_fname'])
    abs_trc_fname = os.path.join(current_app.config['TRC_FOLDER'], trc_fname)

    file_exists = os.path.isfile(abs_trc_fname)

    if file_extension(trc_fname) == TRC_EXTENSION and file_exists:
        pass
    else:
        return jsonify(error_msg="Upload the TRC prior to running an analysis on it."), status.HTTP_404_NOT_FOUND

    raw_trc = read_raw_trc(abs_trc_fname, preload=False)
    str_time = int(content['str_time'])
    stp_time = int(content['stp_time'])
    cycle_time = int(content['cycle_time'])
    sug_montage = content['sug_montage']
    bp_montage = content['bp_montage']

    if str_time < 0 or stp_time < str_time or stp_time > duration_snds(raw_trc):
        return jsonify(error_msg="Time-window is incorrect for the current trc."), status.HTTP_409_CONFLICT
    elif sug_montage not in montage_names(raw_trc):
        return jsonify(error_msg="Suggested montage is not an option for current TRC file."), \
               status.HTTP_409_CONFLICT
    elif bp_montage not in montage_names(raw_trc):
        return jsonify(error_msg="Bipolar montage is not an option for current TRC file."), \
               status.HTTP_409_CONFLICT

    evt_fname = Path(trc_fname).stem + '.evt'
    abs_evt_fname = os.path.join(current_app.config['EVT_FOLDER'], evt_fname)

    try:
        job_id = current_app.config['JOB_MANAGER'].create_analysis_job(abs_trc_fname,
                                                                       abs_evt_fname,
                                                                       str_time,
                                                                       stp_time,
                                                                       cycle_time,
                                                                       sug_montage,
                                                                       bp_montage,
                                                                       analysis_procedure)
        return jsonify(task_id=job_id)

    except AssertionError:
        return jsonify(error_msg=('Server has reached the maximum of posible '
                                  'active analyzer jobs, please try again later.')), \
               status.HTTP_409_CONFLICT


@analyzer_bp.route('/download/evts/<path:evt_fname>')
def download_evt_file(evt_fname):
    evt_fname = secure_filename(evt_fname)
    abs_evt_fname = os.path.join(current_app.config['EVT_FOLDER'], evt_fname)
    file_exists = os.path.isfile(abs_evt_fname)
    if file_extension(evt_fname) == EVT_EXTENSION and file_exists:
        return send_from_directory(current_app.config['EVT_FOLDER'], evt_fname)
    else:
        return jsonify(error_msg="That file does not exist."), status.HTTP_404_NOT_FOUND


# Analyzer Main Logic

def montage_names(raw_trc):
    return list(raw_trc._raw_extras[0]['montages'].keys())


def duration_snds(raw_trc):
    return raw_trc._raw_extras[0]['n_samples'] // raw_trc._raw_extras[0]['sfreq']


def analysis_procedure(trc_fname, evt_fname, str_time, stp_time, cycle_time,
                       sug_montage, bp_montage, job_state, job_manager):
    job_state.progress.update(1)

    paths = config.getAllPaths(trc_fname, evt_fname)
    config.clean_previous_execution()
    try:
        hfo_annotate(paths, str_time, stp_time, cycle_time,
                     sug_montage, bp_montage, progress_notifier=job_state.progress)
        with job_state.status_code.get_lock():
            job_state.status_code.value = status.HTTP_201_CREATED

    except Exception:
        with job_state.error_msg.get_lock():
            job_state.error_msg.value = "hfo_annotate internal error".encode('utf-8')
        with job_state.status_code.get_lock():
            job_state.status_code.value = status.HTTP_500_INTERNAL_SERVER_ERROR

    finally:
        job_manager.on_analysis_finished()
