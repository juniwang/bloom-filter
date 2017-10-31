using BloomFilter.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Abstract layer for BloomFilter and Countint Bloom Filter to provide some common logic.
    /// </summary>
    /// <typeparam name="TEntity">type of the entity to add/query in bloom filter.
    /// It must implement IHasKey interface,
    /// </typeparam>
    /// <typeparam name="TKey">type of the key(sth like unique identifier of the TEntity) to generate hash. 
    /// It contains a ToBytes method to generate raw not-hashed byte array.</typeparam>
    abstract class AbstractBloomFilter<TEntity, TKey>
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        protected BloomFilterBuilder<TEntity, TKey> config;

        protected AbstractBloomFilter(BloomFilterBuilder<TEntity, TKey> config)
        {
            this.config = config;
        }

        /// <summary>
        /// To determine that the raw not-hashed elements is contained in BloomFilter or not.
        /// </summary>
        /// <param name="elements">the raw bytes before hash, should be generated from the entity's key</param>
        /// <returns>True if it's contained in BF otherwise not.</returns>
        protected abstract bool ContainsRaw(byte[] elements);

        /// <summary>
        /// Add a raw not-hashed elements to BF.
        /// </summary>
        /// <param name="elements">the raw bytes before hash, should be generated from the entity's key</param>
        /// <returns>true if newly added to BF, false if it's already contained.</returns>
        protected abstract bool AddRaw(byte[] elements);

        /// <summary>
        /// Add a new entity to BF. Will use ToBytes method of its Key to generate raw bytes.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <returns>true if newly added to BF, false if it's already contained.</returns>
        public bool Add(TEntity entity)
        {
            bool added = AddRaw(entity.GetKey().ToBytes());
            AddToStorage(entity);
            return added;
        }

        /// <summary>
        /// To determine whether a entity is contained in BF.
        /// </summary>
        /// <param name="entity">The entity to check. Acturally check its Key.</param>
        /// <returns>True if it's contained in BF otherwise not.</returns>
        public bool Contains(TEntity entity)
        {
            return Contains(entity.GetKey());
        }

        /// <summary>
        /// To determine whether a key is contained in BF.
        /// </summary>
        /// <param name="key">the key of the entity.</param>
        /// <returns>True if it's contained in BF otherwise not.</returns>
        public bool Contains(TKey key)
        {
            return ContainsRaw(key.ToBytes());
        }

        /// <summary>
        /// Query the entity by its key from the backend storage. Will check BF first before load it from backend storage.
        /// </summary>
        /// <param name="key">the key of the entity</param>
        /// <returns>instance of type TEntity if exists in both BF and backend storage, otherwise null.</returns>
        public TEntity Get(TKey key)
        {
            if (Contains(key))
            {
                return LoadFromStorage(key);
            }
            return default(TEntity);
        }

        /// <summary>
        /// Compute the hash using configured HashFunction.
        /// </summary>
        /// <param name="elements">the raw data before hashing</param>
        /// <returns>hashed bytes.</returns>
        protected int[] Hash(byte[] elements)
        {
            return config.HashFunction.Probe(elements, config.Size, config.Hashes);
        }

        /// <summary>
        /// Save entity to backend storage
        /// </summary>
        /// <param name="entity">then entity to save.</param>
        protected void AddToStorage(TEntity entity)
        {
            if (config.DataStorage != null)
            {
                config.DataStorage.Save(entity);
            }
        }

        /// <summary>
        /// Read entity from backend storage by its key
        /// </summary>
        /// <param name="key">key of the entity</param>
        /// <returns>instance of type TEntity if exists in backend storage, otherwise null.</returns>
        protected TEntity LoadFromStorage(TKey key)
        {
            if (config.DataStorage != null)
            {
                return config.DataStorage.Load(key);
            }

            return default(TEntity);
        }
    }
}
