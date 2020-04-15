using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms;

using PluginInterface;

namespace AlcoholicDrinks
{
    public delegate void PluginReturnMethod(IPlugin plugin);

    public partial class frmPlugins : Form
    {
        private int PickedPlugin = -1;
        private List<IPlugin> PluginList = new List<IPlugin>();
        private PluginReturnMethod ReturnPlugin;

        public frmPlugins(PluginReturnMethod RetPlugin)
        {
            InitializeComponent();

            ReturnPlugin = RetPlugin;
            var loadObj = new PluginLoader();
            PluginList = loadObj.RefreshPlugins();
            PluginList.Add(null);
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
            int startY = 10, countY = 0;
            foreach (IPlugin plugin in PluginList)
            {
                CreateRadioButton(45, startY + countY * 30, GetPluginCaption(plugin), countY);
                countY++;
            }
            CreateButton(45, startY + countY++ * 30 + 10);
            Height = startY + countY * 30 + 55;
            Width = 180;
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            if (btn != null && PickedPlugin != -1)
            {
                ReturnPlugin(PluginList[PickedPlugin]);
                Close();
            }
        }

        private void RadioButtonOnCheckChanged(object sender, EventArgs e)
        {
            var rbtn = (RadioButton)sender;
            if (rbtn.Checked) 
                PickedPlugin = (int)rbtn.Tag;
        }
    }
}
