# Copyright 2018 FastWave LLC
#
# NOTICE:  All information contained herein is, and remains the property of
# FastWave LLC. The intellectual and technical concepts contained
# herein are proprietary to FastWave LLC and its suppliers and may be covered
# by U.S. and Foreign Patents, patents in process, and are protected by
# trade secret or copyright law. Dissemination of this information or
# reproduction of this material is strictly forbidden unless prior written
# permission is obtained from FastWave LLC.

from os import path as op

from setuptools import setup

VERSION = '1.0.0'

#URL = 'https://gitlab.liaa.dc.uba.ar/tju-uba/TODO'
#url=URL,
#download_url=URL,

if __name__ == "__main__":
    setup(name='hfo_engine_web',
          description='Web service for running hfo engine app remotely.',
          version=VERSION,
          license='Propietary',
          classifiers=['Programming Language :: Python'],
          platforms='any',
          packages=['hfo_engine_web'],
          include_package_data=True,
          zip_safe=False,
          install_requires=['flask',],
          )
