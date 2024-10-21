using System;
using System.Collections.Generic;

namespace ExportExcel
{
    //约束的属性
    public class ConstraintAttr
    {
    }

    public sealed class ConAttrUnique : ConstraintAttr
    {
        public static ConAttrUnique Inst = new ConAttrUnique();
    }

    public sealed class ConAttrBlankForbid : ConstraintAttr
    {
        public static ConAttrBlankForbid Inst = new ConAttrBlankForbid();
    }

    public sealed class ConAttrAlias : ConstraintAttr
    {
        private readonly AliasItem _Alias;
        private ConAttrAlias(AliasItem item)
        {
            _Alias = item;
        }

        public string Name
        {
            get
            {
                if (_Alias == null)
                    return null;
                return _Alias.Name;
            }
        }

        public string GetField(int index)
        {
            if (_Alias == null)
                return null;
            return _Alias.GetField(index);
        }

        public string CSharp
        {
            get
            {
                if (_Alias == null)
                    return null;
                return _Alias.CSharp;
            }
        }

        public string Go
        {
            get
            {
                if (_Alias == null)
                    return null;
                return _Alias.Go;
            }
        }

        public static ConAttrAlias Create(AliasItem item)
        {
            if (item == null)
                return null;
            return new ConAttrAlias(item);
        }
    }

    public sealed class ConAttrEnum : ConstraintAttr
    {
        public readonly EnumType Enum;
        private ConAttrEnum(EnumType item)
        {
            Enum = item;
        }

        public static ConAttrEnum Create(EnumType item)
        {
            if (item == null)
                return null;
            return new ConAttrEnum(item);
        }

        public string Name
        {
            get
            {
                if (Enum == null)
                    return null;
                return Enum.Name;
            }
        }

        public static implicit operator EnumType(ConAttrEnum v)
        {
            if (v == null)
                return null;
            return v.Enum;
        }

        public static implicit operator string(ConAttrEnum v)
        {
            if (v == null)
                return null;
            return v.Enum.Name;
        }
    }

    public sealed class ConAttrDefault : ConstraintAttr
    {
        public readonly string Value;
        public ConAttrDefault(string value)
        {
            this.Value = value;
        }

        public static implicit operator string(ConAttrDefault v)
        {
            if (v == null)
                return null;
            return v.Value;
        }
    }

    public sealed class ConAttrLookup : ConstraintAttr
    {
        public readonly string SheetName;
        public readonly string ColName;

        public ConAttrLookup(string sheetName, string colName)
        {
            this.SheetName = sheetName.Trim();
            this.ColName = colName.Trim();
        }

        public override string ToString()
        {
            return $"LookUp[{SheetName}.{ColName}]";
        }
    }

    public sealed class ConAttrFilePath : ConstraintAttr
    {
        public readonly string DirPrefix;
        public readonly string FileSuffix;

        private ConAttrFilePath(string dirPrefix, string fileSuffix)
        {
            DirPrefix = dirPrefix;
            FileSuffix = fileSuffix;
        }

        public static ConAttrFilePath Create(string dirPrefix, string fileSuffix)
        {
            if (dirPrefix == null)
                return null;
            dirPrefix = dirPrefix.Trim();
            if (fileSuffix != null)
                fileSuffix = fileSuffix.Trim();
            return new ConAttrFilePath(dirPrefix, fileSuffix);
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(FileSuffix))
                return $"FilePath[{DirPrefix}]";
            else
                return $"FilePath[{DirPrefix},{FileSuffix}]";
        }
    }

    public sealed class ConAttrRange : ConstraintAttr
    {
        public readonly string Min;
        public readonly string Max;

        public ConAttrRange(string min, string max)
        {
            Min = min.Trim();
            Max = max.Trim();
        }

        public override string ToString()
        {
            return $"[{Min},{Max}]";
        }
    }

    public sealed class ConAttrPK : ConstraintAttr
    {
        public readonly TableField Field;
        public readonly TableField[] SubKeys = System.Array.Empty<TableField>();
        public ConAttrPK(TableField field)
        {
            Field = field;
        }

        public ConAttrPK(TableField field, TableField[] sub_keys)
        {
            Field = field;
            SubKeys = sub_keys;
        }

        /// <summary>
        /// 是否为组合key
        /// </summary>
        public bool IsCompose()
        {
            return SubKeys.Length > 0;
        }

        public override string ToString()
        {
            if (SubKeys.Length == 0)
                return "PK";

            List<string> temp = new List<string>();
            foreach (var p in SubKeys)
                temp.Add(p.Name);
            return $"PK[{string.Join(',', temp)}]";
        }
    }
}
