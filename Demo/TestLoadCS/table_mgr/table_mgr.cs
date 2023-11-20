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
            if (ret == null)
            {
                Log.E("Table {0} 不存在", typeof(T));
            }
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
            _LoaderMgr.LoaderDict.Add(typeof(T), new TableInfo(loader, is_lang_relation));
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
            if (!_LoaderMgr.LoaderDict.TryGetValue(t, out var info) || info.Loader == null)
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
        public Dictionary<TKey, TItem> GetDict<TKey, TItem>() where TItem : class
        {
            return FindTable<TItem>()?.GetDict<TKey, TItem>();
        }

        public List<TItem> GetList<TItem>() where TItem : class
        {
            return FindTable<TItem>()?.GetList<TItem>();
        }

        public TItem Get<TKey, TItem>(TKey id) where TItem : class
        {
            return FindTable<TItem>()?.Get<TKey, TItem>(id);
        }

        public bool Get<TKey, TItem>(TKey id, out TItem v) where TItem : class
        {
            v = Get<TKey, TItem>(id);
            return v != null;
        }
        #endregion


    }
}
