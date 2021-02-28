namespace StocksApi.Core.Models
{
    public class StockDetail : Stock
    {
        public StockDetail(string symbol, string name, StockPrice price) : base(symbol, name)
        {
            Symbol = symbol;
            Name = name;
            Quote = price;
        }

        public StockPrice Quote { get; set; }
    }
}
