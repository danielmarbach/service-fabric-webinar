using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Back
{
    public class OrderContext : DbContext
    {
        private static int invokeCount;

        public OrderContext(DbContextOptions<OrderContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>().Property(b => b.OrderId).ValueGeneratedNever();
        }

        public DbSet<Order> Orders { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);
            if (Interlocked.Increment(ref invokeCount) % 2 == 0)
            {
                throw new Exception(
                    $"There was a problem creating your order. Please try again or contact sales support if you are still unable to submit your order and reference tracking code {Guid.NewGuid()}");
            }
            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        static Random random = new Random();
    }
}