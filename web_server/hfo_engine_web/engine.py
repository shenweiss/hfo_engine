from flask import (
    Blueprint, flash, g, redirect, render_template, request, url_for, send_from_directory, current_app, jsonify
)
from werkzeug.exceptions import abort
from werkzeug.utils import secure_filename

from hfo_engine_web.db import get_db

import mne
from trcio import read_raw_trc, write_raw_trc
from ez_detect import config, hfo_annotate

from multiprocessing import Process, Value
import ctypes
import os
from pathlib import Path
import uuid

OK = 200
CREATED = 201
CONFLICT = 409
NOT_FOUND = 404
SERVER_ERROR = 500

bp = Blueprint('engine', __name__)

@bp.route('/')
def index():
    return jsonify(message='Welcome to FastWaveLLC HFO engine')

@bp.route('/upload', methods=['GET', 'POST'])
def upload_file():
    if request.method == 'POST':
        validated = current_app.config['validator'].validateUpload(request)
        
        if 'error_msg' in validated.keys():
            status_code = validated['status_code']
            del validated['status_code']
            return jsonify(validated), status_code 
        elif 'redirect_url' in validated.keys():
            flash(validated['flash_msg'])
            return redirect(validated['redirect_url'])

        validated_filename = validated['validated_fname']
        file = request.files['file']
        if file_extention(validated_filename) == 'edf':
            abs_fname = os.path.join(current_app.config['EDF_FOLDER'], validated_filename)
        elif file_extention(validated_filename) == 'TRC':
            abs_fname = os.path.join(current_app.config['TRC_FOLDER'], validated_filename)

        file.save(abs_fname)
        return jsonify(message = "Successful upload."), CREATED

    elif request.method == 'GET':
        return render_template('engine/upload.html')

@bp.route('/task_state/<uuid:pid>')
def task_state(pid):
    pid = str(pid)
    if pid in current_app.config['task_state'].keys():

        progress = current_app.config['task_state'][pid].progress.get()
        error_msg= current_app.config['task_state'][pid].error_msg.value
        status_code = current_app.config['task_state'][pid].status_code.value
        if progress >= 100:
            del current_app.config['task_state'][pid]

        return (jsonify(progress = progress, 
                        error_msg= error_msg.decode('utf-8')),
                status_code)

    else:   
        return (jsonify(error_msg="There is not an active job with that id."), 
                NOT_FOUND)

#Analizer

@bp.route('/trc_info/<path:trc_fname>')
def trc_info(trc_fname):
    validated_filename = current_app.config['validator'].validateFilename(trc_fname)
    abs_trc_fname = os.path.join(current_app.config['TRC_FOLDER'], validated_filename)
    raw_trc = read_raw_trc(abs_trc_fname, preload=False)
    return jsonify( montage_names = montage_names(raw_trc),
                    recording_len_snds = str(duration_snds(raw_trc)) )

@bp.route( ('/hfo_analizer'), methods=['POST'])
def run_hfo_engine():
    
    validated = current_app.config['validator'].validateRun(request, current_app.config['TRC_FOLDER'])
    
    if 'error_msg' in validated.keys():
        status_code = validated['status_code']
        del validated['status_code']
        return jsonify(validated), status_code 

    pid = str(uuid.uuid4())
    current_app.config['task_state'][pid] = TaskState( progress = AtomicCounter(init_value=0, min_value=0, max_value=100),
                                                       error_msg = Value(ctypes.c_char_p, "None".encode('utf-8')),
                                                       status_code = Value('i', OK))

    p = Process(target=hfo_annotate_task, args=(validated['abs_trc_fname'],
                                                validated['abs_evt_fname'],
                                                validated['str_time'],
                                                validated['stp_time'],
                                                validated['cycle_time'],
                                                validated['sug_montage'],
                                                validated['bp_montage'],
                                                current_app.config['task_state'][pid],
                                                current_app.config['JOBS_COUNT']
                                               ))
    p.start()

    return jsonify(task_id=pid)

@bp.route('/download/evts/<path:filename>')
def download_evt_file(filename):
    validated_fname = current_app.config['validator'].validateEvtDownload(filename)
    return send_from_directory( current_app.config['EVT_FOLDER'], validated_fname)

#Conversor

@bp.route('/edf_suggested_ch_map/<path:edf_fname>')
def edf_suggested_ch_map(edf_fname):
    validated_filename = current_app.config['validator'].validateFilename(edf_fname)
    abs_edf_fname = os.path.join(current_app.config['EDF_FOLDER'], validated_filename)
    return jsonify(suggested_mapping = suggested_trc_ch_names(abs_edf_fname))

