using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Intances that can compute its hash
    /// </summary>
    interface IHashable
    {
        /// <summary>
        /// Convert the current object to byte[] for hash computation.
        /// </summary>
        /// <returns>byte array.</returns>
        byte[] ToBytes();
    }
}
