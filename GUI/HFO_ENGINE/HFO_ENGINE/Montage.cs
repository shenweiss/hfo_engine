using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace HFO_ENGINE
{
    public partial class Montage : Form
    {
     
        public Montage()
        {
            InitializeComponent();
        }
        public void LoadMontages (string[] montage_names) {
            Load_list(ComboBox_suggested_montage, montage_names);
            Load_list(ComboBox_bipolar_montage, montage_names);
        }
        private void Load_list(ComboBox C, string[] list){
            C.DataSource = list.ToArray().Clone(); ;
        }

        private void Montage_save_btn_Click(object sender, EventArgs e) {
            Program.Controller.SetMontages(
                                           ComboBox_suggested_montage.Text, 
                                           ComboBox_bipolar_montage.Text
                                           );
            
        }
    }
}
