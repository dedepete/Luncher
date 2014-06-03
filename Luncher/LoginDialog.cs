using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Luncher
{
    public partial class LoginDialog : Telerik.WinControls.UI.RadForm
    {
        public string Result { get; set; }
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            var auth = new Auth
            {
                User = radTextBox1.Text,
                Password = radTextBox2.Text
            };
            var a = auth.Authenticate();
            if (!a.Contains(":"))
            {
                Logging.Log("", true, false,  a);
                radLabel1.Text = a;
            }
            else
            {
                var b = a.Split(':');
                var jo = JObject.Parse(File.ReadAllText(Variables.McFolder + "/luncher/userprofiles.json"));
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
                File.WriteAllText(Variables.McFolder + "/luncher/userprofiles.json", jo.ToString());
                Result = "Added successfuly";
                Close();
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            Result = "Cancelled";
            Close();
        }
    }
}
