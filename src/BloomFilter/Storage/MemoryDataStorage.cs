using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Storage
{
    class MemoryDataStorage<TEntity, TKey> : IDataStorage<TEntity, TKey>
        where TEntity : IHasKey<TKey>
    {
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