@bp.route('/edf_to_trc', methods=['POST'])
def edf_to_trc():
    validated = current_app.config['validator'].validateEDFToTRC_Step1(request, current_app.config['EDF_FOLDER'])
    
    if 'error_msg' in validated.keys():
        status_code = validated['status_code']
        del validated['status_code']
        return jsonify(validated), status_code 
    
    pid = str(uuid.uuid4())
    current_app.config['task_state'][pid] = TaskState( 
                                                       progress = AtomicCounter(init_value=0, 
                                                                                min_value=0, 
                                                                                max_value=100),
                                                       error_msg = Value(ctypes.c_char_p, "None".encode('utf-8')),
                                                       status_code = Value('i', OK)
                                                     )

    p = Process(target=edf_to_trc_task, args=( validated['edf_path'],
                                               validated['ch_names_mapping'],
                                               current_app.config['TRC_FOLDER'],
                                               current_app.config['validator'],
                                               current_app.config['task_state'][pid])
                                              )
    p.start()

    return jsonify(task_id=pid)

@bp.route('/download/TRCs/<path:filename>')
def download_trc_file(filename):
    validated_fname = current_app.config['validator'].validateTRCDownload(filename)
    return send_from_directory( current_app.config['TRC_FOLDER'], validated_fname)


#LOGIC CLASSES

class TaskState:
    def __init__(self, progress, error_msg, status_code):
        self.progress = progress
        self.error_msg = error_msg
        self.status_code = status_code

class AtomicCounter(object):
    def __init__(self, init_value=0, min_value=0,  max_value=10):
        self.counter = Value('i', init_value)
        self.min_value = min_value
        self.max_value = max_value

    def get(self):
        with self.counter.get_lock():
            return self.counter.value
    def delete(self):
        with self.counter.get_lock():
            del self.counter

    def increment(self):
        with self.counter.get_lock():
            assert(self.counter.value < self.max_value)
            self.counter.value += 1

    def decrement(self):
        with self.counter.get_lock():
            assert(self.counter.value > self.min_value)
            self.counter.value -= 1

    def update(self, val):
        with self.counter.get_lock():
            assert(self.min_value <= val and val <= self.max_value)
            self.counter.value = val

class Validator:
    def __init_(self):
        pass

    def validateFilename(self, filename):
        return secure_filename(filename)

    def validateUpload(self, request):
        if 'file' not in request.files:
            flash('No file part')
            return {'redirect_url':request.url,
                    'flash_msg': 'No file part'}
        
        file = request.files['file']
        
        if file.filename == '':
            return {'redirect':request.url,
                    'flash_msg': 'No selected file'}
        
        try:
            if file and allowed_file(file.filename):
                pass
        except ValueError:
            return {'error_msg':'The filename has not an allowed extention', 
                    'status_code':CONFLICT}

        return {'validated_fname': secure_filename(file.filename)}

 	#Analizer

    def validateRun(self, request, trc_directory):
    
	    #Check resources availability
	    try:
	        current_app.config['JOBS_COUNT'].increment()
	    except AssertionError:
	        return {'error_msg':'The server has reached the maximum active jobs. Try again later.', 
	                'status_code':CONFLICT}

        content = request.get_json(silent=True)

	    #check file exists
	    abs_trc_fname = os.path.join(trc_directory, content['trc_fname'])
	    try:
	        abs_trc_fname = str(Path(abs_trc_fname).resolve())
	    except FileNotFoundError:
	        return {'error_msg' : 'You must upload the TRC prior to running hfo_engine on it.', 
	                'status_code' : NOT_FOUND}

	    raw_trc = read_raw_trc(abs_trc_fname, preload=False)
        str_time = int(content['str_time'])
        stp_time = int(content['stp_time'])
	    
        if str_time < 0 or stp_time < str_time:
	        return {'error_msg':"Start time is incorrect for the current trc.", 
	                'status_code':CONFLICT}
	    elif stp_time > duration_snds(raw_trc):
	        return {'error_msg':"Stop time is greater than current trc duration.", 
	                'status_code' : CONFLICT}
	    elif content['sug_montage'] not in montage_names(raw_trc):
	        return {'error_msg':"Suggested montage is not an option for current TRC file.", 
	                'status_code' : CONFLICT}
	    elif content['bp_montage'] not in montage_names(raw_trc):
	        return {'error_msg':"Bipolar montage is not an option for current TRC file.",
	                'status_code' : CONFLICT}
	   
	    evt_fname = Path(content['trc_fname']).stem + '.evt'
	    abs_evt_fname = os.path.join(current_app.config['EVT_FOLDER'], evt_fname)

	    return {
	            'abs_trc_fname': abs_trc_fname, 
	            'abs_evt_fname' : abs_evt_fname, 
	            'str_time'      : str_time,
	            'stp_time'      : stp_time,
	            'cycle_time'    : int(content['cycle_time']), 
	            'sug_montage'   : content['sug_montage'], 
	            'bp_montage'    : content['bp_montage']
	           }

  	def validateEvtDownload(self, filename):
        return secure_filename(filename)

	#Conversor

    def validateEDFToTRC_Step1(self, request, directory ):

        content = request.get_json(silent=True)
        edf_fname = content['edf_fname']
        edf_path = Path(directory, edf_fname)
        try:
            edf_path = edf_path.resolve() #Checks edf exist
        except FileNotFoundError:
            return { 'error_msg':"You must upload the edf prior to do the convertion.",
                     'status_code' : NOT_FOUND
                   } 

        return {'edf_path': edf_path,
                'ch_names_mapping': content['ch_names_mapping']
                }
    
    def validateEDFToTRC_Step2(self, ch_names_translation, edf_ch_names, state_notifier):
        #Check the ch_name list is valid
        if set(ch_names_translation.keys()) != set(edf_ch_names):
            state_notifier.error_msg.value =("The provided ch_names_mapping must have a definition" 
                                             " for (and only for) every channel name in the edf.") 
            state_notifier.status_code = CONFLICT

        #Check the final ch_name list is valid
        #All diferent 
        if len(set(ch_names_translation.values())) != len(list(ch_names_translation.values())): 
            state_notifier.error_msg.value ="The final ch_name list has repetead values."
            state_notifier.status_code = CONFLICT
        
        #Length is correct
        TRC_MAX_CH_NAME_LEN = 5
        for long_name, short_name in ch_names_translation.items():
            if len(short_name) > TRC_MAX_CH_NAME_LEN:
                state_notifier.error_msg.value = ("TRC channel names length must be less or"
                                                  " equal to {}.".format(TRC_MAX_CH_NAME_LEN)) 
                state_notifier.status_code =CONFLICT

   	def validateTRCDownload(self, filename):
        return secure_filename(filename)
    

