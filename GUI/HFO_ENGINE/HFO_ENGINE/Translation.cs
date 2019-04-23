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
        private MainWindow Parent_form;
        private Dictionary<string, string> Translations;
        private string EdfFname;

        public Translation(MainWindow parent)
        {
            InitializeComponent();
            Parent_form = parent;
            Translations = Program.ConversionParameters.ch_names_mapping;
            EdfFname = Program.ConversionParameters.edf_fname;

            int number = 0;
            foreach (var translation in Translations)
            {
                number = number + 1; 
                Panel translationPanel = new Panel();
                translationPanel.Name = "translationPanel_" + number.ToString();

                TextBox LongName = new TextBox();
                LongName.Name = "LongNameTextBox_" + number.ToString();
                LongName.Size = LongNameTextBox.Size;
                LongName.Location = new Point() { X = LongNameTextBox.Location.X, Y = LongNameTextBox.Location.Y };
                LongName.Text = translation.Key;
                LongName.Enabled = false;

                TextBox ShortName = new TextBox();
                ShortName.Name = "ShortNameTextBox_" + number.ToString();
                ShortName.Size = ShortNameTextBox.Size;
                ShortName.Location = new Point() { X = ShortNameTextBox.Location.X, Y = ShortNameTextBox.Location.Y };
                ShortName.Text = translation.Value;

                translationPanel.Controls.Add(LongName);
                translationPanel.Controls.Add(ShortName);
                PanelTranslations.Controls.Add(translationPanel);
            }
        }


        private void Confirm_translations_btn_Click(object sender, EventArgs e)
        {
            foreach(var translation in Translations)
            {
                Program.ConversionParameters.ch_names_mapping[translation.Key] = translation.Value; //VER Si esto actualiza
            }

            Program.GoToFinalConversion(Parent_form);
        }
    }
}
