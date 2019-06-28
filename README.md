# hfo_engine
This project has to do with user level usage, from BQ, from the GUI and inderectly calling the web server, which depends on ez_detect module.


#Backend endpoints in C#

public string URI() { return "http://" + this.Hostname + ":" + this.Port; }
public string GetUri_JobState(string job_id) {return this.URI() + "/task_state/" + job_id; }

//Analizer URIs
public string GetUri_AnalizerBP() {return this.URI() + "/analyzer";}
public string GetUri_UploadTRC() {return this.GetUri_AnalizerBP() + "/upload_trc"; }
public string GetUri_TrcInfo(string trc_fname) {return this.GetUri_AnalizerBP() + "/trc_info/" + trc_fname; }
public string GetUri_Analizer() {return this.GetUri_AnalizerBP() + "/analyze"; }
public string GetUri_DownloadEvt(string evt_fname) {return this.GetUri_AnalizerBP() + "/download_evt/" + evt_fname; }

//Converter URIs
public string GetUri_ConverterBP() {return this.URI() + "/converter";}
public string GetUri_UploadEdf() {return this.GetUri_ConverterBP() + "/upload_edf"; }
public string GetUri_Suggested_ChName_Translation(string edf_fname) {return this.GetUri_ConverterBP() + "/suggested_ch_name_mapping/" + edf_fname; }
public string GetUri_Converter() { return this.GetUri_ConverterBP()+"/convert";}
public string GetUri_DownloadTRC(string trc_fname) {return this.GetUri_ConverterBP() + "/download_trc/" + trc_fname; }