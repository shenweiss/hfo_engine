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
            this.Snds_count = 0;
            ProgressBar.DataBindings.Add("Value", this, "WorkerState");

        }

        //Colaborators
        //public BackgroundWorker BgWorker = new BackgroundWorker();
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
        private int Snds_count { get; set; }

        private delegate void ProgressSafeCallDelegate(int progress);
        private delegate void ProgressDescSafeCallDelegate(string description);

        private delegate void StopTimer_SafeCallDelegate();

        //Methods
        public void StartTimer() {
            timer.Start();
        }

        public void SaveAndReset_timer_Safe()
        {
            if (previous_hs_txt.InvokeRequired ||  previous_min_txt.InvokeRequired || previous_snds_txt.InvokeRequired)
            {
                var d = new StopTimer_SafeCallDelegate(SaveAndReset_timer_Safe);
                Invoke(d, new object[] {});
            }
            else
            {
                this.timer.Stop();

                int hs = Snds_count / 3600;
                int mins = (Snds_count - hs * 3600) / 60;
                int snds = Snds_count - hs * 3600 - mins * 60;

                previous_hs_txt.Text = hs.ToString();
                previous_min_txt.Text = mins.ToString();
                previous_snds_txt.Text = snds.ToString();

                this.Snds_count = -1;
                this.Timer_Tick(null, null);

            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.Snds_count++;
            int hs = Snds_count / 3600;
            hours_label.Text = hs.ToString("D2");
            int min = (Snds_count - hs * 3600) / 60;
            minutes_label.Text = min.ToString("D2");
            seconds_label.Text = (Snds_count - hs * 3600 - min * 60).ToString("D2");

        }

        public void UpdateProgressSafe(int progress)
        {
            if (ProgressBar.InvokeRequired)
            {
                var d = new ProgressSafeCallDelegate(UpdateProgressSafe);
                Invoke(d, new object[] { progress });
            }
            else
            {
                this.WorkerState = progress;
            }
        }

        public void UpdateProgressDescSafe(string description)
        {
            if (Progress_label.InvokeRequired)
            {
                var d = new ProgressDescSafeCallDelegate(UpdateProgressDescSafe);
                Invoke(d, new object[] { description });
            }
            else
            {
                this.Progress_label.Text = description;
            }
        }


    }
}
