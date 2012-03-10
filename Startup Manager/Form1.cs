using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Startup_Manager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            RegistryKey userRunKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");

            string[] subKeys = userRunKey.GetValueNames();
            foreach (string key in subKeys)
            {
                CreateRow(key, userRunKey.GetValue(key).ToString());
            }

            userRunKey.Close();
        }
        private void CreateRow(string name, string location)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(location);
            listView1.Items.Add(item);
        }
    }
}
