using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    interface IHashable
    {
        /// <summary>
        /// Convert to byte[] to compute hash.
        /// </summary>
        /// <returns></returns>
        byte[] ToBytes();
    }
}
