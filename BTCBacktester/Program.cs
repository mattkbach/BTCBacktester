using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ChoETL;
using ConsoleTables;
using System.Linq;
using System.Net;

namespace BTCBacktester
{
    public class Program
    {
        //
        public static List<CandleData> candleData = new List<CandleData>();

        static async Task Main(string[] args)
        {
            Console.Title = "BTC Backtester 1.1";
            Console.WriteLine("Loading Candle Data, please wait ...");
            LoadCandleData();
            //StartAsync();
            while (true)
            {
                Console.Clear();
                Console.Write("Enter BTC Wallet Address: ");
                string walletAddress = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(walletAddress))
                {
                    await StartAsync(walletAddress.Trim());
                }
            }
        }

        private static void LoadCandleData()
        {
            candleData = Helper.CandleDataImport("C:\\CandleData\\1minCandleData.json");
            candleData.Reverse();
        }
        
        private static async Task StartAsync(string walletAddress)
        {
            Console.WriteLine($"Searching for {walletAddress} ...");
            List<BTCTransaction> transactions = await GetBTCTransactionsAsync($"https://bitinfocharts.com/bitcoin/address/{walletAddress}");

            if (transactions is object)
            {
                Console.WriteLine($"Found {transactions.Count} transactions. Calculating, please wait ...");
                List<TransactionStats> transactionStats = CalcuateTransactionStats(transactions, walletAddress);
                
                //CalculateWalletStats(transactionStats)
                
                
                Console.WriteLine("Press enter to continue");
                Console.ReadKey();
            }

        }

        private static void CalculateWalletStats(List<TransactionStats> transactionStats)
        {
            
        }

        private static List<TransactionStats> CalcuateTransactionStats(List<BTCTransaction> transactions, string walletAddress)
        {
            int numberOfHours = 24;
            List<TransactionStats> transactionStats = new List<TransactionStats>();
            Parallel.ForEach(transactions, transaction =>
            {
                if (transaction.amountBtc >= 400)
                {
                    Helper helper = new Helper();
                    //Console.WriteLine($"--- Time: {transaction.timestamp.ToString()} Direction: {transaction.direction} AmountBTC: {Math.Round(transaction.amountBtc,2)} ({transaction.amountUsd.ToString("C2")}) USD ---");

                    //Get start price
                    double startPrice = helper.GetPriceAtTime(transaction.timestamp);
                    if (startPrice == -1)
                    {
                        return;
                    }
                    //Console.WriteLine($"--- Start Price: {startPrice}");

                    //Get index values for each hour 
                    List<int> indexList = helper.GetIndexValues(transaction.timestamp, numberOfHours);

                    //Calculate highs and lows 
                    IDictionary<string, double> highs = helper.CalculateHighs(transaction.timestamp, indexList, numberOfHours);
                    IDictionary<string, double> lows = helper.CalculateLows(transaction.timestamp, indexList, numberOfHours);

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
                    double hhPrice = helper.HighestHigh(indexList[0], indexList[indexList.Count - 1]);
                    double llPrice = helper.LowestLow(indexList[0], indexList[indexList.Count - 1]);

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

        private static async Task<List<BTCTransaction>> GetBTCTransactionsAsync(string walletUrl)
        {
            try
            {
                List<BTCTransaction> btcTransactions = new List<BTCTransaction>();
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(walletUrl);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                foreach (HtmlNode row in htmlDocument.DocumentNode.SelectNodes("/html/body/div[6]/table/tbody/tr"))
                {
                    HtmlNodeCollection cells = row.SelectNodes("td");
                    int block = 0;
                    DateTime timestamp = DateTime.Now;
                    decimal amountBtc = 0;
                    decimal amountUsd = 0;
                    string direction = "";

                    for (int i = 0; i < cells.Count; ++i)
                    {
                        if (cells[i].InnerHtml.Contains("<a href="))
                        {
                            Match _block = Regex.Match(cells[i].InnerHtml, @">[0-9]{6}<");
                            block = Convert.ToInt32(_block.Groups[0].Value.Replace("<", "").Replace(">", ""));
                        }
                        if (cells[i].InnerHtml.Contains("<span class=\"muted utc hidden-desktop\">") && cells[i].InnerText.Contains(":"))
                        {
                            Match _timestamp = Regex.Match(cells[i].InnerText, @"[0-9]{4}-[0-9]{2}-[0-9]{2} [0-9]{2}:[0-9]{2}:[0-9]{2}");
                            timestamp = Convert.ToDateTime(_timestamp.Groups[0].Value);
                        }
                        if (cells[i].OuterHtml.Contains("text-success"))
                        {
                            direction = "IN";
                            string[] InnerTextSplit = cells[i].InnerText.Split(' ');
                            amountBtc = Convert.ToDecimal(InnerTextSplit[0].Replace("+", ""));
                            amountUsd = Math.Round(Convert.ToDecimal(InnerTextSplit[2].Replace("(", "")), 2);
                        }
                        if (cells[i].OuterHtml.Contains("text-error"))
                        {
                            direction = "OUT";
                            string[] InnerTextSplit = cells[i].InnerText.Split(' ');
                            amountBtc = Convert.ToDecimal(InnerTextSplit[0].Replace("-", ""));
                            amountUsd = Math.Round(Convert.ToDecimal(InnerTextSplit[2].Replace("(", "")), 2);
                        }
                    }

                    BTCTransaction transaction = new BTCTransaction
                    {
                        block = block,
                        direction = direction,
                        timestamp = timestamp,
                        amountBtc = amountBtc,
                        amountUsd = amountUsd
                    };

                    btcTransactions.Add(transaction);

                    //string Output = String.Format("Block: {0}, TimeStamp: {1}, Direction: {2} AmountBTC: {3} BTC, AmountUSD: {4} USD", Block, TimeStamp.ToString(), Direction, AmountBTC, AmountUSD);
                    //Console.WriteLine(Output);
                }
                btcTransactions.Reverse();
                return btcTransactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error!: {ex}");
                Console.WriteLine("Press enter to continue");
                Console.ReadKey();
            }
            return null;
        }



    }
}

