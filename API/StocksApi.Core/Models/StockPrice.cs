using System;

namespace StocksApi.Core.Models
{
    public class StockPrice
    {
        public decimal Price { get; set; }
        public StockCurrency Currency { get; set; }
        public DateTime Date { get; set; }

        public StockPrice(decimal price, StockCurrency currency, DateTime date)
        {
            Price = price;
            Currency = currency;
            Date = date;
        }
    }
}
