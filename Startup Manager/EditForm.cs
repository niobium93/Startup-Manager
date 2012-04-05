using System;
using System.Windows.Forms;
using System.IO;

namespace Startup_Manager
{
    public partial class EditForm : Form
    {
        public string ItemName = "";
        public string ItemLocation = "";
        public bool AdminRights = false;
        public EditForm(bool isAdmin)
        {
            InitializeComponent();
            if (!isAdmin)
                adminRadioButton.Enabled = false;
        }

        public EditForm(bool isAdmin, string name, string location, bool adminRights)
        {
            InitializeComponent();
            if (!isAdmin)
                adminRadioButton.Enabled = false;

            nameBox.Text = name;
            locationBox.Text = location.Split('"')[1];

            if(adminRights)
            {
                adminRadioButton.Checked = true;
                userRadioButton.Checked = false;
            }
            else
            {
                adminRadioButton.Checked = false;
                userRadioButton.Checked = true;
            }

            this.Text = "Edit startup item...";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (nameBox.Text == "")
            {
                MessageBox.Show("You must enter a name.");
            }
            else if (locationBox.Text == "")
            {
                MessageBox.Show("You must enter a location.");
            }
            else if (!File.Exists(locationBox.Text))
            {
                MessageBox.Show("File specified in location either doesn't exist or you don't have rights to access it.");
            }
            else
            {
                ItemName = nameBox.Text;
                ItemLocation = "\"" + locationBox.Text + "\"";
                if (adminRadioButton.Checked)
                    AdminRights = true;

                this.DialogResult = DialogResult.OK;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(locationBox.Text))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(locationBox.Text);
            }
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                locationBox.Text = openFileDialog.FileName;
            }
        }
    }
}
