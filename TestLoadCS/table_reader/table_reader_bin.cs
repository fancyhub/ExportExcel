using System;
using System.Collections.Generic;
using Test;

namespace TestLoadCs.table_reader
{
    public class TableReaderBin : ITableReader
    {
        public MemoryStream _ms;
        public BinaryReader _reader;
        public ETableReaderType ReaderType => ETableReaderType.Bin;
        public Dictionary<string, (int offset, int len)> _table_dict;
        public List<string> _str_list;
        public List<Str> _str_list_header = new List<Str>();
        public TableListReaderBin _list_reader;
        public TablePairReaderBin _pair_reader;
        public int _row_count;
        public int _row_index;

        public string CurLang;

        public void Reset(byte[] buff)
        {
            _ms = null;
            _reader = null;
            if (buff == null)
                return;

            _ms = new MemoryStream(buff);
            _reader = new BinaryReader(_ms);

            int sign = _reader.ReadInt32();
            //Read Str
            {
                int body_len = _reader.Read7BitEncodedInt();
                int count = _reader.Read7BitEncodedInt();
                _str_list = new List<string>(count);
                for (int i = 0; i < count; i++)
                {
                    _str_list.Add(_reader.ReadString());
                }
            }

            //table index
            {
                int body_len = _reader.Read7BitEncodedInt();
                int count = _reader.Read7BitEncodedInt();
                _table_dict = new Dictionary<string, (int offset, int len)>(count);
                for (int i = 0; i < count; i++)
                {
                    string sheet_name = _ReadRefString();
                    int offset = _reader.ReadInt32();
                    int len = _reader.Read7BitEncodedInt();
                    _table_dict.Add(sheet_name, (offset, len));
                }
            }
        }

        private string _ReadRefString()
        {
            int idx = _reader.Read7BitEncodedInt();
            return _str_list[idx];
        }

        public bool Start(string sheet_name)
        {
            if (!_table_dict.TryGetValue(sheet_name, out var t))
                return false;

            _ms.Seek(t.offset, SeekOrigin.Begin);
            _row_count = 0;
            _row_index = 0;
            return true;
        }

        public List<Str> ReadHeader()
        {
            _str_list_header.Clear();
            int count = _reader.Read7BitEncodedInt();
            for (int i = 0; i < count; i++)
            {
                _str_list_header.Add(_ReadRefString());
            }

            _row_count = _reader.Read7BitEncodedInt();
            _row_index = 0;
            return _str_list_header;
        }

        public bool NextRow()
        {
            _row_index++;
            if (_row_index > _row_count)
                return false;
            int row_len = _reader.Read7BitEncodedInt();
            return true;
        }

        public bool ReadBool()
        {
            return _reader.ReadByte() == 1;
        }

        public float ReadF32()
        {
            return _reader.ReadSingle();
        }

        public double ReadF64()
        {
            return _reader.ReadDouble();
        }

        public int ReadInt32()
        {
            return _reader.Read7BitEncodedInt();
        }
        public uint ReadUInt32()
        {
            return (uint)_reader.Read7BitEncodedInt();
        }

        public long ReadInt64()
        {
            return _reader.Read7BitEncodedInt64();
        }

        public ulong ReadUInt64()
        {
            return (ulong)_reader.Read7BitEncodedInt64();
        }

        public Str ReadString()
        {
            return _ReadRefString();
        }

        public ITableListReader BeginList()
        {
            if (_list_reader == null)
            {
                _list_reader = new TableListReaderBin();
            }

            _list_reader.Reset(this, _reader.Read7BitEncodedInt());
            return _list_reader;
        }

        public ITablePairReader BeginPair()
        {
            if (_pair_reader == null)
                _pair_reader = new TablePairReaderBin();
            _pair_reader.Reset(this, _reader.Read7BitEncodedInt());
            return _pair_reader;
        }
    }

    public class TableListReaderBin : ITableListReader
    {
        public TablePairReaderBin _pair_reader;
        public ITableReader _orig_reader;
        public int _count;
        public void Reset(ITableReader reader, int count)
        {
            _orig_reader = reader;
            _count = count;
        }

        public int GetCount()
        {
            return _count;
        }

        public ITableReader OrigReader => _orig_reader;

        public bool ReadBool() { return _orig_reader.ReadBool(); }
        public int ReadInt32() { return _orig_reader.ReadInt32(); }
        public uint ReadUInt32() { return _orig_reader.ReadUInt32(); }
        public long ReadInt64() { return _orig_reader.ReadInt64(); }
        public ulong ReadUInt64() { return _orig_reader.ReadUInt64(); }
        public float ReadF32() { return _orig_reader.ReadF32(); }
        public double ReadF64() { return _orig_reader.ReadF64(); }
        public Str ReadString() { return _orig_reader.ReadString(); }

        public ITablePairReader BeginPair()
        {
            if (_pair_reader == null)
                _pair_reader = new TablePairReaderBin();
            _pair_reader.Reset(_orig_reader, 0);
            return _pair_reader;
        }
    }


    public class TablePairReaderBin : ITablePairReader
    {
        public ITableReader _orig_reader;
        public int _count;
        public void Reset(ITableReader reader, int count)
        {
            _orig_reader = reader;
            _count = count;
        }

        public int GetCount()
        {
            return _count;
        }

        public bool ReadBool() { return _orig_reader.ReadBool(); }
        public int ReadInt32() { return _orig_reader.ReadInt32(); }
        public uint ReadUInt32() { return _orig_reader.ReadUInt32(); }
        public long ReadInt64() { return _orig_reader.ReadInt64(); }
        public ulong ReadUInt64() { return _orig_reader.ReadUInt64(); }
        public float ReadF32() { return _orig_reader.ReadF32(); }
        public double ReadF64() { return _orig_reader.ReadF64(); }
        public Str ReadString() { return _orig_reader.ReadString(); }
    }
}
