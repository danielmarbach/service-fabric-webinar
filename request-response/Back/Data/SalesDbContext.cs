using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using Back.Model;

namespace Back.Data
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    class SalesDbContext : IDisposable
    {
        static int invokeCount = 0;
        static Random random = new Random();
        static List<Action> saveActions = new List<Action>();

        public DbSet<Order> Orders { get; } = new DbSet<Order>(saveActions.Add);

        public Task SaveChangesAsync()
        {
            var task = Task.Delay(random.Next(200, 2000));
            if (invokeCount % 2 == 0)
            {
                task = task.ContinueWith(t => throw new Exception($"There was a problem creating your order. Please try again or contact sales support if you are still unable to submit your order and reference tracking code {Guid.NewGuid()}"));
            }
            else
            {
                saveActions.ForEach(a => a());
            }
            invokeCount++;
            return task;
        }

        public void Dispose()
        {
        }
    }

    #region Fake DbSet
    class DbSet<T> : IQueryable<T>
    {
        static ConcurrentBag<T> items = new ConcurrentBag<T>();

        List<T> added = new List<T>();

        public DbSet(Action<Action> subscriber)
        {
            subscriber(Save);
        }

        public T Add(T item)
        {
            added.Add(item);

            return item;
        }

        void Save()
        {
            added.ForEach(i => items.Add(i));
            added.Clear();
        }

        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        public Expression Expression { get; } = items.AsQueryable().Expression;
        public Type ElementType { get; } = items.AsQueryable().ElementType;
        public IQueryProvider Provider { get; } = items.AsQueryable().Provider;
    }
    #endregion
}