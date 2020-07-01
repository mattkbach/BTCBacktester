using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class CandleDataHelper
    {
        public static List<BitmexCandleData> CandleDataImport(string sFile)
        {
            List<BitmexCandleData> candleData = new List<BitmexCandleData>();
            string candleDataFileRead = File.ReadAllText(sFile);
            candleData = JsonConvert.DeserializeObject<List<BitmexCandleData>>(candleDataFileRead);
            return candleData;
        }

        public static void LoadCandleData()
        {
            Console.WriteLine("Loading Candle Data, please wait ...");
            if (System.IO.File.Exists(Program.candleDataLocation))
            {
                
                //Import candle data
                Program.candleData = CandleDataImport(Program.candleDataLocation);
                Console.Clear();
                Console.WriteLine($"Loaded {Program.candleData.Count.ToString("N0")} candles");

                Console.WriteLine($"First timestamp: {Program.candleData.First().timestamp.ToString()}\nLast timestamp: {Program.candleData.Last().timestamp.ToString()}");

                Console.Write("Update candledata? (y/n): ");
                string updateCandleData = Console.ReadLine();
                if (updateCandleData == "y")
                {
                    Console.WriteLine("Updating CandleData ...");
                    UpdateCandleData();

                    Console.WriteLine($"Candle update complete! Last timestamp: {Program.candleData.Last().timestamp.ToString()}");

                    Console.Write("Verify candledata? (y/n): ");
                    string verifyCandleData = Console.ReadLine();
                    if (verifyCandleData == "y")
                    {
                        VerifyCandleData();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error! Unable to find CandleData file at {Program.candleDataLocation}. Press enter to continue.");
                Console.ReadKey();
            }
        }

        public static void UpdateCandleData()
        {
            List<BitmexCandleData> responseJSON = new List<BitmexCandleData>();
            int iStart = 1;
            string response = "";
            while (response != "[]")
            {
                try
                {
                    string lastCandleTimestamp = Convert.ToDateTime(Program.candleData.Last().timestamp).ToString();
                    string bitMexURL = $"https://www.bitmex.com/api/v1/trade/bucketed?binSize=1m&partial=false&symbol=XBTUSD&reverse=false&startTime={lastCandleTimestamp}&start=1&count=1000";
                    using (var client = new System.Net.WebClient())
                    {
                        client.Headers.Add("User-Agent", "Nothing");
                        response = client.DownloadString(bitMexURL);

                        //Cast candle data into BitmexCandleData
                        responseJSON.AddRange(JsonConvert.DeserializeObject<List<BitmexCandleData>>(response));

                        iStart += 1000;
                        Console.WriteLine($"Processed: {iStart}");
                    }
                }
                catch (Exception exception)
                {
                    if (exception.Message == "The remote server returned an error: (429) Too Many Requests.")
                    {
                        Console.WriteLine("429 received, waiting 10 seconds ... ");
                        Thread.Sleep(10000);

                    }
                }

                Program.candleData.AddRange(responseJSON);

                //responseJSON.Reverse();

                if (responseJSON.Count < 1000)
                {
                    Console.WriteLine("Writing candle data to disk ...");
                    var output = JsonConvert.SerializeObject(Program.candleData);
                    File.WriteAllText(Program.candleDataLocation, output);
                    return;
                }

            }
        }

        public static void VerifyCandleData()
        {
            int totalCandles = Program.candleData.Count;
            int i = 1;
            bool resetTimestamp = false;
            int missingCandles = 0;
            BitmexCandleData lastCandle = new BitmexCandleData();
            DateTime currentTimestamp = Convert.ToDateTime(Program.candleData.First().timestamp.ToString());

            foreach (BitmexCandleData candle in Program.candleData)
            {
                //If the last candle was missing, resetTimeStamp set to true
                if (resetTimestamp == true)
                {
                    //Calculate difference in minutes between last candle and current candle
                    TimeSpan timeSpan = new TimeSpan();
                    DateTime lastTimestamp = (DateTime)lastCandle.timestamp;
                    timeSpan = lastTimestamp - (DateTime)candle.timestamp;
                    i += Math.Abs(timeSpan.Minutes);
                    missingCandles += Math.Abs(timeSpan.Minutes);

                    //Set currentTimestamp to timestamp of current candle
                    currentTimestamp = (DateTime)candle.timestamp;
                    resetTimestamp = false;

                }

                Console.WriteLine($"Processed {i}/{totalCandles}");
                if (candle.timestamp != currentTimestamp)
                {
                    Console.WriteLine($"Missing candle found at: (candleData){candle.timestamp.ToString()} != (currentTimestamp){currentTimestamp}.");
                    resetTimestamp = true;
                    lastCandle = candle;
                }
                currentTimestamp = currentTimestamp.AddMinutes(1);
                i++;
            }

            Console.WriteLine($"There are {missingCandles} missing candles in dataset. Press enter to continue");

            Console.ReadKey();
        }

        //private static void RepairCandleData()
        //{
        //    List<BitmexCandleData> repairedCandleData = new List<BitmexCandleData>();
        //    BitmexCandleData lastCandle = new BitmexCandleData();
        //    int i = 0;
        //    //Loop through each candle and check for missing candle
        //    foreach (BitmexCandleData candle in Program.candleData)
        //    {
        //        if (i > 0)
        //        {
        //            DateTime lastTimestamp = Convert.ToDateTime(lastCandle.timestamp);
        //            DateTime currentTimestamp = Convert.ToDateTime(candle.timestamp);
        //            TimeSpan timeSpan = new TimeSpan();
        //            timeSpan = lastTimestamp - currentTimestamp;
        //            if (timeSpan.Minutes > 1)
        //            {
        //                //Missing Candle
        //                //Build missing candle
        //                BitmexCandleData newCandle = new BitmexCandleData();

        //            }

        //        }
        //        else
        //        {
        //            //First candle
        //            //Add to repairedCandleData
        //            //Set lastCandle to current candle
        //            repairedCandleData.Add(candle);
        //            lastCandle = candle;
        //        }
                
                
        //    }
        //}

        public static double HighestHigh(int startIndex, int endIndex)
        {
            double highestHigh = 0.0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i == -1)
                {
                    continue;
                }
                else
                {
                    if (Program.candleData[i].high > highestHigh)
                    {
                        highestHigh = (double)Program.candleData[i].high;
                    }
                }
            }

            return highestHigh;
        }

        public static double LowestLow(int StartIndex, int endIndex)
        {
            double lowestLow = double.MaxValue;
            for (int i = StartIndex; i <= endIndex; i++)
            {
                if (i == -1)
                {
                    continue;
                }
                else
                {
                    if (Program.candleData[i].low < lowestLow)
                    {
                        lowestLow = (double)Program.candleData[i].low;
                    }
                }
            }

            return lowestLow;
        }

        public static double GetPriceAtTime(DateTime timestamp)
        {
            //Set seconds to 0
            DateTime startTime = timestamp.AddSeconds(-timestamp.Second);

            //Get index from timestamp
            int index = Program.candleData.FindIndex(f => f.timestamp == startTime);

            if (index == -1)
            {
                Console.WriteLine($"Error, candle not found in CandleData. {timestamp.ToString()}");
                return -1;
            }
            else
            {
                double price = (double)Program.candleData[index].close;
                return price;
            }
        }

        public static List<int> GetIndexValues(DateTime origStartTime, int numberOfHours)
        {
            //Calculate Times 
            DateTime startTime = origStartTime.AddSeconds(-origStartTime.Second);

            //Create list for index values 
            List<int> indexList = new List<int>();

            for (int i = 0; i <= numberOfHours; i++)
            {
                //Index 0 will be candle at startTime
                DateTime time = startTime.AddHours(i);
                int index = Program.candleData.FindIndex(f => f.timestamp == time);
                if (index == -1)
                {
                    Console.WriteLine($"Error, candle not found in CandleData. {time.ToString()}");
                }
                indexList.Add(index);
            }

            return indexList;
        }

        public static IDictionary<string, double> CalculateHighs(DateTime origStartTime, List<int> indexList, int numberOfHours)
        {
            DateTime startTime = origStartTime.AddSeconds(-origStartTime.Second);

            //Calculate highs and store in dictioary 
            IDictionary<string, double> highs = new Dictionary<string, double>();
            for (int i = 1; i <= numberOfHours; i++)
            {
                string name = $"high{i}";
                double HH = HighestHigh(indexList[i - 1], indexList[i]);
                highs.Add(name, HH);
                //Console.WriteLine($"{name} {HH}");
            }

            return highs;

            ////Serialize dictionary to JSON
            //var json = JsonConvert.SerializeObject(highLow, Newtonsoft.Json.Formatting.Indented);

            ////Cast to HighLow object
            //HighLow highLow1 = JsonConvert.DeserializeObject<HighLow>(json);

            //return highLow1;
        }

        public static IDictionary<string, double> CalculateLows(DateTime origStartTime, List<int> indexList, int numberOfHours)
        {
            DateTime startTime = origStartTime.AddSeconds(-origStartTime.Second);

            //Calculate lows and store in dictioary 
            IDictionary<string, double> lows = new Dictionary<string, double>();
            for (int i = 1; i <= numberOfHours; i++)
            {
                string name = $"low{i}";
                double LL = HighestHigh(indexList[i - 1], indexList[i]);
                lows.Add(name, LL);
                //Console.WriteLine($"{name} {LL}");
            }

            return lows;

            ////Serialize dictionary to JSON
            //var json = JsonConvert.SerializeObject(highLow, Newtonsoft.Json.Formatting.Indented);

            ////Cast to HighLow object
            //HighLow highLow1 = JsonConvert.DeserializeObject<HighLow>(json);

            //return highLow1;
        }
    }
}
