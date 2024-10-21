//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
namespace Test{

   public partial class Table
    {
        public abstract bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader);
    }


    public sealed partial class TableTItemData
    {        
        public override bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader)
        {
            var items=  jsonSerializer.Deserialize<List<TItemData>>(reader);
            if (items == null)
                return false;
            List = items;
            return true;
        }
    }

    public sealed partial class TableTTestComposeKey
    {        
        public override bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader)
        {
            var items=  jsonSerializer.Deserialize<List<TTestComposeKey>>(reader);
            if (items == null)
                return false;
            List = items;
            return true;
        }
    }

    public sealed partial class TableTLoc
    {        
        public override bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader)
        {
            var items=  jsonSerializer.Deserialize<List<TLoc>>(reader);
            if (items == null)
                return false;
            List = items;
            return true;
        }
    }

    public sealed partial class TableTTC
    {        
        public override bool LoadFromJson(Newtonsoft.Json.JsonSerializer jsonSerializer, Newtonsoft.Json.JsonReader reader)
        {
            var items=  jsonSerializer.Deserialize<List<TTC>>(reader);
            if (items == null)
                return false;
            List = items;
            return true;
        }
    }
}
