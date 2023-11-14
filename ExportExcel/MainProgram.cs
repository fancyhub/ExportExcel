using System;
namespace ExportExcel
{
    public class MainProgram
    {
        public static int Main(string[] args)
        {
            Logger.Print("载入 config.json");
            ExeConfig config = ExeConfig.Load();
            if (config == null)
                return -2;


            PipeLine pipeline = PipelineBuilder.CreatePipeLine(config);

            if (!_is_watch_mode(args))
                return pipeline.Process(true);
            else
            {
                FileWatcher watcher = FileWatcher.Create(config);
                return Watch(watcher, pipeline);
            }
        }

        public static int Watch(FileWatcher watch, PipeLine pipeline)
        {
            pipeline.Process(false);
            Console.WriteLine("=================Waching===================");
            for (; ; )
            {
                var changes = watch.Select();
                if (changes.Count == 0)
                {
                    System.Threading.Thread.Sleep(1000);//1秒
                    continue;
                }
                Console.WriteLine(DateTime.Now.ToString() + " has Changed");
                //changes.Print();

                pipeline.Process(false);
                Console.WriteLine("=================Waching===================");

                GC.Collect();
            }
            return 1;
        }

        public static bool _is_watch_mode(string[] args)
        {
            foreach (var p in args)
            {
                if (p.ToLower() == "watch")
                    return true;
            }
            return false;
        }
    }
}


