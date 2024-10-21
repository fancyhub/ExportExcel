using System;
using System.Collections;
using System.Collections.Generic;

namespace Test
{
    
    public partial class TableMgr
    {
        public ITableCsvReaderCreator TableReaderCreator;

        public TableMgr(ITableCsvReaderCreator creator)
        {
            this.TableReaderCreator = creator;
        }


        public void LoadFromCsv(string lang)
        {
            TableLoaderCsvUtil.Init();
            foreach (var p in AllTables)
            {
                string lang2 = lang;
                if (!p.IsMutiLang)
                    lang2 = null;
                TableReaderCreator.CreateTableReader(p.SheetName, lang2, out var reader);

                if (p.LoadFromCsv(reader))                
                    p.BuildMap();                
            }
        }
    }
}
