using System;
using System.Collections.Generic;

namespace WinCmdTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleMode.SetActive(E_CONSOLE_MODE.ENABLE_QUICK_EDIT_MODE, false);

            if (args.Length == 0)
                return;

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WorkingDirectory = System.Environment.CurrentDirectory;
            p.StartInfo.FileName = args[0];


            var arguments = "";
            for (int i = 1; i < args.Length; i++)
            {
                arguments = args[i] + " ";
            }
            p.StartInfo.Arguments = arguments;
            p.Start();//启动


            p.WaitForExit();
        }
    }
}
