﻿using System;
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
                CreateRow(key, userRunKey.GetValue(key).ToString());

            if (isAdmin)
            {
                machineRunKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run");
                subKeys = machineRunKey.GetValueNames();
                foreach (string key in subKeys)
                    CreateRow(key, machineRunKey.GetValue(key).ToString());
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            userRunKey.Close();
            if (isAdmin) machineRunKey.Close();
        }
        private void CreateRow(string name, string location)
        {
            ListViewItem item = new ListViewItem(name);
            item.SubItems.Add(location);
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
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
        }
    }
}