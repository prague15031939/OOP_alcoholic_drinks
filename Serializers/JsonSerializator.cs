using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

using AlcoholicDrinks;
using PluginInterface;

namespace Serializers
{
    class JsonSerializator : Serializator
    {
        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
        };

        public JsonSerializator(string fPath)
        {
            FilePath = fPath;
        }

        public override void Serialize(List<object> ObjectList, IPlugin plugin)
        {
            string JsonString = JsonConvert.SerializeObject(ObjectList, JsonSettings);
            if (plugin == null)
            {
                using (StreamWriter fStream = new StreamWriter(FilePath))
                {
                    fStream.Write(JsonString);
                }
            }
            else
                plugin.PostProcessString(JsonString, FilePath);
        }

        public override List<object> Deserealize(IPlugin plugin)
        {
            string JsonString;
            if (plugin == null)
            {
                using (StreamReader fStream = new StreamReader(FilePath))
                {
                    JsonString = fStream.ReadToEnd();
                }
            }
            else
                JsonString = plugin.PreProcessString(FilePath);
            var ObjectList = JsonConvert.DeserializeObject<List<object>>(JsonString, JsonSettings);
            return ObjectList;
        }
    }
}
