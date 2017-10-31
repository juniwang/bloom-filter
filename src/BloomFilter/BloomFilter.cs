using BloomFilter.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    class BloomFilter<TEntity, TKey> : AbstractBloomFilter<TEntity, TKey>
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        BitArray bloom;

        public BloomFilter(BloomFilterBuilder<TEntity, TKey> config)
            : base(config)
        {
            bloom = new BitArray(config.Size);
        }

        protected override bool ContainsRaw(byte[] elements)
        {
            foreach (var index in Hash(elements))
            {
                if (!bloom.Get(index))
                    return false;
            }

            return true;
        }

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
