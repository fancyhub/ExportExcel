#pragma once
#include "../gen/table_loader.h"
using namespace Test;

class TableRowReaderCsv : public ITableRowReader
{
public:
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
