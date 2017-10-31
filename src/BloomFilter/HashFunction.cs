using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    class HashFunction
    {
        /// <param name="value">the byte[] representation of the element to be hashed </param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <returns>int array of hashes hash values</returns>
        Func<byte[], int, int, int[]> hashFunction;

        public HashFunction(Func<byte[], int, int, int[]> hashFunction)
        {
            if (hashFunction == null)
                throw new ArgumentNullException();

            this.hashFunction = hashFunction;
        }

        #region commonly used HashFunctions
        public static HashFunction CarterWegman
        {
            get
            {
                return new HashFunction(HashProvider.HashCarterWegman);
            }
        }

        public static HashFunction RNG
        {
            get
            {
                return new HashFunction(HashProvider.HashRNG);
            }
        }

        public static HashFunction SimpleLCG
        {
            get
            {
                return new HashFunction(HashProvider.HashSimpleLCG);
            }
        }

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

        public static HashFunction MD5
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.MD5.Name));
            }
        }

        public static HashFunction SHA1
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA1.Name));
            }
        }

        public static HashFunction SHA256
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA256.Name));
            }
        }

        public static HashFunction SHA384
        {
            get
            {
                return new HashFunction((v, m, k) => HashProvider.HashCrypt(v, m, k, HashAlgorithmName.SHA384.Name));
            }
        }

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
