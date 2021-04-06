using BithumbDealer.src;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BithumbDealer.form
{
    public partial class graph : Form
    {
        bool isInit = false;
        bool AllStop = false;
        string coinName;
        ApiData apiData;

        DataView Main_Data;
        DataView Bollinger_Data;
        DataView MFI_Data;
        DataView Stochastic_Data;

        DateTime xRangeMaxInit = new DateTime();
        DateTime xRangeMax = new DateTime();
        int needShowType = 0;
        int dataShowType = 0;
        int beforeShowType = 0;
        double mouseDownX;

        DateTime bindLastTime = DateTime.Now;
        DateTime bindCurTime = DateTime.Now;

        Thread updateThread;
        private readonly object lock_update = new object();


        public graph(string coinName, string sAPI_Key, string sAPI_Secret)
        {
            InitializeComponent();
            this.coinName = coinName;
            this.Name = coinName + " Chart";
            this.Text = coinName + " Chart";
            apiData = new ApiData(sAPI_Key, sAPI_Secret);
        }
        private void graph_Load(object sender, EventArgs e)
        {
            // load and make data
            // make indicators and bind to chart
            {
                Main_Data = getDataTable(0);
                if (Main_Data == null)
                {
                    MessageBox.Show("Fail to load candle data.");
                    Close();
                    return;
                }
                Bollinger_Data = makeBollinger(Main_Data);
                MFI_Data = makeMFI(Main_Data);
                Stochastic_Data = makeStochastic(Main_Data);
                bindChart();
            }

            // set initial value
            {
                chart.ChartAreas["ChartArea1"].AxisX.Maximum = ((DateTime)Main_Data[Main_Data.Count - 1]["date"]).ToOADate();
                chart.ChartAreas["ChartArea2"].AxisX.Maximum = ((DateTime)Main_Data[Main_Data.Count - 1]["date"]).ToOADate();
                chart.ChartAreas["ChartArea1"].AxisY2.Maximum = 100;
                chart.ChartAreas["ChartArea1"].AxisY2.Minimum = 0;
                chart.ChartAreas["ChartArea2"].AxisY.Maximum = 100;
                chart.ChartAreas["ChartArea2"].AxisY.Minimum = 0;
                xRangeMaxInit = DateTime.Now.AddSeconds(DateTime.Now.Second).AddMilliseconds(DateTime.Now.Millisecond);

                defaultRangeSetting();
                DateTime datetime = (DateTime)Main_Data[Main_Data.Count - 1]["date"];
                xRangeMax = datetime.AddMinutes(3);
                chart.ChartAreas["ChartArea1"].AxisX.Maximum = xRangeMax.ToOADate();
                chart.ChartAreas["ChartArea2"].AxisX.Maximum = xRangeMax.ToOADate();
                setXaxisRange();
                setYaxisRange();
            }

            // thread start
            {
                updateThread = new Thread(executeUpdate);
                updateThread.Start();

                isInit = true;
            }
        }
        private void graph_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isInit)
            {
                AllStop = true;
                updateThread.Join();
            }
        }
        private void graph_Resize(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                chart.Size = new Size(Size.Width - 63, Size.Height - 112);
                trkbar_vertical.Size = new Size(45, Size.Height - 112);
                trkbar_horizontal.Size = new Size(Size.Width - 63, 45);
                btn_reset.Location = new Point(Size.Width - 101, 9);
                trkbar_vertical.Location = new Point(Size.Width - 51, 38);
                trkbar_horizontal.Location = new Point(12, Size.Height - 72);
                setXaxisRange();
            }
        }


        private DataView getDataTable(int typeVal)
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("date", typeof(DateTime));
            dataTable.Columns.Add("open", typeof(float));
            dataTable.Columns.Add("close", typeof(float));
            dataTable.Columns.Add("max", typeof(float));
            dataTable.Columns.Add("min", typeof(float));
            dataTable.Columns.Add("volume", typeof(float));

            string type = "";
            switch (typeVal)
            {
                case 0: type = "1m"; break;
                case 1: type = "3m"; break;
                case 2: type = "5m"; break;
                case 3: type = "10m"; break;
                case 4: type = "30m"; break;
                case 5: type = "1h"; break;
                case 6: type = "6h"; break;
                case 7: type = "12h"; break;
                case 8:
                case 9: type = "24h"; break;
            }

            JObject jObject = apiData.publicInfo(c.bCandleStick, coinName, new Dictionary<string, string> { { "interval", type } });
            if (jObject == null) return null;
            JArray candleData = (JArray)jObject["data"];
            if (candleData.Count < 2) return null;

            DateTime biasDateTime = new DateTime(1970, 1, 1, 9, 0, 0);
            int startIndex = candleData.Count - 500 < 0 ? 0 : candleData.Count - 500;
            if (typeVal == 9) startIndex = 0;

            for (int i = startIndex; i < candleData.Count; i++)
            {
                DataRow dataRow = dataTable.NewRow();
                dataRow["date"] = biasDateTime.AddSeconds(long.Parse(candleData[i][0].ToString()) / 1000);
                dataRow["open"] = (float)candleData[i][1];
                dataRow["close"] = (float)candleData[i][2];
                dataRow["max"] = (float)candleData[i][3];
                dataRow["min"] = (float)candleData[i][4];
                dataRow["volume"] = (float)candleData[i][5];
                dataTable.Rows.Add(dataRow);
            }

            dataTable.Rows[dataTable.Rows.Count - 1]["date"] = dataTable.Rows[dataTable.Rows.Count - 2]["date"];
            switch (typeVal)
            {
                case 0:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddMinutes(1);
                    break;
                case 1:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddMinutes(3);
                    break;
                case 2:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddMinutes(5);
                    break;
                case 3:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddMinutes(10);
                    break;
                case 4:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddMinutes(30);
                    break;
                case 5:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddHours(1);
                    break;
                case 6:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddHours(6);
                    break;
                case 7:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddHours(12);
                    break;
                case 8:
                case 9:
                    dataTable.Rows[dataTable.Rows.Count - 1]["date"] = ((DateTime)dataTable.Rows[dataTable.Rows.Count - 1]["date"]).AddDays(1);
                    break;
            }

            if (typeVal == 9)
            {
                bool isLastAdded = false;
                DateTime dateTime = (DateTime)dataTable.Rows[0]["date"];
                dateTime = dateTime.AddDays(1 - dateTime.Day);

                bool isFirst = true;
                DataTable monthTable = dataTable.Clone();
                DataRow dataRow = monthTable.NewRow();
                dataRow["date"] = dateTime;
                dataRow["max"] = float.MinValue;
                dataRow["min"] = float.MaxValue;
                dataRow["volume"] = 0f;

                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    DateTime compDate = (DateTime)dataTable.Rows[i]["date"];
                    compDate = compDate.AddDays(1 - compDate.Day);

                    if (dateTime.Month != compDate.Month)
                    {
                        isLastAdded = true;
                        dateTime = compDate;

                        monthTable.Rows.Add(dataRow);
                        dataRow = monthTable.NewRow();
                        dataRow["date"] = dateTime;
                        dataRow["max"] = float.MinValue;
                        dataRow["min"] = float.MaxValue;
                        dataRow["volume"] = 0f;
                        isFirst = true;
                    }
                    else
                        isLastAdded = false;

                    if (isFirst)
                    {
                        dataRow["open"] = (float)dataTable.Rows[i]["open"];
                        isFirst = false;
                    }
                    dataRow["close"] = (float)dataTable.Rows[i]["close"];
                    if ((float)dataRow["max"] < (float)dataTable.Rows[i]["max"])
                        dataRow["max"] = (float)dataTable.Rows[i]["max"];
                    if ((float)dataRow["min"] > (float)dataTable.Rows[i]["min"])
                        dataRow["min"] = (float)dataTable.Rows[i]["min"];
                    dataRow["volume"] = (float)dataTable.Rows[i]["volume"];
                }
                if (!isLastAdded)
                    monthTable.Rows.Add(dataRow);

                return new DataView(monthTable);
            }
            else
                return new DataView(dataTable);
        }
        private DataView makeBollinger(DataView candleData)
        {
            DataTable bollingerTable = new DataTable();
            bollingerTable.Columns.Add("date", typeof(DateTime));
            bollingerTable.Columns.Add("top", typeof(float));
            bollingerTable.Columns.Add("mid", typeof(float));
            bollingerTable.Columns.Add("bot", typeof(float));

            for (int j = 0; j < candleData.Count; j++)
            {
                DataRow dataRow = bollingerTable.NewRow();
                dataRow["date"] = candleData[j]["date"];
                dataRow["top"] = 0;
                dataRow["mid"] = 0;
                dataRow["bot"] = 0;

                int startIndex = j - 28 < 0 ? 0 : j - 28;
                float averagePrice = 0;
                float dispersion = 0;
                for (int k = startIndex; k < j; k++)
                    averagePrice += (float)candleData[k]["close"];
                averagePrice /= j - startIndex;
                for (int k = startIndex; k < j; k++)
                    dispersion += (float)Math.Pow(averagePrice - (float)candleData[k]["close"], 2);
                dispersion /= j - startIndex;
                dispersion = (float)Math.Sqrt(dispersion);

                dataRow["top"] = averagePrice + 2 * dispersion;
                dataRow["mid"] = averagePrice;
                dataRow["bot"] = averagePrice - 2 * dispersion;

                bollingerTable.Rows.Add(dataRow);
            }

            return new DataView(bollingerTable);
        }
        private DataView makeMFI(DataView candleData)
        {
            DataTable MFITable = new DataTable();
            MFITable.Columns.Add("date", typeof(DateTime));
            MFITable.Columns.Add("value", typeof(float));

            for (int j = 1; j < candleData.Count; j++)
            {
                DataRow dataRow = MFITable.NewRow();
                dataRow["date"] = candleData[j]["date"];
                dataRow["value"] = 0;

                int startIndex = j - 14 < 1 ? 1 : j - 14;
                float PM = 0;
                float NM = 0;
                float MR = 0;
                for (int k = startIndex; k <= j; k++)
                {
                    float before =
                        ((float)candleData[k - 1]["max"]
                        + (float)candleData[k - 1]["min"]
                        + (float)candleData[k - 1]["close"])
                        / 3f;
                    float cur =
                        ((float)candleData[k]["max"]
                        + (float)candleData[k]["min"]
                        + (float)candleData[k]["close"])
                        / 3f;

                    if (before < cur) PM += cur * (float)candleData[k]["volume"];
                    else if (before > cur) NM += cur * (float)candleData[k]["volume"];
                }

                MR = PM / NM;
                dataRow["value"] = NM < 0.1 ? 100 : MR / (1 + MR) * 100;
                MFITable.Rows.Add(dataRow);
            }

            return new DataView(MFITable);
        }
        private DataView makeStochastic(DataView candleData)
        {
            DataTable StochasticTable = new DataTable();
            StochasticTable.Columns.Add("date", typeof(DateTime));
            StochasticTable.Columns.Add("value", typeof(float));
            StochasticTable.Columns.Add("K", typeof(float));
            StochasticTable.Columns.Add("D", typeof(float));

            for (int j = 0; j < candleData.Count; j++)
            {
                DataRow dataRow = StochasticTable.NewRow();
                dataRow["date"] = candleData[j]["date"];
                dataRow["value"] = 0;
                dataRow["K"] = 0;
                dataRow["D"] = 0;

                float max = float.MinValue;
                float min = float.MaxValue;
                float last = (float)candleData[j]["close"];
                int startIndex = j - 13 < 0 ? 0 : j - 13;
                for (int k = startIndex; k <= j; k++)
                {
                    if (max < (float)candleData[k]["max"])
                        max = (float)candleData[k]["max"];
                    if (min > (float)candleData[k]["min"])
                        min = (float)candleData[k]["min"];
                }

                if (max > float.MinValue + 1 && min < float.MaxValue - 1)
                {
                    dataRow["value"] = ((last - min) / (max - min)) * 100.0;
                    StochasticTable.Rows.Add(dataRow);
                }
            }

            // slow stochastic
            for (int j = 0; j < StochasticTable.Rows.Count; j++)
            {
                int startIndex = j - 2 < 0 ? 0 : j - 2;
                float avg = 0;
                for (int k = startIndex; k <= j; k++)
                    avg += (float)StochasticTable.Rows[k]["value"];
                avg /= 3f;
                StochasticTable.Rows[j]["K"] = avg;
            }
            for (int j = 0; j < StochasticTable.Rows.Count; j++)
            {
                int startIndex = j - 2 < 0 ? 0 : j - 2;
                float avg = 0;
                for (int k = startIndex; k <= j; k++)
                    avg += (float)StochasticTable.Rows[k]["K"];
                avg /= 3f;
                StochasticTable.Rows[j]["D"] = avg;
            }

            return new DataView(StochasticTable);
        }
        private void bindChart()
        {
            lock (lock_update)
            {
                chart.Series["candle"].Points.DataBind(Main_Data, "date", "max,min,open,close", "");

                chart.Series["top"].Points.DataBind(Bollinger_Data, "date", "top", "");
                chart.Series["mid"].Points.DataBind(Bollinger_Data, "date", "mid", "");
                chart.Series["bot"].Points.DataBind(Bollinger_Data, "date", "bot", "");

                chart.Series["MFI"].Points.DataBind(MFI_Data, "date", "value", "");

                chart.Series["Stochastic_k"].Points.DataBind(Stochastic_Data, "date", "K", "");
                chart.Series["Stochastic_d"].Points.DataBind(Stochastic_Data, "date", "D", "");
            }
        }


        private void trkbar_horizontal_Scroll(object sender, EventArgs e)
        {
            setXaxisRange();
            setYaxisRange();
        }
        private void trkbar_vertical_Scroll(object sender, EventArgs e)
        {
            setYaxisRange();
        }
        private void setXaxisRange()
        {
            int numCandle = 50 + trkbar_horizontal.Value;
            chart.Series["candle"]["PixelPointWidth"] = (chart.Width / numCandle * 0.6).ToString();
            switch (beforeShowType)
            {
                case 0:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH:mm";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 5;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH:mm";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 5;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH:mm";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 20;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH:mm";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 20;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddMinutes(-numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddMinutes(-numCandle).ToOADate();
                        break;
                    }
                case 1:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH:mm";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 15;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH:mm";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 15;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd/HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd/HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 1;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddMinutes(-3 * numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddMinutes(-3 * numCandle).ToOADate();
                        break;
                    }
                case 2:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 30;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Minutes;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 30;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 2;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 2;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddMinutes(-5 * numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddMinutes(-5 * numCandle).ToOADate();
                        break;
                    }
                case 3:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 1;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd-HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 4;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd-HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 4;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddMinutes(-10 * numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddMinutes(-10 * numCandle).ToOADate();
                        break;
                    }
                case 4:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 2;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 2;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd-HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 8;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd-HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 8;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddMinutes(-30 * numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddMinutes(-30 * numCandle).ToOADate();
                        break;
                    }
                case 5:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd-HH";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 5;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Hours;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd-HH";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 5;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 1;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddHours(-numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddHours(-numCandle).ToOADate();
                        break;
                    }
                case 6:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 1;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "MM-dd";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 5;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "MM-dd";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 5;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddHours(-6 * numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddHours(-6 * numCandle).ToOADate();
                        break;
                    }
                case 7:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 2;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 2;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "MM-dd";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 10;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "MM-dd";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 10;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddHours(-12 * numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddHours(-12 * numCandle).ToOADate();
                        break;
                    }
                case 8:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 5;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Days;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "dd";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 5;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Months;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "yyyy-MM";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Months;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "yyyy-MM";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 1;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddDays(-numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddDays(-numCandle).ToOADate();
                        break;
                    }
                case 9:
                    {
                        if (numCandle < 100)
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Months;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "yyyy-MM";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 5;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Months;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "yyyy-MM";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 5;
                        }
                        else
                        {
                            chart.ChartAreas["ChartArea1"].AxisX.IntervalType = DateTimeIntervalType.Years;
                            chart.ChartAreas["ChartArea1"].AxisX.LabelStyle.Format = "yyyy";
                            chart.ChartAreas["ChartArea1"].AxisX.Interval = 1;
                            chart.ChartAreas["ChartArea2"].AxisX.IntervalType = DateTimeIntervalType.Years;
                            chart.ChartAreas["ChartArea2"].AxisX.LabelStyle.Format = "yyyy";
                            chart.ChartAreas["ChartArea2"].AxisX.Interval = 1;
                        }
                        chart.ChartAreas["ChartArea1"].AxisX.Minimum = xRangeMax.AddMonths(-numCandle).ToOADate();
                        chart.ChartAreas["ChartArea2"].AxisX.Minimum = xRangeMax.AddMonths(-numCandle).ToOADate();
                        break;
                    }
            }
        }
        private void setYaxisRange()
        {
            double max = Double.MinValue;
            double min = Double.MaxValue;
            double leftLimit = chart.ChartAreas[0].AxisX.Minimum;
            double rightLimit = chart.ChartAreas[0].AxisX.Maximum;

            foreach (DataPoint dp in chart.Series["candle"].Points)
            {
                if (dp.XValue >= leftLimit && dp.XValue <= rightLimit)
                {
                    min = Math.Min(min, dp.YValues[1]);
                    max = Math.Max(max, dp.YValues[0]);
                }
            }

            double gap = (max - min) * (trkbar_vertical.Value + 10) / 50;
            if (gap > 0)
            {
                chart.ChartAreas["ChartArea1"].AxisY.Maximum = max + gap;
                chart.ChartAreas["ChartArea1"].AxisY.Minimum = min - gap;

                if (chart.ChartAreas["ChartArea1"].AxisY.Maximum > 10000000000)
                    chart.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "{0:#,0,,, G}";
                else if (chart.ChartAreas["ChartArea1"].AxisY.Maximum > 10000000)
                    chart.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "{0:#,0,, M}";
                else if (chart.ChartAreas["ChartArea1"].AxisY.Maximum > 10000)
                    chart.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "{0:#,0,K}";
                else
                    chart.ChartAreas["ChartArea1"].AxisY.LabelStyle.Format = "{0:,0.###}";
            }
        }
        private void chart_MouseDown(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left)) return;
            mouseDownX = e.Location.X;
        }
        private void chart_MouseMove(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left)) return;
            else if (e.Location.X < 0 || e.Location.X > chart.Size.Width) return;

            double range = chart.ChartAreas["ChartArea1"].AxisX.Maximum - chart.ChartAreas["ChartArea1"].AxisX.Minimum;
            double distance = (e.Location.X - mouseDownX) / (double)chart.Size.Width * 2;
            chart.ChartAreas["ChartArea1"].AxisX.Maximum -= range * distance;
            chart.ChartAreas["ChartArea1"].AxisX.Minimum = chart.ChartAreas["ChartArea1"].AxisX.Maximum - range;
            chart.ChartAreas["ChartArea2"].AxisX.Maximum = chart.ChartAreas["ChartArea1"].AxisX.Maximum;
            chart.ChartAreas["ChartArea2"].AxisX.Minimum = chart.ChartAreas["ChartArea1"].AxisX.Minimum;
            mouseDownX = e.Location.X;
        }
        private void chart_MouseUp(object sender, MouseEventArgs e)
        {
            if (!e.Button.HasFlag(MouseButtons.Left)) return;
            if (chart.ChartAreas["ChartArea1"].AxisX.Maximum <= chart.ChartAreas["ChartArea1"].AxisX.Minimum)
            {
                MessageBox.Show("Error : x range");
                Close();
                return;
            }
            xRangeMax = DateTime.FromOADate(chart.ChartAreas["ChartArea1"].AxisX.Maximum).AddMinutes(-1);
            setYaxisRange();
        }


        private void resetRangeBtn()
        {
            btn_1min.BackColor = Color.DarkGray;
            btn_3min.BackColor = Color.DarkGray;
            btn_5min.BackColor = Color.DarkGray;
            btn_10min.BackColor = Color.DarkGray;
            btn_30min.BackColor = Color.DarkGray;
            btn_1hour.BackColor = Color.DarkGray;
            btn_6hour.BackColor = Color.DarkGray;
            btn_12hour.BackColor = Color.DarkGray;
            btn_24hour.BackColor = Color.DarkGray;
            btn_month.BackColor = Color.DarkGray;
        }
        private void defaultRangeSetting()
        {
            needShowType = 0;
            resetRangeBtn();
            btn_1min.BackColor = Color.Red;
        }
        private void btn_reset_Click(object sender, EventArgs e)
        {
            xRangeMax = xRangeMaxInit.AddMinutes(3);
            defaultRangeSetting();
        }
        private void btn_1min_Click(object sender, EventArgs e)
        {
            needShowType = 0;
            resetRangeBtn();
            btn_1min.BackColor = Color.Red;
        }
        private void btn_3min_Click(object sender, EventArgs e)
        {
            needShowType = 1;
            resetRangeBtn();
            btn_3min.BackColor = Color.Red;
        }
        private void btn_5min_Click(object sender, EventArgs e)
        {
            needShowType = 2;
            resetRangeBtn();
            btn_5min.BackColor = Color.Red;
        }
        private void btn_10min_Click(object sender, EventArgs e)
        {
            needShowType = 3;
            resetRangeBtn();
            btn_10min.BackColor = Color.Red;
        }
        private void btn_30min_Click(object sender, EventArgs e)
        {
            needShowType = 4;
            resetRangeBtn();
            btn_30min.BackColor = Color.Red;
        }
        private void btn_1hour_Click(object sender, EventArgs e)
        {
            needShowType = 5;
            resetRangeBtn();
            btn_1hour.BackColor = Color.Red;
        }
        private void btn_6hour_Click(object sender, EventArgs e)
        {
            needShowType = 6;
            resetRangeBtn();
            btn_6hour.BackColor = Color.Red;
        }
        private void btn_12hour_Click(object sender, EventArgs e)
        {
            needShowType = 7;
            resetRangeBtn();
            btn_12hour.BackColor = Color.Red;
        }
        private void btn_24hour_Click(object sender, EventArgs e)
        {
            needShowType = 8;
            resetRangeBtn();
            btn_24hour.BackColor = Color.Red;
        }
        private void btn_month_Click(object sender, EventArgs e)
        {
            needShowType = 9;
            resetRangeBtn();
            btn_month.BackColor = Color.Red;
        }


        private void timer_update_Tick(object sender, EventArgs e)
        {
            bindChart();
            if (beforeShowType != dataShowType)
            {
                DateTime datetime;
                lock (lock_update) datetime = (DateTime)Main_Data[Main_Data.Count - 1]["date"];
                if (DateTime.Compare(datetime, xRangeMax) < 0)
                {
                    switch (dataShowType)
                    {
                        case 0:
                            xRangeMax = datetime.AddMinutes(3);
                            break;
                        case 1:
                            xRangeMax = datetime.AddMinutes(9);
                            break;
                        case 2:
                            xRangeMax = datetime.AddMinutes(15);
                            break;
                        case 3:
                            xRangeMax = datetime.AddMinutes(30);
                            break;
                        case 4:
                            xRangeMax = datetime.AddMinutes(90);
                            break;
                        case 5:
                            xRangeMax = datetime.AddHours(3);
                            break;
                        case 6:
                            xRangeMax = datetime.AddHours(18);
                            break;
                        case 7:
                            xRangeMax = datetime.AddHours(36);
                            break;
                        case 8:
                            xRangeMax = datetime.AddDays(3);
                            break;
                        case 9:
                            xRangeMax = datetime.AddMonths(3);
                            break;
                    }
                    chart.ChartAreas["ChartArea1"].AxisX.Maximum = xRangeMax.ToOADate();
                    chart.ChartAreas["ChartArea2"].AxisX.Maximum = xRangeMax.ToOADate();
                }
                beforeShowType = dataShowType;
                setXaxisRange();
                setYaxisRange();
            }
            bindCurTime = DateTime.Now;
            if (bindLastTime.Minute != bindCurTime.Minute)
            {
                bindLastTime = bindCurTime;
                xRangeMaxInit = xRangeMaxInit.AddMinutes(1);

                if (beforeShowType == 0) { xRangeMax = xRangeMax.AddMinutes(1); }
                else if (beforeShowType == 1 && bindCurTime.Minute % 3 == 0) { xRangeMax = xRangeMax.AddMinutes(3); }
                else if (beforeShowType == 2 && bindCurTime.Minute % 5 == 0) { xRangeMax = xRangeMax.AddMinutes(5); }
                else if (beforeShowType == 3 && bindCurTime.Minute % 10 == 0) { xRangeMax = xRangeMax.AddMinutes(10); }
                else if (beforeShowType == 4 && bindCurTime.Minute % 30 == 0) { xRangeMax = xRangeMax.AddMinutes(30); }
                else if (beforeShowType == 5 && bindCurTime.Minute == 0) { xRangeMax = xRangeMax.AddHours(1); }
                else if (beforeShowType == 6 && bindCurTime.Hour % 6 == 0) { xRangeMax = xRangeMax.AddHours(6); }
                else if (beforeShowType == 7 && bindCurTime.Hour % 12 == 0) { xRangeMax = xRangeMax.AddHours(12); }
                else if (beforeShowType == 8 && bindCurTime.Hour == 0) { xRangeMax = xRangeMax.AddDays(1); }

                chart.ChartAreas["ChartArea1"].AxisX.Maximum = xRangeMax.ToOADate();
                chart.ChartAreas["ChartArea2"].AxisX.Maximum = xRangeMax.ToOADate();
                setXaxisRange();
                setYaxisRange();
            }
        }
        private void executeUpdate()
        {
            while (!AllStop)
            {
                int tempType = needShowType;
                DataView candleData = getDataTable(needShowType);
                if (candleData == null) continue;
                DataView bbData = makeBollinger(candleData);
                DataView mfiData = makeMFI(candleData);
                DataView stocData = makeStochastic(candleData);

                lock (lock_update)
                {
                    Main_Data = candleData;
                    Bollinger_Data = bbData;
                    MFI_Data = mfiData;
                    Stochastic_Data = stocData;
                }
                dataShowType = tempType;

                for (int i = 0; i < 20; i++)
                {
                    if (AllStop || dataShowType != needShowType) break;
                    Thread.Sleep(100);
                }
            }
        }
    }
}
