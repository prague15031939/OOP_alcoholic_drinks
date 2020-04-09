using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

using AlcoholicDrinks;

namespace Serializers
{
    class JsonSerializator : Serializator
    {
        private JsonSerializerSettings json_settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
        };

        public JsonSerializator(string fPath)
        {
            FilePath = fPath;
        }

        public override void Serialize(List<object> object_list)
        {
            using (StreamWriter fStream = new StreamWriter(FilePath))
            {
                string json_string = JsonConvert.SerializeObject(object_list, json_settings);
                fStream.Write(json_string);
            }
        }

        public override List<object> Deserealize()
        {
            using (StreamReader fStream = new StreamReader(FilePath))
            {
                string json_string = fStream.ReadToEnd();
                var object_list = JsonConvert.DeserializeObject<List<object>>(json_string, json_settings);
                return object_list;
            }
        }
    }
}
