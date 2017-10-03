﻿using System;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using NServiceBus;
using NServiceBus.Persistence.ServiceFabric;

namespace Back_Stateful
{
    public class EndpointCommunicationListener :
        ICommunicationListener
    {
        EndpointConfiguration endpointConfiguration;
        private StatefulServiceContext context;
        private IEndpointInstance endpointInstance;
        private IReliableStateManager stateManager;

        public EndpointCommunicationListener(StatefulServiceContext context, IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.context = context;
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            endpointConfiguration = new EndpointConfiguration("back-stateful");

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.EnableInstallers();

            var persistence = endpointConfiguration.UsePersistence<ServiceFabricPersistence>();
            persistence.StateManager(stateManager);

            var recoverability = endpointConfiguration.Recoverability();
            recoverability.DisableLegacyRetriesSatellite();
            // for demo purposes
            recoverability.Immediate(d => d.NumberOfRetries(5));
            recoverability.Delayed(d => d.NumberOfRetries(0));

            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var connectionString = configurationPackage.Settings.Sections["NServiceBus"].Parameters["ConnectionString"];

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            if (string.IsNullOrWhiteSpace(connectionString.Value))
            {
                throw new Exception("Could not read the 'NServiceBus.ConnectionString'. Check the sample prerequisites.");
            }
            transport.ConnectionString(connectionString.Value);
            var delayedDelivery = transport.DelayedDelivery();
            delayedDelivery.DisableTimeoutManager();

            return Task.FromResult(default(string));
        }

        public async Task Run()
        {
            if (endpointConfiguration == null)
            {
                var message =
                    $"{nameof(EndpointCommunicationListener)} Run() method should be invoked after communication listener has been opened and not before.";

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
    }
}