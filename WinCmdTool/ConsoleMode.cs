using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/9/17 9:50:45
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace WinCmdTool
{

    //https://docs.microsoft.com/en-us/windows/console/getconsolemode
    public enum E_CONSOLE_MODE : uint
    {
        ENABLE_ECHO_INPUT = 0x0004,

        //插入模式
        ENABLE_INSERT_MODE = 0x0020,

        ENABLE_LINE_INPUT = 0x0002,

        ENABLE_MOUSE_INPUT = 0x0010,

        ENABLE_PROCESSED_INPUT = 0x0001,

        ENABLE_QUICK_EDIT_MODE = 0x0040,

        ENABLE_WINDOW_INPUT = 0x0008,

        ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200,
    }

    public static class ConsoleMode
    {

        // STD_INPUT_HANDLE (DWORD): -10 is the standard input device.
        private const int STD_INPUT_HANDLE = -10;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int dwProcessId);
        public const int ATTACH_PARENT_PROCESS = -1;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        public static void SetActive(E_CONSOLE_MODE mode, bool enable)
        {
            IntPtr handle = GetStdHandle(STD_INPUT_HANDLE);

            uint console_mode;
            if (!GetConsoleMode(handle, out console_mode))
            {
                // ERROR: Unable to get console mode.
                return;
            }

            uint new_mode = console_mode;

            if (enable)
            {
                new_mode |= ((uint)mode);
            }
            else
            {
                new_mode &= ~((uint)mode);
            }

            if (new_mode == console_mode)
                return;


            // set the new mode
            if (!SetConsoleMode(handle, new_mode))
            {
                // ERROR: Unable to set console mode
                return;
            }
        }
    }
}
