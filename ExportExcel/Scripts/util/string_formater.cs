using System;
using System.Collections.Generic;
using System.IO;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/15 17:08:50
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class StringFormater
    {
        public Dictionary<string, string> _dict = new Dictionary<string, string>();
        public string this[string key]
        {
            set
            {
                var key_name = "{" + key + "}";
                _dict[key_name] = value;
            }
            get
            {
                var key_name = "{" + key + "}";
                _dict.TryGetValue(key_name, out var v);
                return v;
            }
        }

        public string Format(string str)
        {
            foreach (var p in _dict)
            {
                str = str.Replace(p.Key, p.Value);
            }
            return str;
        }         
    }

    public static class StreamWriterExt
    {
        public static void WriteExt(this StreamWriter self, StringFormater formater, string v)
        {
            v = formater.Format(v);
            self.Write(v);
        }
        public static void WriteLineExt(this StreamWriter self, StringFormater formater, string v)
        {
            v = formater.Format(v);
            self.WriteLine(v);
        }
    }
}
