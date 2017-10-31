using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    class WebCrawlerEntity : IHasKey<StrKey>
    {
        public WebCrawlerEntity(string uri)
        {
            this.Uri = uri;
        }

        public string Uri { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }

        public static implicit operator WebCrawlerEntity(string uri)
        {
            return new WebCrawlerEntity(uri);
        }

        public StrKey GetKey()
        {
            return new StrKey(Uri);
        }
    }

    class StrKey : IHashable
    {
        string key;
        Encoding encoding;

        public StrKey(string key)
            : this(key, Encoding.UTF8)
        {
        }

        public StrKey(string key, Encoding encoding)
        {
            this.key = key;
            this.encoding = encoding;
        }

        public static implicit operator StrKey(int i)
        {
            return new StrKey(i.ToString());
        }

        public static implicit operator StrKey(string s)
        {
            return new StrKey(s);
        }

        public byte[] ToBytes()
        {
            return encoding.GetBytes(key);
        }
    }
}
