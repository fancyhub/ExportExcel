// See https://aka.ms/new-console-template for more information
using Test;

const string dirPath0 = "../../../../Data/0_no_loc/Output/Client/Data";
const string dirPath1 = "../../../../Data/1_loc/Output/Client/Data";
const string dirPath2 = "../../../../Data/2_loc_auto_key/Output/Client/Data";

string dir = Path.GetFullPath(dirPath2);

LocLang.Lang = TableLoaderMgr.LangList[0];
ITableReaderCreator tableReaderCreator = new TableReaderCsvCreator(dir);
tableReaderCreator = new TableReaderBinCreator(dirPath2);
TableMgr mgr = new TableMgr(tableReaderCreator);

mgr.LoadAllTable();
var v = mgr.GetTTestComposeKey(1, 2);

List<TItemData> itemList = mgr.GetList<TItemData>();
List<TTestComposeKey> composeKeyItemList = mgr.GetList<TTestComposeKey>();

v = mgr.GetTTestComposeKey(1, 2);
 Console.WriteLine("Hello, World! " + v.Name);

