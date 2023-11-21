
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

		const TItem* Get(TKey key)const
		{
			auto it = Dict.find(key);
			return it != Dict.end() ? it->second : nullptr;
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


enum class EItemType
{
	//  无
	None = 0,

	//  武器
	Weapon = 1,

	//  消耗品
	Cosume = 2,

};

enum class EItemSubType
{
	//  无
	None = 0,

	//  手枪
	ShotGun = 1,

	//  加农炮
	Cannon = 2,

};

enum class EItemQuality
{
	//  普通
	None = 0,

	//  灰色
	Gray = 2,

	//  绿色
	Green = 3,

	//  紫色
	Purple = 4,

};

enum class EItemFlag
{
	//  无
	None = 0,

	//  可堆叠
	Stack = 1,

	//  可删除
	CanDelete = 2,

};

struct TItemData 
{
	// PK
	// 物品ID
	int Id;

	// 名称
	LocId Name;

	// 类型
	EItemType Type;

	// 子类
	EItemSubType SubType;

	// 品质
	EItemQuality Quality;

	// 测试Pair
	std::tuple<int,bool> PairField;

	// 测试Pair
	std::tuple<int,bool> PairField2;

	// 测试Pair
	std::tuple<int,int> PairField3;

	// 测试PairList
	std::vector<std::tuple<int,long long>> PairFieldList;

	// 测试PairList
	std::vector<std::tuple<int,long long>> PairFieldList2;

	// 测试List
	std::vector<int> ListField;

};

struct TTestComposeKey 
{
	// PK[Level]
	// 角色Id
	unsigned int Id;

	// 等级
	int Level;

	// 名字
	LocId Name;

	// 位置
	std::tuple<float,float,float> Pos;

	// 标记位
	EItemFlag Flags;

};

struct TLoc 
{
	// PK
	// id
	int Id;

	// 
	std::string Val;

};
}
