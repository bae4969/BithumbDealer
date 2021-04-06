using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Data;
using System.Collections.Generic;

// use 'ApiData' class to run 'XCoinAPI' class, and in the meantime there is an 'Bithumb' class

namespace BithumbDealer.src
{
    // this class downloaded at bitsum homepage
    class XCoinAPI
    {
        private string m_sAPI_URL = "https://api.bithumb.com";
        private string m_sAPI_Key = "";
        private string m_sAPI_Secret = "";

        public XCoinAPI(string sAPI_Key, string sAPI_Secret)
        {
            m_sAPI_Key = sAPI_Key;
            m_sAPI_Secret = sAPI_Secret;
        }

        private string ByteToString(byte[] rgbyBuff)
        {
            string sHexStr = "";
            for (int nCnt = 0; nCnt < rgbyBuff.Length; nCnt++)
                sHexStr += rgbyBuff[nCnt].ToString("x2"); // Hex format
            return sHexStr;
        }
        private byte[] StringToByte(string sStr)
        {
            byte[] rgbyBuff = Encoding.UTF8.GetBytes(sStr);
            return rgbyBuff;
        }

        private long MicroSecTime()
        {
            long nEpochTicks = 0;
            long nUnixTimeStamp = 0;
            long nNowTicks = 0;
            long nowMiliseconds = 0;
            string sNonce = "";
            DateTime DateTimeNow;

            nEpochTicks = new DateTime(1970, 1, 1).Ticks;
            DateTimeNow = DateTime.UtcNow;
            nNowTicks = DateTimeNow.Ticks;
            nowMiliseconds = DateTimeNow.Millisecond;
            nUnixTimeStamp = (nNowTicks - nEpochTicks) / TimeSpan.TicksPerSecond;
            sNonce = nUnixTimeStamp.ToString() + nowMiliseconds.ToString("D03");

            return Convert.ToInt64(sNonce);
        }

        private string Hash_HMAC(string sKey, string sData)
        {
            byte[] rgbyKey = Encoding.UTF8.GetBytes(sKey);
            using (var hmacsha512 = new HMACSHA512(rgbyKey))
            {
                hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(sData));
                return ByteToString(hmacsha512.Hash);
            }
        }

