using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using AlcoholicDrinks;

namespace Serializers
{
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
            result += '?' + class_type.ToString() + "{";

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

        private void SetFieldValue(object obj, FieldInfo field, string value)
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
            int class_str_start = str.IndexOf('?') + 1;
            int class_str_end = str.IndexOf('{');
            int class_decl_end = GetCloseBracketPos(str, class_str_end);
            string class_str = str.Substring(class_str_start, class_str_end - class_str_start);
            Type class_type = Type.GetType(class_str, false, true);
            object obj = Activator.CreateInstance(class_type);
            str = str.Substring(class_str_end + 1, class_decl_end - class_str_end - 1);

            var sub_objects = new List<object>();
            while (str.Contains("?"))
            {
                sub_objects.Add(StrToObject(str));
                int rem_start = str.IndexOf("?");
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
                                SetFieldValue(obj, field, value);
                            }
                            else
                            {
                                foreach (object sub_obj in sub_objects)
                                {
                                    if (field.FieldType == sub_obj.GetType())
                                    {
                                        field.SetValue(obj, sub_obj);
                                        sub_objects.Remove(sub_obj);
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
}
