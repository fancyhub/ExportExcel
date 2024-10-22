
#include <iostream>
#include "table_loader_csv.h"

namespace Test {	

	template <class T1, class T2>
	static bool _ReadTuple(ITableRowReader* reader, std::tuple<T1, T2>& item)
	{
		auto tupleReader= reader->BeginTuple();
		if (tupleReader == nullptr)
			return true;
		tupleReader->Read(std::get<0>(item));
		tupleReader->Read(std::get<1>(item));
		return true;
	}


	template <class T1, class T2>
	static bool _ReadListTuple(ITableRowReader* reader, std::vector<std::tuple<T1, T2>>& list)
	{
		auto listReader = reader->BeginList();
		if (listReader == nullptr)
			return true;
		int count = listReader->GetCount();
		list.resize(count);
		list.clear();
		for (int i = 0; i < count; i++)
		{
			std::tuple<T1, T2> item;
			auto tupleReader = listReader->NextTuple();
			tupleReader->Read(std::get<0>(item));
			tupleReader->Read(std::get<1>(item));
			list.push_back(item);
		}
		return true;
	}

	template <class T1, class T2, class T3>
	static bool _ReadTuple(ITableRowReader* reader, std::tuple<T1, T2, T3>& item)
	{
		auto tupleReader = reader->BeginTuple();
		if (tupleReader == nullptr)
			return true;
		tupleReader->Read(std::get<0>(item));
		tupleReader->Read(std::get<1>(item));
		tupleReader->Read(std::get<2>(item));
		return true;
	}

	template <class T1, class T2, class T3>
	static bool _ReadListTuple(ITableRowReader* reader, std::vector<std::tuple<T1, T2, T3>>& list)
	{
		auto listReader = reader->BeginList();
		if (listReader == nullptr)
			return true;
		int count = listReader->GetCount();
		list.resize(count);
		list.clear();
		for (int i = 0; i < count; i++)
		{
			std::tuple<T1, T2, T3> item;
			auto tupleReader = listReader->NextTuple();
			tupleReader->Read(std::get<0>(item));
			tupleReader->Read(std::get<1>(item));
			tupleReader->Read(std::get<2>(item));
			list.push_back(item);
		}
		return true;
	}


	template <class T1, class T2, class T3, class T4>
	static bool _ReadTuple(ITableRowReader* reader, std::tuple<T1, T2, T3, T4>& item)
	{
		auto tupleReader = reader->BeginTuple();
		if (tupleReader == nullptr)
			return true;
		tupleReader->Read(std::get<0>(item));
		tupleReader->Read(std::get<1>(item));
		tupleReader->Read(std::get<2>(item));
		tupleReader->Read(std::get<3>(item));
		return true;
	}

	template <class T1, class T2, class T3, class T4>
	static bool _ReadListTuple(ITableRowReader* reader, std::vector<std::tuple<T1, T2, T3, T4>>& list)
	{
		auto listReader = reader->BeginList();
		if (listReader == nullptr)
			return true;
		int count = listReader->GetCount();
		list.resize(count);
		list.clear();
		for (int i = 0; i < count; i++)
		{
			std::tuple<T1, T2, T3, T4> item;
			auto tupleReader = listReader->NextTuple();
			tupleReader->Read(std::get<0>(item));
			tupleReader->Read(std::get<1>(item));
			tupleReader->Read(std::get<2>(item));
			tupleReader->Read(std::get<3>(item));
			list.push_back(item);
		}
		return true;
	}

	template <class T1, class T2, class T3, class T4, class T5>
	static bool _ReadTuple(ITableRowReader* reader, std::tuple<T1, T2, T3, T4, T5>& item)
	{
		auto tupleReader = reader->BeginTuple();
		if (tupleReader == nullptr)
			return true;
		tupleReader->Read(std::get<0>(item));
		tupleReader->Read(std::get<1>(item));
		tupleReader->Read(std::get<2>(item));
		tupleReader->Read(std::get<3>(item));
		tupleReader->Read(std::get<4>(item));
		return true;
	}

