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

namespace HFO_ENGINE
{
    class ProgressResponse
    {
        public int progress { get; set; }
        public string error_msg { get; set; }
    }

    class TRCInfo
    {
        public string[] montage_names { get; set; }
        public int recording_len_snds { get; set; }
    }
    class ChannelMappingSuggestion {
        public  Dictionary<String, Dictionary<String, String> > value { get; set; }
    }

    class ConversionParams
    {
        public ConversionParams(string edf_filename, Dictionary<String, String> _suggested_mapping) {
            edf_fname = edf_filename;
            ch_names_mapping = _suggested_mapping;
        }
        public Dictionary<String, String> ch_names_mapping { get; set; }
        public string edf_fname { get; set; }
    }
    static class Program
    {
        //API
        public static readonly HttpClient client = new HttpClient();
        public static string Hostname { get; set; } = "grito.exp.dc.uba.ar"; //grito
        public static string Port { get; set; } = "8080";
        public static string API_URI = "http://" + Hostname + ":" + Port + "/";


        //Conversor logic
        public static ConversionParams ConversionParameters;


        //Analizer logic
        public static string TrcFile { get; set; } = "";
        public static string[] Montage_names;
        public static string SuggestedMontage { get; set; } = "";
        public static string BpMontage { get; set; } = "";
        public static int StartTime { get; set; } = 0;
        public static int StopTime { get; set; } = 0;
        public static int Trc_duration { get; set; } = 0;
        public static int CycleTime { get; set; } = -1;
        public static bool MultiProcessingEnabled { get; set; } = false;
        public static int CycleTimeMin { get; set; } = 0;
        public static string EvtFile { get; set; } = "";

        public static string mainDir = AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
        public static string Log_file { get; set; } = mainDir + "logs/ez_detect_gui_log.txt";
        public static string TrcTempDir { get; set; } = mainDir + "temp/";
        public static string TrcTempPath { get; set; } = "";

        public static bool IsAnalizing { get; set; } = false;

        public static Form mainForm;
        public static Progress ProgressScreen;

        class Options
        {
            [Option('t', "trc", Required = false,
             HelpText = "Full path to input trc file to be processed.")]
            public string TrcFile { get; set; }

            [Option('x', "xml", Required = false,
              HelpText = "Full path to output evt file to be saved.")]
            public string EvtFile { get; set; }

            // Omitting long name, defaults to name of property, ie "--verbose"
            [Option(
              Default = false,
              HelpText = "Prints all messages to standard output.")]
            public bool Verbose { get; set; }
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            //TODO
            throw new NotImplementedException();
        }
        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            if (!string.IsNullOrEmpty(opts.TrcFile)) TrcFile = (opts.TrcFile).Replace("\\", "/");
            if (!string.IsNullOrEmpty(opts.EvtFile)) EvtFile = (opts.EvtFile).Replace("\\", "/");
        }

