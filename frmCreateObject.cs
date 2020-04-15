using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

using Alcohol;

namespace AlcoholicDrinks
{
    public delegate void ObjectReturnMethod(object obj);

    public partial class frmCreateObject : Form
    {
        private List<Control> ControlList = new List<Control>();
        private Type CurrentClassType;
        private Dictionary<FieldInfo, object> AssociationDict = new Dictionary<FieldInfo, object>();
        private object TargetObject;
        private FieldInfo TargetField;
        private ObjectReturnMethod ReturnObject;
        private int startY = 25, countY = 0, startX = 25, countX = 0;

        public frmCreateObject(string ClassStr, object TargetObj, ObjectReturnMethod ReturnObj)
        {
            InitializeComponent();

            CurrentClassType = Type.GetType(ClassStr, false, true);
            TargetObject = TargetObj;
            ReturnObject = ReturnObj;
            GenerateComponents(CurrentClassType.GetFields());
        }

        private void CreateButton(string caption, string name, object tag = null)
        {
            var btn = new Button();
            btn.Location = new Point(startX + 215 * countX, startY + 50 * countY);
            btn.Name = name;
            btn.Text = caption;
            btn.Tag = tag;
            btn.Size = new Size(75, 25);
            btn.Click += ButtonOnClick;
            Controls.Add(btn);
        }

        private void CreateLabel(FieldInfo field)
        {
            var label = new Label();
            label.Location = new Point(startX + 215 * countX, startY + 50 * countY - 15);
            label.Name = "lbl" + countX + countY;
            label.Text = field.Name;
            if (Attribute.IsDefined(field, typeof(NameAttribute)))
                label.Text = (Attribute.GetCustomAttribute(field, typeof(NameAttribute)) as NameAttribute).Name;
            if (!(field.FieldType.IsEnum || field.FieldType.Name == "Boolean" || field.FieldType.Namespace == "Alcohol"))
                label.Text += $" ({field.FieldType.Name})";
            label.Size = new Size(140, 15);
            Controls.Add(label);
        }

        private void ConfigControl(Control ControlObj, FieldInfo field)
        {
            ControlObj.Location = new Point(startX + 215 * countX, startY + 50 * countY);
            ControlObj.Name = field.Name;
            ControlObj.Size = new Size(140, 25);
            Controls.Add(ControlObj);
            ControlList.Add(ControlObj);
        }

        private Control GetControl(FieldInfo field)
        {
            Control ControlObj;
            if (field.FieldType.Name == "Boolean")
                ControlObj = new CheckBox();
            else if (field.FieldType.IsEnum)
            {
                ControlObj = new ComboBox();
                foreach (string item in Enum.GetNames(field.FieldType))
                    (ControlObj as ComboBox).Items.Add(item);
                (ControlObj as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
            }
            else
                ControlObj = new TextBox();
            return ControlObj;
        }

        private void GenerateComponents(FieldInfo[] FieldList)  
        {
            foreach (FieldInfo field in FieldList)
            {
                if (startY + 50 * countY + 30 >= ClientSize.Height)
                {
                    countY = 0;
                    countX++;
                }

                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                    CreateButton(field.Name + " ->", "btn_" + field.Name, field);
                else
                {
                    Control ControlObj = GetControl(field);
                    if (TargetObject != null)
                        SetControlData(ControlObj, field, TargetObject);
                    ConfigControl(ControlObj, field);
                    CreateLabel(field);
                }

                countY++;
            }

            CreateButton(TargetObject != null ? "edit" : "create", TargetObject != null ? "btn_edit" : "btn_create");
            Text = TargetObject != null ? "edit" : "create";
        }

        private void SetControlData(Control ControlObj, FieldInfo field, object TargetObject)
        {
            if (field.FieldType.Name == "String" || field.FieldType.Name == "Double" || field.FieldType.Name == "Int32")
                ControlObj.Text = field.GetValue(TargetObject).ToString();
            else if (field.FieldType.Name == "Boolean")
                (ControlObj as CheckBox).Checked = (bool)field.GetValue(TargetObject);
            else if (field.FieldType.IsEnum)
                (ControlObj as ComboBox).Text = field.GetValue(TargetObject).ToString();
        }

        private void GetControlData(Control obj, FieldInfo field, object entity)
        {
            if (field.FieldType.Name == "String")
            {
                char[] BadSymbols = { '{', '}', ';', ':', '?', };
                if (obj.Text.IndexOfAny(BadSymbols) != -1)
                    throw new FormatException();
                field.SetValue(entity, obj.Text);
            }
            else if (obj.Text != "" && field.FieldType.Name == "Int32")
                field.SetValue(entity, Convert.ToInt32(obj.Text));
            else if (obj.Text != "" && field.FieldType.Name == "Double")
                field.SetValue(entity, Convert.ToDouble(obj.Text, System.Globalization.CultureInfo.InvariantCulture));
            else if (field.FieldType.Name == "Boolean")
                field.SetValue(entity, (obj as CheckBox).Checked);
            else if (obj.Text != "" && field.FieldType.IsEnum)
                field.SetValue(entity, Enum.Parse(field.FieldType, obj.Text));
        }

        private object FillObjectFields(Type ClassType)
        {
            var entity = Activator.CreateInstance(ClassType);
            FieldInfo[] FieldList = ClassType.GetFields();

            foreach (FieldInfo field in FieldList)
            {
                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                {
                    if (AssociationDict.ContainsKey(field))
                        field.SetValue(entity, AssociationDict[field]);
                    continue;
                }

                foreach (Control obj in ControlList)
                {
                    if (obj.Name == field.Name)
                    {
                        try
                        {
                            GetControlData(obj, field, entity);
                            break;
                        }
                        catch (FormatException exp)
                        {
                            MessageBox.Show($"field \"{field.Name}\": invalid value", exp.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return null;
                        }
                    }
                }
            }
            return entity;
        }

        private void AddAssociation(object obj)
        {
            if (!AssociationDict.ContainsKey(TargetField))
                AssociationDict.Add(TargetField, obj);
            else
                AssociationDict[TargetField] = obj;
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn != null && (btn.Name == "btn_create" || btn.Name == "btn_edit"))
            {
                object obj = FillObjectFields(CurrentClassType);
                if (obj != null)
                {
                    ReturnObject(obj);
                    Close();
                }
            }
            else if (btn != null)
            {
                TargetField = btn.Tag as FieldInfo;
                object temp = TargetObject == null ? null : TargetField.GetValue(TargetObject);
                if (AssociationDict.ContainsKey(TargetField))
                    temp = AssociationDict[TargetField];
                var frm = new frmCreateObject((btn.Tag as FieldInfo).FieldType.ToString(), temp, AddAssociation);
                frm.ShowDialog();
            }
        }

    }

}