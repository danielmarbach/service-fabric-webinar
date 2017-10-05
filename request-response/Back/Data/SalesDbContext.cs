using System.Threading;
using Back.Model;

namespace Back.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class SalesDbContext : IDisposable
    {
        static long invokeCount = 0;
        static Random random = new Random();
        static List<Action> saveActions = new List<Action>();

        public DbSet<Order> Orders { get; } = new DbSet<Order>(saveActions.Add);

        public async Task SaveChangesAsync()
        {
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);
            if (Interlocked.Increment(ref invokeCount) % 2 == 0)
            {
                throw new Exception(
                    $"There was a problem creating your order. Please try again or contact sales support if you are still unable to submit your order and reference tracking code {Guid.NewGuid()}");
            }
            saveActions.ForEach(a => a());
        }

        public void Dispose()
        {
            saveActions.Clear();
        }
    }
}