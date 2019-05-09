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
    public partial class ConversorStep1 : Form, INotifyPropertyChanged
    {
        //Constructor
        public ConversorStep1()
        {
            InitializeComponent();
            uploadProgressBar.DataBindings.Add("Value", this, "Progress");
        }

        //Colaborators 
        private int _progress;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Progress"));
            }
        }
        private delegate void ProgressSafeCallDelegate(int progress);
        private delegate void ProgressDescSafeCallDelegate(string description);

        public void UpdateProgressSafe(int progress)
        {
            if (uploadProgressBar.InvokeRequired)
            {
                var d = new ProgressSafeCallDelegate(UpdateProgressSafe);
                Invoke(d, new object[] { progress });
            }
            else
            {
                this.Progress = progress;
            }
        }

        public void UpdateProgressDescSafe(string description)
        {
            if (Progress_label.InvokeRequired)
            {
                var d = new ProgressDescSafeCallDelegate(UpdateProgressDescSafe);
                Invoke(d, new object[] { description });
            }
            else
            {
                this.Progress_label.Text = description;
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

        private void Conversor_start_btn_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(EdfPath_txtBx.Text))
                MessageBox.Show("Please select an EDF file to be converted.");
            else
            {
                if (Program.Controller.IsBusy()) MessageBox.Show("There is already a task running. Try again later.");
                else Program.Controller.StartEdfConversion(EdfPath_txtBx.Text);
            }
        }

    }
  
}
