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

        //Children Forms buttons
        public void AbrirFormHija(object formhija) {
            if (this.panelContenedor.Controls.Count > 0)
                this.panelContenedor.Controls.RemoveAt(0);
            Form fh = formhija as Form;
            fh.TopLevel = false;
            fh.Dock = DockStyle.Fill;
            this.panelContenedor.Controls.Add(fh);
            this.panelContenedor.Tag = fh;
            fh.Show();
        }

        private void BtnEEG_Click(object sender, EventArgs e) {
            AbrirFormHija(Program.Controller.GetScreen_EEG());
        }
        private void BtnMontage_Click(object sender, EventArgs e)
        {
            if (!Program.Controller.IsMetadataSetted()) {
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else AbrirFormHija( Program.Controller.GetScreen_Montage() );
        }
        private void BtnTimeWindow_Click(object sender, EventArgs e)
        {
            if (!Program.Controller.IsMetadataSetted()){
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else AbrirFormHija( Program.Controller.GetScreen_TimeWindow() );
        }
        private void BtnMultiprocessing_Click(object sender, EventArgs e) {
            AbrirFormHija( Program.Controller.GetScreen_CycleTime());
        }
        private void BtnOutput_Click(object sender, EventArgs e) {
            AbrirFormHija(Program.Controller.GetScreen_Evt());
        }
        private void BtnAdvancedSettings_Click(object sender, EventArgs e) {
            AbrirFormHija( Program.Controller.GetScreen_Settings());
        }

        private void StartBtn_Click(object sender, EventArgs e) {
            if (Program.Controller.IsAnalizing()) AbrirFormHija(Program.Controller.GetScreen_AnalizerProgress());
            else if (Program.Controller.IsConverting()) MessageBox.Show("Please complete the conversion prior to analizing.");
            else
               {
                AnalizerParams args = Program.Controller.GetAnalizerParams();

                //Check that all params have been setted by the user
                if (string.IsNullOrEmpty(args.TrcFile))
                {
                    MessageBox.Show("Please pick a TRC file to analize in the EEG menu.");
                }
                else if (string.IsNullOrEmpty(args.SuggestedMontage) || string.IsNullOrEmpty(args.BipolarMontage))
                {
                    MessageBox.Show("Please set the montages in Montage menu.");
                }
                else if (args.StopTime == 0) {
                    MessageBox.Show("Please set your Time-window settings in Time-Window menu.");
                }
                else if (args.CycleTime == 0) {
                    Program.Controller.SetCycleTime(false, 0); //tells the controller that we are not running in parallel if cycle_time wasn't setted
                }
                else if (string.IsNullOrEmpty(args.EvtFile))
                {
                    MessageBox.Show("Please set the evt saving path from the Output menu.");
                }
                else if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(args.EvtFile))) //Cover default evt fname using trc fname
                {
                    string evt_dir = Path.GetDirectoryName(args.EvtFile);
                    string evt_fname = Path.GetFileNameWithoutExtension(args.TrcFile) + ".evt";
                }
                else if (string.IsNullOrEmpty(Program.Controller.GetAPI().Hostname) ||
                         string.IsNullOrEmpty(Program.Controller.GetAPI().Port))
                {
                    MessageBox.Show("Please set the API settings in Advanced settings menu.");
                }
                else
                {
                    Program.Controller.RunHFOAnalizer(); //From now on we know we are working with valid params.
                }
            }
        }
        private void Convert_btn_Click(object sender, EventArgs e)
        {
            if (Program.Controller.IsBusy()) MessageBox.Show("There is already a conversion or analizing task running, please try later.");
            else AbrirFormHija(Program.Controller.GetScreen_Conv_1());
        }

    }
}
