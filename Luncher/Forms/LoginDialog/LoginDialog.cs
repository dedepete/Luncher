using System;
using System.IO;
using Luncher.Localization;
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
                var auth = new AuthManager {Email = username.Text, Password = password.Text};
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
                    new JProperty("UUID", auth.Uuid),
                    new JProperty("properties", auth.UserProperties)
                }));
                File.WriteAllText(Variables.McFolder + "/luncher/userprofiles.json", jo.ToString());
                Result = Localization_LoginForm.LoginDialog_Added_successfuly;
                Processing.ShowAlert(Localization_LoginForm.LoginDialog_Added_successfuly, Localization_LoginForm.LoginDialog_Added_successfuly_message);
                Close();
            }
            catch(Exception a)
            {
                Logging.Info(Localization_LoginForm.LoginDialog_SmthWentWrong + "\n" + a);
                radLabel1.Text = Localization_LoginForm.LoginDialog_SmthWentWrong + "\n" + a.Data;
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            Result = "Cancelled";
            Close();
        }

        private void KeyPressed(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == System.Windows.Forms.Keys.Enter)
                acceptButton.PerformClick();
        }
    }
}
