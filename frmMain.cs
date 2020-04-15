using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

using PluginInterface;

namespace AlcoholicDrinks
{
    public partial class frmMain : Form
    {
        private List<Creator> CreatorList = new List<Creator> { new TextCreator(), new BinaryCreator(), new JsonCreator() };
        private List<object> ObjectList = new List<object>();
        private int TargetListIndex;

        public void AddAlcoholObject(object obj)
        {
            if (TargetListIndex == -1)
            {
                ObjectList.Add(obj);
                lvMain_AddObject(obj);
            }
            else
            {
                ObjectList[TargetListIndex] = obj;
                lvMain.Items.Clear();
                foreach (object entity in ObjectList)
                    lvMain_AddObject(entity);
            }
        }

        private void lvMain_AddObject(object obj)
        {
            var item = new ListViewItem();
            var ClassType = obj.GetType();
            item.Text = (string)ClassType.GetField("title").GetValue(obj);
            item.SubItems.Add((string)ClassType.GetField("manufacturer").GetValue(obj));
            item.SubItems.Add(((double)ClassType.GetField("degree").GetValue(obj)).ToString());
            item.SubItems.Add(((double)ClassType.GetField("container_volume").GetValue(obj)).ToString());
            lvMain.Items.Add(item);
        }

        public frmMain()
        {
            InitializeComponent();
        }

        private void DeleteObject()
        {
            var TempListObject = new List<object>();
            for (int i = 0; i < ObjectList.Count; i++)
                if (!lvMain.SelectedIndices.Contains(i))
                    TempListObject.Add(ObjectList[i]);

            lvMain.Items.Clear();
            ObjectList.Clear();
            foreach (object obj in TempListObject)
                lvMain_AddObject(obj);        
        }

        private void EditObject() {
            TargetListIndex = lvMain.SelectedIndices[0];
            string ClassStr = ObjectList[TargetListIndex].GetType().FullName;
            var frm = new frmCreateObject(ClassStr, ObjectList[TargetListIndex], AddAlcoholObject);
            frm.ShowDialog();
        }

        private void CreateObject()
        {
            TargetListIndex = -1;
            var frm = new frmCreateObject($"Alcohol.{cbClasses.Text}", null, AddAlcoholObject);
            frm.ShowDialog();
        }

        private void ViewObject()
        {
            var frm = new frmReport(ObjectList[lvMain.SelectedIndices[0]]);
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

        private void RefreshFileOpenInfo(List<IPlugin> PluginList)
        {
            openDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin|Json files (*.json)|*.json";
            foreach (IPlugin plugin in PluginList)
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

        private IPlugin GetOpenPlugin(List<IPlugin> PluginList, string FileName)
        {
            foreach (IPlugin plugin in PluginList)
            {
                if (Attribute.IsDefined(plugin.GetType(), typeof(NameAttribute)))
                {
                    string PluginExtension = (Attribute.GetCustomAttribute(plugin.GetType(), typeof(NameAttribute)) as NameAttribute).PluginExtension;
                    var FileInfo = new FileInfo(FileName);
                    if (FileInfo.Extension.Contains(PluginExtension))
                        return plugin;
                }
            }
            return null;
        }

        private void OpenFile()
        {
            var loadObj = new PluginLoader();
            List<IPlugin> PluginList = loadObj.RefreshPlugins();
            RefreshFileOpenInfo(PluginList);

            if (openDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string FileName = openDialog.FileName;
            int CreatorIndex = (openDialog.FilterIndex - 1) % CreatorList.Count;
            var serializator = CreatorList[CreatorIndex].Create(FileName);

            IPlugin plugin = GetOpenPlugin(PluginList, FileName);
            ObjectList = serializator.Deserealize(plugin);

            lvMain.Items.Clear();
            foreach (object entity in ObjectList)
                lvMain_AddObject(entity);
        }

        private void SaveFile(IPlugin plugin)
        {
            RefreshFileSaveInfo(plugin);
            if (saveDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string FileName = saveDialog.FileName;
            int CreatorIndex = saveDialog.FilterIndex - 1;
            var serializator = CreatorList[CreatorIndex].Create(FileName);
            serializator.Serialize(ObjectList, plugin);
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
