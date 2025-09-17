using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExportExcel
{
    public class AliasItem
    {
        public readonly string Name;
        public readonly string[] Fields;
        
        public string CSharp;
        public string Go;
        public string Cpp;

        public AliasItem(string name, string[] fields)
        {
            Name = name;
            Fields = fields;
        }

        public string GetField(int index)
        {
            if (index < 0 || index >= Fields.Length)
                return null;
            return Fields[index];
        }
    }

    public sealed class DBAlias : IEnumerable<KeyValuePair<string, AliasItem>>
    {
        public Dictionary<string, AliasItem> _Dict;

        public DBAlias()
        {
            _Dict = new Dictionary<string, AliasItem>();
        }
        public void Add(AliasItem item)
        {
            _Dict.Add(item.Name, item);
        }

        public AliasItem Find(string name)
        {
            _Dict.TryGetValue(name, out AliasItem item);
            return item;
        }

        public Dictionary<string, AliasItem>.Enumerator GetEnumerator()
        {
            return _Dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Dict.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, AliasItem>> IEnumerable<KeyValuePair<string, AliasItem>>.GetEnumerator()
        {
            return _Dict.GetEnumerator();
        }
    }
}
