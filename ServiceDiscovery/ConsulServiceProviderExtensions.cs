using ServiceDiscovery.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceDiscovery
{
    public static class ConsulServiceProviderExtensions
    {
        public static IServiceBuilder CreateServiceBuilder(this IConsulServiceProvider serviceProvider, Action<IServiceBuilder> config)
        {

            var builder = new ServiceBuilder(serviceProvider);
            config(builder);
            return builder;
        }
    }
}
