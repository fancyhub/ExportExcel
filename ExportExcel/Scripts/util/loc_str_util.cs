using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public static class LocStrUtil
    {
        public static int ToLocId(this string value)
        {
            unchecked
            {
                // check for degenerate input
                if (string.IsNullOrEmpty(value))
                    return 0;

                int length = value.Length;
                uint hash = (uint)length;

                int remainder = length & 1;
                length >>= 1;

                // main loop
                int index = 0;
                for (; length > 0; length--)
                {
                    hash += value[index];
                    uint temp = (uint)(value[index + 1] << 11) ^ hash;
                    hash = (hash << 16) ^ temp;
                    index += 2;
                    hash += hash >> 11;
                }

                // handle odd string length
                if (remainder == 1)
                {
                    hash += value[index];
                    hash ^= hash << 11;
                    hash += hash >> 17;
                }

                // force "avalanching" of final 127 bits
                hash ^= hash << 3;
                hash += hash >> 5;
                hash ^= hash << 4;
                hash += hash >> 17;
                hash ^= hash << 25;
                hash += hash >> 6;
                return (int)hash;
            }
        }
    }
}
