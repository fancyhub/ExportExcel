
#pragma once
//自动生成的
#include <tuple>
#include <vector>
#include <string>
#include <unordered_map>
#include <typeinfo>

#include "../dep/loc_str.h"

namespace Test {
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
}

namespace Test {


class TableItemData : public TableList<ItemData>
{
public:
	TableItemData() :TableList("ItemData",  false) {}

private:
	std::unordered_map<int, ItemData*> _Map;

public:
	const std::unordered_map<int, ItemData*>  GetMap() const {return _Map;}
	const ItemData* Find(int Id)const
	{
		auto it = _Map.find(Id);
		return it != _Map.end() ? it->second : nullptr;
	}
	
protected:
		void BuildMap() override
		{
			_Map.clear();
			for (auto it = _List.begin(); it != _List.end(); ++it)
			{
				ItemData* item = *it;				
				_Map[item->Id] = item;
			}
		}

};

class TableTestComposeKey : public TableList<TestComposeKey>
{
public:
	TableTestComposeKey() :TableList("TestComposeKey",  false) {}


private:
	std::unordered_map<std::tuple<unsigned int, int, int>, TestComposeKey*,TupleHash> _Map;

public:
	const std::unordered_map<std::tuple<unsigned int, int, int>, TestComposeKey*,TupleHash>  GetMap() const {return _Map;}
	const TestComposeKey* Find(unsigned int Id, int Level, int Gender)const
	{
		auto it = _Map.find(std::tuple<unsigned int, int, int>(Id,Level,Gender));
		return it != _Map.end() ? it->second : nullptr;
	}
	
protected:
		void BuildMap() override
		{
			_Map.clear();
			for (auto it = _List.begin(); it != _List.end(); ++it)
			{
				TestComposeKey* item = *it;				
				_Map[std::tuple<unsigned int, int, int>(item->Id,item->Level,item->Gender)]= item;
			}
		}

};

class TableLoc : public TableList<Loc>
{
public:
	TableLoc() :TableList("Loc",  false) {}

private:
	std::unordered_map<int, Loc*> _Map;

public:
	const std::unordered_map<int, Loc*>  GetMap() const {return _Map;}
	const Loc* Find(int Id)const
	{
		auto it = _Map.find(Id);
		return it != _Map.end() ? it->second : nullptr;
	}
	
protected:
		void BuildMap() override
		{
			_Map.clear();
			for (auto it = _List.begin(); it != _List.end(); ++it)
			{
				Loc* item = *it;				
				_Map[item->Id] = item;
			}
		}

};

struct TableMgr
{	
	const static int LangCount = 2;
	const static std::string LangList[LangCount];
	std::unordered_map<TableTypeInfo, Table*, TableTypeInfo, TableTypeInfo> AllTableDict;


	TableItemData ItemData;
	TableTestComposeKey TestComposeKey;
	TableLoc Loc;

	TableMgr():AllTableDict()
	{
       AllTableDict[typeid(TableItemData)] = &ItemData;
       AllTableDict[typeid(TableTestComposeKey)] = &TestComposeKey;
       AllTableDict[typeid(TableLoc)] = &Loc;
	}
};
inline const std::string TableMgr::LangList[] = {"{lang_list}"};	
}
