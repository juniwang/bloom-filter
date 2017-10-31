using BloomFilter.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Bloom Filter based on BitArray.
    /// </summary>
    /// <typeparam name="TEntity">type of the entity to add/query in bloom filter.
    /// It must implement IHasKey interface,
    /// </typeparam>
    /// <typeparam name="TKey">type of the key(sth like unique identifier of the TEntity) to generate hash. 
    /// It contains a ToBytes method to generate raw not-hashed byte array.</typeparam>
    class BloomFilter<TEntity, TKey> : AbstractBloomFilter<TEntity, TKey>
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        BitArray bloom;

        /// <summary>
        /// Create new instance using BloomFilterBuilder.
        /// </summary>
        /// <param name="config">instance of BloomFilterBuilder.</param>
        public BloomFilter(BloomFilterBuilder<TEntity, TKey> config)
            : base(config)
        {
            bloom = new BitArray(config.Size);
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
                if (!bloom.Get(index))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Add a raw not-hashed elements to BF.
        /// </summary>
        /// <param name="elements">the raw bytes before hash, should be generated from the entity's key</param>
        /// <returns>true if newly added to BF, false if it's already contained.</returns>
        protected override bool AddRaw(byte[] elements)
        {
            bool added = false;
            foreach (var index in Hash(elements))
            {
                if (!bloom.Get(index))
                {
                    bloom.Set(index, true);
                    added = true;
                }
            }
            return added;
        }
    }
}
