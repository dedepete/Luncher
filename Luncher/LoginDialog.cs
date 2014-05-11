using System;
using System.IO;
using Newtonsoft.Json.Linq;

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
            var auth = new Auth()
            {
                user = radTextBox1.Text,
                password = radTextBox2.Text
            };
            var a = auth.Authenticate();
            if (a.Contains("Error"))
            {
                radLabel1.Text = "Failed to log in";
            }
            else
            {
                var b = a.Split(':');
                var jo = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/luncher/userprofiles.json"));
                var item = (JObject)jo["profiles"];
                try
                {
                    item.Remove(b[0]);
                }
                catch { }
                var j = new JObject
                {
                    new JProperty("type", "official"),
                    new JProperty("accessToken", b[1]),
                    new JProperty("clientToken", b[2]),
                    new JProperty("UUID", b[3])
                };
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
