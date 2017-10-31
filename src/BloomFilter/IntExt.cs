using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BloomFilter
{
    /// <summary>
    /// Extension methods for integer
    /// </summary>
    public static class IntExt
    {
        public static int RotateLeft(this int value, int count)
        {
            uint val = (uint)value;
            return (int)((val << count) | (val >> (32 - count)));
        }

        public static int RotateRight(this int value, int count)
        {
            uint val = (uint)value;
            return (int)((value >> count) | (value << (32 - count)));
        }
    }
}
