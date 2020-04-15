using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

using PluginInterface;

namespace AlcoholicDrinks
{
    public delegate void PluginMethod(IPlugin plugin);

    public partial class frmPlugins : Form
    {
        private int picked_plugin = -1;
        private List<IPlugin> plugin_list = new List<IPlugin>();
        private PluginMethod ReturnPlugin;

        public frmPlugins(PluginMethod RetPlugin)
        {
            InitializeComponent();

            ReturnPlugin = RetPlugin;
            var load_obj = new PluginLoader();
            plugin_list = load_obj.RefreshPlugins();
            plugin_list.Add(null);
            GenerateComponents();
        }

        private void CreateButton(int x, int y)
        {
            var btn = new Button();
            btn.Location = new Point(x, y);
            btn.Name = "btnAccept";
            btn.Text = "ok";
            btn.Size = new Size(75, 25);
            btn.Click += ButtonOnClick;
            Controls.Add(btn);
        }

        private void CreateRadioButton(int x, int y, string text, int num)
        {
            var rbtn = new RadioButton();
            rbtn.Location = new Point(x, y);
            rbtn.Name = $"rbtn{num}";
            rbtn.Text = text;
            rbtn.Tag = num;
            rbtn.Size = new Size(100, 25);
            rbtn.CheckedChanged += RadioButtonOnCheckChanged;
            Controls.Add(rbtn);
        }

        private string GetPluginCaption(IPlugin plugin)
        {
            if (plugin == null)
                return "plain text";
            if (Attribute.IsDefined(plugin.GetType(), typeof(NameAttribute)))
                return (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginName;
            return null;
        }

        private void GenerateComponents()
        {
            int start_y = 10, count_y = 0;
            foreach (IPlugin plugin in plugin_list)
            {
                CreateRadioButton(45, start_y + count_y * 30, GetPluginCaption(plugin), count_y);
                count_y++;
            }
            CreateButton(45, start_y + count_y++ * 30 + 10);
            Height = start_y + count_y * 30 + 55;
            Width = 180;
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn != null && picked_plugin != -1)
            {
                ReturnPlugin(plugin_list[picked_plugin]);
                Close();
            }
        }

        private void RadioButtonOnCheckChanged(object sender, EventArgs e)
        {
            var rbtn = (RadioButton)sender;
            if (rbtn.Checked) 
                picked_plugin = (int)rbtn.Tag;
        }
    }
}
