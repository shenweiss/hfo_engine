using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFO_ENGINE
{
    public partial class Fastwave_conversor : Form, INotifyPropertyChanged
    {
        //Constructor
        public Fastwave_conversor()
        {
            InitializeComponent();
            uploadProgressBar.DataBindings.Add("Value", this, "UploadProgress");
        }

        //Colaborators 
        private int _uploadProgress;
        public event PropertyChangedEventHandler PropertyChanged;
        public int UploadProgress
        {
            get { return _uploadProgress; }
            set
            {
                _uploadProgress = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("UploadProgress"));
            }
        }

        //Methods
        private void SelectEDFbtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                Title = "Browse EDF",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "edf",
                Filter = "EDF files(*.edf)| *.edf",
                FilterIndex = 2,
                RestoreDirectory = true,
            };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                EdfPath_txtBx.Text = openFileDialog1.FileName;
            }
        }

        private void conversor_start_btn_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(EdfPath_txtBx.Text))
                MessageBox.Show("Please select an EDF file to be converted.");
            else
                Program.Controller.StartEdfConversion(EdfPath_txtBx.Text);
        }
        
        
    }
  
}
