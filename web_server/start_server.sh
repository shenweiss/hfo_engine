#!/bin/bash
where=$1
init_db=$2

pyenv activate shennan

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
	waitress-serve --max-request-body-size=5368709120 --call 'hfo_engine_web:create_app'   #5GB
else
	export FLASK_ENV=development
	flask run
fi

