import os

from flask import Flask

from .engine import JobManager


def create_app(test_config=None):
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

    MAX_ANALYZER_JOBS_RUNNING = 2
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
    app.config['MAX_CONTENT_LENGTH'] = 5 * 1024 ** 3  # 5 GB

    app.config['JOB_MANAGER'] = JobManager(max_jobs=MAX_ANALYZER_JOBS_RUNNING)

    from . import db, engine, analyzer, converter
    db.init_app(app)
    app.register_blueprint(engine.engine_bp)
    app.register_blueprint(analyzer.analyzer_bp)
    app.register_blueprint(converter.converter_bp)

    app.add_url_rule('/', endpoint='index')

    return app


def make_dir(a_dir):
    try:
        os.makedirs(a_dir)
    except OSError:
        pass
