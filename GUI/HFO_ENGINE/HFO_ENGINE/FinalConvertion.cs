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
    public partial class FinalConvertion : Form, INotifyPropertyChanged
    {
        
        //Constructor     
        public FinalConvertion( )
        {
            InitializeComponent();
        }

        //Colaborators
        private string EdfFile(){
            return Program.Controller.GetConvParams().edf_fname;
        }
        private int _uploadProgress;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Progress
        {
            get { return _uploadProgress; }
            set
            {
                _uploadProgress = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("UploadProgress"));
            }
        }

        //Methods
        private void browse_trc_out_dir_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.EdfFile()))
            {
                MessageBox.Show("Please select an EDF file prior to setting the output saving path.");
            }
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            Trc_out_conv_dir_txt.Text = dialog.SelectedPath + "\\" + Path.GetFileNameWithoutExtension(this.EdfFile()) + ".TRC";
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Trc_out_conv_dir_txt.Text))
            {
                MessageBox.Show("Please select an EDF file and set the the output saving path.");
            }
            else
            {
               Program.Controller.ConvertEdf(Trc_out_conv_dir_txt.Text);
            }
        }
    }
}
