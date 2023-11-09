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
        public static PipeLine CreatePipeLine(ExeConfig config)
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

            //3. 处理引用
            {
                ret.Add(new PostProcessRef());
            }

            //3. 处理多语言
            {
                ret.Add(new PPLocalization());                
            }

            //4. 检查
            {
                ret.Add(new PPConstraintEnum());
                ret.Add(new PPConstraint_Unique_LoopUp_BlankForbid());
                ret.Add(new PPFieldTypeChecker());
                ret.Add(new PPConstraintRange());
                ret.Add(new PPConstraintFilePath(config.validation.searchFileRoot));
            }

            //5. 多语言到LocId
            {
                ret.Add(new PPLocalizationHashId());
            }

            //6. 导出
            {
                ProcessNodeList node_list = new ProcessNodeList("导出");
                ret.Add(node_list);
                _AddExporter(node_list, E_EXPORT_FLAG.client, config.exportClient);
                _AddExporter(node_list, E_EXPORT_FLAG.svr, config.exportServer);


                node_list.Add(new Exporter_LangTrans());

                //node_list.Add(new Exporter_Rule(@"D:\work\p4_dev\Trunk\Design\rules"));
            }
            return ret;
        }

        private static void _AddExporter(ProcessNodeList node_list, E_EXPORT_FLAG flag, ExeConfig.ExportConfig config)
        {
            node_list.Add(new Exporter_CSStruct(flag, config.csharp));
            node_list.Add(new Exporter_CSLoader(flag, config.csharp));
            node_list.Add(new Exporter_CSIds(flag, config.csharp));

            node_list.Add(new Exporter_LuaIds(flag, config.lua));
            node_list.Add(new Exporter_LuaStruct(flag, config.lua));
            node_list.Add(new Exporter_LuaLoader(flag, config.lua));
            node_list.Add(new Exporter_LuaStructDef(flag, config.lua));

            node_list.Add(new ExporterCSV(flag, config.csv));
            node_list.Add(new Exporter_BinData(flag, config.bin));

            node_list.Add(new ExporterGOStruct(flag, config.go));
            node_list.Add(new ExporterGOLoader(flag, config.go));

        }
    }
}
