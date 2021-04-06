using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace BithumbDealer.form
{
    partial class Main
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.text_log = new System.Windows.Forms.TextBox();
            this.btn_save = new System.Windows.Forms.Button();
            this.timer_log = new System.Windows.Forms.Timer(this.components);
            this.list_coinName = new System.Windows.Forms.ListBox();
            this.btn_showChart = new System.Windows.Forms.Button();
            this.btn_macro = new System.Windows.Forms.Button();
            this.text_curTime = new System.Windows.Forms.TextBox();
            this.timer_panel = new System.Windows.Forms.Timer(this.components);
            this.text_BTMI = new System.Windows.Forms.TextBox();
            this.text_btmiValue = new System.Windows.Forms.TextBox();
            this.text_btaiValue = new System.Windows.Forms.TextBox();
            this.text_BTAI = new System.Windows.Forms.TextBox();
            this.group_account = new System.Windows.Forms.GroupBox();
            this.dataGridView_holdList = new System.Windows.Forms.DataGridView();
            this.text_totalKrw = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btn_trader = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.btn_history = new System.Windows.Forms.Button();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripTextBox_show = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripTextBox_exit = new System.Windows.Forms.ToolStripTextBox();
            this.group_account.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_holdList)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // text_log
            // 
            this.text_log.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.text_log.BackColor = System.Drawing.SystemColors.WindowText;
            this.text_log.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_log.ForeColor = System.Drawing.SystemColors.Window;
            this.text_log.Location = new System.Drawing.Point(6, 20);
            this.text_log.Multiline = true;
            this.text_log.Name = "text_log";
            this.text_log.ReadOnly = true;
            this.text_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.text_log.Size = new System.Drawing.Size(272, 471);
            this.text_log.TabIndex = 2;
            // 
            // btn_save
            // 
            this.btn_save.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btn_save.BackColor = System.Drawing.Color.DarkGray;
            this.btn_save.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btn_save.Location = new System.Drawing.Point(638, 520);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(284, 30);
            this.btn_save.TabIndex = 5;
            this.btn_save.Text = "Save Log";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // timer_log
            // 
            this.timer_log.Enabled = true;
            this.timer_log.Interval = 200;
            this.timer_log.Tick += new System.EventHandler(this.timer_logOut_Tick);
            // 
            // list_coinName
            // 
            this.list_coinName.BackColor = System.Drawing.SystemColors.WindowText;
            this.list_coinName.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.list_coinName.ForeColor = System.Drawing.SystemColors.Window;
            this.list_coinName.FormattingEnabled = true;
            this.list_coinName.ItemHeight = 19;
            this.list_coinName.Location = new System.Drawing.Point(6, 20);
            this.list_coinName.Name = "list_coinName";
            this.list_coinName.ScrollAlwaysVisible = true;
            this.list_coinName.Size = new System.Drawing.Size(98, 460);
            this.list_coinName.Sorted = true;
            this.list_coinName.TabIndex = 17;
            this.list_coinName.MouseUp += new System.Windows.Forms.MouseEventHandler(this.list_coinName_MouseUp);
            // 
            // btn_showChart
            // 
            this.btn_showChart.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_showChart.BackColor = System.Drawing.Color.DarkGray;
            this.btn_showChart.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold);
            this.btn_showChart.Location = new System.Drawing.Point(12, 519);
            this.btn_showChart.Name = "btn_showChart";
            this.btn_showChart.Size = new System.Drawing.Size(110, 30);
            this.btn_showChart.TabIndex = 18;
            this.btn_showChart.Text = "Chart";
            this.btn_showChart.UseVisualStyleBackColor = false;
            this.btn_showChart.Click += new System.EventHandler(this.btn_showChart_Click);
            // 
            // btn_macro
            // 
            this.btn_macro.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_macro.BackColor = System.Drawing.Color.DarkGray;
            this.btn_macro.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold);
            this.btn_macro.Location = new System.Drawing.Point(468, 519);
            this.btn_macro.Name = "btn_macro";
            this.btn_macro.Size = new System.Drawing.Size(164, 30);
            this.btn_macro.TabIndex = 13;
            this.btn_macro.Text = "Macro";
            this.btn_macro.UseVisualStyleBackColor = false;
            this.btn_macro.Click += new System.EventHandler(this.btn_macro_Click);
            // 
            // text_curTime
            // 
            this.text_curTime.BackColor = System.Drawing.Color.Black;
            this.text_curTime.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.text_curTime.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.text_curTime.Font = new System.Drawing.Font("Arial Black", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_curTime.ForeColor = System.Drawing.Color.White;
            this.text_curTime.Location = new System.Drawing.Point(200, 12);
            this.text_curTime.Name = "text_curTime";
            this.text_curTime.ReadOnly = true;
            this.text_curTime.Size = new System.Drawing.Size(360, 39);
            this.text_curTime.TabIndex = 19;
            this.text_curTime.Text = "2020-01-01 / 00:00:00";
            this.text_curTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.text_curTime.Enter += new System.EventHandler(this.text_focus_disable);
            // 
            // timer_panel
            // 
            this.timer_panel.Enabled = true;
            this.timer_panel.Interval = 500;
            this.timer_panel.Tick += new System.EventHandler(this.timer_panel_Tick);
            // 
            // text_BTMI
            // 
            this.text_BTMI.BackColor = System.Drawing.Color.Black;
            this.text_BTMI.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.text_BTMI.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_BTMI.ForeColor = System.Drawing.Color.White;
            this.text_BTMI.Location = new System.Drawing.Point(223, 62);
            this.text_BTMI.Name = "text_BTMI";
            this.text_BTMI.ReadOnly = true;
            this.text_BTMI.Size = new System.Drawing.Size(47, 19);
            this.text_BTMI.TabIndex = 20;
            this.text_BTMI.Text = "BTMI";
            this.text_BTMI.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // text_btmiValue
            // 
            this.text_btmiValue.BackColor = System.Drawing.Color.Black;
            this.text_btmiValue.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.text_btmiValue.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_btmiValue.Location = new System.Drawing.Point(276, 59);
            this.text_btmiValue.Name = "text_btmiValue";
            this.text_btmiValue.ReadOnly = true;
            this.text_btmiValue.Size = new System.Drawing.Size(256, 26);
            this.text_btmiValue.TabIndex = 24;
            this.text_btmiValue.Text = "0";
            this.text_btmiValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.text_btmiValue.Enter += new System.EventHandler(this.text_focus_disable);
            // 
            // text_btaiValue
            // 
            this.text_btaiValue.BackColor = System.Drawing.Color.Black;
            this.text_btaiValue.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.text_btaiValue.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_btaiValue.Location = new System.Drawing.Point(276, 91);
            this.text_btaiValue.Name = "text_btaiValue";
            this.text_btaiValue.ReadOnly = true;
            this.text_btaiValue.Size = new System.Drawing.Size(256, 26);
            this.text_btaiValue.TabIndex = 27;
            this.text_btaiValue.Text = "0";
            this.text_btaiValue.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.text_btaiValue.Enter += new System.EventHandler(this.text_focus_disable);
            // 
            // text_BTAI
            // 
            this.text_BTAI.BackColor = System.Drawing.Color.Black;
            this.text_BTAI.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.text_BTAI.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_BTAI.ForeColor = System.Drawing.Color.White;
            this.text_BTAI.Location = new System.Drawing.Point(223, 94);
            this.text_BTAI.Name = "text_BTAI";
            this.text_BTAI.ReadOnly = true;
            this.text_BTAI.Size = new System.Drawing.Size(47, 19);
            this.text_BTAI.TabIndex = 30;
            this.text_BTAI.Text = "BTAI";
            this.text_BTAI.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // group_account
            // 
            this.group_account.Controls.Add(this.dataGridView_holdList);
            this.group_account.Controls.Add(this.text_totalKrw);
            this.group_account.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.group_account.ForeColor = System.Drawing.Color.White;
            this.group_account.Location = new System.Drawing.Point(128, 123);
            this.group_account.Name = "group_account";
            this.group_account.Size = new System.Drawing.Size(504, 390);
            this.group_account.TabIndex = 31;
            this.group_account.TabStop = false;
            this.group_account.Text = "Account Info";
            // 
            // dataGridView_holdList
            // 
            this.dataGridView_holdList.AllowUserToAddRows = false;
            this.dataGridView_holdList.AllowUserToDeleteRows = false;
            this.dataGridView_holdList.AllowUserToOrderColumns = true;
            this.dataGridView_holdList.AllowUserToResizeColumns = false;
            this.dataGridView_holdList.AllowUserToResizeRows = false;
            this.dataGridView_holdList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView_holdList.BackgroundColor = System.Drawing.Color.Black;
            this.dataGridView_holdList.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.SingleVertical;
            this.dataGridView_holdList.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridView_holdList.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_holdList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_holdList.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_holdList.EnableHeadersVisualStyles = false;
            this.dataGridView_holdList.GridColor = System.Drawing.Color.White;
            this.dataGridView_holdList.Location = new System.Drawing.Point(6, 65);
            this.dataGridView_holdList.MultiSelect = false;
            this.dataGridView_holdList.Name = "dataGridView_holdList";
            this.dataGridView_holdList.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.DarkGray;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.DarkGray;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_holdList.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridView_holdList.RowHeadersVisible = false;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.NullValue = null;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.White;
            this.dataGridView_holdList.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView_holdList.RowTemplate.Height = 23;
            this.dataGridView_holdList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridView_holdList.Size = new System.Drawing.Size(492, 316);
            this.dataGridView_holdList.TabIndex = 9;
            this.dataGridView_holdList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dataGridView_holdList_MouseUp);
            // 
            // text_totalKrw
            // 
            this.text_totalKrw.BackColor = System.Drawing.Color.Black;
            this.text_totalKrw.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.text_totalKrw.Font = new System.Drawing.Font("Arial", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.text_totalKrw.ForeColor = System.Drawing.Color.White;
            this.text_totalKrw.Location = new System.Drawing.Point(6, 20);
            this.text_totalKrw.Name = "text_totalKrw";
            this.text_totalKrw.ReadOnly = true;
            this.text_totalKrw.Size = new System.Drawing.Size(492, 39);
            this.text_totalKrw.TabIndex = 8;
            this.text_totalKrw.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.text_totalKrw.Enter += new System.EventHandler(this.text_focus_disable);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Black;
            this.groupBox1.Controls.Add(this.text_log);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.Color.White;
            this.groupBox1.Location = new System.Drawing.Point(638, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(284, 500);
            this.groupBox1.TabIndex = 34;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.list_coinName);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.ForeColor = System.Drawing.Color.White;
            this.groupBox2.Location = new System.Drawing.Point(12, 25);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(110, 488);
            this.groupBox2.TabIndex = 35;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "List";
            // 
            // btn_trader
            // 
            this.btn_trader.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_trader.BackColor = System.Drawing.Color.DarkGray;
            this.btn_trader.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold);
            this.btn_trader.Location = new System.Drawing.Point(128, 519);
            this.btn_trader.Name = "btn_trader";
            this.btn_trader.Size = new System.Drawing.Size(164, 30);
            this.btn_trader.TabIndex = 36;
            this.btn_trader.Text = "Trader";
            this.btn_trader.UseVisualStyleBackColor = false;
            this.btn_trader.Click += new System.EventHandler(this.btn_trader_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipTitle = "Bithumb Dealer";
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Bithumb Dealer";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseClick);
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // btn_history
            // 
            this.btn_history.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_history.BackColor = System.Drawing.Color.DarkGray;
            this.btn_history.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Bold);
            this.btn_history.Location = new System.Drawing.Point(298, 519);
            this.btn_history.Name = "btn_history";
            this.btn_history.Size = new System.Drawing.Size(164, 30);
            this.btn_history.TabIndex = 37;
            this.btn_history.Text = "History";
            this.btn_history.UseVisualStyleBackColor = false;
            this.btn_history.Click += new System.EventHandler(this.btn_history_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox_show,
            this.toolStripTextBox_exit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.ShowItemToolTips = false;
            this.contextMenuStrip.Size = new System.Drawing.Size(161, 52);
            // 
            // toolStripTextBox_show
            // 
            this.toolStripTextBox_show.BackColor = System.Drawing.Color.White;
            this.toolStripTextBox_show.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripTextBox_show.Name = "toolStripTextBox_show";
            this.toolStripTextBox_show.ReadOnly = true;
            this.toolStripTextBox_show.Size = new System.Drawing.Size(100, 22);
            this.toolStripTextBox_show.Text = "Show";
            this.toolStripTextBox_show.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolStripTextBox_show.Click += new System.EventHandler(this.toolStripTextBox_show_Click);
            // 
            // toolStripTextBox_exit
            // 
            this.toolStripTextBox_exit.BackColor = System.Drawing.Color.White;
            this.toolStripTextBox_exit.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripTextBox_exit.Name = "toolStripTextBox_exit";
            this.toolStripTextBox_exit.ReadOnly = true;
            this.toolStripTextBox_exit.Size = new System.Drawing.Size(100, 22);
            this.toolStripTextBox_exit.Text = "Exit";
            this.toolStripTextBox_exit.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.toolStripTextBox_exit.Click += new System.EventHandler(this.toolStripTextBox_exit_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(934, 561);
            this.Controls.Add(this.btn_history);
            this.Controls.Add(this.btn_trader);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.group_account);
            this.Controls.Add(this.text_BTAI);
            this.Controls.Add(this.text_btaiValue);
            this.Controls.Add(this.text_btmiValue);
            this.Controls.Add(this.text_BTMI);
            this.Controls.Add(this.text_curTime);
            this.Controls.Add(this.btn_showChart);
            this.Controls.Add(this.btn_macro);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(950, 600);
            this.MinimumSize = new System.Drawing.Size(950, 600);
            this.Name = "Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Bithumb Dealer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.main_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.main_FormClosed);
            this.Load += new System.EventHandler(this.main_Load);
            this.Resize += new System.EventHandler(this.main_Resize);
            this.group_account.ResumeLayout(false);
            this.group_account.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_holdList)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.contextMenuStrip.ResumeLayout(false);
            this.contextMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox text_log;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Timer timer_log;
        private System.Windows.Forms.ListBox list_coinName;
        private System.Windows.Forms.Button btn_showChart;
        private System.Windows.Forms.Button btn_macro;
        private System.Windows.Forms.TextBox text_curTime;
        private System.Windows.Forms.Timer timer_panel;
        private System.Windows.Forms.TextBox text_BTMI;
        private System.Windows.Forms.TextBox text_btmiValue;
        private TextBox text_btaiValue;
        private TextBox text_BTAI;
        private GroupBox group_account;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private TextBox text_totalKrw;
        private Button btn_trader;
        private NotifyIcon notifyIcon;
        private DataGridView dataGridView_holdList;
        private Button btn_history;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripTextBox toolStripTextBox_show;
        private ToolStripTextBox toolStripTextBox_exit;
    }
}

