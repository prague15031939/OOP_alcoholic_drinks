using System;
using System.Windows.Forms;
using System.Reflection;

using Alcohol;

namespace AlcoholicDrinks
{
    public partial class frmReport : Form
    {
        public frmReport(object SourceObject)
        {
            InitializeComponent();

            ViewObject(SourceObject);
        }

        private void ViewObject(object obj, int indent = 0)
        {
            if (obj == null)
                return;

            Type ClassType = obj.GetType();
            if (indent == 0)
                txtReport.AppendText($"class: {ClassType}\n\n");

            foreach (FieldInfo field in ClassType.GetFields())
            {
                string ItemStr = "";
                for (int i = 0; i < indent; i++)
                    ItemStr += " ";

                if (Attribute.IsDefined(field, typeof(NameAttribute)))
                    ItemStr += (Attribute.GetCustomAttribute(field, typeof(NameAttribute)) as NameAttribute).Name;
                else
                    ItemStr += field.Name;

                object value;
                if (field.GetValue(obj) == null)
                    value = "null";
                else if ((field.GetValue(obj) as string) == "")
                    value = "\"\"";
                else if (!(field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum))
                    value = field.GetValue(obj);
                else
                    value = "...";
                ItemStr += $" -> {value.ToString()}";
                txtReport.AppendText($"{ItemStr}\n");

                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                    ViewObject(field.GetValue(obj), 4);
            }
        }
    }
}
