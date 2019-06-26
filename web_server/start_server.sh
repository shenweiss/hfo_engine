#!/bin/bash
where=$1
init_db=$2

source venv/bin/activate
#pyenv activate shennan
export LC_ALL=C.UTF-8
export LANG=C.UTF-8

export FLASK_APP=hfo_engine_web

if [ $init_db == "--init" ]
then
	flask init-db
fi

IP=$(ip -4 addr show wlo1 | grep -oP '(?<=inet\s)\d+(\.\d+){3}')
echo 'IP >' $IP 

if [ $where == "--production" ]
then
	MAX_BODY_SIZE=5368709120
	waitress-serve --max-request-body-size=$MAX_BODY_SIZE --call 'hfo_engine_web:create_app'
else
	export FLASK_ENV=development
	flask run
fi

