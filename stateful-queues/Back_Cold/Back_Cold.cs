using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Back_Cold
{
    internal sealed class Back_Cold : StatelessService
    {
        public Back_Cold(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            listener = new NServiceBusListener(Context);
            return new List<ServiceInstanceListener>
            {
                new ServiceInstanceListener(context => listener, "NServiceBusListener")
            };
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            return listener.Run();
        }

        NServiceBusListener listener;
    }
}
