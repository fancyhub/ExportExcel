package config

import (
    "sync"
);
// EItemType
type EItemType int32
const (
	// 无
	EItemType_None EItemType = 0
	// 武器
	EItemType_Weapon EItemType = 1
	// 消耗品
	EItemType_Cosume EItemType = 2
)

// EItemSubType
type EItemSubType int32
const (
	// 无
	EItemSubType_None EItemSubType = 0
	// 手枪
	EItemSubType_ShotGun EItemSubType = 1
	// 加农炮
	EItemSubType_Cannon EItemSubType = 2
)

// EItemQuality
type EItemQuality int32
const (
	// 普通
	EItemQuality_None EItemQuality = 0
	// 灰色
	EItemQuality_Gray EItemQuality = 2
	// 绿色
	EItemQuality_Green EItemQuality = 3
	// 紫色
	EItemQuality_Purple EItemQuality = 4
)

// EItemFlag
type EItemFlag int32
const (
	// 无
	EItemFlag_None EItemFlag = 0
	// 可堆叠
	EItemFlag_Stack EItemFlag = 1
	// 可删除
	EItemFlag_CanDelete EItemFlag = 2
)

type TupleInt32Bool struct{
	Item0 int32
	Item1 bool
}
type TupleInt32Int32 struct{
	Item0 int32
	Item1 int32
}
type TupleInt32Int64 struct{
	Item0 int32
	Item1 int64
}
type TupleFloat32Float32Float32 struct{
	Item0 float32
	Item1 float32
	Item2 float32
}
type TItemData struct {
	// PK
	// 物品ID
	Id int32

	// 类型
	Type EItemType

	// 子类
	SubType EItemSubType

	// 品质
	Quality EItemQuality

	// 测试Pair
	PairField TupleInt32Bool

	// 测试Pair
	PairField2 TupleInt32Bool

	// 测试Pair
	PairField3 TupleInt32Int32

	// 测试PairList
	PairFieldList []TupleInt32Int64

	// 测试PairList
	PairFieldList2 []TupleInt32Int64

	// 测试List
	ListField []int32

}
type TTestComposeKey struct {
	// PK[Level]
	// 角色Id
	Id uint32

	// 等级
	Level int32

	// 名字
	Name int32

	// 位置
	Pos TupleFloat32Float32Float32

	// 标记位
	Flags EItemFlag

}
