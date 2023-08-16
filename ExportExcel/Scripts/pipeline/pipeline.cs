using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/6 12:00:10
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    public interface I_ProcessNode
    {
        public string GetName();
        void Process(DataBase data_base);
    }

    public class ProcessNodeList : I_ProcessNode
    {
        public string _name;
        public ProcessNodeList(string name)
        {
            _name = name;
        }
        public string GetName()
        {
            return _name;
        }
        public List<I_ProcessNode> _all_child_nodes = new List<I_ProcessNode>();

        public void Add(I_ProcessNode node)
        {
            _all_child_nodes.Add(node);
        }

        public void Process(DataBase data_base)
        {
            foreach (var p in _all_child_nodes)
            {
                p.Process(data_base);
            }
        }
    }

    public class PipeLine
    {
        public List<I_ProcessNode> _node_list = new List<I_ProcessNode>();
        public ExeConfig _config;
        public PipeLine(ExeConfig conf)
        {
            _config = conf;
        }

        public void Add(I_ProcessNode node)
        {
            _node_list.Add(node);
        }

        public int Process(bool error_pause)
        {
            ErrSet.Clear();
            int count = _node_list.Count;
            int index = 1;
            DataBase data_base = new DataBase(_config);
            foreach (var p in _node_list)
            {
                try
                {
                    Console.WriteLine($"{index}/{count}\t{p.GetName()}");
                    index++;
                    p.Process(data_base);
                }
                catch (Exception e)
                {
                    ErrSet.E(e.Message + "\n" + e.StackTrace);
                }

                if (ErrSet.HasError())
                {
                    break;
                }
            }

            if (ErrSet.HasError())
            {
                if (error_pause)
                {
                    Console.WriteLine("有错误, 按任意键继续=========");
                    Console.ReadKey();
                }
                return -1;
            }
            return 0;
        }
    }
}
