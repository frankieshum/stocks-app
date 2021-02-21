using Microsoft.AspNetCore.Mvc;
using StocksApi.Core.Models;
using StocksApi.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace StocksApi.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StocksController : ControllerBase
    {
        private readonly IStocksService _stocksService;

        public StocksController(IStocksService stocksService)
        {
            _stocksService = stocksService;
        }

        /// <summary>
        /// Returns a list of stocks matching the search query.
        /// </summary>
        /// <param name="query">Query to filter stocks by symbol or name.</param>
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> SearchStocks([FromQuery, Required] string query)
        {
            List<Stock> serviceResult = await _stocksService.SearchStocksAsync(query);

            if (serviceResult == null)
                return InternalServerErrorResult();

            return Ok(serviceResult);
        }

        /// <summary>
        /// Returns a stock for the specified symbol.
        /// </summary>
        /// <param name="symbol">Ticker symbol of the stock.</param>
        [HttpGet("{symbol}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStock([FromRoute] string symbol)
        {
            Stock serviceResult = await _stocksService.GetStockAsync(symbol);

            if (serviceResult == null)
                return InternalServerErrorResult();

            return Ok(serviceResult);
        }

        /// <summary>
        /// Returns the price history for the specified stock and period.
        /// </summary>
        /// <param name="symbol">Ticker symbol of the stock.</param>
        /// <param name="startDate">Start date for price history period.</param>
        /// <param name="endDate">End date for price history period.</param>
        [HttpGet("{symbol}/history")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetStockHistory([FromRoute] string symbol, [FromQuery, Required] DateTime startDate, [FromQuery, Required] DateTime endDate)
        {
            StockHistory serviceResult = await _stocksService.GetStockHistoryAsync(symbol, startDate, endDate);

            if (serviceResult == null)
                return InternalServerErrorResult();

            return Ok(serviceResult);
        }

        /// <summary>
        /// Creates an object result for InternalServerError HTTP response.
        /// </summary>
        private ObjectResult InternalServerErrorResult()
        {
            var details = new ProblemDetails() { 
                Status = 500,
                Title = "Internal Server Error",
                Detail = "Something went wrong. Please try again!"
            };
            return StatusCode(500, details);
        }

        /// <summary>
        /// Creates an object result for NotFound HTTP response.
        /// </summary>
        private ObjectResult NotFoundResult()
        {
            var details = new ProblemDetails() { 
                Status = 404,
                Title = "Not Found",
                Detail = "The requested resource was not found."
            };
            return NotFound(details);
        }
    }
}
