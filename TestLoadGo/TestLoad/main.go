package main

import (
	"TestLoad/TestLoad/csvData"
	"encoding/json"
	"fmt"

	"go.uber.org/zap"
)

func createLog() *zap.Logger {
	sampleJSON := []byte(`{
		"level" : "debug",
		"encoding": "json",
		"outputPaths":["stdout"],
		"errorOutputPaths":["stderr"],
		"encoderConfig": {
			"messageKey":"message",
			"levelKey":"level",
			"levelEncoder":"lowercase"
		}
	}`)

	var cfg zap.Config

	if err := json.Unmarshal(sampleJSON, &cfg); err != nil {
		panic(err)
	}

	logger, _ := cfg.Build()
	return logger
}

func main() {

	//1. 准备
	logger := createLog()
	reader := CreateDataReader(logger)

	//2. 初始化
	dataMgr, _ := csvData.CreateCsvDataMgr(logger, reader)

	//3. 加载
	dataMgr.LoadAll()

	//4. 读取配置
	d := dataMgr.TTestComposeKeyMap[1][2]
	if d != nil {
		println(d.Name)
		println(fmt.Sprint(float32(d.Level)))
	}
	println("Hello World")
}
