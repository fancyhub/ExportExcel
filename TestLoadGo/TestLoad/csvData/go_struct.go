package csvData

import (
	"sync"

	"go.uber.org/zap"
)

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

type CsvPair_int32_bool struct {
	Item0 int32
	Item1 bool
}
type CsvPair_int32_int64 struct {
	Item0 int32
	Item1 int64
}
type CsvPair_float32_float32_float32 struct {
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

	//
	PairField CsvPair_int32_bool

	//
	PairFieldList []CsvPair_int32_int64

	//
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
	Pos CsvPair_float32_float32_float32
}

type csvLoader func() error
type IDataReader interface {
	Read2Array(file_name string) ([][]string, error)
}
type CsvDataMgr struct {
	logger            *zap.Logger
	reader            IDataReader
	FileName2Func     map[string]csvLoader
	FileName2ListData map[string]interface{}
	FileName2MapData  map[string]interface{}

	TItemDataMux  sync.RWMutex
	TItemDataList []TItemData
	TItemDataMap  map[int32]*TItemData

	TTestComposeKeyMux  sync.RWMutex
	TTestComposeKeyList []TTestComposeKey
	TTestComposeKeyMap  map[uint32]map[int32]*TTestComposeKey
}
