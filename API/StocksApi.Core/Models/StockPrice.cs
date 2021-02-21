using System;
using System.Collections.Generic;
using System.Text;

namespace StocksApi.Core.Models
{
    public class StockPrice
    {
        public decimal Price { get; set; }
        public Currency Currency { get; set; }
        public DateTime Date { get; set; }
    }
}
