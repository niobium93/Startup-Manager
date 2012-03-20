using System;
using System.Windows.Forms;
using System.Security.Principal;
using System.IO;
using Microsoft.Win32;

namespace Startup_Manager
{
    public partial class Form1 : Form
    {
        private RegistryKey userRunKey;
        private RegistryKey machineRunKey;
        private bool isAdmin;
        public Form1()
        {
            InitializeComponent();
            IsUserAdministrator();
            string[] subKeys;

            userRunKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            subKeys = userRunKey.GetValueNames();
            foreach (string key in subKeys)
                CreateRow(key, userRunKey.GetValue(key).ToString(), "Current User");

            if (isAdmin)
            {
                if (IntPtr.Size == 8)
                {
                    machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    subKeys = machineRunKey.GetValueNames();
                    foreach (string key in subKeys)
                        CreateRow(key, machineRunKey.GetValue(key).ToString(), "All Users", true);
                    machineRunKey.Close();
                }
                machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                subKeys = machineRunKey.GetValueNames();
                foreach (string key in subKeys)
                    CreateRow(key, machineRunKey.GetValue(key).ToString(), "All Users");
            }

            addButton.Enabled = true;
            removeButton.Enabled = true;
            openLocButton.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            userRunKey.Close();
            if (isAdmin) machineRunKey.Close();
        }

        private void CreateRow(string name, string location, string privs, bool wow = false)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(location);
            item.SubItems.Add(privs);
            if (wow) item.SubItems.Add("Wow6432");

            ItemTag myItem;
            myItem.name = name;
            myItem.filePath = location;
            myItem.privs = privs;
            myItem.wow = wow;
            item.Tag = myItem;

            listView1.Items.Add(item);
        }

        private void IsUserAdministrator()
        {
            try
            {
                // Get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                isAdmin = false;
            }
            catch (Exception)
            {
                isAdmin = false;
            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddForm dialog = new AddForm(isAdmin);
            dialog.ShowDialog();
            if (dialog.done)
            {
                Add(dialog.name, dialog.location, dialog.allUsers);
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                ItemTag myItem = (ItemTag)item.Tag;
                if (myItem.privs == "Current User")
                {
                    userRunKey.DeleteValue(myItem.name, false);
                    listView1.Items.Remove(item);
                }
                else if (myItem.privs == "All Users")
                {
                    machineRunKey.DeleteValue(myItem.name, false);
                    listView1.Items.Remove(item);
                }
            }
        }

        private void openLocButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                ItemTag itemTag = (ItemTag)item.Tag;
                Location location = new Location(itemTag.filePath);

                if (Path.GetDirectoryName(location.GetLocation()) == "")
                    MessageBox.Show("Relative file paths not yet implemented.");
                else
                    System.Diagnostics.Process.Start(@Path.GetDirectoryName(location.GetLocation()));
            }
        }
        private void Add(string name, string location, bool allUsers)
        {
            if (allUsers)
            {
                machineRunKey.SetValue(name, location);
                CreateRow(name, location, "All Users");
            }
            else
            {
                userRunKey.SetValue(name, location);
                CreateRow(name, location, "Current User");
            }
        }
    }
}
