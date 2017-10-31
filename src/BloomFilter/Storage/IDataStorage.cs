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
        /// <summary>
        /// save entity to the backend storage
        /// </summary>
        /// <param name="entity">the entity to save.</param>
        void Save(TEntity entity);
        /// <summary>
        /// read entity from the backend storage
        /// </summary>
        /// <param name="key">The key of the entity</param>
        /// <returns>The entity if found otherwise null</returns>
        TEntity Load(TKey key);
    }
}
