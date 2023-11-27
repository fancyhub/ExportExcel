using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ExportExcel
{

    public class Exporter_Schema : IProcessNode
    {
        public Config.SchemaConfig _config;
        public Exporter_Schema(Config.SchemaConfig config)
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
            ret.ExportServer = (table.TableExportFlag & EExportFlag.svr) != EExportFlag.none;
            ret.ExportClient = (table.TableExportFlag & EExportFlag.client) != EExportFlag.none;
            ret.IsMultiLang = table.MultiLangBody != null;

            var pk = table.Header.Pk;
            string secKey = null;
            if (pk != null)
            {
                secKey = pk.AttrPK._sec_key_col_name;
            }

            foreach (var item in table.Header.List)
            {
                ret.Columns.Add(_CreateColumn(item, secKey));
            }
            return ret;
        }

        private SchemaColumn _CreateColumn(TableField item, string sec_key_name)
        {
            SchemaColumn ret = new SchemaColumn();
            ret.Name = item.Name;
            ret.Desc = item.Desc;
            ret.ExportServer = (item.ExportFlag & EExportFlag.svr) != EExportFlag.none;
            ret.ExportClient = (item.ExportFlag & EExportFlag.client) != EExportFlag.none;
            ret.Constraint = _CreateSchemaConstraint(item, sec_key_name);

            DataType data_type = item.DataType;
            for (int i = 0; i < data_type.Count; i++)
            {
                ret.Types.Add(data_type.Get(i).ToString());
            }
            return ret;
        }

        private SchemaConstraint _CreateSchemaConstraint(TableField item, string sec_key_name)
        {
            SchemaConstraint ret = new SchemaConstraint();
            if (item.StrConstraints != null)
                ret.Constraints.AddRange(item.StrConstraints);

            if (item.AttrPK != null)
            {
                ret.Key = ESchemaKey.PK.ToString();
            }
            else if (item.Name == sec_key_name)
            {
                ret.Key = ESchemaKey.SecondKey.ToString();
            }

            if (item.AttrEnum != null)
            {
                ret.Enum = item.AttrEnum.Name;
            }

            if (item.AttrTupleAlias != null)
            {
                ret.TupleAlias = item.AttrTupleAlias.AliasName;
            }

            ret.Unique = item.AttrUnique;
            ret.BlankForbid = item.AttrBlankForbid;
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

            public SchemaConstraint Constraint = new SchemaConstraint();
        }

        public class SchemaConstraint
        {
            public string Key;
            public string Enum;
            public string TupleAlias;
            public bool Unique = false;
            public bool BlankForbid = false;

            public List<string> Constraints = new List<string>();
        }
    }
}
