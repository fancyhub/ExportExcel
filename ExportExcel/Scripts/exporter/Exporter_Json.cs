using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ExportExcel
{
    public class Exporter_Json : IProcessNode
    {
        public static System.Text.Encodings.Web.JavaScriptEncoder Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
        public EExportFlag _flag;
        public ExeConfig.JsonConfig _config;
        public Exporter_Json(EExportFlag flag, ExeConfig.JsonConfig config)
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

                List<FilterTable> multi_lang_tables = _GetMultiLangTable(p.Value);

                foreach (var table in multi_lang_tables)
                {
                    string out_file_path = Path.Combine(_config.dir, table.SheetName + ".json");
                    FileUtil.CreateFileDir(out_file_path);

                    JsonObject root = new JsonObject();
                    root["Header"] = _CreateTableHeader(table.Header);
                    root["Data"] = _CreateTableData(table);


                    using (Stream sw = new FileStream(out_file_path, FileMode.Create, FileAccess.Write))
                    {
                        Utf8JsonWriter writer = new Utf8JsonWriter(sw, new JsonWriterOptions()
                        {
                            Indented = true,
                            Encoder = Encoder,
                        });
                        root.WriteTo(writer);
                        writer.Flush();
                    }
                }
            }
        }

        private static JsonNode _CreateTableData(FilterTable table)
        {
            int row_count = table.RowCount;
            var header = table.Header;

            List<JsonNode> array = new List<JsonNode>(row_count);
            for (int i = 0; i < row_count; i++)
            {
                array.Add(_CreateRowData(table, header, i));
            }

            return new JsonArray(array.ToArray());
        }

        private static JsonNode _CreateRowData(FilterTable table, List<TableHeaderItem> header, int row)
        {
            JsonObject ret = new JsonObject();
            for (int i = 0; i < header.Count; i++)
            {
                var header_item = header[i];
                ret[header_item.Name] = _ParseCellData(table[row, i], header_item);
            }
            return ret;
        }

        private static JsonNode _ParseCellData(string data, TableHeaderItem header_item)
        {
            if (header_item.DataType.IsList)
            {
                string[] array = data.Split(';', StringSplitOptions.RemoveEmptyEntries);
                JsonArray ret = new JsonArray();

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

        private static JsonNode _ParseTuple(string data, DataType data_type)
        {
            string[] array = data.Split('|');
            JsonObject ret = new JsonObject();
            for (int i = 0; i < array.Length; i++)
            {
                ret["Item" + (i + 1)] = _ParseBaseData(array[i], data_type.Get(i));
            }
            return ret;
        }

        private static JsonNode _ParseBaseData(string data, EDataType data_type)
        {
            switch (data_type)
            {
                case EDataType.String:
                case EDataType.LocStr:
                    return data;
                case EDataType.Bool:
                    return data == "1";

                case EDataType.Int64:
                case EDataType.UInt64:
                case EDataType.LocId:
                case EDataType.Int32:
                case EDataType.UInt32:
                    if (string.IsNullOrEmpty(data))
                        return 0;
                    return long.Parse(data);

                case EDataType.Float32:
                case EDataType.Float64:
                    if (string.IsNullOrEmpty(data))
                        return 0;
                    return double.Parse(data);

                default:
                    throw new Exception("未知类型 " + data_type);
            }
        }

        private static JsonNode _CreateTableHeader(List<TableHeaderItem> list)
        {
            JsonArray ret = new JsonArray();

            foreach (var p in list)
            {
                JsonObject node = new JsonObject();
                node["Name"] = p.Name;
                node["Type"] = p.DataType.ToCsvStr();

                ret.Add(node);
            }
            return ret;
        }

        private List<FilterTable> _GetMultiLangTable(Table table)
        {
            List<FilterTable> ret = new List<FilterTable>();
            if (table.MultiLangBody == null)
            {
                ret.Add(new FilterTable(table, _flag));
                return ret;
            }

            foreach (var p in table.MultiLangBody)
            {
                FilterTable t = new FilterTable(table, _flag);
                t._body = p.Value;
                t.SheetName = table.SheetName + "_" + p.Key;
                t._row_count = p.Value.GetLength(0);
                ret.Add(t);
            }
            return ret;
        }
    }
}
