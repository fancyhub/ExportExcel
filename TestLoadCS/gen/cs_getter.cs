//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
namespace Test{
    
    public partial class TableMgr
    {   

      
        public  List<TItemData> GetTItemDataList()
        {
            return GetList<TItemData>();
        }
        

        public  TItemData GetTItemData(int Id)
        {
            return Get<int,TItemData>(Id);
        }

        public  Dictionary<int, TItemData> GetTItemDataDict()
        {
            return GetDict<int, TItemData>();
        }
        
      
        public  List<TTestComposeKey> GetTTestComposeKeyList()
        {
            return GetList<TTestComposeKey>();
        }
        

        public  TTestComposeKey GetTTestComposeKey(uint Id,int Level)
        {        
            return Get<TTestComposeKey>((uint)Id,(uint)Level);
        }

        public  Dictionary<ulong, TTestComposeKey> GetTTestComposeKeyDict()
        {
            return GetDict<ulong, TTestComposeKey>();
        }
        
      
        public  List<TLoc> GetTLocList()
        {
            return GetList<TLoc>();
        }
        

        public  TLoc GetTLoc(int Id)
        {
            return Get<int,TLoc>(Id);
        }

        public  Dictionary<int, TLoc> GetTLocDict()
        {
            return GetDict<int, TLoc>();
        }
        
}
}