        public static async Task<JsonObject> GetJsonAsync(string uri) {
            HttpClient httpClient = new HttpClient();
            string content = await httpClient.GetStringAsync(uri);
            return await Task.Run(() => (JsonObject)JsonValue.Parse(content));
        }
        public static async Task<JsonObject> PostJsonASync(string uri, JsonObject serialized_json)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                StringContent body = new StringContent(serialized_json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(uri, body);
                return (JsonObject)JsonValue.Parse(await response.Content.ReadAsStringAsync());
            }
        }

        public static string GetJsonSync(string uri) {
            HttpClient httpClient = new HttpClient();
            return httpClient.GetStringAsync(uri).Result;
            //return (JsonObject)JsonValue.Parse(content);
        }
        public static string PostJsonSync(string uri, string serialized_json)
        {
            using (HttpClient httpClient = new HttpClient()) {
                StringContent body = new StringContent(serialized_json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = httpClient.PostAsync(uri, body).Result;
                return response.Content.ReadAsStringAsync().Result;
                //return (JsonObject)JsonValue.Parse(response.Content.ReadAsStringAsync().Result);
            }
        }


        private static async Task<JsonObject> UploadFileAsync(string fname, string uri)
        {
            using (FileStream fileStream = File.OpenRead(fname))
            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent formData = new MultipartFormDataContent())
            {
                HttpContent fileStreamContent = new StreamContent(fileStream);
                formData.Add(fileStreamContent, "file", "file");
                Task<HttpResponseMessage> response = client.PostAsync(uri, formData);
                Task<string> string_response = (await response).Content.ReadAsStringAsync();
                return (JsonObject)JsonValue.Parse(await string_response);
            }
        } 
        
        private static JsonObject UploadFile(string fname, string uri)
        {
            using (FileStream fileStream = File.OpenRead(fname))
            using (HttpClient client = new HttpClient())
            using (MultipartFormDataContent formData = new MultipartFormDataContent())
            {
                HttpContent fileStreamContent = new StreamContent(fileStream);
                formData.Add(fileStreamContent, "file", "file");
                HttpResponseMessage response = client.PostAsync(uri, formData).Result;
                string string_response = response.Content.ReadAsStringAsync().Result;
                return (JsonObject)JsonValue.Parse(string_response);
            }
        } //sync

        public static async void UploadTRCAsync() {
            TrcTempPath = TrcTempDir + Path.GetFileName(TrcFile);
            MessageBox.Show(TrcTempPath);
            File.Copy(TrcFile, TrcTempPath, true);
            string uri_upload = API_URI + "upload";
            Task<JsonObject> status = UploadFileAsync(TrcTempPath, uri_upload);
            MessageBox.Show((await status)["message"]);
        }
        public static void GetTRC_metadata() {
            string uri_trc_info = API_URI + "trc_info/" + Path.GetFileName(TrcFile);
            string json_resp = GetJsonSync(uri_trc_info);
            TRCInfo trc_info = JsonConvert.DeserializeObject<TRCInfo>(json_resp);
            Trc_duration = trc_info.recording_len_snds;
            Montage_names = trc_info.montage_names;
        }

        public static void StartEDFConversion(string edf_fname, MainWindow main_form) {
            UploadEDF(edf_fname);
            string uri_suggestion = API_URI + "edf_suggested_ch_map/" + Path.GetFileName(edf_fname);
            string suggestion_response = GetJsonSync(uri_suggestion); //se asume que se el edf subio bien, sino salta un error antes
            Dictionary<String, String> ch_mapping = JsonConvert.DeserializeObject<ChannelMappingSuggestion>(suggestion_response).value["suggested_mapping"];
            ConversionParameters = new ConversionParams(Path.GetFileName(edf_fname), ch_mapping); //guardo la sugerencia del server
            main_form.AbrirFormHija(new Translation(main_form)); //Abro ventana para que el usuario valide el mapeo de nombres de canales
        }
        public static void UploadEDF(string edf_fname) {
            string uri_upload = API_URI + "upload";
            JsonObject status = UploadFile(edf_fname, uri_upload);
            MessageBox.Show(status["message"]);
        }
        public static void GoToFinalConversion(MainWindow main_form) {
            main_form.AbrirFormHija(new FinalConvertion());
        }
        public static void ConvertEDF(string TrcSavingPath)
        {
            string uri_edf_to_trc = API_URI + "edf_to_trc/";
            string serialized_conv_params = new JavaScriptSerializer().Serialize(ConversionParameters);
            string run_response_str = PostJsonSync(uri_edf_to_trc, serialized_conv_params);
            JsonObject run_response = (JsonObject)JsonValue.Parse(run_response_str);
            if (run_response.ContainsKey("error_msg")) MessageBox.Show(run_response["error_msg"]);
            else
            {
                string pid = run_response["task_id"];
                string uri_task_state = Program.API_URI + "task_state/" + pid;
                int progress = 0;
                do
                {
                    string task_state_string = Program.GetJsonSync(uri_task_state);
                    JsonObject task_state = (JsonObject)JsonValue.Parse(task_state_string);
                    if (!task_state.ContainsKey("progress")) { MessageBox.Show(task_state["error_msg"]); return; }
                    else progress = task_state["progress"];
                    MessageBox.Show(progress.ToString());
                    //UpdateProgress(progress);
                    Thread.Sleep(1000);
                } while (progress < 100);
                DownloadTRC(Path.GetFileName(Program.TrcFile), TrcSavingPath); //the remote EvtFile name is created this way
            }

            /*HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(URI);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";
            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())){
                string json = new JavaScriptSerializer().Serialize(ConversionParameters);
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }*/



            //Download TRC


            /*HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (Stream httpResponseStream = httpResponse.GetResponseStream()){
                int bufferSize = 1024;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                FileStream fileStream = File.Create(TrcSavingPath);
                while ((bytesRead = httpResponseStream.Read(buffer, 0, bufferSize)) != 0){
                    fileStream.Write(buffer, 0, bytesRead);
                } // end while
            }*/
        }
        public static void DownloadTRC(string remote_trc_fname, string trc_saving_path) {
            string uri_get_trc = Program.API_URI + "download/TRCs/" + remote_trc_fname;
            using (var client = new WebClient()){
                client.DownloadFile(uri_get_trc, trc_saving_path);
            }
        }

        public static void IsRunningMessage() {
            MessageBox.Show("You can't modify settings while a previous job is still running. Please wait for it to finish.");
        }

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts))
              .WithNotParsed<Options>((errs) => HandleParseError(errs));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            mainForm  = new MainWindow();
            Application.Run(mainForm);

            
        }
    }
}
