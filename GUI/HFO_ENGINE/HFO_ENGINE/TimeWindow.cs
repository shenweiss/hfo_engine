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
        public TimeWindow() //assumes Program.Trc_duration to be setted
        {
            InitializeComponent();
            seconds_to_timer(Program.Trc_duration, trc_duration_picker);
            seconds_to_timer(Program.StartTime, start_time_picker);
            seconds_to_timer(Program.StopTime, stop_time_picker);

            //int len_hours = Program.Trc_duration / 3600;
            //int len_minutes = (Program.Trc_duration - len_hours * 3600) / 60;
            //int len_snds = Program.Trc_duration - len_hours * 3600 - len_minutes * 60; ;
            //TRC_duration_visible_text.Text = len_hours.ToString() + ":" + len_minutes.ToString() + ":" + len_snds.ToString();

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
            if (Program.IsAnalizing) {
                Program.IsRunningMessage();
            }            
            else{
                int str_time = timer_to_seconds(start_time_picker);
                int stp_time = timer_to_seconds(stop_time_picker);

                if (str_time < 0)
                {
                    MessageBox.Show("Changes were NOT saved because start time must be greater or equal to 0.");
                    return;
                }
                if (stp_time > Program.Trc_duration){
                    MessageBox.Show("Changes were NOT saved because stop time is greater than TRC_duration.");
                    return;
                }
                if (str_time > stp_time)
                {
                    MessageBox.Show("Changes were NOT saved because start time is greater than stop time.");
                    return;
                }
                Program.StartTime = str_time;
                Program.StopTime = stp_time;
            }


        }

    }
}
