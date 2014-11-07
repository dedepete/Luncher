using System;
using System.IO;
using Luncher.YaDra4il;
using Newtonsoft.Json.Linq;

namespace Luncher.Forms.LoginDialog
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
                var auth = new AuthManager {Email = radTextBox1.Text, Password = radTextBox2.Text};
                auth.Login();
                var jo = JObject.Parse(File.ReadAllText(Variables.McFolder + "/luncher/userprofiles.json"));
                jo["selectedUsername"] = auth.Username;
                var item = (JObject)jo["profiles"];
                try
                {
                    item.Remove(auth.Username);
                }
                catch { }
                item.Add(new JProperty(auth.Username, new JObject
                {
                    new JProperty("type", "official"),
                    new JProperty("accessToken", auth.SessionToken),
                    new JProperty("clientToken", auth.AccessToken),
                    new JProperty("UUID", auth.Uuid)
                }));
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
