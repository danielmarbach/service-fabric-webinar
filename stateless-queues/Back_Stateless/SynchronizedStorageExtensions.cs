using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Persistence;

namespace Back_Stateless
{
    // TODO: 2.3
    static class SynchronizedStorageExtensions
    {
        public static OrderContext FromCurrentSession(this SynchronizedStorageSession session)
        {
            var sqlPersistenceSession = session.SqlPersistenceSession();

            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            optionsBuilder.UseSqlServer(sqlPersistenceSession.Connection);
            var context = new OrderContext(optionsBuilder.Options);
            context.Database.UseTransaction(sqlPersistenceSession.Transaction);
            return context;
        }
    }
}