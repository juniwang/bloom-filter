using BloomFilter.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Counting Bloom Filter based on an Array of integer which holds the number been hit.
    /// </summary>
    /// <typeparam name="TEntity">type of the entity to add/query in bloom filter.
    /// It must implement IHasKey interface,
    /// </typeparam>
    /// <typeparam name="TKey">type of the key(sth like unique identifier of the TEntity) to generate hash. 
    /// It contains a ToBytes method to generate raw not-hashed byte array.</typeparam>
    class CountingBloomFilter<TEntity, TKey> : AbstractBloomFilter<TEntity, TKey>
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        private int[] counters;

        /// <summary>
        /// Create new instance using BloomFilterBuilder.
        /// </summary>
        /// <param name="config">instance of BloomFilterBuilder.</param>
        public CountingBloomFilter(BloomFilterBuilder<TEntity, TKey> config)
            : base(config)
        {
            counters = new int[config.Size];
        }

        /// <summary>
        /// Add a raw not-hashed elements to BF.
        /// </summary>
        /// <param name="elements">the raw bytes before hash, should be generated from the entity's key</param>
        /// <returns>true if newly added to BF, false if it's already contained.</returns>
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

        /// <summary>
        /// To determine that the raw not-hashed elements is contained in BloomFilter or not.
        /// </summary>
        /// <param name="elements">the raw bytes before hash, should be generated from the entity's key</param>
        /// <returns>True if it's contained in BF otherwise not.</returns>
        protected override bool ContainsRaw(byte[] elements)
        {
            foreach (var index in Hash(elements))
            {
                if (counters[index] == 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Remove an entity from the CBF.
        /// </summary>
        /// <param name="key">the key of the entity to remove.</param>
        /// <returns>true if the entity is contained and removed otherwise false.</returns>
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