	template <class T1, class T2, class T3, class T4, class T5>
	static bool _ReadListTuple(ITableRowReader* reader, std::vector<std::tuple<T1, T2, T3, T4, T5>>& list)
	{
		auto listReader = reader->BeginList();
		if (listReader == nullptr)
			return true;
		int count = listReader->GetCount();
		list.resize(count);
		list.clear();
		for (int i = 0; i < count; i++)
		{
			std::tuple<T1, T2, T3, T4, T5> item;
			auto tupleReader = listReader->NextTuple();
			tupleReader->Read(std::get<0>(item));
			tupleReader->Read(std::get<1>(item));
			tupleReader->Read(std::get<2>(item));
			tupleReader->Read(std::get<3>(item));
			tupleReader->Read(std::get<4>(item));
			tupleReader.push_back(item);
		}
		return true;
	}


	template <class T>
	static bool _ReadEnum(ITableRowReader* reader, T& v)
	{
		int vv;
		if (!reader->Read(vv))
			return false;
		v = (T)vv;
		return true;
	}

	template <class T>
	static bool _ReadList(ITableRowReader* reader, std::vector<T>& list)
	{
		auto listReader = reader->BeginList();
		if (listReader == nullptr)
			return true;
		int count = listReader->GetCount();
		list.resize(count);
		list.clear();
		for (int i = 0; i < count; i++)
		{
			T item;
			listReader->Read(item);
			list.push_back(item);
		}
		return true;
	}


static Table* _LoadItemData(Table* table, std::string lang, ITableReaderCreator& tableReaderCreator)
{
    auto tableList = dynamic_cast<TableList<ItemData>*>(table);

    const std::string& sheet_name = table->SheetName();
    lang = "";
    int col_count = 11;

    ITableReader* reader = tableReaderCreator.ReadTable(sheet_name,lang);
    if(reader == nullptr)
        return nullptr;

    //Check Header
    const std::vector<std::string>& header = reader->ReadHeader();
    if (header.size() != (col_count*2))
    {
		std::cerr << "加载错误,表头数量不对" << sheet_name << std::endl;
        return nullptr;
    }
    bool head_rst = true;
    
	head_rst &= (header[0].compare("Id")==0) && (header[0+11].compare("int32")==0);
	head_rst &= (header[1].compare("Name")==0) && (header[1+11].compare("locid")==0);
	head_rst &= (header[2].compare("Type")==0) && (header[2+11].compare("int32")==0);
	head_rst &= (header[3].compare("SubType")==0) && (header[3+11].compare("int32")==0);
	head_rst &= (header[4].compare("Quality")==0) && (header[4+11].compare("int32")==0);
	head_rst &= (header[5].compare("PairField")==0) && (header[5+11].compare("int32_bool")==0);
	head_rst &= (header[6].compare("PairField2")==0) && (header[6+11].compare("int32_bool")==0);
	head_rst &= (header[7].compare("PairField3")==0) && (header[7+11].compare("int32_int32")==0);
	head_rst &= (header[8].compare("PairFieldList")==0) && (header[8+11].compare("list_int32_int64")==0);
	head_rst &= (header[9].compare("PairFieldList2")==0) && (header[9+11].compare("list_int32_int64")==0);
	head_rst &= (header[10].compare("ListField")==0) && (header[10+11].compare("list_int32")==0);

    if (!head_rst)
    {
		std::cerr << "加载错误,表头不匹配" << sheet_name << std::endl;
        return nullptr;
    }

    int rowCount = reader->ReadRowCount();
    ItemData* array = new ItemData[rowCount];
    //加载数据
    for (int i = 0; i < rowCount; i++)
    { 
		auto rowReader = reader->NextRow();
        ItemData item;
		rowReader->Read(item.Id);
		rowReader->Read(item.Name);
		_ReadEnum(rowReader,item.Type);
		_ReadEnum(rowReader,item.SubType);
		_ReadEnum(rowReader,item.Quality);
		_ReadTuple(rowReader, item.PairField);
		_ReadTuple(rowReader, item.PairField2);
		_ReadTuple(rowReader, item.PairField3);
		_ReadListTuple(rowReader, item.PairFieldList);
		_ReadListTuple(rowReader, item.PairFieldList2);
		_ReadList(rowReader, item.ListField);

        array[i] = item;
    }
    tableList->SetListData(array, rowCount);  

	return tableList;
}

static Table* _LoadTestComposeKey(Table* table, std::string lang, ITableReaderCreator& tableReaderCreator)
{
    auto tableList = dynamic_cast<TableList<TestComposeKey>*>(table);

    const std::string& sheet_name = table->SheetName();
    lang = "";
    int col_count = 6;

    ITableReader* reader = tableReaderCreator.ReadTable(sheet_name,lang);
    if(reader == nullptr)
        return nullptr;

    //Check Header
    const std::vector<std::string>& header = reader->ReadHeader();
    if (header.size() != (col_count*2))
    {
		std::cerr << "加载错误,表头数量不对" << sheet_name << std::endl;
        return nullptr;
    }
    bool head_rst = true;
    
	head_rst &= (header[0].compare("Id")==0) && (header[0+6].compare("uint32")==0);
	head_rst &= (header[1].compare("Level")==0) && (header[1+6].compare("int32")==0);
	head_rst &= (header[2].compare("Gender")==0) && (header[2+6].compare("int32")==0);
	head_rst &= (header[3].compare("Name")==0) && (header[3+6].compare("locid")==0);
	head_rst &= (header[4].compare("Pos")==0) && (header[4+6].compare("float32_float32_float32")==0);
	head_rst &= (header[5].compare("Flags")==0) && (header[5+6].compare("int32")==0);

    if (!head_rst)
    {
		std::cerr << "加载错误,表头不匹配" << sheet_name << std::endl;
        return nullptr;
    }

    int rowCount = reader->ReadRowCount();
    TestComposeKey* array = new TestComposeKey[rowCount];
    //加载数据
    for (int i = 0; i < rowCount; i++)
    { 
		auto rowReader = reader->NextRow();
        TestComposeKey item;
		rowReader->Read(item.Id);
		rowReader->Read(item.Level);
		rowReader->Read(item.Gender);
		rowReader->Read(item.Name);
		_ReadTuple(rowReader, item.Pos);
		_ReadEnum(rowReader,item.Flags);

        array[i] = item;
    }
    tableList->SetListData(array, rowCount);  

	return tableList;
}

static Table* _LoadLoc(Table* table, std::string lang, ITableReaderCreator& tableReaderCreator)
{
    auto tableList = dynamic_cast<TableList<Loc>*>(table);

    const std::string& sheet_name = table->SheetName();
    
    int col_count = 2;

    ITableReader* reader = tableReaderCreator.ReadTable(sheet_name,lang);
    if(reader == nullptr)
        return nullptr;

    //Check Header
    const std::vector<std::string>& header = reader->ReadHeader();
    if (header.size() != (col_count*2))
    {
		std::cerr << "加载错误,表头数量不对" << sheet_name << std::endl;
        return nullptr;
    }
    bool head_rst = true;
    
	head_rst &= (header[0].compare("Id")==0) && (header[0+2].compare("int32")==0);
	head_rst &= (header[1].compare("Val")==0) && (header[1+2].compare("string")==0);

    if (!head_rst)
    {
		std::cerr << "加载错误,表头不匹配" << sheet_name << std::endl;
        return nullptr;
    }

    int rowCount = reader->ReadRowCount();
    Loc* array = new Loc[rowCount];
    //加载数据
    for (int i = 0; i < rowCount; i++)
    { 
		auto rowReader = reader->NextRow();
        Loc item;
		rowReader->Read(item.Id);
		rowReader->Read(item.Val);

        array[i] = item;
    }
    tableList->SetListData(array, rowCount);  

	return tableList;
}

TableLoaderMgrCsv::TableLoaderMgrCsv()
{
	LoaderDict[typeid(TableItemData)] = TableLoaderInfo(_LoadItemData, false);
	LoaderDict[typeid(TableTestComposeKey)] = TableLoaderInfo(_LoadTestComposeKey, false);
	LoaderDict[typeid(TableLoc)] = TableLoaderInfo(_LoadLoc, true);
}
}
