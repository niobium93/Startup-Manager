using System;
using System.Windows.Forms;
using System.IO;

namespace Startup_Manager
{
    public partial class AddForm : Form
    {
        public string name = "";
        public string location = "";
        public bool allUsers = false;
        public bool done = false;
        public AddForm(bool isAdmin)
        {
            InitializeComponent();
            if (!isAdmin)
                adminRadioButton.Enabled = false;
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
                MessageBox.Show("File specified in location doesn't exist or you don't have rights to access it.");
            }
            else
            {
                name = nameBox.Text;
                location = "\"" + locationBox.Text + "\"";
                if (adminRadioButton.Checked)
                    allUsers = true;
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
