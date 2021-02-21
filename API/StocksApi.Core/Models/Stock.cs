using System;
using System.Collections.Generic;
using System.Text;

namespace StocksApi.Core.Models
{
    public class Stock
    {
        public string Symbol { get; set; }
        public string Name { get; set; }

        public Stock(string symbol, string name)
        {
            Symbol = symbol;
            Name = name;
        }
    }
}
