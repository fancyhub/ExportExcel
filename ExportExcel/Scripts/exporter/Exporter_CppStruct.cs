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
    public class Exporter_CppStruct : IProcessNode
    {
        public const string C_FILE_NAME = "table_struct.h";
        public StringFormater _formater = new StringFormater();
        public ExeConfig.CppConfig _config;
        public EExportFlag _flag;

        public Exporter_CppStruct(EExportFlag flag, ExeConfig.CppConfig config)
        {
            _flag = flag;
            _config = config;
        }

        public string GetName()
        {
            return "Export CS";
        }

        public void Process(DataBase data)
        {
            if (_config == null || !_config.enable)
                return;

            _formater["namespace_start"] = "";
            _formater["namespace_end"] = "";

            if (!string.IsNullOrEmpty(_config.namespaceName))
            {
                _formater["namespace_start"] = "namespace " + _config.namespaceName + " {";
                _formater["namespace_end"] = "}";
            }

			_formater["user_header"] = _config.header;

            string name_space = _config.namespaceName;
            List<FilterTable> tables = FilterTable.Filter(data, _flag);
            string dest_file_path = System.IO.Path.Combine(_config.dir, C_FILE_NAME);
            FileUtil.CreateFileDir(dest_file_path);
            StreamWriter sw = new StreamWriter(dest_file_path);
          
            sw.WriteLineExt(_formater, @"
#pragma once
//自动生成的
#include <tuple>
#include <vector>
#include <string>
#include <unordered_map>

{user_header}

{namespace_start}
class Table
	{
	private:
		std::string _TableName;
		bool _MulitLang;
		std::string _LangName;

	public:
		Table() :_MulitLang(false)
		{
			_TableName = "";
			_LangName = "";
		}

		Table(const std::string& tableName, bool multiLang) :_MulitLang(multiLang)
		{
			_TableName = tableName;
			_LangName = "";
		}

		void SetLang(const std::string& langName)
		{
			if (_MulitLang)
				_LangName = langName;
		}

		virtual ~Table() {}
		const std::string& TableName()const { return _TableName; }
		const std::string& LangName()const { return _LangName; }
		bool IsMultiLang()const { return _MulitLang; }
	};

	template <class TItem>
	class TableList :public Table
	{
	protected:
		TItem* _Array;
		int _Count;

	public:
		std::vector<TItem*> List;

		TableList():Table(){ _Array = nullptr; _Count = 0; }
		TableList(const std::string& tableName, bool multiLang) :Table(tableName, multiLang) { _Array = nullptr; _Count = 0; }
		virtual ~TableList() { SetListData(nullptr, 0); }

		void SetListData(TItem* array, int count)
		{
			if (_Array != nullptr)
				delete[] _Array;
			_Array = array;
			_Count = count;

			List.clear();
			if (count > 0 && array != nullptr)
			{
				List.resize(count);
				for (int i = 0; i < count; i++)
				{
					List.push_back(array + 1);
				}
			}
		}
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

	template <class TKey, class TItem>
	class TableDict : public TableList<TItem>
	{
	public:
		std::unordered_map<TKey, TItem*, TupleHash> Dict;

		TableDict():TableList<TItem>(){}
		TableDict(const std::string& tableName, bool multiLang) :TableList<TItem>(tableName, multiLang) {}
		virtual ~TableDict() { Dict.clear(); }

		TItem* Get(TKey key)
		{
			auto it = Dict.find(key);
			return it != Dict.end() ? it->second : nullptr;
		}
	};

{namespace_end}

{namespace_start}
");

            //生成枚举结构
            foreach (var p in data.EnumDB)
            {
                sw.WriteLine(@"
enum class {enum_name}
{".Replace("{enum_name}", p.Key));

                List<EnumField> enum_field_list = p.Value.GetAllFields().ToList();
                enum_field_list.Sort((a, b) =>
                {
                    return a.Val - b.Val;
                });

                foreach (var p2 in enum_field_list)
                {
                    sw.WriteLine("\t//  " + p2.ExcelVal.Replace("\n", "\n\t\t/// "));
                    sw.WriteLine("\t{0} = {1},", p2.Name, p2.Val);
                }
                sw.WriteLine("};");
            }

            //生成类结构
            foreach (FilterTable table in tables)
            {
                _formater["sheet_name"] = table.SheetName;
				_formater["class_name"] = _config.GetClassName(table.SheetName);

                List<TableHeaderItem> header = table.Header;

                sw.WriteLineExt(_formater,
                    @"
struct {class_name}
    {");

                foreach (TableHeaderItem col in header)
                {
                    //写注释
                    sw.WriteLine("\t/// <summary>");
                    if (col.AttrPK != null)
                        sw.WriteLine("\t/// " + col.AttrPK.ToString());
                    sw.WriteLine("\t/// " + col.Desc.Replace("\n", "\n\t\t/// "));
                    sw.WriteLine("\t/// </summary>");
                    sw.WriteLine("\t " + col.DataType.ToCppStr() + " " + col.Name + ";");
                }
                sw.WriteLine(@"
    };");
            }
            if (!string.IsNullOrEmpty(name_space))
                sw.WriteLine("}");
            sw.Close();
        }
    }
}
