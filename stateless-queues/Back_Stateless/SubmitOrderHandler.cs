using System.Threading.Tasks;
using Messages_Stateless;
using NServiceBus;

namespace Back_Stateless
{
    public class SubmitOrderHandler : IHandleMessages<SubmitOrder>
    {
        public Task Handle(SubmitOrder message, IMessageHandlerContext context)
        {
            ServiceEventSource.Current.Write(nameof(SubmitOrder), message);
            return Task.CompletedTask;
        }
    }
}