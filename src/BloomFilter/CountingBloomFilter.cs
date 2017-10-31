using BloomFilter.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    class CountingBloomFilter<TEntity, TKey> : AbstractBloomFilter<TEntity, TKey>
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        private int[] counters;

        public CountingBloomFilter(BloomFilterBuilder<TEntity, TKey> config)
            : base(config)
        {
            counters = new int[config.Size];
        }

        protected override bool AddRaw(byte[] elements)
        {
            lock (counters)
            {
                foreach (var index in Hash(elements))
                {
                    counters[index] = counters[index] + 1;
                }
            }
            return true;
        }

        protected override bool ContainsRaw(byte[] elements)
        {
            foreach (var index in Hash(elements))
            {
                if (counters[index] == 0)
                    return false;
            }
            return true;
        }

        public bool Remove(TKey key)
        {
            bool removed = false;
            lock (counters)
            {
                int[] hashes = Hash(key.ToBytes());
                // make sure it exists
                foreach (var index in hashes)
                {
                    if (counters[index] == 0)
                        return false;
                }
                // decrement
                foreach (var index in hashes)
                {
                    counters[index] = counters[index] - 1;
                }
                removed = true;
            }
            return removed;
        }
    }
}
