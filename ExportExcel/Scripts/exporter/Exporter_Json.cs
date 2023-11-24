using System;
using System.Collections.Generic;
using System.IO;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace ExportExcel
{
    public class Exporter_Json : IProcessNode
    {
        public static System.Text.Encodings.Web.JavaScriptEncoder Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
        public EExportFlag _flag;
        public Config.JsonConfig _config;
        public Exporter_Json(EExportFlag flag, Config.JsonConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export json";
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

                List<FilterTable> multi_lang_tables = GetMultiLangTable(p.Value, _flag);

                foreach (var table in multi_lang_tables)
                {
                    string out_file_path = Path.Combine(_config.dir, table.SheetName + ".json");
                    FileUtil.CreateFileDir(out_file_path);

                    var data = ConvertToJsonObj(table, _config.header);
                    using (StreamWriter sw = new StreamWriter(out_file_path, false, System.Text.Encoding.UTF8))
                    {
                        sw.Write(data.ToString(Newtonsoft.Json.Formatting.Indented));
                    } 
                }
            }
        }

        public static List<FilterTable> GetMultiLangTable(Table table, EExportFlag flag)
        {
            List<FilterTable> ret = new List<FilterTable>();
            if (table.MultiLangBody == null)
            {
                ret.Add(new FilterTable(table, flag));
                return ret;
            }

            foreach (var p in table.MultiLangBody)
            {
                FilterTable t = new FilterTable(table, flag);
                t._body = p.Value;
                t.SheetName = table.SheetName + "_" + p.Key;
                t._row_count = p.Value.GetLength(0);
                ret.Add(t);
            }
            return ret;
        }

        public static JToken ConvertToJsonObj(FilterTable table, bool with_header)
        {
            JToken ret = _CreateTableData(table);
            if (!with_header)
                return ret;

            var temp = new JObject();
            temp["Header"] = _CreateTableHeader(table.GetHeader());
            temp["Data"] = ret;
            return temp;
        }

        private static JArray _CreateTableData(FilterTable table)
        {
            int row_count = table.RowCount;
            var header = table.GetHeader();

            JArray array = new JArray();
            for (int i = 0; i < row_count; i++)
            {
                array.Add(_CreateRowData(table, header, i));
            }
            return array;
        }

        private static JToken _CreateRowData(FilterTable table, List<TableField> header, int row)
        {
            JObject ret = new JObject();
            for (int i = 0; i < header.Count; i++)
            {
                var header_item = header[i];
                ret[header_item.Name] = _ParseCellData(table[row, i], header_item);
            }
            return ret;
        }

        private static JToken _ParseCellData(string data, TableField header_item)
        {
            if (header_item.DataType.IsList)
            {
                string[] array = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
                JArray ret = new JArray();

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

        private static JObject _ParseTuple(string data, DataType data_type)
        {
            string[] array = data.Split('|');
            JObject ret = new JObject();
            for (int i = 0; i < array.Length; i++)
            {
                ret["Item" + (i + 1)] = _ParseBaseData(array[i], data_type.Get(i));
            }
            return ret;
        }

        private static JValue _ParseBaseData(string data, EDataType data_type)
        {
            switch (data_type)
            {
                case EDataType.String:
                case EDataType.LocStr:
                    return new JValue(data);
                case EDataType.Bool:
                    return new JValue(data == "1");

                case EDataType.Int32:
                case EDataType.LocId:
                    return string.IsNullOrEmpty(data) ? new JValue((int)0) : new JValue(int.Parse(data));
                case EDataType.UInt32:
                    return string.IsNullOrEmpty(data) ? new JValue((uint)0) : new JValue(uint.Parse(data));

                case EDataType.Int64:
                    return string.IsNullOrEmpty(data) ? new JValue((long)0) : new JValue(long.Parse(data));
                case EDataType.UInt64:
                    return string.IsNullOrEmpty(data) ? new JValue((ulong)0) : new JValue(ulong.Parse(data));

                case EDataType.Float32:
                    return string.IsNullOrEmpty(data) ? new JValue((float)0) : new JValue(float.Parse(data));
                case EDataType.Float64:
                    return string.IsNullOrEmpty(data) ? new JValue((double)0) : new JValue(double.Parse(data));

                default:
                    throw new Exception("未知类型 " + data_type);
            }
        }

        private static JArray _CreateTableHeader(List<TableField> list)
        {
            JArray ret = new JArray();

            foreach (var p in list)
            {
                JObject node = new JObject();
                node["Name"] = p.Name;
                node["Type"] = p.DataType.ToCsvStr();

                ret.Add(node);
            }
            return ret;
        }


    }
}
