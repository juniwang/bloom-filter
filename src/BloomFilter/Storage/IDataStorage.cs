using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter.Storage
{
    /// <summary>
    /// Backend data storage, sth like DB, disks, cache and so on.
    /// </summary>
    interface IDataStorage<TEntity, TKey> where TEntity : IHasKey<TKey>
    {
        void Save(TEntity entity);
        TEntity Load(TKey key);
    }
}
