using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public enum ETableReaderType
    {
        Csv,
        Bin,
    }

    public interface ITableDataReader
    {
        public bool ReadBool();
        public uint ReadUInt32();
        public int ReadInt32();
        public long ReadInt64();
        public ulong ReadUInt64();
        public float ReadF32();
        double ReadF64();
        public Str ReadString();
    }

    public interface ITableReader : ITableDataReader
    {
        public ETableReaderType ReaderType { get; }
        public List<Str> ReadHeader();
        public bool NextRow();

        public ITablePairReader BeginPair();
        public ITableListReader BeginList();
    }

    public interface ITableListReader : ITableDataReader
    {
        public int GetCount();
        public ITablePairReader BeginPair();
    }

    public interface ITablePairReader : ITableDataReader
    {
        public int GetCount();
    }
}
