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
    public partial class TimeWindow : Form
    {
        //REQUIRES: TRCs metadata setted.
        public TimeWindow() //
        {
            InitializeComponent();
            
        }
        public void SetTRCDuration(int duration_snds) {
            seconds_to_timer(duration_snds, trc_duration_picker);
        }

        private void seconds_to_timer(int seconds, DateTimePicker dt) {
            int hs = seconds / 3600;
            int mins = (seconds - hs * 3600) / 60;
            int snds = seconds - hs * 3600 - mins * 60;
            dt.Value = new DateTime(2020, 1, 1, hs, mins, snds);
        }

        private int timer_to_seconds(DateTimePicker dt){
            return ( dt.Value.Second + dt.Value.Minute * 60 + dt.Value.Hour * 3600);
        }

        private void TimeWindow_save_btn_Click(object sender, EventArgs e)
        {
            int start_time = timer_to_seconds(start_time_picker);
            int stop_time = timer_to_seconds(stop_time_picker);
            Program.Controller.SetTimeWindow(start_time, stop_time);
        }
    }
}
