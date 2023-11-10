package main

import (
	"example/config"
	"example/csvloader"
	"fmt"
	"log/slog"
)

const (
	csvDirPath0 = "../../TestData/0_no_loc/Output/Server/Data"
	csvDirPath1 = "../../TestData/1_loc/Output/Server/Data"
	csvDirPath2 = "../../TestData/2_loc_auto_key/Output/Server/Data"
)

type ExampleLogger struct {
}

func (log ExampleLogger) Error(msg string) {
	slog.Error(msg)
}

func main() {

	//1. 准备
	logger := new(ExampleLogger)
	reader := csvloader.CreateDataReader(csvDirPath2, logger)

	//2. 初始化
	dataMgr, _ := config.CreateCsvDataMgr(logger, reader)

	//3. 加载
	dataMgr.LoadAll()

	//4. 读取配置
	d := dataMgr.TTestComposeKeyMap[1][2]
	if d != nil {
		fmt.Println(*d)
	}
	fmt.Println("Hello World")
}
