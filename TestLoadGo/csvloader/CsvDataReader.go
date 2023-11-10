package csvloader

import (
	"encoding/csv"
	"io"
	"os"
	"path/filepath"
)

type ICsvReaderLogger interface {
	Error(msg string)
}

type CsvDataReader struct {
	logger ICsvReaderLogger
	dir    string
}

func CreateDataReader(dir string, logger ICsvReaderLogger) *CsvDataReader {
	r := new(CsvDataReader)
	r.logger = logger
	r.dir = dir
	return r
}

func (c *CsvDataReader) Read2Array(file_name string) ([][]string, error) {
	file_path := filepath.Join(c.dir, file_name)
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
