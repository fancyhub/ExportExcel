
#pragma once
//自动生成的
#include <unordered_map>
#include <tuple>
#include <string>
#include "table_struct.h"

namespace Test {
struct TableMgr
{
	std::unordered_map<TableTypeInfo, Table*, TableTypeInfo, TableTypeInfo> AllTableDict;


	TableDict<int,TItemData> TableTItemData;
	TableDict<std::tuple<unsigned int,int>,TTestComposeKey> TableTTestComposeKey;
	TableDict<int,TLoc> TableTLoc;

	TableMgr():AllTableDict()
       ,TableTItemData("TItemData",false)
       ,TableTTestComposeKey("TTestComposeKey",false)
       ,TableTLoc("TLoc",true)
	{
       AllTableDict[typeid(TItemData)] = &TableTItemData;
       AllTableDict[typeid(TTestComposeKey)] = &TableTTestComposeKey;
       AllTableDict[typeid(TLoc)] = &TableTLoc;
	}

    const TItemData* GetTItemData(int Id)const
    {
        return TableTItemData.Get(Id);
    }


    const TTestComposeKey* GetTTestComposeKey(unsigned int Id,int Level)const
    {
        return TableTTestComposeKey.Get(std::tuple<unsigned int,int>(Id,Level));
    }


    const TLoc* GetTLoc(int Id)const
    {
        return TableTLoc.Get(Id);
    }

};
}
