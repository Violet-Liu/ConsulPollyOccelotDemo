using System;

namespace ConsulServiceRegistration
{
    public class ConsulServiceOptions
    {
        public string ConsulAddress { get; set; }

        public string ServiceId { get; set; }

        public string ServiceName { get; set; }

        public string HealthCheck { get; set; }
        
    }
}
