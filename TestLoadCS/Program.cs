// See https://aka.ms/new-console-template for more information
using Test;

const string dirPath0 = "../../../../TestData/0_no_loc/Output/Client/Data";
const string dirPath1 = "../../../../TestData/1_loc/Output/Client/Data";
const string dirPath2 = "../../../../TestData/2_loc_auto_key/Output/Client/Data";

LocLang.Lang = TableLoaderMgr.LangList[0];
ITableReaderCreator tableReaderCreator = new TableReaderCsvCreator(dirPath2);
//tableReaderCreator = new TableReaderBinCreator(dirPath2);
TableMgr mgr = new TableMgr(tableReaderCreator);

mgr.LoadAllTable();
mgr.Get(1, 2, out TTestComposeKey v);
List<TItemData> itemList = mgr.GetList<TItemData>();
List<TTestComposeKey> composeKeyItemList = mgr.GetList<TTestComposeKey>();

v = mgr.GetTTestComposeKey(1, 2);
Console.WriteLine("Hello, World! " + v.Name);

