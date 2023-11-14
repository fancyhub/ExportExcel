using System;
using System.Collections;
using System.Collections.Generic;

namespace Test
{
    public delegate void EventTableLoaded(Table table);

    public partial class TableMgr
    { 
        private TableLoaderMgr _LoaderMgr;
        private Dictionary<Type, Table> _all = new Dictionary<Type, Table>();
        private Dictionary<Type, List<EventTableLoaded>> _post_processer = new Dictionary<Type, List<EventTableLoaded>>();
        public ITableReaderCreator TableReaderCreator;

        public TableMgr(ITableReaderCreator creator)
        {
            TableReaderCreator = creator;
            _LoaderMgr = new TableLoaderMgr(creator.CreateTableReader);
            _all = new Dictionary<Type, Table>(_LoaderMgr.LoaderDict.Count);

            LocLang.EvtLangChange0 = _OnLangChange;
            OnInstCreate();
        }

        #region  Table 相关
        private void _OnLangChange()
        {
            //1. 先卸载
            foreach (var p in _LoaderMgr.LoaderDict)
            {
                if (p.Value.MultiLang)
                    _all.Remove(p.Key);
            }

            //2. 重新加载
            foreach (var p in _LoaderMgr.LoaderDict)
            {
                if (p.Value.MultiLang)
                    _LoadTable(p.Key, false);
            }

            TableReaderCreator.CloseReader();
        }

        public void LoadAllTable()
        {            
            bool is_empty = _all.Count == 0;

            //先加载语言
            foreach (var p in _LoaderMgr.LoaderDict)
            {
                if (p.Value.MultiLang)
                    _LoadTable(p.Key, false);
            }

            //加载其他表
            foreach (var p in _LoaderMgr.LoaderDict)
            {
                if (!p.Value.MultiLang)
                    _LoadTable(p.Key, false);
            }

            TableReaderCreator.CloseReader();

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

        public void AddPostProcesser<T>(EventTableLoaded loaded_action) where T : class
        {
            if (loaded_action == null)
                return;

            _post_processer.TryGetValue(typeof(T), out List<EventTableLoaded> list);
            if (list == null)
            {
                list = new List<EventTableLoaded>();
                _post_processer.Add(typeof(T), list);
            }
            list.Add(loaded_action);
        }

        public void AddTableLoader<T>(TableLoader loader, bool is_lang_relation = false) where T : class
        {
            if (loader == null)
                return;

            Type t = typeof(T);
            _LoaderMgr.LoaderDict.Add(typeof(T), new TableInfo(loader,is_lang_relation));            
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
            if (!_LoaderMgr.LoaderDict.TryGetValue(t, out var info) || info.Loader== null)
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
            _post_processer.TryGetValue(t, out List<EventTableLoaded> list);
            if (list != null)
            {
                foreach (EventTableLoaded p in list)
                {
                    p(ret);
                }
            }
            return ret;
        }
         
        #endregion

        #region Common Get
        public Dictionary<TKey, TVal> GetDict<TKey, TVal>() where TVal : class
        {
            Table ts = FindTable<TVal>();
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

        public List<T> GetList<T>() where T : class
        {
            Table ts = FindTable<T>();
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

        public TVal Get<TKey, TVal>(TKey id) where TVal : class
        {
            Dictionary<TKey, TVal> dict = GetDict<TKey, TVal>();
            if (dict == null)
                return null;
            bool succ = dict.TryGetValue(id, out TVal ret);
            Log.Assert(succ, "Table {0}, 不存在Key {1}", typeof(TVal), id);
            return ret;
        }

        public bool Get<TKey, TVal>(TKey id, out TVal v) where TVal : class
        {
            v = Get<TKey, TVal>(id);
            return v != null;
        }
        #endregion

        #region  组合key
        public TVal Get<TVal>(uint key1, uint key2) where TVal : class
        {
            ulong key = Table.MakeKey(key1, key2);

            Dictionary<ulong, TVal> dict = GetDict<ulong, TVal>();
            if (dict == null)
            {
                return null;
            }

            dict.TryGetValue(key, out TVal ret);
            Log.Assert(ret != null, "Table {0}, 不存在Key ( {1},{2} )", typeof(TVal), key1, key2);
            return ret;
        }

        public bool Get<TVal>(uint key1, uint key2, out TVal v) where TVal : class
        {
            v = Get<TVal>(key1, key2);
            return v != null;
        }

        public bool Get<TVal>(uint key1, int key2, out TVal v) where TVal : class
        {
            v = Get<TVal>(key1, (uint)key2);
            return v != null;
        }

        public bool Get<TVal>(int key1, int key2, out TVal v) where TVal : class
        {
            v = Get<TVal>((uint)key1, (uint)key2);
            return v != null;
        }

        public bool Get<TVal>(int key1, uint key2, out TVal v) where TVal : class
        {
            v = Get<TVal>((uint)key1, key2);
            return v != null;
        }         
        #endregion

    }
}
