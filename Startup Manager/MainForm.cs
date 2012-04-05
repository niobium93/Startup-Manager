using System;
using System.Windows.Forms;
using System.Security.Principal;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace Startup_Manager
{
    public partial class MainForm : Form
    {
        private RegistryKey UserRunKey;
        private RegistryKey MachineRunKey;
        private bool isAdmin;
        public MainForm()
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

            UserRunKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            subKeys = UserRunKey.GetValueNames();
            foreach (string key in subKeys)
                CreateRow(key, UserRunKey.GetValue(key).ToString());

            if (isAdmin)
            {
                if (IntPtr.Size == 8) //If running on x64 also load Wow6432 entries.
                {
                    MachineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                    subKeys = MachineRunKey.GetValueNames();
                    foreach (string key in subKeys)
                        CreateRow(key, MachineRunKey.GetValue(key).ToString(), true, true);
                    MachineRunKey.Close();
                }
                MachineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                subKeys = MachineRunKey.GetValueNames();
                foreach (string key in subKeys)
                    CreateRow(key, MachineRunKey.GetValue(key).ToString(), true);
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
            UserRunKey.Close();
            if (isAdmin) MachineRunKey.Close();
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
            tag.Name = name;
            tag.FilePath = location;
            tag.AdminRights = adminRights;
            tag.Wow = wow;
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
            EditForm dialog = new EditForm(isAdmin);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.AdminRights)
                    MachineRunKey.SetValue(dialog.ItemName, dialog.ItemLocation);
                else
                    UserRunKey.SetValue(dialog.ItemName, dialog.ItemLocation);

                CreateRow(dialog.ItemName, dialog.ItemLocation, dialog.AdminRights, false, true);
            }
        }

        private void editButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                ItemTag tag = (ItemTag)item.Tag;
                if (tag.Wow)
                    break;
                EditForm dialog = new EditForm(isAdmin, tag.Name, tag.FilePath, tag.AdminRights);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    item.SubItems[0].Text = dialog.ItemName;
                    item.SubItems[1].Text = dialog.ItemLocation;
                    if (dialog.AdminRights)
                    {
                        MachineRunKey.DeleteValue(tag.Name, false);
                        MachineRunKey.SetValue(dialog.ItemName, dialog.ItemLocation);
                        item.SubItems[2].Text = "All Users";
                    }
                    else
                    {
                        UserRunKey.DeleteValue(tag.Name, false);
                        UserRunKey.SetValue(dialog.ItemName, dialog.ItemLocation);
                        item.SubItems[2].Text = "Current User";
                    }
                    
                    //if (wow) item.SubItems[3].Text = "Wow6432";

                    tag.Name = dialog.ItemName;
                    tag.FilePath = dialog.ItemLocation;
                    tag.AdminRights = dialog.AdminRights;
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

                if (tag.AdminRights)
                    MachineRunKey.DeleteValue(tag.Name, false);
                else
                    UserRunKey.DeleteValue(tag.Name, false);

                listView.Items.Remove(item);
            }
        }

        private void openLocButton_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView.SelectedItems)
            {
                ItemTag tag = (ItemTag)item.Tag;
                LocationHandler location = new LocationHandler(tag.FilePath);
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
