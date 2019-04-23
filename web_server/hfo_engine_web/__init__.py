import os
from flask import Flask
from .engine import AtomicCounter, TaskState, Validator

def create_app(test_config=None):
    # create and configure the app
    app = Flask(__name__, instance_relative_config=True)
    app.config.from_mapping(
        SECRET_KEY='dev',
        DATABASE=os.path.join(app.instance_path, 'hfo_engine.sqlite'),
    )
    if test_config is None:
        # load the instance config, if it exists, when not testing
        app.config.from_pyfile('config.py', silent=True)
    else:
        # load the test config if passed in
        app.config.from_mapping(test_config)

    MAX_JOBS_RUNNING = 2
    EDF_FOLDER = os.path.join(app.instance_path, 'edfs')
    TRC_FOLDER = os.path.join(app.instance_path, 'TRCs')
    EVT_FOLDER = os.path.join(app.instance_path, 'evts')
    
    make_dir(app.instance_path)
    make_dir(EDF_FOLDER)
    make_dir(TRC_FOLDER)
    make_dir(EVT_FOLDER)

    app.config['EDF_FOLDER'] = EDF_FOLDER
    app.config['TRC_FOLDER'] = TRC_FOLDER
    app.config['EVT_FOLDER'] = EVT_FOLDER
    app.config['MAX_CONTENT_LENGTH'] = 5 * 1024 ** 3 #5 GB
    app.config['JOBS_COUNT'] = AtomicCounter(init_value=0, min_value=0, max_value=MAX_JOBS_RUNNING)
    app.config['task_state'] = {} #UUID.HEX : TaskState
    app.config['validator'] = Validator()

    @app.route('/hello')
    def hello():
        return 'Hello LIAA!'
        
    from . import db, engine
    db.init_app(app)
    #app.register_blueprint(auth.bp)
    app.register_blueprint(engine.bp)
    app.add_url_rule('/', endpoint='index')

    return app

def make_dir(aDir):
    try:
        os.makedirs(aDir)
    except OSError:
        pass