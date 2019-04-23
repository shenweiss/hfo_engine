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
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace HFO_ENGINE
{
    public partial class AdvancedSettings : Form
    {
        public AdvancedSettings()
        {
            InitializeComponent();
            Hostname_txt.Text = Program.Hostname;
            Port_txt.Text = Program.Port;
            Logfile_txt.Text = Program.Log_file;
            TrcTemp_txt.Text = Program.TrcTempDir;
        }

        private void AdvancedSettings_save_btn_Click(object sender, EventArgs e)
        {
            if (Program.IsAnalizing)
            {
                Program.IsRunningMessage();
            }
            else
            {
                Program.Hostname = Hostname_txt.Text;
                Program.Port = Port_txt.Text;
                Program.API_URI = "http://" + Program.Hostname + ":" + Program.Port + "/";
                Program.Log_file = Logfile_txt.Text;
                Program.TrcTempDir = TrcTemp_txt.Text;
            }
        }

        private void test_btn_Click(object sender, EventArgs e)
        {
            string index_uri = Program.API_URI;
            string json_index_str = Program.GetJsonSync(index_uri);
            JsonObject json_index = (JsonObject)JsonValue.Parse(json_index_str);
            MessageBox.Show(json_index["message"]);
        }
    }
}
