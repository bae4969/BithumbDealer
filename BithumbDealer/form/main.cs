using BithumbDealer.src;
using BithumbDealer.form;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Globalization;
using System.ComponentModel;

namespace BithumbDealer.form
{
    public partial class Main : Form
    {
        private string sAPI_Key;
        private string sAPI_Secret;

        private List<string> coinList = new List<string>();

        private bool isInit = false;
        private bool AllStop = false;

        private List<string> logList = new List<string>();
        private readonly object lock_logList = new object();

        private Thread thread_updater;
        private MainUpdater mainUpdater;
        private readonly object lock_mainUpdater = new object();
        private DataTable holdList;
        private Index btmiIndex;
        private Index btaiIndex;
        private DataTable showHoldList;

        private Thread thread_tradeHistory;
        public TradeHistory tradeHistory;
        public readonly object lock_tradeHistory = new object();

        private Thread thread_macro;
        public MacroSetting macro;
        public readonly object lock_macro = new object();


        public Main(string sAPI_Key, string sAPI_Secret)
        {
            InitializeComponent();
            this.sAPI_Key = sAPI_Key;
            this.sAPI_Secret = sAPI_Secret;
        }
        private void main_Load(object sender, EventArgs e)
        {
            {
                mainUpdater = new MainUpdater(sAPI_Key, sAPI_Secret);

                coinList = mainUpdater.getUpdateList();
                if (coinList.Count < 1)
                {
                    MessageBox.Show("Init error due to API error, try again.");
                    Close();
                    return;
                }
                mainUpdater.update();
                btmiIndex = mainUpdater.btmiIndex;
                btaiIndex = mainUpdater.btaiIndex;
                holdList = mainUpdater.holdList.Copy();

                for (int i = 0; i < coinList.Count; i++)
                    list_coinName.Items.Add(coinList[i]);

                showHoldList = new DataTable();
                showHoldList.Columns.Add("Name", typeof(string));
                showHoldList.Columns.Add("Units", typeof(float));
                showHoldList.Columns.Add("Value", typeof(float));
                showHoldList.Columns.Add("Total", typeof(float));

                DataRow dataRow = showHoldList.NewRow();
                dataRow["Name"] = holdList.Rows[0]["name"];
                dataRow["Units"] = (float)holdList.Rows[0]["total"];
                dataRow["Value"] = (float)holdList.Rows[0]["last"];
                dataRow["Total"] = (float)dataRow["Units"] * (float)dataRow["Value"];
                showHoldList.Rows.Add(dataRow);

                dataGridView_holdList.DataSource = showHoldList;
                dataGridView_holdList.Columns["Units"].DefaultCellStyle.Format = "#,0.####";
                dataGridView_holdList.Columns["Value"].DefaultCellStyle.Format = "#,0.##";
                dataGridView_holdList.Columns["Total"].DefaultCellStyle.Format = "#,0.##";
            }

            {
                tradeHistory = new TradeHistory(sAPI_Key, sAPI_Secret);
                if (tradeHistory.loadFile() < 0)
                {
                    Close();
                    return;
                }
            }

            {
                macro = new MacroSetting(sAPI_Key, sAPI_Secret, coinList);
                if (macro.loadFile() < 0)
                {
                    Close();
                    return;
                }
            }

            {
                thread_updater = new Thread(() => executeMainUpdater());
                thread_tradeHistory = new Thread(() => executeTradeHistoryUpdate());
                thread_macro = new Thread(() => executeMacro());

                thread_updater.Start();
                thread_tradeHistory.Start();
                thread_macro.Start();
            }

            isInit = true;
        }
        private void main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isInit)
                return;

