using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExportExcel
{
    public enum EAliasCode
    {
        None,
        CSharp,
        Cpp,
        Go,
        Lua,
    }

    public struct DbAliasKey : IEquatable<DbAliasKey>
    {
        public readonly string SheetName;
        public readonly string ColumnName;

        public DbAliasKey(string sheetName, string columnName)
        {
            SheetName = sheetName;
            ColumnName = columnName;
        }

        public bool Equals(DbAliasKey other)
        {
            return other.SheetName == SheetName && other.ColumnName == ColumnName;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(SheetName, ColumnName);
        }
    }

    public class DbAliasKeyEqualityComparer : IEqualityComparer<DbAliasKey>
    {
        public bool Equals(DbAliasKey x, DbAliasKey y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] DbAliasKey obj)
        {
            return obj.GetHashCode();
        }
    }


    public class AliasDict : Dictionary<DbAliasKey, string>
    {
        public readonly EAliasCode CodeType;
        public readonly EExportFlag Flag;
        public AliasDict(EAliasCode code, EExportFlag flag)
            : base(new DbAliasKeyEqualityComparer())
        {
            CodeType = code;
            Flag = flag;
        }

        public string GetAlias(string sheet_name, string col_name)
        {
            var key = new DbAliasKey(sheet_name, col_name);
            this.TryGetValue(key, out var value);
            return value;
        }
    }

    public static class AliasDictExt
    {
        public static string ExtGetAliasName(this AliasDict self, string sheet_name, string col_name)
        {
            if (self == null)
                return null;
            if (string.IsNullOrEmpty(sheet_name) || string.IsNullOrEmpty(col_name))
            {
                return null;
            }
            return self.GetAlias(sheet_name, col_name);
        }
    }

    public struct AliasValue
    {
        public EAliasCode Code;
        public string SheetName;
        public string ColumnName;
        public string Client;
        public string Server;
    }

    public sealed class DBAlias
    {
        public AliasDict[] AliasDictList;

        public List<AliasValue> AliasList;

        public DBAlias()
        {
            AliasDictList = new AliasDict[((int)EAliasCode.Lua + 1) * 2];
            AliasList = new List<AliasValue>();

            for (var i = EAliasCode.CSharp; i <= EAliasCode.Lua; i++)
            {
                AliasDictList[(int)i * 2] = new AliasDict(i, EExportFlag.Client);
                AliasDictList[(int)i * 2 + 1] = new AliasDict(i, EExportFlag.Server);
            }
        }

        public AliasDict GetAliasDict(EAliasCode code, EExportFlag flag)
        {
            int index = (int)code * 2 + (int)flag;
            if (index < 0 || index >= AliasDictList.Length)
                return null;
            return AliasDictList[index];
        }


        public void Add(EAliasCode code, string sheet_name, string column_name, string client, string server)
        {
            if (string.IsNullOrEmpty(sheet_name) || string.IsNullOrEmpty(column_name))
                return;
            if (code == EAliasCode.None)
                return;
            sheet_name = sheet_name.Trim();
            column_name = column_name.Trim();
            _Add(code, EExportFlag.Client, sheet_name, column_name, client);
            _Add(code, EExportFlag.Server, sheet_name, column_name, server);

            AliasList.Add(new AliasValue()
            {
                Code = code,
                SheetName = sheet_name,
                ColumnName = column_name,
                Client = client,
                Server = server
            });
        }

        private void _Add(EAliasCode code, EExportFlag flag, string sheet_name, string column_name, string alias_name)
        {
            int index = (int)code * 2 + (int)flag;
            if (index < 0 || index >= AliasDictList.Length)
                return;
            var aliasDict = AliasDictList[index];
            if (aliasDict == null)
                return;

            if (string.IsNullOrEmpty(alias_name))
                return;
            alias_name = alias_name.Trim();

            var key = new DbAliasKey(sheet_name, column_name);
            aliasDict[key] = alias_name;
        }

        public static EAliasCode ToAliasCode(string code)
        {
            if (code == null)
                return EAliasCode.None;

            string a = code.ToLower();

            switch (a)
            {
                case "csharp": return EAliasCode.CSharp;
                case "cpp": return EAliasCode.Cpp;
                case "lua": return EAliasCode.Lua;
                case "go": return EAliasCode.Go;
                default: return EAliasCode.None;
            }
        }
    }
}
