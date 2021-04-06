using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace BithumbDealer.src
{
    public struct output
    {
        public int level;
        public string title;
        public string str;

        public output(int level, string title, string str)
        {
            this.level = level;
            this.title = title;
            this.str = str;
        }
    }
    public struct Index
    {
        public float index;
        public float width;
        public float rate;

        public Index(float index = 0, float width = 0, float rate = 0)
        {
            this.index = index;
            this.width = width;
            this.rate = rate;
        }

        public Index(JObject jObject)
        {
            this.index = float.Parse((string)jObject["market_index"]);
            this.width = float.Parse((string)jObject["width"]);
            this.rate = float.Parse((string)jObject["rate"]);
        }
    }
    public struct TradeData
    {
        public ulong id;
        public DateTime date;
        public string coinName;
        public bool isBid;
        public float unit;
        public float price;
        public float fee;
    }



    public class MainUpdater
    {
        private ApiData apiData;
        private Dictionary<string, string> apiParameter = new Dictionary<string, string>();

        public Index btmiIndex;
        public Index btaiIndex;
        public DataTable holdList = new DataTable();
        private DataTable balanceData = new DataTable();


        public MainUpdater(string sAPI_Key, string sAPI_Secret)
        {
            apiData = new ApiData(sAPI_Key, sAPI_Secret);
            apiParameter.Add("currency", "ALL");

            holdList.Columns.Add("name", typeof(string));
            holdList.Columns.Add("total", typeof(float));
            holdList.Columns.Add("locked", typeof(float));
            holdList.Columns.Add("balance", typeof(float));
            holdList.Columns.Add("last", typeof(float));

            balanceData.Columns.Add("name", typeof(string));
            balanceData.Columns.Add("total", typeof(float));
            balanceData.Columns.Add("inUse", typeof(float));
            balanceData.Columns.Add("available", typeof(float));
            balanceData.Columns.Add("last", typeof(float));
        }

        public List<string> getUpdateList()
        {
            List<string> retVal = new List<string>();

            JObject retData = apiData.privateInfo(c.vBALANCE, apiParameter);
            if (retData == null) return retVal;
            if ((string)retData["status"] != "0000") return retVal;
            retData = (JObject)retData["data"];
            string[] strList = (retData.ToString()).Split('\n');
            strList[strList.Length - 2] += ',';


            for (int i = 1; i < strList.Length - 1; i++)
            {
                string[] tempList = strList[i].Split(':');
                tempList[0] = tempList[0].Substring(3, tempList[0].Length - 4);
                tempList[1] = tempList[1].Substring(2, tempList[1].Length - 5);

                float value = float.Parse(tempList[1]);
                if (value == 0) continue;
                string[] indexName = tempList[0].Split('_');
                if (indexName[0].StartsWith("xcoin"))
                    retVal.Add(indexName[2].ToUpper());
                else continue;
            }

            return retVal;
        }


        public int update()
        {
            updateBtmi();
            return updateHoldList();
        }
        private int updateBtmi()
        {
            JObject retData = apiData.publicInfo(c.bBTCI);
            if (retData == null) return -1;
            if ((string)retData["status"] != "0000") return -1;
            if (retData.ContainsKey("message")) return -1;

            retData = (JObject)retData["data"];
            btmiIndex = new Index((JObject)retData["btmi"]);
            btaiIndex = new Index((JObject)retData["btai"]);

            return 0;
        }
        private int updateHoldList()
        {
            JObject retData = apiData.privateInfo(c.vBALANCE, apiParameter);
            if (retData == null) return -1;
            if ((string)retData["status"] != "0000") return -1;
            if (retData.ContainsKey("message")) return -1;

            retData = (JObject)retData["data"];
            string[] strList = (retData.ToString()).Split('\n');
            strList[strList.Length - 2] += ',';

            for (int i = 1; i < strList.Length - 1; i++)
            {
                string[] tempList = strList[i].Split(':');
                tempList[0] = tempList[0].Substring(3, tempList[0].Length - 4);
                tempList[1] = tempList[1].Substring(2, tempList[1].Length - 5);

                float value = float.Parse(tempList[1]);
                if (value == 0) continue;
                string[] indexName = tempList[0].Split('_');
                string name;
                int type;
                if (indexName[0].StartsWith("total"))
                {
                    name = indexName[1].ToUpper();
                    type = 0;
                }
                else if (indexName[0].StartsWith("in"))
                {
                    name = indexName[2].ToUpper();
                    type = 1;
                }
                else if (indexName[0].StartsWith("available"))
                {
                    name = indexName[1].ToUpper();
                    type = 2;
                }
                else if (indexName[0].StartsWith("xcoin"))
                {
                    name = indexName[2].ToUpper();
                    type = 3;
                }
                else continue;

                bool isExist = false;
                for (int j = 0; j < balanceData.Rows.Count; j++)
                {
                    if (name == (string)balanceData.Rows[j]["name"])
                    {
                        switch (type)
                        {
                            case 0: balanceData.Rows[j]["total"] = value; break;
                            case 1: balanceData.Rows[j]["inUse"] = value; break;
                            case 2: balanceData.Rows[j]["available"] = value; break;
                            case 3: balanceData.Rows[j]["last"] = value; break;
                        }
                        isExist = true;
                        break;
                    }
                }
                if (!isExist)
                {
                    DataRow dataRow = balanceData.NewRow();
                    dataRow["name"] = name;
                    dataRow["total"] = 0;
                    dataRow["inUse"] = 0;
                    dataRow["available"] = 0;
                    dataRow["last"] = 1;
                    switch (type)
                    {
                        case 0: dataRow["total"] = value; break;
                        case 1: dataRow["inUse"] = value; break;
                        case 2: dataRow["available"] = value; break;
                        case 3: dataRow["last"] = value; break;
                    }
                    balanceData.Rows.Add(dataRow);
                }
            }

            holdList.Clear();
            for (int i = 0; i < balanceData.Rows.Count; i++)
                if (Convert.ToDouble(balanceData.Rows[i]["total"]) > 0)
                {
                    DataRow dataRow = holdList.NewRow();
                    dataRow["name"] = balanceData.Rows[i]["name"];
                    dataRow["total"] = balanceData.Rows[i]["total"];
                    dataRow["locked"] = balanceData.Rows[i]["inUse"];
                    dataRow["balance"] = balanceData.Rows[i]["available"];
                    dataRow["last"] = balanceData.Rows[i]["last"];
                    holdList.Rows.Add(dataRow);
                }
            balanceData.Rows.Clear();

            return 0;
        }
    }



    // for 'trader' form trade and result
    public class React
    {
        private ApiData apiData;

        private Dictionary<string, string> parTrans = new Dictionary<string, string>();
        private Dictionary<string, string> parOrderBook = new Dictionary<string, string>();
        private Dictionary<string, string> parBalance = new Dictionary<string, string>();


        public React(string sAPI_Key, string sAPI_Secret)
        {
            apiData = new ApiData(sAPI_Key, sAPI_Secret);

            parTrans.Add("count", "5");
            parOrderBook.Add("group_orders", "0");
            parBalance.Add("currency", "");
        }


        public float[] getTickerData(string coinName)
        {
            float[] retTickerData = null;
            JObject retData = apiData.publicInfo(c.bTICKER, coinName);
            if (retData != null)
            {
                if (retData["status"].ToString() == "0000")
                {
                    retData = (JObject)retData["data"];
                    retTickerData = new float[11];
                    retTickerData[0] = (float)retData["opening_price"];
                    retTickerData[1] = (float)retData["closing_price"];
                    retTickerData[2] = (float)retData["min_price"];
                    retTickerData[3] = (float)retData["max_price"];
                    retTickerData[4] = (float)retData["units_traded"];
                    retTickerData[5] = (float)retData["acc_trade_value"];
                    retTickerData[6] = (float)retData["prev_closing_price"];
                    retTickerData[7] = (float)retData["units_traded_24H"];
                    retTickerData[8] = (float)retData["acc_trade_value_24H"];
                    retTickerData[9] = (float)retData["fluctate_24H"];
                    retTickerData[10] = (float)retData["fluctate_rate_24H"];
                }
                else retTickerData = null;
            }
            else retTickerData = null;

            return retTickerData;
        }
        public JArray getTransactionData(string coinName)
        {
            JArray retTHData = null;
            JObject retData = apiData.publicInfo(c.bTRANSACTION_HISTORY, coinName, parTrans);
            if (retData != null)
            {
                if (retData["status"].ToString() == "0000") retTHData = (JArray)retData["data"];
                else retTHData = null;
            }
            else retTHData = null;

            return retTHData;
        }
        public JArray[] getOrderBookData(string coinName)
        {
            JArray[] retOBData = null;
            JObject retData = apiData.publicInfo(c.bORDERBOOK, coinName);
            if (retData != null)
            {
                if (retData["status"].ToString() == "0000")
                {
                    retOBData = new JArray[2];
                    retOBData[0] = (JArray)retData["data"]["bids"];
                    retOBData[1] = (JArray)retData["data"]["asks"];
                }
                else retOBData = null;
            }
            else retOBData = null;

            return retOBData;
        }
        public float[] getBalanceData(string coinName)
        {
            parBalance["currency"] = coinName;
            float[] retBalanceData;
            JObject retData = apiData.privateInfo(c.vBALANCE, parBalance);

            if (retData == null) return null;
            if (retData["status"].ToString() != "0000") return null;
            if (retData.ContainsKey("message")) return null;

            retData = (JObject)retData["data"];
            retBalanceData = new float[7];
            retBalanceData[0] = (float)retData["total_krw"];
            retBalanceData[1] = (float)retData["in_use_krw"];
            retBalanceData[2] = (float)retData["available_krw"];
            retBalanceData[3] = (float)retData["total_" + coinName.ToLower()];
            retBalanceData[4] = (float)retData["in_use_" + coinName.ToLower()];
            retBalanceData[5] = (float)retData["available_" + coinName.ToLower()];
            retBalanceData[6] = (float)retData["xcoin_last_" + coinName.ToLower()];

            return retBalanceData;
        }


        public JObject executeDeal
                (bool isBuy, bool isPlace, string coinName, float units, float price = 0)
        {
            Dictionary<string, string> par = new Dictionary<string, string>();
            par.Add("order_currency", coinName);
            par.Add("payment_currency", "KRW");
            par.Add("units", units.ToString("0.####"));

            if (isPlace)
            {
                par.Add("price", price.ToString("0.####"));
                if (isBuy)
                {
                    par.Add("type", "bid");
                    return apiData.privateTrade(c.vPLACE, par);
                }
                else
                {
                    par.Add("type", "ask");
                    return apiData.privateTrade(c.vPLACE, par);
                }
            }
            else
            {
                if (isBuy)
                    return apiData.privateTrade(c.vMARKET_BUY, par);
                else
                    return apiData.privateTrade(c.vMARKET_SELL, par);
            }
        }
    }


    public class TradeHistory
    {
        private CultureInfo provider = CultureInfo.InvariantCulture;
        private ApiData apiData;

        private Dictionary<string, string> parOrderDetail = new Dictionary<string, string>();
        private Dictionary<string, string> parCancel = new Dictionary<string, string>();

        public DataTable pendingData = new DataTable();
        public DataTable historyData = new DataTable();

        public List<output> executionStr = new List<output>();


        public TradeHistory(string sAPI_Key, string sAPI_Secret)
        {
            apiData = new ApiData(sAPI_Key, sAPI_Secret);

            pendingData.Columns.Add("id", typeof(ulong));
            pendingData.Columns.Add("date", typeof(DateTime));
            pendingData.Columns.Add("coinName", typeof(string));
            pendingData.Columns.Add("isBid", typeof(bool));
            pendingData.Columns.Add("unit", typeof(float));
            pendingData.Columns.Add("price", typeof(int));
            pendingData.Columns.Add("fee", typeof(float));

            historyData.Columns.Add("date", typeof(DateTime));
            historyData.Columns.Add("coinName", typeof(string));
            historyData.Columns.Add("isBid", typeof(bool));
            historyData.Columns.Add("unit", typeof(float));
            historyData.Columns.Add("price", typeof(float));
            historyData.Columns.Add("fee", typeof(float));

            parOrderDetail.Add("order_id", "");
            parOrderDetail.Add("order_currency", "");
            parOrderDetail.Add("payment_currency", "KRW");

            parCancel.Add("type", "");
            parCancel.Add("order_id", "");
            parCancel.Add("order_currency", "");
            parCancel.Add("payment_currency", "KRW");
        }
        public int loadFile()
        {
            string tradePath = System.IO.Directory.GetCurrentDirectory() + "/pending.dat";
            try
            {
                if (System.IO.File.Exists(tradePath))
                {
                    string[] reader = System.IO.File.ReadAllLines(tradePath);
                    if (reader.Length > 0)
                        for (int i = 0; i < reader.Length; i++)
                        {
                            string[] tempLine = reader[i].Split('\t');
                            TradeData tempData = new TradeData();
                            tempData.id = ulong.Parse(tempLine[0]);
                            tempData.date = DateTime.ParseExact(tempLine[1], "u", provider);
                            tempData.coinName = tempLine[2];
                            tempData.isBid = bool.Parse(tempLine[3]);
                            tempData.unit = float.Parse(tempLine[4]);
                            tempData.price = int.Parse(tempLine[5]);
                            tempData.fee = tempLine[6] == "" ? 0 : float.Parse(tempLine[6]);
                            addNewPending(tempData);
                        }
                }
                else System.IO.File.Create(tradePath);
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Trade Execution", "Fail to load pending data (" + ex.Message + ")"));
                return -1;
            }

            return 0;
        }
        public int saveFile()
        {
            string tradePath = System.IO.Directory.GetCurrentDirectory() + "/pending.dat";
            try
            {
                if (!System.IO.File.Exists(tradePath)) System.IO.File.Create(tradePath);
                if (pendingData.Rows.Count == 0) System.IO.File.WriteAllText(tradePath, "");
                else
                {
                    List<string> savingList = new List<string>();
                    for (int i = 0; i < pendingData.Rows.Count; i++)
                    {
                        string temp
                            = pendingData.Rows[i][0].ToString() + '\t'
                            + ((DateTime)pendingData.Rows[i][1]).ToString("u") + '\t'
                            + pendingData.Rows[i][2].ToString() + '\t'
                            + pendingData.Rows[i][3].ToString() + '\t'
                            + pendingData.Rows[i][4].ToString() + '\t'
                            + pendingData.Rows[i][5].ToString() + '\t'
                            + pendingData.Rows[i][6].ToString();
                        savingList.Add(temp);
                    }
                    System.IO.File.WriteAllText(tradePath, string.Join("\n", savingList) + "\n");
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Trade Execution", "Fail to save pending data (" + ex.Message + ")"));
                return -1;
            }

            return 0;
        }


        public void addNewPending(TradeData tradeData)
        {
            DataRow row = pendingData.NewRow();
            row["id"] = tradeData.id;
            row["date"] = tradeData.date;
            row["coinName"] = tradeData.coinName;
            row["isBid"] = tradeData.isBid;
            row["unit"] = tradeData.unit;
            row["price"] = tradeData.price;
            pendingData.Rows.Add(row);
        }


        public int cancelPending(bool isBid, ulong id, string coinName)
        {
            parCancel["type"] = isBid ? "bid" : "ask";
            parCancel["order_id"] = "C" + id.ToString("D19");
            parCancel["order_currency"] = coinName;
            JObject jObject = apiData.privateTrade(c.vCANCEL, parCancel);
            if (jObject == null)
            {
                executionStr.Add(new output(0, "Cancel Execution",
                    "Fail to cancel pending (NULL)"));
                return -1;
            }
            if (jObject["status"].ToString() != "0000")
            {
                executionStr.Add(new output(0, "Cancel Execution",
                    "Fail to cancel pending (" + jObject["status"].ToString() + ")"));
                return -2;
            }
            if (jObject.ContainsKey("message"))
            {
                executionStr.Add(new output(0, "Cancel Execution",
                    "Fail to cancel pending (" + jObject["message"].ToString() + ")"));
                return -2;
            }

            executionStr.Add(new output(0, "Cancel Execution",
                "Success to cancel pending (" + coinName + ")"));

            return 0;
        }
        public int updateSinglePendingData(int index)
        {
            parOrderDetail["order_id"] = "C" + ((ulong)pendingData.Rows[index]["id"]).ToString("D19");
            parOrderDetail["order_currency"] = pendingData.Rows[index]["coinName"].ToString();
            JObject ret = apiData.privateInfo(c.vORDERS_DETAIL, parOrderDetail);
            if (ret == null) return -2;
            if (ret["status"].ToString() != "0000") return -2;
            if (ret.ContainsKey("message")) return -2;
            if (ret["data"]["order_status"].ToString() == "Completed")
            {
                float unit = 0;
                float price = 0;
                float fee = 0;
                JArray jArray = (JArray)ret["data"]["contract"];
                for (int i = 0; i < jArray.Count; i++)
                {
                    unit += (float)jArray[i]["units"];
                    price += (float)jArray[i]["price"] * (float)jArray[i]["units"];
                    fee += (float)jArray[i]["fee"];
                }
                price /= unit;

                if ((bool)pendingData.Rows[index]["isBid"])
                    executionStr.Add(new output(1, "Trade Execution",
                        pendingData.Rows[index]["coinName"]
                        + " buy " + unit.ToString("0.####") + " " + pendingData.Rows[index]["coinName"]
                        + " for " + (unit * price).ToString("0.##") + " KRW"));

                else
                    executionStr.Add(new output(1, "Trade Execution",
                        pendingData.Rows[index]["coinName"]
                        + " sell (" + unit.ToString("0.####") + " " + pendingData.Rows[index]["coinName"]
                        + " for " + (unit * price).ToString("0.##") + " KRW"));

                pendingData.Rows.RemoveAt(index);
                return 1;
            }
            else if (ret["data"]["order_status"].ToString() == "Cancel")
            {
                pendingData.Rows.RemoveAt(index);
                return 1;
            }

            return 0;
        }


        public int updateHistoryData(string coinName, int offset)
        {
            Dictionary<string, string> parOrder = new Dictionary<string, string>();
            parOrder.Add("offset", offset.ToString());
            parOrder.Add("count", "50");
            parOrder.Add("searchGb", "1");
            parOrder.Add("order_currency", coinName);
            parOrder.Add("payment_currency", "KRW");

            JObject jObject = apiData.privateInfo(c.vUSER_TRANS, parOrder);
            if (jObject == null) return -1;
            if (jObject["status"].ToString() != "0000") return -2;
            if (jObject.ContainsKey("message")) return -2;
            JArray jArray = (JArray)jObject["data"];

            historyData.Rows.Clear();
            for (int i = 0; i < jArray.Count; i++)
            {
                DataRow dataRow = historyData.NewRow();
                dataRow["date"] = new DateTime(1970, 1, 1).AddSeconds(double.Parse(jArray[i]["transfer_date"].ToString()) / 1000000);
                dataRow["coinName"] = coinName;
                dataRow["isBid"] = true;
                dataRow["unit"] = (float)jArray[i]["units"];
                dataRow["price"] = (float)jArray[i]["price"];
                dataRow["fee"] = (float)jArray[i]["fee"];
                historyData.Rows.Add(dataRow);
            }

            parOrder["searchGb"] = "2";
            jObject = apiData.privateInfo(c.vUSER_TRANS, parOrder);
            if (jObject == null) return -1;
            if (jObject["status"].ToString() != "0000") return -2;
            if (jObject.ContainsKey("message")) return -2;
            jArray = (JArray)jObject["data"];

            for (int i = 0; i < jArray.Count; i++)
            {
                DataRow dataRow = historyData.NewRow();
                dataRow["date"] = new DateTime(1970, 1, 1).AddSeconds(double.Parse(jArray[i]["transfer_date"].ToString()) / 1000000);
                dataRow["coinName"] = coinName;
                dataRow["isBid"] = false;
                dataRow["unit"] = (float)jArray[i]["units"];
                dataRow["price"] = (int)jArray[i]["price"];
                dataRow["fee"] = (float)jArray[i]["fee"];
                historyData.Rows.Add(dataRow);
            }

            return 1;
        }
    }
}
