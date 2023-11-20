
#pragma once

#include "table_struct.h"
#include <functional>
namespace Test {
	class ITableDataReader
	{
	public:
		virtual bool Read(bool& v) = 0;

		virtual bool Read(int& v) = 0;
		virtual bool Read(unsigned int& v) = 0;

		virtual bool Read(long long& v) = 0;
		virtual bool Read(unsigned long long& v) = 0;

		virtual bool Read(float& v) = 0;
		virtual bool Read(double& v) = 0;

		virtual bool Read(LocId& v) = 0;
		virtual bool Read(LocStr& v) = 0;

		virtual bool Read(std::string& v) = 0;
	};

	class ITableTupleReader :public  ITableDataReader
	{

	};

	class ITableListReader : public ITableDataReader
	{
	public:
		virtual int GetCount() = 0;
		virtual ITableTupleReader* NextTuple() = 0;
	};	

	class ITableRowReader : public  ITableDataReader
	{
	public:
		virtual ITableListReader* BeginList() = 0;
		virtual ITableTupleReader* BeginTuple() = 0;
	};

	class ITableReader
	{
	public:
		virtual const std::vector<std::string>& ReadHeader() = 0;
		virtual int ReadRowCount() = 0;

		virtual ITableRowReader* NextRow() = 0;
	};

	class ITableReaderCreator
	{
	public:
		virtual ITableReader* ReadTable(const std::string& sheet_name, const std::string& lang_name) = 0;
	};
	using TableLoader = std::function <bool(Table* table, std::string lang_name, ITableReaderCreator& tableReaderCreator)>;

	struct TableInfo
	{
		TableLoader Loader;
		bool MultiLang;
		TableInfo() { Loader = nullptr; MultiLang = false; }
		TableInfo(TableLoader loader, bool multiLang){Loader = loader;MultiLang = multiLang;}
	};

	class TableLoaderMgr
	{
	public:
		const static int LangCount = 2;
		const static std::string LangList[LangCount];
		std::unordered_map < std::string, TableInfo> LoaderDict;

	public:
		TableLoaderMgr();	

		bool FindLoader(const std::string& table_name, TableLoader& loader)
		{
			auto it = LoaderDict.find(table_name);
			if (it == LoaderDict.end())
				return false;

			loader = it->second.Loader;
			return true;
		}
	};
}

