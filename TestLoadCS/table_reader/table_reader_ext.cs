using System;
using System.Collections.Generic;
using System.Data;
using Test;

namespace TestLoadCs.table_reader
{
    public static class TableReaderExt
    {
        static TableReaderExt()
        {
            EnumConverterMgr.RegFunc((v) => (EItemType)v, (v) => (int)v);
            EnumConverterMgr.RegFunc((v) => (EItemSubType)v, (v) => (int)v);
            EnumConverterMgr.RegFunc((v) => (EItemQuality)v, (v) => (int)v);
        }

        #region Base
        public static void ExRead(this ITableDataReader self, ref bool v)
        {
            v = self.ReadBool();
        }
        public static void ExRead(this ITableDataReader self, ref int v)
        {
            v = self.ReadInt32();
        }
        public static void ExRead(this ITableDataReader self, ref uint v)
        {
            v = self.ReadUInt32();
        }
        public static void ExRead(this ITableDataReader self, ref long v)
        {
            v = self.ReadInt64();
        }
        public static void ExRead(this ITableDataReader self, ref ulong v)
        {
            v = self.ReadUInt64();
        }
        public static void ExRead(this ITableDataReader self, ref float v)
        {
            v = self.ReadF32();
        }
        public static void ExRead(this ITableDataReader self, ref double v)
        {
            v = self.ReadF64();
        }
        public static void ExRead(this ITableDataReader self, ref string v)
        {
            v = self.ReadString();
        }
        public static void ExRead(this ITableDataReader self, ref LocStr v)
        {
            string s = self.ReadString();
            v = s;
        }
        public static void ExRead(this ITableDataReader self, ref LocId v)
        {
            v = self.ReadInt32();
        }
        public static void ExRead<T>(this ITableDataReader self, ref T v) where T : struct
        {
            if (!EnumConverterMgr.Convert(self.ReadInt32(), ref v))
            {
                Log.E("没有找到类型 {0} 的转换", typeof(T));
            }
        }
        #endregion

        #region Pair
        public static bool ExRead(this ITableReader self, ref ValueTuple<int, float> v)
        {
            v = default;
            var pair_reader = self.BeginPair();
            int c = pair_reader != null ? pair_reader.GetCount() : 0;
            if (c == 0)
                return true;
            if (c != 2)
                return false;
            pair_reader.ExRead(ref v.Item1);
            pair_reader.ExRead(ref v.Item2);
            return true;
        }
        public static bool ExRead(this ITableReader self, ref ValueTuple<int, bool> v)
        {
            v = default;
            var pair_reader = self.BeginPair();
            int c = pair_reader != null ? pair_reader.GetCount() : 0;
            if (c == 0)
                return true;
            if (c != 2)
                return false;
            pair_reader.ExRead(ref v.Item1);
            pair_reader.ExRead(ref v.Item2);
            return true;
        }
        public static bool ExRead(this ITableReader self, ref ValueTuple<float, float, float> v)
        {
            v = default;
            var pair_reader = self.BeginPair();
            int c = pair_reader != null ? pair_reader.GetCount() : 0;
            if (c == 0)
                return true;
            if (c != 3)
                return false;
            pair_reader.ExRead(ref v.Item1);
            pair_reader.ExRead(ref v.Item2);
            pair_reader.ExRead(ref v.Item3);
            return true;
        }
        #endregion

        #region List 

        public static void ExRead(this ITableReader self, ref int[] v)
        {
            var list_reader = self.BeginList();
            int count = list_reader != null ? list_reader.GetCount() : 0;
            if (count == 0)
            {
                v = Array.Empty<int>();
                return;
            }
            v = new int[count];

            for (int i = 0; i < count; i++)
            {
                int item = 0;
                list_reader.ExRead(ref item);
                v[i] = item;
            }
        }

        public static void ExRead(this ITableReader self, ref ValueTuple<int, long>[] v)
        {
            var list_reader = self.BeginList();
            int count = list_reader != null ? list_reader.GetCount() : 0;
            if (count == 0)
            {
                v = Array.Empty<ValueTuple<int, long>>();
                return;
            }
            v = new ValueTuple<int, long>[count];

            for (int i = 0; i < count; i++)
            {
                var pair_reader = list_reader.BeginPair();
                var item = new ValueTuple<int, long>();
                pair_reader.ExRead(ref item.Item1);
                pair_reader.ExRead(ref item.Item2);
                v[i] = item;
            }
        }
        #endregion      
    }
}
