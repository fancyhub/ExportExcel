using Newtonsoft.Json.Linq;
using Test;

public class MainProgram
{
    const string dirPath0 = "../../../../Data/0_no_loc/Output/Client/Data";
    const string dirPath1 = "../../../../Data/1_loc/Output/Client/Data";
    const string dirPath2 = "../../../../Data/2_loc_auto_key/Output/Client/Data";

    public static int Main(string[] args)
    {
        //TestLoadJson();
        //TestLoadBson();

        string dir = Path.GetFullPath(dirPath2);

        //cvs loader
        {
            ITableCsvReaderCreator tableReaderCreator = new TableReaderCsvTextCreator(dir);
            TableMgr mgr = new TableMgr(tableReaderCreator);

            mgr.LoadFromCsv("zh-Hans");

            List<TItemData> itemList = mgr.ItemData.List;
            List<TTestComposeKey> composeKeyItemList = mgr.TestComposeKey.List;
            var v = mgr.TestComposeKey.Find(1, 2);
            Console.WriteLine("Hello, World! " + v.Name);
        }

        //cvs bin
        {
            ITableCsvReaderCreator tableReaderCreator = new TableReaderCsvBinCreator(dir);
            TableMgr mgr = new TableMgr(tableReaderCreator);

            mgr.LoadFromCsv("zh-Hans");

            List<TItemData> itemList = mgr.ItemData.List;
            List<TTestComposeKey> composeKeyItemList = mgr.TestComposeKey.List;
            var v = mgr.TestComposeKey.Find(1, 2);
            Console.WriteLine("Hello, World! " + v.Name);
        }


        //json
        {
            TableMgr mgr = new TableMgr(null);

            mgr.LoadFromJson("zh-Hans",dir);

            List<TItemData> itemList = mgr.ItemData.List;
            List<TTestComposeKey> composeKeyItemList = mgr.TestComposeKey.List;
            var v = mgr.TestComposeKey.Find(1, 2);
            Console.WriteLine("Hello, World! " + v.Name);
        }

        return 0;
    }   
}
