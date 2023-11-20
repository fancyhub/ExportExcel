//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
namespace Test{
    
    public partial class TableMgr
    {   

      
        public  List<TItemData> GetTItemDataList()
        {
            return FindTable<TItemData>()?.GetList<TItemData>();
        }
        

        public  TItemData GetTItemData(int Id)
        {
            return FindTable<TItemData>()?.Get<int,TItemData>(Id);
        }

        public  Dictionary<int, TItemData> GetTItemDataDict()
        {
            return FindTable<TItemData>()?.GetDict<int, TItemData>();
        }
        
      
        public  List<TTestComposeKey> GetTTestComposeKeyList()
        {
            return FindTable<TTestComposeKey>()?.GetList<TTestComposeKey>();
        }
        

        public  TTestComposeKey GetTTestComposeKey(uint Id,int Level)
        {        
            return FindTable<TTestComposeKey>()?.Get<(uint,int), TTestComposeKey>((Id,Level));
        }

        public  Dictionary<(uint,int), TTestComposeKey> GetTTestComposeKeyDict()
        {
            return FindTable<TTestComposeKey>()?.GetDict<(uint,int), TTestComposeKey>();            
        }
        
}
}
