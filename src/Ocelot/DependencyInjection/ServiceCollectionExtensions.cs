﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Ocelot.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IOcelotBuilder AddOcelot(this IServiceCollection services)
        {
            var service = services.First(x => x.ServiceType == typeof(IConfiguration));
            var configuration = (IConfiguration)service.ImplementationInstance;
            services.AddHttpClient();
            return new OcelotBuilder(services, configuration);
        }

        public static IOcelotBuilder AddOcelot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            return new OcelotBuilder(services, configuration);
        }
    }
}
