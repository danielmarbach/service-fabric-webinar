using System;
using System.Fabric;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Front_Stateless
{
    public static class NServiceBusServiceCollectionExtensions
    {
        public static void AddNServiceBus(this IServiceCollection services)
        {
            var endpointConfiguration = new EndpointConfiguration("front-stateless");

            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
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
            routing.RouteToEndpoint(typeof(Order), "back-stateless");

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }

    // will be moved into dedicated assembly
    public class Order : ICommand
    {
        public Order(int orderNumber)
        {
        }

        public int OrderNumber { get; private set; }
    }
}