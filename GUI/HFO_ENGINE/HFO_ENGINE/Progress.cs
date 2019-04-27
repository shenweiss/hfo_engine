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
    public partial class Progress : Form, INotifyPropertyChanged
    {
        //Constructor
        public Progress()
        {
            InitializeComponent();
            this.snds_count = 0;
            ProgressBar.DataBindings.Add("Value", this, "WorkerState");
           
        }

        //Colaborators
        public BackgroundWorker BgWorker = new BackgroundWorker();
        public event PropertyChangedEventHandler PropertyChanged;
        private int _workerState;
        public int WorkerState
        {
            get { return _workerState; }
            set
            {
                _workerState = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("WorkerState"));
            }
        }
        public void UpdateProgress(int progressState) { this.WorkerState = progressState; }
        private int snds_count { get; set; }

        //Methods
        public void StartTimer() {
            timer.Start();
        }
        public void SaveAndReset_timer(){
            this.timer.Stop();

            int hs = snds_count / 3600;
            int mins = (snds_count - hs * 3600) / 60;
            int snds = snds_count - hs * 3600 - mins * 60;

            previous_hs_txt.Text = hs.ToString();
            previous_min_txt.Text = mins.ToString();
            previous_snds_txt.Text = snds.ToString();

            this.snds_count = 0;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.snds_count++;
            int hs = snds_count / 3600;
            hours_label.Text = hs.ToString("D2");
            int min = (snds_count - hs * 3600) / 60;
            minutes_label.Text = min.ToString("D2");
            seconds_label.Text = (snds_count - hs * 3600 - min * 60).ToString("D2");

        }
    }
}
