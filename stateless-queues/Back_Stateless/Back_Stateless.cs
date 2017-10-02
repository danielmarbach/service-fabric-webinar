using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Back_Stateless
{
    internal sealed class Back_Stateless : StatelessService
    {
        private EndpointCommunicationListener listener;

        public Back_Stateless(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            listener = new EndpointCommunicationListener(Context);
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => listener)
            };
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            return listener.Run();
        }
    }
}
