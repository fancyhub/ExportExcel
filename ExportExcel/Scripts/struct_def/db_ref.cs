using System;
using System.Collections.Generic;

namespace ExportExcel
{
    /// <summary>
    /// 被引用的DB
    /// </summary>
    public class DBRef
    {
        public Dictionary<(string, string), string> _dict = new Dictionary<(string, string), string>();
        public HashSet<string> _key_set = new HashSet<string>();

        public void AddValue(string key, string col_name, string v)
        {
            _dict.Add((key, col_name), v);
        }

        public bool GetValue(string key, string col_name, out string v)
        {
            return _dict.TryGetValue((key, col_name), out v);
        }

        public bool AddKey(string key)
        {
            if (_key_set.Contains(key))
                return false;
            _key_set.Add(key);
            return true;
        }
    }

}
