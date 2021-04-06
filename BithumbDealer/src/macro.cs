using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace BithumbDealer.src
{
    public struct MacroSettingData
    {
        public string coinName;
        public float yield;
        public float krw;
        public float time;
        public float hour24_from;
        public float hour24_to;
        public float hour12_from;
        public float hour12_to;
        public float hour6_from;
        public float hour6_to;
        public float hour1_from;
        public float hour1_to;
        public float min30_from;
        public float min30_to;

        public bool hour24_bias;
        public bool hour12_bias;
        public bool hour6_bias;
        public bool hour1_bias;
        public bool min30_bias;
    }
    public class BollingerAverage
    {
        public float total = 0;
        public float count = 0;
        public float avg = 0;


        public void addTotal(float val, float weight)
        {
            total += (val * weight);
            count += weight;
        }
        public double setAverage()
        {
            avg = count < 1 ? 0 : total / count;
            total = 0;
            count = 0;
            return avg;
        }
    }



    public class MacroSetting
    {
        private CultureInfo provider = CultureInfo.InvariantCulture;
        private ApiData apiData;

        public MacroSettingData setting = new MacroSettingData();
        public DataSet state = new DataSet();
        public DataTable order = new DataTable();

        private List<string> coinList = new List<string>();
        private List<string> errorList = new List<string>();
        public List<output> executionStr = new List<output>();

        private float holdKRW;
        private DataTable lastCandleUpdate = new DataTable();
        private DataSet min30_candle = new DataSet();
        private DataSet hour1_candle = new DataSet();
        private DataSet hour6_candle = new DataSet();
        private DataSet hour12_candle = new DataSet();
        private DataSet hour24_candle = new DataSet();

        public List<BollingerAverage> ba0 = new List<BollingerAverage>();
        public List<BollingerAverage> ba1 = new List<BollingerAverage>();
        public List<BollingerAverage> ba2 = new List<BollingerAverage>();

        public List<BollingerAverage> bb0 = new List<BollingerAverage>();
        public List<BollingerAverage> bb1 = new List<BollingerAverage>();
        public List<BollingerAverage> bb2 = new List<BollingerAverage>();



        public MacroSetting(string sAPI_Key, string sAPI_Secret, List<string> coinList)
        {
            apiData = new ApiData(sAPI_Key, sAPI_Secret);

            lastCandleUpdate.Columns.Add("hour24", typeof(DateTime));
            lastCandleUpdate.Columns.Add("hour12", typeof(DateTime));
            lastCandleUpdate.Columns.Add("hour6", typeof(DateTime));
            lastCandleUpdate.Columns.Add("hour1", typeof(DateTime));
            lastCandleUpdate.Columns.Add("min30", typeof(DateTime));

            order.Columns.Add("coinName", typeof(string));
            order.Columns.Add("id", typeof(ulong));
            order.Columns.Add("target_id", typeof(ulong));

            for (int i = 0; i < 5; i++)
            {
                ba0.Add(new BollingerAverage());
                ba1.Add(new BollingerAverage());
                ba2.Add(new BollingerAverage());
                bb0.Add(new BollingerAverage());
                bb1.Add(new BollingerAverage());
                bb2.Add(new BollingerAverage());
            }

            this.coinList = coinList;
            for (int i = 0; i < coinList.Count; i++)
            {
                DataRow dataRow = lastCandleUpdate.NewRow();
                dataRow["hour24"] = DateTime.Now.AddMonths(-1);
                dataRow["hour12"] = DateTime.Now.AddMonths(-1);
                dataRow["hour6"] = DateTime.Now.AddMonths(-1);
                dataRow["hour1"] = DateTime.Now.AddMonths(-1);
                dataRow["min30"] = DateTime.Now.AddMonths(-1);
                lastCandleUpdate.Rows.Add(dataRow);

                DataTable hour24_candle_single = new DataTable(coinList[i]);
                hour24_candle_single.Columns.Add("date", typeof(DateTime));
                hour24_candle_single.Columns.Add("open", typeof(float));
                hour24_candle_single.Columns.Add("close", typeof(float));
                hour24_candle_single.Columns.Add("max", typeof(float));
                hour24_candle_single.Columns.Add("min", typeof(float));
                hour24_candle_single.Columns.Add("volume", typeof(float));
                hour24_candle.Tables.Add(hour24_candle_single);

                DataTable hour12_candle_single = new DataTable(coinList[i]);
                hour12_candle_single.Columns.Add("date", typeof(DateTime));
                hour12_candle_single.Columns.Add("open", typeof(float));
                hour12_candle_single.Columns.Add("close", typeof(float));
                hour12_candle_single.Columns.Add("max", typeof(float));
                hour12_candle_single.Columns.Add("min", typeof(float));
                hour12_candle_single.Columns.Add("volume", typeof(float));
                hour12_candle.Tables.Add(hour12_candle_single);

                DataTable hour6_candle_single = new DataTable(coinList[i]);
                hour6_candle_single.Columns.Add("date", typeof(DateTime));
                hour6_candle_single.Columns.Add("open", typeof(float));
                hour6_candle_single.Columns.Add("close", typeof(float));
                hour6_candle_single.Columns.Add("max", typeof(float));
                hour6_candle_single.Columns.Add("min", typeof(float));
                hour6_candle_single.Columns.Add("volume", typeof(float));
                hour6_candle.Tables.Add(hour6_candle_single);

                DataTable hour1_candle_single = new DataTable(coinList[i]);
                hour1_candle_single.Columns.Add("date", typeof(DateTime));
                hour1_candle_single.Columns.Add("open", typeof(float));
                hour1_candle_single.Columns.Add("close", typeof(float));
                hour1_candle_single.Columns.Add("max", typeof(float));
                hour1_candle_single.Columns.Add("min", typeof(float));
                hour1_candle_single.Columns.Add("volume", typeof(float));
                hour1_candle.Tables.Add(hour1_candle_single);

                DataTable min30_candle_single = new DataTable(coinList[i]);
                min30_candle_single.Columns.Add("date", typeof(DateTime));
                min30_candle_single.Columns.Add("open", typeof(float));
                min30_candle_single.Columns.Add("close", typeof(float));
                min30_candle_single.Columns.Add("max", typeof(float));
                min30_candle_single.Columns.Add("min", typeof(float));
                min30_candle_single.Columns.Add("volume", typeof(float));
                min30_candle.Tables.Add(min30_candle_single);


                DataTable state_table = new DataTable(coinList[i]);
                state_table.Columns.Add("id", typeof(ulong));
                state_table.Columns.Add("date", typeof(DateTime));
                state_table.Columns.Add("unit", typeof(float));
                state_table.Columns.Add("price", typeof(float));
                state_table.Columns.Add("krw", typeof(float));
                state.Tables.Add(state_table);
            }
        }
        public int loadFile()
        {
            string macroSettingDataPath = System.IO.Directory.GetCurrentDirectory() + "/macroSetting.dat";
            string macroStateDataPath = System.IO.Directory.GetCurrentDirectory() + "/macroState.dat";
            string macroOrderDataPath = System.IO.Directory.GetCurrentDirectory() + "/macroOrder.dat";

            try
            {
                if (!System.IO.File.Exists(macroSettingDataPath))
                {
                    setDefaultSetting();
                    System.IO.File.Create(macroSettingDataPath);
                }
                else
                {
                    string[] reader = System.IO.File.ReadAllLines(macroSettingDataPath);
                    if (reader.Length < 1) setDefaultSetting();
                    else
                    {
                        string[] singleData = reader[0].Split('\t');
                        if (singleData.Length < 18) setDefaultSetting();
                        else
                        {
                            setting.yield = float.Parse(singleData[0]);
                            setting.krw = float.Parse(singleData[1]);
                            setting.time = float.Parse(singleData[2]);
                            setting.hour24_from = float.Parse(singleData[3]);
                            setting.hour24_to = float.Parse(singleData[4]);
                            setting.hour12_from = float.Parse(singleData[5]);
                            setting.hour12_to = float.Parse(singleData[6]);
                            setting.hour6_from = float.Parse(singleData[7]);
                            setting.hour6_to = float.Parse(singleData[8]);
                            setting.hour1_from = float.Parse(singleData[9]);
                            setting.hour1_to = float.Parse(singleData[10]);
                            setting.min30_from = float.Parse(singleData[11]);
                            setting.min30_to = float.Parse(singleData[12]);

                            setting.hour24_bias = bool.Parse(singleData[13]);
                            setting.hour12_bias = bool.Parse(singleData[14]);
                            setting.hour6_bias = bool.Parse(singleData[15]);
                            setting.hour1_bias = bool.Parse(singleData[16]);
                            setting.min30_bias = bool.Parse(singleData[17]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Macro Execution", "Fail to load macro setting (" + ex.Message + ")"));
                return -1;
            }
            try
            {
                if (!System.IO.File.Exists(macroStateDataPath)) System.IO.File.Create(macroStateDataPath);
                else
                {
                    string[] reader = System.IO.File.ReadAllLines(macroStateDataPath);
                    for (int i = 0; i < reader.Length; i++)
                    {
                        string[] singleData = reader[i].Split('\t');
                        if (singleData.Length < 6) continue;
                        if (!state.Tables.Contains(singleData[0])) continue;

                        DataRow tempRow = state.Tables[singleData[0]].NewRow();
                        tempRow["id"] = ulong.Parse(singleData[1]);
                        tempRow["date"] = DateTime.ParseExact(singleData[2], "u", provider);
                        tempRow["unit"] = float.Parse(singleData[3]);
                        tempRow["price"] = float.Parse(singleData[4]);
                        tempRow["krw"] = float.Parse(singleData[5]);
                        state.Tables[singleData[0]].Rows.Add(tempRow);
                    }
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Macro Execution", "Fail to load macro state (" + ex.Message + ")"));
                return -2;
            }
            try
            {
                if (!System.IO.File.Exists(macroOrderDataPath)) System.IO.File.Create(macroOrderDataPath);
                else
                {
                    string[] reader = System.IO.File.ReadAllLines(macroOrderDataPath);
                    for (int i = 0; i < reader.Length; i++)
                    {
                        string[] singleData = reader[i].Split('\t');
                        if (singleData.Length < 3) continue;

                        DataRow tempRow = order.NewRow();
                        tempRow["coinName"] = singleData[0];
                        tempRow["id"] = ulong.Parse(singleData[1]);
                        tempRow["target_id"] = ulong.Parse(singleData[2]);
                        order.Rows.Add(tempRow);
                    }
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Macro Execution", "Fail to load macro order (" + ex.Message + ")"));
                return -3;
            }

            return 0;
        }
        public int saveFile()
        {
            string macroSettingDataPath = System.IO.Directory.GetCurrentDirectory() + "/macroSetting.dat";
            string macroStateDataPath = System.IO.Directory.GetCurrentDirectory() + "/macroState.dat";
            string macroOrderDataPath = System.IO.Directory.GetCurrentDirectory() + "/macroOrder.dat";

            try
            {
                if (!System.IO.File.Exists(macroSettingDataPath)) System.IO.File.Create(macroSettingDataPath);
                else
                {
                    string tempStr
                        = setting.yield.ToString("0.########") + '\t'
                        + setting.krw.ToString("0.########") + '\t'
                        + setting.time.ToString("0.########") + '\t'
                        + setting.hour24_from.ToString("0.########") + '\t'
                        + setting.hour24_to.ToString("0.########") + '\t'
                        + setting.hour12_from.ToString("0.########") + '\t'
                        + setting.hour12_to.ToString("0.########") + '\t'
                        + setting.hour6_from.ToString("0.########") + '\t'
                        + setting.hour6_to.ToString("0.########") + '\t'
                        + setting.hour1_from.ToString("0.########") + '\t'
                        + setting.hour1_to.ToString("0.########") + '\t'
                        + setting.min30_from.ToString("0.########") + '\t'
                        + setting.min30_to.ToString("0.########") + '\t'
                        + setting.hour24_bias.ToString() + '\t'
                        + setting.hour12_bias.ToString() + '\t'
                        + setting.hour6_bias.ToString() + '\t'
                        + setting.hour1_bias.ToString() + '\t'
                        + setting.min30_bias.ToString();
                    System.IO.File.WriteAllText(macroSettingDataPath, tempStr + "\n");
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Macro Execution", "Fail to save macro setting (" + ex.Message + ")"));
                return -1;
            }
            try
            {
                if (!System.IO.File.Exists(macroStateDataPath)) System.IO.File.Create(macroStateDataPath);
                if (state.Tables.Count == 0) System.IO.File.WriteAllText(macroStateDataPath, "");
                else
                {
                    List<string> savingList = new List<string>();
                    for (int i = 0; i < state.Tables.Count; i++)
                    {
                        for (int j = 0; j < state.Tables[i].Rows.Count; j++)
                        {
                            string tempStr
                                = state.Tables[i].TableName + '\t'
                                + state.Tables[i].Rows[j][0].ToString() + '\t'
                                + ((DateTime)state.Tables[i].Rows[j][1]).ToString("u") + '\t'
                                + state.Tables[i].Rows[j][2].ToString() + '\t'
                                + state.Tables[i].Rows[j][3].ToString() + '\t'
                                + state.Tables[i].Rows[j][4].ToString();
                            savingList.Add(tempStr);
                        }
                    }
                    System.IO.File.WriteAllText(macroStateDataPath, string.Join("\n", savingList) + '\n');
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Macro Execution", "Fail to save macro state (" + ex.Message + ")"));
                return -2;
            }
            try
            {
                if (!System.IO.File.Exists(macroOrderDataPath)) System.IO.File.Create(macroOrderDataPath);
                if (order.Rows.Count == 0) System.IO.File.WriteAllText(macroOrderDataPath, "");
                else
                {
                    List<string> savingList = new List<string>();
                    for (int i = 0; i < order.Rows.Count; i++)
                    {
                        string tempStr
                            = order.Rows[i][0].ToString() + '\t'
                            + order.Rows[i][1].ToString() + '\t'
                            + order.Rows[i][2].ToString();
                        savingList.Add(tempStr);
                    }
                    System.IO.File.WriteAllText(macroOrderDataPath, string.Join("\n", savingList) + '\n');
                }
            }
            catch (Exception ex)
            {
                executionStr.Add(new output(2, "Macro Execution", "Fail to save macro order (" + ex.Message + ")"));
                return -3;
            }

            return 0;
        }
        public void saveMacroSetting(MacroSettingData setting)
        {
            this.setting = setting;
            saveFile();
        }


        private void setDefaultSetting()
        {
            setting.yield = 1;
            setting.krw = 5000;
            setting.time = 1;
            setting.hour24_from = -100;
            setting.hour24_to = 100;
            setting.hour12_from = -100;
            setting.hour12_to = 100;
            setting.hour6_from = -100;
            setting.hour6_to = 100;
            setting.hour1_from = -100;
            setting.hour1_to = 100;
            setting.min30_from = -100;
            setting.min30_to = 100;

            setting.hour24_bias = false;
            setting.hour12_bias = false;
            setting.hour6_bias = false;
            setting.hour1_bias = false;
            setting.min30_bias = false;
        }
        public int setLastKrw(int index)
        {
            string coinName = coinList[index];
            Dictionary<string, string> par = new Dictionary<string, string>();
            par.Add("currency", coinName);
            JObject jObject = apiData.privateInfo(c.vBALANCE, par);
            if (jObject == null) return -1;
            if (jObject["status"].ToString() != "0000") return -2;
            if (jObject.ContainsKey("message")) return -3;

            holdKRW = (float)jObject["data"]["available_krw"];

            //min30_candle.Tables[coinName].Rows[0]["close"] = jObject["data"]["xcoin_last_" + coinName.ToLower()];
            //hour1_candle.Tables[coinName].Rows[0]["close"] = jObject["data"]["xcoin_last_" + coinName.ToLower()];
            //hour6_candle.Tables[coinName].Rows[0]["close"] = jObject["data"]["xcoin_last_" + coinName.ToLower()];
            //hour12_candle.Tables[coinName].Rows[0]["close"] = jObject["data"]["xcoin_last_" + coinName.ToLower()];
            //hour24_candle.Tables[coinName].Rows[0]["close"] = jObject["data"]["xcoin_last_" + coinName.ToLower()];

            return 0;
        }
        public int setCandleData(int index)
        {
            string coinName = coinList[index];
            DateTime last;
            DateTime now = DateTime.Now.AddSeconds(-20);

            Dictionary<string, string> par = new Dictionary<string, string> { { "interval", "" } };
            DateTime biasDateTime = new DateTime(1970, 1, 1, 9, 0, 0);
            JObject jObject;
            JArray jArray;


            par["interval"] = c.CANDLE_MIN30;
            jObject = apiData.publicInfo(c.bCandleStick, coinName, par);
            if (jObject == null) return -10;
            if (jObject.ContainsKey("message")) return -11;
            jArray = (JArray)jObject["data"];
            if (jArray.Count < min30_candle.Tables[coinName].Rows.Count) return -12;

            last = (DateTime)lastCandleUpdate.Rows[index]["min30"];
            if (min30_candle.Tables[coinName].Rows.Count < 1 || now.Minute % 30 < last.Minute % 30)
            {
                min30_candle.Tables[coinName].Rows.Clear();
                for (int i = jArray.Count - 1; i >= jArray.Count - 200 && i >= 0; i--)
                {
                    DataRow dataRow = min30_candle.Tables[coinName].NewRow();
                    dataRow["date"] = biasDateTime.AddSeconds(long.Parse(jArray[i][0].ToString()) / 1000);
                    dataRow["open"] = (float)jArray[i][1];
                    dataRow["close"] = (float)jArray[i][2];
                    dataRow["max"] = (float)jArray[i][3];
                    dataRow["min"] = (float)jArray[i][4];
                    dataRow["volume"] = (float)jArray[i][5];
                    min30_candle.Tables[coinName].Rows.Add(dataRow);
                }
                lastCandleUpdate.Rows[index]["min30"] = now;
            }
            else
            {
                int count = jArray.Count - 1;
                min30_candle.Tables[coinName].Rows[0]["date"] = biasDateTime.AddSeconds(long.Parse(jArray[count][0].ToString()) / 1000);
                min30_candle.Tables[coinName].Rows[0]["open"] = (float)jArray[count][1];
                min30_candle.Tables[coinName].Rows[0]["close"] = (float)jArray[count][2];
                min30_candle.Tables[coinName].Rows[0]["max"] = (float)jArray[count][3];
                min30_candle.Tables[coinName].Rows[0]["min"] = (float)jArray[count][4];
                min30_candle.Tables[coinName].Rows[0]["volume"] = (float)jArray[count][5];
            }


            par["interval"] = c.CANDLE_HOUR1;
            jObject = apiData.publicInfo(c.bCandleStick, coinName, par);
            if (jObject == null) return -20;
            if (jObject.ContainsKey("message")) return -21;
            jArray = (JArray)jObject["data"];
            if (jArray.Count < hour1_candle.Tables[coinName].Rows.Count) return -22;

            last = (DateTime)lastCandleUpdate.Rows[index]["hour1"];
            if (hour1_candle.Tables[coinName].Rows.Count < 1 || now.Minute < last.Minute)
            {
                hour1_candle.Tables[coinName].Rows.Clear();
                for (int i = jArray.Count - 1; i >= jArray.Count - 200 && i >= 0; i--)
                {
                    DataRow dataRow = hour1_candle.Tables[coinName].NewRow();
                    dataRow["date"] = biasDateTime.AddSeconds(long.Parse(jArray[i][0].ToString()) / 1000);
                    dataRow["open"] = (float)jArray[i][1];
                    dataRow["close"] = (float)jArray[i][2];
                    dataRow["max"] = (float)jArray[i][3];
                    dataRow["min"] = (float)jArray[i][4];
                    dataRow["volume"] = (float)jArray[i][5];
                    hour1_candle.Tables[coinName].Rows.Add(dataRow);
                }
                lastCandleUpdate.Rows[index]["hour1"] = now;
            }
            else
            {
                int count = jArray.Count - 1;
                hour1_candle.Tables[coinName].Rows[0]["date"] = biasDateTime.AddSeconds(long.Parse(jArray[count][0].ToString()) / 1000);
                hour1_candle.Tables[coinName].Rows[0]["open"] = (float)jArray[count][1];
                hour1_candle.Tables[coinName].Rows[0]["close"] = (float)jArray[count][2];
                hour1_candle.Tables[coinName].Rows[0]["max"] = (float)jArray[count][3];
                hour1_candle.Tables[coinName].Rows[0]["min"] = (float)jArray[count][4];
                hour1_candle.Tables[coinName].Rows[0]["volume"] = (float)jArray[count][5];
            }


            par["interval"] = c.CANDLE_HOUR6;
            jObject = apiData.publicInfo(c.bCandleStick, coinName, par);
            if (jObject == null) return -30;
            if (jObject.ContainsKey("message")) return -31;
            jArray = (JArray)jObject["data"];
            if (jArray.Count < hour6_candle.Tables[coinName].Rows.Count) return -32;

            last = (DateTime)lastCandleUpdate.Rows[index]["hour6"];
            if (hour6_candle.Tables[coinName].Rows.Count < 1 || now.Hour % 6 < last.Hour % 6)
            {
                hour6_candle.Tables[coinName].Rows.Clear();
                for (int i = jArray.Count - 1; i >= jArray.Count - 200 && i >= 0; i--)
                {
                    DataRow dataRow = hour6_candle.Tables[coinName].NewRow();
                    dataRow["date"] = biasDateTime.AddSeconds(long.Parse(jArray[i][0].ToString()) / 1000);
                    dataRow["open"] = (float)jArray[i][1];
                    dataRow["close"] = (float)jArray[i][2];
                    dataRow["max"] = (float)jArray[i][3];
                    dataRow["min"] = (float)jArray[i][4];
                    dataRow["volume"] = (float)jArray[i][5];
                    hour6_candle.Tables[coinName].Rows.Add(dataRow);
                }
                lastCandleUpdate.Rows[index]["hour6"] = now;
            }
            else
            {
                int count = jArray.Count - 1;
                hour6_candle.Tables[coinName].Rows[0]["date"] = biasDateTime.AddSeconds(long.Parse(jArray[count][0].ToString()) / 1000);
                hour6_candle.Tables[coinName].Rows[0]["open"] = (float)jArray[count][1];
                hour6_candle.Tables[coinName].Rows[0]["close"] = (float)jArray[count][2];
                hour6_candle.Tables[coinName].Rows[0]["max"] = (float)jArray[count][3];
                hour6_candle.Tables[coinName].Rows[0]["min"] = (float)jArray[count][4];
                hour6_candle.Tables[coinName].Rows[0]["volume"] = (float)jArray[count][5];
            }


            par["interval"] = c.CANDLE_HOUR12;
            jObject = apiData.publicInfo(c.bCandleStick, coinName, par);
            if (jObject == null) return -40;
            if (jObject.ContainsKey("message")) return -41;
            jArray = (JArray)jObject["data"];
            if (jArray.Count < hour12_candle.Tables[coinName].Rows.Count) return -42;

            last = (DateTime)lastCandleUpdate.Rows[index]["hour12"];
            if (hour12_candle.Tables[coinName].Rows.Count < 1 || now.Hour % 12 < last.Hour % 12)
            {

                hour12_candle.Tables[coinName].Rows.Clear();
                for (int i = jArray.Count - 1; i >= jArray.Count - 200 && i >= 0; i--)
                {
                    DataRow dataRow = hour12_candle.Tables[coinName].NewRow();
                    dataRow["date"] = biasDateTime.AddSeconds(long.Parse(jArray[i][0].ToString()) / 1000);
                    dataRow["open"] = (float)jArray[i][1];
                    dataRow["close"] = (float)jArray[i][2];
                    dataRow["max"] = (float)jArray[i][3];
                    dataRow["min"] = (float)jArray[i][4];
                    dataRow["volume"] = (float)jArray[i][5];
                    hour12_candle.Tables[coinName].Rows.Add(dataRow);
                }
                lastCandleUpdate.Rows[index]["hour12"] = now;
            }
            else
            {
                int count = jArray.Count - 1;
                hour12_candle.Tables[coinName].Rows[0]["date"] = biasDateTime.AddSeconds(long.Parse(jArray[count][0].ToString()) / 1000);
                hour12_candle.Tables[coinName].Rows[0]["open"] = (float)jArray[count][1];
                hour12_candle.Tables[coinName].Rows[0]["close"] = (float)jArray[count][2];
                hour12_candle.Tables[coinName].Rows[0]["max"] = (float)jArray[count][3];
                hour12_candle.Tables[coinName].Rows[0]["min"] = (float)jArray[count][4];
                hour12_candle.Tables[coinName].Rows[0]["volume"] = (float)jArray[count][5];
            }


            par["interval"] = c.CANDLE_HOUR24;
            jObject = apiData.publicInfo(c.bCandleStick, coinName, par);
            if (jObject == null) return -50;
            if (jObject.ContainsKey("message")) return -51;
            jArray = (JArray)jObject["data"];
            if (jArray.Count < hour24_candle.Tables[coinName].Rows.Count) return -52;

            last = (DateTime)lastCandleUpdate.Rows[index]["hour24"];
            if (hour24_candle.Tables[coinName].Rows.Count < 1 || now.Hour < last.Hour)
            {
                hour24_candle.Tables[coinName].Rows.Clear();
                for (int i = jArray.Count - 1; i >= jArray.Count - 200 && i >= 0; i--)
                {
                    DataRow dataRow = hour24_candle.Tables[coinName].NewRow();
                    dataRow["date"] = biasDateTime.AddSeconds(long.Parse(jArray[i][0].ToString()) / 1000);
                    dataRow["open"] = (float)jArray[i][1];
                    dataRow["close"] = (float)jArray[i][2];
                    dataRow["max"] = (float)jArray[i][3];
                    dataRow["min"] = (float)jArray[i][4];
                    dataRow["volume"] = (float)jArray[i][5];
                    hour24_candle.Tables[coinName].Rows.Add(dataRow);
                }
                lastCandleUpdate.Rows[index]["hour24"] = now;
            }
            else
            {
                int count = jArray.Count - 1;
                hour24_candle.Tables[coinName].Rows[0]["date"] = biasDateTime.AddSeconds(long.Parse(jArray[count][0].ToString()) / 1000);
                hour24_candle.Tables[coinName].Rows[0]["open"] = (float)jArray[count][1];
                hour24_candle.Tables[coinName].Rows[0]["close"] = (float)jArray[count][2];
                hour24_candle.Tables[coinName].Rows[0]["max"] = (float)jArray[count][3];
                hour24_candle.Tables[coinName].Rows[0]["min"] = (float)jArray[count][4];
                hour24_candle.Tables[coinName].Rows[0]["volume"] = (float)jArray[count][5];
            }

            return 0;
        }
        public void addBABB(int index)
        {
            string coinName = coinList[index];
            float tempPercent;

            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_MIN30, 0)) > -10000)
            {
                ba0[0].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb0[0].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR1, 0)) > -10000)
            {
                ba0[1].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb0[1].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR6, 0)) > -10000)
            {
                ba0[2].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb0[2].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR12, 0)) > -10000)
            {
                ba0[3].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb0[3].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR24, 0)) > -10000)
            {
                ba0[4].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb0[4].addTotal(tempPercent, 1);
            }

            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_MIN30, 1)) > -10000)
            {
                ba1[0].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb1[0].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR1, 1)) > -10000)
            {
                ba1[1].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb1[1].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR6, 1)) > -10000)
            {
                ba1[2].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb1[2].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR12, 1)) > -10000)
            {
                ba1[3].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb1[3].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR24, 1)) > -10000)
            {
                ba1[4].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb1[4].addTotal(tempPercent, 1);
            }

            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_MIN30, 2)) > -10000)
            {
                ba2[0].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb2[0].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR1, 2)) > -10000)
            {
                ba2[1].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb2[1].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR6, 2)) > -10000)
            {
                ba2[2].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb2[2].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR12, 2)) > -10000)
            {
                ba2[3].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb2[3].addTotal(tempPercent, 1);
            }
            if ((tempPercent = getBollingerPercent(coinName, c.CANDLE_HOUR24, 2)) > -10000)
            {
                ba2[4].addTotal(tempPercent, 1 / (index + 1) / (index + 1));
                bb2[4].addTotal(tempPercent, 1);
            }
        }
        public void setBABB()
        {
            for (int i = 0; i < 5; i++)
            {
                ba0[i].setAverage();
                ba1[i].setAverage();
                ba2[i].setAverage();
                bb0[i].setAverage();
                bb1[i].setAverage();
                bb2[i].setAverage();
            }
        }


        private float getCandleAverageValue(string coinName, string dataType, int index)
        {
            DataTable lastCandle = null;
            if (dataType == c.CANDLE_HOUR24) lastCandle = hour24_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR12) lastCandle = hour12_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR6) lastCandle = hour6_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR1) lastCandle = hour1_candle.Tables[coinName];
            else if (dataType == c.CANDLE_MIN30) lastCandle = min30_candle.Tables[coinName];
            if (lastCandle.Rows.Count < 28 + index) return -1f;

            float averagePrice = 0;
            for (int i = 0; i < 28; i++)
                averagePrice += (float)lastCandle.Rows[i + index]["close"];

            return averagePrice / 28f;
        }
        private float[] getBollingerValue(string coinName, string dataType, int index)
        {
            DataTable lastCandle = null;
            if (dataType == c.CANDLE_HOUR24) lastCandle = hour24_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR12) lastCandle = hour12_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR6) lastCandle = hour6_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR1) lastCandle = hour1_candle.Tables[coinName];
            else if (dataType == c.CANDLE_MIN30) lastCandle = min30_candle.Tables[coinName];
            if (lastCandle.Rows.Count < 28 + index) return null;

            float averagePrice = getCandleAverageValue(coinName, dataType, index);
            if (averagePrice < 0) return null;
            float dispersion = 0;
            for (int i = 0; i < 28; i++)
                dispersion += (float)Math.Pow(averagePrice - (float)lastCandle.Rows[i + index]["close"], 2);
            dispersion = (float)Math.Sqrt(dispersion / 28d);

            return new float[] { averagePrice, dispersion };
        }
        private float getBollingerPercent(string coinName, string dataType, int index)
        {
            DataTable lastCandle = null;
            if (dataType == c.CANDLE_HOUR24) lastCandle = hour24_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR12) lastCandle = hour12_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR6) lastCandle = hour6_candle.Tables[coinName];
            else if (dataType == c.CANDLE_HOUR1) lastCandle = hour1_candle.Tables[coinName];
            else if (dataType == c.CANDLE_MIN30) lastCandle = min30_candle.Tables[coinName];
            if (lastCandle.Rows.Count < 28 + index) return -100000;

            float[] avgDis = getBollingerValue(coinName, dataType, index);
            float retVal = (float)lastCandle.Rows[index]["close"];

            retVal -= avgDis[0];
            retVal /= 2 * avgDis[1];
            retVal *= 100f;

            return retVal;
        }


        private int getBollingerResult(string coinName, string dataType, int index, double targetPercent)
        {
            float curPercent = getBollingerPercent(coinName, dataType, index);
            if (curPercent < -10000) return 0;
            if (curPercent < targetPercent) return -1;
            else return 1;
        }


        public int executeCheckResult(int index)
        {
            Dictionary<string, string> orderDetailPar = new Dictionary<string, string>();
            orderDetailPar.Add("order_id", "C" + ((ulong)order.Rows[index]["id"]).ToString("D19"));
            orderDetailPar.Add("order_currency", order.Rows[index]["coinName"].ToString());
            orderDetailPar.Add("payment_currency", "KRW");
            JObject jObject = apiData.privateInfo(c.vORDERS_DETAIL, orderDetailPar);
            if (jObject == null) return -1;
            if (jObject["status"].ToString() != "0000") return -2;
            if (jObject.ContainsKey("message")) return -3;

            if (jObject["data"]["order_status"].ToString() != "Completed") return 0;

            float unit = 0f;
            float price = 0f;
            float fee = 0f;

            JArray jArray = (JArray)jObject["data"]["contract"];
            for (int i = 0; i < jArray.Count; i++)
            {
                unit += (float)jArray[i]["units"];
                price += (float)jArray[i]["price"] * (float)jArray[i]["units"];
                fee += (float)jArray[i]["fee"];
            }
            price /= unit;

            TradeData tradeData = new TradeData();
            tradeData.id = (ulong)order.Rows[index]["id"];
            tradeData.date = DateTime.Now;
            tradeData.coinName = order.Rows[index]["coinName"].ToString();
            tradeData.isBid = jObject["data"]["type"].ToString() == "bid" ? true : false;
            tradeData.unit = unit;
            tradeData.price = price;
            tradeData.fee = fee;

            if (tradeData.isBid)
            {
                DataRow row = state.Tables[order.Rows[index]["coinName"].ToString()].NewRow();
                row["id"] = tradeData.id;
                row["date"] = tradeData.date;
                row["unit"] = tradeData.unit;
                row["price"] = price;
                row["krw"] = tradeData.unit * tradeData.price;
                state.Tables[tradeData.coinName].Rows.Add(row);

                executionStr.Add(new output(1, "Macro Execution",
                    "Buy " + tradeData.unit.ToString("0.####") + " "
                    + tradeData.coinName + " for " + (tradeData.price * tradeData.unit).ToString("0.##") + " KRW"));
            }
            else
            {
                for (int i = 0; i < state.Tables[tradeData.coinName].Rows.Count; i++)
                {
                    if ((ulong)order.Rows[index]["target_id"] == (ulong)state.Tables[tradeData.coinName].Rows[i]["id"])
                    {
                        float temp_price = (float)state.Tables[tradeData.coinName].Rows[i]["price"];

                        executionStr.Add(new output(1, "Macro Execution",
                            "Sold " + tradeData.unit.ToString("0.####") + " "
                            + tradeData.coinName + " for " + (tradeData.price * tradeData.unit).ToString("0.##")
                            + " KRW (yield : " + ((tradeData.price - temp_price) * tradeData.unit - (tradeData.fee * 2f)).ToString("0.##") + " KRW)"));
                        state.Tables[tradeData.coinName].Rows.RemoveAt(i);
                        break;
                    }
                }
            }

            order.Rows.RemoveAt(index);
            return 1;
        }
        public int executeMacroBuy(int index)
        {
            string coinName = coinList[index];
            if (holdKRW < setting.krw * 1.5f) return 0;
            if (state.Tables[coinName].Rows.Count >= setting.time && setting.time != 0) return 0;


            DataTable buyCandle = null;
            if (setting.min30_from > -90000d) buyCandle = min30_candle.Tables[coinName];
            else if (setting.hour1_from > -90000d) buyCandle = hour1_candle.Tables[coinName];
            else if (setting.hour6_from > -90000d) buyCandle = hour6_candle.Tables[coinName];
            else if (setting.hour12_from > -90000d) buyCandle = hour12_candle.Tables[coinName];
            else if (setting.hour24_from > -90000d) buyCandle = hour24_candle.Tables[coinName];
            if (buyCandle == null)
            {
                executionStr.Add(new output(0, "Macro Execution", "Fail to load " + coinName + " buy candle (NULL)"));
                return -1;
            }
            if (buyCandle.Rows.Count < 28)
            {
                if (errorList.Contains(coinName))
                {
                    coinList.Remove(coinName);
                    errorList.Remove(coinName);
                    executionStr.Add(new output(0, "Macro Execution",
                        "Fail to load " + coinName + " buy candle (Not Enouph) " + coinName + " remove from macro list"));
                }
                else
                {
                    errorList.Add(coinName);
                    executionStr.Add(new output(0, "Macro Execution",
                        "Fail to load " + coinName + " buy candle (Not Enouph) If one more error, " + coinName + " remove from macro list"));
                }
                return -1;
            }
            if (errorList.Contains(coinName)) errorList.Remove(coinName);
            if ((float)buyCandle.Rows[0]["open"] >= (float)buyCandle.Rows[0]["close"]) return 0;
            if (((float)buyCandle.Rows[0]["open"] + (float)buyCandle.Rows[0]["close"]) / 2d <
                ((float)buyCandle.Rows[1]["open"] + (float)buyCandle.Rows[1]["close"] * 4d) / 5d) return 0;


            DateTime lastBuyDate = DateTime.Now.AddYears(-1);
            for (int i = 0; i < state.Tables[coinName].Rows.Count; i++)
                if (DateTime.Compare(lastBuyDate, (DateTime)state.Tables[coinName].Rows[i]["date"]) < 0)
                    lastBuyDate = (DateTime)state.Tables[coinName].Rows[i]["date"];

            if (setting.min30_from > -90000d) { if (DateTime.Compare(DateTime.Now, lastBuyDate.AddMinutes(30)) <= 0) return 0; }
            else if (setting.hour1_from > -90000d) { if (DateTime.Compare(DateTime.Now, lastBuyDate.AddHours(1)) <= 0) return 0; }
            else if (setting.hour6_from > -90000d) { if (DateTime.Compare(DateTime.Now, lastBuyDate.AddHours(6)) <= 0) return 0; }
            else if (setting.hour12_from > -90000d) { if (DateTime.Compare(DateTime.Now, lastBuyDate.AddHours(12)) <= 0) return 0; }
            else if (setting.hour24_from > -90000d) { if (DateTime.Compare(DateTime.Now, lastBuyDate.AddHours(24)) <= 0) return 0; }


            if (setting.hour24_from > -90000d)
            {
                if (setting.hour24_bias)
                { if (getBollingerResult(coinName, c.CANDLE_HOUR24, 0, setting.hour24_from + bb0[4].avg) >= 0) return 0; }
                else
                { if (getBollingerResult(coinName, c.CANDLE_HOUR24, 0, setting.hour24_from) <= 0) return 0; }
            }
            if (setting.hour12_from > -90000d)
            {
                if (setting.hour12_bias)
                { if (getBollingerResult(coinName, c.CANDLE_HOUR12, 0, setting.hour12_from + bb0[3].avg) >= 0) return 0; }
                else
                { if (getBollingerResult(coinName, c.CANDLE_HOUR12, 0, setting.hour12_from) >= 0) return 0; }
            }
            if (setting.hour6_from > -90000d)
            {
                if (setting.hour6_bias)
                { if (getBollingerResult(coinName, c.CANDLE_HOUR6, 0, setting.hour6_from + bb0[2].avg) >= 0) return 0; }
                else
                { if (getBollingerResult(coinName, c.CANDLE_HOUR6, 0, setting.hour6_from) >= 0) return 0; }
            }
            if (setting.hour1_from > -90000d)
            {
                if (setting.hour1_bias)
                { if (getBollingerResult(coinName, c.CANDLE_HOUR1, 0, setting.hour1_from + bb0[1].avg) >= 0) return 0; }
                else
                { if (getBollingerResult(coinName, c.CANDLE_HOUR1, 0, setting.hour1_from) >= 0) return 0; }
            }
            if (setting.min30_from > -90000d)
            {
                if (setting.min30_bias)
                { if (getBollingerResult(coinName, c.CANDLE_MIN30, 0, setting.min30_from + bb0[0].avg) >= 0) return 0; }
                else
                { if (getBollingerResult(coinName, c.CANDLE_MIN30, 0, setting.min30_from) >= 0) return 0; }
            }


            float unit = setting.krw / (float)buyCandle.Rows[0]["close"];
            Dictionary<string, string> par = new Dictionary<string, string>();
            par.Add("units", unit.ToString("0.####"));
            par.Add("order_currency", coinName);
            par.Add("payment_currency", "KRW");
            JObject jObject = apiData.privateTrade(c.vMARKET_BUY, par);
            if (jObject == null)
            {
                executionStr.Add(new output(0, "Macro Execution", "Fail to buy " + coinName + " (NULL)"));
                return -2;
            }
            if (jObject["status"].ToString() != "0000")
            {
                executionStr.Add(new output(0, "Macro Execution", "Fail to buy " + coinName + " (" + jObject["status"].ToString() + ")"));
                return -2;
            }
            if (jObject.ContainsKey("message"))
            {
                executionStr.Add(new output(0, "Macro Execution", "Fail to buy " + coinName + " (" + jObject["message"].ToString() + ")"));
                return -2;
            }

            DataRow row = order.NewRow();
            row["coinName"] = coinName;
            row["id"] = ulong.Parse(jObject["order_id"].ToString().Substring(1));
            row["target_id"] = 0;
            order.Rows.Add(row);

            return 1;
        }
        public int executeMacroSell(int index)
        {
            string coinName = coinList[index];
            if (state.Tables[coinName].Rows.Count < 1) return 0;


            DataTable sellCandle = min30_candle.Tables[coinName];
            if (sellCandle == null)
            {
                executionStr.Add(new output(0, "Macro Execution", "Fail to load " + coinName + " sell candle (NULL)"));
                return -1;
            }
            if (sellCandle.Rows.Count < 28)
            {
                executionStr.Add(new output(0, "Macro Execution", "Fail to load " + coinName + " sell candle (Not Enouph)"));
                return -1;
            }
            if ((float)sellCandle.Rows[0]["open"] <= (float)sellCandle.Rows[0]["close"]) return 0;
            if (((float)sellCandle.Rows[0]["open"] + (float)sellCandle.Rows[0]["close"]) / 2d >
                ((float)sellCandle.Rows[1]["open"] + (float)sellCandle.Rows[1]["close"] * 4d) / 5d) return 0;


            for (int i = 0; i < state.Tables[coinName].Rows.Count; i++)
            {
                float unit = (float)state.Tables[coinName].Rows[i]["unit"];
                float buyPrice = (float)state.Tables[coinName].Rows[i]["price"];
                buyPrice *= (100f + setting.yield) / 100f;
                if ((float)sellCandle.Rows[0]["close"] < buyPrice) continue;


                if (setting.hour24_to > -90000d)
                {
                    if (setting.hour24_bias)
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR24, 0, setting.hour24_to + bb0[4].avg) <= 0) return 0; }
                    else
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR24, 0, setting.hour24_to) <= 0) return 0; }
                }
                if (setting.hour12_to > -90000d)
                {
                    if (setting.hour12_bias)
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR12, 0, setting.hour12_to + bb0[3].avg) <= 0) return 0; }
                    else
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR12, 0, setting.hour12_to) <= 0) return 0; }
                }
                if (setting.hour6_to > -90000d)
                {
                    if (setting.hour6_bias)
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR6, 0, setting.hour6_to + bb0[2].avg) <= 0) return 0; }
                    else
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR6, 0, setting.hour6_to) <= 0) return 0; }
                }
                if (setting.hour1_to > -90000d)
                {
                    if (setting.hour1_bias)
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR1, 0, setting.hour1_to + bb0[1].avg) <= 0) return 0; }
                    else
                    { if (getBollingerResult(coinName, c.CANDLE_HOUR1, 0, setting.hour1_to) <= 0) return 0; }
                }
                if (setting.min30_to > -90000d)
                {
                    if (setting.min30_bias)
                    { if (getBollingerResult(coinName, c.CANDLE_MIN30, 0, setting.min30_to + bb0[0].avg) <= 0) return 0; }
                    else
                    { if (getBollingerResult(coinName, c.CANDLE_MIN30, 0, setting.min30_to) <= 0) return 0; }
                }


                Dictionary<string, string> par = new Dictionary<string, string>();
                par.Add("units", unit.ToString("0.####"));
                par.Add("order_currency", coinName);
                par.Add("payment_currency", "KRW");
                JObject jObject = apiData.privateTrade(c.vMARKET_SELL, par);
                if (jObject == null)
                {
                    executionStr.Add(new output(0, "Macro Execution", "Fail to sell " + coinName + " (NULL)"));
                    return -2;
                }
                if (jObject["status"].ToString() != "0000")
                {
                    executionStr.Add(new output(0, "Macro Execution", "Fail to sell " + coinName + " (" + jObject["status"].ToString() + ")"));
                    return -2;
                }
                if (jObject.ContainsKey("message"))
                {
                    executionStr.Add(new output(0, "Macro Execution", "Fail to sell " + coinName + " (" + jObject["message"].ToString() + ")"));
                    return -2;
                }

                DataRow row = order.NewRow();
                row["coinName"] = coinName;
                row["id"] = ulong.Parse(jObject["order_id"].ToString().Substring(1));
                row["target_id"] = state.Tables[coinName].Rows[i]["id"];
                order.Rows.Add(row);
            }

            return 1;
        }
    }
}
