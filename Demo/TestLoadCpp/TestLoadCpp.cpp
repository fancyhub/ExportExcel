// TestLoadCpp.cpp : 此文件包含 "main" 函数。程序执行将在此处开始并结束。
//

#include <iostream>
#include <filesystem>


#include "gen/struct_item.h"
#include "gen/struct_table.h"
#include "gen/table_loader_csv.h"
#include "reader/csv_reader.h"
using namespace Test;


std::filesystem::path dir0("../Data/0_no_loc/Output/Client/Data");
std::filesystem::path dir1("../Data/1_loc/Output/Client/Data");
std::filesystem::path dir2("../Data/2_loc_auto_key/Output/Client/Data");


class TableReaderCreator : public ITableReaderCreator
{
private:
	TableReaderCsv _reader;
	std::filesystem::path _base_dir;

public:
	TableReaderCreator(const std::filesystem::path& base_dir)
	{
		_base_dir = base_dir;
	}
public:
	ITableReader* ReadTable(const std::string& sheet_name, const std::string& lang_name)
	{
		auto file_path = _base_dir;				
		if (lang_name.empty())
			file_path /= sheet_name + ".csv";
		else
			file_path /= sheet_name +"_"+ lang_name + ".csv";
		_reader.LoadCsvFile(file_path.string());
		return &_reader;
	}
};


int main()
{
	TableMgr mgr;
	TableLoaderMgrCsv loaderMgr;	
	TableReaderCreator readerCreator(std::filesystem::absolute(dir2));

	std::string lang_name = TableMgr::LangList[0];

	TableLoader loader;
	for (auto it = mgr.AllTableDict.begin(); it != mgr.AllTableDict.end(); it++)
	{
		if (!loaderMgr.FindLoader(it->first, loader))
			continue;
		loader(it->second, lang_name, readerCreator);
	}	
	auto item1 = mgr.TestComposeKey.Find(1, 1, 1);

	std::cout << "Hello World!\n";
}
