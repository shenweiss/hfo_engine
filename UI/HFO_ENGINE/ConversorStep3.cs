using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFO_ENGINE
{
    public partial class ConversorStep3 : Form, INotifyPropertyChanged
    {
        //Constructor     
        public ConversorStep3()
        {
            InitializeComponent();
            ConvProgressBar.DataBindings.Add("Value", this, "Progress");
        }

        //Colaborators
        private string EdfFile(){
            return Program.Controller.GetConvParams().Edf_fname;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private int _Progress;
        public int Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Progress"));
            }
        }
        private delegate void ProgressSafeCallDelegate(int progress);
        public void UpdateProgressSafe(int progress)
        {
            if (ConvProgressBar.InvokeRequired)
            {
                var d = new ProgressSafeCallDelegate(UpdateProgressSafe);
                Invoke(d, new object[] { progress });
            }
            else
            {
                this.Progress = progress;
            }
        }
        private delegate void ProgressDescSafeCallDelegate(string description);
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
        private void Browse_trc_out_dir_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.EdfFile()))
            {
                MessageBox.Show("Please select an EDF file prior to setting the output saving path.");
            }
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            Trc_out_conv_dir_txt.Text = dialog.SelectedPath + "\\" ;
        }
        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Trc_out_conv_dir_txt.Text))
            {
                MessageBox.Show("Please set the the output saving path.");
            }
            else
            {
               Program.Controller.ConvertEdf(Trc_out_conv_dir_txt.Text);
            }
        }
    }
}
