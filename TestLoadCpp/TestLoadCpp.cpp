// TestLoadCpp.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include "gen/table_struct.h"
#include "gen/table_getter.h"
#include "gen/table_loader.h"
#include "reader/csv_reader.h"
using namespace Test;


class TableReaderCreator : public ITableReaderCreator
{
	TableReaderCsv _reader;
public :
	ITableReader* ReadTable(const std::string& sheet_name, const std::string& lang_name)
	{
		char buff[1024];
		if (lang_name.empty())
			sprintf_s(buff, 1024, "../TestData/2_loc_auto_key/Output/Client/Data/%s.csv", sheet_name.c_str());
		else
			sprintf_s(buff, 1024, "../TestData/2_loc_auto_key/Output/Client/Data/%s_%s.csv", sheet_name.c_str(),lang_name.c_str());
		_reader.LoadCsvFile(buff);
		return &_reader;
	}
};
int main()
{
	TableMgr mgr;
	TableLoaderMgr loaderMgr;
	std::string lang_name = TableLoaderMgr::LangList[0];

	TableReaderCreator readerCreator;

	TableLoader loader;
	for (auto it = mgr.AllTableDict.begin(); it != mgr.AllTableDict.end(); it++)
	{
		const std::string& name = it->first;
		if (!loaderMgr.FindLoader(name, loader))
			continue;

		loader(it->second, lang_name, readerCreator);
	}
	 
	std::cout << "Hello World!\n";
}




// 运行程序: Ctrl + F5 或调试 >“开始执行(不调试)”菜单
// 调试程序: F5 或调试 >“开始调试”菜单

// 入门使用技巧: 
//   1. 使用解决方案资源管理器窗口添加/管理文件
//   2. 使用团队资源管理器窗口连接到源代码管理
//   3. 使用输出窗口查看生成输出和其他消息
//   4. 使用错误列表窗口查看错误
//   5. 转到“项目”>“添加新项”以创建新的代码文件，或转到“项目”>“添加现有项”以将现有代码文件添加到项目
//   6. 将来，若要再次打开此项目，请转到“文件”>“打开”>“项目”并选择 .sln 文件
