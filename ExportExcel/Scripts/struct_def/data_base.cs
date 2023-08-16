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
        public ExeConfig Config;

        public DBRef RefDB;
        public DBEnum EnumDB;
        public Dictionary<string, string> LangDefault;
        public List<string> LangList;
        public Dictionary<string, Table> Tables;

        public DataBase(ExeConfig config)
        {
            Config = config;
            RefDB = new DBRef();
            EnumDB = new DBEnum();
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
