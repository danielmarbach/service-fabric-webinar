using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Newtonsoft.Json;

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

        public async Task<OrderResponse> Order(int orderId)
        {
            using (var content = new StringContent(string.Empty))
            {
                var httpResponse = await HttpClient.PutAsync(new Uri(Url, $"api/orders/{orderId}"), content)
                    .ConfigureAwait(false);

                var errorsString = httpResponse.IsSuccessStatusCode ? null : await httpResponse.Content.ReadAsStringAsync();

                return new OrderResponse
                {
                    Success = httpResponse.IsSuccessStatusCode,
                    Errors = httpResponse.IsSuccessStatusCode
                        ? null
                        : JsonConvert.DeserializeObject<Dictionary<string, string[]>>(errorsString)
                };
            }
        }

        public class OrderResponse
        {
            public bool Success { get; set; }
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
}