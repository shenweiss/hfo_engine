#!/bin/bash
where=$1
init_db=$2

#source venv/bin/activate

export FLASK_APP=hfo_engine_web

if [ $init_db == "--init" ]
then
	flask init-db
fi

IP=$(ip -4 addr show eno1 | grep -oP '(?<=inet\s)\d+(\.\d+){3}')
echo 'IP >' $IP 

if [ $where == "--production" ]
then
	waitress-serve --call 'hfo_engine_web:create_app'
else
	export FLASK_ENV=development
	flask run
fi

