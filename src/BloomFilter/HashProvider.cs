using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    class HashProvider
    {
        private static readonly int seed32 = 89478583;

        #region HashCarterWegman
        /// <summary>
        /// hash a byte array
        /// </summary>
        /// <param name="a">the byte array to be hashed</param>
        /// <returns>the 32 bit integer hash value</returns>
        static int HashBytes(byte[] a)
        {
            // 32 bit FNV constants. Using longs as Java does not support unsigned
            // datatypes.
            long FNV_PRIME = 16777619;
            long FNV_OFFSET_BASIS = 2166136261L;

            if (a == null)
                return 0;

            long result = FNV_OFFSET_BASIS;
            foreach (byte element in a)
            {
                result = (result * FNV_PRIME) & 0xFFFFFFFF;
                result ^= element;
            }

            return (int)result;
        }

        /// <summary>
        /// Hash with Carter Wegman methdod: https://en.wikipedia.org/wiki/Universal_hashing#Hashing_integers
        /// </summary>
        /// <param name="value">the value to be hashed</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <returns>array with <i>hashes</i> integer hash positions in the range <i>[0,size)</i></returns>
        public static int[] HashCarterWegman(byte[] value, int m, int k)
        {
            int[] positions = new int[k];
            BigInteger prime32 = 4294967279L;
            BigInteger prime64 = 53200200938189L;
            BigInteger prime128 = BigInteger.Parse("21213943449988109084994671");
            Random r = new Random(seed32);
            BigInteger v = new BigInteger(value.Length > 0 ? value : new byte[1]);

            for (int i = 0; i < k; i++)
            {
                BigInteger a = r.Next();
                BigInteger b = r.Next();
                positions[i] = (int)((a * v + b) % prime64 % m);
            }
            return positions;
        }
        #endregion

        #region HashRNG
        /// <summary>
        /// Hash with RNG(Random Number Generator)
        /// </summary>
        /// <param name="value">the value to be hashed</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <returns>array with <i>hashes</i> integer hash positions in the range <i>[0,size)</i></returns>
        public static int[] HashRNG(byte[] value, int m, int k)
        {
            int[] positions = new int[k];
            Random r = new Random(HashBytes(value));
            for (int i = 0; i < k; i++)
            {
                positions[i] = r.Next(m);
            }
            return positions;
        }

        #endregion

        #region HashSimpleLCG
        /// <summary>
        /// Hash with simple LCG(Linear Congruential Generator)
        /// </summary>
        /// <param name="value">the value to be hashed</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <returns>array with <i>hashes</i> integer hash positions in the range <i>[0,size)</i></returns>
        public static int[] HashSimpleLCG(byte[] value, int m, int k)
        {
            // Java constants
            long multiplier = 0x5DEECE66DL;
            long addend = 0xBL;
            long mask = (1L << 48) - 1;

            // Generate int from byte Array using the FNV hash
            int reduced = Math.Abs(HashBytes(value));
            // Make number positive
            // Handle the special case: smallest negative number is itself as the
            // absolute value
            if (reduced == int.MinValue)
                reduced = 42;

            // Calculate hashes numbers iteratively
            int[] positions = new int[k];
            long seed = reduced;
            for (int i = 0; i < k; i++)
            {
                // LCG formula: x_i+1 = (multiplier * x_i + addend) mod mask
                seed = Math.Abs((seed * multiplier + addend) & mask);
                positions[i] = (int)((uint)seed >> (48 - 30)) % m;
            }
            return positions;
        }
        #endregion

        #region HashMurmur3 && HashCanssandra
        /// <summary>
        /// Compute hash using murmur3: https://en.wikipedia.org/wiki/MurmurHash#MurmurHash3
        /// </summary>
        /// <param name="value">the value to be hashed</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <returns>array with <i>hashes</i> integer hash positions in the range <i>[0,size)</i></returns>
        public static int[] HashMurmur3(byte[] value, int m, int k)
        {
            return RejectionSample(murmur3_signed, value, m, k);
        }

        public static int[] HashCassandra(byte[] value, int m, int k)
        {
            int[] result = new int[k];
            long hash1 = murmur3(0, value);
            long hash2 = murmur3((int)hash1, value);
            for (int i = 0; i < k; i++)
            {
                result[i] = Math.Abs((int)(hash1 + i * hash2) % m);
            }
            return result;
        }

        private static long murmur3(int seed, byte[] bytes)
        {
            return murmur3_signed(seed, bytes);
        }

        private static int murmur3_signed(int seed, byte[] bytes)
        {
            int h1 = seed;
            int c1 = unchecked((int)0xcc9e2d51);
            int c2 = 0x1b873593;
            int len = bytes.Length;
            int i = 0;

            while (len >= 4)
            {
                //process()
                int t1 = (bytes[i++] & 0xFF);
                t1 |= (bytes[i++] & 0xFF) << 8;
                t1 |= (bytes[i++] & 0xFF) << 16;
                t1 |= (bytes[i++] & 0xFF) << 24;

                t1 *= c1;
                t1 = t1.RotateLeft(15);
                t1 *= c2;

                h1 ^= t1;
                h1 = h1.RotateLeft(13);
                h1 = (int)(h1 * 5 + 0xe6546b64);

                len -= 4;
            }

            //processingRemaining()
            int k1 = 0;
            int nlen = len;
            if (nlen == 3)
            {
                k1 ^= (bytes[i + 2] & 0xFF) << 16;
                nlen = nlen - 1;
            }
            if (nlen == 2)
            {
                k1 ^= (bytes[i + 1] & 0xFF) << 8;
                nlen = nlen - 1;
            }
            if (nlen == 1)
            {
                k1 ^= (bytes[i] & 0xFF);

                k1 *= c1;
                k1 = k1.RotateLeft(15);
                k1 *= c2;
                h1 ^= k1;
            }

            i += len;
            //makeHash()
            h1 ^= i;
            h1 ^= (int)((uint)h1 >> 16);
            h1 *= unchecked((int)0x85ebca6b);
            h1 ^= (int)((uint)h1 >> 13);
            h1 *= unchecked((int)0xc2b2ae35);
            h1 ^= (int)((uint)h1 >> 16);

            return h1;
        }


        // Code taken from:
        // http://dmy999.com/article/50/murmurhash-2-java-port by Derekt
        // Young (Public Domain)
        // as the Hadoop implementation by Andrzej Bialecki is buggy
        public static int[] HashMurmur2(byte[] value, int em, int ka)
        {
            int[] positions = new int[ka];

            int hashes = 0;
            int lastHash = 0;
            byte[] data = (byte[])value.Clone();
            while (hashes < ka)
            {
                for (int ind = 0; ind < value.Length; ind++)
                {
                    if (data[ind] == 127)
                    {
                        data[ind] = 0;
                        continue;
                    }
                    else
                    {
                        data[ind]++;
                        break;
                    }
                }

                // 'size' and 'r' are mixing constants generated offline.
                // They're not really 'magic', they just happen to work well.
                int m = 0x5bd1e995;
                int r = 24;

                // Initialize the hash to a 'random' value
                int len = data.Length;
                int h = seed32 ^ len;

                int i = 0;
                while (len >= 4)
                {
                    int k = data[i + 0] & 0xFF;
                    k |= (data[i + 1] & 0xFF) << 8;
                    k |= (data[i + 2] & 0xFF) << 16;
                    k |= (data[i + 3] & 0xFF) << 24;

                    k *= m;
                    k ^= (int)((uint)k >> r);
                    k *= m;

                    h *= m;
                    h ^= k;

                    i += 4;
                    len -= 4;
                }

                int tlen = len;
                if (tlen == 3)
                {
                    h ^= (data[i + 2] & 0xFF) << 16;
                    tlen = tlen - 1;
                }
                if (tlen == 2)
                {
                    h ^= (data[i + 1] & 0xFF) << 8;
                    tlen = tlen - 1;
                }
                if (tlen == 1)
                {
                    h ^= (data[i + 0] & 0xFF);
                    h *= m;
                }

                h ^= (int)((uint)h >> 13);
                h *= m;
                h ^= (int)((uint)h >> 15);

                lastHash = RejectionSample(h, em);
                if (lastHash != -1)
                {
                    positions[hashes++] = lastHash;
                }
            }
            return positions;
        }

        /// <summary>
        /// Performs rejection sampling on a random 32bit int (sampled from int.MinValue to int.MaxValue).
        /// </summary>
        /// <param name="random">int</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <returns>the number down-sampled to interval [0, size]. Or -1 if it has to be rejected.</returns>
        private static int RejectionSample(int random, int m)
        {
            random = Math.Abs(random);
            if (random > (2147483647 - 2147483647 % m) || random == int.MinValue)
                return -1;
            else
                return random % m;
        }

        private static int[] RejectionSample(Func<int, byte[], int> hashFunction, byte[] value, int m, int k)
        {
            int[] hashes = new int[k];
            int seed = 0;
            int pos = 0;
            while (pos < k)
            {
                seed = hashFunction(seed, value);
                int hash = RejectionSample(seed, m);
                if (hash != -1)
                {
                    hashes[pos++] = hash;
                }
            }
            return hashes;
        }
        #endregion

        #region HashCrypt via HashAlgorithm
        /// <summary>
        /// Encrypt with single hash
        /// </summary>
        /// <param name="value">the value to be hashed</param>
        /// <param name="m">integer output range [1,size]</param>
        /// <param name="k">number of hashes to be computed</param>
        /// <param name="method">the hash method name used by HashAlgorithm</param>
        /// <returns>array with <i>hashes</i> integer hash positions in the range <i>[0,size)</i></returns>
        public static int[] HashCrypt(byte[] value, int m, int k, string method)
        {
            HashAlgorithm hashAlgorithm = HashAlgorithm.Create(method);
            if (hashAlgorithm == null)
                throw new ArgumentOutOfRangeException("No such has `method`");

            int[] positions = new int[k];
            int computedHashes = 0;
            // Add salt to the hash deterministically in order to generate different
            // hashes for each round
            // Alternative: use pseudorandom sequence
            Random r = new Random(seed32);
            byte[] digest = new byte[0];
            while (computedHashes < k)
            {
                digest = hashAlgorithm.ComputeHash(value);
                int chunk = hashAlgorithm.HashSize / 32;
                for (int split = 0; split < chunk && computedHashes < k; split++)
                {
                    int intHash = Math.Abs(BitConverter.ToInt32(digest, split)) % m;
                    positions[computedHashes] = intHash;
                    computedHashes++;
                }
            }

            return positions;
        }
        #endregion
    }
}
