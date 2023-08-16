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
                ret.Add(new PPConstraintFilePath(config.validation.search_file_root));
            }

            //5. 多语言到LocId
            {
                ret.Add(new PPLocalizationHashId());
            }

            //6. 导出
            {
                ProcessNodeList node_list = new ProcessNodeList("导出");
                ret.Add(node_list);
                node_list.Add(new Exporter_CSStruct());
                node_list.Add(new Exporter_CSLoader());
                node_list.Add(new Exporter_CSIds());

                node_list.Add(new Exporter_LuaIds());
                node_list.Add(new Exporter_LuaStruct());
                node_list.Add(new Exporter_LuaLoader());
                node_list.Add(new Exporter_LuaStructDef());

                node_list.Add(new ExporterGOStruct());
                node_list.Add(new ExporterGOLoader());

                node_list.Add(new ExporterCSV(E_EXPORT_FLAG.client));
                node_list.Add(new ExporterCSV(E_EXPORT_FLAG.svr));
                node_list.Add(new Exporter_BinData());

                node_list.Add(new Exporter_LangTrans());

                //node_list.Add(new Exporter_Rule(@"D:\work\p4_dev\Trunk\Design\rules"));
            }
            return ret;
        }
    }
}
