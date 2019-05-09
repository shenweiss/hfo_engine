namespace HFO_ENGINE
{
    partial class ConversorStep3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConversorStep3));
            this.panel4 = new System.Windows.Forms.Panel();
            this.Translation_label = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.browse_trc_out_dir = new System.Windows.Forms.PictureBox();
            this.SavingConvTrcDir = new System.Windows.Forms.Label();
            this.ConvertButton = new System.Windows.Forms.Button();
            this.ConvProgressBar = new System.Windows.Forms.ProgressBar();
            this.Trc_out_conv_dir_txt = new System.Windows.Forms.TextBox();
            this.line = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Progress_label = new System.Windows.Forms.Label();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.browse_trc_out_dir)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.Translation_label);
            this.panel4.Controls.Add(this.pictureBox1);
            this.panel4.Location = new System.Drawing.Point(30, 50);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(390, 67);
            this.panel4.TabIndex = 37;
            // 
            // Translation_label
            // 
            this.Translation_label.Font = new System.Drawing.Font("Arial", 22F);
            this.Translation_label.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Translation_label.Location = new System.Drawing.Point(3, 10);
            this.Translation_label.Name = "Translation_label";
            this.Translation_label.Size = new System.Drawing.Size(269, 40);
            this.Translation_label.TabIndex = 29;
            this.Translation_label.Text = "Download directory";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(292, 10);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(70, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // browse_trc_out_dir
            // 
            this.browse_trc_out_dir.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.browse_trc_out_dir.Image = ((System.Drawing.Image)(resources.GetObject("browse_trc_out_dir.Image")));
            this.browse_trc_out_dir.InitialImage = ((System.Drawing.Image)(resources.GetObject("browse_trc_out_dir.InitialImage")));
            this.browse_trc_out_dir.Location = new System.Drawing.Point(273, 49);
            this.browse_trc_out_dir.Name = "browse_trc_out_dir";
            this.browse_trc_out_dir.Size = new System.Drawing.Size(50, 50);
            this.browse_trc_out_dir.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.browse_trc_out_dir.TabIndex = 41;
            this.browse_trc_out_dir.TabStop = false;
            this.browse_trc_out_dir.Click += new System.EventHandler(this.browse_trc_out_dir_Click_1);
            // 
            // SavingConvTrcDir
            // 
            this.SavingConvTrcDir.BackColor = System.Drawing.Color.Transparent;
            this.SavingConvTrcDir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.SavingConvTrcDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SavingConvTrcDir.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.SavingConvTrcDir.Location = new System.Drawing.Point(-3, 11);
            this.SavingConvTrcDir.Name = "SavingConvTrcDir";
            this.SavingConvTrcDir.Size = new System.Drawing.Size(266, 26);
            this.SavingConvTrcDir.TabIndex = 38;
            this.SavingConvTrcDir.Text = "Pick the output TRC saving directory";
            // 
            // ConvertButton
            // 
            this.ConvertButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ConvertButton.FlatAppearance.BorderSize = 2;
            this.ConvertButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ConvertButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold);
            this.ConvertButton.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.ConvertButton.Location = new System.Drawing.Point(121, 199);
            this.ConvertButton.Name = "ConvertButton";
            this.ConvertButton.Size = new System.Drawing.Size(121, 35);
            this.ConvertButton.TabIndex = 42;
            this.ConvertButton.Text = "Convert";
            this.ConvertButton.UseVisualStyleBackColor = true;
            this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
            // 
            // ConvProgressBar
            // 
            this.ConvProgressBar.Location = new System.Drawing.Point(3, 289);
            this.ConvProgressBar.Name = "ConvProgressBar";
            this.ConvProgressBar.Size = new System.Drawing.Size(357, 10);
            this.ConvProgressBar.TabIndex = 43;
            // 
            // Trc_out_conv_dir_txt
            // 
            this.Trc_out_conv_dir_txt.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Trc_out_conv_dir_txt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.Trc_out_conv_dir_txt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Trc_out_conv_dir_txt.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.Trc_out_conv_dir_txt.Enabled = false;
            this.Trc_out_conv_dir_txt.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.Trc_out_conv_dir_txt.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Trc_out_conv_dir_txt.HideSelection = false;
            this.Trc_out_conv_dir_txt.Location = new System.Drawing.Point(3, 123);
            this.Trc_out_conv_dir_txt.Name = "Trc_out_conv_dir_txt";
            this.Trc_out_conv_dir_txt.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            this.Trc_out_conv_dir_txt.Size = new System.Drawing.Size(350, 17);
            this.Trc_out_conv_dir_txt.TabIndex = 45;
            // 
            // line
            // 
            this.line.BackColor = System.Drawing.Color.White;
            this.line.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.line.Location = new System.Drawing.Point(0, 143);
            this.line.Name = "line";
            this.line.Size = new System.Drawing.Size(350, 2);
            this.line.TabIndex = 44;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Progress_label);
            this.panel1.Controls.Add(this.ConvertButton);
            this.panel1.Controls.Add(this.ConvProgressBar);
            this.panel1.Controls.Add(this.line);
            this.panel1.Controls.Add(this.Trc_out_conv_dir_txt);
            this.panel1.Controls.Add(this.SavingConvTrcDir);
            this.panel1.Controls.Add(this.browse_trc_out_dir);
            this.panel1.Location = new System.Drawing.Point(60, 157);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(360, 320);
            this.panel1.TabIndex = 46;
            // 
            // Progress_label
            // 
            this.Progress_label.BackColor = System.Drawing.Color.Transparent;
            this.Progress_label.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Progress_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Progress_label.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.Progress_label.Location = new System.Drawing.Point(3, 260);
            this.Progress_label.Name = "Progress_label";
            this.Progress_label.Size = new System.Drawing.Size(347, 26);
            this.Progress_label.TabIndex = 47;
            // 
            // ConversorStep3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.ClientSize = new System.Drawing.Size(480, 512);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ConversorStep3";
            this.Text = "FinalConvertion";
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.browse_trc_out_dir)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label Translation_label;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox browse_trc_out_dir;
        private System.Windows.Forms.Label SavingConvTrcDir;
        private System.Windows.Forms.Button ConvertButton;
        private System.Windows.Forms.ProgressBar ConvProgressBar;
        private System.Windows.Forms.TextBox Trc_out_conv_dir_txt;
        private System.Windows.Forms.Label line;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label Progress_label;
    }
}