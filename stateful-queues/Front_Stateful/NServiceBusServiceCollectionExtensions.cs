using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using Messages_Stateful;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services;
using NServiceBus;

namespace Front_Stateful
{
    public static class NServiceBusServiceCollectionExtensions
    {
        // TODO: 3.8
        public static void AddNServiceBus(this IServiceCollection services)
        {
            var endpointConfiguration = new EndpointConfiguration("front-stateful");

            #region Not Important

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

            #endregion

            var routing = transport.Routing();
            var backStateful = "back-stateful";
            routing.RouteToEndpoint(typeof(SubmitOrder), backStateful);
            routing.RouteToEndpoint(typeof(CancelOrder), backStateful);
            
            var uriBuilder = new ServiceUriBuilder("Back_Stateful");
            var backServiceUri = uriBuilder.Build();

            var partitionInfo = ServicePartitionQueryHelper.QueryServicePartitions(backServiceUri, Guid.Empty).GetAwaiter().GetResult();

            string ConvertOrderIdToPartitionLowKey(Guid orderId)
            {
                var key = CRC64.ToCRC64(orderId.ToByteArray());

                var partition = partitionInfo.Partitions.Single(p => p.LowKey <= key && p.HighKey >= key);

                return partition.LowKey.ToString();
            }

            var senderSideDistribution =
                routing.RegisterPartitionedDestinationEndpoint(backStateful,
                    partitionInfo.Partitions.Select(k => k.LowKey.ToString()).ToArray());

            senderSideDistribution.AddPartitionMappingForMessageType<SubmitOrder>(msg => ConvertOrderIdToPartitionLowKey(msg.OrderId));
            senderSideDistribution.AddPartitionMappingForMessageType<CancelOrder>(msg => ConvertOrderIdToPartitionLowKey(msg.OrderId));

            var endpointInstance = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            services.AddSingleton<IMessageSession>(endpointInstance);
        }
    }
}