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
    public partial class MainWindow : Form
    {
        //Constructor
        public MainWindow(){
            InitializeComponent();
        }
        private delegate void SafeCallDelegate(object formhija);

        //Buttons events
        private void Stack_menu_btn_Click(object sender, EventArgs e)
        {
            Stack_menu.Visible = !Stack_menu.Visible;
        }
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

        private string IS_BUSY_MSG = "There is already a conversion or analizing task running, please try later.";
        private void _Confirm_loosing_conv(object form)
        {
            if (MessageBox.Show("Conversion in progress will be lost. Continue?", "Analizer", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Program.Controller.SetConvFlag(false);
                AbrirFormHija(form);
            }
        }
        private bool _CanRunWith(AnalizerParams args)
        {

            //TRC file
            if (string.IsNullOrEmpty(args.TrcFile))
            {
                MessageBox.Show("Please pick a TRC file to analize in the EEG menu.");
                return false;
            }

            //Montage names
            if (!Program.Controller.GetMontageNames().Contains(args.SuggestedMontage) ||
                !Program.Controller.GetMontageNames().Contains(args.BipolarMontage))
            {
                MessageBox.Show("Please set the montages in Montage menu.");
                return false;
            }

            //Time window
            if (args.StartTime < 0 ||
                args.StopTime <= args.StartTime ||
                args.StopTime > Program.Controller.GetTrcDuration())
            {
                MessageBox.Show("Please set your Time-window settings in Time-Window menu.");
                return false;
            }

            //Cycle time
            if (args.CycleTime == 0)
            {
                Program.Controller.SetCycleTime(false, 0); //tells the controller that we are not running in parallel if cycle_time wasn't setted
            }

            //Evt file directory
            if (string.IsNullOrEmpty(args.EvtFile))
            {
                MessageBox.Show("Please set the evt saving path from the Output menu.");
                return false;
            }

            //Evt file name
            if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(args.EvtFile))) //Covers default evt fname using trc fname
            {
                string evt_dir = Path.GetDirectoryName(args.EvtFile);
                string evt_fname = Path.GetFileNameWithoutExtension(args.TrcFile) + ".evt";
                Program.Controller.SetEvtFile(evt_dir, evt_fname);
            }

            return true;
        }
        //Children Forms buttons
        public void AbrirFormHija(object formhija) {
            if (panelContenedor.InvokeRequired)
            {
                var d = new SafeCallDelegate(AbrirFormHija);
                Invoke(d, new object[] { formhija });
            }
            else
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
        }

        private void Convert_btn_Click(object sender, EventArgs e)
        {
            if (Program.Controller.IsBusy()) MessageBox.Show(IS_BUSY_MSG);
            else AbrirFormHija(Program.Controller.GetScreen_Conv_1());
        }

        private void BtnEEG_Click(object sender, EventArgs e) {
            if (Program.Controller.IsConverting()) _Confirm_loosing_conv(Program.Controller.GetScreen_EEG());
            else AbrirFormHija(Program.Controller.GetScreen_EEG()); 
        }
        private void BtnMontage_Click(object sender, EventArgs e)
        {
            if (!Program.Controller.IsMetadataSetted()) {
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else { if (Program.Controller.IsConverting()) _Confirm_loosing_conv(Program.Controller.GetScreen_Montage());
                else AbrirFormHija(Program.Controller.GetScreen_Montage());
            }
        }
        private void BtnTimeWindow_Click(object sender, EventArgs e)
        {
            if (!Program.Controller.IsMetadataSetted()){
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else
            {
                if (Program.Controller.IsConverting()) _Confirm_loosing_conv(Program.Controller.GetScreen_TimeWindow());
                else AbrirFormHija(Program.Controller.GetScreen_TimeWindow());
            }
        }
        private void BtnMultiprocessing_Click(object sender, EventArgs e) {
            if (Program.Controller.IsConverting()) _Confirm_loosing_conv(Program.Controller.GetScreen_CycleTime());
            else AbrirFormHija(Program.Controller.GetScreen_CycleTime());
        }
        private void BtnOutput_Click(object sender, EventArgs e) {
            if (Program.Controller.IsConverting()) _Confirm_loosing_conv(Program.Controller.GetScreen_Evt());
            else AbrirFormHija(Program.Controller.GetScreen_Evt());
        }
        private void BtnAdvancedSettings_Click(object sender, EventArgs e) {
            if (Program.Controller.IsConverting()) _Confirm_loosing_conv(Program.Controller.GetScreen_Settings());
            else AbrirFormHija(Program.Controller.GetScreen_Settings());
        }
         
        private void StartBtn_Click(object sender, EventArgs e) {

            //If it's already analizing just open the current progress screen
            if (Program.Controller.IsAnalizing()) {
                AbrirFormHija(Program.Controller.GetScreen_AnalizerProgress());
                return;
            } 

            //If it's converting decide to wait or continue but loosing conversion
            if (Program.Controller.IsConverting()) {
                if (MessageBox.Show("Conversion in progress will be lost. Continue?", "Analizer",
                    MessageBoxButtons.OKCancel) != DialogResult.OK) return;
                else Program.Controller.SetConvFlag(false);
            }
            //At this point we know that the controller is not busy

            //Validate API is setted
            if (string.IsNullOrEmpty(Program.Controller.GetAPI().Hostname) ||
                string.IsNullOrEmpty(Program.Controller.GetAPI().Port))
            {
                MessageBox.Show("Please set the API settings in Advanced settings menu.");
                return;
            }

            AnalizerParams args = Program.Controller.GetAnalizerParams();

            if (this._CanRunWith(args)) {
                Program.Controller.RunHFOAnalizer();
            }
        }
    }
}
