using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 16:50:44
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{    
    public class Exporter_CppLoader : IProcessNode
    {
        public const string C_HEADER_FILE_NAME = "table_loader.h";
        public const string C_CPP_FILE_NAME = "table_loader.cpp";

        public StringFormater _formater = new StringFormater();

        public Config.CppConfig _config;
        public EExportFlag _flag;

        public Exporter_CppLoader(EExportFlag flag, Config.CppConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export";
        }

        public void Process(DataBase data_base)
        {
            if (_config == null || !_config.enable || !_config.loader.enable)
                return;

            _formater["lang_count"] = data_base.LangList.Count.ToString();
            _formater["namespace_start"] = "";
            _formater["namespace_end"] = "";

            if (!string.IsNullOrEmpty(_config.namespaceName))
            {
                _formater["namespace_start"] = "namespace " + _config.namespaceName + " {";
                _formater["namespace_end"] = "}";
            }

            _ExportHeader(data_base);
            _ExportCpp(data_base);

        }

        public void _ExportHeader(DataBase data_base)
        {
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_HEADER_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
            _formater["user_header"] = _config.header;

            sw.WriteLineExt(_formater, @"
#pragma once

#include ""table_struct.h""
#include <functional>
{namespace_start}
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

	struct TableLoaderInfo
	{
		TableLoader Loader;
		bool MultiLang;
		TableLoaderInfo() { Loader = nullptr; MultiLang = false; }
		TableLoaderInfo(TableLoader loader, bool multiLang){Loader = loader;MultiLang = multiLang;}
	};

	class TableLoaderMgr
	{
	public:
		const static int LangCount = {lang_count};
		const static std::string LangList[LangCount];
		std::unordered_map <TableTypeInfo, TableLoaderInfo, TableTypeInfo, TableTypeInfo> LoaderDict;

	public:
		TableLoaderMgr();	

		template<class TItem> bool FindLoader(TableLoader& loader)
		{
			return FindLoader(typeid(TItem), loader);
		}

		bool FindLoader(const TableTypeInfo& info, TableLoader& loader)
		{
			auto it = LoaderDict.find(info);
			if (it == LoaderDict.end())
				return false;

			loader = it->second.Loader;
			return true;
		}
	};
{namespace_end}
");

            sw.Close();
        }

        public void _ExportCpp(DataBase data_base)
        {
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_CPP_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);

            _formater["lang_list"]= string.Join("\",\"", data_base.LangList);

            sw.WriteLineExt(_formater, @"
#include <iostream>
#include ""table_loader.h""

{namespace_start}
	const std::string TableLoaderMgr::LangList[] = {""{lang_list}""};	

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
");

            List<FilterTable> tables = FilterTable.Filter(data_base, _flag);
            foreach (FilterTable table in tables)
            {
                _ExportLoaderFunc(table, sw);
            }

            {
                sw.WriteLine(@"
TableLoaderMgr::TableLoaderMgr()
{");
                foreach (FilterTable table in tables)
                {
                    var className = _config.GetClassName(table.SheetName);
                    var multi_lang = table.MultiLang.ToString().ToLower();

                    sw.WriteLine($"\tLoaderDict[typeid({className})] = TableLoaderInfo(_Load{table.SheetName}, {multi_lang});");
                }
                sw.WriteLine("}");
            }
            

            sw.WriteLine(_formater["namespace_end"]);
            sw.Close();
        }
         
         
        public void _ExportLoaderFunc(FilterTable table, StreamWriter sw)
        {
            List<TableHeaderItem> header_list = table.Header;
            string multi_name = "";
            if (!table.MultiLang)
                multi_name = "lang = \"\";";
            _formater["sheet_name_lang"] = multi_name;
            _formater["col_count"] = header_list.Count.ToString();
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            sw.WriteLineExt(_formater, @"
static Table* _Load{sheet_name}(Table* table, std::string lang, ITableReaderCreator& tableReaderCreator)
{
    auto tableList = dynamic_cast<TableList<{class_name}>*>(table);

    std::string sheet_name = ""{sheet_name}"";
    {sheet_name_lang}
    int col_count = {col_count};

    ITableReader* reader = tableReaderCreator.ReadTable(sheet_name,lang);
    if(reader == nullptr)
        return nullptr;

    //Check Header
    const std::vector<std::string>& header = reader->ReadHeader();
    if (header.size() != (col_count*2))
    {
		std::cerr << ""加载错误,表头数量不对"" << sheet_name << std::endl;
        return nullptr;
    }
    bool head_rst = true;
    ");


            for (int i = 0; i < header_list.Count; i++)
            {
                sw.WriteLine("\thead_rst &= (header[{0}].compare(\"{1}\")==0) && (header[{0}+{2}].compare(\"{3}\")==0);", i, header_list[i].Name, header_list.Count, header_list[i].DataType.ToCsvStr());
            }

            sw.WriteLineExt(_formater,
                @"
    if (!head_rst)
    {
		std::cerr << ""加载错误,表头不匹配"" << sheet_name << std::endl;
        return nullptr;
    }

    int rowCount = reader->ReadRowCount();
    {class_name}* array = new {class_name}[rowCount];
    //加载数据
    for (int i = 0; i < rowCount; i++)
    { 
		auto rowReader = reader->NextRow();
        {class_name} item;");

            for (int i = 0; i < header_list.Count; i++)
            {
                var header = header_list[i];
                var data_type = header.DataType;
                if (data_type.IsList)
                {
                    data_type.IsList = false;
                    if (header.DataType.IsTuple)
                        sw.WriteLine($"\t\t_ReadListTuple(rowReader, item.{header.Name});");
                    else
                        sw.WriteLine($"\t\t_ReadList(rowReader, item.{header.Name});");
                }
                else if (data_type.IsTuple)
                {
                    sw.WriteLine($"\t\t_ReadTuple(rowReader, item.{header.Name});");                    
                }
                else
                {
                    if(data_type.enum_type!=null)
                        sw.WriteLine($"\t\t_ReadEnum(rowReader,item.{header.Name});");
                    else
                        sw.WriteLine($"\t\trowReader->Read(item.{header.Name});");
                }
            }

            sw.WriteLineExt(_formater, @"
        array[i] = item;
    }
    tableList->SetListData(array, rowCount);  
");

            TableHeaderItem pk = table.PK;
			if (pk != null)
			{
				_formater["pk_name"] = pk.Name;
				_formater["pk_type"] = pk.DataType.ToCppStr();
				if (!pk.AttrPK.IsCompose())
				{
					sw.WriteLineExt(_formater, @"
		auto tableDict= dynamic_cast<TableDict<{pk_type},{class_name}>*>(table);
		tableDict->Dict.clear();
		for (int i = 0; i < rowCount; i++)
		{
			{class_name}* item = array + i;
			{pk_type} key = item->{pk_name};			 
			tableDict->Dict[key] = item;
			
		}");
				}
				else
				{
                    _formater["pk_sec_name"] = pk.AttrPK._sec_key.Name;
                    _formater["pk_sec_type"] = pk.AttrPK._sec_key.DataType.ToCppStr();
                    sw.WriteLineExt(_formater, @"
		auto tableDict= dynamic_cast<TableDict<std::tuple<{pk_type},{pk_sec_type}>,{class_name}>*>(table);
		tableDict->Dict.clear();
		for (int i = 0; i < rowCount; i++)
		{
			{class_name}* item = array + i;
			std::tuple<{pk_type},{pk_sec_type}> key(item->{pk_name},item->{pk_sec_name});
			tableDict->Dict[key] = item;
			
		}");

                }
			}
             

            sw.WriteLine("\treturn tableList;");
            sw.WriteLine("}");
        }
    }
}
