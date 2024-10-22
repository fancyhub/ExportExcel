
#pragma once
//自动生成的
#include <tuple>
#include <vector>
#include <string>
#include <unordered_map>
#include <typeinfo>
#include "../dep/loc_str.h"
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

struct ItemData 
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

struct TestComposeKey 
{
	// PK[Level,Gender]
	// 角色Id
	unsigned int Id;

	// 等级
	int Level;

	// 性别
	int Gender;

	// 名字
	LocId Name;

	// 位置
	std::tuple<float,float,float> Pos;

	// 标记位
	EItemFlag Flags;

};

struct Loc 
{
	// PK
	// id
	int Id;

	// 
	std::string Val;

};
}
