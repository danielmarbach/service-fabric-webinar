using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Messages_Stateful;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NServiceBus;
using NServiceBus.Persistence.ServiceFabric;

namespace Back_Stateful
{
    public class NServiceBusListener :
        ICommunicationListener
    {
        public NServiceBusListener(StatefulServiceContext context, IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.context = context;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            endpointConfiguration = new EndpointConfiguration("back-stateful");

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();

            var partitionInfo =
                await ServicePartitionQueryHelper.QueryServicePartitions(context.ServiceName, context.PartitionId);

            endpointConfiguration.RegisterPartitionsForThisEndpoint(partitionInfo.LocalPartitionKey.Value.ToString(), partitionInfo.Partitions.Select(k => k.LowKey.ToString()).ToArray());

            var persistence = endpointConfiguration.UsePersistence<ServiceFabricPersistence>();
            persistence.StateManager(stateManager);

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.DisableLegacyRetriesSatellite();
            // for demo purposes
            recoverability.Immediate(d => d.NumberOfRetries(5));
            recoverability.Delayed(d => d.NumberOfRetries(0));

            var connectionString = context.GetTransportConnectionString();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString(connectionString);

            var routing = transport.Routing();
            routing.RouteToEndpoint(typeof(UpdateOrderColdStorage), "back-cold");

            string convertOrderIdToPartitionLowKey(Guid orderId)
            {
                var key = orderId.GetHashCode();

                var partition = partitionInfo.Partitions.Single(p => p.LowKey <= key && p.HighKey >= key);

                return partition.LowKey.ToString();
            }

            var recieverSideDistribution = routing.EnableReceiverSideDistribution(partitionInfo.Partitions.Select(k => k.LowKey.ToString()).ToArray());
            recieverSideDistribution.AddPartitionMappingForMessageType<OrderAccepted>(msg => convertOrderIdToPartitionLowKey(msg.OrderId));
            recieverSideDistribution.AddPartitionMappingForMessageType<OrderCanceled>(msg => convertOrderIdToPartitionLowKey(msg.OrderId));
            recieverSideDistribution.AddPartitionMappingForMessageType<OrderCreated>(msg => convertOrderIdToPartitionLowKey(msg.OrderId));

            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            return string.Empty;
        }

        public async Task Run()
        {
            if (endpointConfiguration == null)
            {
                var message =
                    $"{nameof(NServiceBusListener)} Run() method should be invoked after communication listener has been opened and not before.";

                throw new Exception(message);
            }

            endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            return endpointInstance.Stop();
        }

        public void Abort()
        {
            // Fire & Forget Close
            CloseAsync(CancellationToken.None);
        }

        EndpointConfiguration endpointConfiguration;
        StatefulServiceContext context;
        IEndpointInstance endpointInstance;
        IReliableStateManager stateManager;
    }
}