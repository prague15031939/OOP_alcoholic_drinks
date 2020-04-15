using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using PluginInterface;
using AlcoholicDrinks;

namespace Serializers
{
    class BinarySerializator : Serializator
    {
        private BinaryFormatter formatter = new BinaryFormatter();

        public BinarySerializator(string fPath)
        {
            FilePath = fPath;
        }

        public override void Serialize(List<object> object_list, IPlugin plugin)
        {
            MemoryStream serialization_stream = new MemoryStream();
            formatter.Serialize(serialization_stream, object_list);
            if (plugin == null)
            {
                using (FileStream fStream = new FileStream(FilePath, FileMode.Create))
                {
                    serialization_stream.Position = 0;
                    serialization_stream.CopyTo(fStream);
                }
            }
            else
                plugin.PostProcessStream(serialization_stream, FilePath);
        }

        public override List<object> Deserealize(IPlugin plugin)
        {
            MemoryStream serialization_stream = new MemoryStream();
            if (plugin == null)
            {
                using (FileStream fStream = new FileStream(FilePath, FileMode.Open))
                {
                    fStream.CopyTo(serialization_stream);
                }
            }
            else
                serialization_stream = plugin.PreProcessStream(FilePath);
            serialization_stream.Position = 0;
            List<object> object_list = (List<object>)formatter.Deserialize(serialization_stream);
            return object_list;
        }
    }
}
