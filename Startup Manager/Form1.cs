using System;
using System.Windows.Forms;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;
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
            LoadEntries();
        }

        private void LoadEntries()
        {
            //Disable buttons before loading entries.
            addButton.Enabled = false;
            removeButton.Enabled = false;
            openLocButton.Enabled = false;
            refreshButton.Enabled = false;
            elevateButton.Enabled = false;

            string[] subKeys;

            userRunKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            subKeys = userRunKey.GetValueNames();
            foreach (string key in subKeys)
                CreateRow(key, userRunKey.GetValue(key).ToString(), "Current User");

            if (isAdmin)
            {
                if (IntPtr.Size == 8) //If running on x64 also load Wow6432 entries.
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
            else
            {
                ListViewItem item = new ListViewItem("!!!");
                item.SubItems.Add("To see programs that start for all users elevate to admin access.");
                listView.Items.Add(item);
            }

            //Enable buttons after loading entries.
            addButton.Enabled = true;
            removeButton.Enabled = true;
            openLocButton.Enabled = true;
            refreshButton.Enabled = true;
            if (!isAdmin) elevateButton.Enabled = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            userRunKey.Close();
            if (isAdmin) machineRunKey.Close();
        }

        private void CreateRow(string name, string location, string privs, bool wow = false, bool select = false)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(location);
            item.SubItems.Add(privs);
            if (wow) item.SubItems.Add("Wow6432");

            ItemTag tag;
            tag.name = name;
            tag.filePath = location;
            tag.privs = privs;
            tag.wow = wow;
            item.Tag = tag;

            if (select)
            {
                listView.SelectedItems.Clear();
                item.Selected = true;
            }

            listView.Items.Add(item);
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
            foreach (ListViewItem item in listView.SelectedItems)
            {
                ItemTag myItem = (ItemTag)item.Tag;
                if (myItem.privs == "Current User")
                {
                    userRunKey.DeleteValue(myItem.name, false);
                    listView.Items.Remove(item);
                }
                else if (myItem.privs == "All Users")
                {
                    machineRunKey.DeleteValue(myItem.name, false);
                    listView.Items.Remove(item);
                }
            }
        }

        private void openLocButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                ItemTag itemTag = (ItemTag)item.Tag;
                LocationHandler location = new LocationHandler(itemTag.filePath);
                System.Diagnostics.Process.Start(@"explorer.exe", @"/select," + @location.Location);
            }
        }
        private void Add(string name, string location, bool allUsers)
        {
            if (allUsers)
            {
                machineRunKey.SetValue(name, location);
                CreateRow(name, location, "All Users", false, true);
            }
            else
            {
                userRunKey.SetValue(name, location);
                CreateRow(name, location, "Current User", false, true);
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            listView.Items.Clear();
            LoadEntries();
        }

        private void elevateButton_Click(object sender, EventArgs e) // Restarts current application with admin rights.
        {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = true;
            proc.WorkingDirectory = Environment.CurrentDirectory;
            proc.FileName = Application.ExecutablePath;
            proc.Verb = "runas"; // Makes the application start with admin rights.

            try
            {
                Process.Start(proc);
            }
            catch
            {
                // The user refused the elevation.
                // Do nothing and return directly.
                return;
            }
            Application.Exit();  // Quit itself
        }
    }
}
