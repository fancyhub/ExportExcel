//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
namespace Test{

    public delegate bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader);
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
    public partial class TableLoaderMgr
    {
        private static List<System.Object> _temp = new List<System.Object>(10000);
        public static List<string> LangList;

        public CreateTableReader CreateTableReader;
        public Dictionary<Type, TableInfo> LoaderDict;
        static TableLoaderMgr()
        {        
            LangList= new List<string>(2);

			LangList.Add("zh-Hans");
			LangList.Add("EN");
			EnumConverterMgr.RegFunc((v) => (EItemType)v, (v) => (int)v);
			EnumConverterMgr.RegFunc((v) => (EItemSubType)v, (v) => (int)v);
			EnumConverterMgr.RegFunc((v) => (EItemQuality)v, (v) => (int)v);
		}

        public TableLoaderMgr(CreateTableReader createTableReader)
        {
            CreateTableReader = createTableReader;            
            LoaderDict = new Dictionary<Type, TableInfo>(20+3);
            

			LoaderDict.Add(typeof(TItemData),new TableInfo(_LoadItemData,false));
			LoaderDict.Add(typeof(TTestComposeKey),new TableInfo(_LoadTestComposeKey,false));
			LoaderDict.Add(typeof(TLoc),new TableInfo(_LoadLoc,true));
		}

        private Table _LoadItemData(string lang)
        {
            string sheet_name = "ItemData";
            lang = null;
            int col_count = 8;

            if(!CreateTableReader(sheet_name,lang,out var reader))
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
				_Read(rowReader, ref row.Id);
				_Read(rowReader, ref row.Name);
				_Read(rowReader, ref row.Type);
				_Read(rowReader, ref row.SubType);
				_Read(rowReader, ref row.Quality);
				_ReadTuple(rowReader.BeginTuple(), ref row.PairField);
				_ReadList(rowReader, ref row.PairFieldList);
				_ReadList(rowReader, ref row.ListField);

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

        private Table _LoadTestComposeKey(string lang)
        {
            string sheet_name = "TestComposeKey";
            lang = null;
            int col_count = 4;

            if(!CreateTableReader(sheet_name,lang,out var reader))
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
				_Read(rowReader, ref row.Id);
				_Read(rowReader, ref row.Level);
				_Read(rowReader, ref row.Name);
				_ReadTuple(rowReader.BeginTuple(), ref row.Pos);

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
                ulong key = Table.MakeKey((uint)p.Id, (uint)p.Level);
                if (dict.ContainsKey(key))
                {
                    Log.E("{0} Contain Multi Id: {1},{2}, 如果允许ID重复, 修改表格", typeof(TTestComposeKey), p.Id,p.Level);
                    continue;
                }
                dict.Add(key, p);
            }
            return Table.Create(list,dict);
                
		}

        private Table _LoadLoc(string lang)
        {
            string sheet_name = "Loc";
            
            int col_count = 2;

            if(!CreateTableReader(sheet_name,lang,out var reader))
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
				_Read(rowReader, ref row.Id);
				_Read(rowReader, ref row.Val);

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

        #region Base Reader
        private static void _Read(ITableDataReader reader, ref bool v)
        {
            v = reader.ReadBool();
        }
        private static void _Read(ITableDataReader reader, ref int v)
        {
            v = reader.ReadInt32();
        }
        private static void _Read(ITableDataReader reader, ref uint v)
        {
            v = reader.ReadUInt32();
        }
        private static void _Read(ITableDataReader reader, ref long v)
        {
            v = reader.ReadInt64();
        }
        private static void _Read(ITableDataReader reader, ref ulong v)
        {
            v = reader.ReadUInt64();
        }
        private static void _Read(ITableDataReader reader, ref float v)
        {
            v = reader.ReadF32();
        }
        private static void _Read(ITableDataReader reader, ref double v)
        {
            v = reader.ReadF64();
        }
        private static void _Read(ITableDataReader reader, ref string v)
        {
            v = reader.ReadString();
        }
        private static void _Read(ITableDataReader reader, ref LocStr v)
        {
            string s = reader.ReadString();
            v = s;
        }
        private static void _Read(ITableDataReader reader, ref LocId v)
        {
            v = reader.ReadInt32();
        }
        private static void _Read<T>(ITableDataReader reader, ref T v) where T : Enum
        {
            if (!EnumConverterMgr.Convert(reader.ReadInt32(), ref v))
            {
                Log.E("没有找到类型 {0} 的转换", typeof(T));
            }
        }
        #endregion
		#region Tuple Reader

        private static void _ReadTuple(ITableTupleReader tupleReader, ref (int,bool)v)
        {
            if(tupleReader==null)
                return;

			_Read(tupleReader,ref v.Item1);
			_Read(tupleReader,ref v.Item2);
		}

        private static void _ReadTuple(ITableTupleReader tupleReader, ref (int,long)v)
        {
            if(tupleReader==null)
                return;

			_Read(tupleReader,ref v.Item1);
			_Read(tupleReader,ref v.Item2);
		}

        private static void _ReadTuple(ITableTupleReader tupleReader, ref (float,float,float)v)
        {
            if(tupleReader==null)
                return;

			_Read(tupleReader,ref v.Item1);
			_Read(tupleReader,ref v.Item2);
			_Read(tupleReader,ref v.Item3);
		}
		#endregion

		#region List Reader

        private static void _ReadList(ITableRowReader rowReader, ref (int,long)[]v)
        {
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<(int,long)>();
            else
            {
                v = new (int,long)[count];
                for (int i = 0; i < count; i++)
                {
                    (int,long) item = default;
                    _ReadTuple(listReader.BeginTuple(), ref item);                    
                    v[i] = item;
                }
            }
        }

        private static void _ReadList(ITableRowReader rowReader, ref int[]v)
        {
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<int>();
            else
            {
                v = new int[count];
                for (int i = 0; i < count; i++)
                {
                    int item = default;
                    _Read(listReader, ref item);                    
                    v[i] = item;
                }
            }
        }
		#endregion
	}
}
