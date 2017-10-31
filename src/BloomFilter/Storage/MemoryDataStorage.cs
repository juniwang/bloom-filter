using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Storage
{
    /// <summary>
    /// In-memory storage which store the data in a Dictionary<>.
    /// </summary>
    /// <typeparam name="TEntity">The type of the Values in Dictionary.</typeparam>
    /// <typeparam name="TKey">The type of the Keys in Dictionary.</typeparam>
    class MemoryDataStorage<TEntity, TKey> : IDataStorage<TEntity, TKey>
        where TEntity : IHasKey<TKey>
    {
        /// <summary>
        /// internal in-memory data store.
        /// </summary>
        private Dictionary<TKey, TEntity> dictionary = new Dictionary<TKey, TEntity>();

        public TEntity Load(TKey key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            return default(TEntity);
        }

        public void Save(TEntity entity)
        {
            dictionary[entity.GetKey()] = entity;
        }
    }
}
