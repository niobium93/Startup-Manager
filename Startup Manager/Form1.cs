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
            if (!isAdmin)
                elevateLabel.Visible = true;
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
                CreateRow(key, userRunKey.GetValue(key).ToString());

            if (isAdmin)
            {
                if (IntPtr.Size == 8) //If running on x64 also load Wow6432 entries.
                {
                    machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    subKeys = machineRunKey.GetValueNames();
                    foreach (string key in subKeys)
                        CreateRow(key, machineRunKey.GetValue(key).ToString(), true, true);
                    machineRunKey.Close();
                }
                machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                subKeys = machineRunKey.GetValueNames();
                foreach (string key in subKeys)
                    CreateRow(key, machineRunKey.GetValue(key).ToString(), true);
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

        private void CreateRow(string name, string location, bool adminRights = false, bool wow = false, bool select = false)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(location);
            if(adminRights)
                item.SubItems.Add("All Users");
            else
                item.SubItems.Add("Current User");
            if (wow) item.SubItems.Add("Wow6432");

            ItemTag tag;
            tag.name = name;
            tag.filePath = location;
            tag.adminRights = adminRights;
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
                if (dialog.m_adminRights)
                    machineRunKey.SetValue(dialog.m_name, dialog.m_location);
                else
                    userRunKey.SetValue(dialog.m_name, dialog.m_location);

                CreateRow(dialog.m_name, dialog.m_location, dialog.m_adminRights, false, true);
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                ItemTag tag = (ItemTag)item.Tag;
                if (tag.wow)
                    break;
                AddForm dialog = new AddForm(isAdmin, tag.name, tag.filePath, tag.adminRights);
                dialog.ShowDialog();
                if (dialog.done)
                {
                    item.SubItems[0].Text = dialog.m_name;
                    item.SubItems[1].Text = dialog.m_location;
                    if (dialog.m_adminRights)
                    {
                        machineRunKey.DeleteValue(tag.name, false);
                        machineRunKey.SetValue(dialog.m_name, dialog.m_location);
                        item.SubItems[2].Text = "All Users";
                    }
                    else
                    {
                        userRunKey.DeleteValue(tag.name, false);
                        userRunKey.SetValue(dialog.m_name, dialog.m_location);
                        item.SubItems[2].Text = "Current User";
                    }
                    
                    //if (wow) item.SubItems[3].Text = "Wow6432";

                    tag.name = dialog.m_name;
                    tag.filePath = dialog.m_location;
                    tag.adminRights = dialog.m_adminRights;
                    //tag.wow = wow;

                    item.Tag = tag;
                    item.Selected = true;
                }
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                ItemTag tag = (ItemTag)item.Tag;

                if (tag.adminRights)
                    machineRunKey.DeleteValue(tag.name, false);
                else
                    userRunKey.DeleteValue(tag.name, false);

                listView.Items.Remove(item);
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
            this.Close();  // Quit itself
        }
    }
}
