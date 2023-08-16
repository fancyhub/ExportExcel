using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/1 15:50:12
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public static class ErrSet
    {
        public static int _count = 0;

        public static void E(string msg, string path = null)
        {
            _count++;
            if (path == null)
                Logger.PrintError(msg);
            else
                Logger.PrintError($"{msg}, {path}");
        }

        public static void E(TableCell cell, string msg)
        {
            _count++;
            Logger.PrintError($"表 {cell.SheetColName}, {msg}, {cell.FindDataPath()}");
        }

        public static void E(TableCol col, string msg)
        {
            _count++;
            Logger.PrintError($"表 {col.SheetColName}, {msg}");
        }

        public static bool HasError()
        {
            return _count > 0;
        }

        public static void Clear()
        {
            _count = 0;
        }
    }
}
