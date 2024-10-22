using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/7 16:50:44
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace ExportExcel
{
    //导出 csharp的数据结构
    public class Exporter_CppStructTable : IProcessNode
    {
        public const string C_FILE_NAME = "struct_table.h";
        public StringFormater _formater = new StringFormater();
        public Config.CppConfig _config;
        public EExportFlag _flag;

        public Exporter_CppStructTable(EExportFlag flag, Config.CppConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export C++ Struct Table";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);

            StreamWriter sw = null;
            {
                string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
                FileUtil.CreateFileDir(dest_file_path);
                sw = new StreamWriter(dest_file_path);
            }

            _formater["lang_count"] = data.LangList.Count.ToString();
            _formater["lang_list"] = string.Join("\",\"", data.LangList);
            _BuildBaseFormater();

            _ExportTableBase(sw);

            //生成类结构
            foreach (FilterTable table in tables)
            {
                _ExportTable(table, sw);
            }

            _ExportTableMgr(tables, sw);

            if (!string.IsNullOrEmpty(_config.namespaceName))
                sw.WriteLine("}");
            sw.Close();
        }

        private void _ExportTable(FilterTable table, StreamWriter sw)
        {
            _formater["sheet_name"] = table.SheetName;
            _formater["class_name"] = _config.GetClassName(table.SheetName);

            List<TableField> header = table.FiltedHeader;

            sw.WriteLineExt(_formater,
                @"
class Table{class_name} : public TableList<{class_name}>
{");


            sw.WriteLineExt(_formater, @"public:
	Table{class_name}() :TableList(""{sheet_name}"",  false) {}
");

            _BuildPKFormater(table);

            var pk = table.PK;
            if (pk == null)
            {

            }
            else if (!pk.AttrPK.IsCompose())
            {
                sw.WriteLineExt(_formater, @"private:
	std::unordered_map<{pk_type}, {class_name}*> _Map;

public:
	const std::unordered_map<{pk_type}, {class_name}*>  GetMap() const {return _Map;}
	const {class_name}* Find({pk_type} {pk_name})const
	{
		auto it = _Map.find({pk_name});
		return it != _Map.end() ? it->second : nullptr;
	}
	
protected:
		void BuildMap() override
		{
			_Map.clear();
			for (auto it = _List.begin(); it != _List.end(); ++it)
			{
				{class_name}* item = *it;				
				_Map[item->{pk_name}] = item;
			}
		}
");

            }
            else
            {
                sw.WriteLineExt(_formater, @"
private:
	std::unordered_map<std::tuple<{key_type_list}>, {class_name}*,TupleHash> _Map;

public:
	const std::unordered_map<std::tuple<{key_type_list}>, {class_name}*,TupleHash>  GetMap() const {return _Map;}
	const {class_name}* Find({key_list})const
	{
		auto it = _Map.find(std::tuple<{key_type_list}>({key_name_list}));
		return it != _Map.end() ? it->second : nullptr;
	}
	
protected:
		void BuildMap() override
		{
			_Map.clear();
			for (auto it = _List.begin(); it != _List.end(); ++it)
			{
				{class_name}* item = *it;				
				_Map[std::tuple<{key_type_list}>({key_name_list_with_item})]= item;
			}
		}
");
            }

            sw.WriteLine("};");
        }

        private void _BuildBaseFormater()
        {
            _formater["namespace_start"] = "";
            _formater["namespace_end"] = "";
            if (!string.IsNullOrEmpty(_config.namespaceName))
            {
                _formater["namespace_start"] = "namespace " + _config.namespaceName + " {";
                _formater["namespace_end"] = "}";
            }
            _formater["user_header"] = _config.header;
        }

        private void _BuildPKFormater(FilterTable table)
        {
            var pk = table.PK;
            if (pk == null)
                return;
            if (!pk.AttrPK.IsCompose())
            {
                _formater["pk_type"] = pk.ToCppStr();
                _formater["pk_name"] = pk.Name;
                return;
            }

            List<string> temp = new List<string>();
            temp.Add(pk.ToCppStr() + " " + pk.Name);
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add(p.ToCppStr() + " " + p.Name);
            }
            string key_list = string.Join(", ", temp);

            temp.Clear();
            temp.Add(pk.ToCppStr());
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add(p.ToCppStr());
            }
            string key_type_list = string.Join(", ", temp);


            temp.Clear();
            temp.Add(pk.Name);
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add(p.Name);
            }
            string key_name_list = string.Join(",", temp);

            temp.Clear();
            temp.Add("item->" + pk.Name);
            foreach (var p in pk.AttrPK.SubKeys)
            {
                temp.Add("item->" + p.Name);
            }
            string key_name_list_with_item = string.Join(",", temp);

            _formater["key_list"] = key_list;
            _formater["key_type_list"] = key_type_list;
            _formater["key_name_list"] = key_name_list;
            _formater["key_name_list_with_item"] = key_name_list_with_item;
        }

        private void _ExportTableBase(StreamWriter sw)
        {
            sw.WriteLineExt(_formater, @"
#pragma once
//自动生成的
#include <tuple>
#include <vector>
#include <string>
#include <unordered_map>
#include <typeinfo>

{user_header}

{namespace_start}
class Table
	{
	private:
		std::string _SheetName;
		bool _MulitLang;


	public:
		Table(const std::string& sheetName, bool multiLang) :_MulitLang(multiLang)
		{
			_SheetName = sheetName;
		} 

		virtual ~Table() {}
		const std::string& SheetName()const { return _SheetName; }
		bool IsMultiLang()const { return _MulitLang; }
	};

	template <class TItem>
	class TableList :public Table
	{
	protected:
		TItem* _Array;
		int _Count;
		std::vector<TItem*> _List;

	public:

		TableList( const std::string& sheetName, bool multiLang) :Table(sheetName, multiLang) { _Array = nullptr; _Count = 0; }
		virtual ~TableList() { SetListData(nullptr, 0); }
		const std::vector<TItem*>& GetList()const { return _List; }

		void SetListData(TItem* array, int count)
		{
			if (_Array != nullptr)
				delete[] _Array;
			_Array = array;
			_Count = count;

			_List.clear();
			if (count > 0 && array != nullptr)
			{
				if (_List.capacity() < count) {
					_List.reserve(count);
				}
				for (int i = 0; i < count; i++)
				{
					_List.push_back(array + i);
				}
			}
			this->BuildMap();
		}

	protected:
		virtual void BuildMap() {}
	};

	struct TupleHash
	{
	public:
		template<class T1, class T2>
		std::size_t operator()(const std::tuple<T1, T2>& p)const {
			std::size_t seed = 0;
			_HashCombine(seed, std::get<0>(p));
			_HashCombine(seed, std::get<1>(p));
			return seed;
		}

		template<class T1, class T2, class T3>
		std::size_t operator()(const std::tuple<T1, T2, T3>& p)const {
			std::size_t seed = 0;
			_HashCombine(seed, std::get<0>(p));
			_HashCombine(seed, std::get<1>(p));
			_HashCombine(seed, std::get<2>(p));
			return seed;
		}

		template<class T1, class T2, class T3, class T4>
		std::size_t operator()(const std::tuple<T1, T2, T3, T4>& p)const {
			std::size_t seed = 0;
			_HashCombine(seed, std::get<0>(p));
			_HashCombine(seed, std::get<1>(p));
			_HashCombine(seed, std::get<2>(p));
			_HashCombine(seed, std::get<3>(p));
			return seed;
		}

		template<class T1, class T2, class T3, class T4, class T5>
		std::size_t operator()(const std::tuple<T1, T2, T3, T4, T5>& p)const {
			std::size_t seed = 0;
			_HashCombine(seed, std::get<0>(p));
			_HashCombine(seed, std::get<1>(p));
			_HashCombine(seed, std::get<2>(p));
			_HashCombine(seed, std::get<3>(p));
			_HashCombine(seed, std::get<4>(p));
			return seed;
		}

		template<class T1>
		std::size_t operator()(const T1& p)const {
			return std::hash<T1>()(p);
		}

	private:
		template <class T> static  void _HashCombine(std::size_t& seed, const T& val)
		{
			seed ^= std::hash<T>()(val) + 0x9e3779b9 + (seed << 6) + (seed >> 2);
		}
	}; 
	struct TableTypeInfo
	{
	private:
		std::size_t _hash_code;
		const char* _name;

	public:
		TableTypeInfo() :_name(nullptr), _hash_code(0) {}
		TableTypeInfo(const TableTypeInfo& other) :_name(other._name), _hash_code(other._hash_code) {}
		TableTypeInfo(const std::type_info& info) { _hash_code = info.hash_code(); _name = info.name(); }
		const char* name()const { return _name; }
		template<class T>static TableTypeInfo Create() { return typeid(T); }
		std::size_t operator()(const TableTypeInfo& p)const { return p._hash_code; }
		bool operator()(const TableTypeInfo& _Left, const TableTypeInfo& _Right) const { return _Left._hash_code == _Right._hash_code && _Left._name == _Right._name; }
	};
{namespace_end}

{namespace_start}
");
        }


        private void _ExportTableMgr(List<FilterTable> tables, StreamWriter sw)
        {            
            sw.WriteLineExt(_formater, @"
struct TableMgr
{	
	const static int LangCount = {lang_count};
	const static std::string LangList[LangCount];
	std::unordered_map<TableTypeInfo, Table*, TableTypeInfo, TableTypeInfo> AllTableDict;

");
            foreach (var table in tables)
            {
                _formater["class_name"] = _config.GetClassName(table.SheetName);
                _formater["sheet_name"] = table.SheetName;

                sw.WriteLineExt(_formater, "\tTable{class_name} {sheet_name};");
            }
            sw.WriteLine("\n\tTableMgr():AllTableDict()");


            sw.WriteLine("\t{");
            foreach (var table in tables)
            {
                _formater["sheet_name"] = table.SheetName;
                _formater["class_name"] = _config.GetClassName(table.SheetName);
                sw.WriteLineExt(_formater, @"       AllTableDict[typeid(Table{class_name})] = &{sheet_name};");
            }
            sw.WriteLine("\t}");

            sw.WriteLine("};");

			sw.WriteLine("inline const std::string TableMgr::LangList[] = {\"{lang_list}\"};\t");
        }
    }
}
