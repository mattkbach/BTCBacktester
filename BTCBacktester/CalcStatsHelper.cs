using ChoETL;
using ConsoleTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class CalcStatsHelper
    {
        public static void CalculateWalletStats(List<TransactionStats> transactionStats)
        {
            decimal inPctUpTotal = 0;
            decimal inPctDownTotal = 0;
            decimal outPctUpTotal = 0;
            decimal outPctDownTotal = 0;

            WalletStats walletStats = new WalletStats();

            foreach (TransactionStats transaction in transactionStats)
            {
                if (transaction.transactionDirection == "IN")
                {
                    walletStats.totalInTransactions++;
                    inPctUpTotal = inPctUpTotal + transaction.pctChgUp;
                    inPctDownTotal = inPctDownTotal + transaction.pctChgDown;
                }

                if (transaction.transactionDirection == "OUT")
                {
                    walletStats.totalOutTransactions++;
                    outPctUpTotal = outPctUpTotal + transaction.pctChgUp;
                    outPctDownTotal = outPctDownTotal + transaction.pctChgDown;
                }
            }
            walletStats.inPctUpAvg = Math.Round(inPctUpTotal / walletStats.totalInTransactions, 2);
            walletStats.inPctDownAvg = Math.Round(inPctDownTotal / walletStats.totalInTransactions, 2);
            walletStats.outPctUpAvg = Math.Round(outPctUpTotal / walletStats.totalOutTransactions, 2);
            walletStats.outPctDownAvg = Math.Round(outPctDownTotal / walletStats.totalOutTransactions, 2);

            //Write table to console
            Console.WriteLine("===== Wallet Stats =====");
            List<WalletStats> walletStatsList = new List<WalletStats>();
            walletStatsList.Add(walletStats);
            ConsoleTable.From<WalletStats>(walletStatsList).Write();

        }

        public static List<TransactionStats> CalcuateTransactionStats(List<BTCTransaction> transactions, string walletAddress)
        {
            int numberOfHours = 24;
            List<TransactionStats> transactionStats = new List<TransactionStats>();
            Parallel.ForEach(transactions, transaction =>
            {
                if (transaction.amountBtc >= 1000)
                {
                    //Ignore transactions on March 12/13th
                    DateTime dtMarch122020 = new DateTime(2020, 03, 12);
                    DateTime dtMarch132020 = new DateTime(2020, 03, 13);
                    if ((transaction.timestamp.Date == dtMarch132020.Date) || (transaction.timestamp.Date == dtMarch122020.Date))
                    {
                        return;
                    }

                    //Console.WriteLine($"--- Time: {transaction.timestamp.ToString()} Direction: {transaction.direction} AmountBTC: {Math.Round(transaction.amountBtc,2)} ({transaction.amountUsd.ToString("C2")}) USD ---");

                    //Get start price
                    double startPrice = CandleDataHelper.GetPriceAtTime(transaction.timestamp);

                    //Not found in candledata
                    if (startPrice == -1)
                    {
                        return;
                    }
                    //Console.WriteLine($"--- Start Price: {startPrice}");

                    //Get index values for each hour 
                    List<int> indexList = CandleDataHelper.GetIndexValues(transaction.timestamp, numberOfHours);

                    //Calculate highs and lows 
                    IDictionary<string, double> highs = CandleDataHelper.CalculateHighs(transaction.timestamp, indexList, numberOfHours);
                    IDictionary<string, double> lows = CandleDataHelper.CalculateLows(transaction.timestamp, indexList, numberOfHours);

                    //Calculate price difference for numberOfHours
                    decimal pctChange1Hr = 0;
                    decimal pctChange2Hr = 0;
                    decimal pctChange3Hr = 0;
                    decimal pctChange6Hr = 0;
                    decimal pctChange12Hr = 0;
                    decimal pctChange24Hr = 0;
                    for (int i = 1; i <= numberOfHours; i++)
                    {
                        double difference = 0;
                        decimal pctChange = 0;
                        if (highs[$"high{i}"] > startPrice)
                        {
                            difference = highs[$"high{i}"] - startPrice;
                            pctChange = Math.Round(((decimal)difference / (decimal)startPrice) * 100, 2);
                        }

                        if (lows[$"low{i}"] < startPrice)
                        {
                            difference = lows[$"low{i}"] - startPrice;
                            pctChange = Math.Round(((decimal)difference / (decimal)startPrice) * 100, 2);
                        }
                        if (i == 1)
                        {
                            pctChange1Hr = pctChange;
                        }
                        if (i == 2)
                        {
                            pctChange2Hr = pctChange;
                        }
                        if (i == 3)
                        {
                            pctChange3Hr = pctChange;
                        }
                        if (i == 6)
                        {
                            pctChange6Hr = pctChange;
                        }
                        if (i == 12)
                        {
                            pctChange12Hr = pctChange;
                        }
                        if (i == 24)
                        {
                            pctChange24Hr = pctChange;
                        }
                        //Console.WriteLine($"{i}HR Difference: {difference} ({pctChange}%)");

                    }

                    //Calculate highest high and lowest low 
                    double hhPrice = CandleDataHelper.HighestHigh(indexList[0], indexList[indexList.Count - 1]);
                    double llPrice = CandleDataHelper.LowestLow(indexList[0], indexList[indexList.Count - 1]);

                    if (hhPrice > 40000 || hhPrice <= 0 || llPrice > 40000 || llPrice <= 0)
                    {
                        return;
                    }

                    //Calculate % change from startPrice in both directions
                    decimal pctChangeUp = Math.Round((((decimal)hhPrice - (decimal)startPrice) / (decimal)startPrice) * 100, 2);
                    decimal pctChangeDown = Math.Round(((decimal)llPrice - (decimal)startPrice) / (decimal)startPrice * 100, 2);

                    //Calculate overall direction change over 24 hours and dollar per btc value
                    string overallDirection;
                    if (Math.Abs(hhPrice - startPrice) > Math.Abs(llPrice - startPrice))
                    {
                        overallDirection = "UP";
                    }
                    else
                    {
                        overallDirection = "DOWN";
                    }

                    //Add to transactionStats object
                    TransactionStats transactionStat = new TransactionStats();
                    transactionStat.startTime = transaction.timestamp;
                    transactionStat.amountBtc = Math.Round(transaction.amountBtc, 2);
                    transactionStat.transactionDirection = transaction.direction;
                    transactionStat.startPrice = startPrice;
                    transactionStat.overallDirection = overallDirection;
                    transactionStat.highestPrice = hhPrice;
                    transactionStat.lowestPrice = llPrice;
                    transactionStat.pctChgUp = pctChangeUp;
                    transactionStat.pctChgDown = pctChangeDown;
                    transactionStat.pctChg1Hr = pctChange1Hr;
                    transactionStat.pctChg2Hr = pctChange2Hr;
                    transactionStat.pctChg3Hr = pctChange3Hr;
                    transactionStat.pctChg6Hr = pctChange6Hr;
                    transactionStat.pctChg12Hr = pctChange12Hr;
                    transactionStat.pctChg24Hr = pctChange24Hr;


                    transactionStats.Add(transactionStat);

                }
            });

            //Sort list by ascending
            List<TransactionStats> transactionStatsSorted = transactionStats.OrderBy(o => o.startTime).ToList();

            //Write table to Console
            ConsoleTable.From<TransactionStats>(transactionStatsSorted).Write();

            //Write to CSV
            using (var parser = new ChoCSVWriter<TransactionStats>($"{walletAddress}.csv").WithFirstLineHeader())
            {
                parser.Write(transactionStatsSorted);
            }
            return transactionStatsSorted;
        }



    }
}
