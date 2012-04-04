using System;
using System.Windows.Forms;
using System.IO;

namespace Startup_Manager
{
    public partial class AddForm : Form
    {
        public string m_name = "";
        public string m_location = "";
        public bool m_adminRights = false;
        public bool done = false;
        public AddForm(bool isAdmin)
        {
            InitializeComponent();
            if (!isAdmin)
                adminRadioButton.Enabled = false;
        }

        public AddForm(bool isAdmin, string name, string location, bool adminRights)
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
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
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
                m_name = nameBox.Text;
                m_location = "\"" + locationBox.Text + "\"";
                if (adminRadioButton.Checked)
                    m_adminRights = true;
                done = true;
                this.Close();
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                locationBox.Text = openFileDialog1.FileName;
            }
        }
    }
}
