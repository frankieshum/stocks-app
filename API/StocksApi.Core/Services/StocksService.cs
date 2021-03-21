using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StocksApi.Core.Models;
using StocksApi.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using StocksApi.Core.Utilities;

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
            try
            {
                // Get search results from Finnhub API
                string contentJson = await GetDataFromApiAsync("finnhub", $"search?q={searchTerm}&token={_finnhubApiToken}");
                if (string.IsNullOrEmpty(contentJson))
                    return null;

                // Retrieve stock symbols and names
                JToken searchResults = JObject.Parse(contentJson)["result"];
                var response = new List<Stock>();
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
        /// Gets stock detail for the specified symbol.
        /// </summary>
        /// <param name="symbol">The symbol for which to get the stock.</param>
        /// <returns>A matching StockDetail object.</returns>
        public async Task<StockDetail> GetStockAsync(string symbol)
        {
            try
            {
                // Get price (from IEX)
                string quoteString = await GetDataFromApiAsync("iex", $"stock/{symbol}/quote/latestPrice?token={_iexApiToken}");
                if (string.IsNullOrEmpty(quoteString))
                    return null;

                // Get stock info (from Finnhub)
                string stockInfoJson = await GetDataFromApiAsync("finnhub", $"stock/profile2?symbol={symbol}&token={_finnhubApiToken}");
                if (string.IsNullOrEmpty(stockInfoJson))
                    return null;

                // Ensure that stock info contains valid data
                string currencyString = JObject.Parse(stockInfoJson)?["currency"]?.ToString();
                if (!Enum.TryParse(currencyString, out StockCurrency currency))
                    return null;

                // Construct the response
                string name = JObject.Parse(stockInfoJson)["name"].ToString();
                decimal quote = decimal.Parse(quoteString);
                var price = new StockPrice(quote, currency, DateTime.Today);
                return new StockDetail(symbol, name, price);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StocksService.GetStockAsync: Exception occurred while getting stock '{message}'.", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets list of historical prices for the specified symbol and period.
        /// </summary>
        /// <param name="symbol">The symbol for which to get the stock.</param>
        /// <param name="range">The date range for which to get the history.</param>
        /// <returns>A StockHistory object for the specified stock and period.</returns>
        public async Task<StockHistory> GetStockHistoryAsync(string symbol, DateRange range)
        {
            var response = new StockHistory();
            try
            {
                // Get search results from IEX API
                string contentJson = await GetDataFromApiAsync("iex", $"stock/{symbol}/chart/{range.ToShortString()}?token={_iexApiToken}");
                if (string.IsNullOrEmpty(contentJson))
                    return null;
            
                // Retrieve dates and prices
                JArray historicalPrices = JArray.Parse(contentJson);
                foreach (JObject historicalPrice in historicalPrices.OfType<JObject>())
                {
                    string date = historicalPrice["date"].ToString();
                    string price = historicalPrice["close"].ToString();
                    //var stockPrice = new StockPrice()
                    //
                    //string symbol = searchResult["symbol"].ToString();
                    //string description = searchResult["description"].ToString();
                    //response.Add(new Stock(symbol, description));
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
        /// Makes a GET request to the specified relative URI with the specified named HTTP client. 
        /// </summary>
        /// <param name="httpClientName">The name of the HTTP client.</param>
        /// <param name="uri">The relative endpoint URI.</param>
        /// <returns>Returns the HTTP response as a string, or null if the request was unsuccessful.</returns>
        private async Task<string> GetDataFromApiAsync(string httpClientName, string uri)
        {
            try
            {
                HttpClient httpClient = _httpClientFactory.CreateClient(httpClientName);
                HttpResponseMessage apiResponse = await httpClient.GetAsync(uri);
                if (!apiResponse.IsSuccessStatusCode)
                {
                    // Received not OK response from data source
                    _logger.LogError("StocksService.{methodName}: Request to external API returned unexpected response '{status}'. Request '{action} {uri}'.",
                        nameof(SearchStocksAsync), apiResponse.ReasonPhrase, apiResponse.RequestMessage.Method.ToString(), apiResponse.RequestMessage.RequestUri.ToString());
                    return null;
                }
                // Parse HTTP content to JSON
                string content = await apiResponse.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StocksService.{methodName}: Exception occurred while calling external API.", nameof(GetDataFromApiAsync));
                return null;
            }
        }
    }
}