######### AUX FUNCTIONS ###########

ALLOWED_EXTENSIONS = set(['edf', 'TRC'])

def allowed_file(filename):
    return '.' in filename and \
           file_extention(filename) in ALLOWED_EXTENSIONS

def file_extention(filename):
    try:
        return filename.rsplit('.', 1)[1]
    except IndexError:
        raise ValueError('There is not extention in filename ' + filename)
    
#Analizer

def montage_names(raw_trc):
    return list(raw_trc._raw_extras[0]['montages'].keys())

def duration_snds(raw_trc):
    return raw_trc._raw_extras[0]['n_samples'] // raw_trc._raw_extras[0]['sfreq']


def hfo_annotate_task(abs_trc_fname, abs_evt_fname, str_time, stp_time, cycle_time, 
                      sug_montage, bp_montage, state_notifier, jobs_count):
    state_notifier.progress.update(0)
    state_notifier.error_msg.value = "testing".encode('utf-8') 

    paths = config.getAllPaths(abs_trc_fname, 
                               abs_evt_fname)
    config.clean_previous_execution()
    try:
        hfo_annotate(paths, str_time, stp_time, cycle_time, sug_montage, bp_montage, 
                     progress_notifier=state_notifier.progress) 
        state_notifier.progress.update(100) 
        state_notifier.status_code.value = CREATED
    except Exception as e:
        state_notifier.error_msg.value = "Internal error".encode('utf-8') #no anda bien el encoding
    finally: 
        jobs_count.decrement()

    
#Conversor

def suggested_trc_ch_names(edf_fname):
    
    def suggested_translation(ch_name, known_translation):
	    if ch_name in known_translation.keys():
	        return known_translation[ch_name]
	    else:
	        return ch_name[-5:]

    raw_edf = mne.io.read_raw_edf(edf_fname, preload=True)
    raw_edf.pick_types(eeg=True, stim=False)
    translation = dict()
    db = get_db()
    rows = db.execute('SELECT * FROM ch_name_translation').fetchall()
    known_translation = {translation['long_name']:translation['short_name'] for translation in rows}
    for ch_name in raw_edf.ch_names:
        translation[ch_name] = suggested_translation(ch_name, known_translation)
    return translation

def edf_to_trc_task(edf_path, ch_names_translation, saving_directory, validator, state_notifier):
    state_notifier.progress.update(0)
    
    raw_edf = mne.io.read_raw_edf( str(edf_path), preload=True)
    raw_edf.pick_types(eeg=True, stim=False)
    state_notifier.progress.update(20)

    validator.validateEDFToTRC_Step2(ch_names_translation, raw_edf.ch_names, state_notifier)
    state_notifier.progress.update(30)

    # If data comes in microvolts, this information is stored 
    # in the info dict in the unit_mul entry as -6. If it is volt, then it is stored as 1.
    #info['chs'][0]['unit'] info['chs'][0]['unit_mul']
    #to_rename = {x: x.split(' ')[1].split('-')[0] for x in raw_edf.ch_names}
    raw_edf._data *= 1e-06 #From microvolts to volts

    to_rename = {long:short for long,short in ch_names_translation.items() if long != short}
    raw_edf.rename_channels(to_rename)
    state_notifier.progress.update(40)

    db = get_db()
    for long, short in to_rename.items():
        db.execute(
                    'REPLACE INTO ch_name_translation'
                    ' VALUES (?,?)', 
                    (long, short)
                  )
    db.commit()
    state_notifier.progress.update(50)
    trc_fname = edf_path.stem + '.TRC'
    abs_trc_fname = os.path.join(saving_directory, trc_fname)
    write_raw_trc(raw_edf, abs_trc_fname)

    state_notifier.progress.update(100)
    state_notifier.status_code.value = CREATED
