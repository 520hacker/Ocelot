using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Ocelot.Requester
{
    public static class HttpExtensions
    {
        internal const string NAME = "ocelot.client";

        /// <summary>
        /// gzip enabled?
        /// </summary>
        public const bool DEFAULT_GZIP = true;

        /// <summary>
        /// default Agent name
        /// </summary>
        public const string DEFAULT_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.133 Safari/537.36";


        /// <summary>
        /// HttpClient DI settings by name
        /// </summary>
        public static void AddOcelotHttpClient(this IServiceCollection services,
            string name= NAME, bool gzipEnabled = DEFAULT_GZIP, string userAgent = DEFAULT_AGENT, WebProxy proxy = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(nameof(name));
            }

            services.AddHttpClient(name,                
                client => client.DefaultRequestHeaders.Add("User-Agent", userAgent))
                .ConfigureHttpMessageHandlerBuilder(config => new HttpClientHandler
                {
                    AutomaticDecompression = gzipEnabled ? DecompressionMethods.GZip : DecompressionMethods.None,
                    UseProxy = proxy != null
                });
        }
     
    }
}
