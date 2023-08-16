/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/2 16:19:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace ExportExcel
{
    public static class Logger
    {
        public static ConsoleColor textColorDefault = ConsoleColor.Gray;
        public static ConsoleColor textColorError = ConsoleColor.Red;

        
        public static void Print(string msg, params object[] args)
        {
            Console.WriteLine(msg, args);
        }

        public static void Print(ConsoleColor color, string msg, params object[] args)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg, args);
            Console.ForegroundColor = textColorDefault;
        }

        public static void PrintError(string msg, params object[] args)
        {
            Console.ForegroundColor = textColorError;
            Console.WriteLine(msg, args);
            Console.ForegroundColor = textColorDefault;
        }
    }
}
