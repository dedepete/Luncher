using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Telerik.WinControls;

namespace Luncher
{
    public partial class LoginDialog : Telerik.WinControls.UI.RadForm
    {
        public string result { get; set; }
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            Auth auth = new Auth()
            {
                user = radTextBox1.Text,
                password = radTextBox2.Text
            };
            string a = auth.Authenticate();
            if (a.Contains("Error"))
            {
                radLabel1.Text = "Failed to log in";
            }
            else
            {
                string[] b = a.Split(':');
                JObject jo = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/luncher/userprofiles.json"));
                JObject item = (JObject)jo["profiles"];
                try
                {
                    item.Remove(b[0]);
                }
                catch { }
                JObject j = new JObject();
                j.Add(new JProperty("type", "official"));
                j.Add(new JProperty("accessToken", b[1]));
                j.Add(new JProperty("clientToken", b[2]));
                j.Add(new JProperty("UUID", b[3]));
                item.Add(new JProperty(b[0], j));
                File.WriteAllText(Variables.MCFolder + "/luncher/userprofiles.json", jo.ToString());
                result = "Added successfuly";
                Close();
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            result = "Cancelled";
            Close();
        }
    }
}
