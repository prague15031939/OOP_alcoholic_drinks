using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

using System.Reflection;
using Alcohol;

namespace lab1
{
    abstract class Serializator
    {
        protected string FilePath;
        public abstract void Serialize(List<object> object_list);
        public abstract List<object> Deserealize();
    }

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

    class TextSerializator : Serializator
    {
        public TextSerializator(string fPath)
        {
            FilePath = fPath;
        }

        private string ObjectToStr(object obj)
        {
            string result = "";
            if (obj == null)
                return result;

            Type class_type = obj.GetType();
            result += '\u0000' + class_type.ToString() + "{";

            foreach (FieldInfo field in class_type.GetFields())
            {
                string str_item = field.Name;

                object value = "";
                if (field.GetValue(obj) == null)
                    value = "null";
                else if ((field.GetValue(obj) as string) == "")
                    value = "\"\"";
                else if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                    value += ObjectToStr(field.GetValue(obj));
                else
                    value = field.GetValue(obj);
                str_item += $":{value.ToString()};";
                result += str_item;
            }
            return result + "}";
        }

        private void SetFieldValue(ref object obj, FieldInfo field, string value)
        {
            if (value == "null")
                field.SetValue(obj, null);
            else if (value == "\"\"")
                field.SetValue(obj, "");
            else if (field.FieldType.Name == "String")
                field.SetValue(obj, value);
            else if (field.FieldType.Name == "Int32")
                field.SetValue(obj, Convert.ToInt32(value));
            else if (field.FieldType.Name == "Double")
                field.SetValue(obj, Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture));
            else if (field.FieldType.Name == "Boolean")
                field.SetValue(obj, Convert.ToBoolean(value));
            else if (field.FieldType.IsEnum)
                field.SetValue(obj, Enum.Parse(field.FieldType, value));
        }

        private int GetCloseBracketPos(string str, int start_pos)
        {
            bool started = false;
            int rank = 0;
            for (int i = start_pos; i < str.Length; i++)
            {
                if (str[i] == '{') { rank++; started = true; }
                if (str[i] == '}') rank--;
                if (rank == 0 && started) return i;
            }
            return -1;
        }
        private object StrToObject(string str)
        {
            int class_str_start = str.IndexOf('\u0000') + 1;
            int class_str_end = str.IndexOf('{');
            int class_decl_end = GetCloseBracketPos(str, class_str_end);
            string class_str = str.Substring(class_str_start, class_str_end - class_str_start);
            Type class_type = Type.GetType(class_str, false, true);
            object obj = Activator.CreateInstance(class_type);
            str = str.Substring(class_str_end + 1, class_decl_end - class_str_end - 1);

            var sub_objects = new List<object>();
            while (str.Contains("\u0000"))
            {
                sub_objects.Add(StrToObject(str));
                int rem_start = str.IndexOf("\u0000");
                int rem_end = GetCloseBracketPos(str, rem_start);
                str = str.Remove(rem_start, rem_end - rem_start + 1);
            }

            string[] item_list = str.Split(';');
            foreach (string item in item_list)
            {
                if (item != "")
                {
                    string field_str = item.Split(':')[0];
                    string value = item.Split(':')[1];

                    foreach (FieldInfo field in class_type.GetFields())
                    {
                        if (field.Name == field_str)
                        {
                            if (value != "")
                            {
                                SetFieldValue(ref obj, field, value);
                            }
                            else
                            {
                                foreach (object sub_obj in sub_objects)
                                {
                                    if (field.FieldType == sub_obj.GetType())
                                    {
                                        field.SetValue(obj, sub_obj);
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            return obj;
        }

        public override void Serialize(List<object> object_list)
        {
            using (StreamWriter fStream = new StreamWriter(FilePath))
            {
                fStream.WriteLine(object_list.Count.ToString());
                foreach (object obj in object_list)
                    fStream.WriteLine(ObjectToStr(obj));
            }
        }

        public override List<object> Deserealize()
        {
            using (StreamReader fStream = new StreamReader(FilePath))
            {
                var object_list = new List<object>();
                int object_amount = Convert.ToInt32(fStream.ReadLine());
                for (int i = 0; i < object_amount; i++)
                {
                    object obj = StrToObject(fStream.ReadLine());
                    object_list.Add(obj);
                }
                return object_list;
            }
        }
    }

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