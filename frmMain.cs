﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Alcohol;

namespace lab1
{
    public partial class frmMain : Form
    {
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
            item.Text = (string)Type.GetType(cbClasses.Text, false, true).GetField("title").GetValue(obj);
            item.SubItems.Add((string)Type.GetType(cbClasses.Text, false, true).GetField("manufacturer").GetValue(obj));
            item.SubItems.Add(((double)Type.GetType(cbClasses.Text, false, true).GetField("degree").GetValue(obj)).ToString());
            item.SubItems.Add(((double)Type.GetType(cbClasses.Text, false, true).GetField("container_volume").GetValue(obj)).ToString());
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
            var frm = new frmCreateObject(cbClasses.Text, null, AddAlcoholObject);
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
        }

    }
}
