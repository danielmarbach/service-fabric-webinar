using System.Fabric;
using Messages_Stateless;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;

namespace Front_Stateless
{
    public static class NServiceBusServiceCollectionExtensions
    {
        // TODO: 2.3
        public static void AddNServiceBus(this IServiceCollection services)
        {
            var endpointConfiguration = new EndpointConfiguration("front-stateless");
            endpointConfiguration.SendOnly();
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();

            #region Not Important

            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();
            var provider = services.BuildServiceProvider();
            var context = provider.GetService<StatelessServiceContext>();
            var connectionString = context.GetTransportConnectionString();

            transport.ConnectionString(connectionString);
            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            #endregion
            
            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(SubmitOrder), "back-stateless");

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }
}