using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static CryptoTradingForTaxes.CryptoAPI;

namespace CryptoTradingForTaxes
{
    



    class Program
    {
        static void Main(string[] args)
        {
            //configure date here
            DateTime test = DateTime.Now - new TimeSpan(24, 0, 0); //minues 1 day (24h)
            int epoch = TimeConvertion.FromDateToEpochsEST(test);

            //Configure other items here:
            string coin = "BTC";
            string baseCurrency = "USD";
            string numberOfCallsLimit = "1";//max of 2,000



            List<CoinHourlyInfo> transactions = runAPIcall_OrganizeResponse(coin, baseCurrency, numberOfCallsLimit, epoch.ToString()).ToList();
            
        }
    }

    public static class CryptoAPI {

        public static HttpClient httpClient = new HttpClient();

        public static ICollection<CoinHourlyInfo> runAPIcall_OrganizeResponse(string coin, string baseCurrency, string numberOfCallsLimit, string timeInEpocs)
        {
           

            //Run help methods
            string url = BuildUrl(coin, baseCurrency, numberOfCallsLimit, timeInEpocs);
            string response = GetStringResponse(url, httpClient);
            string parsedResponse = ParseResponse(response);

            //run items to return
            return organizeJson(parsedResponse);

        }


        private static ICollection<CoinHourlyInfo> organizeJson(string aData)
        {
            JArray jay = JArray.Parse(aData);

            IList<CoinHourlyInfo> transactions = jay.Select(p => new CoinHourlyInfo
            {
                TimeFrom = TimeConvertion.FromEpochsToDateTime((int)p["time"]),
                open = (float)p["open"],
                close = (float)p["close"],
                high = (float)p["high"],
                low = (float)p["low"],
                volumefrom = (float)p["volumefrom"],
                volumeto = (float)p["volumeto"]
            }).ToList();

            return transactions;
        }

        private static string ParseResponse(string response)
        {
            string Data = response.Split(new string[] { "Data" }, StringSplitOptions.None).Last();
            Data = Data.Split(new char[] { ':' }, 2).Last();
            return Data.Substring(0, Data.Length - 2);
        }

        private static string BuildUrl(string coin, string baseCurrency, string numberOfCallsLimit, string timeInEpocs)
        {
            string myUrl = "https://min-api.cryptocompare.com/data/v2/histohour";
            string apiKey = "a28b2e8efc2792e5018d9440f66ce864383ce607ad2da57e4f10278630fcda6d";
            string keyID = "&api_key=";

            string attributes = "?fsym=" + coin + "&tsym=" + baseCurrency + "&limit=" + numberOfCallsLimit + "&toTs=" + timeInEpocs + keyID + apiKey;

            return myUrl + attributes;
        }

        private static string GetStringResponse(string Fullurl, HttpClient client)
        {
            var task = Task.Run(async () => await GetAsync(Fullurl, client));

            return task.Result;

        }

        private static async Task<string> GetAsync(string uri, HttpClient client)
        {

            var response = await client.GetAsync(uri);

            //will throw an exception if not successful
            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => content);
        }
    }

    public class CoinHourlyInfo
    {
        public DateTime TimeFrom;
        public int TimeTo;
        public float open;
        public float close;
        public float high;
        public float low;
        public float volumefrom;
        public float volumeto;
        //calc using avg of high and low
    }

    public class TimeConvertion
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromEpochsToDateTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        public static int FromDateToEpochsEST(DateTime time)
        {
            return FromDateToEpochsUTC(time.ToUniversalTime());
        }

        public static int FromDateToEpochsUTC(DateTime time)
        {
            TimeSpan t = time - epoch;
            return (int)t.TotalSeconds;
        }

    }

}
