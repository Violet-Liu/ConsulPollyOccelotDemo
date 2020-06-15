using Consul;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
    public class ConsulServiceProvider : IConsulServiceProvider
    {
        private readonly ConsulClient _consulClient;

        public ConsulServiceProvider(Uri uri)
        {
            _consulClient = new ConsulClient(consulConfig => { consulConfig.Address = uri; });
        }


        public async Task<IList<string>> GetServicesAsync(string serviceName)
        {
            var queryResult = await _consulClient.Health.Service(serviceName, "", true);
            var result = new List<string>();
            foreach (var service in queryResult.Response)
            {
                result.Add(service.Service.Address + ":" + service.Service.Port);
            }
            return result;

        }
    }
}
