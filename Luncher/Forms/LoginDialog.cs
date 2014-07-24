using System;
using System.IO;
using Luncher.YaDra4il;
using Newtonsoft.Json.Linq;

namespace Luncher.Forms
{
    public partial class LoginDialog : Telerik.WinControls.UI.RadForm
    {
        public string Result { get; private set; }
        public LoginDialog()
        {
            InitializeComponent();
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            try
            {

                Logging.Info("Authenticating...");
                var auth = new AuthManager {email = radTextBox1.Text, password = radTextBox2.Text};
                auth.Login();

                var jo = JObject.Parse(File.ReadAllText(Variables.McFolder + "/luncher/userprofiles.json"));
                var item = (JObject)jo["profiles"];
                try
                {
                    item.Remove(auth.GetUsernameByUUID());
                }
                catch { }
                var j = new JObject
                {
                    new JProperty("type", "official"),
                    new JProperty("accessToken", auth.sessionToken),
                    new JProperty("clientToken", auth.accessToken),
                    new JProperty("UUID", auth.uuid)
                };
                item.Add(new JProperty(auth.GetUsernameByUUID(), j));
                File.WriteAllText(Variables.McFolder + "/luncher/userprofiles.json", jo.ToString());
                Result = "Added successfuly";
                Close();
            }
            catch(Exception a)
            {
                const string text = "Smth went wrong. Invalid credentials?";
                Logging.Info(text + "\n" + a);
                radLabel1.Text = text + "\n" + a.Data;
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            Result = "Cancelled";
            Close();
        }
    }
}
