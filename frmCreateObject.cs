using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

using Alcohol;

namespace AlcoholicDrinks
{ 
    public partial class frmCreateObject : Form
    {
        private List<Control> control_list = new List<Control>();
        private Type current_class_type;
        private Dictionary<FieldInfo, object> association_dict = new Dictionary<FieldInfo, object>();
        private object target_object;
        private FieldInfo target_field;
        private ReturnMethod ReturnObject;
        private int start_y = 25, count_y = 0, start_x = 25, count_x = 0;

        public frmCreateObject(string class_str, object target_obj, ReturnMethod ReturnObj)
        {
            InitializeComponent();

            current_class_type = Type.GetType(class_str, false, true);
            target_object = target_obj;
            ReturnObject = ReturnObj;
            GenerateComponents(current_class_type.GetFields());
        }

        private void CreateButton(string caption, string name, object tag = null)
        {
            var btn = new Button();
            btn.Location = new Point(start_x + 215 * count_x, start_y + 50 * count_y);
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
            label.Location = new Point(start_x + 215 * count_x, start_y + 50 * count_y - 15);
            label.Name = "lbl" + count_x + count_y;
            label.Text = field.Name;
            if (Attribute.IsDefined(field, typeof(NameAttribute)))
                label.Text = (Attribute.GetCustomAttribute(field, typeof(NameAttribute)) as NameAttribute).Name;
            if (!(field.FieldType.IsEnum || field.FieldType.Name == "Boolean" || field.FieldType.Namespace == "Alcohol"))
                label.Text += $" ({field.FieldType.Name})";
            label.Size = new Size(140, 15);
            Controls.Add(label);
        }

        private void ConfigControl(Control control_obj, FieldInfo field)
        {
            control_obj.Location = new Point(start_x + 215 * count_x, start_y + 50 * count_y);
            control_obj.Name = field.Name;
            control_obj.Size = new Size(140, 25);
            Controls.Add(control_obj);
            control_list.Add(control_obj);
        }

        private Control GetControl(FieldInfo field)
        {
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
            return control_obj;
        }

        private void GenerateComponents(FieldInfo[] field_list)  
        {
            foreach (FieldInfo field in field_list)
            {
                if (start_y + 50 * count_y + 30 >= ClientSize.Height)
                {
                    count_y = 0;
                    count_x++;
                }

                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                    CreateButton(field.Name + " ->", "btn_" + field.Name, field);
                else
                {
                    Control control_obj = GetControl(field);
                    if (target_object != null)
                        SetControlData(control_obj, field, target_object);
                    ConfigControl(control_obj, field);
                    CreateLabel(field);
                }

                count_y++;
            }

            CreateButton(target_object != null ? "edit" : "create", target_object != null ? "btn_edit" : "btn_create");
            Text = target_object != null ? "edit" : "create";
        }

        private void SetControlData(Control control_obj, FieldInfo field, object target_object)
        {
            if (field.FieldType.Name == "String" || field.FieldType.Name == "Double" || field.FieldType.Name == "Int32")
                control_obj.Text = field.GetValue(target_object).ToString();
            else if (field.FieldType.Name == "Boolean")
                (control_obj as CheckBox).Checked = (bool)field.GetValue(target_object);
            else if (field.FieldType.IsEnum)
                (control_obj as ComboBox).Text = field.GetValue(target_object).ToString();
        }

        private void GetControlData(Control obj, FieldInfo field, object entity)
        {
            if (field.FieldType.Name == "String")
            {
                char[] bad_symbols = { '{', '}', ';', ':', '?', };
                if (obj.Text.IndexOfAny(bad_symbols) != -1)
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

        private object FillObjectFields(Type class_type)
        {
            var entity = Activator.CreateInstance(class_type);
            FieldInfo[] field_list = class_type.GetFields();

            foreach (FieldInfo field in field_list)
            {
                if (field.FieldType.Namespace == "Alcohol" && !field.FieldType.IsEnum)
                {
                    if (association_dict.ContainsKey(field))
                        field.SetValue(entity, association_dict[field]);
                    continue;
                }

                foreach (Control obj in control_list)
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
            if (!association_dict.ContainsKey(target_field))
                association_dict.Add(target_field, obj);
            else
                association_dict[target_field] = obj;
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn != null && (btn.Name == "btn_create" || btn.Name == "btn_edit"))
            {
                object obj = FillObjectFields(current_class_type);
                if (obj != null)
                {
                    ReturnObject(obj);
                    Close();
                }
            }
            else if (btn != null)
            {
                target_field = btn.Tag as FieldInfo;
                object temp = target_object == null ? null : target_field.GetValue(target_object);
                if (association_dict.ContainsKey(target_field))
                    temp = association_dict[target_field];
                var frm = new frmCreateObject((btn.Tag as FieldInfo).FieldType.ToString(), temp, AddAssociation);
                frm.ShowDialog();
            }
        }

    }

}