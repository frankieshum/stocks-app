using Microsoft.Extensions.DependencyInjection;
using System;

namespace StocksApi.Web.Dependencies
{
    public static class ConfigureServicesExtensions
    {
        /// <summary>
        /// Adds named HttpClients for each external API.
        /// </summary>
        public static void AddHttpClients(this IServiceCollection services)
        {
            services.AddHttpClient("iex", c =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("IEX_API_BASE_URL"));
            });
            services.AddHttpClient("finnhub", c =>
            {
                c.BaseAddress = new Uri(Environment.GetEnvironmentVariable("FINNHUB_API_BASE_URL"));
            });
        }
    }
}
