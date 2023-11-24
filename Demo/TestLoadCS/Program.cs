using Newtonsoft.Json.Linq;
using Test;

public class MainProgram
{
    const string dirPath0 = "../../../../Data/0_no_loc/Output/Client/Data";
    const string dirPath1 = "../../../../Data/1_loc/Output/Client/Data";
    const string dirPath2 = "../../../../Data/2_loc_auto_key/Output/Client/Data";

    public static int Main(string[] args)
    {
        var converts = new List<Newtonsoft.Json.JsonConverter>()
        {
            new JsonLocIdConverter()
        };

        string dir = Path.GetFullPath(dirPath2);
        string json_data = System.IO.File.ReadAllText(System.IO.Path.Combine(dirPath2, "ItemData.json"));
        List<TItemData> list_item_data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TItemData>>(json_data, converts.ToArray());


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


        return 0;
    }
}

public class JsonLocIdConverter : Newtonsoft.Json.Converters.CustomCreationConverter<LocId>
{
    public override LocId Create(Type objectType)
    {
        return new LocId();
    }

    public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
        {
            return null;
        }

        var value = JToken.Load(reader) as Newtonsoft.Json.Linq.JValue;
        if (value != null && value.Value is long t)
        {
            return new LocId((int)t);
        }

        return new LocId();
    }
}

