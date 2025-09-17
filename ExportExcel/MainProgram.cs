using System;
namespace ExportExcel
{
    public enum ECmdArg
    {
        NormalMode,
        WatchMode,
        ShowUsage,
        CreateConfig,
        FormatConfig,
    }

    public class MainProgram
    {
        public const string C_FILE_NAME = "config.json";
        public static string GetConfigFilePath()
        {
            string dir = Environment.CurrentDirectory;
            return System.IO.Path.Combine(dir, C_FILE_NAME);
        }

        public static int Main(string[] args)
        {
            ECmdArg arg = ParseArgs(args);
            switch (arg)
            {
                default:
                case ECmdArg.ShowUsage:
                    DisplayUsage();
                    return 0;

                case ECmdArg.CreateConfig:
                    new Config().Save(GetConfigFilePath());
                    return 0;

                case ECmdArg.FormatConfig:
                    {
                        Config config = Config.Load(GetConfigFilePath());
                        if (config == null)
                            config = new Config();
                        config.Save(GetConfigFilePath());
                    }
                    return 0;

                case ECmdArg.WatchMode:
                case ECmdArg.NormalMode:
                    {
                        Config config = Config.Load(GetConfigFilePath());
                        if (config == null)
                        {
                            DisplayUsage();
                            return -2;
                        }

                        if (!string.IsNullOrEmpty(config.tableDataRule.separatorList.Trim()))
                        {
                            ConstDef.SeparatorList = config.tableDataRule.separatorList.Trim()[0];
                        }
                        if (!string.IsNullOrEmpty(config.tableDataRule.separatorTuple.Trim()))
                        {
                            ConstDef.SeparatorTuple = config.tableDataRule.separatorTuple.Trim()[0];
                        }
                        ConstDef.NameRowIndex = config.tableDataRule.nameRowIndex - 1;
                        ConstDef.TypeRowIndex = config.tableDataRule.typeRowIndex - 1;
                        ConstDef.DescRowIndex = config.tableDataRule.descRowIndex - 1;
                        ConstDef.DataStartRowIndex = config.tableDataRule.dataStartRowIndex - 1;
                        ConstDef.CalculateFormula = config.tableDataRule.calculateFormula;
                        ConstDef.EmptyPlaceholder = config.tableDataRule.emptyPlaceholder;

                        PipeLine pipeline = PipelineBuilder.CreatePipeLine(config);

                        if (arg != ECmdArg.WatchMode)
                            return pipeline.Process(true);

                        FileWatcher watcher = FileWatcher.Create(config);
                        Watch(watcher, pipeline);
                    }
                    return 0;
            }
        }

        public static void Watch(FileWatcher watch, PipeLine pipeline)
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
        }

        public static ECmdArg ParseArgs(string[] args)
        {
            if (args.Length == 0)
                return ECmdArg.NormalMode;

            if (args.Length > 1)
                return ECmdArg.ShowUsage;

            switch (args[0].ToLower())
            {
                default: return ECmdArg.ShowUsage;
                case "-watch":
                case "watch":
                    return ECmdArg.WatchMode;
                case "-createconfig":
                case "createconfig":
                    return ECmdArg.CreateConfig;

                case "-formatconfig":
                case "formatconfig":
                    return ECmdArg.FormatConfig;
            }
        }


        public static void DisplayUsage()
        {
            string exeName = System.AppDomain.CurrentDomain.FriendlyName;

            Console.WriteLine(@$"
ReadMe https://github.com/fancyhub/ExportExcel

Usage:
    {exeName}                       simplest export excel
    {exeName} -watch                export exel with watching mode
    {exeName} -createconfig         create config.json
    {exeName} -formatconfig         format config.json
    {exeName} -help                 show this usage    
");
        }
    }
}


