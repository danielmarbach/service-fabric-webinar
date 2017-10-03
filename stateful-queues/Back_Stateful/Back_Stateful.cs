using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Back_Stateful
{
    internal sealed class Back_Stateful : StatefulService
    {
        private EndpointCommunicationListener listener;

        public Back_Stateful(StatefulServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            listener = new EndpointCommunicationListener(Context, StateManager);
            return new List<ServiceReplicaListener>
            {
                new ServiceReplicaListener(context => listener)
            };
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            return listener.Run();
        }
    }
}
