using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Front
{
    public class OrderBackendClientFactory : CommunicationClientFactoryBase<OrderBackendClient>
    {
        private HttpClient httpClient = new HttpClient();

        public OrderBackendClientFactory(IServicePartitionResolver resolver = null, IEnumerable<IExceptionHandler> exceptionHandlers = null)
            : base(resolver, CreateExceptionHandlers(exceptionHandlers))
        {
        }

        protected override void AbortClient(OrderBackendClient client)
        {
            // client with persistent connections should be abort their connections here.
            // HTTP clients don't hold persistent connections, so no action is taken.
        }

        protected override Task<OrderBackendClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
        {
            // clients that maintain persistent connections to a service should 
            // create that connection here.
            // an HTTP client doesn't maintain a persistent connection.
            return Task.FromResult(new OrderBackendClient(this.httpClient, endpoint));
        }

        protected override bool ValidateClient(OrderBackendClient client)
        {
            // client with persistent connections should be validated here.
            // HTTP clients don't hold persistent connections, so no validation needs to be done.
            return true;
        }

        protected override bool ValidateClient(string endpoint, OrderBackendClient client)
        {
            // client with persistent connections should be validated here.
            // HTTP clients don't hold persistent connections, so no validation needs to be done.
            return true;
        }

        private static IEnumerable<IExceptionHandler> CreateExceptionHandlers(IEnumerable<IExceptionHandler> additionalHandlers)
        {
            return new[] { new HttpExceptionHandler() }.Union(additionalHandlers ?? Enumerable.Empty<IExceptionHandler>());
        }
    }
}