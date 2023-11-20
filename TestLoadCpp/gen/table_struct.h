
#pragma once
//自动生成的
#include <tuple>
#include <vector>
#include <string>
#include <unordered_map>
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

		TItem* Get(TKey key)
		{
			auto it = Dict.find(key);
			return it != Dict.end() ? it->second.get() : nullptr;
		}
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
		/// <summary>
		/// PK
		/// 物品ID
		/// </summary>
		int Id;
		/// <summary>
		/// 名称
		/// </summary>
		LocId Name;
		/// <summary>
		/// 类型
		/// </summary>
		EItemType Type;
		/// <summary>
		/// 子类
		/// </summary>
		EItemSubType SubType;
		/// <summary>
		/// 品质
		/// </summary>
		EItemQuality Quality;
		/// <summary>
		/// 测试Pair
		/// </summary>
		std::tuple<int, bool> PairField;
		/// <summary>
		/// 测试Pair
		/// </summary>
		std::tuple<int, bool> PairField2;
		/// <summary>
		/// 测试Pair
		/// </summary>
		std::tuple<int, int> PairField3;
		/// <summary>
		/// 测试PairList
		/// </summary>
		std::vector<std::tuple<int, long long>> PairFieldList;
		/// <summary>
		/// 测试PairList
		/// </summary>
		std::vector<std::tuple<int, long long>> PairFieldList2;
		/// <summary>
		/// 测试List
		/// </summary>
		std::vector<int> ListField;

	};

	struct TTestComposeKey
	{
		/// <summary>
		/// PK[Level]
		/// 角色Id
		/// </summary>
		unsigned int Id;
		/// <summary>
		/// 等级
		/// </summary>
		int Level;
		/// <summary>
		/// 名字
		/// </summary>
		LocId Name;
		/// <summary>
		/// 位置
		/// </summary>
		std::tuple<float, float, float> Pos;
		/// <summary>
		/// 标记位
		/// </summary>
		EItemFlag Flags;

	};

	struct TLoc
	{
		/// <summary>
		/// PK
		/// id
		/// </summary>
		int Id;
		/// <summary>
		/// 
		/// </summary>
		std::string Val;

	};
}
