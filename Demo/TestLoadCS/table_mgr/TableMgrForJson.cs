using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test
{

    public partial class TableMgr
    {
        private JsonSerializer _JsonSerializer;
        private JsonSerializer _CreateJsonSerializer()
        {
            if (_JsonSerializer != null)
                return _JsonSerializer;

            JsonSerializerSettings jsonSetting = new JsonSerializerSettings()
            {
                Converters = new[] { new JsonLocIdConverter() },
                CheckAdditionalContent = true,
            };

            _JsonSerializer = JsonSerializer.CreateDefault(jsonSetting);
            return _JsonSerializer;

        }

        public void LoadFromJson(string lang, string dir)
        {
            var jsonSerializer = _CreateJsonSerializer();
            foreach (var p in AllTables)
            {
                var path = Path.Combine(dir, p.SheetName + ".json");

                if (p.IsMutiLang)
                {
                    continue;
                }

                var content = System.IO.File.ReadAllText(path);
                using JsonReader jsonReader = new JsonTextReader(new StringReader(content));
                if (p.LoadFromJson(jsonSerializer, jsonReader))
                    p.BuildMap();
            }
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
}