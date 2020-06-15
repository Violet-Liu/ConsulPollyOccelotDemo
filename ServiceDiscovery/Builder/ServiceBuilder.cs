using ServiceDiscovery.LoadBalancer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery.Builder
{
    public class ServiceBuilder:IServiceBuilder
    {
        public IConsulServiceProvider ServiceProvider { get; set; }

        public string ServiceName { get; set; }

        public string UriScheme { get; set; }

        public ILoadBalancer LoadBalancer { get; set; }

        public ServiceBuilder(IConsulServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task<Uri> BuildAsync(string path)
        {
            var serviceList = await ServiceProvider.GetServicesAsync(ServiceName);
            var service = LoadBalancer.Resolve(serviceList);
            var baseUri = new Uri($"{UriScheme}://{service}");
            var uri = new Uri(baseUri, path);
            return uri;
        }
    }
}