        public JObject xcoinApiCall(string sEndPoint, string sParams, ref string sRespBodyData)
        {
            string sAPI_Sign = "";
            string sPostData = sParams;
            string sHMAC_Key = "";
            string sHMAC_Data = "";
            string sResult = "";
            long nNonce = 0;

            sPostData += "&endpoint=" + Uri.EscapeDataString(sEndPoint);
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(m_sAPI_URL + sEndPoint);
                byte[] rgbyData = Encoding.ASCII.GetBytes(sPostData);

                nNonce = MicroSecTime();
                sHMAC_Key = m_sAPI_Secret;
                sHMAC_Data = sEndPoint + (char)0 + sPostData + (char)0 + nNonce.ToString();
                sResult = Hash_HMAC(sHMAC_Key, sHMAC_Data);
                sAPI_Sign = Convert.ToBase64String(StringToByte(sResult));

                Request.Headers.Add("Api-Key", m_sAPI_Key);
                Request.Headers.Add("Api-Sign", sAPI_Sign);
                Request.Headers.Add("Api-Nonce", nNonce.ToString());
                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded";
                Request.ContentLength = rgbyData.Length;

                using (var stream = Request.GetRequestStream())
                    stream.Write(rgbyData, 0, rgbyData.Length);

                var Response = (HttpWebResponse)Request.GetResponse();
                sRespBodyData = new StreamReader(Response.GetResponseStream()).ReadToEnd();

                return JObject.Parse(sRespBodyData);
            }
            catch
            {
                return null;
            }
        }
    }



    static class c
    {
        public const int PUBLIC_INFO = 0;
        public const int PRIVATE_INFO = 1;
        public const int PRIVATE_TRADE = 2;

        public const int bTICKER = 0;
        public const int bORDERBOOK = 1;
        public const int bTRANSACTION_HISTORY = 2;
        public const int bASSETS_STATUS = 3;
        public const int bBTCI = 4;
        public const int bCandleStick = 5;

        public const int vACCOUNT = 0;
        public const int vBALANCE = 1;
        public const int vWALLET_ADDR = 2;
        public const int vTICKER = 3;
        public const int vORDERS = 4;
        public const int vORDERS_DETAIL = 5;
        public const int vUSER_TRANS = 6;

        public const int vPLACE = 0;
        public const int vCANCEL = 1;
        public const int vMARKET_BUY = 2;
        public const int vMARKET_SELL = 3;
        public const int vBTC_WITHDRAWAL = 4;
        public const int vKRW_WITHDRAWAL = 5;

        public const string CANDLE_MIN1 = "1m";
        public const string CANDLE_MIN3 = "3m";
        public const string CANDLE_MIN5 = "5m";
        public const string CANDLE_MIN10 = "10m";
        public const string CANDLE_MIN30 = "30m";
        public const string CANDLE_HOUR1 = "1h";
        public const string CANDLE_HOUR6 = "6h";
        public const string CANDLE_HOUR12 = "12h";
        public const string CANDLE_HOUR24 = "24h";
    }

    class Bithumb
    {
        private string[] publicInfo = new string[]{
            "/ticker",
            "/orderbook",
            "/transaction_history",
            "/assetsstatus",
            "/btci",
            "/candlestick"
        };
        private string[] privateInfo = new string[]{
            "/account",
            "/balance",
            "/wallet_address",
            "/ticker",
            "/orders",
            "/order_detail",
            "/user_transactions"
        };
        private string[] privateTrade = new string[]{
            "/place",
            "/cancel",
            "/market_buy",
            "/market_sell",
            "/btc_withdrawal",
            "/krw_withdrawal"
        };

        private XCoinAPI hAPI_Svr;
        private string endPoint = "";
        private string parameter = "";
        private string sRespBodyData;

        // set public key and private key
        // before use this class, need to call this functinon
        public Bithumb(string sAPI_Key, string sAPI_Secret)
        {
            hAPI_Svr = new XCoinAPI(sAPI_Key, sAPI_Secret);
        }

        // dictionary parameter to string parameter
        public string toParStr(Dictionary<string, string> parameter)
        {
            if (parameter == null) return null;
            return string.Join("&", parameter.Select(x => x.Key + "=" + x.Value).ToArray());
        }


        // set endprint
        public int setEndPoint(
            int type, int funcNum, string coinName = null, string KRW = null)
        {
            switch (type)
            {
                case c.PUBLIC_INFO:
                    if (funcNum < 0 || funcNum > 5) return -1;
                    endPoint = "/public";
                    endPoint += publicInfo[funcNum];
                    if (coinName != null) endPoint += "/" + coinName;
                    if (KRW != null) endPoint += "_" + KRW;
                    return 0;
                case c.PRIVATE_INFO:
                    if (funcNum < 0 || funcNum > 6) return -1;
                    endPoint = "/info";
                    endPoint += privateInfo[funcNum];
                    if (coinName != null) endPoint += "/" + coinName;
                    if (KRW != null) endPoint += "_" + KRW;
                    return 0;
                case c.PRIVATE_TRADE:
                    if (funcNum < 0 || funcNum > 5) return -1;
                    endPoint = "/trade";
                    endPoint += privateTrade[funcNum];
                    if (coinName != null) endPoint += "/" + coinName;
                    if (KRW != null) endPoint += "_" + KRW;
                    return 0;
                default:
                    return -1;
            }
        }

        // set parameter
        public int setParameter(string parameter = null)
        {
            if (parameter == null)
                this.parameter = "";
            else
                this.parameter = parameter;
            return 0;
        }


        // get api JObject type
        public JObject getResult(int type)
        {
            if (type == c.PUBLIC_INFO)
                if (parameter == "")
                    return hAPI_Svr.xcoinApiCall(endPoint, null, ref sRespBodyData);
                else
                    return hAPI_Svr.xcoinApiCall(endPoint + "?" + parameter, null, ref sRespBodyData);
            else
                return hAPI_Svr.xcoinApiCall(endPoint, parameter, ref sRespBodyData);
        }

        // get api string type
        public string getStrResult()
        {
            return sRespBodyData;
        }


        // execute api, before execute, need to call 'setEndPoint', and 'setParameter' function
        public JObject coinAPI(int type, int funcNum,
            string coinName, string KRW, string parameter)
        {
            if (setEndPoint(type, funcNum, coinName, KRW) < 0) return null;
            if (setParameter(parameter) < 0) return null;
            return getResult(type);
        }

        // execute api, before execute, need to call 'setEndPoint', and 'setParameter' function
        public JObject coinAPI(int type, int funcNum,
            string coinName, string KRW, Dictionary<string, string> parameter)
        {
            return coinAPI(type, funcNum, coinName, KRW, toParStr(parameter));
        }

        // execute api, before execute, need to call 'setEndPoint', and 'setParameter' function
        public JObject coinAPI(int type, int funcNum, string coinName, string KRW = null)
        {
            return coinAPI(type, funcNum, coinName, KRW, (string)null);
        }

        // execute api, before execute, need to call 'setEndPoint', and 'setParameter' function
        public JObject coinAPI(int type, int funcNum, Dictionary<string, string> parameter)
        {
            return coinAPI(type, funcNum, null, null, toParStr(parameter));
        }
    }



    class ApiData
    {
        private Bithumb bithumb;


        public ApiData(string sAPI_Key, string sAPI_Secret)
        {
            bithumb = new Bithumb(sAPI_Key, sAPI_Secret);
        }

        public int checkValidCoin(string coinName)
        {
            JObject apiRet = bithumb.coinAPI(c.PUBLIC_INFO, c.bTICKER, coinName, "KRW");
            if (apiRet == null)
                return -1;
            else if (apiRet["status"].ToString() != "0000")
                return -(int)apiRet["status"];
            else
                return 0;
        }

        public JObject publicInfo(int type, string coinName = null,
            Dictionary<string, string> parameter = null)
        {
            switch (type)
            {
                case c.bTICKER:
                    return bTicker(coinName);
                case c.bORDERBOOK:
                    return bOrderbook(coinName, parameter);
                case c.bTRANSACTION_HISTORY:
                    return bTransactionHistory(coinName, parameter);
                case c.bASSETS_STATUS:
                    return bAssetsStatus(coinName);
                case c.bBTCI:
                    return bBtci();
                case c.bCandleStick:
                    return bCandleStick(coinName, parameter);
                default:
                    return null;
            }
        }
        private JObject bTicker(string coinName)
        {
            return bithumb.coinAPI(c.PUBLIC_INFO, c.bTICKER, coinName, "KRW");
        }
        private JObject bOrderbook(string coinName, Dictionary<string, string> parameter)
        {
            //return bithumb.coinAPI(c.PUBLIC_INFO, c.bORDERBOOK, coinName, "KRW", parameter);
            return bithumb.coinAPI(c.PUBLIC_INFO, c.bORDERBOOK, null, null);
        }
        private JObject bTransactionHistory(string coinName, Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PUBLIC_INFO, c.bTRANSACTION_HISTORY, coinName, "KRW", parameter);
        }
        private JObject bAssetsStatus(string coinName)
        {
            return bithumb.coinAPI(c.PUBLIC_INFO, c.bASSETS_STATUS, coinName);
        }
        private JObject bBtci()
        {
            return bithumb.coinAPI(c.PUBLIC_INFO, c.bBTCI, (string)null);
        }
        private JObject bCandleStick(string coinName, Dictionary<string, string> parameter)
        {
            if (parameter == null) return null;
            return bithumb.coinAPI(c.PUBLIC_INFO, c.bCandleStick, coinName, "KRW/" + parameter["interval"]);
        }

        public JObject privateInfo(int type, Dictionary<string, string> parameter = null)
        {
            switch (type)
            {
                case c.vACCOUNT:
                    return vAccount(parameter);
                case c.vBALANCE:
                    return vBalance(parameter);
                case c.vWALLET_ADDR:
                    return vWalletAddr(parameter);
                case c.vTICKER:
                    return vTicker(parameter);
                case c.vORDERS:
                    return vOrders(parameter);
                case c.vORDERS_DETAIL:
                    return vOrdersDetail(parameter);
                case c.vUSER_TRANS:
                    return vUserTransactions(parameter);
                default:
                    return null;
            }
        }
        private JObject vAccount(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vACCOUNT, parameter);
        }
        private JObject vBalance(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vBALANCE, parameter);
        }
        private JObject vWalletAddr(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vWALLET_ADDR, parameter);
        }
        private JObject vTicker(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vTICKER, parameter);
        }
        private JObject vOrders(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vORDERS, parameter);
        }
        private JObject vOrdersDetail(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vORDERS_DETAIL, parameter);
        }
        private JObject vUserTransactions(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_INFO, c.vUSER_TRANS, parameter);
        }

        public JObject privateTrade(int type, Dictionary<string, string> parameter = null)
        {
            switch (type)
            {
                case c.vPLACE:
                    return vPlace(parameter);
                case c.vCANCEL:
                    return vCancel(parameter);
                case c.vMARKET_BUY:
                    return vMarketBuy(parameter);
                case c.vMARKET_SELL:
                    return vMarketSell(parameter);
                case c.vBTC_WITHDRAWAL:
                    return vBtcWithdrawal(parameter);
                case c.vKRW_WITHDRAWAL:
                    return vKrwWithdrawal(parameter);
                default:
                    return null;
            }
        }
        private JObject vPlace(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_TRADE, c.vPLACE, parameter);
        }
        private JObject vCancel(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_TRADE, c.vCANCEL, parameter);
        }
        private JObject vMarketBuy(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_TRADE, c.vMARKET_BUY, parameter);
        }
        private JObject vMarketSell(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_TRADE, c.vMARKET_SELL, parameter);
        }
        private JObject vBtcWithdrawal(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_TRADE, c.vBTC_WITHDRAWAL, parameter);
        }
        private JObject vKrwWithdrawal(Dictionary<string, string> parameter)
        {
            return bithumb.coinAPI(c.PRIVATE_TRADE, c.vKRW_WITHDRAWAL, parameter);
        }
    }
}
