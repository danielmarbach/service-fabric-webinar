using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NServiceBus;

namespace Back_Stateless
{
    public class NServiceBusListener :
        ICommunicationListener
    {
        public NServiceBusListener(StatelessServiceContext context)
        {
            this.context = context;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            endpointConfiguration = new EndpointConfiguration("back-stateless");

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.DisableLegacyRetriesSatellite();
            // for demo purposes
            recoverability.Immediate(d => d.NumberOfRetries(5));
            recoverability.Delayed(d => d.NumberOfRetries(0));

            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            var transportConnectionString = configurationPackage.Settings.Sections["NServiceBus"].Parameters["ConnectionString"];

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            if (string.IsNullOrWhiteSpace(transportConnectionString.Value))
            {
                throw new Exception("Could not read the 'NServiceBus_ConnectionString'. Check the sample prerequisites.");
            }
            transport.ConnectionString(transportConnectionString.Value);
            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            var sqlServerConnectionString = configurationPackage.Settings.Sections["SqlServer"].Parameters["ConnectionString"];
            if (string.IsNullOrWhiteSpace(sqlServerConnectionString.Value))
            {
                throw new Exception("Could not read the 'SqlServer_ConnectionString'. Check the sample prerequisites.");
            }

            var builder = new DbContextOptionsBuilder<OrderContext>();
            builder.UseSqlServer(sqlServerConnectionString.Value);
            endpointConfiguration.RegisterComponents(c => c.ConfigureComponent(() => new OrderContext(builder.Options), DependencyLifecycle.InstancePerUnitOfWork));

            return Task.FromResult(default(string));
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
            return endpointInstance == null ? Task.CompletedTask : endpointInstance.Stop();
        }

        public void Abort()
        {
            // Fire & Forget Close
            CloseAsync(CancellationToken.None);
        }

        EndpointConfiguration endpointConfiguration;
        StatelessServiceContext context;
        IEndpointInstance endpointInstance;
    }
}