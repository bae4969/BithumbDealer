using BithumbDealer.src;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace BithumbDealer.form
{
    public partial class Macro : Form
    {

        public Macro()
        {
            InitializeComponent();
        }
        private void Macro_Load(object sender, EventArgs e)
        {
            setDefaultSetting();
        }
        private void text_focus_disable(object sender, EventArgs e)
        {
            groupBox1.Focus();
        }


        private void setDefaultSetting()
        {
            MacroSettingData setting;
            List<BollingerAverage> ba0;
            List<BollingerAverage> ba1;
            List<BollingerAverage> ba2;
            List<BollingerAverage> bb0;
            List<BollingerAverage> bb1;
            List<BollingerAverage> bb2;

            lock (((Main)Owner).lock_macro)
            {
                setting = ((Main)Owner).macro.setting;
                ba0 = ((Main)Owner).macro.ba0;
                ba1 = ((Main)Owner).macro.ba1;
                ba2 = ((Main)Owner).macro.ba2;
                bb0 = ((Main)Owner).macro.bb0;
                bb1 = ((Main)Owner).macro.bb1;
                bb2 = ((Main)Owner).macro.bb2;
            }

            text_ba_min30_0.Text = ba0[0].avg.ToString("0.##");
            text_ba_min30_1.Text = ba1[0].avg.ToString("0.##");
            text_ba_min30_2.Text = ba2[0].avg.ToString("0.##");
            text_ba_hour1_0.Text = ba0[1].avg.ToString("0.##");
            text_ba_hour1_1.Text = ba1[1].avg.ToString("0.##");
            text_ba_hour1_2.Text = ba2[1].avg.ToString("0.##");
            text_ba_hour6_0.Text = ba0[2].avg.ToString("0.##");
            text_ba_hour6_1.Text = ba1[2].avg.ToString("0.##");
            text_ba_hour6_2.Text = ba2[2].avg.ToString("0.##");
            text_ba_hour12_0.Text = ba0[3].avg.ToString("0.##");
            text_ba_hour12_1.Text = ba1[3].avg.ToString("0.##");
            text_ba_hour12_2.Text = ba2[3].avg.ToString("0.##");
            text_ba_hour24_0.Text = ba0[4].avg.ToString("0.##");
            text_ba_hour24_1.Text = ba1[4].avg.ToString("0.##");
            text_ba_hour24_2.Text = ba2[4].avg.ToString("0.##");

            text_bb_min30_0.Text = bb0[0].avg.ToString("0.##");
            text_bb_min30_1.Text = bb1[0].avg.ToString("0.##");
            text_bb_min30_2.Text = bb2[0].avg.ToString("0.##");
            text_bb_hour1_0.Text = bb0[1].avg.ToString("0.##");
            text_bb_hour1_1.Text = bb1[1].avg.ToString("0.##");
            text_bb_hour1_2.Text = bb2[1].avg.ToString("0.##");
            text_bb_hour6_0.Text = bb0[2].avg.ToString("0.##");
            text_bb_hour6_1.Text = bb1[2].avg.ToString("0.##");
            text_bb_hour6_2.Text = bb2[2].avg.ToString("0.##");
            text_bb_hour12_0.Text = bb0[3].avg.ToString("0.##");
            text_bb_hour12_1.Text = bb1[3].avg.ToString("0.##");
            text_bb_hour12_2.Text = bb2[3].avg.ToString("0.##");
            text_bb_hour24_0.Text = bb0[4].avg.ToString("0.##");
            text_bb_hour24_1.Text = bb1[4].avg.ToString("0.##");
            text_bb_hour24_2.Text = bb2[4].avg.ToString("0.##");

            text_yield.Text = setting.yield.ToString();
            text_krw.Text = setting.krw.ToString();
            text_time.Text = setting.time.ToString();
            if (setting.hour24_from > -90000d) text_hour24_from.Text = setting.hour24_from.ToString();
            if (setting.hour24_to > -90000d) text_hour24_to.Text = setting.hour24_to.ToString();
            if (setting.hour12_from > -90000d) text_hour12_from.Text = setting.hour12_from.ToString();
            if (setting.hour12_to > -90000d) text_hour12_to.Text = setting.hour12_to.ToString();
            if (setting.hour6_from > -90000d) text_hour6_from.Text = setting.hour6_from.ToString();
            if (setting.hour6_to > -90000d) text_hour6_to.Text = setting.hour6_to.ToString();
            if (setting.hour1_from > -90000d) text_hour1_from.Text = setting.hour1_from.ToString();
            if (setting.hour1_to > -90000d) text_hour1_to.Text = setting.hour1_to.ToString();
            if (setting.min30_from > -90000d) text_min30_from.Text = setting.min30_from.ToString();
            if (setting.min30_to > -90000d) text_min30_to.Text = setting.min30_to.ToString();
        }


        private void btn_save_Click(object sender, EventArgs e)
        {
            if (text_yield.Text != "" && text_krw.Text != "" && text_time.Text != "")
            {
                MacroSettingData setting = new MacroSettingData();

                if (!float.TryParse(text_yield.Text, out setting.yield)
                    || !float.TryParse(text_krw.Text, out setting.krw)
                    || !float.TryParse(text_time.Text, out setting.time))
                {
                    MessageBox.Show("One of yield, krw and time value is not number.");
                    return;
                }

                if (text_hour24_from.Text == "" && text_hour12_from.Text == "" && text_hour6_from.Text == ""
                    && text_hour1_from.Text == "" && text_min30_from.Text == "")
                {
                    MessageBox.Show("At least one of rate parameter need.");
                    return;
                }

                if (text_hour24_from.Text != "")
                {
                    if (!float.TryParse(text_hour24_from.Text, out setting.hour24_from))
                    {
                        MessageBox.Show("24 hour rate is not number.");
                        return;
                    }
                    if (setting.hour24_from < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour24_from = -100000;
                if (text_hour24_to.Text != "")
                {
                    if (!float.TryParse(text_hour24_to.Text, out setting.hour24_to))
                    {
                        MessageBox.Show("24 hour rate is not number.");
                        return;
                    }
                    if(setting.hour24_to < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour24_to = -100000;

                if (text_hour12_from.Text != "")
                {
                    if (!float.TryParse(text_hour12_from.Text, out setting.hour12_from))
                    {
                        MessageBox.Show("12 hour rate is not number.");
                        return;
                    }
                    if (setting.hour12_from < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour12_from = -100000;
                if (text_hour12_to.Text != "")
                {
                    if (!float.TryParse(text_hour12_to.Text, out setting.hour12_to))
                    {
                        MessageBox.Show("12 hour rate is not number.");
                        return;
                    }
                    if (setting.hour12_to < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour12_to = -100000;

                if (text_hour6_from.Text != "")
                {
                    if (!float.TryParse(text_hour6_from.Text, out setting.hour6_from))
                    {
                        MessageBox.Show("6 hour rate is not number.");
                        return;
                    }
                    if (setting.hour6_from < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour6_from = -100000;
                if (text_hour6_to.Text != "")
                {
                    if (!float.TryParse(text_hour6_to.Text, out setting.hour6_to))
                    {
                        MessageBox.Show("6 hour rate is not number.");
                        return;
                    }
                    if (setting.hour6_to < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour6_to = -100000;

                if (text_hour1_from.Text != "")
                {
                    if (!float.TryParse(text_hour1_from.Text, out setting.hour1_from))
                    {
                        MessageBox.Show("1 hour rate is not number.");
                        return;
                    }
                    if (setting.hour1_from < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour1_from = -100000;
                if (text_hour1_to.Text != "")
                {
                    if (!float.TryParse(text_hour1_to.Text, out setting.hour1_to))
                    {
                        MessageBox.Show("1 hour rate is not number.");
                        return;
                    }
                    if (setting.hour1_to < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.hour1_to = -100000;

                if (text_min30_from.Text != "")
                {
                    if (!float.TryParse(text_min30_from.Text, out setting.min30_from))
                    {
                        MessageBox.Show("30 minute rate is not number.");
                        return;
                    }
                    if (setting.min30_from < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.min30_from = -100000;
                if (text_min30_to.Text != "")
                {
                    if (!float.TryParse(text_min30_to.Text, out setting.min30_to))
                    {
                        MessageBox.Show("30 minute rate is not number.");
                        return;
                    }
                    if (setting.min30_to < -10000)
                    {
                        MessageBox.Show("Rate value must be at least -10000.");
                        return;
                    }
                }
                else setting.min30_to = -100000;

                setting.hour24_bias = check_hour24.Checked;
                setting.hour12_bias = check_hour12.Checked;
                setting.hour6_bias = check_hour6.Checked;
                setting.hour1_bias = check_hour1.Checked;
                setting.min30_bias = check_min30.Checked;

                lock (((Main)Owner).lock_macro)
                    ((Main)Owner).macro.saveMacroSetting(setting);

                setDefaultSetting();
                MessageBox.Show("Save success.");
            }
            else
                MessageBox.Show("Check Parameters.");
        }
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            setDefaultSetting();
        }
    }
}
