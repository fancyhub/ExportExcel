// See https://aka.ms/new-console-template for more information
using Test;
using TestLoadCs.table_reader;

const string dirPath0 = "../../../../TestData/0_no_loc/Output/Client/Data";
const string dirPath1 = "../../../../TestData/1_loc/Output/Client/Data";
const string dirPath2 = "../../../../TestData/2_loc_auto_key/Output/Client/Data";

LocLang.Lang = "zh-Hans";

TableMgr.Inst.LoadAllTable(ETableReaderType.Bin, dirPath2);
TableMgr.Get(1, 2, out TTestComposeKey v);
List<TItemData> itemList = TableMgr.GetList<TItemData>();
List<TTestComposeKey> composeKeyItemList = TableMgr.GetList<TTestComposeKey>();

v = TableMgr.GetTTestComposeKey(1, 2);
Console.WriteLine("Hello, World! " + v.Name);

