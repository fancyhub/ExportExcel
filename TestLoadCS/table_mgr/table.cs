using System;
using System.Collections;
using System.Collections.Generic;


namespace Test
{
    public sealed class Table
    {
        public readonly IList List;
        public readonly Type DataType;

        public readonly Type KeyType;
        public readonly IDictionary Dict;

        public static Table Create<T>(List<T> list) where T : class
        {
            if (list == null)
                return null;
            return new Table(list, typeof(T));
        }

        public static Table Create<TKey, TVal>(List<TVal> list, Dictionary<TKey, TVal> dict) where TVal : class
        {
            if (list == null || dict == null)
                return null;

            return new Table(list, typeof(TVal), dict, typeof(TKey));
        }

        private Table(IList list, Type data_type)
        {
            this.List = list;
            this.DataType = data_type;
        }

        private Table(IList list, Type data_type, IDictionary dict, Type key_type)
        {
            this.List = list;
            this.DataType = data_type;
            this.Dict = dict;
            this.KeyType = key_type;
        }

        public static ulong MakeKey(uint k1, uint k2)
        {
            ulong u1 = (uint)k1;
            ulong u2 = (uint)k2;
            return (u1 << 32) | u2;
        }         
    }
}
