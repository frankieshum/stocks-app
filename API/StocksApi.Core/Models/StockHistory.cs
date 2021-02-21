using System;
using System.Collections.Generic;
using System.Text;

namespace StocksApi.Core.Models
{
    public class StockHistory
    {
        public Stock Stock { get; set; }
        public List<StockPrice> PriceHistory { get; set; }

        public StockHistory()
        {
            PriceHistory = new List<StockPrice>();
        }
    }
}
