using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class BitInfoChartsHelper
    {
        public static async Task<List<BTCTransaction>> GetBTCTransactionsAsync(string walletUrl)
        {
            try
            {
                List<BTCTransaction> btcTransactions = new List<BTCTransaction>();
                var httpClient = new HttpClient();
                var html = await httpClient.GetStringAsync(walletUrl);

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);
                int i2 = 0;

                foreach (HtmlNode row in htmlDocument.DocumentNode.SelectNodes("/html/body/div[6]/table/tbody/tr"))
                {
                    i2++;
                    //Console.WriteLine(i2);


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
                            if (cells[i].InnerText == "0 BTC")
                            {
                                amountBtc = 0;
                            }
                            else
                            {
                                string[] InnerTextSplit = cells[i].InnerText.Split(' ');
                                amountBtc = Convert.ToDecimal(InnerTextSplit[0].Replace("+", ""));
                            }

                        }
                        if (cells[i].OuterHtml.Contains("text-error"))
                        {
                            direction = "OUT";
                            if (cells[i].InnerText == "0 BTC")
                            {
                                amountBtc = 0;
                            }
                            else
                            {
                                string[] InnerTextSplit = cells[i].InnerText.Split(' ');
                                amountBtc = Convert.ToDecimal(InnerTextSplit[0].Replace("-", ""));
                            }
                        }
                    }

                    BTCTransaction transaction = new BTCTransaction
                    {
                        block = block,
                        direction = direction,
                        timestamp = timestamp,
                        amountBtc = amountBtc,
                        //amountUsd = amountUsd
                    };

                    btcTransactions.Add(transaction);
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
