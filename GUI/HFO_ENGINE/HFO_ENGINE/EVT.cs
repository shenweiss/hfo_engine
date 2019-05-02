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
    public partial class EVT : Form
    {
        public EVT()
        {
            InitializeComponent();
        }
        public void SetEvtFile(string evt_fname)
        {
            this.EvtPath_txtBx.Text = Path.GetDirectoryName(evt_fname);
            this.Evt_fname_txtBox.Text = Path.GetFileNameWithoutExtension(evt_fname);
        }

        private void Browse_evt_dir_btn_Click(object sender, EventArgs e) {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            EvtPath_txtBx.Text = dialog.SelectedPath + "\\";
        }

        private void Browse_evt_dir_btn_MouseLeave(object sender, EventArgs e)
        {
            Browse_evt_dir_btn.Size = new Size(50, 50);
            Browse_evt_dir_btn.Location = new Point(150, 50);
        }
        private void Browse_evt_dir_btn_MouseEnter(object sender, EventArgs e)
        {
            Browse_evt_dir_btn.Size = new Size(60, 60);
            Browse_evt_dir_btn.Location = new Point(145, 45);
        }

        private void Evt_save_btn_Click(object sender, EventArgs e)
        {
            string evt_fname = Path.GetFileNameWithoutExtension(Evt_fname_txtBox.Text) + ".evt";
            Program.Controller.SetEvtFile(EvtPath_txtBx.Text, evt_fname);

        }

    }
}
