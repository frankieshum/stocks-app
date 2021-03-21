using StocksApi.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StocksApi.Core.Services.Interfaces
{
    public interface IStocksService
    {
        Task<List<Stock>> SearchStocksAsync(string searchTerm);
        Task<StockDetail> GetStockAsync(string symbol);
        Task<StockHistory> GetStockHistoryAsync(string symbol, DateRange range);
    }
}
