using BithumbDealer.src;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BithumbDealer.form
{
    public partial class Trader : Form
    {
        private React react;
        private List<string> coinList;

        private bool AllStop = false;
        private bool isInit = false;

        private string selectedName = "";
        private float krw = 0;
        private float currency = 0;
        private float price = 0;
        private float units = 0;
        private float total = 0;

        private Thread thread_updater;
        private readonly object lock_updater = new object();
        private readonly object lock_select = new object();
        private float[] tickerData = new float[11];
        private JArray transactionData = new JArray();
        private JArray bid = new JArray();
        private JArray ask = new JArray();
        private float THMax = float.MinValue;
        private float[] vbalanceData = new float[7];

        private bool selected = true;
        private bool needTradeInit = false;
        private bool needTradeUpdate = false;
        private bool canTradeSet = false;
        private bool isBuy = true;
        private bool isPlace = true;


        public Trader(string sAPI_Key, string sAPI_Secret, List<string> coinList)
        {
            InitializeComponent();
            this.react = new React(sAPI_Key, sAPI_Secret);
            this.coinList = coinList;
        }
        private void Trader_Load(object sender, EventArgs e)
        {
            if (coinList.Count == 0)
            {
                MessageBox.Show("Init fail, try again.");
                Close();
            }

            for (int i = 0; i < coinList.Count; i++)
                list_coinName.Items.Add(coinList[i]);

            thread_updater = new Thread(() => executeDataUpdate());
            thread_updater.Start();
            isInit = true;
        }
        private void Trader_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!isInit) return;

            AllStop = true;
            thread_updater.Join();
        }
        private void text_focus_disable(object sender, EventArgs e)
        {
            groupBox3.Focus();
        }


        private void executeDataUpdate()
        {
            while (!AllStop)
            {
                lock (lock_select)
                    if (selectedName != "" && selected)
                    {
                        string name = selectedName;
                        bool[] isNeedUpdate = { false, false, false, false };

                        float[] tempTickerData = null;
                        JArray tempTHData = null;
                        JArray[] tempOBData = null;
                        float tempTHMax = float.MinValue;
                        float[] tempBalacneData = null;

                        if (selected)
                        {
                            tempTickerData = react.getTickerData(name);
                            if (tempTickerData != null) isNeedUpdate[0] = true;
                            else selected = false;
                        }

                        if (selected)
                        {
                            tempTHData = react.getTransactionData(name);
                            if (tempTHData != null) isNeedUpdate[1] = true;
                            else selected = false;
                        }

                        if (selected)
                        {
                            tempOBData = react.getOrderBookData(name);
                            if (tempOBData != null)
                            {
                                for (int i = 0; i < 15; i++)
                                    tempTHMax = Math.Max(tempTHMax, (float)tempOBData[0][i]["quantity"]);
                                for (int i = 5; i < 20; i++)
                                    tempTHMax = Math.Max(tempTHMax, (float)tempOBData[1][i]["quantity"]);
                                isNeedUpdate[2] = true;
                            }
                            else selected = false;
                        }

                        if (needTradeInit && selected)
                        {
                            tempBalacneData = react.getBalanceData(name);
                            if (tempBalacneData != null)
                            {
                                isNeedUpdate[3] = true;
                                needTradeInit = false;
                            }
                            else selected = false;
                        }

                        if (selected)
                        {
                            lock (lock_updater)
                            {
                                if (isNeedUpdate[0])
                                    for (int i = 0; i < 11; i++)
                                        tickerData[i] = tempTickerData[i];

                                if (isNeedUpdate[1])
                                    transactionData = tempTHData;

                                if (isNeedUpdate[2])
                                {
                                    bid = tempOBData[0];
                                    ask = tempOBData[1];
                                    THMax = tempTHMax;
                                }

                                if (isNeedUpdate[3])
                                {
                                    for (int i = 0; i < 7; i++)
                                        vbalanceData[i] = tempBalacneData[i];
                                    needTradeUpdate = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("API error, try again.");
                        }
                    }

                for (int i = 0; i < 10; i++)
                {
                    if (AllStop) break;
                    Thread.Sleep(100);
                }
            }
        }
        private void timer_updater_Tick(object sender, EventArgs e)
        {
            if (selected)
            {
                lock (lock_updater)
                {
                    refreshTicker();
                    refreshTransactionHistory();
                    refreshOrderBook();
                    refreshTrade();
                }
            }
            else
            {
                text_value.ForeColor = Color.White;
                text_value.Text = "Delisting";
                text_fluctate.Text = "";
                text_fluctate_rate.Text = "";
                text_prev_close.Text = "";
                text_max.Text = "";
                text_min.Text = "";
                text_open.Text = "";
                text_close.Text = "";
                text_candle1.BackColor = Color.DarkGray;
                text_candle2.BackColor = Color.DarkGray;
                text_candle3.BackColor = Color.DarkGray;
            }
        }
        private void refreshTicker()
        {
            if (selectedName != "")
            {
                text_name.Text = selectedName;
                if (tickerData[10] >= 0)
                {
                    text_value.ForeColor = Color.Red;
                    text_fluctate.ForeColor = Color.Red;
                    text_fluctate_rate.ForeColor = Color.Red;
                }
                else
                {
                    text_value.ForeColor = Color.DodgerBlue;
                    text_fluctate.ForeColor = Color.DodgerBlue;
                    text_fluctate_rate.ForeColor = Color.DodgerBlue;
                }
                text_fluctate.Text = tickerData[9].ToString(",0.####");
                text_fluctate_rate.Text = tickerData[10].ToString() + "%";
                text_prev_close.Text = tickerData[6].ToString(",0.####");
                if (tickerData[0] <= tickerData[1])
                {
                    text_candle1.BackColor = Color.Red;
                    text_candle2.BackColor = Color.Red;
                    text_candle3.BackColor = Color.Red;
                    text_open.Text = tickerData[0].ToString(",0.####");
                    text_close.Text = tickerData[1].ToString(",0.####");
                }
                else
                {
                    text_candle1.BackColor = Color.DodgerBlue;
                    text_candle2.BackColor = Color.DodgerBlue;
                    text_candle3.BackColor = Color.DodgerBlue;
                    text_open.Text = tickerData[1].ToString(",0.####");
                    text_close.Text = tickerData[0].ToString(",0.####");
                }
                text_min.Text = tickerData[2].ToString(",0.####");
                text_max.Text = tickerData[3].ToString(",0.####");
            }
            else
            {
                text_candle1.BackColor = Color.DarkGray;
                text_candle2.BackColor = Color.DarkGray;
                text_candle3.BackColor = Color.DarkGray;
                text_name.Text = "";
                text_fluctate.Text = "";
                text_fluctate_rate.Text = "";
                text_prev_close.Text = "";
                text_open.Text = "";
                text_close.Text = "";
                text_min.Text = "";
                text_max.Text = "";
            }
        }
        private void refreshTransactionHistory()
        {
            if (transactionData.Count > 4)
            {
                text_value.Text = transactionData[4]["price"].ToString();

                {
                    if (transactionData[4]["type"].ToString() == "bid")
                    {
                        text_TH0_date.BackColor = Color.LightBlue;
                        text_TH0_unit.BackColor = Color.LightBlue;
                        text_TH0_value.BackColor = Color.LightBlue;
                        text_TH0_total.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        text_TH0_date.BackColor = Color.LightPink;
                        text_TH0_unit.BackColor = Color.LightPink;
                        text_TH0_value.BackColor = Color.LightPink;
                        text_TH0_total.BackColor = Color.LightPink;
                    }
                    text_TH0_date.Text = transactionData[4]["transaction_date"].ToString();
                    text_TH0_unit.Text = transactionData[4]["units_traded"].ToString();
                    text_TH0_value.Text = transactionData[4]["price"].ToString();
                    text_TH0_total.Text = transactionData[4]["total"].ToString();
                }
                {
                    if (transactionData[3]["type"].ToString() == "bid")
                    {
                        text_TH1_date.BackColor = Color.LightBlue;
                        text_TH1_unit.BackColor = Color.LightBlue;
                        text_TH1_value.BackColor = Color.LightBlue;
                        text_TH1_total.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        text_TH1_date.BackColor = Color.LightPink;
                        text_TH1_unit.BackColor = Color.LightPink;
                        text_TH1_value.BackColor = Color.LightPink;
                        text_TH1_total.BackColor = Color.LightPink;
                    }
                    text_TH1_date.Text = transactionData[3]["transaction_date"].ToString();
                    text_TH1_unit.Text = transactionData[3]["units_traded"].ToString();
                    text_TH1_value.Text = transactionData[3]["price"].ToString();
                    text_TH1_total.Text = transactionData[3]["total"].ToString();
                }
                {
                    if (transactionData[2]["type"].ToString() == "bid")
                    {
                        text_TH2_date.BackColor = Color.LightBlue;
                        text_TH2_unit.BackColor = Color.LightBlue;
                        text_TH2_value.BackColor = Color.LightBlue;
                        text_TH2_total.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        text_TH2_date.BackColor = Color.LightPink;
                        text_TH2_unit.BackColor = Color.LightPink;
                        text_TH2_value.BackColor = Color.LightPink;
                        text_TH2_total.BackColor = Color.LightPink;
                    }
                    text_TH2_date.Text = transactionData[2]["transaction_date"].ToString();
                    text_TH2_unit.Text = transactionData[2]["units_traded"].ToString();
                    text_TH2_value.Text = transactionData[2]["price"].ToString();
                    text_TH2_total.Text = transactionData[2]["total"].ToString();
                }
                {
                    if (transactionData[1]["type"].ToString() == "bid")
                    {
                        text_TH3_date.BackColor = Color.LightBlue;
                        text_TH3_unit.BackColor = Color.LightBlue;
                        text_TH3_value.BackColor = Color.LightBlue;
                        text_TH3_total.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        text_TH3_date.BackColor = Color.LightPink;
                        text_TH3_unit.BackColor = Color.LightPink;
                        text_TH3_value.BackColor = Color.LightPink;
                        text_TH3_total.BackColor = Color.LightPink;
                    }
                    text_TH3_date.Text = transactionData[1]["transaction_date"].ToString();
                    text_TH3_unit.Text = transactionData[1]["units_traded"].ToString();
                    text_TH3_value.Text = transactionData[1]["price"].ToString();
                    text_TH3_total.Text = transactionData[1]["total"].ToString();
                }
                {
                    if (transactionData[0]["type"].ToString() == "bid")
                    {
                        text_TH4_date.BackColor = Color.LightBlue;
                        text_TH4_unit.BackColor = Color.LightBlue;
                        text_TH4_value.BackColor = Color.LightBlue;
                        text_TH4_total.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        text_TH4_date.BackColor = Color.LightPink;
                        text_TH4_unit.BackColor = Color.LightPink;
                        text_TH4_value.BackColor = Color.LightPink;
                        text_TH4_total.BackColor = Color.LightPink;
                    }
                    text_TH4_date.Text = transactionData[0]["transaction_date"].ToString();
                    text_TH4_unit.Text = transactionData[0]["units_traded"].ToString();
                    text_TH4_value.Text = transactionData[0]["price"].ToString();
                    text_TH4_total.Text = transactionData[0]["total"].ToString();
                }
            }
            else
            {
                text_TH0_date.BackColor = Color.Black;
                text_TH0_unit.BackColor = Color.Black;
                text_TH0_value.BackColor = Color.Black;
                text_TH0_total.BackColor = Color.Black;
                text_TH1_date.BackColor = Color.Black;
                text_TH1_unit.BackColor = Color.Black;
                text_TH1_value.BackColor = Color.Black;
                text_TH1_total.BackColor = Color.Black;
                text_TH2_date.BackColor = Color.Black;
                text_TH2_unit.BackColor = Color.Black;
                text_TH2_value.BackColor = Color.Black;
                text_TH2_total.BackColor = Color.Black;
                text_TH3_date.BackColor = Color.Black;
                text_TH3_unit.BackColor = Color.Black;
                text_TH3_value.BackColor = Color.Black;
                text_TH3_total.BackColor = Color.Black;
                text_TH4_date.BackColor = Color.Black;
                text_TH4_unit.BackColor = Color.Black;
                text_TH4_value.BackColor = Color.Black;
                text_TH4_total.BackColor = Color.Black;
            }
        }
        private void refreshOrderBook()
        {
            if (bid.Count > 19 && ask.Count > 19)
            {
                text_askPrice00.Text = ((float)ask[14]["price"]).ToString(",0.####");
                text_askPrice01.Text = ((float)ask[13]["price"]).ToString(",0.####");
                text_askPrice02.Text = ((float)ask[12]["price"]).ToString(",0.####");
                text_askPrice03.Text = ((float)ask[11]["price"]).ToString(",0.####");
                text_askPrice04.Text = ((float)ask[10]["price"]).ToString(",0.####");
                text_askPrice05.Text = ((float)ask[9]["price"]).ToString(",0.####");
                text_askPrice06.Text = ((float)ask[8]["price"]).ToString(",0.####");
                text_askPrice07.Text = ((float)ask[7]["price"]).ToString(",0.####");
                text_askPrice08.Text = ((float)ask[6]["price"]).ToString(",0.####");
                text_askPrice09.Text = ((float)ask[5]["price"]).ToString(",0.####");
                text_askPrice10.Text = ((float)ask[4]["price"]).ToString(",0.####");
                text_askPrice11.Text = ((float)ask[3]["price"]).ToString(",0.####");
                text_askPrice12.Text = ((float)ask[2]["price"]).ToString(",0.####");
                text_askPrice13.Text = ((float)ask[1]["price"]).ToString(",0.####");
                text_askPrice14.Text = ((float)ask[0]["price"]).ToString(",0.####");

                text_askQuantity00.Text = ((float)ask[14]["quantity"]).ToString("F5");
                text_askQuantity01.Text = ((float)ask[13]["quantity"]).ToString("F5");
                text_askQuantity02.Text = ((float)ask[12]["quantity"]).ToString("F5");
                text_askQuantity03.Text = ((float)ask[11]["quantity"]).ToString("F5");
                text_askQuantity04.Text = ((float)ask[10]["quantity"]).ToString("F5");
                text_askQuantity05.Text = ((float)ask[9]["quantity"]).ToString("F5");
                text_askQuantity06.Text = ((float)ask[8]["quantity"]).ToString("F5");
                text_askQuantity07.Text = ((float)ask[7]["quantity"]).ToString("F5");
                text_askQuantity08.Text = ((float)ask[6]["quantity"]).ToString("F5");
                text_askQuantity09.Text = ((float)ask[5]["quantity"]).ToString("F5");
                text_askQuantity10.Text = ((float)ask[4]["quantity"]).ToString("F5");
                text_askQuantity11.Text = ((float)ask[3]["quantity"]).ToString("F5");
                text_askQuantity12.Text = ((float)ask[2]["quantity"]).ToString("F5");
                text_askQuantity13.Text = ((float)ask[1]["quantity"]).ToString("F5");
                text_askQuantity14.Text = ((float)ask[0]["quantity"]).ToString("F5");

                text_askQuantity00.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[19]["quantity"] / THMax), 0, 0);
                text_askQuantity01.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[18]["quantity"] / THMax), 0, 0);
                text_askQuantity02.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[17]["quantity"] / THMax), 0, 0);
                text_askQuantity03.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[16]["quantity"] / THMax), 0, 0);
                text_askQuantity04.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[15]["quantity"] / THMax), 0, 0);
                text_askQuantity05.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[14]["quantity"] / THMax), 0, 0);
                text_askQuantity06.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[13]["quantity"] / THMax), 0, 0);
                text_askQuantity07.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[12]["quantity"] / THMax), 0, 0);
                text_askQuantity08.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[11]["quantity"] / THMax), 0, 0);
                text_askQuantity09.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[10]["quantity"] / THMax), 0, 0);
                text_askQuantity10.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[9]["quantity"] / THMax), 0, 0);
                text_askQuantity11.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[8]["quantity"] / THMax), 0, 0);
                text_askQuantity12.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[7]["quantity"] / THMax), 0, 0);
                text_askQuantity13.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[6]["quantity"] / THMax), 0, 0);
                text_askQuantity14.BackColor = Color.FromArgb(55 + (int)(200 * (float)ask[5]["quantity"] / THMax), 0, 0);

                text_bidPrice00.Text = ((float)bid[0]["price"]).ToString(",0.####");
                text_bidPrice01.Text = ((float)bid[1]["price"]).ToString(",0.####");
                text_bidPrice02.Text = ((float)bid[2]["price"]).ToString(",0.####");
                text_bidPrice03.Text = ((float)bid[3]["price"]).ToString(",0.####");
                text_bidPrice04.Text = ((float)bid[4]["price"]).ToString(",0.####");
                text_bidPrice05.Text = ((float)bid[5]["price"]).ToString(",0.####");
                text_bidPrice06.Text = ((float)bid[6]["price"]).ToString(",0.####");
                text_bidPrice07.Text = ((float)bid[7]["price"]).ToString(",0.####");
                text_bidPrice08.Text = ((float)bid[8]["price"]).ToString(",0.####");
                text_bidPrice09.Text = ((float)bid[9]["price"]).ToString(",0.####");
                text_bidPrice10.Text = ((float)bid[10]["price"]).ToString(",0.####");
                text_bidPrice11.Text = ((float)bid[11]["price"]).ToString(",0.####");
                text_bidPrice12.Text = ((float)bid[12]["price"]).ToString(",0.####");
                text_bidPrice13.Text = ((float)bid[13]["price"]).ToString(",0.####");
                text_bidPrice14.Text = ((float)bid[14]["price"]).ToString(",0.####");

                text_bidQuantity00.Text = ((float)bid[0]["quantity"]).ToString("F5");
                text_bidQuantity01.Text = ((float)bid[1]["quantity"]).ToString("F5");
                text_bidQuantity02.Text = ((float)bid[2]["quantity"]).ToString("F5");
                text_bidQuantity03.Text = ((float)bid[3]["quantity"]).ToString("F5");
                text_bidQuantity04.Text = ((float)bid[4]["quantity"]).ToString("F5");
                text_bidQuantity05.Text = ((float)bid[5]["quantity"]).ToString("F5");
                text_bidQuantity06.Text = ((float)bid[6]["quantity"]).ToString("F5");
                text_bidQuantity07.Text = ((float)bid[7]["quantity"]).ToString("F5");
                text_bidQuantity08.Text = ((float)bid[8]["quantity"]).ToString("F5");
                text_bidQuantity09.Text = ((float)bid[9]["quantity"]).ToString("F5");
                text_bidQuantity10.Text = ((float)bid[10]["quantity"]).ToString("F5");
                text_bidQuantity11.Text = ((float)bid[11]["quantity"]).ToString("F5");
                text_bidQuantity12.Text = ((float)bid[12]["quantity"]).ToString("F5");
                text_bidQuantity13.Text = ((float)bid[13]["quantity"]).ToString("F5");
                text_bidQuantity14.Text = ((float)bid[14]["quantity"]).ToString("F5");

                text_bidQuantity00.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[0]["quantity"] / THMax));
                text_bidQuantity01.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[1]["quantity"] / THMax));
                text_bidQuantity02.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[2]["quantity"] / THMax));
                text_bidQuantity03.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[3]["quantity"] / THMax));
                text_bidQuantity04.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[4]["quantity"] / THMax));
                text_bidQuantity05.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[5]["quantity"] / THMax));
                text_bidQuantity06.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[6]["quantity"] / THMax));
                text_bidQuantity07.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[7]["quantity"] / THMax));
                text_bidQuantity08.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[8]["quantity"] / THMax));
                text_bidQuantity09.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[9]["quantity"] / THMax));
                text_bidQuantity10.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[10]["quantity"] / THMax));
                text_bidQuantity11.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[11]["quantity"] / THMax));
                text_bidQuantity12.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[12]["quantity"] / THMax));
                text_bidQuantity13.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[13]["quantity"] / THMax));
                text_bidQuantity14.BackColor = Color.FromArgb(0, 0, 55 + (int)(200 * (float)bid[14]["quantity"] / THMax));
            }
        }
        private void refreshTrade()
        {
            if (needTradeUpdate)
            {
                needTradeUpdate = false;

                isBuy = true;
                isPlace = true;

                setDefaultTrade(isBuy, isPlace);

                but_buy.BackColor = Color.Red;

                canTradeSet = true;
            }
        }


        private void list_coinName_MouseUp(object sender, MouseEventArgs e)
        {
            lock (lock_select)
            {
                if (list_coinName.SelectedIndex < 0 || list_coinName.SelectedIndex > list_coinName.Items.Count - 1)
                {
                    list_coinName.SelectedIndex = -1;
                    selectedName = "";

                    selected = false;
                    needTradeInit = false;
                    canTradeSet = false;
                }
                else
                {
                    selectedName = list_coinName.SelectedItem.ToString();

                    selected = true;
                    needTradeInit = true;
                    canTradeSet = false;
                }
            }

            text_trade_price.ReadOnly = true;
            text_trade_units.ReadOnly = true;
            text_trade_total.ReadOnly = true;
            text_trade_units.ForeColor = Color.DarkGray;
            text_trade_total.ForeColor = Color.DarkGray;
            text_trade_krw.Text = "";
            text_trade_price.Text = "";
            text_trade_units.Text = "";
            text_trade_total.Text = "";
            but_buy.BackColor = Color.DarkGray;
            but_sell.BackColor = Color.DarkGray;
            but_place.BackColor = Color.DarkGray;
            but_market.BackColor = Color.DarkGray;
        }
        private void setDefaultTrade(bool isBuy, bool isPlace)
        {
            this.isBuy = isBuy;
            this.isPlace = isPlace;

            krw = vbalanceData[2];
            currency = vbalanceData[5];
            price = vbalanceData[6];
            units = 0;
            total = 0;

            if (isBuy)
            {
                text_trade_type.Text = "KRW :";
                text_trade_krw.Text = krw.ToString(",0.####");
                but_buy.BackColor = Color.Red;
                but_sell.BackColor = Color.DarkGray;
            }
            else
            {
                text_trade_type.Text = selectedName + " :";
                text_trade_krw.Text = currency.ToString(",0.####");
                but_buy.BackColor = Color.DarkGray;
                but_sell.BackColor = Color.DodgerBlue;
            }

            if (isPlace)
            {
                text_trade_price.Text = price.ToString(",0");
                text_trade_price.ReadOnly = false;
                text_trade_price.BackColor = Color.Black;
                but_place.BackColor = Color.ForestGreen;
                but_market.BackColor = Color.DarkGray;
            }
            else
            {
                text_trade_price.Text = "";
                text_trade_price.ReadOnly = true;
                text_trade_price.BackColor = Color.DimGray;
                but_place.BackColor = Color.DarkGray;
                but_market.BackColor = Color.ForestGreen;
            }

            trackBar_price.Value = 100;
            trackBar_total.Value = 0;
            text_priceTrack_value.Text = "[   100%   ]";
            text_totalTrack_value.Text = "[   0%   ]";
            text_trade_total.Text = "Total  (1000↑)";
            if (1000f / price < 0.0001)
                text_trade_units.Text = "Units  (0.0001↑)";
            else
                text_trade_units.Text = "Units  (" + (1000f / price).ToString(",0.####") + "↑)";
            text_trade_units.ForeColor = Color.DarkGray;
            text_trade_total.ForeColor = Color.DarkGray;
        }


        private void text_trade_price_KeyUp(object sender, KeyEventArgs e)
        {
            if (!canTradeSet || !isPlace) return;
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Oemcomma || e.KeyCode == Keys.Tab) return;

            float tempPrice;
            if (float.TryParse(text_trade_price.Text, out tempPrice))
            {
                price = tempPrice;
                if (price > 1000000) price = Convert.ToInt32(price / 1000) * 1000;
                else if (price > 500000) price = Convert.ToInt32(price / 500) * 500;
                else if (price > 100000) price = Convert.ToInt32(price / 100) * 100;
                else if (price > 50000) price = Convert.ToInt32(price / 50) * 50;
                else if (price > 10000) price = Convert.ToInt32(price / 10) * 10;
                else if (price > 5000) price = Convert.ToInt32(price / 5) * 5;
                else if (price > 1000) price = (float)Math.Round(price);
                else if (price > 100) price = (float)Math.Round(price, 1);
                else if (price > 10) price = (float)Math.Round(price, 2);
                else if (price > 1) price = (float)Math.Round(price, 3);
                else price = (float)Math.Round(price, 4);

                if (isBuy && total > 0)
                {
                    units = total / price;
                    text_trade_units.Text = units.ToString(",0.####");
                }
                else if (!isBuy && units > 0)
                {
                    total = units * price;
                    text_trade_total.Text = total.ToString(",0.####");
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter) return;
                price = vbalanceData[6];
                text_trade_price.Text = price.ToString(",0.####");
                MessageBox.Show("Only can write NUMBER.");
            }
        }
        private void text_trade_price_Leave(object sender, EventArgs e)
        {
            if (!canTradeSet) return;

            if (price < vbalanceData[6] / 10)
            {
                price = vbalanceData[6];
                text_trade_price.Text = price.ToString(",0.####");
                MessageBox.Show("Low price, set more than 10%.");
            }
            else
            {
                text_trade_price.Text = price.ToString(",0.####");
            }
        }
        private void trackBar_price_Scroll(object sender, EventArgs e)
        {
            if (!canTradeSet || !isPlace) return;

            price = (vbalanceData[6] * trackBar_price.Value / 100f);
            if (price > 1000000) price = Convert.ToInt32(price / 1000) * 1000;
            else if (price > 500000) price = Convert.ToInt32(price / 500) * 500;
            else if (price > 100000) price = Convert.ToInt32(price / 100) * 100;
            else if (price > 50000) price = Convert.ToInt32(price / 50) * 50;
            else if (price > 10000) price = Convert.ToInt32(price / 10) * 10;
            else if (price > 5000) price = Convert.ToInt32(price / 5) * 5;
            else if (price > 1000) price = (float)Math.Round(price);
            else if (price > 100) price = (float)Math.Round(price, 1);
            else if (price > 10) price = (float)Math.Round(price, 2);
            else if (price > 1) price = (float)Math.Round(price, 3);
            else price = (float)Math.Round(price, 4);

            text_trade_price.Text = price.ToString();
            text_priceTrack_value.Text = "[   " + trackBar_price.Value + "%   ]";
            if (isBuy && total > 0)
            {
                units = (float)(total / price);
                text_trade_units.Text = units.ToString(",0.####");
            }
            else if (!isBuy && units > 0)
            {
                total = units * price;
                text_trade_total.Text = total.ToString(",0");
            }
        }


        private void text_trade_input_MouseUp(object sender, MouseEventArgs e)
        {
            if (!canTradeSet) return;

            ((TextBox)sender).ReadOnly = false;
            float tempTotal;
            if (!float.TryParse(((TextBox)sender).Text, out tempTotal))
                ((TextBox)sender).Text = "";
            ((TextBox)sender).ForeColor = Color.White;
        }
        private void text_trade_input_KeyUp(object sender, KeyEventArgs e)
        {
            if (!canTradeSet) return;

            float tempUnits = 0;
            float tempTotal = 0;
            bool unitTry = float.TryParse(text_trade_units.Text, out tempUnits);
            bool totalTry = float.TryParse(text_trade_total.Text, out tempTotal);
            bool test = (sender == text_trade_units) ? unitTry : totalTry;
            if (test)
            {
                units = (sender == text_trade_units) ? tempUnits : (float)(total / price);
                total = (sender == text_trade_units) ? price * units : tempTotal;
                if (sender == text_trade_units)
                {
                    text_trade_total.ForeColor = Color.White;
                    text_trade_total.Text = (price * units).ToString(",0");
                }
                else
                {
                    text_trade_units.ForeColor = Color.White;
                    text_trade_units.Text = (total / price).ToString(",0.####");
                }
            }
            else
            {
                if (e.KeyCode == Keys.Enter) return;
                units = 0;
                total = 0;
                text_trade_units.Text = "";
                text_trade_total.Text = "";
                MessageBox.Show("Only can write NUMBER.");
            }
        }
        private void text_trade_input_Leave(object sender, EventArgs e)
        {
            if (canTradeSet)
                checkUnitTotal();
        }
        private void trackBar_total_Scroll(object sender, EventArgs e)
        {
            if (!canTradeSet) return;

            if (isBuy)
            {
                total = (trackBar_total.Value == 100) ? krw : (float)(krw * trackBar_total.Value / 100d);
                if (isPlace && price > 0)
                    units = total / price;
                else if (!isPlace && vbalanceData[6] > 0)
                    units = total / vbalanceData[6];
            }
            else
            {
                units = (trackBar_total.Value == 100) ? currency : (float)(currency * trackBar_total.Value / 100d);
                if (isPlace && price > 0)
                    total = units * price;
                else if (!isPlace && vbalanceData[6] > 0)
                    total = units * vbalanceData[6];
            }
            text_trade_units.Text = units.ToString(",0.####");
            text_trade_total.Text = total.ToString(",0.##");
            text_trade_units.ForeColor = Color.White;
            text_trade_total.ForeColor = Color.White;
            text_totalTrack_value.Text = "[   " + trackBar_total.Value + "%   ]";
        }
        private void trackBar_total_MouseUp(object sender, MouseEventArgs e)
        {
            if (canTradeSet)
                if (!checkUnitTotal())
                    trackBar_total.Value = 0;
        }
        private bool checkUnitTotal()
        {
            float tempkrw = isBuy ? krw : currency;
            float tempTotal = isBuy ? total : units;
            if (text_trade_total.Text == "" || total == 0) return true;
            if (total < 1000
                || units < 0.0001
                || tempTotal > tempkrw)
            {
                units = 0;
                total = 0;
                text_trade_units.Text = "";
                text_trade_total.Text = "";
                MessageBox.Show("Range error.");

                return false;
            }

            return true;
        }


        private void but_buy_Click(object sender, EventArgs e)
        {
            if (canTradeSet) setDefaultTrade(true, isPlace);
        }
        private void but_sell_Click(object sender, EventArgs e)
        {
            if (canTradeSet) setDefaultTrade(false, isPlace);
        }
        private void but_place_Click(object sender, EventArgs e)
        {
            if (canTradeSet) setDefaultTrade(isBuy, true);
        }
        private void but_market_Click(object sender, EventArgs e)
        {
            if (canTradeSet) setDefaultTrade(isBuy, false);
        }
        private void but_execute_Click(object sender, EventArgs e)
        {
            if (!canTradeSet) return;

            string type = isBuy ? "Buy" : "Sell";
            string how = isPlace ? "Place " : "Market ";
            string tempPrice = isPlace ? price.ToString(",0.####") : "Market Price";
            DialogResult test = MessageBox.Show(
                "Name : " + selectedName + Environment.NewLine
                + "Type : " + how + type + Environment.NewLine
                + "Unit : " + units.ToString("0.####") + Environment.NewLine
                + "Price : " + tempPrice + Environment.NewLine
                + "Total : " + total.ToString(",0.##") + Environment.NewLine
                , "Confirm", MessageBoxButtons.OKCancel);
            if (test != DialogResult.OK) return;


            JObject ret = isPlace ?
                react.executeDeal(isBuy, isPlace, selectedName, units, price) :
                react.executeDeal(isBuy, isPlace, selectedName, units);

            if (ret == null)
            {
                MessageBox.Show("API error, try again.");
                return;
            }
            if (ret["status"].ToString() != "0000")
            {
                MessageBox.Show(ret.ToString());
                return;
            }
            if (ret.ContainsKey("message"))
            {
                MessageBox.Show(ret.ToString());
                return;
            }

            lock (((Main)Owner).lock_tradeHistory)
            {
                TradeData tempData = new TradeData();
                tempData.id = ulong.Parse(ret["order_id"].ToString().Substring(1));
                tempData.date = DateTime.Now;
                tempData.coinName = selectedName;
                tempData.isBid = isBuy;
                tempData.unit = units;
                tempData.price = isPlace ? price : 0;
                tempData.fee = 0;
                ((Main)Owner).tradeHistory.addNewPending(tempData);
                ((Main)Owner).tradeHistory.saveFile();
            }
            canTradeSet = false;
            needTradeInit = true;
            ((Main)Owner).logIn(new output(0, "Trade execution", selectedName + ", " + how + type + ", " + units + ", " + tempPrice));
            MessageBox.Show("Success!");
        }
    }
}
