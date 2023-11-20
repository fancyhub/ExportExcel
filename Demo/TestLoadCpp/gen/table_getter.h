
#pragma once
//自动生成的
#include <unordered_map>
#include <tuple>
#include <string>
#include "table_struct.h"

namespace Test {
	struct TableMgr
	{
		std::unordered_map<std::string, Table*> AllTableDict;


		TableDict<int, TItemData> TableTItemData;
		TableDict<std::tuple<unsigned int, int>, TTestComposeKey> TableTTestComposeKey;
		TableDict<int, TLoc> TableTLoc;

		TableMgr() :AllTableDict()
			, TableTItemData("TItemData", false)
			, TableTTestComposeKey("TTestComposeKey", false)
			, TableTLoc("TLoc", true)
		{
			AllTableDict["TItemData"] = &TableTItemData;
			AllTableDict["TTestComposeKey"] = &TableTTestComposeKey;
			AllTableDict["TLoc"] = &TableTLoc;
		}
	};
}
