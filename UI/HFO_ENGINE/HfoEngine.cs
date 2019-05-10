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
                this.Log(String.Format("TrcFile setted to {0}", trc_fname));
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
                this.Log(String.Format("Montages setted to Suggested: {0} and Bipolar: {1}", sug_montage, bp_montage));

            }
        }
        public void SetTimeWindow(int start_time, int stop_time)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.StartTime = start_time;
                this.Model.AnalizerParams.StopTime = stop_time;
                this.Log(String.Format("Time-window setted to [{0} , {1})  ", start_time, stop_time));

            }
        }
        public void SetCycleTime(bool parallel_flag, int cycle_time)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                if (parallel_flag) this.Model.AnalizerParams.CycleTime = cycle_time;
                else this.Model.AnalizerParams.CycleTime = GetAnalizerParams().StopTime - GetAnalizerParams().StartTime;
                this.Log(String.Format("Cycletime setted"));
            }
        }
        public void SetEvtFile(string evt_dir, string evt_fname){
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else
            {
                this.Model.AnalizerParams.EvtFile = evt_dir + evt_fname;
                this.Log(String.Format("Evt saving path setted to {0}", evt_dir + evt_fname));
            }
        }
        public void SetAdvancedSettings(string hostname, string port, string log_file, string trc_temp_dir)
        {
            if (this.IsBusy()) this.UnavailableOptionMsg();
            else {
                this.Model.API.Hostname = hostname;
                this.Model.API.Port = port;
                this.Model.LogFile = log_file;
                this.Model.TRCTempDir =  trc_temp_dir;
                this.Log(String.Format("Setting advanced parameters {0} {1}, {2}, {3}", 
                        hostname, port, log_file, trc_temp_dir));

            }
        }
        private void UploadTrcFile_And_GetMetadata(string trc_fname)
        {
            File.Copy(trc_fname, this.GetTRCTempPath(trc_fname), true);
            Program.Controller.GetScreen_EEG().UpdateProgressDesc("Uploading TRC to the server...");
            string uri_upload = this.GetAPI().URI() + "upload";
            WebClient webClient = new WebClient();
            void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
            {
                Console.WriteLine("Upload {0}% complete. ", e.ProgressPercentage);
                Program.Controller.GetScreen_EEG().UploadProgress = (int)e.ProgressPercentage;
            }
            void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
            {
                this.Log("Upload TRC was completed.");
                this.GetScreen_EEG().UpdateProgressDesc("Setting TRC metadata...");
                Program.Controller.GetScreen_EEG().UploadProgress = 20;
                Program.Controller.SetTrcMetadata(trc_fname);
                this.GetScreen_EEG().UpdateProgressDesc("");
                Program.Controller.GetScreen_EEG().UploadProgress = 0;
            }
            webClient.UploadProgressChanged += WebClientUploadProgressChanged;
            webClient.UploadFileCompleted += WebClientUploadCompleted;
            webClient.UploadFileAsync(new Uri(uri_upload), this.GetTRCTempPath(trc_fname));
            //Program.Controller.SetTrcMetadata(trc_fname); // mock if trc is already uploaded
        } 
        private void SetTrcMetadata(string trc_fname)
        {
            this.Log("Setting TRC metadata");
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
            this.Log("Starting TRC analisis.");
            this.Model.IsAnalizing = true;
            this.GetScreen_Home().AbrirFormHija(this.GetScreen_AnalizerProgress());
            this.GetScreen_AnalizerProgress().StartTimer();

            Thread hfo_analisis = new Thread( () =>
                {
                    this.GetScreen_AnalizerProgress().UpdateProgressSafe(1);
                    this.GetScreen_AnalizerProgress().UpdateProgressDescSafe("Analizing TRC in remote server...");
                    this.AnalizeWith(this.GetAnalizerParams());
                    this.GetScreen_AnalizerProgress().SaveAndReset_timer_Safe();

                    this.GetScreen_AnalizerProgress().UpdateProgressSafe(1);
                    this.GetScreen_AnalizerProgress().UpdateProgressDescSafe("Downloading detected events as evt file...");
                    string remote_evt_fname = Path.GetFileNameWithoutExtension(GetAnalizerParams().TrcFile) + ".evt";
                    this.DownloadEvt(remote_evt_fname, GetAnalizerParams().EvtFile);
                    this.GetScreen_AnalizerProgress().UpdateProgressSafe(100);
                    this.GetScreen_AnalizerProgress().UpdateProgressDescSafe("Evt was downloaded.");

                    MessageBox.Show("Analisis finished."); //Reseting for next run
                    this.GetScreen_AnalizerProgress().UpdateProgressSafe(0);
                    this.GetScreen_AnalizerProgress().UpdateProgressDescSafe("");

                    this.Model.IsAnalizing = false;
                    this.Log("Analisis thread finished");
                }
            );
            hfo_analisis.Start();
        }
        private void AnalizeWith(AnalizerParams args)
        {
            //Analizer call
            string uri_run = this.GetAPI().URI() + "hfo_analizer";
            Dictionary<string, string> Params = new Dictionary<string, string>
            {
                { "trc_fname", Path.GetFileName(args.TrcFile) },
                { "str_time", args.StartTime.ToString() },
                { "stp_time", args.StopTime.ToString() },
                { "cycle_time", args.CycleTime.ToString() },
                { "sug_montage", args.SuggestedMontage },
                { "bp_montage", args.BipolarMontage }
            };
            string serialized_params = JsonConvert.SerializeObject(Params, new KeyValuePairConverter());

            this.Log(String.Format("Analisis request with params: {0},{1},{2},{3},{4},{5}",
                            Params["trc_fname"], Params["str_time"], Params["stp_time"],
                            Params["cycle_time"], Params["sug_montage"], Params["bp_montage"]));
            string run_response_str = this.PostJsonSync(uri_run, serialized_params);

            //Analizer response
            JsonObject run_response = (JsonObject)JsonValue.Parse(run_response_str);

            if (run_response.ContainsKey("error_msg")) MessageBox.Show(run_response["error_msg"]);
            else
            {
                //Keep updating progress bar while working remotely
                string pid = run_response["task_id"];
                string uri_task_state = this.GetAPI().URI() + "task_state/" + pid;
                int progress = 0;
                do
                {
                    string task_state_string = this.GetJsonSync(uri_task_state);
                    //ensures: status code is OK and progress is defined
                    JsonObject task_state = (JsonObject)JsonValue.Parse(task_state_string);
                    progress = task_state["progress"];
                    this.GetScreen_AnalizerProgress().UpdateProgressSafe(progress);
                    Thread.Sleep(1000);
                } while (progress < 100);
                this.Log("Analizer has finished remotely.");
            }
        }
        private void DownloadEvt(string remote_evt_fname, string dest)
        {
            this.Log("Downloading evt");
            string uri_get_evt = this.GetAPI().URI() + "download/evts/" + remote_evt_fname;
            using (var client = new WebClient()) client.DownloadFile(uri_get_evt, dest);
        }
        public bool IsAnalizing() { return this.Model.IsAnalizing; }

        //Conversor Logic
        public void StartEdfConversion(string edf_fname)
        {
            //Requires: Params have been validated and IsBusy() == false
            this.Model.IsConverting = true;
            this.GetScreen_Conv_1().UpdateProgressDescSafe("Uploading edf to the server...");

            string uri_upload = this.GetAPI().URI() + "upload";
            WebClient webClient = new WebClient();
            void WebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
            {
                Console.WriteLine("Uploading edf to the server... {0}% complete.", e.ProgressPercentage);
                this.GetScreen_Conv_1().UpdateProgressSafe(e.ProgressPercentage);
            }
            void WebClientUploadCompleted(object sender, UploadFileCompletedEventArgs e)
            {
                this.GetScreen_Conv_1().UpdateProgressDescSafe("Getting suggested channel name mapping...");
                this.GetScreen_Conv_1().UpdateProgressSafe(15);
                Dictionary<string, string> suggested_mapping = this.GetChMapping(edf_fname);
                this.GetScreen_Conv_1().UpdateProgressSafe(50);

                this.Model.ConversionParams = new ConversionParams(Path.GetFileName(edf_fname),
                                                                    suggested_mapping);
                this.GetScreen_Conv_1().UpdateProgressSafe(65);

                //Go to confirm translations screen
                this.Model.ConvScreen_2 = new ConversorStep2(suggested_mapping);
                this.GetScreen_Conv_1().UpdateProgressSafe(100);
                this.GetScreen_Conv_1().UpdateProgressDescSafe("");
                this.GetScreen_Conv_1().UpdateProgressSafe(0);
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
            this.GetScreen_Conv_3().UpdateProgressDescSafe("Performing conversion in remote server...");
            this.GetScreen_Conv_3().UpdateProgressSafe(5);

            Thread conversion = new Thread( () => {
                string uri_edf_to_trc = this.GetAPI().URI() + "edf_to_trc";
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
                        //ensures: status code is OK, progress is defined
                        progress = task_state["progress"];
                        this.GetScreen_Conv_3().UpdateProgressSafe(progress);
                        Thread.Sleep(1000);
                    } while (progress < 100);
                    this.Log("Conversion completed.");

                    string basename = Path.GetFileNameWithoutExtension(GetConvParams().Edf_fname);
                    string remote_trc_fname = basename + ".TRC";
                    string trc_saving_path = trc_saving_dir + remote_trc_fname;
                    this.DownloadTRC(remote_trc_fname, trc_saving_path);
                }
                this.Model.IsConverting = false;
                }
            );
            conversion.Start();
           
        }
        public void DownloadTRC(string remote_trc_fname, string trc_saving_path)
        {
            this.GetScreen_Conv_3().UpdateProgressSafe(0);
            this.GetScreen_Conv_3().UpdateProgressDescSafe("Downloading converted TRC from remote server...");

            WebClient webClient = new WebClient();
            void WebClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                Console.WriteLine("Downloading converted TRC {0}% complete... ", e.ProgressPercentage);
                this.GetScreen_Conv_3().UpdateProgressSafe(e.ProgressPercentage);
            }
            void WebClientDownloadCompleted(object sender, EventArgs e)
            {
                this.GetScreen_Conv_3().UpdateProgressSafe(100);
                this.GetScreen_Conv_3().UpdateProgressDescSafe("Download has finished.");

                this.Log("Converted TRC download is complete.");
            }
            webClient.DownloadProgressChanged += WebClientDownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClientDownloadCompleted;

            string uri_download_trc = this.GetAPI().URI() + "download/TRCs/" + remote_trc_fname;
            webClient.DownloadFileAsync(new Uri(uri_download_trc), trc_saving_path);
        }
        public bool IsConverting() { return this.Model.IsConverting; }
        public void SetConvFlag(bool flag) { this.Model.IsConverting = flag; }

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
        private string GetJsonSync(string uri)
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync(uri).Result;

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Unsuccesfull status code from server: " + response.StatusCode.ToString() );
            }
            return response.Content.ReadAsStringAsync().Result;
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
            bool log_activated = false;
            if (log_activated)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(this.GetLogFile(), true))
                {
                    file.WriteLine(info);
                }
            }
        }
        
    } 

    class Model {
        //Constructor
        public Model() {
            this.API = new API("grito.exp.dc.uba.ar", "8080");
            this.AnalizerParams = new AnalizerParams();
            this.ConversionParams = null;

            this.TrcDuration = 0;
            this.MontageNames = new string[] { };
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

            this.IsAnalizing = false;
            this.IsConverting = false;
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
            Edf_fname = edf_filename;
            ch_names_mapping = _suggested_mapping;
        }

        //Colaborators
        public string Edf_fname { get; set; }
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
