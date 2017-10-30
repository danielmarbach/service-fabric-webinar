using Microsoft.EntityFrameworkCore;
using NServiceBus;
using NServiceBus.Persistence;

namespace Back_Stateless
{
    static class SynchronizedStorageExtensions
    {
        public static OrderContext FromCurrentSession(this SynchronizedStorageSession session)
        {
            var optionsBuilder = new DbContextOptionsBuilder<OrderContext>();
            var sqlPersistenceSession = session.SqlPersistenceSession();
            optionsBuilder.UseSqlServer(sqlPersistenceSession.Connection);
            var context = new OrderContext(optionsBuilder.Options);
            context.Database.UseTransaction(sqlPersistenceSession.Transaction);
            return context;
        }
    }
}