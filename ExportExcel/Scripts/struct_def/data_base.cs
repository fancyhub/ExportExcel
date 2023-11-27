using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/1 17:12:27
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class DataBase
    {
        public Config Config;

        public DBRef RefDB;
        public DBEnum EnumDB;
        public DBAlias AliasDB;

        public Dictionary<string, Table> Tables;

        public Table TableLocTrans;
        public Table TableLocOld;
        public Dictionary<string, string> LangDefault;
        public List<string> LangList;

        public DataBase(Config config)
        {
            Config = config;
            RefDB = new DBRef();
            EnumDB = new DBEnum();
            AliasDB = new DBAlias();
            Tables = new Dictionary<string, Table>();
            LangDefault = new Dictionary<string, string>();
            LangList = new List<string>();
        }

        public Table FindTable(string sheet_name)
        {
            Tables.TryGetValue(sheet_name, out Table ret);
            return ret;
        }
    }
}
