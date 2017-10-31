using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bloom Filter:");
            var bf = new BloomFilterBuilder<WebCrawlerEntity, StrKey>(300, 0.01)
                .WithHashFunction(HashFunction.Murmur3)
                .BuildBloomFilter();
            for (int i = 0; i < 300; i++)
            {
                bf.Add(new WebCrawlerEntity(i.ToString()));
            }
            for (int i = 300; i < 1000; i++)
            {
                if (bf.Contains(i))
                    Console.WriteLine("False positive: " + i.ToString());
            }

            Console.WriteLine();
            Console.WriteLine("Counting Bloom Filter:");
            var cbf = new BloomFilterBuilder<WebCrawlerEntity, StrKey>(300, 0.01)
                .WithHashFunction(HashFunction.Murmur3)
                .BuildCountingBloomFilter();
            for (int i = 0; i < 300; i++)
            {
                cbf.Add(new WebCrawlerEntity(i.ToString()));
            }
            //Console.WriteLine(bf.Contains("5"));
            for (int i = 300; i < 1000; i++)
            {
                if (cbf.Contains(i))
                    Console.WriteLine("False positive: " + i.ToString());
            }
            if (cbf.Remove(0))
            {
                Console.WriteLine("0 Removed, probe result:" + cbf.Contains(0));
            }
            Console.WriteLine("try removing 0 again:" + cbf.Remove(0));
            Console.WriteLine("try removing 888:" + cbf.Remove(888));

            Console.ReadLine();
        }
    }
}
