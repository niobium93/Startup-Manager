using System;
using System.Windows.Forms;
using System.Security.Principal;
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

            userRunKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
            subKeys = userRunKey.GetValueNames();
            foreach (string key in subKeys)
                CreateRow(key, userRunKey.GetValue(key).ToString(), "Current User");

            if (isAdmin)
            {
                if (IntPtr.Size == 8)
                {
                    machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run");
                    subKeys = machineRunKey.GetValueNames();
                    foreach (string key in subKeys)
                        CreateRow(key, machineRunKey.GetValue(key).ToString(), "All Users", "Wow6432");
                    machineRunKey.Close();
                }
                machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
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

        private void CreateRow(string name, string location, string privs, string wow = "")
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(location);
            item.SubItems.Add(privs);
            item.SubItems.Add(wow);
            listView1.Items.Add(item);
        }

        private void IsUserAdministrator()
        {
            try
            {
                //get the currently logged in user
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
            AddForm dialog = new AddForm();
            if (isAdmin)
                dialog.isAdmin = true;
            dialog.ShowDialog();
            if (dialog.done)
            {
                Add(dialog.name, dialog.location, dialog.allUsers);
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {

        }

        private void openLocButton_Click(object sender, EventArgs e)
        {

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
