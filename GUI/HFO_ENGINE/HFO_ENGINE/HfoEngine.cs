using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using CommandLine;
using System.Web.Script;
using System.Web.Script.Serialization;
using System.Net;
using System.Json;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using HFO_ENGINE;
using Newtonsoft.Json.Converters;
using System.ComponentModel;

namespace HFO_ENGINE{

    //Classes

    static class Program {
        
        //Colaborators
        public static Controller Controller { get; set; } 

        //Methods
        public static string MainDir() { return AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/"); }
        
        [STAThread]
        static void Main(string[] args){
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Controller = new Controller();
            Controller.Init(args);        
        }

    }

    class Controller {
        //Constructor
        public Controller() {
            this.Model = new Model();
            this.BgWorker = new BackgroundWorker();
            this.BgWorker = new BackgroundWorker();
        }

        //Collaborators
        private Model Model { get; set; }
        private BackgroundWorker BgWorker { get; set; }

        //************ Methods **************//
        public void Init(string[] args) {
            //Command line parsing
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
            .WithNotParsed<Options>((errs) => HandleParseError(errs));
            
            Application.Run(this.GetScreen_Home());
        }
        public void InitConversions() {
            this.Model.Conversion_1_Form = new Fastwave_conversor();
        }

          //Getters of the model  
        public API GetAPI() { return this.Model.API; }
        public string GetTRCFile() { return this.Model.AnalizerParams.TrcFile; }
        public int GetStartTime() { return this.Model.AnalizerParams.StartTime; }
        public int GetStopTime() { return this.Model.AnalizerParams.StopTime; }
        public int GetCycleTime() { return this.Model.AnalizerParams.CycleTime; }
        public string GetEvtFile() { return this.Model.AnalizerParams.EvtFile; }
        public string GetSuggestedMontage() { return this.Model.AnalizerParams.SuggestedMontage; }
        public string GetBipolarMontage() { return this.Model.AnalizerParams.BipolarMontage; }
        public ConversionParams GetConvParams() { return this.Model.ConversionParams; }
        public MainWindow GetScreen_Home() { return this.Model.HomeScreen; }
        public EEG GetScreen_EEG() { return this.Model.EEG_Form; }
        public Montage GetScreen_Montage() { return this.Model.Montage_Form; }
        public TimeWindow GetScreen_TimeWindow() { return this.Model.TimeWindow_Form; }
        public CycleTime GetScreen_CycleTime() { return this.Model.CycleTime_Form; }
        public AdvancedSettings GetScreen_Settings() { return this.Model.Settings_Form; }
        public EVT GetScreen_Evt() { return this.Model.Evt_Form; }
        public Progress GetScreen_AnalizerProgress() { return this.Model.ProgressScreen; }
        public Fastwave_conversor GetScreen_Conv_1() { return this.Model.Conversion_1_Form; }
        public Translation GetScreen_Conv_2() { return this.Model.Conversion_2_Form; }
        public FinalConvertion GetScreen_Conv_3() { return this.Model.Conversion_3_Form; }
        public int GetTRCDuration() { return this.Model.TRCDuration; }
        public string[] GetMontageNames() { return this.Model.MontageNames; }
        public string GetLogFile() { return this.Model.LogFile; }
        public string GetTRCTempDir() { return this.Model.TRCTempDir; }

