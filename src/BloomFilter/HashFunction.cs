using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// A class defines how to compute hash. Also defines some known HashFunctions like MD5 based or SHA1 based function.
    /// </summary>
    class HashFunction
    {
        /// <param name="value">the byte[] representation of the raw element to be hashed </param>
        /// <param name="m">the size of the BF. integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <returns>int array of hashes hash values</returns>
        Func<byte[], int, int, int[]> hashFunction;

        /// <summary>
        /// Create a new HashFunction using the concrete hash method.
        /// </summary>
        /// <param name="hashFunction">concrete hash method to compute hash.</param>
        public HashFunction(Func<byte[], int, int, int[]> hashFunction)
        {
            this.hashFunction = hashFunction ?? throw new ArgumentNullException();
        }

        #region commonly used HashFunctions
        /// <summary>
        /// Hash with Carter Wegman methdod: https://en.wikipedia.org/wiki/Universal_hashing#Hashing_integers
        /// </summary>
        public static HashFunction CarterWegman
        {
            get
            {
                return new HashFunction(HashProvider.HashCarterWegman);
            }
        }

        /// <summary>
        /// Hash with RNG(Random Number Generator)
        /// </summary>
        public static HashFunction RNG
        {
            get
            {
                return new HashFunction(HashProvider.HashRNG);
            }
        }

        /// <summary>
        /// Hash with simple LCG(Linear Congruential Generator)
        /// </summary>
        public static HashFunction SimpleLCG
        {
            get
            {
                return new HashFunction(HashProvider.HashSimpleLCG);
            }
        }

        /// <summary>
        /// Compute hash using murmur3: https://en.wikipedia.org/wiki/MurmurHash#MurmurHash3
        /// </summary>
        public static HashFunction Murmur3
        {
            get
            {
                return new HashFunction(HashProvider.HashMurmur3);
            }
        }

        public static HashFunction Cassandra
        {
            get
            {
                return new HashFunction(HashProvider.HashCassandra);
            }
        }

        /// <summary>
        /// Compute hash using MD5 HashAlgorithm.
        /// </summary>
        public static HashFunction MD5
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.MD5.Name));
            }
        }

        /// <summary>
        /// Compute hash using SHA1 HashAlgorithm.
        /// </summary>
        public static HashFunction SHA1
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA1.Name));
            }
        }

        /// <summary>
        /// Compute hash using SHA256 HashAlgorithm.
        /// </summary>
        public static HashFunction SHA256
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA256.Name));
            }
        }

        /// <summary>
        /// Compute hash using SHA384 HashAlgorithm.
        /// </summary>
        public static HashFunction SHA384
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA384.Name));
            }
        }

        /// <summary>
        /// Compute hash using SHA512 HashAlgorithm.
        /// </summary>
        public static HashFunction SHA512
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA512.Name));
            }
        }
        #endregion

        /// <summary>
        /// Compute the indices
        /// </summary>
        /// <param name="value">the value to be hashed</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <param name="method">the hash method name used by HashAlgorithm</param>
        /// <returns>array with <i>hashes</i> integer hash positions in the range <i>[0,size)</i></returns>
        public int[] Probe(byte[] value, int m, int k)
        {
            return hashFunction(value, m, k);
        }
    }
}
