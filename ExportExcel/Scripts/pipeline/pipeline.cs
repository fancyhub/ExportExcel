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
    public interface IProcessNode
    {
        public string GetName();
        void Process(DataBase data_base);
    }

    public class ProcessNodeList : IProcessNode
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
        public List<IProcessNode> _all_child_nodes = new List<IProcessNode>();

        public void Add(IProcessNode node)
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
        public List<IProcessNode> _node_list = new List<IProcessNode>();
        public ExeConfig _config;
        public PipeLine(ExeConfig conf)
        {
            _config = conf;
        }

        public void Add(IProcessNode node)
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
