using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace ExportExcel
{
    public class Exporter_Bson : IProcessNode
    {
        public EExportFlag _flag;
        public Config.BsonConfig _config;
        public Exporter_Bson(EExportFlag flag, Config.BsonConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export bson";
        }

        public void Process(DataBase data_base)
        {
            if (_config == null || !_config.enable)
                return;

            foreach (var p in data_base.Tables)
            {
                //判断是否需要导出
                if ((p.Value.TableExportFlag & _flag) == 0)
                    continue;

                List<FilterTable> multi_lang_tables = Exporter_Json.GetMultiLangTable(p.Value, _flag);

                foreach (var table in multi_lang_tables)
                {
                    string out_file_path = Path.Combine(_config.dir, table.SheetName + ".bson");
                    FileUtil.CreateFileDir(out_file_path);
                    
                    var data = ConvertToBsonObj(table, true);

                    using FileStream fs = new FileStream(out_file_path, FileMode.Create, FileAccess.Write);
                    using BsonBinaryWriter bsonWriter = new BsonBinaryWriter(fs);
                    MongoDB.Bson.Serialization.BsonSerializer.Serialize(bsonWriter, data);
                }
            }
        }


        public static BsonValue ConvertToBsonObj(FilterTable table, bool with_header)
        {            
            BsonValue ret = _CreateTableData(table);
            if (!with_header)
                return ret;

            var temp = new BsonDocument();
            temp["Header"] = _CreateTableHeader(table.GetHeader());
            temp["Data"] = ret;            
            return temp;
        }

        private static BsonArray _CreateTableData(FilterTable table)
        {
            int row_count = table.RowCount;
            var header = table.GetHeader();

            BsonArray array = new BsonArray();
            for (int i = 0; i < row_count; i++)
            {
                array.Add(_CreateRowData(table, header, i));
            }
            return array;
        }

        private static BsonValue _CreateRowData(FilterTable table, List<TableField> header, int row)
        {
            BsonDocument ret = new BsonDocument();
            for (int i = 0; i < header.Count; i++)
            {
                var header_item = header[i];
                ret[header_item.Name] = _ParseCellData(table[row, i], header_item);
            }
            return ret;
        }

        private static BsonValue _ParseCellData(string data, TableField header_item)
        {
            if (header_item.DataType.IsList)
            {
                string[] array = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
                BsonArray ret = new BsonArray();

                if (header_item.DataType.IsTuple)
                {
                    foreach (var p in array)
                        ret.Add(_ParseTuple(p, header_item.DataType));
                }
                else
                {
                    foreach (var p in array)
                        ret.Add(_ParseBaseData(p, header_item.DataType.Get(0)));
                }
                return ret;
            }
            else if (header_item.DataType.IsTuple)
            {
                return _ParseTuple(data, header_item.DataType);
            }
            else
            {
                return _ParseBaseData(data, header_item.DataType.Get(0));
            }
        }

        private static BsonValue _ParseTuple(string data, DataType data_type)
        {
            string[] array = data.Split('|');
            BsonDocument ret = new BsonDocument();
            for (int i = 0; i < array.Length; i++)
            {
                ret["Item" + (i + 1)] = _ParseBaseData(array[i], data_type.Get(i));
            }
            return ret;
        }

        private static BsonValue _ParseBaseData(string data, EDataType data_type)
        {
            switch (data_type)
            {
                case EDataType.String:
                case EDataType.LocStr:
                    return new BsonString(data);
                case EDataType.Bool:
                    return new BsonBoolean(data == "1");

                case EDataType.Int32:
                case EDataType.LocId:
                    return string.IsNullOrEmpty(data) ? new BsonInt32((int)0) : new BsonInt32(int.Parse(data));
                case EDataType.UInt32:
                    return string.IsNullOrEmpty(data) ? new BsonInt32((int)0) : new BsonInt32((int)uint.Parse(data));

                case EDataType.Int64:
                    return string.IsNullOrEmpty(data) ? new BsonInt64((long)0) : new BsonInt64(long.Parse(data));
                case EDataType.UInt64:
                    return string.IsNullOrEmpty(data) ? new BsonInt64((long)0) : new BsonInt64((long)ulong.Parse(data));

                case EDataType.Float32:
                    return string.IsNullOrEmpty(data) ? new BsonDouble((float)0) : new BsonDouble(float.Parse(data));
                case EDataType.Float64:
                    return string.IsNullOrEmpty(data) ? new BsonDouble((double)0) : new BsonDouble(double.Parse(data));

                default:
                    throw new Exception("未知类型 " + data_type);
            }
        }

        private static BsonArray _CreateTableHeader(List<TableField> list)
        {
            BsonArray ret = new BsonArray();

            foreach (var p in list)
            {
                BsonDocument node = new BsonDocument();
                node["Name"] = p.Name;
                node["Type"] = p.DataType.ToCsvStr();

                ret.Add(node);
            }
            return ret;
        }
    }
}
