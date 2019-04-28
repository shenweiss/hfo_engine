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
    public partial class Translation : Form
    {
        private Dictionary<string, string> Translations { get; set; }

        public Translation(Dictionary<string, string> translations)
        {
            InitializeComponent();
            this.Translations = translations;
            int number = 0;
            
            foreach (var translation in Translations)
            {
                Console.WriteLine(number.ToString()); //DEBUG

                TextBox LongName = new TextBox();
                LongName.Name = "LongNameTextBox_" + number.ToString();
                LongName.Size = LongNameTextBox.Size;
                LongName.Text = translation.Key;
                LongName.Enabled = false;

                TextBox ShortName = new TextBox();
                ShortName.Name = "ShortNameTextBox_" + number.ToString();
                ShortName.Size = ShortNameTextBox.Size;
                ShortName.Text = translation.Value;

                Panel translationPanel = new Panel();

                translationPanel.Name = "translationPanel_" + number.ToString();
                translationPanel.Controls.Add(LongName);
                translationPanel.Controls[LongName.Name].Location = LongNameTextBox.Location;

                translationPanel.Controls.Add(ShortName);
                translationPanel.Controls[ShortName.Name].Location = ShortNameTextBox.Location;

                PanelTranslations.Controls.Add(translationPanel);

                number = number + 1;
            }
            

        }

        private void Confirm_translations_btn_Click(object sender, EventArgs e)
        {
            Program.Controller.ConfirmChMapping(Translations);
        }
    }
}
