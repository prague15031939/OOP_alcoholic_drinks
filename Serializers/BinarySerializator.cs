using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        public override void Serialize(List<object> object_list)
        {
            using (FileStream fStream = new FileStream(FilePath, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fStream, object_list);
            }
        }

        public override List<object> Deserealize()
        {
            using (FileStream fStream = new FileStream(FilePath, FileMode.Open))
            {
                List<object> object_list = (List<object>)formatter.Deserialize(fStream);
                return object_list;
            }
        }
    }
}
