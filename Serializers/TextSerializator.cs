using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using AlcoholicDrinks;
using PluginInterface;

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

            Type ClassType = obj.GetType();
            result += '?' + ClassType.ToString() + "{";

            foreach (FieldInfo field in ClassType.GetFields())
            {
                string ItemStr = field.Name;

                object value = "";
                if (field.GetValue(obj) == null)
                    value = "null";
                else if ((field.GetValue(obj) as string) == "")
                    value = "\"\"";
                else if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                    value += ObjectToStr(field.GetValue(obj));
                else
                    value = field.GetValue(obj);
                ItemStr += $":{value.ToString()};";
                result += ItemStr;
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

        private int GetCloseBracketPos(string str, int StartPos)
        {
            bool started = false;
            int rank = 0;
            for (int i = StartPos; i < str.Length; i++)
            {
                if (str[i] == '{') { rank++; started = true; }
                if (str[i] == '}') rank--;
                if (rank == 0 && started) return i;
            }
            return -1;
        }

        private object StrToObject(string str)
        {
            int ClassStrStart = str.IndexOf('?') + 1;
            int ClassStrEnd = str.IndexOf('{');
            int ClassDeclarationEnd = GetCloseBracketPos(str, ClassStrEnd);
            string class_str = str.Substring(ClassStrStart, ClassStrEnd - ClassStrStart);
            Type ClassType = Type.GetType(class_str, false, true);
            object obj = Activator.CreateInstance(ClassType);
            str = str.Substring(ClassStrEnd + 1, ClassDeclarationEnd - ClassStrEnd - 1);

            var SubObjects = new List<object>();
            while (str.Contains("?"))
            {
                SubObjects.Add(StrToObject(str));
                int remStart = str.IndexOf("?");
                int remEnd = GetCloseBracketPos(str, remStart);
                str = str.Remove(remStart, remEnd - remStart + 1);
            }

            string[] ItemList = str.Split(';');
            foreach (string item in ItemList)
            {
                if (item != "")
                {
                    string FieldStr = item.Split(':')[0];
                    string value = item.Split(':')[1];

                    foreach (FieldInfo field in ClassType.GetFields())
                    {
                        if (field.Name == FieldStr)
                        {
                            if (value != "")
                            {
                                SetFieldValue(obj, field, value);
                            }
                            else
                            {
                                foreach (object SubObj in SubObjects)
                                {
                                    if (field.FieldType == SubObj.GetType())
                                    {
                                        field.SetValue(obj, SubObj);
                                        SubObjects.Remove(SubObj);
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

        public override void Serialize(List<object> ObjectList, IPlugin plugin)
        {
            string TextString = "";
            foreach (object obj in ObjectList)
                TextString += $"{ObjectToStr(obj)}\n";
            if (plugin == null)
            {
                using (StreamWriter fStream = new StreamWriter(FilePath))
                {
                    fStream.Write(TextString);
                }
            }
            else
                plugin.PostProcessString(TextString, FilePath);
        }

        public override List<object> Deserealize(IPlugin plugin)
        {
            string TextString;
            if (plugin == null)
            {
                using (StreamReader fStream = new StreamReader(FilePath))
                {
                    TextString = fStream.ReadToEnd();    
                }
            }
            else
                TextString = plugin.PreProcessString(FilePath);

            var ObjectList = new List<object>();
            string[] LineList = TextString.Split('\n');
            foreach (string line in LineList)
            {
                if (line != "")
                {
                    object obj = StrToObject(line);
                    ObjectList.Add(obj);
                }
            }
            return ObjectList;
        }
    }
}
