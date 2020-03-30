using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class TransactionStat
    {
        public DateTime startTime { get; set; }
        public string transactionDirection { get; set; }
        public decimal amountBtc { get; set; }
        public string overallDirection { get; set; }
        public double startPrice { get; set; }

        public double highestPrice { get; set; }

        public double lowestPrice { get; set; }
        public decimal pctChgUp { get; set; }
        public decimal pctChgDown { get; set; }
        public decimal pctChg1Hr { get; set; }
        public decimal pctChg2Hr { get; set; }
        public decimal pctChg3Hr { get; set; }
        public decimal pctChg6Hr { get; set; }
        public decimal pctChg12Hr { get; set; }
        public decimal pctChg24Hr { get; set; }
    }
}
