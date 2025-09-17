using System;
using System.Collections.Generic;

namespace ExportExcel
{
    public static class ConstDef
    {
        public static char SeparatorTuple = '|';
        public static char SeparatorList = ';';

        public static int NameRowIndex = 0;
        public static int TypeRowIndex = 1;
        public static int DescRowIndex = 2;
        public static int DataStartRowIndex = 3;

        public static bool CalculateFormula = true;
        public static string EmptyPlaceholder = null;

        public const string Comment = "#";

        public const char SheetNameConstraintSeparator = '|'; //export_client, export_server 的分隔符
        public const char SheetNamePartialSeparator = '_';   // 表格分表专用的分隔符

        public const string SpecSheetNameEnum = "@@EnumConfig";
        public const string SpecSheetNameRef= "@@RefTable";
        public const string SpecSheetNameAlias= "@@Alias";

    }
}
