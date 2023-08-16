using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Log
    {
        public static void E(string msg, params object[] args)
        {
            Console.Error.WriteLine(msg, args);
        }

        public static void Assert(bool condition, string msg, params object[] args)
        {
            if(!condition)
                Console.Error.WriteLine(msg, args);
        }

        public static void E(Exception e)
        {

        }
    }
}
