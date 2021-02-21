using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using StocksApi.Core.Models;
using StocksApi.Core.Services.Interfaces;
using StocksApi.Web.Controllers;
using System;
using System.Collections.Generic;

namespace StocksApi.Test
{
    public class StocksControllerTest
    {
        private StocksController _stocksController;
        private Mock<IStocksService> _stocksService;

        [SetUp]
        public void Setup()
        {
            _stocksService = new Mock<IStocksService>();
            _stocksController = new StocksController(_stocksService.Object);
        }

        [Test]
        [Description("Test that controller returns OK if expected object received from service.")]
        public void TestSearchStocks_200()
        {
            // Set up service result
            var serviceResult = new List<Stock>() { new Stock("AAA", "Company A") };
            _stocksService.Setup(a => a.SearchStocksAsync(It.IsAny<string>()))
                .ReturnsAsync(serviceResult);

            // Run test
            var result = _stocksController.SearchStocks("abc").Result;

            // Verify result
            var resultObj = (OkObjectResult)result;
            Assert.AreEqual(serviceResult, resultObj.Value);
        }

        [Test]
        [Description("Test that controller returns InternalServerError if null object received from service.")]
        public void TestSearchStocks_500()
        {
            // Set up service result
            List<Stock> serviceResult = null;
            _stocksService.Setup(a => a.SearchStocksAsync(It.IsAny<string>()))
                .ReturnsAsync(serviceResult);

            // Run test
            var result = _stocksController.SearchStocks("abc").Result;

            // Verify result
            var resultObj = (ObjectResult)result;
            Assert.AreEqual(500, resultObj.StatusCode);
            Assert.IsInstanceOf<ProblemDetails>(resultObj.Value);
        }

        [Test]
        [Description("Test that controller returns OK if expected object received from service.")]
        public void TestGetStock_200()
        {
            // Set up service result
            var serviceResult = new Stock("AAA", "Company A");
            _stocksService.Setup(a => a.GetStockAsync(It.IsAny<string>()))
                .ReturnsAsync(serviceResult);

            // Run test
            var result = _stocksController.GetStock("AAA").Result;

            // Verify result
            var resultObj = (OkObjectResult)result;
            Assert.AreEqual(serviceResult, resultObj.Value);
        }

        [Test]
        [Description("Test that controller returns InternalServerError if null object received from service.")]
        public void TestGetStock_500()
        {
            // Set up service result
            Stock serviceResult = null;
            _stocksService.Setup(a => a.GetStockAsync(It.IsAny<string>()))
                .ReturnsAsync(serviceResult);

            // Run test
            var result = _stocksController.GetStock("AAA").Result;

            // Verify result
            var resultObj = (ObjectResult)result;
            Assert.AreEqual(500, resultObj.StatusCode);
            Assert.IsInstanceOf<ProblemDetails>(resultObj.Value);
        }

        [Test]
        [Description("Test that controller returns OK if expected object received from service.")]
        public void TestGetStockHistory_200()
        {
            // Set up service result
            var serviceResult = new StockHistory() { Stock = new Stock("AAA", "Company A") };
            _stocksService.Setup(a => a.GetStockHistoryAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(serviceResult);

            // Run test
            var result = _stocksController.GetStockHistory("AAA", DateTime.Now, DateTime.Now).Result;

            // Verify result
            var resultObj = (OkObjectResult)result;
            Assert.AreEqual(serviceResult, resultObj.Value);
        }

        [Test]
        [Description("Test that controller returns InternalServerError if null object received from service.")]
        public void TestGetStockHistory_500()
        {
            // Set up service result
            StockHistory serviceResult = null;
            _stocksService.Setup(a => a.GetStockHistoryAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(serviceResult);

            // Run test
            var result = _stocksController.GetStockHistory("AAA", DateTime.Now, DateTime.Now).Result;

            // Verify result
            var resultObj = (ObjectResult)result;
            Assert.AreEqual(500, resultObj.StatusCode);
            Assert.IsInstanceOf<ProblemDetails>(resultObj.Value);
        }
    }
}