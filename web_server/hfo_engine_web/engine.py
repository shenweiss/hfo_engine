import os
import uuid
from ctypes import c_char_p
from multiprocessing import Process, Value, Manager

from ez_detect.config import ProgressNotifier
from flask import (
    Blueprint, render_template,
    current_app, jsonify
)
from flask_api import status
from werkzeug.utils import secure_filename

engine_bp = Blueprint('engine', __name__)


#         GENERAL ENDPOINTS           #

@engine_bp.route('/')
def index():
    return jsonify(message='Welcome to FastWaveLLC HFO engine')


@engine_bp.route('/task_state/<uuid:job_id>', methods=['GET'])
def task_state(job_id):
    job_id = str(job_id)
    state = current_app.config['JOB_MANAGER'].get_state(job_id)

    return (
        jsonify(progress=state['progress'],
                error_msg=state['error_msg']),
        state['status_code']
    )


###############################

#        SHARED LOGIC         #

EDF_EXTENSION = 'EDF'
TRC_EXTENSION = 'TRC'


def is_allowed_file(filename):
    return '.' in filename and \
           file_extension(filename) in {EDF_EXTENSION, TRC_EXTENSION}


def get_saving_fname(fname):
    if file_extension(fname) == EDF_EXTENSION:
        return os.path.join(current_app.config['EDF_FOLDER'], fname)
    elif file_extension(fname) == TRC_EXTENSION:
        return os.path.join(current_app.config['TRC_FOLDER'], fname)


def file_extension(filename):
    try:
        return filename.rsplit('.', 1)[1].upper()
    except IndexError:
        raise ValueError('There is no extension in filename ' + filename)


def upload_file(request):
    if request.method == 'POST':

        if 'file' not in request.files or request.files['file'] is None:
            return jsonify(error_msg='Request has no file part.'), \
                   status.HTTP_400_BAD_REQUEST

        file = request.files['file']

        if is_allowed_file(file.filename):
            fname = secure_filename(file.filename)
            abs_fname = get_saving_fname(fname)
            file.save(abs_fname)
            return jsonify(message="Successful upload."), status.HTTP_201_CREATED

        else:
            return jsonify(error_msg='This file extension is not allowed.'), \
                   status.HTTP_409_CONFLICT

    elif request.method == 'GET':
        return render_template('engine/upload.html')


#######################################

#       Async task management         #

class JobManager:

    def __init__(self, max_jobs):
        self.jobs = dict()  # UUID.HEX : TaskState
        self.analyzer_job_count = AtomicCounter(min_value=0, max_value=max_jobs)

    def get_state(self, job_id):
        if job_id in self.jobs.keys():
            progress = self.jobs[job_id].progress.get()
            error_msg = self.jobs[job_id].error_msg.value.decode('utf-8')
            status_code = self.jobs[job_id].status_code.value

            is_finished = progress >= 100 or status_code != status.HTTP_200_OK

            if is_finished:
                del self.jobs[job_id]

            return dict(
                progress=progress,
                error_msg=error_msg,
                status_code=status_code
            )
        else:
            return dict(
                progress=0,
                error_msg='There is not an active job with that id.',
                status_code=status.HTTP_404_NOT_FOUND
            )

    def create_conversion_job(self, edf_fname, ch_names_mapping, conversion_procedure):
        # Precondition: Params have been validated
        job_id = str(uuid.uuid4())
        job_state = TaskState()
        self.jobs[job_id] = job_state
        p = Process(target=conversion_procedure,
                    args=(edf_fname, ch_names_mapping, job_state))
        p.start()

        return job_id

    def create_analysis_job(self, trc_fname, evt_fname, str_time, stp_time,
                            cycle_time, sug_montage, bp_montage, analysis_procedure):
        # Precondition: Params have been validated
        self.analyzer_job_count.increment()
        job_id = str(uuid.uuid4())
        job_state = TaskState()
        self.jobs[job_id] = job_state

        p = Process(target=analysis_procedure, args=(trc_fname,
                                                     evt_fname,
                                                     str_time,
                                                     stp_time,
                                                     cycle_time,
                                                     sug_montage,
                                                     bp_montage,
                                                     job_state,
                                                     self))
        p.start()

        return job_id

    def on_analysis_finished(self):
        self.analyzer_job_count.decrement()


class TaskState:

    def __init__(self):
        manager = Manager()
        self.progress = ProgressNotifier()
        self.error_msg = manager.Value(c_char_p, "".encode('utf-8'))
        self.status_code = Value('i', status.HTTP_200_OK)


class AtomicCounter(object):

    def __init__(self, min_value=0, max_value=10):
        self.counter = Value('i', 0)
        self.min_value = min_value
        self.max_value = max_value

    def get(self):
        with self.counter.get_lock():
            return self.counter.value

    def increment(self):
        with self.counter.get_lock():
            assert (self.counter.value < self.max_value)
            self.counter.value += 1

    def decrement(self):
        with self.counter.get_lock():
            assert (self.counter.value > self.min_value)
            self.counter.value -= 1
