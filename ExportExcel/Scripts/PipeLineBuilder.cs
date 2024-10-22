using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/6 13:39:48
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public class PipelineBuilder
    {
        public static PipeLine CreatePipeLine(Config config)
        {
            PipeLine ret = new PipeLine(config);

            //1第一步
            {
                ret.Add(new TableLoader(config));
            }

            //2.
            {
                ret.Add(new PPconstraintParser());
            }
            
            //3. 处理转换
            {
                ret.Add(new PPConstraintDefault()); //默认值的检查
                ret.Add(new PostProcessRef());      //
                ret.Add(new PPConstraintEnum());
            }
 
            //4. 检查
            {                
                ret.Add(new PPFieldTypeChecker());
                ret.Add(new PPConstraint_Unique_LoopUp_BlankForbid());
                ret.Add(new PPConstraintRange());
                ret.Add(new PPConstraintFilePath(config.validation.searchFileRoot));
            }

            //5. 多语言到LocId
            {
                ret.Add(new PPLocalization());
                ret.Add(new PPLocalizationHashId());
            }

            //6. 导出
            {
                ProcessNodeList node_list = new ProcessNodeList("导出");
                ret.Add(node_list);
                _AddExporter(node_list, EExportFlag.Client, config.exportClient);
                _AddExporter(node_list, EExportFlag.Server, config.exportServer);


                node_list.Add(new Exporter_ExcelLangTrans(config.exportCommon.localizationTranslate));
                node_list.Add(new Exporter_RuleExcel(config.exportCommon.ruleExcel));
                node_list.Add(new Exporter_RuleSchema(config.exportCommon.schema));
            }
            return ret;
        }

        private static void _AddExporter(ProcessNodeList node_list, EExportFlag flag, Config.ExportConfig config)
        {
            node_list.Add(new Exporter_CSStructItem(flag, config.csharp));
            node_list.Add(new Exporter_CSStructTable(flag, config.csharp));
            node_list.Add(new Exporter_CSLoaderFromCsv(flag, config.csharp));
            node_list.Add(new Exporter_CSLoaderFromJson(flag, config.csharp));            
            node_list.Add(new Exporter_CSLocDef(flag, config.csharp));

            node_list.Add(new Exporter_CppStructItem(flag, config.cpp));
            node_list.Add(new Exporter_CppStructTable(flag, config.cpp));
            node_list.Add(new Exporter_CppLoaderFromCsv(flag, config.cpp));

            node_list.Add(new Exporter_LuaIds(flag, config.lua));
            node_list.Add(new Exporter_LuaStructDef(flag, config.lua));

            node_list.Add(new Exporter_DataCSV(flag, config.csv));
            node_list.Add(new Exporter_DataBinCsv(flag, config.bin));
            node_list.Add(new Exporter_DataJson(flag, config.json));
            node_list.Add(new Exporter_DataBson(flag, config.bson));

            node_list.Add(new Exporter_GoStructItem(flag, config.go));

        }
    }
}
