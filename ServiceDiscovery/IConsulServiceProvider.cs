using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDiscovery
{
    public interface IConsulServiceProvider
    {
        Task<IList<string>> GetServicesAsync(string serviceName);
    }
}