        //Analizer Logic
        public void SetTRCFile(string trc_fname)
        {
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.TrcFile = trc_fname; //already checked that exists
                File.Copy(trc_fname, this.GetTRCTempPath(trc_fname), true);

                string uri_upload = this.GetAPI().URI() + "upload";
                WebClient webClient = new WebClient();
                void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
                {
                    Console.WriteLine("Upload {0}% complete. ", e.ProgressPercentage);
                    Program.Controller.GetScreen_EEG().UploadProgress = (int)e.ProgressPercentage;
                }
                void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
                {
                    MessageBox.Show("Upload is complete. ");
                    this.SetTRC_metadata();
                    this.Model.EEG_Form.UploadProgress = 0;

                }
                webClient.UploadProgressChanged += WebClientUploadProgressChanged;
                webClient.UploadFileCompleted += WebClientUploadCompleted;
                webClient.UploadFileAsync(new Uri(uri_upload), this.GetTRCTempPath(trc_fname));
            }
        }
        public void SetMontages(string sug_montage, string bp_montage)
        {
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.SuggestedMontage = sug_montage;
                this.Model.AnalizerParams.BipolarMontage = bp_montage;
            }
        }
        public void SetTimeWindow(int start_time, int stop_time)
        {
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else
            {
                string NOT_SAVED_MSG = "Changes were NOT saved.";
                if (start_time < 0) MessageBox.Show(NOT_SAVED_MSG + "Reason: Start time must be greater or equal to 0.");
                else if (stop_time > this.Model.TRCDuration) MessageBox.Show(NOT_SAVED_MSG + "Reason: Stop time is greater than TRC_duration.");
                else if (start_time > stop_time) MessageBox.Show(NOT_SAVED_MSG + "Reason: Start time is greater than stop time.");
                else
                {
                    this.Model.AnalizerParams.StartTime = start_time;
                    this.Model.AnalizerParams.StopTime = stop_time;
                }
            }
        }
        public void SetCycleTime(bool parallel_flag, int cycle_time)
        {
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else
            {
                if (parallel_flag) this.Model.AnalizerParams.CycleTime = cycle_time;
                else this.Model.AnalizerParams.CycleTime = this.GetStopTime() - this.GetStartTime() + 1;
            }
        }
        public void SetEvt(string evt_fname){
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else this.Model.AnalizerParams.EvtFile = evt_fname;
        }
        public void SetAdvancedSettings(string hostname, string port, string log_file, string trc_temp_dir)
        {
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else {
                this.Model.API.Hostname = hostname;
                this.Model.API.Port = port;
                this.Model.LogFile = log_file;
                this.Model.TRCTempDir =  trc_temp_dir;
            }
        }
        private void SetTRC_metadata()
        {
            string uri_trc_info = this.GetAPI().URI() + "trc_info/" + Path.GetFileName(this.GetTRCFile());
            string json_resp = GetJsonSync(uri_trc_info);
            TRCInfo trc_info = JsonConvert.DeserializeObject<TRCInfo>(json_resp);
            this.Model.MontageNames = trc_info.montage_names;
            this.GetScreen_Montage().LoadMontages(trc_info.montage_names);
            this.Model.TRCDuration = trc_info.recording_len_snds;
            this.GetScreen_TimeWindow().SetTRCDuration(trc_info.recording_len_snds);
            
        }
        public bool Is_TRC_metadata_setted() { return this.GetTRCDuration() != 0; }
        public void StartHFOAnalizer()
        {
            if (this.Model.IsBusy){this.GetScreen_Home().AbrirFormHija(this.GetScreen_AnalizerProgress());
            } else
            {
                if (string.IsNullOrEmpty(this.GetTRCFile()))
                {
                    MessageBox.Show("Please pick a TRC file to analize in the EEG menu.");
                }
                else if (string.IsNullOrEmpty(this.GetSuggestedMontage()) || string.IsNullOrEmpty(this.GetBipolarMontage()))
                {
                    MessageBox.Show("Please set the montages in Montage menu.");
                }
                else if (this.GetStopTime() == 0)
                {
                    MessageBox.Show("Please set your Time-window settings in Time-Window menu.");
                }
                else if (this.GetCycleTime() == 0)
                {
                    this.Model.AnalizerParams.CycleTime = this.GetStartTime() - this.GetStartTime() + 1;
                }
                else if (string.IsNullOrEmpty(this.GetEvtFile()))
                {
                    MessageBox.Show("Please set the evt saving path from the Output menu.");
                }
                else if (string.IsNullOrEmpty(this.GetAPI().Hostname) || string.IsNullOrEmpty(this.GetAPI().Port))
                {
                    MessageBox.Show("Please set the API settings in Advanced settings menu.");
                }
                else {
                    this.Model.IsBusy = true;
                    this.GetScreen_Home().AbrirFormHija(this.GetScreen_AnalizerProgress());
                    this.GetScreen_AnalizerProgress().StartTimer();
                    this.GetScreen_AnalizerProgress().BgWorker.DoWork += (s, f) =>{
                        this.RunHFOEngineWith(
                                this.GetTRCFile(),
                                this.GetStartTime(),
                                this.GetStopTime(),
                                this.GetCycleTime(),
                                this.GetSuggestedMontage(),
                                this.GetBipolarMontage(),
                                this.GetEvtFile()
                            );
                    };
                    this.GetScreen_AnalizerProgress().BgWorker.RunWorkerAsync();
                }
            }
        }
        private void RunHFOEngineWith(string trc_fname, int start_time, int stop_time, int cycle_time,
                                      string sug_montage, string bp_montage, string evt_fname)
        {
            this.GetScreen_AnalizerProgress().UpdateProgress(1);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.GetLogFile(), true)) { file.WriteLine("Running HfoAnnotate App..."); }
            string lines = "Input trc_path: " + trc_fname + Environment.NewLine +
                           "Output xml_path: " + evt_fname + Environment.NewLine;
            File.WriteAllText( this.GetLogFile(), lines);

            string uri_run = this.GetAPI() + "run";
            Dictionary<string, string> Params = new Dictionary<string, string>();
            Params.Add("trc_fname", Path.GetFileName(trc_fname));
            Params.Add("str_time", start_time.ToString());
            Params.Add("stp_time", stop_time.ToString());
            Params.Add("cycle_time", cycle_time.ToString());
            Params.Add("sug_montage", sug_montage);
            Params.Add("bp_montage", bp_montage);
            string serialized_params = JsonConvert.SerializeObject(Params, new KeyValuePairConverter());
            string run_response_str = this.PostJsonSync(uri_run, serialized_params);

            JsonObject run_response = (JsonObject)JsonValue.Parse(run_response_str);
            if (run_response.ContainsKey("error_msg")) MessageBox.Show(run_response["error_msg"]);
            else
            {
                string pid = run_response["task_id"];
                MessageBox.Show("DEBUG: "+ pid);

                string uri_task_state = this.GetAPI().URI() + "task_state/" + pid;
                int progress = 0;
                do
                {
                    string task_state_string = this.GetJsonSync(uri_task_state);
                    JsonObject task_state = (JsonObject)JsonValue.Parse(task_state_string);
                    if (!task_state.ContainsKey("progress")) { MessageBox.Show(task_state["error_msg"]); break; }
                    else progress = task_state["progress"];
                    this.GetScreen_AnalizerProgress().UpdateProgress(progress);
                    Thread.Sleep(1000);
                } while (progress < 100);
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.GetLogFile(), true)) { file.WriteLine("Getting evt from remote server..."); }
                DownloadEvt(Path.GetFileName(trc_fname)); //the remote EvtFile name is created this way
                this.GetScreen_AnalizerProgress().UpdateProgress(100);
                this.GetScreen_AnalizerProgress().SaveAndReset_timer();
                this.Model.IsBusy = false;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.GetLogFile(), true)) { file.WriteLine("Exiting."); }
            }
        }
        private void DownloadEvt(string remote_evt_fname)
        {
            string uri_get_evt = this.GetAPI().URI() + "download/evts/" + remote_evt_fname;
            using (var client = new WebClient())
            {
                client.DownloadFile(uri_get_evt, this.GetEvtFile());
            }
        }

        //Conversor Logic
        public void StartEdfConversion(string edf_fname)
        {
            if (this.Model.IsBusy) this.UnavailableOptionMsg();
            else {
                this.Model.IsBusy = true;
               
                string uri_upload = this.GetAPI().URI() + "upload";
                WebClient webClient = new WebClient();
                void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
                {
                    Console.WriteLine("Edf upload {0}% complete. ", e.ProgressPercentage);
                    this.GetScreen_Conv_1().UploadProgress = (int)e.ProgressPercentage;
                }
                void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
                {
                    MessageBox.Show("Edf upload is complete. ");
                    this.GetScreen_Conv_1().UploadProgress = 0;
                    this.GetChMapping(edf_fname);
                }
                webClient.UploadProgressChanged += WebClientUploadProgressChanged;
                webClient.UploadFileCompleted += WebClientUploadCompleted;
                webClient.UploadFileAsync(new Uri(uri_upload), edf_fname);
            }
            
        }
        public void GetChMapping(string edf_fname)
        {
            string uri_suggestion = this.GetAPI().URI() + "edf_suggested_ch_map/" + Path.GetFileName(edf_fname);
            string suggestion_response = this.GetJsonSync(uri_suggestion);
            MessageBox.Show(suggestion_response);
            Dictionary<String, String> ch_mapping = ((ChannelMappingSuggestion)(JsonConvert.DeserializeObject<ChannelMappingSuggestion>(suggestion_response))).value["suggested_mapping"];

            this.Model.ConversionParams = new ConversionParams(Path.GetFileName(edf_fname), ch_mapping); //Save server suggestion
            this.Model.Conversion_2_Form = new Translation( this.GetConvParams().ch_names_mapping);
            this.GetScreen_Home().AbrirFormHija( this.GetScreen_Conv_2());
        }
        public void ConfirmChMapping(Dictionary<string, string> ch_translations)
        {
            this.Model.ConversionParams.ch_names_mapping = ch_translations;
            this.Model.Conversion_3_Form = new FinalConvertion();

            this.GetScreen_Home().AbrirFormHija(this.GetScreen_Conv_3());
        }
        public void ConvertEdf(string TrcSavingPath)
        {
            string uri_edf_to_trc = this.GetAPI().URI() + "edf_to_trc/";
            string serialized_conv_params = new JavaScriptSerializer().Serialize(this.GetConvParams());
            string run_response_str = this.PostJsonSync(uri_edf_to_trc, serialized_conv_params);
            JsonObject run_response = (JsonObject)JsonValue.Parse(run_response_str);
            if (run_response.ContainsKey("error_msg")) MessageBox.Show(run_response["error_msg"]);
            else
            {
                string pid = run_response["task_id"];
                string uri_task_state = this.GetAPI().URI() + "task_state/" + pid;
                int progress = 0;
                do
                {
                    string task_state_string = this.GetJsonSync(uri_task_state);
                    JsonObject task_state = (JsonObject)JsonValue.Parse(task_state_string);
                    if (!task_state.ContainsKey("progress")) { MessageBox.Show(task_state["error_msg"]); return; }
                    else progress = task_state["progress"];
                    MessageBox.Show(progress.ToString());
                    this.GetScreen_Conv_3().Progress = progress;
                    Thread.Sleep(1000);
                } while (progress < 100);


                DownloadTRC(Path.GetFileName(this.GetTRCFile()), TrcSavingPath); 
            }
        }
        public void DownloadTRC(string remote_trc_fname, string trc_saving_path)
        {
            WebClient webClient = new WebClient();
            void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                Console.WriteLine("TRC download {0}% complete. ", e.ProgressPercentage);
                this.GetScreen_Conv_3().Progress = (int)e.ProgressPercentage;
            }
            void WebClientDownloadCompleted(object sender, EventArgs e)
            {
                MessageBox.Show("TRC download is complete. ");
                this.GetScreen_Conv_3().Progress = 0;
            }
            webClient.DownloadProgressChanged += WebClientDownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClientDownloadCompleted;

            string uri_download_trc = this.GetAPI().URI() + "download/TRCs/" + remote_trc_fname;
            webClient.DownloadFileAsync(new Uri(uri_download_trc), trc_saving_path);
        }

        //General Program Logic
        private void RunOptionsAndReturnExitCode(Options opts)
        {
            if (!string.IsNullOrEmpty(opts.TrcFile)) {
                this.Model.AnalizerParams.TrcFile = (opts.TrcFile).Replace("\\", "/");
                this.GetScreen_EEG().SetTRCFile(this.GetTRCFile());
            }
            if (!string.IsNullOrEmpty(opts.EvtFile)) {
                this.Model.AnalizerParams.EvtFile = (opts.EvtFile).Replace("\\", "/");
                this.GetScreen_Evt().SetEvtFile(this.GetEvtFile());
            }
            
        }
        public void TestConnection()
        {
            string index_uri = Program.Controller.GetAPI().URI();
            string json_index_str = this.GetJsonSync(index_uri);
            JsonObject json_index = (JsonObject)JsonValue.Parse(json_index_str);
            MessageBox.Show(json_index["message"]);
        }
        private string GetTRCTempPath(string trc_fname) { return this.GetTRCTempDir() + Path.GetFileName(trc_fname); }
        private void UnavailableOptionMsg()
        {
            MessageBox.Show("This option is unavailable at this moment. Please try again later.");
        }
        private void HandleParseError(IEnumerable<Error> errs)
        {
            //TODO
            throw new NotImplementedException();
        }

        //Extras
        public async Task<JsonObject> GetJsonAsync(string uri)
        {
            HttpClient httpClient = new HttpClient();
            string content = await httpClient.GetStringAsync(uri);
            return await Task.Run(() => (JsonObject)JsonValue.Parse(content));
        }
        public async Task<JsonObject> PostJsonASync(string uri, JsonObject serialized_json)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent body = new StringContent(serialized_json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uri, body);
                return (JsonObject)JsonValue.Parse(await response.Content.ReadAsStringAsync());
            }
        }
        public string GetJsonSync(string uri)
        {
            HttpClient httpClient = new HttpClient();
            return httpClient.GetStringAsync(uri).Result;
            //return (JsonObject)JsonValue.Parse(content);
        }
        public string PostJsonSync(string uri, string serialized_json)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent body = new StringContent(serialized_json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PostAsync(uri, body).Result;
                return response.Content.ReadAsStringAsync().Result;
                //return (JsonObject)JsonValue.Parse(response.Content.ReadAsStringAsync().Result);
            }
        }

        private static void UploadFileAsync(Tuple<WebClient, Uri, string> Params) {
            Params.Item1.UploadFileAsync(Params.Item2, Params.Item3);
        }
    }

    class Model {
        //Constructor
        public Model() {
            this.API = new API("grito.exp.dc.uba.ar", "8080");
            this.AnalizerParams = new AnalizerParams();
            this.ConversionParams = null;

            this.TRCDuration = 0;
            this.IsBusy = false;
            this.LogFile = Program.MainDir() + "logs/ez_detect_gui_log.txt";
            this.TRCTempDir = Program.MainDir() + "temp/";

            //Build screens 
            this.HomeScreen = new MainWindow();
            this.EEG_Form = new EEG();
            this.Montage_Form = new Montage();
            this.TimeWindow_Form = new TimeWindow();
            this.CycleTime_Form = new CycleTime();
            this.Evt_Form = new EVT();
            this.Settings_Form = new AdvancedSettings(this.API.Hostname, this.API.Port, this.LogFile, this.TRCTempDir);

            this.ProgressScreen = new Progress();

            this.Conversion_1_Form = null;
            this.Conversion_2_Form = null;
            this.Conversion_3_Form = null;

        }

        //Collaborators
        public API API { get; set; }
        public AnalizerParams AnalizerParams { get; set; }
        public ConversionParams ConversionParams { get; set; }

        public string[] MontageNames { get; set; }
        public int TRCDuration { get; set; }
        public bool IsBusy { get; set; }
        public string LogFile { get; set; }
        public string TRCTempDir { get; set; }

        public MainWindow HomeScreen { get; set; }
        public EEG EEG_Form { get; set; }
        public Montage Montage_Form { get; set; }
        public TimeWindow TimeWindow_Form { get; set; }
        public CycleTime CycleTime_Form { get; set; }
        public EVT Evt_Form { get; set; }
        public AdvancedSettings Settings_Form { get; set; }
        public Progress ProgressScreen { get; set; }

        public Fastwave_conversor Conversion_1_Form { get; set; }
        public Translation Conversion_2_Form { get; set; }
        public FinalConvertion Conversion_3_Form { get; set; }
       
    }

    class API {
        //Constructor
        public API(string hostname, string port) {
            this.Hostname = hostname;
            this.Port = port;
        }

        //Colaborators
        public string Hostname { get; set; } 
        public string Port { get; set; } 
        public string URI() { return "http://" + this.Hostname + ":" + this.Port + "/"; }
    }
    class AnalizerParams
    {
        //Constructor 
        public AnalizerParams() {
            this.TrcFile = String.Empty;
            this.EvtFile = String.Empty;
            this.StartTime = 0;
            this.StopTime = 0;
            this.CycleTime = 0; 
            this.SuggestedMontage = String.Empty;
            this.BipolarMontage = String.Empty;

        }

        public string TrcFile { get; set; } 
        public string EvtFile { get; set; }
        public int StartTime { get; set; } 
        public int StopTime { get; set; }
        public int CycleTime { get; set; }
        public string SuggestedMontage { get; set; } 
        public string BipolarMontage { get; set; } 
    }
    class ConversionParams{
        //Constructor
        public ConversionParams(string edf_filename, Dictionary<String, String> _suggested_mapping) {
            edf_fname = edf_filename;
            ch_names_mapping = _suggested_mapping;
        }

        //Colaborators
        public Dictionary<String, String> ch_names_mapping { get; set; }
        public string edf_fname { get; set; }

    }

    //Aux classes
    class Options
    {
        [Option('t', "trc", Required = false,
        HelpText = "Full path to input trc file to be processed.")]
        public string TrcFile { get; set; }

        [Option('x', "xml", Required = false,
         HelpText = "Full path to output evt file to be saved.")]
        public string EvtFile { get; set; }

        // Omitting long name, defaults to name of property, ie "--verbose"
        [Option(Default = false,
        HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
    class ChannelMappingSuggestion
    {
        public ChannelMappingSuggestion(Dictionary<String, Dictionary<String, String>> _value)
        {
            value = _value;
        }
        public Dictionary<String, Dictionary<String, String>> value { get; set; }
    }
    class TRCInfo
    {
        public string[] montage_names { get; set; }
        public int recording_len_snds { get; set; }
    }

}
