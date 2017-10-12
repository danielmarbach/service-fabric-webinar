using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

public static class ServicePartitionQueryHelper
{
    public static async Task<PartitionsInformation> QueryServicePartitions(Uri serviceName, Guid partitionId)
    {
        using (var client = new FabricClient())
        {
            var servicePartitionList = await client.QueryManager.GetPartitionListAsync(serviceName)
                .ConfigureAwait(false);

            var partitionInformations = servicePartitionList.Select(p => p.PartitionInformation)
                .Cast<Int64RangePartitionInformation>().ToList();

            return new PartitionsInformation
            {
                LocalPartitionKey = partitionInformations.SingleOrDefault(p => p.Id == partitionId)?.LowKey,
                Partitions = partitionInformations.ToDictionary(p => p.LowKey, p => p.HighKey)
            };
        }
    }
}

public class PartitionsInformation
{
    public long? LocalPartitionKey { get; set; }
    public Dictionary<long, long> Partitions { get; set; }
}