using System;
using System.Windows.Forms;
using System.Reflection;

using Alcohol;

namespace AlcoholicDrinks
{
    public partial class frmReport : Form
    {
        public frmReport(object source_object)
        {
            InitializeComponent();

            ViewObject(source_object);
        }

        private void ViewObject(object obj, int indent = 0)
        {
            if (obj == null)
                return;

            Type class_type = obj.GetType();
            if (indent == 0)
                txtReport.AppendText($"class: {class_type}\n\n");

            foreach (FieldInfo field in class_type.GetFields())
            {
                string str_item = "";
                for (int i = 0; i < indent; i++)
                    str_item += " ";

                if (Attribute.IsDefined(field, typeof(NameAttribute)))
                    str_item += (Attribute.GetCustomAttribute(field, typeof(NameAttribute)) as NameAttribute).Name;
                else
                    str_item += field.Name;

                object value;
                if (field.GetValue(obj) == null)
                    value = "null";
                else if ((field.GetValue(obj) as string) == "")
                    value = "\"\"";
                else if (!(field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum))
                    value = field.GetValue(obj);
                else
                    value = "...";
                str_item += $" -> {value.ToString()}";
                txtReport.AppendText($"{str_item}\n");

                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                    ViewObject(field.GetValue(obj), 4);
            }
        }
    }
}
