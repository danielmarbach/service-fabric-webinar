using System;
using System.Fabric;
using Messages_Stateful;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Front_Stateful
{
    public static class NServiceBusServiceCollectionExtensions
    {
        public static void AddNServiceBus(this IServiceCollection services)
        {
            var endpointConfiguration = new EndpointConfiguration("front-stateful");

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.SendOnly();

            var provider = services.BuildServiceProvider();
            var context = provider.GetService<StatelessServiceContext>();

            var connectionString = context.GetTransportConnectionString();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(connectionString);

            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SubmitOrder), "back-stateful");

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }
}