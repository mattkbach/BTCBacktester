using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTCBacktester
{
    public class BTCTransaction
    {
        public int block { get; set; }
        public DateTime timestamp { get; set; }
        public decimal amountBtc { get; set; }
        public decimal amountUsd { get; set; }
        public string direction { get; set; }
    }
}
