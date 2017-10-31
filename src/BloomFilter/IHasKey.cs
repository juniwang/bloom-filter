using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Instance should define its key
    /// </summary>
    /// <typeparam name="TKey">The type of its key.</typeparam>
    interface IHasKey<TKey>
    {
        TKey GetKey();
    }
}
