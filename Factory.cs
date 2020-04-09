using System.Collections.Generic;

using Serializers;

namespace AlcoholicDrinks
{
    abstract class Serializator
    {
        protected string FilePath;
        public abstract void Serialize(List<object> object_list);
        public abstract List<object> Deserealize();
    }

    abstract class Creator
    {
        public abstract Serializator Create(string fPath);
    }

    class BinaryCreator : Creator
    {
        public override Serializator Create(string fPath)
        {
            return new BinarySerializator(fPath);
        }
    }

    class TextCreator : Creator
    {
        public override Serializator Create(string fPath)
        {
            return new TextSerializator(fPath);
        }
    }

    class JsonCreator : Creator
    {
        public override Serializator Create(string fPath)
        {
            return new JsonSerializator(fPath);
        }
    }

}