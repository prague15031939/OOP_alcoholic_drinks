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

        public override void Serialize(List<object> ObjectList, IPlugin plugin)
        {
            MemoryStream SerializationStream = new MemoryStream();
            formatter.Serialize(SerializationStream, ObjectList);
            if (plugin == null)
            {
                using (FileStream fStream = new FileStream(FilePath, FileMode.Create))
                {
                    SerializationStream.Position = 0;
                    SerializationStream.CopyTo(fStream);
                }
            }
            else
                plugin.PostProcessStream(SerializationStream, FilePath);
        }

        public override List<object> Deserealize(IPlugin plugin)
        {
            MemoryStream SerializationStream = new MemoryStream();
            if (plugin == null)
            {
                using (FileStream fStream = new FileStream(FilePath, FileMode.Open))
                {
                    fStream.CopyTo(SerializationStream);
                }
            }
            else
                SerializationStream = plugin.PreProcessStream(FilePath);
            SerializationStream.Position = 0;
            List<object> ObjectList = (List<object>)formatter.Deserialize(SerializationStream);
            return ObjectList;
        }
    }
}
