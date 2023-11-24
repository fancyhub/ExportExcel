using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

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

                    var data = Exporter_Json.ConvertToJsonObj(table, _config.header);


                    using FileStream fs = new FileStream(out_file_path, FileMode.Create, FileAccess.Write);
                    using BsonWriter bsonWriter = new BsonWriter(fs);
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(bsonWriter, data);                     
                }
            }
        }
    }
}
