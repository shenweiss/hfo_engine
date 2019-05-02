using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFO_ENGINE
{
    public partial class ConversorStep2 : Form
    {
        private int count { get; set; } = 0;

        public ConversorStep2( Dictionary<string, string> translations)
        {
            InitializeComponent();
            this.count = 0;
            
            foreach (var translation in translations)
            {
                Console.WriteLine(count.ToString()); //DEBUG

                TextBox LongName = new TextBox();
                LongName.Name = "LongNameTextBox_" + count.ToString();
                LongName.Size = LongNameTextBox.Size;
                LongName.Text = translation.Key;
                LongName.Enabled = false;

                TextBox ShortName = new TextBox();
                ShortName.Name = "ShortNameTextBox_" + count.ToString();
                ShortName.Size = ShortNameTextBox.Size;
                ShortName.Text = translation.Value;

                Panel translationPanel = new Panel();

                translationPanel.Name = "translationPanel_" + count.ToString();
                translationPanel.Controls.Add(LongName);
                translationPanel.Controls[LongName.Name].Location = LongNameTextBox.Location;

                translationPanel.Controls.Add(ShortName);
                translationPanel.Controls[ShortName.Name].Location = ShortNameTextBox.Location;

                PanelTranslations.Controls.Add(translationPanel);

                count = count + 1;
            }
            

        }

        private void Confirm_translations_btn_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> translations = new Dictionary<string, string>();

            foreach (Panel line in PanelTranslations.Controls)
            {
                int i = 0;
                string long_name= string.Empty;
                string short_name = string.Empty;

                foreach (TextBox ch_name_box in line.Controls)
                {
                    if (i == 0)
                    {
                        long_name = ch_name_box.Text;
                        i = 1;
                    }
                    else {
                        short_name = ch_name_box.Text;
                        i = 0;
                    }
                }
                translations.Add(long_name, short_name);
            }

            Program.Controller.ConfirmChMapping(translations);
        }
    }
}
