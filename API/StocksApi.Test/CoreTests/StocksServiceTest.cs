using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using StocksApi.Core.Models;
using StocksApi.Core.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StocksApi.Test
{
    public class StocksServiceTest
    {
        private StocksService _stocksService;
        private Mock<IHttpClientFactory> _httpClientFactory;
        private Mock<ILogger> _logger;
        private Mock<HttpClient> _httpClient;
        private Mock<HttpMessageHandler> _httpMessageHandler;

        [SetUp]
        public void Setup()
        {
            _httpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new Mock<HttpClient>(_httpMessageHandler.Object);
            _httpClient.Object.BaseAddress = new Uri("http://some.uri.to/prevent/exceptions");
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _logger = new Mock<ILogger>();
            _stocksService = new StocksService(_httpClientFactory.Object, _logger.Object);
        }

        [Test]
        [Description("Test that service returns valid object if data retrieved successfully.")]
        public void TestSearchStocksAsync_OK()
        {
            // Create data from external API
            dynamic externalData = new 
            { 
                result = new List<dynamic>() 
                { 
                    new { symbol = "AAA", description = "Company A" }
                } 
            };
            // Create HTTP response from external API
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK) 
            {
                Content = new StringContent(JsonConvert.SerializeObject(externalData), Encoding.UTF8, "application/json")
            };
            // Set up HTTP client result
            _httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var result = _stocksService.SearchStocksAsync("abc").Result;

            // Verify result
            Assert.AreEqual(externalData.result.Count, result.Count);
            Assert.AreEqual(externalData.result[0].symbol, result[0].Symbol);
            Assert.AreEqual(externalData.result[0].description, result[0].Name);
        }

        [Test]
        [Description("Test that service returns null object if data source returns unexpected response.")]
        public void TestSearchStocksAsync_UnexpectedResponse()
        {
            // Set up HTTP client result
            _httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var result = _stocksService.SearchStocksAsync("abc").Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns null object if exception occurs.")]
        public void TestSearchStocksAsync_Exception()
        {
            // Set up HTTP client result
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Throws(new Exception());

            // Run test
            var result = _stocksService.SearchStocksAsync("abc").Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns valid object if data retrieved successfully.")]
        public void TestGetStockAsync_OK()
        {
            // Create data from external API
            string quoteString = "123.45";
            dynamic externalStockData = new
            {
                name = "Some Company",
                currency = "USD"
            };
            // Create quote HTTP response from external API
            var quoteHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(quoteString, Encoding.UTF8, "application/html")
            };
            // Create stock data HTTP response from external API
            var stockDataHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(externalStockData), Encoding.UTF8, "application/json")
            };
            // Set up HTTP client result
            _httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(quoteHttpResponse)
                .ReturnsAsync(stockDataHttpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var symbol = "abc";
            var result = _stocksService.GetStockAsync(symbol).Result;

            // Verify result
            Assert.AreEqual(quoteString, result.Quote.Price.ToString());
            Assert.AreEqual(externalStockData.currency, result.Quote.Currency.ToString());
            Assert.AreEqual(externalStockData.name, result.Name);
            Assert.AreEqual(symbol, result.Symbol);
        }

        [Test]
        [Description("Test that service returns null object if data source returns unexpected response for first request.")]
        public void TestGetStockAsync_UnexpectedResponse_Request1()
        {
            // Create quote HTTP response from external API
            var quoteHttpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            // Set up HTTP client result
            _httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(quoteHttpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var result = _stocksService.GetStockAsync("abc").Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns null object if data source returns unexpected response for second request.")]
        public void TestGetStockAsync_UnexpectedResponse_Request2()
        {
            // Create quote HTTP response from external API
            var quoteHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("123.45", Encoding.UTF8, "application/html")
            };
            // Create stock data HTTP response from external API
            var stockDataHttpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            // Set up HTTP client result
            _httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(quoteHttpResponse)
                .ReturnsAsync(stockDataHttpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var result = _stocksService.GetStockAsync("abc").Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns null object if exception occurs.")]
        public void TestGetStockAsync_Exception()
        {
            // Set up HTTP client result
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Throws(new Exception());

            // Run test
            var result = _stocksService.GetStockAsync("abc").Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns valid object if data retrieved successfully.")]
        public void TestGetStockHistoryAsync_OK()
        {
            // Create data from external API
            var externalStockHistory = new dynamic[]
            {
                new
                {
                    date = "2021-03-01",
                    close = "123.45"
                },
                new
                {
                    date = "2021-03-02",
                    close = "234.56"
                }
            };
            dynamic externalStockData = new
            {
                name = "Some Company"
            };

            // Create quote HTTP response from external API
            var stockHistoryHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(externalStockHistory), Encoding.UTF8, "application/html")
            };
            // Create stock data HTTP response from external API
            var stockDataHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(externalStockData), Encoding.UTF8, "application/json")
            };

            // Set up HTTP client result
            _httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(stockHistoryHttpResponse)
                .ReturnsAsync(stockDataHttpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var symbol = "abc";
            var result = _stocksService.GetStockHistoryAsync(symbol, DateRange.FIVEDAY).Result;

            // Verify result
            var stockResult = result.Stock;
            Assert.AreEqual(externalStockData.name, stockResult.Name);
            Assert.AreEqual(symbol, stockResult.Symbol);
            var historyResult = result.PriceHistory;
            Assert.AreEqual(externalStockHistory.Length, historyResult.Count);
            Assert.AreEqual(externalStockHistory[0].date, historyResult[0].Date.ToString("yyyy-MM-dd"));
            Assert.AreEqual(externalStockHistory[0].close, historyResult[0].Price.ToString());
            Assert.AreEqual(StockCurrency.USD, historyResult[0].Currency);
        }

        [Test]
        [Description("Test that service returns null object if data source returns unexpected response for first request.")]
        public void TestGetStockHistoryAsync_UnexpectedResponse_Request1()
        {
            // Create quote HTTP response from external API
            var stockHistoryHttpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            // Set up HTTP client result
            _httpMessageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(stockHistoryHttpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var result = _stocksService.GetStockHistoryAsync("abc", DateRange.FIVEDAY).Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns null object if data source returns unexpected response for second request.")]
        public void TestGetStockHistoryAsync_UnexpectedResponse_Request2()
        {
            // Create quote HTTP response from external API
            var externalStockHistory = new dynamic[]
            {
                new
                {
                    date = "2021-03-01",
                    close = "123.45"
                }
            };
            var stockHistoryHttpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(externalStockHistory), Encoding.UTF8, "application/html")
            };
            var stockDataHttpResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            // Set up HTTP client result
            _httpMessageHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(stockHistoryHttpResponse)
                .ReturnsAsync(stockDataHttpResponse);
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Returns(_httpClient.Object);

            // Run test
            var result = _stocksService.GetStockHistoryAsync("abc", DateRange.FIVEDAY).Result;

            // Verify result
            Assert.IsNull(result);
        }

        [Test]
        [Description("Test that service returns null object if exception occurs.")]
        public void TestGetStockHistoryAsync_Exception()
        {
            // Set up HTTP client result
            _httpClientFactory.Setup(a => a.CreateClient(It.IsAny<string>())).Throws(new Exception());

            // Run test
            var result = _stocksService.GetStockHistoryAsync("abc", DateRange.FIVEDAY).Result;

            // Verify result
            Assert.IsNull(result);
        }
    }
}
