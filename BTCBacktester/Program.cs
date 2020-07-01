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
using System.Diagnostics;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;

namespace BTCBacktester
{
    public class Program
    {
        public static List<BitmexCandleData> candleData = new List<BitmexCandleData>();
        public static string candleDataLocation = "C:\\CandleData\\1minCandleData.json";
        public static string appTitle = "BTC Backtester 1.3.1";

        static async Task Main(string[] args)
        {
            Console.Title = appTitle;


            CandleDataHelper.LoadCandleData();
            
            while (true)
            {
                Console.Clear();
                Console.Write("Enter BTC Wallet Address: ");
                string walletAddress = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(walletAddress))
                {
                    await SearchBTCWalletAsync(walletAddress.Trim());
                }
            }
        }
        
        private static async Task SearchBTCWalletAsync(string walletAddress)
        {
            Console.WriteLine($"Searching for {walletAddress} ...");
            List<BTCTransaction> transactions = await BitInfoChartsHelper.GetBTCTransactionsAsync($"https://bitinfocharts.com/bitcoin/address/{walletAddress}-full");

            if (transactions is object)
            {
                Console.WriteLine($"Found {transactions.Count} transactions. Calculating, please wait ...");
                List<TransactionStats> transactionStats = CalcStatsHelper.CalcuateTransactionStats(transactions, walletAddress);

                CalcStatsHelper.CalculateWalletStats(transactionStats);
                
                Console.WriteLine("Press enter to continue");
                Console.ReadKey();
            }

        }



    }
}

