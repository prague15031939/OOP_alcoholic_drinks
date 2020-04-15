using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

using AlcoholicDrinks;
using PluginInterface;

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

        public override void Serialize(List<object> object_list, IPlugin plugin)
        {
            string json_string = JsonConvert.SerializeObject(object_list, json_settings);
            if (plugin == null)
            {
                using (StreamWriter fStream = new StreamWriter(FilePath))
                {
                    fStream.Write(json_string);
                }
            }
            else
                plugin.PostProcessString(json_string, FilePath);
        }

        public override List<object> Deserealize(IPlugin plugin)
        {
            string json_string;
            if (plugin == null)
            {
                using (StreamReader fStream = new StreamReader(FilePath))
                {
                    json_string = fStream.ReadToEnd();
                }
            }
            else
                json_string = plugin.PreProcessString(FilePath);
            var object_list = JsonConvert.DeserializeObject<List<object>>(json_string, json_settings);
            return object_list;
        }
    }
}
