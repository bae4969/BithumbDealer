using BithumbDealer.src;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BithumbDealer.form
{
    public partial class login : Form
    {
        string path = System.IO.Directory.GetCurrentDirectory() + "/login.dat";
        public bool isGood = false;
        public string sAPI_Key;
        public string sAPI_Secret;


        public login()
        {
            InitializeComponent();
        }
        private void login_Load(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    checkBox_remember.Checked = true;
                    string[] keyValue = System.IO.File.ReadAllLines(path);
                    if (keyValue.Length > 1)
                    {
                        text_sAPI_Key.Text = keyValue[0];
                        text_sAPI_Secret.Text = keyValue[1];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void text_focus_disable(object sender, EventArgs e)
        {
            text_sAPI_Key.Focus();
        }

        private void but_login_Click(object sender, EventArgs e)
        {
            if (!checkBox_remember.Checked)
            {
                try
                {
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            sAPI_Key = text_sAPI_Key.Text;
            sAPI_Secret = text_sAPI_Secret.Text;
            ApiData apiData = new ApiData(sAPI_Key, sAPI_Secret);
            Dictionary<string, string> par = new Dictionary<string, string>();
            par.Add("order_currency", "BTC");
            par.Add("payment_currency", "KRW");
            JObject ret = apiData.privateInfo(c.vACCOUNT, par);
            if (ret == null)
            {
                MessageBox.Show("Invalid API key, check keys");
                return;
            }
            if (ret["status"].ToString() != "0000")
            {
                MessageBox.Show("Invalid API key, check keys");
                return;
            }

            if (checkBox_remember.Checked)
            {
                try
                {
                    System.IO.File.WriteAllText(path, "");
                    System.IO.File.AppendAllText(path, text_sAPI_Key.Text + Environment.NewLine);
                    System.IO.File.AppendAllText(path, text_sAPI_Secret.Text + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            isGood = true;
            Close();
        }
    }
}
