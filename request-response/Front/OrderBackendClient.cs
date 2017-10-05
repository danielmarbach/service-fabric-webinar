using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Front.Models;
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

        public async Task<IEnumerable<OrderModel>> List()
        {
            var httpResponse = await HttpClient.GetAsync(new Uri(Url, "api/orders/"))
                .ConfigureAwait(false);

            var json = await httpResponse.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<OrderModel>>(json);
        }

        public async Task<CreateOrderResponse> Order(NewOrderRequest request)
        {
            using (var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"))
            {
                var httpResponse = await HttpClient.PutAsync(new Uri(Url, $"api/orders/"), content)
                    .ConfigureAwait(false);

                var json = await httpResponse.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<CreateOrderResponse>(json);
            }
        }

        public class CreateOrderResponse
        {
            public OrderModel NewOrder { get; set; }
            public List<string> Errors { get; set; }
        }
    }
}