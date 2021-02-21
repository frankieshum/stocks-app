using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using StocksApi.Core.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StocksApi.Test.CoreTests
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
    }
}
