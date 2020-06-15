using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsulServiceRegistration
{
    public static class RegistrationExtensions
    {
        public static void AddConsul(this IServiceCollection services)
        {
            var config = new ConfigurationBuilder().AddJsonFile("service.config.json").Build();
            services.Configure<ConsulServiceOptions>(config);
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

            var serviceOptions = app.ApplicationServices.GetRequiredService<IOptions<ConsulServiceOptions>>().Value;

            serviceOptions.ServiceId = Guid.NewGuid().ToString();

            var consulClient = new ConsulClient(configuration => { configuration.Address = new Uri(serviceOptions.ConsulAddress); });

            var features = app.Properties["server.Features"] as FeatureCollection;
            var address = features?.Get<IServerAddressesFeature>().Addresses.First();

            var uri = new Uri(address);

            var registration = new AgentServiceRegistration()
            {
                ID = serviceOptions.ServiceId,
                Name = serviceOptions.ServiceName,
                Address = uri.Host,

                Port = uri.Port,
                Check = new AgentServiceCheck
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                    HTTP = $"{uri.Scheme}://{uri.Host}:{uri.Port}{serviceOptions.HealthCheck}",
                    Interval = TimeSpan.FromSeconds(10),
                }
            };

            consulClient.Agent.ServiceRegister(registration).Wait();
            lifetime.ApplicationStopping.Register(() => { consulClient.Agent.ServiceDeregister(serviceOptions.ServiceId).Wait(); });

            return app;
        }

    }
}
