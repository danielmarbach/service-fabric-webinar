using System.Threading.Tasks;
using Messages_Stateless;
using NServiceBus;

namespace Back_Stateless
{
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            await Task.Delay(2000).ConfigureAwait(false);
            ServiceEventSource.Current.Write(nameof(SubmitOrder), message);
        }
    }
}