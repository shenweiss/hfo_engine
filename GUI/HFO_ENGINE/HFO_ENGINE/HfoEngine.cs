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

//TODO

//Debug Conversor
//Debug analizer

namespace HFO_ENGINE
{
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
        }

        //Collaborators
        private Model Model { get; set; }
        
        //************ Methods **************//
        public void Init(string[] args) {
            //Command line parsing
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
            .WithNotParsed<Options>((errs) => HandleParseError(errs));
            
            Application.Run(this.GetScreen_Home());
        }

        //Getters of the model  
        public API GetAPI() { return this.Model.API; }
        public AnalizerParams GetAnalizerParams() { return this.Model.AnalizerParams; }
        public ConversionParams GetConvParams() { return this.Model.ConversionParams; }

        public MainWindow GetScreen_Home() { return this.Model.HomeScreen; }
        public EEG GetScreen_EEG() { return this.Model.EEGScreen; }
        public Montage GetScreen_Montage() { return this.Model.MontageScreen; }
        public TimeWindow GetScreen_TimeWindow() { return this.Model.TimeWindowScreen; }
        public CycleTime GetScreen_CycleTime() { return this.Model.CycleTimeScreen; }
        public EVT GetScreen_Evt() { return this.Model.EvtScreen; }
        public AdvancedSettings GetScreen_Settings() { return this.Model.SettingsScreen; }
        public Progress GetScreen_AnalizerProgress() { return this.Model.ProgressScreen; }
        public ConversorStep1 GetScreen_Conv_1() { return this.Model.ConvScreen_1; }
        public ConversorStep2 GetScreen_Conv_2() { return this.Model.ConvScreen_2; }
        public ConversorStep3 GetScreen_Conv_3() { return this.Model.ConvScreen_3; }

        public int GetTrcDuration() { return this.Model.TrcDuration; }
        public string[] GetMontageNames() { return this.Model.MontageNames; }
        public string GetLogFile() { return this.Model.LogFile; }
        public string GetTrcTempDir() { return this.Model.TRCTempDir; }

        //Analizer Logic
        public void SetTrcFile(string trc_fname)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.TrcFile = trc_fname;
                this.UploadTrcFile_And_GetMetadata(trc_fname);
            }
        }
        public void SetMontages(string sug_montage, string bp_montage)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.SuggestedMontage = sug_montage;
                this.Model.AnalizerParams.BipolarMontage = bp_montage;
            }
        }
        public void SetTimeWindow(int start_time, int stop_time)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.StartTime = start_time;
                this.Model.AnalizerParams.StopTime = stop_time;
            }
        }
        public void SetCycleTime(bool parallel_flag, int cycle_time)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                if (parallel_flag) this.Model.AnalizerParams.CycleTime = cycle_time;
                else this.Model.AnalizerParams.CycleTime = GetAnalizerParams().StopTime - GetAnalizerParams().StartTime;
            }
        }
        public void SetEvtFile(string evt_dir, string evt_fname){
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else this.Model.AnalizerParams.EvtFile = evt_dir + evt_fname;
        }
        public void SetAdvancedSettings(string hostname, string port, string log_file, string trc_temp_dir)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else {
                this.Model.API.Hostname = hostname;
                this.Model.API.Port = port;
                this.Model.LogFile = log_file;
                this.Model.TRCTempDir =  trc_temp_dir;
            }
        }
        private void UploadTrcFile_And_GetMetadata(string trc_fname)
        {
            File.Copy(trc_fname, this.GetTRCTempPath(trc_fname), true);
            /*

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
                Program.Controller.SetTrcMetadata(trc_fname);
                Program.Controller.GetScreen_EEG().UploadProgress = 0;

            }
            webClient.UploadProgressChanged += WebClientUploadProgressChanged;
            webClient.UploadFileCompleted += WebClientUploadCompleted;
            webClient.UploadFileAsync(new Uri(uri_upload), this.GetTRCTempPath(trc_fname));*/
            Program.Controller.SetTrcMetadata(trc_fname); // delete
        }
        private void SetTrcMetadata(string trc_fname)
        {
            string uri_trc_info = this.GetAPI().URI() + "trc_info/" + Path.GetFileName(trc_fname);
            string json_resp = GetJsonSync(uri_trc_info);
            TRCInfo trc_info = JsonConvert.DeserializeObject<TRCInfo>(json_resp);
            this.Model.MontageNames = trc_info.montage_names;
            this.GetScreen_Montage().LoadMontages(trc_info.montage_names);
            this.Model.TrcDuration = trc_info.recording_len_snds;
            this.GetScreen_TimeWindow().SetTRCDuration(trc_info.recording_len_snds);
            
        }
        public bool IsMetadataSetted() { return this.GetTrcDuration() != 0; }
        public void RunHFOAnalizer()
        {
            //Requires: Params have been validated and IsBusy() == false
            this.Model.IsAnalizing = true;
            this.GetScreen_Home().AbrirFormHija(this.GetScreen_AnalizerProgress());
            this.GetScreen_AnalizerProgress().StartTimer();


            this.AnalizeWith(this.GetAnalizerParams());
            this.GetScreen_AnalizerProgress().SaveAndReset_timer();
            this.Log("Getting evt from remote server...");
            this.DownloadEvt(Path.GetFileName(GetAnalizerParams().TrcFile), //The remote EvtFile name to be fetched
                                              GetAnalizerParams().EvtFile); //Where to save it
            this.Model.IsAnalizing = false;
            this.Log("Analisis finished.");
            /*this.GetScreen_AnalizerProgress().BgWorker.DoWork += (s, f) =>{
                
            };*/
            //this.GetScreen_AnalizerProgress().BgWorker.RunWorkerAsync();
        }
        private void AnalizeWith(AnalizerParams args)
        {
            MessageBox.Show("Entering AnalizeWith");
            this.GetScreen_AnalizerProgress().WorkerState = 10;
            MessageBox.Show("Passed in AnalizeWith");

            this.Log("Input trc_path: " + args.TrcFile);
            this.Log("Output xml_path: " + args.EvtFile);

            string uri_run = this.GetAPI().URI() + "run";
            Dictionary<string, string> Params = new Dictionary<string, string>();
            Params.Add("trc_fname", Path.GetFileName(args.TrcFile));
            Params.Add("str_time", args.StartTime.ToString());
            Params.Add("stp_time", args.StopTime.ToString());
            Params.Add("cycle_time", args.CycleTime.ToString());
            Params.Add("sug_montage", args.SuggestedMontage);
            Params.Add("bp_montage", args.BipolarMontage);
            string serialized_params = JsonConvert.SerializeObject(Params, new KeyValuePairConverter());
            MessageBox.Show(serialized_params); //Debug
            string run_response_str = this.PostJsonSync(uri_run, serialized_params);
            MessageBox.Show(run_response_str); //Debug

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
                    MessageBox.Show("DEBUG cycle: " + task_state_string); //Debug

                    JsonObject task_state = (JsonObject)JsonValue.Parse(task_state_string);
                    if (!task_state.ContainsKey("progress")) { MessageBox.Show(task_state["error_msg"]); break; }
                    else progress = task_state["progress"];
                    this.GetScreen_AnalizerProgress().UpdateProgress(progress);
                    Thread.Sleep(1000);
                } while (progress < 100);
            }
        }
        private void DownloadEvt(string remote_evt_fname, string dest)
        {
            MessageBox.Show("Evt downloading started.");
            string uri_get_evt = this.GetAPI().URI() + "download/evts/" + remote_evt_fname;
            using (var client = new WebClient())  
            {
                client.DownloadFile(uri_get_evt, dest);
            }
            MessageBox.Show("Evt was downloaded");

        }
        public bool IsAnalizing() { return this.Model.IsAnalizing; }

        //Conversor Logic
        public void StartEdfConversion(string edf_fname)
        {
            //Requires: Params have been validated and IsBusy() == false
            this.Model.IsConverting = true;

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
                Dictionary<string, string> suggested_mapping = this.GetChMapping(edf_fname);
                this.Model.ConversionParams = new ConversionParams(Path.GetFileName(edf_fname),
                                                                    suggested_mapping);
                //Go to confirm translations screen
                this.Model.ConvScreen_2 = new ConversorStep2(suggested_mapping);
                this.GetScreen_Home().AbrirFormHija(this.GetScreen_Conv_2());
            }
            webClient.UploadProgressChanged += WebClientUploadProgressChanged;
            webClient.UploadFileCompleted += WebClientUploadCompleted;
            webClient.UploadFileAsync(new Uri(uri_upload), edf_fname);
        }
        private Dictionary<string, string> GetChMapping(string edf_fname)
        {
            string uri_suggestion = this.GetAPI().URI() + "edf_suggested_ch_map/" + Path.GetFileName(edf_fname);
            string suggestion_response = this.GetJsonSync(uri_suggestion);
            SuggestionResponse suggestion_response_dict = JsonConvert.DeserializeObject<SuggestionResponse>(suggestion_response);
            return suggestion_response_dict.suggested_mapping;
           
        }
        public void ConfirmChMapping( Dictionary<string, string> ch_translations)
        {
            this.Model.ConversionParams.ch_names_mapping = ch_translations;
            this.Model.ConvScreen_3 = new ConversorStep3();
            this.GetScreen_Home().AbrirFormHija(this.GetScreen_Conv_3());
        }
        public void ConvertEdf(string trc_saving_dir)
        {
            string uri_edf_to_trc = this.GetAPI().URI() + "edf_to_trc";
            string serialized_conv_params = new JavaScriptSerializer().Serialize(this.GetConvParams()); 
            string run_response_str = this.PostJsonSync(uri_edf_to_trc, serialized_conv_params);
            JsonObject run_response = (JsonObject)JsonValue.Parse(run_response_str);

            if (run_response.ContainsKey("error_msg")) MessageBox.Show(run_response["error_msg"]);
            else
            {
                string pid = run_response["task_id"];
                string uri_task_state = this.GetAPI().URI() + "task_state/" + pid;
                this.GetScreen_Conv_3().Progress = 10;
                int progress = 0;
                do
                {
                    string task_state_string = this.GetJsonSync(uri_task_state);
                    JsonObject task_state = (JsonObject)JsonValue.Parse(task_state_string);
                    if (!task_state.ContainsKey("progress")) { MessageBox.Show(task_state["error_msg"]); return; }
                    else progress = task_state["progress"];
                    this.GetScreen_Conv_3().Progress = progress;
                    Thread.Sleep(1000);
                } while (progress < 100);

                string basename = Path.GetFileNameWithoutExtension(GetConvParams().edf_fname);
                string remote_trc_fname = basename + ".TRC";
                string trc_saving_path = trc_saving_dir + remote_trc_fname;
                MessageBox.Show("Conversion completed, downloading TRC from remote server...");
                this.DownloadTRC(remote_trc_fname, trc_saving_path);
                this.Model.IsConverting = false;
                this.Log("Conversion finished.");
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
        public bool IsConverting() { return this.Model.IsConverting; }

        //General Program Logic
        public void TestConnection()
        {
            string index_uri = Program.Controller.GetAPI().URI();
            string json_index_str = this.GetJsonSync(index_uri);
            JsonObject json_index = (JsonObject)JsonValue.Parse(json_index_str);
            MessageBox.Show(json_index["message"]);
        }
        private void RunOptionsAndReturnExitCode(Options opts)
        {
            if (!string.IsNullOrEmpty(opts.TrcFile)) {
                this.Model.AnalizerParams.TrcFile = (opts.TrcFile).Replace("\\", "/");
                this.GetScreen_EEG().SetTrcFile( GetAnalizerParams().TrcFile );
            }
            if (!string.IsNullOrEmpty(opts.EvtFile)) {
                this.Model.AnalizerParams.EvtFile = (opts.EvtFile).Replace("\\", "/");
                this.GetScreen_Evt().SetEvtFile( GetAnalizerParams().EvtFile);
            }
            
        }
        private void HandleParseError(IEnumerable<Error> errs)
        {
            //TODO
            throw new NotImplementedException();
        }
        private string GetTRCTempPath(string trc_fname) { return this.GetTrcTempDir() + Path.GetFileName(trc_fname); }
        private void UnavailableOptionMsg()
        {
            MessageBox.Show("This option is unavailable at this moment. Please try again later.");
        }
        public bool IsBusy() { return IsConverting() || IsAnalizing(); }

        //Extras
        /*private async Task<JsonObject> GetJsonAsync(string uri)
        {
            HttpClient httpClient = new HttpClient();
            string content = await httpClient.GetStringAsync(uri);
            return await Task.Run(() => (JsonObject)JsonValue.Parse(content));
        }
        private async Task<JsonObject> PostJsonASync(string uri, JsonObject serialized_json)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent body = new StringContent(serialized_json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uri, body);
                return (JsonObject)JsonValue.Parse(await response.Content.ReadAsStringAsync());
            }
        }*/
        private string GetJsonSync(string uri)
        {
            HttpClient httpClient = new HttpClient();
            return httpClient.GetStringAsync(uri).Result;
        }
        private string PostJsonSync(string uri, string serialized_json)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent body = new StringContent(serialized_json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PostAsync(uri, body).Result;
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        private void Log(string info)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.GetLogFile(), true))
            {
                file.WriteLine(info);
            }
        }
        /*private void PrintDict(Dictionary<string, string> aDict)
        {
            Console.WriteLine("{");
            foreach (KeyValuePair<string, string> kvp in aDict){
                Console.WriteLine("       {0} : {1}", kvp.Key, kvp.Value);
            }
            Console.WriteLine("}");
        }*/
    } 

    class Model {
        //Constructor
        public Model() {
            this.API = new API("grito.exp.dc.uba.ar", "8080");
            this.AnalizerParams = new AnalizerParams();
            this.ConversionParams = null;

            this.TrcDuration = 0;
            this.IsAnalizing = false;
            this.IsConverting = false;
            this.LogFile = Program.MainDir() + "logs/ez_detect_gui_log.txt";
            this.TRCTempDir = Program.MainDir() + "temp/";

            //Build screens 
            this.HomeScreen = new MainWindow();
            this.EEGScreen = new EEG();
            this.MontageScreen = new Montage();
            this.TimeWindowScreen = new TimeWindow();
            this.CycleTimeScreen = new CycleTime();
            this.EvtScreen = new EVT();
            this.SettingsScreen = new AdvancedSettings(this.API.Hostname, this.API.Port, this.LogFile, this.TRCTempDir);
            this.ProgressScreen = new Progress();

            this.ConvScreen_1 = new ConversorStep1();
            this.ConvScreen_2 = null;
            this.ConvScreen_3 = null;

        }

        //Collaborators
        public API API { get; set; }
        public AnalizerParams AnalizerParams { get; set; }
        public ConversionParams ConversionParams { get; set; }

        public int TrcDuration { get; set; }
        public string[] MontageNames { get; set; }
        public string LogFile { get; set; }
        public string EvtDir { get; set; }
        public string TRCTempDir { get; set; }

        public MainWindow HomeScreen { get; set; }
        public EEG EEGScreen { get; set; }
        public Montage MontageScreen { get; set; }
        public TimeWindow TimeWindowScreen { get; set; }
        public CycleTime CycleTimeScreen { get; set; }
        public EVT EvtScreen { get; set; }
        public AdvancedSettings SettingsScreen { get; set; }
        public Progress ProgressScreen { get; set; }

        public ConversorStep1 ConvScreen_1 { get; set; }
        public ConversorStep2 ConvScreen_2 { get; set; }
        public ConversorStep3 ConvScreen_3 { get; set; }

        public bool IsAnalizing { get; set; }
        public bool IsConverting { get; set; }
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
        public string edf_fname { get; set; }
        public Dictionary<string,string> ch_names_mapping { get; set; }

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
    class SuggestionResponse
    {
        public Dictionary<string, string> suggested_mapping { get; set; } //todo rename with mayus in the server
    }
    class TRCInfo
    {
        public string[] montage_names { get; set; }
        public int recording_len_snds { get; set; }
    }

}
