using System;
using System.Collections;
using System.Collections.Generic;

namespace Test
{

    public delegate void TableLoaded(Table table);

    public partial class TableMgr
    {


        private static TableMgr _;
        public static TableMgr Inst
        {
            get
            {
                if (_ == null)
                {
                    _ = new TableMgr();
                    LocLang.EvtLangChange0 = _._OnLangChange;
                    _.OnInstCreate();
                }
                return _;
            }
        }

        private Dictionary<Type, Table> _all = new Dictionary<Type, Table>();
        private Dictionary<Type, List<TableLoaded>> _post_processer = new Dictionary<Type, List<TableLoaded>>();

        #region  Table 相关
        private void _OnLangChange()
        {
            //1. 先卸载
            foreach (var p in LoaderDict)
            {
                if (p.Value.MultiLang)
                    _all.Remove(p.Key);
            }

            //2. 重新加载
            foreach (var p in LoaderDict)
            {
                if (p.Value.MultiLang)
                    _LoadTable(p.Key, false);
            }

            _CloseReader();
        }

        public void LoadAllTable(ETableReaderType readType, string baseDir)
        {
            _ReaderType = readType;
            _BaseDir = baseDir;
            bool is_empty = _all.Count == 0;

            //先加载语言
            foreach (var p in LoaderDict)
            {
                if (p.Value.MultiLang)
                    _LoadTable(p.Key, false);
            }

            //加载其他表
            foreach (var p in LoaderDict)
            {
                if (!p.Value.MultiLang)
                    _LoadTable(p.Key, false);
            } 

            _CloseReader();

            if (is_empty)
                OnAllLoaded();
        }

        public void ClearAllTable()
        {
            _all.Clear();
        }

        public Table FindTable<T>() where T : class
        {
            _all.TryGetValue(typeof(T), out Table ret);
            return ret;
        }

        public void AddPostProcesser<T>(TableLoaded loaded_action) where T : class
        {
            if (loaded_action == null)
                return;

            _post_processer.TryGetValue(typeof(T), out List<TableLoaded> list);
            if (list == null)
            {
                list = new List<TableLoaded>();
                _post_processer.Add(typeof(T), list);
            }
            list.Add(loaded_action);
        }

        public void AddTableLoader<T>(TableLoader loader, bool is_lang_relation = false) where T : class
        {
            if (loader == null)
                return;

            Type t = typeof(T);
            LoaderDict.Add(typeof(T), new TableInfo(loader,is_lang_relation));            
        }

        public bool UnloadTable<T>() where T : class
        {
            return _all.Remove(typeof(T));
        }

        private Table _LoadTable(Type t, bool reload = false)
        {
            //0. 如果是强制的
            if (reload)
                _all.Remove(t);

            //1. 先判断是否存在
            bool succ = _all.TryGetValue(t, out Table ret);
            if (succ)
                return ret;

            //2. 获取 loader            
            if (!LoaderDict.TryGetValue(t, out var info) || info.Loader== null)
                return default;            

            //3. 加载
            ret = info.Loader(LocLang.Lang);
            if (ret == null || ret.List == null)
            {
                //虽然加载失败了,但是为了防止二次加载,先把空的添加进去
                _all.Add(t, ret);
                return ret;
            }

            //4. 添加
            _all.Add(t, ret);

            //5. 后处理
            _post_processer.TryGetValue(t, out List<TableLoaded> list);
            if (list != null)
            {
                foreach (TableLoaded p in list)
                {
                    p(ret);
                }
            }
            return ret;
        }

        private static ETableReaderType _ReaderType = ETableReaderType.Csv;
        private static string _BaseDir = "";
        private static bool _CreateReader(string sheet_name, string lang_name, out ITableReader reader)
        {
            switch (_ReaderType)
            {
                default:
                    reader = null;
                    return false;
                case ETableReaderType.Csv:
                    reader = _CreateCsvReader(_BaseDir, sheet_name, lang_name);
                    return reader != null;
                case ETableReaderType.Bin:
                    reader = _CreateBinReader(_BaseDir, sheet_name, lang_name);
                    return reader != null;
            }
        }

