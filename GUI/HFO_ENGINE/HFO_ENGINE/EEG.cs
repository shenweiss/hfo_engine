using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HFO_ENGINE;

namespace HFO_ENGINE
{
    public partial class EEG : Form, INotifyPropertyChanged
    {
        private int _uploadProgress;
        public event PropertyChangedEventHandler PropertyChanged;
        public int UploadProgress
        {
            get { return _uploadProgress; }
            set {
                _uploadProgress = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("UploadProgress"));
            }
        }

        public EEG()
        {
            InitializeComponent();
            uploadProgressBar.DataBindings.Add("Value", this, "UploadProgress");
        }
        public void SetTrcFile(string trc_fname) {
            this.TrcPath_txtBx.Text = trc_fname;
        }

        private void BrowseTRC_Btn_Click(object sender, EventArgs e){
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse TRC",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "TRC",
                Filter = "TRC files(*.TRC)| *.TRC",
                FilterIndex = 2,
                RestoreDirectory = true,
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TrcPath_txtBx.Text = openFileDialog1.FileName;
            }

        }

        private void BrowseTRC_btn_MouseLeave(object sender, EventArgs e){
            BrowseTRCbtn.Size = new Size(50, 50);
            BrowseTRCbtn.Location = new Point(150, 50);
        }

        private void BrowseTRC_btn_MouseEnter(object sender, EventArgs e){
            BrowseTRCbtn.Size = new Size(60, 60);
            BrowseTRCbtn.Location = new Point(145, 45);
        }

        private void EEG_save_btn_Click(object sender, EventArgs e){
            Program.Controller.SetTrcFile(this.TrcPath_txtBx.Text);
        }

    }
}