            DialogResult dialogResult =
                MessageBox.Show(
                    "Really Exit?", "Exit",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
            if (dialogResult != DialogResult.Yes)
                e.Cancel = true;
        }
        private void main_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isInit)
            {
                MessageBox.Show("Init fail.");
                return;
            }

            savingMsg closing = new savingMsg();
            closing.Show(this);

            AllStop = true;
            thread_updater.Join();
            thread_tradeHistory.Join();
            thread_macro.Join();

            tradeHistory.saveFile();
            macro.saveFile();

            closing.Close();
        }
        private void main_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Visible = false;
                ShowIcon = false;
            }
        }
        private void text_focus_disable(object sender, EventArgs e)
        {
            group_account.Focus();
        }


        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                notifyIcon.ContextMenuStrip = contextMenuStrip;
                System.Reflection.MethodInfo methodInfo =
                       typeof(NotifyIcon).GetMethod("ShowContextMenu",
                        System.Reflection.BindingFlags.Instance |
                           System.Reflection.BindingFlags.NonPublic);
                methodInfo.Invoke(notifyIcon, null);
            }
        }
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Visible = true;
            ShowIcon = true;
            WindowState = FormWindowState.Normal;
        }
        private void toolStripTextBox_show_Click(object sender, EventArgs e)
        {
            Visible = true;
            ShowIcon = true;
            WindowState = FormWindowState.Normal;
        }
        private void toolStripTextBox_exit_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void executeMainUpdater()
        {
            while (!AllStop)
            {
                lock (lock_mainUpdater)
                {
                    if (mainUpdater.update() > -1)
                    {
                        btmiIndex = mainUpdater.btmiIndex;
                        btaiIndex = mainUpdater.btaiIndex;
                        holdList = mainUpdater.holdList.Copy();
                    }
                }
                for (int i = 0; i < 10; i++)
                {
                    if (AllStop) break;
                    Thread.Sleep(100);
                }
            }
        }
        private void executeTradeHistoryUpdate()
        {
            while (!AllStop)
            {
                for (int i = 0; i < tradeHistory.pendingData.Rows.Count; i++)
                {
                    lock (lock_tradeHistory)
                    {
                        bool needSave = false;
                        if (tradeHistory.updateSinglePendingData(i) > 0) needSave = true;

                        for (int j = 0; j < tradeHistory.executionStr.Count; j++)
                            logIn(tradeHistory.executionStr[j]);

                        tradeHistory.executionStr.Clear();
                        if (needSave) tradeHistory.saveFile();
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        if (AllStop) break;
                        Thread.Sleep(100);
                    }
                }
                Thread.Sleep(100);
            }
        }
        private void executeMacro()
        {
            logIn(new output(0, "Macro Exection", "Load candle data"));
            for (int i = 0; i < 70 && i < coinList.Count; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (AllStop) break;
                    Thread.Sleep(100);
                }
                if (AllStop) break;

                lock (lock_macro)
                {
                    if (macro.setCandleData(i) < 0) continue;
                    macro.addBABB(i);
                }
            }
            logIn(new output(0, "Macro Exection", "Finish to load, Start macro"));
            while (!AllStop)
            {
                macro.setBABB();
                for (int i = 0; i < 70 && i < coinList.Count; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        if (AllStop) break;
                        Thread.Sleep(100);
                    }
                    if (AllStop) break;

                    lock (lock_macro)
                    {
                        int ret = 0;
                        if ((ret = macro.setCandleData(i)) < 0)
                        {
                            logIn(new output(0, "Macro Exection", "Fail to update candle (" + ret + ")"));
                            continue;
                        }
                        if ((ret = macro.setLastKrw(i)) < 0)
                        {
                            logIn(new output(0, "Macro Exection", "Fail to update hold krw (" + ret + ")"));
                            continue;
                        }
                        macro.addBABB(i);

                        bool needSave = false;
                        if (macro.executeMacroBuy(i) > 0) needSave = true;
                        if (macro.executeMacroSell(i) > 0) needSave = true;

                        for (int j = 0; j < macro.order.Rows.Count; j++) 
                            if(macro.executeCheckResult(j) > 0)
                                needSave = true;
                        for (int j = 0; j < macro.executionStr.Count; j++) 
                            logIn(macro.executionStr[j]);

                        macro.executionStr.Clear();
                        if (needSave) macro.saveFile();
                    }
                }
                Thread.Sleep(100);
            }
        }


        private void timer_panel_Tick(object sender, EventArgs e)
        {
            text_curTime.Text = DateTime.Now.ToString("yyyy-MM-dd || HH:mm:ss");
            lock (lock_mainUpdater)
            {
                if (btmiIndex.width >= 0)
                {
                    text_btmiValue.ForeColor = Color.Red;
                    text_btmiValue.Text = btmiIndex.index.ToString() + "   " +
                        btmiIndex.width.ToString() + char.ConvertFromUtf32(0x2191) + "   +" +
                        btmiIndex.rate.ToString() + "%";
                }
                else
                {
                    text_btmiValue.ForeColor = Color.DodgerBlue;
                    text_btmiValue.Text = btmiIndex.index.ToString() + "   " +
                        btmiIndex.width.ToString() + char.ConvertFromUtf32(0x2193) + "   " +
                        btmiIndex.rate.ToString() + "%";
                }
                if (btaiIndex.width >= 0)
                {
                    text_btaiValue.ForeColor = Color.Red;
                    text_btaiValue.Text = btaiIndex.index.ToString() + "   " +
                        btaiIndex.width.ToString() + char.ConvertFromUtf32(0x2191) + "   +" +
                        btaiIndex.rate.ToString() + "%";
                }
                else
                {
                    text_btaiValue.ForeColor = Color.DodgerBlue;
                    text_btaiValue.Text = btaiIndex.index.ToString() + "   " +
                        btaiIndex.width.ToString() + char.ConvertFromUtf32(0x2193) + "   " +
                        btaiIndex.rate.ToString() + "%";
                }

                int index = dataGridView_holdList.FirstDisplayedCell.RowIndex;
                float totalKrw = 0;
                showHoldList.Clear();
                for (int i = 0; i < holdList.Rows.Count; i++)
                {
                    DataRow dataRow = showHoldList.NewRow();
                    dataRow["Name"] = holdList.Rows[i]["name"];
                    dataRow["Units"] = (float)holdList.Rows[i]["total"];
                    dataRow["Value"] = (float)holdList.Rows[i]["last"];
                    dataRow["Total"] = (float)dataRow["Units"] * (float)dataRow["Value"];
                    totalKrw += (float)dataRow["Total"];
                    showHoldList.Rows.Add(dataRow);
                }
                text_totalKrw.Text = "Total : " + Math.Round(totalKrw).ToString("0,0");
                dataGridView_holdList.FirstDisplayedScrollingRowIndex = index;

            }
        }
        private void timer_logOut_Tick(object sender, EventArgs e)
        {
            lock (lock_logList)
                while (logList.Count > 0)
                {
                    text_log.AppendText(logList[0] + Environment.NewLine);
                    logList.Remove(logList[0]);
                }
        }
        public void logIn(output log)
        {
            lock (lock_logList)
                logList.Add(DateTime.Now.ToString(" [yyyy-MM-dd_HH:mm:ss] ") + log.title + " : " + log.str);

            if (log.level == 1)
            {
                notifyIcon.BalloonTipTitle = log.title;
                notifyIcon.BalloonTipText = log.str;
                notifyIcon.ShowBalloonTip(1000);
            }
            else if (log.level == 2)
            {
                notifyIcon.BalloonTipTitle = log.title;
                notifyIcon.BalloonTipText = log.str;
                notifyIcon.ShowBalloonTip(10000);
            }
        }


        private void dataGridView_holdList_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                showHoldList.DefaultView.Sort = "";
        }


        private void list_coinName_MouseUp(object sender, MouseEventArgs e)
        {
            if (list_coinName.SelectedIndex < 0 || list_coinName.SelectedIndex > list_coinName.Items.Count - 1)
                list_coinName.SelectedIndex = -1;
        }
        private void btn_showChart_Click(object sender, EventArgs e)
        {
            if (list_coinName.SelectedIndex > -1 && list_coinName.SelectedIndex < list_coinName.Items.Count)
            {
                string name = list_coinName.SelectedItem.ToString();
                foreach (Form fm in Application.OpenForms)
                    if (fm.Name == name + " Chart")
                    {
                        MessageBox.Show("Fail to open '" + name + " Chart', already opened.");
                        return;
                    }

                graph chartForm = new graph(name, sAPI_Key, sAPI_Secret);
                chartForm.Show(this);
                return;
            }

            MessageBox.Show("Fail to open chart, choose one from the list.");
        }
        private void btn_trader_Click(object sender, EventArgs e)
        {
            foreach (Form fm in Application.OpenForms)
                if (fm.Name == "Trader")
                {
                    MessageBox.Show("Fail to open 'Trader', already opened.");
                    return;
                }

            Trader traderForm;
            lock (lock_mainUpdater)
                traderForm = new Trader(sAPI_Key, sAPI_Secret, coinList);
            traderForm.Show(this);
        }
        private void btn_history_Click(object sender, EventArgs e)
        {
            foreach (Form fm in Application.OpenForms)
                if (fm.Name == "History")
                {
                    MessageBox.Show("Fail to open 'History', already opened.");
                    return;
                }

            History historyForm;
            lock (lock_mainUpdater)
                historyForm = new History();
            historyForm.Show(this);
        }
        private void btn_macro_Click(object sender, EventArgs e)
        {
            foreach (Form fm in Application.OpenForms)
                if (fm.Name == "Macro")
                {
                    MessageBox.Show("Fail to show 'Macro' window, already opened.");
                    return;
                }

            Macro macroForm = new Macro();
            macroForm.Show(this);
        }
        private void btn_save_Click(object sender, EventArgs e)
        {
            timer_log.Stop();
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory =
                    Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
                saveFileDialog.Title = "Save BTC log";
                saveFileDialog.DefaultExt = "log";
                saveFileDialog.Filter = "Log files(*.log)|*.log";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName.ToString();
                    System.IO.File.WriteAllText(filePath, text_log.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            timer_log.Start();
        }
    }
}
