using BloomFilter.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    abstract class AbstractBloomFilter<TEntity, TKey>
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        protected BloomFilterBuilder<TEntity, TKey> config;

        protected AbstractBloomFilter(BloomFilterBuilder<TEntity, TKey> config)
        {
            this.config = config;
        }

        protected abstract bool ContainsRaw(byte[] elements);
        protected abstract bool AddRaw(byte[] elements);

        public bool Add(TEntity entity)
        {
            bool added = AddRaw(entity.GetKey().ToBytes());
            AddToStorage(entity);
            return added;
        }

        public bool Contains(TEntity entity)
        {
            return Contains(entity.GetKey());
        }

        public bool Contains(TKey key)
        {
            return ContainsRaw(key.ToBytes());
        }

        public TEntity Get(TKey key)
        {
            if (Contains(key))
            {
                return LoadFromStorage(key);
            }
            return default(TEntity);
        }

        protected int[] Hash(byte[] elements)
        {
            return config.HashFunction.Probe(elements, config.Size, config.Hashes);
        }

        protected void AddToStorage(TEntity entity)
        {
            if (config.DataStorage != null)
            {
                config.DataStorage.Save(entity);
            }
        }

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
