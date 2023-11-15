using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class PairItemIntBool
    {
        public int Key;
        public bool Value;

        public static PairItemIntBool CreateInst(bool hasV, (int, bool) v )
        {
            if (hasV)
                return null;
            return new PairItemIntBool()
            {
                Key = v.Item1,
                Value = v.Item2
            };
        }

        public static PairItemIntBool CreateInst(bool hasV, (int, int) v)
        {
            if (hasV)
                return null;
            return new PairItemIntBool()
            {
                Key = v.Item1,                
            };
        }
    }

    public class PairItemIntInt64
    {
        public static PairItemIntInt64 CreateInst(bool hasValue, (int, long)  v)
        {
            return null;
        }
    }
}
