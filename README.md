# hfo_engine
This project has to do with user level usage, from BQ, from the GUI and inderectly calling the web server, which depends on ez_detect module.


#Backend endpoints

welcome_msg ="https://hostname:port/"
get_job_state = "https://hostname:port/task_state/job_id_string"

#Analizer URIs
upload_trc = "https://hostname:port/analyzer/upload_trc"
trc_info = "https://hostname:port/analyzer/trc_info"
analize_trc = "https://hostname:port/analyzer/analyze"
download_evt = "https://hostname:port/analyzer/download_evt/evt_fname" 

#Converter URIs
upload_edf = "https://hostname:port/converter/upload_edf"
suggested_ch_name_mapping = "https://hostname:port/converter/suggested_ch_name_mapping/edf_fname"
convert = "https://hostname:port/converter/convert"
download_trc = "https://hostname:port/converter/download_trc/trc_fname" 
