using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Back_Stateless.Data
{
    class DbSet<T> : IQueryable<T>
    {
        static ConcurrentDictionary<int, T> items = new ConcurrentDictionary<int, T>();

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
            try
            {
                added.ForEach(i =>
                {
                    dynamic item = i;
                    if (!items.TryAdd(item.Id, i))
                    {
                        throw new Exception("Concurrency violation.");
                    }
                });
            }
            finally
            {
                added.Clear();
            }
        }

        public IEnumerator<T> GetEnumerator() => items.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        public Expression Expression { get; } = items.Values.AsQueryable().Expression;
        public Type ElementType { get; } = items.Values.AsQueryable().ElementType;
        public IQueryProvider Provider { get; } = items.Values.AsQueryable().Provider;
    }
}