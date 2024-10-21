using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExportExcel
{
    public class Exporter_RuleSchema : IProcessNode
    {
        public Config.SchemaConfig _config;
        public Exporter_RuleSchema(Config.SchemaConfig config)
        {
            _config = config;
        }

        public string GetName()
        {
            return "Export Schema.json";
        }

        public void Process(DataBase data_base)
        {
            if (_config == null || !_config.enable)
                return;

            Schema schema = new Schema();
            foreach (var p in data_base.EnumDB)
            {
                schema.Enums.Add(_CreateEnumSchema(p.Value));
            }
            schema.TupleAlias = new List<AliasItem>(data_base.AliasDB._Dict.Values);

            foreach (var p in data_base.Tables)
            {
                schema.Tables.Add(_CreateTableSchema(p.Value));
            }


            string file_path = System.IO.Path.Combine(_config.dir, "schema.json");
            FileUtil.CreateFileDir(file_path);
            System.IO.File.WriteAllText(file_path, JsonConvert.SerializeObject(schema, Formatting.Indented));
        }

        private SchemaEnum _CreateEnumSchema(EnumType enumType)
        {
            SchemaEnum ret = new SchemaEnum();
            ret.Name = enumType.Name;
            foreach (var child in enumType.Dict)
            {
                SchemaEnumField field = new SchemaEnumField();
                field.Name = child.Value.Name;
                field.Comment = child.Value.ExcelVal;
                field.Value = child.Value.Val;
                ret.Fields.Add(field);
            }
            return ret;
        }

        private SchemaTable _CreateTableSchema(Table table)
        {
            SchemaTable ret = new SchemaTable();
            ret.SheetName = table.SheetName;
            ret.ExportServer = (table.TableExportFlag & EExportFlagMask.Server) != EExportFlagMask.None;
            ret.ExportClient = (table.TableExportFlag & EExportFlagMask.Client) != EExportFlagMask.None;
            ret.IsMultiLang = table.MultiLangBody != null;

            foreach (var item in table.Header.List)
            {
                ret.Columns.Add(_CreateColumn(item));
            }
            return ret;
        }

        private SchemaColumn _CreateColumn(TableField item)
        {
            SchemaColumn ret = new SchemaColumn();
            ret.Name = item.Name;
            ret.Desc = item.Desc;
            ret.ExportServer = (item.ExportFlag & EExportFlagMask.Server) != EExportFlagMask.None;
            ret.ExportClient = (item.ExportFlag & EExportFlagMask.Client) != EExportFlagMask.None;
            DataType data_type = item.DataType;
            for (int i = 0; i < data_type.Count; i++)
            {
                ret.Types.Add(data_type.Get(i).ToString());
            }

            ret.Constraints = _CreateSchemaConstraints(item);

            return ret;
        }

        private List<SchemaConstraint> _CreateSchemaConstraints(TableField item)
        {
            List<SchemaConstraint> ret = new();
            if (item.AttrPK != null)
            {
                var t = new SchemaConstraint("PK");

                List<string> tt = new List<string>();
                foreach (var p in item.AttrPK.SubKeys)
                {
                    tt.Add(p.Name);
                }
                t.Value = string.Join(',', tt);
                ret.Add(t);
            }

            if (item.AttrUnique != null)
            {
                ret.Add(new SchemaConstraint("Unique"));
            }

            if (item.AttrBlankForbid != null)
            {
                ret.Add(new SchemaConstraint("BlankForbid"));
            }

            if (item.AttrDefault != null)
            {
                ret.Add(new SchemaConstraint("Default", item.AttrDefault.Value));
            }

            if (item.AttrEnum != null)
            {
                ret.Add(new SchemaConstraint("Enum", item.AttrEnum.Name));
            }

            if (item.AttrAlias != null)
            {
                ret.Add(new SchemaConstraint("Alias", item.AttrAlias.Name));
            }

            if (item.AttrLookUp != null)
            {
                ret.Add(new SchemaConstraint("Lookup", item.AttrLookUp.ToString()));
            }

            if (item.AttrFilePath != null)
            {
                ret.Add(new SchemaConstraint("FilePath", item.AttrFilePath.ToString()));
            }

            if (item.AttrRange != null)
            {
                ret.Add(new SchemaConstraint("Range", item.AttrRange.ToString()));
            }
            return ret;
        }

        public enum ESchemaKey
        {
            None,
            PK,
            SecondKey,
        }

        public class Schema
        {
            public List<SchemaEnum> Enums = new List<SchemaEnum>();
            public List<AliasItem> TupleAlias = new List<AliasItem>();
            public List<SchemaTable> Tables = new List<SchemaTable>();
        }

        public class SchemaTable
        {
            public string SheetName;
            public bool ExportClient;
            public bool ExportServer;
            public bool IsMultiLang;
            public List<SchemaColumn> Columns = new List<SchemaColumn>();
        }

        public class SchemaEnum
        {
            public string Name;
            public List<SchemaEnumField> Fields = new List<SchemaEnumField>();
        }

        public class SchemaEnumField
        {
            public string Name;
            public int Value;
            public string Comment;
        }

        public class SchemaColumn
        {
            public string Name;
            public string Desc;
            public bool ExportClient;
            public bool ExportServer;
            public bool IsList;
            public List<string> Types = new List<string>();
            public List<SchemaConstraint> Constraints = new List<SchemaConstraint>();
        }

        public class SchemaConstraint
        {
            public string Type;
            public string Value;
            public SchemaConstraint(string type)
            {
                this.Type = type;
            }
            public SchemaConstraint(string type, string value)
            {
                Type = type;
                Value = value;
            }
        }
    }
}
