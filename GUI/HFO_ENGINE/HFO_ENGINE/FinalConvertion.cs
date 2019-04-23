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
    public partial class FinalConvertion : Form
    {
        private string edf_fname;
        public FinalConvertion( )
        {
            InitializeComponent();
            edf_fname = Program.ConversionParameters.edf_fname;
        }


        private void browse_trc_out_dir_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(edf_fname))
            {
                MessageBox.Show("Please select an EDF file prior to setting the output saving path.");
            }
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            Trc_out_conv_dir_txt.Text = dialog.SelectedPath + "\\" + Path.GetFileNameWithoutExtension(edf_fname) + ".TRC";
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(edf_fname) || String.IsNullOrEmpty(Trc_out_conv_dir_txt.Text))
            {
                MessageBox.Show("Please select an EDF file and set the the output saving path.");
            }
            else
            {
                Program.ConvertEDF(Trc_out_conv_dir_txt.Text);
            }
        }
    }
}
