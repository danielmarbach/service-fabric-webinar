using System;
using System.Fabric;
using Messages_Stateless;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Front_Stateless
{
    public static class NServiceBusServiceCollectionExtensions
    {
        public static void AddNServiceBus(this IServiceCollection services)
        {
            var endpointConfiguration = new EndpointConfiguration("front-stateless");

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendOnly();

            var provider = services.BuildServiceProvider();
            var context = provider.GetService<StatelessServiceContext>();
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var connectionString = configurationPackage.Settings.Sections["NServiceBus"].Parameters["ConnectionString"];

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            if (string.IsNullOrWhiteSpace(connectionString.Value))
            {
                throw new Exception("Could not read the 'NServiceBus.ConnectionString' environment variable. Check the sample prerequisites.");
            }
            transport.ConnectionString(connectionString.Value);
            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SubmitOrder), "back-stateless");

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }
}