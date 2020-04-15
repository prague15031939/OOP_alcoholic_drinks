using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

using PluginInterface;

namespace AlcoholicDrinks
{
    public partial class frmMain : Form
    {
        private List<Creator> creator_list = new List<Creator> { new TextCreator(), new BinaryCreator(), new JsonCreator() };
        private List<object> object_list = new List<object>();
        private int target_list_index;

        public void AddAlcoholObject(object obj)
        {
            if (target_list_index == -1)
            {
                object_list.Add(obj);
                lvMain_AddObject(obj);
            }
            else
            {
                object_list[target_list_index] = obj;
                lvMain.Items.Clear();
                foreach (object entity in object_list)
                    lvMain_AddObject(entity);
            }
        }

        private void lvMain_AddObject(object obj)
        {
            var item = new ListViewItem();
            var class_type = obj.GetType();
            item.Text = (string)class_type.GetField("title").GetValue(obj);
            item.SubItems.Add((string)class_type.GetField("manufacturer").GetValue(obj));
            item.SubItems.Add(((double)class_type.GetField("degree").GetValue(obj)).ToString());
            item.SubItems.Add(((double)class_type.GetField("container_volume").GetValue(obj)).ToString());
            lvMain.Items.Add(item);
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void DeleteObject()
        {
            var temp_object_list = new List<object>();
            for (int i = 0; i < object_list.Count; i++)
                if (!lvMain.SelectedIndices.Contains(i))
                    temp_object_list.Add(object_list[i]);

            lvMain.Items.Clear();
            object_list.Clear();
            foreach (object obj in temp_object_list)
                lvMain_AddObject(obj);        
        }

        private void EditObject() {
            target_list_index = lvMain.SelectedIndices[0];
            string class_str = object_list[target_list_index].GetType().FullName;
            var frm = new frmCreateObject(class_str, object_list[target_list_index], AddAlcoholObject);
            frm.ShowDialog();
        }

        private void CreateObject()
        {
            target_list_index = -1;
            var frm = new frmCreateObject($"Alcohol.{cbClasses.Text}", null, AddAlcoholObject);
            frm.ShowDialog();
        }

        private void ViewObject()
        {
            var frm = new frmReport(object_list[lvMain.SelectedIndices[0]]);
            frm.ShowDialog();
        }

        private void lvMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lvMain.SelectedItems.Count != 0)
                DeleteObject();
            if (e.KeyCode == Keys.E && lvMain.SelectedItems.Count == 1)
                EditObject();
            if (e.KeyCode == Keys.Q && cbClasses.Text != "")
                CreateObject();
            if (e.KeyCode == Keys.W && lvMain.SelectedItems.Count == 1)
                ViewObject();
        }

        private void addObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cbClasses.Text != "")
                CreateObject();
        }

        private void editObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count == 1)
                EditObject();
        }

        private void viewObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count == 1)
                ViewObject();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lvMain.SelectedItems.Count != 0)
                DeleteObject();
        }

        private void RefreshFileOpenInfo(List<IPlugin> plugin_list)
        {
            openDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin|Json files (*.json)|*.json";
            foreach (IPlugin plugin in plugin_list)
            {
                if (Attribute.IsDefined(plugin.GetType(), typeof(NameAttribute)))
                {
                    string PluginExtension = (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginExtension;
                    string PluginName = (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginName;
                    openDialog.Filter += $"|Compressed {PluginName} files (*.txt{PluginExtension})|*.txt{PluginExtension}";
                    openDialog.Filter += $"|Compressed {PluginName} files (*.bin{PluginExtension})|*.bin{PluginExtension}";
                    openDialog.Filter += $"|Compressed {PluginName} files (*.json{PluginExtension})|*.json{PluginExtension}";
                }
            }
        }

        private void RefreshFileSaveInfo(IPlugin plugin)
        {
            saveDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin|Json files (*.json)|*.json";
            saveDialog.AddExtension = true;
            if (plugin != null && Attribute.IsDefined(plugin.GetType(), typeof(NameAttribute)))
            {
                string PluginExtension = (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginExtension;
                string PluginName = (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginName;
                saveDialog.Filter = $"Compressed {PluginName} files (*.txt{PluginExtension})|*.txt{PluginExtension}";
                saveDialog.Filter += $"|Compressed {PluginName} files (*.bin{PluginExtension})|*.bin{PluginExtension}";
                saveDialog.Filter += $"|Compressed {PluginName} files (*.json{PluginExtension})|*.json{PluginExtension}";
            }
        }

        private IPlugin GetOpenPlugin(List<IPlugin> plugin_list, string file_name)
        {
            foreach (IPlugin plugin in plugin_list)
            {
                if (Attribute.IsDefined(plugin.GetType(), typeof(NameAttribute)))
                {
                    string PluginExtension = (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginExtension;
                    var file_info = new FileInfo(file_name);
                    if (file_info.Extension.Contains(PluginExtension))
                        return plugin;
                }
            }
            return null;
        }

        private void OpenFile()
        {
            var load_obj = new PluginLoader();
            List<IPlugin> plugin_list = load_obj.RefreshPlugins();
            RefreshFileOpenInfo(plugin_list);

            if (openDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string file_name = openDialog.FileName;
            int creator_index = (openDialog.FilterIndex - 1) % creator_list.Count;
            var serializator = creator_list[creator_index].Create(file_name);

            IPlugin plugin = GetOpenPlugin(plugin_list, file_name);
            object_list = serializator.Deserealize(plugin);

            lvMain.Items.Clear();
            foreach (object entity in object_list)
                lvMain_AddObject(entity);
        }

        private void SaveFile(IPlugin plugin)
        {
            RefreshFileSaveInfo(plugin);
            if (saveDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string file_name = saveDialog.FileName;
            int creator_index = saveDialog.FilterIndex - 1;
            var serializator = creator_list[creator_index].Create(file_name);
            serializator.Serialize(object_list, plugin);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var frm = new frmPlugins(SaveFile);
            frm.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
