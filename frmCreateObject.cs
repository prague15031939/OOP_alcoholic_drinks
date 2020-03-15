using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

using Alcohol;

namespace lab1
{ 
    public partial class frmCreateObject : Form
    {
        private List<Control> control_list = new List<Control>();
        private Type current_class_type;
        private int target_list_index;
        private AddAlcoholObject AddObject;
        private int start_y = 25, count_y = 0, start_x = 25, count_x = 0;

        public frmCreateObject(string class_str, object target_object, int target_index, AddAlcoholObject AddObj)
        {
            InitializeComponent();

            current_class_type = Type.GetType(class_str, false, true);
            target_list_index = target_index;
            AddObject = AddObj;
            if (target_object == null)
                GenerateComponents(current_class_type.GetFields());
            else
                GenerateComponents(current_class_type.GetFields(), target_object);

            CreateButton(target_object != null ? "edit" : "create");
        }

        public void CreateButton(string text)
        {
            var btn = new Button();
            btn.Location = new Point(start_x + 215 * count_x, start_y + 50 * count_y);
            btn.Name = "btn0";
            btn.Text = text;
            btn.Size = new Size(75, 25);
            btn.Click += ButtonOnClick;
            Controls.Add(btn);
            Text = text;
        }

        public void CreateLabel(FieldInfo field)
        {
            var label = new Label();
            label.Location = new Point(start_x + 215 * count_x, start_y + 50 * count_y - 15);
            label.Name = "lbl" + count_x + count_y;
            label.Text = field.Name;
            if (Attribute.IsDefined(field, typeof(NameAttribute)))
                label.Text = (Attribute.GetCustomAttribute(field, typeof(NameAttribute)) as NameAttribute).Name;
            if (!(field.FieldType.IsEnum || field.FieldType.Name == "Boolean"))
                label.Text += $" ({field.FieldType.Name})";
            label.Size = new Size(140, 15);
            Controls.Add(label);
        }

        public void ConfigControl(Control control_obj, FieldInfo field)
        {
            control_obj.Location = new Point(start_x + 215 * count_x, start_y + 50 * count_y);
            control_obj.Name = field.Name;
            control_obj.Size = new Size(140, 25);
            Controls.Add(control_obj);
            control_list.Add(control_obj);
        }

        private void GenerateComponents(FieldInfo[] field_list, object target_object = null)   
        {
            foreach (FieldInfo field in field_list)
            {
                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                {
                    object sub_object = null;
                    if (target_object != null)            
                        sub_object = field.GetValue(target_object);         
                    GenerateComponents(field.FieldType.GetFields(), sub_object); 
                    continue;
                }

                Control control_obj;
                if (field.FieldType.Name == "Boolean")
                    control_obj = new CheckBox();
                else if (field.FieldType.IsEnum) 
                {
                    control_obj = new ComboBox();
                    foreach (string item in Enum.GetNames(field.FieldType))
                        (control_obj as ComboBox).Items.Add(item);
                    (control_obj as ComboBox).DropDownStyle = ComboBoxStyle.DropDownList;
                }
                else
                    control_obj = new TextBox();

                if (target_object != null)
                {
                    if (field.FieldType.Name == "String" || field.FieldType.Name == "Double" || field.FieldType.Name == "Int32")
                        control_obj.Text = field.GetValue(target_object).ToString();
                    else if (field.FieldType.Name == "Boolean")
                        (control_obj as CheckBox).Checked = (bool)field.GetValue(target_object);
                    else if (field.FieldType.IsEnum)
                        (control_obj as ComboBox).Text = field.GetValue(target_object).ToString();
                }

                if (start_y + 50 * count_y + 30 >= ClientSize.Height)
                {
                    count_y = 0;
                    count_x++;
                }

                ConfigControl(control_obj, field);
                CreateLabel(field);
                count_y++;
            }
        }

        private object FillObjectFields(Type class_type)
        {
            var entity = Activator.CreateInstance(class_type);
            FieldInfo[] field_list = class_type.GetFields();

            foreach (FieldInfo field in field_list)
            {
                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                {
                    object sub_entity = FillObjectFields(field.FieldType);
                    if (sub_entity == null)
                        return null;
                    field.SetValue(entity, sub_entity);
                    continue;
                }

                foreach (Control obj in control_list)
                {
                    if (obj.Name == field.Name)
                    {
                        try
                        {
                            if (field.FieldType.Name == "String")
                                field.SetValue(entity, obj.Text);
                            else if (obj.Text != "" && field.FieldType.Name == "Int32")
                                field.SetValue(entity, Convert.ToInt32(obj.Text));
                            else if (obj.Text != "" && field.FieldType.Name == "Double")
                                field.SetValue(entity, Convert.ToDouble(obj.Text, System.Globalization.CultureInfo.InvariantCulture));
                            else if (field.FieldType.Name == "Boolean")
                                field.SetValue(entity, (obj as CheckBox).Checked);
                            else if (obj.Text != "" && field.FieldType.IsEnum)
                                field.SetValue(entity, Enum.Parse(field.FieldType, obj.Text));
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

        private void ButtonOnClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn != null)
            {
                object obj = FillObjectFields(current_class_type);
                if (obj != null)
                {
                    AddObject(obj, target_list_index);
                    Close();
                }
            }
        }

    }

}