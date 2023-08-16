//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
namespace Test
{

    public partial class TableMgr
    {
        private static List<System.Object> _temp = new List<System.Object>(10000);
        public TableMgr()
        {
            _all = new Dictionary<Type, Table>(20 + 3);
            _loader_dict = new Dictionary<Type, TableLoader>(20 + 3);

            _loader_dict.Add(typeof(TItemData), _LoadItemData);
            _loader_dict.Add(typeof(TTestComposeKey), _LoadTestComposeKey);
            _loader_dict.Add(typeof(TLoc), _LoadLoc);
            _lang_set.Add(typeof(TLoc));
        }


        private static Table _LoadItemData(string lang)
        {
            string sheet_name = "ItemData";
            lang = null;
            int col_count = 8;

            if (!_CreateReader(sheet_name, lang, out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header == null || header.Count != (col_count * 2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;

            head_rst &= ((header[0] == "Id") && (header[0 + 8] == "int"));
            head_rst &= ((header[1] == "Name") && (header[1 + 8] == "locid"));
            head_rst &= ((header[2] == "Type") && (header[2 + 8] == "int"));
            head_rst &= ((header[3] == "SubType") && (header[3 + 8] == "int"));
            head_rst &= ((header[4] == "Quality") && (header[4 + 8] == "int"));
            head_rst &= ((header[5] == "PairField") && (header[5 + 8] == "int_bool"));
            head_rst &= ((header[6] == "PairFieldList") && (header[6 + 8] == "list_int_int64"));
            head_rst &= ((header[7] == "ListField") && (header[7 + 8] == "list_int"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow())
                    break;
                var row = new TItemData();
                reader.ExRead(ref row.Id);
                reader.ExRead(ref row.Name);
                reader.ExRead(ref row.Type);
                reader.ExRead(ref row.SubType);
                reader.ExRead(ref row.Quality);
                reader.ExRead(ref row.PairField);
                reader.ExRead(ref row.PairFieldList);
                reader.ExRead(ref row.ListField);

                _temp.Add(row);
            }

            //转换数据
            List<TItemData> list = new List<TItemData>(_temp.Count);
            foreach (var p in _temp)
            {
                list.Add(p as TItemData);
            }


            var dict = new Dictionary<int, TItemData>(list.Count);
            foreach (var p in list)
            {
                if (dict.ContainsKey(p.Id))
                {
                    Log.E("{0} Contain Multi Id: {1}, 如果允许ID重复, 修改表格", typeof(TItemData), p.Id);
                    continue;
                }
                dict.Add(p.Id, p);
            }
            return Table.Create(list, dict);

        }

        public static List<TItemData> GetTItemDataList()
        {
            return GetList<TItemData>();
        }


        public static TItemData GetTItemData(int Id)
        {
            return Get<int, TItemData>(Id);
        }

        public static Dictionary<int, TItemData> GetTItemDataDict()
        {
            return GetDict<int, TItemData>();
        }



        private static Table _LoadTestComposeKey(string lang)
        {
            string sheet_name = "TestComposeKey";
            lang = null;
            int col_count = 4;

            if (!_CreateReader(sheet_name, lang, out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header == null || header.Count != (col_count * 2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;

            head_rst &= ((header[0] == "Id") && (header[0 + 4] == "uint"));
            head_rst &= ((header[1] == "Level") && (header[1 + 4] == "int"));
            head_rst &= ((header[2] == "Name") && (header[2 + 4] == "locid"));
            head_rst &= ((header[3] == "Pos") && (header[3 + 4] == "float_float_float"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow())
                    break;
                var row = new TTestComposeKey();
                reader.ExRead(ref row.Id);
                reader.ExRead(ref row.Level);
                reader.ExRead(ref row.Name);
                reader.ExRead(ref row.Pos);

                _temp.Add(row);
            }

            //转换数据
            List<TTestComposeKey> list = new List<TTestComposeKey>(_temp.Count);
            foreach (var p in _temp)
            {
                list.Add(p as TTestComposeKey);
            }


            var dict = new Dictionary<ulong, TTestComposeKey>(list.Count);
            foreach (var p in list)
            {
                ulong key = _MakeKey((uint)p.Id, (uint)p.Level);
                if (dict.ContainsKey(key))
                {
                    Log.E("{0} Contain Multi Id: {1},{2}, 如果允许ID重复, 修改表格", typeof(TTestComposeKey), p.Id, p.Level);
                    continue;
                }
                dict.Add(key, p);
            }
            return Table.Create(list, dict);

        }

        public static List<TTestComposeKey> GetTTestComposeKeyList()
        {
            return GetList<TTestComposeKey>();
        }


        public static TTestComposeKey GetTTestComposeKey(uint Id, int Level)
        {
            return Get<TTestComposeKey>((uint)Id, (uint)Level);
        }

        public static Dictionary<ulong, TTestComposeKey> GetTTestComposeKeyDict()
        {
            return GetDict<ulong, TTestComposeKey>();
        }



        private static Table _LoadLoc(string lang)
        {
            string sheet_name = "Loc";

            int col_count = 2;

            if (!_CreateReader(sheet_name, lang, out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header == null || header.Count != (col_count * 2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;

            head_rst &= ((header[0] == "Id") && (header[0 + 2] == "int"));
            head_rst &= ((header[1] == "Val") && (header[1 + 2] == "string"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow())
                    break;
                var row = new TLoc();
                reader.ExRead(ref row.Id);
                reader.ExRead(ref row.Val);

                _temp.Add(row);
            }

            //转换数据
            List<TLoc> list = new List<TLoc>(_temp.Count);
            foreach (var p in _temp)
            {
                list.Add(p as TLoc);
            }


            var dict = new Dictionary<int, TLoc>(list.Count);
            foreach (var p in list)
            {
                if (dict.ContainsKey(p.Id))
                {
                    Log.E("{0} Contain Multi Id: {1}, 如果允许ID重复, 修改表格", typeof(TLoc), p.Id);
                    continue;
                }
                dict.Add(p.Id, p);
            }
            return Table.Create(list, dict);

        }

        public static List<TLoc> GetTLocList()
        {
            return GetList<TLoc>();
        }


        public static TLoc GetTLoc(int Id)
        {
            return Get<int, TLoc>(Id);
        }

        public static Dictionary<int, TLoc> GetTLocDict()
        {
            return GetDict<int, TLoc>();
        }

    }
}
