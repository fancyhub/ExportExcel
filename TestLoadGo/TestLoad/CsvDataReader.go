package main

import (
	"TestLoad/TestLoad/csvData"
	"encoding/csv"
	"io"
	"os"
	"path/filepath"

	"go.uber.org/zap"
)

const (
	csvDirPath = "../../TestData/2_loc_auto_key/Output/Server/Data"
)

type CsvDataReader struct {
	logger *zap.Logger
}

func CreateDataReader(logger *zap.Logger) csvData.IDataReader {
	r := new(CsvDataReader)
	r.logger = logger
	return r
}

func (c *CsvDataReader) Read2Array(file_name string) ([][]string, error) {
	file_path := filepath.Join(csvDirPath, file_name)
	file_path, _ = filepath.Abs(file_path)

	f, err := os.Open(file_path)
	if err != nil {
		c.logger.Error("Load File Failed " + file_path)
		c.logger.Error(err.Error())
		return nil, err
	}
	defer f.Close()

	bom := make([]byte, 3)
	_, err = io.ReadFull(f, bom)
	if err != nil {
		c.logger.Error(err.Error())
		return nil, err
	}

	if bom[0] != 0xef || bom[1] != 0xbb || bom[2] != 0xbf {
		f.Seek(0, 0)
	}
	csvReader := csv.NewReader(f)
	data, err := csvReader.ReadAll()
	return data, err
}
