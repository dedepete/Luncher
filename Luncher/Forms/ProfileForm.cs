using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Telerik.WinControls.UI;

namespace Luncher.Forms
{
    public partial class ProfileForm : RadForm
    {
        public ProfileForm()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            InitializeComponent();
        }

        readonly ResourceManager _locRm = new ResourceManager("Luncher.Forms.ProfileForm", typeof(ProfileForm).Assembly);

        readonly string _minecraft = Program.Minecraft;
        string _profile;

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            Size = !radButton4.Enabled ? MinimumSize : MaximumSize;
            _profile = ProfileName.Text;
            GetVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
            GetParams(ProfileName.Text);
        }

        string _oldver;
        void GetVersions(bool useexperementbuilds, bool usebetabuilds, bool usealphabuilds, bool useotherbuilds)
        {

            _oldver = Versions.Text;
            Versions.Items.Clear();
            Versions.Items.Add(_locRm.GetString("uselastversion"));
            var list = new List<string>();
            var json = JObject.Parse(File.ReadAllText(Variables.McFolder + "/versions/versions.json"));
            var jr = (JArray) json["versions"];
            for (var i = 0; i < jr.Count; i++)
            {
                var id = "null";
                var type = "null";
                try
                {
                    id = json["versions"][i]["id"].ToString();
                    type = json["versions"][i]["type"].ToString();

                }
                catch { }
                list.Add(type + " " + id);
                switch (type)
                {
                    case "snapshot":
                        if (useexperementbuilds) Versions.Items.Add(type + " " + id);
                        break;
                    case "old_beta":
                        if (usebetabuilds) Versions.Items.Add(type + " " + id);
                        break;
                    case "old_alpha":
                        if (usealphabuilds) Versions.Items.Add(type + " " + id);
                        break;
                    case "release":
                        Versions.Items.Add(type + " " + id);
                        break;
                    default:
                        if (useotherbuilds) Versions.Items.Add(type + " " + id);
                        break;
                }
            }
            foreach (var info in from b in Directory.GetDirectories(_minecraft + "\\versions\\")
                let add =
                    !(from a in list
                        let splitted = a.Split(' ')
                        where a.Contains(new DirectoryInfo(b).Name)
                        select splitted).Any()
                where add
                select
                    JObject.Parse(
                        File.ReadAllText(String.Format("{0}/versions/{1}/{1}.json", Variables.McFolder, new DirectoryInfo(b).Name))))
                Versions.Items.Add(info["type"] + " " + info["id"]);
            try
            {
                if (_oldver != null & _oldver != "")
                {
                    var founded = false;
                    foreach (var a in Versions.Items.Where(a => a.Text.Contains(_oldver)))
                    {
                        Versions.SelectedItem = a;
                        founded = true;
                        break;
                    }
                    if (founded != true)
                        Versions.SelectedIndex = 0;
                }
                else
                    Versions.SelectedIndex = 0;
            }
            catch { Versions.SelectedIndex = 0; }
        }

        void GetParams(string pName)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            if (json["profiles"][pName]["allowedReleaseTypes"] != null)
            {
                var allowedVersions = (JArray)json["profiles"][pName]["allowedReleaseTypes"];
                if (allowedVersions.ToString().Contains("old_alpha"))
                    EnableAlpha.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                if (allowedVersions.ToString().Contains("snapshot"))
                    EnableExp.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                if (allowedVersions.ToString().Contains("old_beta"))
                    EnableBeta.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            ResX.Text = json["profiles"][pName]["resolution"] != null ? json["profiles"][pName]["resolution"]["width"].ToString() : "480";
            ResY.Text = json["profiles"][pName]["resolution"] != null ? json["profiles"][pName]["resolution"]["height"].ToString() : "854";
            if (json["profiles"][pName]["gameDir"] != null)
            {
                Gamedir.Text = json["profiles"][pName]["gameDir"].ToString();
                UseDirectory.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            else { Gamedir.Text = _minecraft; }
            if (json["profiles"][pName]["javaDir"] != null)
            {
                ExecJava.Text = json["profiles"][pName]["javaDir"].ToString();
                UseExec.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            else ExecJava.Text = Variables.JavaExe;
            if (json["profiles"][pName]["javaArgs"] != null)
            {
                Args.Text = json["profiles"][pName]["javaArgs"].ToString();
                UseArgs.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            else
                Args.Text = @"-Xmx1G";
            var ver = json["profiles"][_profile]["lastVersionId"] != null
                ? json["profiles"][_profile]["lastVersionId"].ToString()
                : _locRm.GetString("uselastversion");
            try
            {
                var jsonVer = JObject.Parse(File.ReadAllText(String.Format("{0}\\versions\\{1}\\{1}.json", _minecraft, ver)));
                Versions.SelectedItem = Versions.FindItemExact(String.Format("{0} {1}", jsonVer["type"], ver), true);
            }
            catch
            {
                if (ver != null & ver != "")
                {
                    var founded = false;
                    foreach (var a in Versions.Items.Where(a => a.Text.Contains(ver)))
                    {
                        Versions.SelectedItem = a;
                        founded = true;
                        break;
                    }
                    if (founded != true)
                        Versions.SelectedIndex = 0;
                }
                else
                    Versions.SelectedIndex = 0;
            }
            if (json["profiles"][pName]["launcherVisibilityOnGameClose"] != null)
                switch (json["profiles"][pName]["launcherVisibilityOnGameClose"].ToString())
                {
                    case "close launcher when game starts":
                        LState.SelectedIndex = 2;
                        break;
                    case "hide launcher and re-open when game closes":
                        LState.SelectedIndex = 1;
                        break;
                    case "keep the launcher open":
                        LState.SelectedIndex = 0;
                        break;
                }
            else LState.SelectedIndex = 0;
            if (json["profiles"][pName]["server"] != null)
            {
                ipTextBox.Text = json["profiles"][pName]["server"]["ip"].ToString();
                portTextBox.Text = json["profiles"][pName]["server"]["port"] != null? json["profiles"][pName]["server"]["port"].ToString():String.Empty;
                fastConnectCheckBox.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            _changed = false;
        }

        private void EnableExp_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            GetVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
        }

        private void EnableBeta_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            GetVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
        }

        private void EnableAlpha_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            GetVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
        }

        private void UseDirectory_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            Gamedir.Enabled = UseDirectory.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On;
        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Gamedir.Text);
            }catch{}
        }

        public bool Canceled;
        private void radButton1_Click(object sender, EventArgs e)
        {
            Newprofilename = _profile;
            Canceled = true;
            Close();
        }

        public string Newprofilename;
        bool _changed;
        private void radButton2_Click(object sender, EventArgs e)
        {
            string error = null;
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            var json1 = (JObject)json["profiles"];
            var curprofile = (JObject)json1[_profile];
            var allowed = true;
            if (_changed)
                if (json["profiles"].Cast<JProperty>().Any(peep => peep.Name == ProfileName.Text))
                {
                    error = _locRm.GetString("message.invalidname");
                    allowed = false;
                }
            if (Versions.SelectedItem == null)
            {
                error = _locRm.GetString("message.errornull");
                allowed = false;
            }
            if (allowed)
            {
                json["profiles"] = Processing.Rename(json1, name => name == _profile ? ProfileName.Text : name);
                json["selectedProfile"] = ProfileName.Text;
                Newprofilename = ProfileName.Text;
                var v1 = _locRm.GetString("uselastversion");
                if (v1 != null && !Versions.SelectedItem.Text.Contains(v1))
                {
                    var lastversionid = Versions.SelectedItem.Text.Split(' ');
                    curprofile["lastVersionId"] = lastversionid[1];
                }
                else
                {
                    if (curprofile["lastVersionId"] != null)
                        curprofile.Property("lastVersionId").Remove();
                }
                curprofile["name"] = ProfileName.Text;
                if (UseDirectory.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                        curprofile["gameDir"] = Gamedir.Text;
                else
                {
                    if (curprofile["gameDir"] != null)
                        curprofile.Property("gameDir").Remove();
                }
                if (UseExec.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                        curprofile["javaDir"] = ExecJava.Text;
                else
                {
                    if (curprofile["javaDir"] != null)
                        curprofile.Property("javaDir").Remove();
                }
                if (UseArgs.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    curprofile["javaArgs"] = Args.Text;
                else
                {
                    if (curprofile["javaArgs"] != null)
                        curprofile.Property("javaArgs").Remove();
                }
                if (fastConnectCheckBox.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    var jo = new JObject
                    {
                        {"ip", ipTextBox.Text}
                    };
                    if (portTextBox.Text != String.Empty) jo.Add("port", portTextBox.Text);
                    curprofile["server"] = jo;
                }
                else
                {
                    if (curprofile["server"] != null)
                        curprofile.Property("server").Remove();
                }
                var item = new JArray {"release"};
                if (EnableExp.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    item.Add("snapshot");
                if (EnableAlpha.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    item.Add("old_alpha");
                if (EnableBeta.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    item.Add("old_beta");
                curprofile["allowedReleaseTypes"] = item;
                curprofile["launcherVisibilityOnGameClose"] = LState.SelectedItem.Tag.ToString();
                json["profiles"][ProfileName.Text] = curprofile;
                File.WriteAllText(Variables.ProfileJsonFile, json.ToString());
                Close();
            }
            else
                Logging.Info(error);
        }

        private void ProfileName_TextChanged(object sender, EventArgs e)
        {
            _changed = _profile != ProfileName.Text;
        }

        private void UseExec_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            ExecJava.Enabled = UseExec.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On;
        }

        private void UseArgs_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            Args.Enabled = UseArgs.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On;
        }

        private void Versions_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {

        }

        public bool Deleted;
        private void radButton4_Click(object sender, EventArgs e)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            var json1 = (JObject)json["profiles"];
            if (json1.Count - 1 != 0)
            {
                var dr = MessageBox.Show(_locRm.GetString("message.deleteprofiletext"),
                    _locRm.GetString("message.deleteprofile"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;
                json1.Property(_profile).Remove();
                File.WriteAllText(Variables.ProfileJsonFile, json.ToString());
                Deleted = true;
                Close();
            }
            else
                MessageBox.Show(_locRm.GetString("message.deleteprofilelast"), _locRm.GetString("message.error"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void radCheckBox1_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            ipTextBox.Enabled = fastConnectCheckBox.Checked;
            portTextBox.Enabled = fastConnectCheckBox.Checked;
        }
    }
}
