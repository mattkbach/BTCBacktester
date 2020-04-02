using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class WalletStats
    {
        public int totalInTransactions { get; set; } = 0;
        public int totalOutTransactions { get; set; } = 0;

        public decimal inPctUpAvg { get; set; } = 0;
        public decimal inPctDownAvg { get; set; } = 0;
        public decimal outPctUpAvg { get; set; } = 0;
        public decimal outPctDownAvg { get; set; } = 0;

    }
}
