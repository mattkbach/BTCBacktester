using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BTCBacktester
{
    public class Helper
    {
        public double HighestHigh(int startIndex, int endIndex)
        {
            double highestHigh = 0.0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (Program.candleData[i].high > highestHigh)
                {
                    highestHigh = (double)Program.candleData[i].high;
                }
            }

            return highestHigh;
        }

        public double LowestLow(int StartIndex, int endIndex)
        {
            double lowestLow = double.MaxValue;
            for (int i = StartIndex; i <= endIndex; i++)
            {
                if (Program.candleData[i].low < lowestLow)
                {
                    lowestLow = (double)Program.candleData[i].low;
                }
            }

            return lowestLow;
        }

        public static List<CandleData> CandleDataImport(string sFile)
        {
            List<CandleData> LResponseJSON = new List<CandleData>();
            string CandleDataFileRead = File.ReadAllText(sFile);
            LResponseJSON = JsonConvert.DeserializeObject<List<CandleData>>(CandleDataFileRead);
            //test1
            //test
            return LResponseJSON;
        }


        public double GetPriceAtTime(DateTime timestamp)
        {
            //Set seconds to 0
            DateTime startTime = timestamp.AddSeconds(-timestamp.Second);

            //Get index from timestamp
            int index = Program.candleData.FindIndex(f => f.timestamp == startTime);

            double? price = Convert.ToDouble(Program.candleData[index].close);
            if (price.HasValue)
            {
                return (double)price;
             }
            return -1;

        }

        public List<int> GetIndexValues(DateTime origStartTime, int numberOfHours)
        {
            //Calculate Times 
            DateTime startTime = origStartTime.AddSeconds(-origStartTime.Second);

            //Create list for index values 
            List<int> indexList = new List<int>();
            //Add start which will be index 0
            indexList.Add(Program.candleData.FindIndex(f => f.timestamp == startTime));

            for (int i = 1; i <= numberOfHours; i++)
            {
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

        public IDictionary<string, double> CalculateHighs(DateTime origStartTime, List<int> indexList, int numberOfHours)
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

        public IDictionary<string, double> CalculateLows(DateTime origStartTime, List<int> indexList, int numberOfHours)
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