        private static TableReaderBin _bin_reader;
        private static ITableReader _CreateBinReader(string base_dir, string sheet_name, string lang_name)
        {
            if (_bin_reader != null && _bin_reader.CurLang == lang_name)
            {
                if (_bin_reader.Start(sheet_name))
                    return _bin_reader;
                return null;
            }

            byte[] buff = _LoadResBin(base_dir, lang_name);
            if (buff == null)
                return null;

            if (_bin_reader == null)
                _bin_reader = new TableReaderBin();
            _bin_reader.CurLang = lang_name;
            _bin_reader.Reset(buff);


            if (_bin_reader.Start(sheet_name))
            {
                return _bin_reader;
            }
            return null;
        }

        private static TableReaderCsv _csv_reader;
        private static ITableReader _CreateCsvReader(string base_dir, string sheet_name, string lang_name)
        {

            byte[] buff = _LoadResCsv(base_dir, sheet_name, lang_name);
            if (buff == null)
            {
                return null;
            }
            if (_csv_reader == null)
                _csv_reader = new TableReaderCsv();
            _csv_reader.Reset(buff);
            return _csv_reader;
        }
        private static void _CloseReader()
        {
            _bin_reader?.Reset(null);
            _csv_reader?.Reset(null);
        }
        #endregion

        #region Common Get
        public static Dictionary<TKey, TVal> GetDict<TKey, TVal>() where TVal : class
        {
            Table ts = Inst.FindTable<TVal>();
            if (ts == null || ts.Dict == null)
            {
                Log.E("Table {0} 不存在 Dict<{1},{0}>", typeof(TVal), typeof(TKey));
                return null;
            }

            Dictionary<TKey, TVal> dict_t = ts.Dict as Dictionary<TKey, TVal>;
            if (dict_t == null)
            {
                Log.E("Table {1} 转换失败, Dict<{2},{3}> -> Dict<{0},{1}>", typeof(TKey), typeof(TVal), ts.KeyType, ts.DataType);
                return null;
            }
            return dict_t;
        }

        public static List<T> GetList<T>() where T : class
        {
            Table ts = Inst.FindTable<T>();
            if (ts == null || ts.List == null)
            {
                Log.E("Table {0} 不存在", typeof(T));
                return null;
            }
            List<T> list_t = ts.List as List<T>;
            if (list_t == null)
            {
                Log.E("Table {0} 不存在2", typeof(T));
                return null;
            }
            return list_t;
        }

        public static TVal Get<TKey, TVal>(TKey id) where TVal : class
        {
            Dictionary<TKey, TVal> dict = GetDict<TKey, TVal>();
            if (dict == null)
                return null;
            bool succ = dict.TryGetValue(id, out TVal ret);
            Log.Assert(succ, "Table {0}, 不存在Key {1}", typeof(TVal), id);
            return ret;
        }

        public static bool Get<TKey, TVal>(TKey id, out TVal v) where TVal : class
        {
            v = Get<TKey, TVal>(id);
            return v != null;
        }
        #endregion

        #region  组合key
        public static TVal Get<TVal>(uint key1, uint key2) where TVal : class
        {
            ulong key = _MakeKey(key1, key2);

            Dictionary<ulong, TVal> dict = GetDict<ulong, TVal>();
            if (dict == null)
            {
                return null;
            }

            dict.TryGetValue(key, out TVal ret);
            Log.Assert(ret != null, "Table {0}, 不存在Key ( {1},{2} )", typeof(TVal), key1, key2);
            return ret;
        }

        public static bool Get<TVal>(uint key1, uint key2, out TVal v) where TVal : class
        {
            v = Get<TVal>(key1, key2);
            return v != null;
        }

        public static bool Get<TVal>(uint key1, int key2, out TVal v) where TVal : class
        {
            v = Get<TVal>(key1, (uint)key2);
            return v != null;
        }

        public static bool Get<TVal>(int key1, int key2, out TVal v) where TVal : class
        {
            v = Get<TVal>((uint)key1, (uint)key2);
            return v != null;
        }

        public static bool Get<TVal>(int key1, uint key2, out TVal v) where TVal : class
        {
            v = Get<TVal>((uint)key1, key2);
            return v != null;
        }

        private static ulong _MakeKey(uint k1, uint k2)
        {
            ulong u1 = (uint)k1;
            ulong u2 = (uint)k2;
            return (u1 << 32) | u2;
        }
        #endregion


    }
}
