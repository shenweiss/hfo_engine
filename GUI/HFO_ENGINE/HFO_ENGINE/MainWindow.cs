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
            if (!Program.Controller.Is_TRC_metadata_setted()) {
                MessageBox.Show("Please first save the file to be used from the EEG menu.");
            }
            else AbrirFormHija( Program.Controller.GetScreen_Montage() );
        }
        private void BtnTimeWindow_Click(object sender, EventArgs e)
        {
            if (!Program.Controller.Is_TRC_metadata_setted()){
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
            Program.Controller.StartHFOAnalizer();
        }
        private void Convert_btn_Click(object sender, EventArgs e)
        {
            if (Program.Controller.GetScreen_Conv_1() == null) Program.Controller.InitConversions();
            AbrirFormHija(Program.Controller.GetScreen_Conv_1());
        }

    }
}
