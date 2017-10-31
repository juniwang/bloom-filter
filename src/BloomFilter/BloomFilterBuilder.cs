using BloomFilter.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Configuration of BF.
    /// </summary>
    /// <typeparam name="TEntity">type of the entity to add/query in bloom filter.
    /// It must implement IHasKey interface,
    /// </typeparam>
    /// <typeparam name="TKey">type of the key(sth like unique identifier of the TEntity) to generate hash. 
    /// It contains a ToBytes method to generate raw not-hashed byte array.</typeparam>
    class BloomFilterBuilder<TEntity, TKey> 
        where TEntity : IHasKey<TKey>
        where TKey : IHashable
    {
        private bool configValidated = false;

        public string Name { get; private set; }
        /// <summary>
        /// The accepable false-positive rate (e.g., 0.01F = 1%)
        /// </summary>
        public double FalsePositiveProbability { get; private set; }
        /// <summary>
        /// Function to compute hash values
        /// </summary>
        public HashFunction HashFunction { get; set; }
        /// <summary>
        /// Number of hash functions.
        /// </summary>
        public int Hashes { get; private set; }
        /// <summary>
        /// The anticipated number of items to be added to the filter. More than this number of items
        /// can be added, but the error rate(false positive probability) will exceed what is expected.
        /// In other words, when Capacity items are added, the FalsePositiveProbability should be around.
        /// e.g. say Capacity=1000,FalsePositiveProbability=0.01, that says the FalsePositiveProbability is
        /// 0.01 when 1000 items were added.
        /// </summary>
        public int Capacity { get; private set; }
        /// <summary>
        /// The size of the BloomFilter bit sets.
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// the backend storage
        /// </summary>
        public IDataStorage<TEntity, TKey> DataStorage { get; private set; }

        #region ctor
        /// <summary>
        /// Create new instance with default values. Must specify either (capacity, falsePositiveProbability) 
        /// or (size, hashes) before create BF instance.
        /// </summary>
        public BloomFilterBuilder()
        {
            Initialize();
        }

        /// <summary>
        ///  Create new instance with (capacity, falsePositiveProbability). Will caculate optimal size and hashes 
        ///  if they are not explicitly defined.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. More than this number of items
        /// can be added, but the error rate(false positive probability) will exceed what is expected.</param>
        /// <param name="falsePositiveProbability">The accepable false-positive rate (e.g., 0.01F = 1%)</param>
        public BloomFilterBuilder(int capacity, double falsePositiveProbability)
        {
            Initialize();
            Capacity = capacity;
            FalsePositiveProbability = falsePositiveProbability;
        }

        /// <summary>
        /// Create new instance with (size, hashes). Will caculate optimal capacity and falsePositiveProbability 
        ///  if they are not explicitly defined.
        /// </summary>
        /// <param name="size">The size of the BloomFilter bit sets.</param>
        /// <param name="hashes">Number of hash functions.</param>
        public BloomFilterBuilder(int size, int hashes)
        {
            Initialize();
            Size = size;
            Hashes = hashes;
        }

        /// <summary>
        /// Set default values for required fields.
        /// </summary>
        private void Initialize()
        {
            Name = "Default";
            HashFunction = HashFunction.SHA256;
            DataStorage = new MemoryDataStorage<TEntity, TKey>();
        }
        #endregion

        /// <summary>
        /// Create a new BloomFilter. Either (capacity, falsePositiveProbability) or (size, hashes) must be set.
        /// </summary>
        /// <returns>intance of BloomFilter</returns>
        public BloomFilter<TEntity, TKey> BuildBloomFilter()
        {
            CheckConfig();
            return new BloomFilter<TEntity, TKey>(this);
        }

        /// <summary>
        /// Create a counting bloom filter. Either (capacity, falsePositiveProbability) or (size, hashes) must be set.
        /// </summary>
        /// <returns>intance of Counting BloomFilter</returns>
        public CountingBloomFilter<TEntity, TKey> BuildCountingBloomFilter()
        {
            CheckConfig();
            return new CountingBloomFilter<TEntity, TKey>(this);
        }

        #region calculate optimal values
        /// <summary>
        /// Make sure capacity, falsePositiveProbability, size, hashes are properly set. Either (capacity, falsePositiveProbability)
        /// or (size, hashes) must be set by caller. 
        /// </summary>
        public void CheckConfig()
        {
            if (configValidated)
                return;

            // size of bit set
            if (Size <= 0 && Capacity > 0 && FalsePositiveProbability > 0)
            {
                Size = BestM(Capacity, FalsePositiveProbability);
            }

            // number of hashes
            if (Hashes <= 0 && Capacity > 0 && Size > 0)
            {
                Hashes = BestK(Capacity, Size);
            }

            if (Hashes <= 0 || Size <= 0)
                throw new ArgumentNullException("Neither (capacity, falsePositiveProbability) nor (size, hashes) were specified.");

            // The anticipated number of items to be added to the filter. 
            if (Capacity <= 0)
            {
                Capacity = BestC(Hashes, Size);
            }

            // The accepable false-positive rate (e.g., 0.01F = 1%)
            if (FalsePositiveProbability <= 0)
            {
                FalsePositiveProbability = BestP(Hashes, Size, Capacity);
            }

            configValidated = true;
        }

        /// <summary>
        /// Calculates the optimal size of the bloom filter in bits given Capacity (expected
        /// number of elements in bloom filter) and <i>falsePositiveProbability</i> (tolerable false positive rate).
        /// </summary>
        /// <param name="capacity">Expected number of elements inserted in the bloom filter</param>
        /// <param name="falsPositiveProbability">Tolerable false positive rate</param>
        /// <returns>the optimal size <i>size</i> of the bloom filter in bits</returns>
        public int BestM(int capacity, double falsPositiveProbability)
        {
            return (int)Math.Ceiling(-1 * (capacity * Math.Log(falsPositiveProbability)) / Math.Pow(Math.Log(2), 2));
        }

        /// <summary>
        /// Calculates the optimal <i>hashes</i> (number of hash function) given Capacity (expected number of
        /// elements in bloom filter) and <i>size</i> (size of bloom filter in bits).
        /// </summary>
        /// <param name="capacity">Expected number of elements inserted in the bloom filter</param>
        /// <param name="size">The size of the bloom filter in bits.</param>
        /// <returns>the optimal amount of hash functions hashes</returns>
        public int BestK(int capacity, int size)
        {
            return (int)Math.Ceiling((Math.Log(2) * size) / capacity);
        }

        /// <summary>
        /// Calculates the amount of elements a Bloom filter for which the given configuration of size and hashes is optimal.
        /// </summary>
        /// <param name="hashes">number of hashes</param>
        /// <param name="size">The size of the bloom filter in bits.</param>
        /// <returns>amount of elements a Bloom filter for which the given configuration of size and hashes is optimal.</returns>
        public int BestC(int hashes, int size)
        {
            return (int)Math.Ceiling((Math.Log(2) * size) / hashes);
        }

        /// <summary>
        /// Calculates the best-case (uniform hash function) false positive probability.
        /// </summary>
        /// <param name="hashes">number of hashes</param>
        /// <param name="size">The size of the bloom filter in bits.</param>
        /// <param name="capacity">insertedElements number of elements inserted in the filter</param>
        /// <returns>The calculated false positive probability</returns>
        public double BestP(int hashes, int size, int capacity)
        {
            return Math.Pow((1 - Math.Exp(-hashes * capacity / (double)size)), hashes);
        }
        #endregion

        #region Setter
        /// <summary>
        /// Set the name of BF.
        /// </summary>
        /// <param name="name">name of the BF. Cannot be null or whitespace.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            return this;
        }

        /// <summary>
        /// Set the FalsePositiveProbability.
        /// </summary>
        /// <param name="falsePositiveProbability">the falsePositiveProbability, must be greater than zero.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithFalsePositiveProbability(double falsePositiveProbability)
        {
            if (0 < falsePositiveProbability)
                FalsePositiveProbability = falsePositiveProbability;

            return this;
        }

        /// <summary>
        /// Set the number of hash functions.
        /// </summary>
        /// <param name="hashes">the number of hash functions, must be greater than zero.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithHashes(int hashes)
        {
            if (hashes > 0)
                Hashes = hashes;

            return this;
        }

        /// <summary>
        /// Set The anticipated number of items to be added to the filter.
        /// </summary>
        /// <param name="capacity">The anticipated number of items to be added to the filter. Must be greater than zero.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithCapacity(int capacity)
        {
            if (capacity > 0)
                Capacity = capacity;

            return this;
        }

        /// <summary>
        /// Set the size of the BloomFilter bit sets.
        /// </summary>
        /// <param name="size">The size of the BloomFilter bit sets. Must be greater than zero.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithSize(int size)
        {
            if (size > 0)
                Size = size;

            return this;
        }

        /// <summary>
        /// Set the function to compute hash.
        /// </summary>
        /// <param name="hashFunction">the hash function. Cannot be null.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithHashFunction(HashFunction hashFunction)
        {
            HashFunction = hashFunction ?? throw new ArgumentNullException();
            return this;
        }

        /// <summary>
        /// Set the backend storage.
        /// </summary>
        /// <param name="storage">the storage instance, cannot be null.</param>
        /// <returns></returns>
        public BloomFilterBuilder<TEntity, TKey> WithDataStorage(IDataStorage<TEntity, TKey> storage)
        {
            DataStorage = storage ?? throw new ArgumentNullException();
            return this;
        }
        #endregion
    }
}
