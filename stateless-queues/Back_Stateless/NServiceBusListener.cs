using System;
using System.Data.SqlClient;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NServiceBus;
using NServiceBus.Persistence.Sql;

namespace Back_Stateless
{
    // TODO: 2.9
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
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();

            #region Not Important

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.DisableLegacyRetriesSatellite();
            // for demo purposes
            recoverability.Immediate(d => d.NumberOfRetries(5));
            recoverability.Delayed(d => d.NumberOfRetries(0));

            var transportConnectionString = context.GetTransportConnectionString();

            transport.ConnectionString(transportConnectionString);

            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            var sqlServerConnectionString = context.GetDbConnectionString();

            var builder = new DbContextOptionsBuilder<OrderContext>();
            builder.UseSqlServer(sqlServerConnectionString);
            endpointConfiguration.RegisterComponents(c => c.ConfigureComponent(() => new OrderContext(builder.Options), DependencyLifecycle.InstancePerUnitOfWork));
            
            persistence.SqlVariant(SqlVariant.MsSqlServer);
            persistence.ConnectionBuilder(() => new SqlConnection(sqlServerConnectionString));
            persistence.Schema("dbo");
            persistence.TablePrefix("");

            #endregion

            return Task.FromResult(default(string));
        }

        public async Task Run()
        {
            #region Not Important

            if (endpointConfiguration == null)
            {
                var message =
                    $"{nameof(NServiceBusListener)} Run() method should be invoked after communication listener has been opened and not before.";

                throw new Exception(message);
            }

                #endregion

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