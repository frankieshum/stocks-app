using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StocksApi.Core.Models;
using StocksApi.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace StocksApi.Core.Services
{
    public class StocksService : IStocksService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly string _iexApiToken;
        private readonly string _finnhubApiToken;

        public StocksService(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _iexApiToken = Environment.GetEnvironmentVariable("IEX_API_TOKEN");
            _finnhubApiToken = Environment.GetEnvironmentVariable("FINNHUB_API_TOKEN");
        }

        /// <summary>
        /// Gets list of matching stocks for the specified search string.
        /// </summary>
        /// <param name="searchTerm">Search string for querying stocks by symbol or name.</param>
        /// <returns>A list of matching Stock objects.</returns>
        public async Task<List<Stock>> SearchStocksAsync(string searchTerm)
        {
            var response = new List<Stock>();
            try
            {
                // Get search results from Finnhub API
                HttpClient httpClient = _httpClientFactory.CreateClient("finnhub");
                HttpResponseMessage apiResponse = await httpClient.GetAsync($"search?q={searchTerm}&token={_finnhubApiToken}");

                if (!apiResponse.IsSuccessStatusCode)
                {
                    // Received not OK response from data source
                    _logger.LogError("StocksService.{methodName}: Request to Finnhub API returned unexpected response '{status}'.", nameof(SearchStocksAsync), apiResponse.ReasonPhrase);
                    return null;
                }

                // Parse HTTP content to JSON
                string contentJson = await apiResponse.Content.ReadAsStringAsync();
                JToken searchResults = JObject.Parse(contentJson)["result"];
                // Retrieve stock symbols and names
                foreach (JToken searchResult in searchResults)
                {
                    string symbol = searchResult["symbol"].ToString();
                    string description = searchResult["description"].ToString();
                    response.Add(new Stock(symbol, description));
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StocksService.{methodName}: Exception occurred '{message}'.", nameof(SearchStocksAsync), ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets stock for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol for which to get the stock.</param>
        /// <returns>A matching Stock object.</returns>
        public async Task<Stock> GetStockAsync(string symbol)
        {
            // TODO
            // Call IEX API to get stock
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets list of historical prices for the specified symbol and period.
        /// </summary>
        /// <param name="symbol">The symbol for which to get the stock.</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns>A StockHistory object for the specified stock and period.</returns>
        public async Task<StockHistory> GetStockHistoryAsync(string symbol, DateTime startDate, DateTime endDate)
        {
            // TODO
            // Call IEX API to get stock history
            throw new NotImplementedException();
        }
    }
}
