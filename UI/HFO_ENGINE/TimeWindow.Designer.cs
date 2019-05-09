namespace HFO_ENGINE
{
    partial class TimeWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimeWindow));
            this.Times_title = new System.Windows.Forms.Label();
            this.StartTimeLabel = new System.Windows.Forms.Label();
            this.StopTimeLabel = new System.Windows.Forms.Label();
            this.TimeWindow_save_btn = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.trc_duration_picker = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.stop_time_picker = new System.Windows.Forms.DateTimePicker();
            this.start_time_picker = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.TRC_duration_visible_text = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Times_title
            // 
            this.Times_title.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Times_title.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.Times_title.Location = new System.Drawing.Point(-5, 0);
            this.Times_title.Name = "Times_title";
            this.Times_title.Size = new System.Drawing.Size(189, 40);
            this.Times_title.TabIndex = 1;
            this.Times_title.Text = "Time window";
            // 
            // StartTimeLabel
            // 
            this.StartTimeLabel.BackColor = System.Drawing.Color.Transparent;
            this.StartTimeLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StartTimeLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.StartTimeLabel.Location = new System.Drawing.Point(63, 152);
            this.StartTimeLabel.Name = "StartTimeLabel";
            this.StartTimeLabel.Size = new System.Drawing.Size(92, 26);
            this.StartTimeLabel.TabIndex = 3;
            this.StartTimeLabel.Text = "Start time";
            // 
            // StopTimeLabel
            // 
            this.StopTimeLabel.BackColor = System.Drawing.Color.Transparent;
            this.StopTimeLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.StopTimeLabel.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.StopTimeLabel.Location = new System.Drawing.Point(63, 201);
            this.StopTimeLabel.Name = "StopTimeLabel";
            this.StopTimeLabel.Size = new System.Drawing.Size(92, 26);
            this.StopTimeLabel.TabIndex = 4;
            this.StopTimeLabel.Text = "Stop time";
            // 
            // TimeWindow_save_btn
            // 
            this.TimeWindow_save_btn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TimeWindow_save_btn.FlatAppearance.BorderSize = 2;
            this.TimeWindow_save_btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TimeWindow_save_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.TimeWindow_save_btn.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.TimeWindow_save_btn.Location = new System.Drawing.Point(306, 286);
            this.TimeWindow_save_btn.Name = "TimeWindow_save_btn";
            this.TimeWindow_save_btn.Size = new System.Drawing.Size(82, 34);
            this.TimeWindow_save_btn.TabIndex = 8;
            this.TimeWindow_save_btn.Text = "Save";
            this.TimeWindow_save_btn.UseVisualStyleBackColor = true;
            this.TimeWindow_save_btn.Click += new System.EventHandler(this.TimeWindow_save_btn_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.trc_duration_picker);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.stop_time_picker);
            this.panel2.Controls.Add(this.start_time_picker);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.TimeWindow_save_btn);
            this.panel2.Controls.Add(this.StartTimeLabel);
            this.panel2.Controls.Add(this.StopTimeLabel);
            this.panel2.Location = new System.Drawing.Point(32, 167);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(388, 320);
            this.panel2.TabIndex = 11;
            // 
            // trc_duration_picker
            // 
            this.trc_duration_picker.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.trc_duration_picker.CustomFormat = "HH:mm:ss";
            this.trc_duration_picker.Enabled = false;
            this.trc_duration_picker.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.trc_duration_picker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.trc_duration_picker.Location = new System.Drawing.Point(287, 3);
            this.trc_duration_picker.Name = "trc_duration_picker";
            this.trc_duration_picker.ShowUpDown = true;
            this.trc_duration_picker.Size = new System.Drawing.Size(98, 29);
            this.trc_duration_picker.TabIndex = 74;
            this.trc_duration_picker.Value = new System.DateTime(2019, 3, 21, 12, 1, 0, 0);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label3.Location = new System.Drawing.Point(3, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(336, 26);
            this.label3.TabIndex = 72;
            this.label3.Text = "The uploaded TRC has a duration of:";
            // 
            // stop_time_picker
            // 
            this.stop_time_picker.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.stop_time_picker.CustomFormat = "HH:mm:ss";
            this.stop_time_picker.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.stop_time_picker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.stop_time_picker.Location = new System.Drawing.Point(161, 194);
            this.stop_time_picker.Name = "stop_time_picker";
            this.stop_time_picker.ShowUpDown = true;
            this.stop_time_picker.Size = new System.Drawing.Size(98, 29);
            this.stop_time_picker.TabIndex = 71;
            this.stop_time_picker.Value = new System.DateTime(2019, 3, 21, 12, 0, 0, 0);
            // 
            // start_time_picker
            // 
            this.start_time_picker.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.start_time_picker.CustomFormat = "HH:mm:ss";
            this.start_time_picker.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.start_time_picker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.start_time_picker.Location = new System.Drawing.Point(161, 145);
            this.start_time_picker.Name = "start_time_picker";
            this.start_time_picker.ShowUpDown = true;
            this.start_time_picker.Size = new System.Drawing.Size(98, 29);
            this.start_time_picker.TabIndex = 67;
            this.start_time_picker.Value = new System.DateTime(2019, 3, 21, 12, 1, 0, 0);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.label1.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label1.Location = new System.Drawing.Point(4, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(289, 26);
            this.label1.TabIndex = 9;
            this.label1.Text = "Relative to that range, set the time-window";
            // 
            // TRC_duration_visible_text
            // 
            this.TRC_duration_visible_text.BackColor = System.Drawing.Color.Transparent;
            this.TRC_duration_visible_text.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TRC_duration_visible_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.TRC_duration_visible_text.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.TRC_duration_visible_text.Location = new System.Drawing.Point(-3, 9);
            this.TRC_duration_visible_text.Name = "TRC_duration_visible_text";
            this.TRC_duration_visible_text.Size = new System.Drawing.Size(114, 26);
            this.TRC_duration_visible_text.TabIndex = 73;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.Times_title);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Location = new System.Drawing.Point(30, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(390, 95);
            this.panel1.TabIndex = 13;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(295, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(95, 95);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 9;
            this.pictureBox1.TabStop = false;
            // 
            // TimeWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(66)))), ((int)(((byte)(82)))));
            this.ClientSize = new System.Drawing.Size(480, 512);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.TRC_duration_visible_text);
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "TimeWindow";
            this.Text = "TimeWindow";
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label Times_title;
        private System.Windows.Forms.Label StartTimeLabel;
        private System.Windows.Forms.Label StopTimeLabel;
        private System.Windows.Forms.Button TimeWindow_save_btn;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.DateTimePicker start_time_picker;
        private System.Windows.Forms.DateTimePicker stop_time_picker;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label TRC_duration_visible_text;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker trc_duration_picker;
    }
}