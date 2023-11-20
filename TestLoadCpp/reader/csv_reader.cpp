#include "csv_reader.h"
#include "csv.h"
#include <iostream>
#include <fstream>
#include <stdio.h>

void TableReaderCsv::LoadCsvFile(const std::string& filePath)
{
	_body.clear();
	_header.clear();
	_colCount = 0;
	_rowCount = 0;
	_curRow = -1;

	FILE* file = nullptr;
	auto error= fopen_s(&file,filePath.c_str(), "r");
	if (file == nullptr)
		return;

	fseek(file, 0, SEEK_END);
	size_t size = ftell(file);
	fseek(file, 0, SEEK_SET);

	char* buff = new char[size];
	fread(buff, 1, size, file);
	fclose(file);

	Test::CsvReader reader(buff, size);
	delete[]buff;

	if (!reader.ReadRow(this->_header, true))
		return;
	
	this->_colCount = _header.size();
	if (!reader.ReadRow(this->_header, false))	
		return;	

	this->_body.clear();
	this->_rowCount = 0;
	for (;;)
	{
		if (!reader.ReadRow(this->_body, false))
			break;
		this->_rowCount++;
	}

	this->_curRow = -1;
}

const std::vector<std::string>& TableReaderCsv::ReadHeader()
{
	return this->_header;
}

int TableReaderCsv::ReadRowCount()
{
	return _rowCount;
}

Test::ITableRowReader* TableReaderCsv::NextRow()
{
	this->_curRow++;
	if (this->_curRow >= _rowCount)
		return nullptr;

	int start = _curRow * _colCount;
	_RowReader._row.clear();
	for (int i = 0; i < _colCount; i++)
	{
		auto item = &_body.at(i + start);
		_RowReader._row.push_back(item);
		_RowReader._curIndex = -1;
	}
	return &_RowReader;
}


//////////////////////////////

ITableListReader* TableRowReaderCsv::BeginList()
{
	++_curIndex;
	return nullptr;
}
ITableTupleReader* TableRowReaderCsv::BeginTuple()
{
	++_curIndex;
	return nullptr;
}

bool TableRowReaderCsv::Read(bool& v)
{
	v = _row[++_curIndex]->_Equal("0");
	return true;
}

bool TableRowReaderCsv::Read(int& v)
{
	v = atoi(_row[++_curIndex]->c_str());
	return true;
}

bool TableRowReaderCsv::Read(unsigned int& v)
{
	v = atoi(_row[++_curIndex]->c_str());
	return true;
}

bool TableRowReaderCsv::Read(long long& v)
{
	v = atoll(_row[++_curIndex]->c_str());
	return true;
}
bool TableRowReaderCsv::Read(unsigned long long& v)
{
	v = atoll(_row[++_curIndex]->c_str());
	return true;
}

bool TableRowReaderCsv::Read(float& v)
{
	v =  atof(_row[++_curIndex]->c_str());
	return true;
}
bool TableRowReaderCsv::Read(double& v)
{
	v = atof(_row[++_curIndex]->c_str());
	return true;
}

bool TableRowReaderCsv::Read(LocId& v)
{
	++_curIndex;
	return true;
}
bool TableRowReaderCsv::Read(LocStr& v)
{
	++_curIndex;
	return true;
}
bool TableRowReaderCsv::Read(std::string& v)
{
	v = *_row[++_curIndex];
	return true;
}