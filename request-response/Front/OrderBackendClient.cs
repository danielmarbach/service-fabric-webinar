using System;
using System.Fabric;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace Front
{
    public class OrderBackendClient : ICommunicationClient
    {
        public OrderBackendClient(HttpClient client, string address)
        {
            this.HttpClient = client;
            this.Url = new Uri(address);
        }

        HttpClient HttpClient { get; }

        Uri Url { get; }

        ResolvedServiceEndpoint ICommunicationClient.Endpoint { get; set; }

        string ICommunicationClient.ListenerName { get; set; }

        ResolvedServicePartition ICommunicationClient.ResolvedServicePartition { get; set; }

        public async Task<string> Order(int orderId)
        {
            using (var content = new StringContent(string.Empty))
            {
                var response = await HttpClient.PutAsync(new Uri(Url, $"api/orders/{orderId}"), content)
                    .ConfigureAwait(false);
                return await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);
            }
        }
    }
}