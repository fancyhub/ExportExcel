#pragma once
#include "../gen/table_loader_csv.h"
using namespace Test;

class TableTupleReaderCsv : public ITableTupleReader
{
public:
	std::vector<std::string> _list;
	int _current;

public:
	void Set(const std::string& str);

public:
	bool Read(bool& v);

	bool Read(int& v);
	bool Read(unsigned int& v);

	bool Read(long long& v);
	bool Read(unsigned long long& v);

	bool Read(float& v);
	bool Read(double& v);

	bool Read(LocId& v);
	bool Read(LocStr& v);
	bool Read(std::string& v);
};

class TableListReaderCsv : public ITableListReader
{
public:
	std::vector<std::string> _list;
	int _current;
	TableTupleReaderCsv _tupleReader;

public:
	void Set(const std::string& str);

public:
	int GetCount();
	ITableTupleReader* NextTuple();

	bool Read(bool& v);

	bool Read(int& v);
	bool Read(unsigned int& v);

	bool Read(long long& v);
	bool Read(unsigned long long& v);

	bool Read(float& v);
	bool Read(double& v);

	bool Read(LocId& v);
	bool Read(LocStr& v);
	bool Read(std::string& v);
};

class TableRowReaderCsv : public ITableRowReader
{
public:
	TableListReaderCsv _listReader;
	TableTupleReaderCsv _tupleReader;
	std::vector<std::string*> _row;
	int _curIndex;

public:
	ITableListReader* BeginList();
	ITableTupleReader* BeginTuple();

	bool Read(bool& v);

	bool Read(int& v);
	bool Read(unsigned int& v);

	bool Read(long long& v);
	bool Read(unsigned long long& v);

	bool Read(float& v);
	bool Read(double& v);

	bool Read(LocId& v);
	bool Read(LocStr& v);
	bool Read(std::string& v);
};

class TableReaderCsv : public ITableReader
{
private:
	std::vector<std::string> _body;
	std::vector<std::string> _header;
	int _colCount;
	int _rowCount;
	int _curRow;
	TableRowReaderCsv _RowReader;

public:
	void LoadCsvFile(const std::string& filePath);

public:

	const std::vector<std::string>& ReadHeader();
	int ReadRowCount();

	ITableRowReader* NextRow();
};
