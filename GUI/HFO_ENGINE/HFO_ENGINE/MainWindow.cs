using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.Net;
using System.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HFO_ENGINE

{
    
    public partial class MainWindow : Form, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
        }
      
        
        private BackgroundWorker _bgWorker = new BackgroundWorker();
        private int _workerState;
        public event PropertyChangedEventHandler PropertyChanged;
        public int WorkerState
        {
            get { return _workerState; }
            set
            {
                _workerState = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("WorkerState"));
            }
        }

        public void UpdateProgress(int progressState) { this.WorkerState = progressState; }

        public CycleTime CycleTime_Form = null;
        public EEG EEG_Form = null;

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to Quit?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void btnMaximizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            btnMaximizar.Visible = false;
            btnRestaurar.Visible = true;
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            btnRestaurar.Visible = false;
            btnMaximizar.Visible = true;
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void BarraTitulo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }


        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]

        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
     
        public void AbrirFormHija(object formhija)
        {
            if (this.panelContenedor.Controls.Count > 0)
                this.panelContenedor.Controls.RemoveAt(0);
            Form fh = formhija as Form;
            fh.TopLevel = false;
            fh.Dock = DockStyle.Fill;
            this.panelContenedor.Controls.Add(fh);
            this.panelContenedor.Tag = fh;
            fh.Show();
        }
        private void BtnEEG_Click(object sender, EventArgs e)
        {
            if (EEG_Form == null ) EEG_Form = new EEG();
            AbrirFormHija(EEG_Form);
        }

        private void BtnMontage_Click(object sender, EventArgs e)
        {
            if ( Program.Trc_duration == 0)
            {
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else AbrirFormHija(new Montage());
        }

        private void BtnTimeWindow_Click(object sender, EventArgs e)
        {
            if (Program.Trc_duration == 0)
            {
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else AbrirFormHija(new TimeWindow());
        }
        private void BtnMultiprocessing_Click(object sender, EventArgs e)
        {
            if (CycleTime_Form == null) CycleTime_Form = new CycleTime();
            AbrirFormHija(CycleTime_Form);
        }

        private void BtnOutput_Click(object sender, EventArgs e)
        {
            AbrirFormHija(new EVT());
        }

        private void BtnAdvancedSettings_Click(object sender, EventArgs e)
        {
            AbrirFormHija(new AdvancedSettings());
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {

            if (Program.IsAnalizing)
            {
                AbrirFormHija(Program.ProgressScreen);
            }
            else
            {
                if (string.IsNullOrEmpty(Program.TrcFile)) {
                    MessageBox.Show("Please upload a TRC file from the EEG menu.");
                }
                else if (Program.SuggestedMontage == "" || Program.BpMontage == ""){
                    MessageBox.Show("Please select the montages in Montage menu.");
                }
                else if (Program.StartTime >= Program.StopTime || Program.StopTime > Program.Trc_duration){
                    MessageBox.Show("Invalid time windows, please review your Time-window settings.");
                }
                else if ((bool)Program.MultiProcessingEnabled && Program.CycleTime == -1){
                    MessageBox.Show("Please select a cycle time.");
                }
                else if (string.IsNullOrEmpty(Program.EvtFile)){
                    MessageBox.Show("Please select the evt saving path from the Output menu.");
                }
                else{
                    if (Program.CycleTime == -1) Program.CycleTime = Program.StopTime - Program.StartTime + 1;
                    Program.IsAnalizing = true;
                    Program.ProgressScreen = new Progress();
                    _bgWorker.DoWork += (s, f) =>
                    {
                        this.RunHFOEngine();
                        
                    };
                    _bgWorker.RunWorkerAsync();

                    //new Thread(() => //creo que estoy tirando 2 threads revisar
                    //{
                    //Thread.CurrentThread.IsBackground = true;
                    //}).Start();
                    AbrirFormHija(Program.ProgressScreen);

                }
                //CloseWithMessage("Calculation has finished. The events will automatically load to Brain Quick if the evt saving path was ok.");

            }

        }

        public void RunHFOEngine()
        {
            UpdateProgress(1);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(Program.Log_file, true)) { file.WriteLine("Running HfoAnnotate App..."); }
            string lines = "Input trc_path: " + Program.TrcFile + Environment.NewLine +
                           "Output xml_path: " + Program.EvtFile + Environment.NewLine;
            File.WriteAllText(Program.Log_file, lines);

            string uri_run = Program.API_URI + "run";
            Dictionary<string, string> hfo_params = new Dictionary<string, string>();
            hfo_params.Add("trc_fname", Path.GetFileName(Program.TrcFile));
            hfo_params.Add("str_time", Program.StartTime.ToString());
            hfo_params.Add("stp_time", Program.StopTime.ToString());
            hfo_params.Add("cycle_time", Program.CycleTime.ToString());
            hfo_params.Add("sug_montage", Program.SuggestedMontage);
            hfo_params.Add("bp_montage", Program.BpMontage);
            string serialized_params = JsonConvert.SerializeObject(hfo_params, new KeyValuePairConverter());

            string run_response_str = Program.PostJsonSync(uri_run, serialized_params);
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
                    if (!task_state.ContainsKey("progress")) { MessageBox.Show(task_state["error_msg"]); break; }
                    else progress = task_state["progress"];
                    UpdateProgress(progress);
                    Thread.Sleep(1000);
                } while (progress < 100);
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Program.Log_file, true)) { file.WriteLine("Getting evt from remote server..."); }
                DownloadEvt(Path.GetFileName(Program.TrcFile)); //the remote EvtFile name is created this way
                UpdateProgress(100);
                Program.ProgressScreen.Stop_timer();
                Program.IsAnalizing = false;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Program.Log_file, true)) { file.WriteLine("Exiting."); }
            }
        }

        private void DownloadEvt(string remote_evt_fname)
        {
            string uri_get_evt = Program.API_URI + "download/evts/" + remote_evt_fname;
            using (var client = new WebClient()){
                client.DownloadFile(uri_get_evt, Program.EvtFile);
            }
        }

        private void Stack_menu_btn_Click(object sender, EventArgs e)
        {
            Stack_menu.Visible = !Stack_menu.Visible;
        }

        private void Convert_btn_Click(object sender, EventArgs e)
        {
            AbrirFormHija(new Fastwave_conversor(this));
        }

        
    }
}
