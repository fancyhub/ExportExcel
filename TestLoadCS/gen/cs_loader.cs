//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
namespace Test{

    public delegate Table TableLoader(string lang);
    public struct TableInfo
    {
        public TableLoader Loader;
        public bool MultiLang;
        public TableInfo(TableLoader loader, bool multiLang)
        {
            this.Loader = loader;
            this.MultiLang = multiLang;
        }
    }
    public partial class TableMgr
    {
        private static List<System.Object> _temp = new List<System.Object>(10000);
        public Dictionary<Type, TableInfo> LoaderDict;
        public TableMgr()
        {
            _all = new Dictionary<Type, Table>(20+3);
            LoaderDict = new Dictionary<Type, TableInfo>(20+3);

			LoaderDict.Add(typeof(TItemData),new TableInfo(_LoadItemData,false));
			LoaderDict.Add(typeof(TTestComposeKey),new TableInfo(_LoadTestComposeKey,false));
			LoaderDict.Add(typeof(TLoc),new TableInfo(_LoadLoc,true));
		}


        private static Table _LoadItemData(string lang)
        {
            string sheet_name = "ItemData";
            lang = null;
            int col_count = 8;

            if(!_CreateReader(sheet_name,lang,out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;
            
			head_rst &= ((header[0] == "Id") && (header[0+8] == "int32"));
			head_rst &= ((header[1] == "Name") && (header[1+8] == "locid"));
			head_rst &= ((header[2] == "Type") && (header[2+8] == "int32"));
			head_rst &= ((header[3] == "SubType") && (header[3+8] == "int32"));
			head_rst &= ((header[4] == "Quality") && (header[4+8] == "int32"));
			head_rst &= ((header[5] == "PairField") && (header[5+8] == "int32_bool"));
			head_rst &= ((header[6] == "PairFieldList") && (header[6+8] == "list_int32_int64"));
			head_rst &= ((header[7] == "ListField") && (header[7+8] == "list_int32"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new TItemData();
				rowReader.ExRead(ref row.Id);
				rowReader.ExRead(ref row.Name);
				rowReader.ExRead(ref row.Type);
				rowReader.ExRead(ref row.SubType);
				rowReader.ExRead(ref row.Quality);
				rowReader.ExRead(ref row.PairField);
				rowReader.ExRead(ref row.PairFieldList);
				rowReader.ExRead(ref row.ListField);

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
            return Table.Create(list,dict);
                
		}
      
        public static List<TItemData> GetTItemDataList()
        {
            return GetList<TItemData>();
        }
        

        public static TItemData GetTItemData(int Id)
        {
            return Get<int,TItemData>(Id);
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

            if(!_CreateReader(sheet_name,lang,out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;
            
			head_rst &= ((header[0] == "Id") && (header[0+4] == "uint32"));
			head_rst &= ((header[1] == "Level") && (header[1+4] == "int32"));
			head_rst &= ((header[2] == "Name") && (header[2+4] == "locid"));
			head_rst &= ((header[3] == "Pos") && (header[3+4] == "float32_float32_float32"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new TTestComposeKey();
				rowReader.ExRead(ref row.Id);
				rowReader.ExRead(ref row.Level);
				rowReader.ExRead(ref row.Name);
				rowReader.ExRead(ref row.Pos);

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
                    Log.E("{0} Contain Multi Id: {1},{2}, 如果允许ID重复, 修改表格", typeof(TTestComposeKey), p.Id,p.Level);
                    continue;
                }
                dict.Add(key, p);
            }
            return Table.Create(list,dict);
                
		}
      
        public static List<TTestComposeKey> GetTTestComposeKeyList()
        {
            return GetList<TTestComposeKey>();
        }
        

        public static TTestComposeKey GetTTestComposeKey(uint Id,int Level)
        {        
            return Get<TTestComposeKey>((uint)Id,(uint)Level);
        }

        public static Dictionary<ulong, TTestComposeKey> GetTTestComposeKeyDict()
        {
            return GetDict<ulong, TTestComposeKey>();
        }
        


        private static Table _LoadLoc(string lang)
        {
            string sheet_name = "Loc";
            
            int col_count = 2;

            if(!_CreateReader(sheet_name,lang,out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;
            
			head_rst &= ((header[0] == "Id") && (header[0+2] == "int32"));
			head_rst &= ((header[1] == "Val") && (header[1+2] == "string"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new TLoc();
				rowReader.ExRead(ref row.Id);
				rowReader.ExRead(ref row.Val);

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
            return Table.Create(list,dict);
                
		}
      
        public static List<TLoc> GetTLocList()
        {
            return GetList<TLoc>();
        }
        

        public static TLoc GetTLoc(int Id)
        {
            return Get<int,TLoc>(Id);
        }

        public static Dictionary<int, TLoc> GetTLocDict()
        {
            return GetDict<int, TLoc>();
        }
        
}
}
