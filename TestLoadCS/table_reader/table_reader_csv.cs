using System;
using System.Collections.Generic;
using Test;

namespace TestLoadCs.table_reader
{
    public class TableReaderCsv : ITableReader
    {
        private CsvReader _csv_reader = null;
        public List<Str> _list_str = new List<Str>();
        public int col_index = 0;
        public TableListReaderCsv _list_reader;
        public TablePairReaderCsv _pair_reader;

        public ETableReaderType ReaderType => ETableReaderType.Csv;

        public void Reset(byte[] buff)
        {
            if (buff == null)
                _csv_reader = null;
            else
                _csv_reader = new CsvReader(buff);
        }

        public List<Str> ReadHeader()
        {
            if (_csv_reader == null)
                return null;

            bool ret = _csv_reader.ReadRow(_list_str, true);
            ret &= _csv_reader.ReadRow(_list_str, false);
            return _list_str;
        }

        public bool NextRow()
        {
            col_index = 0;
            return _csv_reader.ReadRow(_list_str);
        }

        public int ReadCellLength()
        {
            return 0;
        }

        public float ReadF32()
        {
            return _list_str[col_index++].ParseFloat();
        }

        public double ReadF64()
        {
            return _list_str[col_index++].ParseDouble();
        }

        public int ReadInt32()
        {
            return _list_str[col_index++].ParseInt32();
        }

        public long ReadInt64()
        {
            return _list_str[col_index++].ParseInt64();
        }

        public Str ReadString()
        {
            return _list_str[col_index++];
        }

        public uint ReadUInt32()
        {
            return _list_str[col_index++].ParseUInt32();
        }

        public ulong ReadUInt64()
        {
            return _list_str[col_index++].ParseUInt64();
        }

        public static bool ParseBool(Str s)
        {
            if (s.IsEmpty())
                return false;
            return s != "0";
        }

        public bool ReadBool()
        {
            return ParseBool(_list_str[col_index++]);
        }

        public ITablePairReader BeginPair()
        {
            if (_pair_reader == null)
                _pair_reader = new TablePairReaderCsv();
            _pair_reader.Reset(ReadString());
            return _pair_reader;
        }

        public ITableListReader BeginList()
        {
            if (_list_reader == null)
                _list_reader = new TableListReaderCsv();
            _list_reader.Reset(ReadString());
            return _list_reader;
        }
    }

    public class TablePairReaderCsv : ITablePairReader
    {
        private const char C_CSV_PAIR_SPLIT = '|';
        public List<Str> _str_list = new List<Str>();
        public int _index = 0;
        public void Reset(Str str)
        {
            if (str.IsEmpty())
            {
                _str_list.Clear();
            }
            else
                str.Split(C_CSV_PAIR_SPLIT, _str_list);
            _index = 0;
        }

        public int GetCount()
        {
            return _str_list.Count;
        }
        public bool ReadBool() { return TableReaderCsv.ParseBool(_str_list[_index++]); }
        public int ReadInt32() { return _str_list[_index++].ParseInt32(); }
        public uint ReadUInt32() { return _str_list[_index++].ParseUInt32(); }
        public long ReadInt64() { return _str_list[_index++].ParseInt64(); }
        public ulong ReadUInt64() { return _str_list[_index++].ParseUInt64(); }
        public float ReadF32() { return _str_list[_index++].ParseFloat(); }
        public double ReadF64() { return _str_list[_index++].ParseDouble(); }
        public Str ReadString() { return _str_list[_index++]; }
    }


    public class TableListReaderCsv : ITableListReader
    {
        private const char C_CSV_LIST_SPLIT = ';';
        public TablePairReaderCsv _pair_reader;
        public List<Str> _str_list = new List<Str>();
        public int _index = 0;
        public void Reset(Str str)
        {
            if (str.IsEmpty())
                _str_list.Clear();
            else
                str.Split(C_CSV_LIST_SPLIT, _str_list);
            _index = 0;
        }

        public int GetCount()
        {
            return _str_list.Count;
        }

        public bool ReadBool() { return TableReaderCsv.ParseBool(_str_list[_index++]); }
        public int ReadInt32() { return _str_list[_index++].ParseInt32(); }
        public uint ReadUInt32() { return _str_list[_index++].ParseUInt32(); }
        public long ReadInt64() { return _str_list[_index++].ParseInt64(); }
        public ulong ReadUInt64() { return _str_list[_index++].ParseUInt64(); }
        public float ReadF32() { return _str_list[_index++].ParseFloat(); }
        public double ReadF64() { return _str_list[_index++].ParseDouble(); }
        public Str ReadString() { return _str_list[_index++]; }

        public ITablePairReader BeginPair()
        {
            if (_pair_reader == null)
                _pair_reader = new TablePairReaderCsv();
            _pair_reader.Reset(ReadString());
            return _pair_reader;
        }
    }
}
