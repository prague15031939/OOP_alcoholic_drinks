using System;
using System.Collections.Generic;
using System.Windows.Forms;

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

            openDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin|Json files (*.json)|*.json";
            saveDialog.Filter = "Text files (*.txt)|*.txt|Binary files (*.bin)|*.bin|Json files (*.json)|*.json";
            saveDialog.AddExtension = true;
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

        private void OpenFile()
        {
            if (openDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string file_name = openDialog.FileName;
            int creator_index = openDialog.FilterIndex - 1;
            var serializator = creator_list[creator_index].Create(file_name);
            object_list = serializator.Deserealize();
            lvMain.Items.Clear();
            foreach (object entity in object_list)
                lvMain_AddObject(entity);
        }

        private void SaveFile()
        {
            if (saveDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string file_name = saveDialog.FileName;
            int creator_index = saveDialog.FilterIndex - 1;
            var serializator = creator_list[creator_index].Create(file_name);
            serializator.Serialize(object_list);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
