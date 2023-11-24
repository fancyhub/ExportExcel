//#define USE_LOC_CLASS

using System;
using System.Collections.Generic;

namespace Test
{
#if USE_LOC_CLASS
    //多语言的 str
    public sealed class LocStr
    {
        public const int C_CAP = 1000;
        private static Dictionary<string, LocStr> Dict = new Dictionary<string, LocStr>(C_CAP);

        //loc 的key
        public readonly string key;
        private string _val = string.Empty;
        //loc 的val
        public string Val { get { return _show_key ? key : _val; } }

        static LocStr()
        {
            _show_key = LocLang.ShowKey;
            LocLang.EvtShowKeyChange0 = _OnShowKeyChange;
        }
        private static bool _show_key;
        private static void _OnShowKeyChange()
        {
            _show_key = LocLang.ShowKey;
        }

        private LocStr(string key) { this.key = key; _val = string.Empty; }
        private LocStr(string key, string default_val) { this.key = key; _val = default_val; }

        public bool IsNull { get { return string.IsNullOrEmpty(key); } }

        public static LocStr Find(string key)
        {
            if (key == null)
                return null;
            Dict.TryGetValue(key, out var ret);
            return ret;
        }

        public static string GetText(string key)
        {
            LocStr loc_str = Find(key);
            if (loc_str == null)
                return string.Empty;
            return loc_str.Val;
        }

        /// <summary>
        /// 如果没有,就创建一个
        /// </summary>
        public static LocStr Get(string key)
        {
            if (key == null)
                key = string.Empty;

            Dict.TryGetValue(key, out var ret);
            if (ret == null)
            {
                ret = new LocStr(key, string.Empty);
                Dict.Add(key, ret);
            }
            return ret;
        }

        public static void Set(string key, string val)
        {
            if (key == null)
                key = string.Empty;

            LocStr ls = Get(key);
            ls._val = val;
            if (val == null)
                ls._val = string.Empty;
        }

        public override string ToString()
        {
            return _show_key ? key : _val;
        }

        public string Format(object arg0)
        {
            try
            {
                return string.Format(Val, arg0);
            }
            catch (Exception e)
            {
                Log.E(e);
                return Val;
            }
        }

        public string Format(object arg0, object arg1)
        {
            if (_show_key)
                return key;

            try
            {
                return string.Format(_val, arg0, arg1);
            }
            catch (Exception e)
            {
                Log.E(e);
                return Val;
            }
        }

        public string Format(object arg0, object arg1, object arg2)
        {
            if (_show_key)
                return key;

            try
            {
                return string.Format(_val, arg0, arg1, arg2);
            }
            catch (Exception e)
            {
                Log.E(e);
                return Val;
            }
        }

        public string Format(params object[] args)
        {
            if (_show_key)
                return key;

            if (args == null || args.Length == 0)
                return _val;
            try
            {
                return string.Format(_val, args);
            }
            catch (Exception e)
            {
                Log.E(e);
                return _val;
            }
        }       
    }

#else
    public struct LocStr
    {
        public string id;
        public LocStr(string id)
        {
            this.id = id;
        }

        public static implicit operator LocStr(string id) { return new LocStr(id); }

        public static implicit operator string(LocStr v)
        {
            return LocMgr.Get(v.id);
        }

        public override string ToString()
        {
            return LocMgr.Get(id);
        }

        public static LocStr Get(string id)
        {
            return new LocStr(id);
        }
    }

    public struct LocId
    {
        public readonly int id;
        public LocId(int id)
        {
            this.id = id;
        }

        public static implicit operator LocId(int id) { return new LocId(id); }

        public static implicit operator string(LocId v)
        {
            return LocMgr.Get(v);
        }

        public override string ToString()
        {
            return LocMgr.Get(this);
        }

        public static LocStr Get(string id)
        {
            return new LocStr(id);
        }
    }

    public class LocMgr
    {
        public static LocMgr Inst = new LocMgr();

        private static bool _show_key;
        private Dictionary<string, string> _dict_str = new Dictionary<string, string>();
        private Dictionary<int, string> _dict_id = new Dictionary<int, string>();

        private LocMgr()
        {
            _show_key = LocLang.ShowKey;
            LocLang.EvtShowKeyChange0 = _OnShowKeyChange;
        }

        private static void _OnShowKeyChange()
        {
            _show_key = LocLang.ShowKey;
        }

        public void Update(Dictionary<string, string> dict)
        {
            _dict_str = dict;
        }

        public void Clear()
        {
            _dict_id.Clear();
            _dict_str.Clear();
        }

        public void Add(string key, string v)
        {
            _dict_str.Add(key, v);
        }

        public void Add(int id, string v)
        {
            _dict_id.Add(id, v);
        }

        public static string Get(LocId id)
        {
            if (_show_key)
                return id.id.ToString();

            if (id.id == 0)
                return string.Empty;

            Inst._dict_id.TryGetValue(id.id, out var value);
            if (value == null)
                return id.id.ToString();
            return value;
        }

        public static string Get(string key)
        {
            if (_show_key)
                return key;

            if (string.IsNullOrEmpty(key))
                return string.Empty;

            Inst._dict_str.TryGetValue(key, out var value);
            if (value == null)
                return key;
            return value;
        }
    }
#endif
}
