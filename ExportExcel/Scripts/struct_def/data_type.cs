using OfficeOpenXml.Table.PivotTable;
using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/8 10:59:39
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public enum EDataType
    {
        None,
        Bool,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
        String,
        LocStr,
        LocId,
    }

    public struct DataType
    {
        public const int C_MAX_COUNT = 5;
        public EDataType type0;
        public EDataType type1;
        public EDataType type2;
        public EDataType type3;
        public EDataType type4;
        public int Count;
        public bool IsPair => Count > 1;
        public bool IsList;
        public EnumType enum_type;

        public void AddType(EDataType type)
        {
            if (type == EDataType.None)
                throw new Exception("类型有问题");

            switch (Count)
            {
                case 0:
                    type0 = type;
                    Count++;
                    break;
                case 1:
                    type1 = type;
                    Count++;
                    break;
                case 2:
                    type2 = type;
                    Count++;
                    break;
                case 3:
                    type3 = type;
                    Count++;
                    break;
                case 4:
                    type4 = type;
                    Count++;
                    break;

                default:
                    throw new Exception("超出了最大上限");
                    break;
            }
        }

        public EDataType Get(int idx)
        {
            switch (idx)
            {
                default: return EDataType.None;
                case 0: return type0;
                case 1: return type1;
                case 2: return type2;
                case 3: return type3;
                case 4: return type4;
            }
        }

        public bool Valid()
        {
            if (type0 == EDataType.None)
                return false;
            return true;
        }

        /// <summary>
        /// 只检查基础类型, 不检查枚举
        /// </summary>
        public bool IsEuqal(DataType t)
        {
            if (IsList != t.IsList)
                return false;
            if (Count != t.Count)
                return false;
            if (type0 != t.type0)
                return false;
            if (type1 != t.type1)
                return false;
            if (type2 != t.type2)
                return false;
            if (type3 != t.type3)
                return false;
            if (type4 != t.type4)
                return false;
            return true;
        }

        public override string ToString()
        {
            return "";
        }
    }
}
