using System.Linq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace Luncher
{
    public partial class ProfileForm : RadForm
    {
        public ProfileForm()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            InitializeComponent();
        }

        readonly ResourceManager _locRm = new ResourceManager("Luncher.ProfileForm", typeof(ProfileForm).Assembly);

        readonly string _minecraft = Program.Minecraft;
        string _profile;

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            if (!radButton4.Enabled)
            {
                    Size = MinimumSize;
            else
            {
                    Size = MaximumSize;
            }
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
            foreach (var info in from b in Directory.GetDirectories(_minecraft + "\\versions\\") let add = !(from a in list let splitted = a.Split(' ') where a.Contains(new DirectoryInfo(b).Name) select splitted).Any() where add select JObject.Parse(File.ReadAllText(Variables.McFolder + "/versions/" + new DirectoryInfo(b).Name + "/" + new DirectoryInfo(b).Name + ".json")))
            {
                Versions.Items.Add(info["type"] + " " + info["id"]);
            }
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
                    {
                        Versions.SelectedIndex = 0;
                    }
                }
                else
                {
                    Versions.SelectedIndex = 0;
                }
            }
            catch { Versions.SelectedIndex = 0; }
        }

        void GetParams(string pName)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.LocalProfileList));
            try
            {
                var allowedVersions = (JArray)json["profiles"][pName]["allowedReleaseTypes"];
                if (allowedVersions.ToString().Contains("old_alpha"))
                {
                    EnableAlpha.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                if (allowedVersions.ToString().Contains("snapshot"))
                {
                    EnableExp.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                if (allowedVersions.ToString().Contains("old_beta"))
                {
                    EnableBeta.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
            }
            catch { }
            try
            {
                ResX.Text = json["profiles"][pName]["resolution"]["width"].ToString();
                ResY.Text = json["profiles"][pName]["resolution"]["height"].ToString();
            }
            catch { }
            try
            {
                if (json["profiles"][pName]["gameDir"].ToString() != null)
                {
                    Gamedir.Text = json["profiles"][pName]["gameDir"].ToString();
                    UseDirectory.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    Gamedir.Text = _minecraft;
                }
            }
            catch { Gamedir.Text = _minecraft; }
            try
            {
                if (json["profiles"][pName]["javaDir"].ToString() != null)
                {
                    ExecJava.Text = json["profiles"][pName]["javaDir"].ToString();
                    UseExec.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ExecJava.Text = Variables.JavaExe;
                }
            }
            catch { ExecJava.Text = Variables.JavaExe; }
            try
            {
                if (json["profiles"][pName]["javaArgs"].ToString() != null)
                {
                    Args.Text = json["profiles"][pName]["javaArgs"].ToString();
                    UseArgs.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    Args.Text = "-Xmx1G";
                }
            }
            catch { Args.Text = "-Xmx1G"; }
            string ver = null;
            try
            {
                ver = json["profiles"][_profile]["lastVersionId"].ToString();
            }
            catch
            {
                _locRm.GetString("uselastversion");
            }
            try
            {
                var jsonVer = JObject.Parse(File.ReadAllText(_minecraft + "\\versions\\" + ver + "\\" + ver + ".json"));
                Versions.SelectedItem = Versions.FindItemExact(jsonVer["type"] + " " + ver, true);
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
                    {
                        Versions.SelectedIndex = 0;
                    }
                }
                else
                {
                    Versions.SelectedIndex = 0;
                }
            }
            try
            {
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
            }
            catch { LState.SelectedIndex = 0; }
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
            var json = JObject.Parse(File.ReadAllText(Variables.LocalProfileList));
            var json1 = (JObject)json["profiles"];
            var curprofile = (JObject)json1[_profile];
            var allowed = true;
            if (_changed)
            {
                if (json["profiles"].Cast<JProperty>().Any(peep => peep.Name == ProfileName.Text))
                {
                    error = _locRm.GetString("message.invalidname");
                    allowed = false;
                }
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
                if (!Versions.SelectedItem.Text.Contains(_locRm.GetString("uselastversion")))
                {
                    var lastversionid = Versions.SelectedItem.Text.Split(' ');
                    curprofile["lastVersionId"] = lastversionid[1];
                }
                else
                {
                    try
                    {
                        curprofile.Property("lastVersionId").Remove();
                    }
                    catch { }
                }
                curprofile["name"] = ProfileName.Text;
                if (UseDirectory.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    try
                    {
                        curprofile["gameDir"] = Gamedir.Text;
                    }
                    catch
                    {
                        curprofile["name"].AddAfterSelf(new JProperty("gameDir", Gamedir.Text));
                    }
                }
                else
                {
                    try
                    {
                    curprofile.Property("gameDir").Remove();
                    }
                    catch {  }
                }
                if (UseExec.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    try
                    {
                        curprofile["javaDir"] = ExecJava.Text;
                    }
                    catch
                    {
                        curprofile["name"].AddAfterSelf(new JProperty("javaDir", ExecJava.Text));
                    }
                }
                else
                {
                    try
                    {
                        curprofile.Property("javaDir").Remove();
                    }
                    catch { }
                }
                if (UseArgs.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    try
                    {
                        curprofile["javaArgs"] = Args.Text;
                    }
                    catch
                    {
                        curprofile["name"].AddAfterSelf(new JProperty("javaArgs", Args.Text));
                    }
                }
                else
                {
                    try
                    {
                        curprofile.Property("javaArgs").Remove();
                    }
                    catch { }
                }
                try
                {
                    var item = new JArray();
                    if (EnableExp.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    {
                        item.Add("snapshot");
                    }
                    if (EnableAlpha.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    {
                        item.Add("old_alpha");
                    }
                    if (EnableBeta.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    {
                        item.Add("old_beta");
                    }
                    item.Add("release");
                    try
                    {
                        curprofile.Property("allowedReleaseTypes").Remove();
                    }
                    catch { }
                    curprofile.Add(new JProperty("allowedReleaseTypes", item));
                }
                catch { }
                try
                {
                    curprofile["launcherVisibilityOnGameClose"] = LState.SelectedItem.Tag.ToString();
                }
                catch
                {
                    curprofile["name"].AddAfterSelf(new JProperty("launcherVisibilityOnGameClose", LState.SelectedItem.Tag.ToString()));
                }
                json["profiles"][ProfileName.Text] = curprofile;
                File.WriteAllText(Variables.LocalProfileList, json.ToString());
                Close();
            }
            else
            {
                Logging.Log("err", true, true, error);
            }
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
            var json = JObject.Parse(File.ReadAllText(Variables.LocalProfileList));
            var json1 = (JObject)json["profiles"];
            if (json1.Count - 1 != 0)
            {
                var dr = MessageBox.Show(_locRm.GetString("message.deleteprofiletext"), _locRm.GetString("message.deleteprofile"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    json1.Property(_profile).Remove();
                    File.WriteAllText(Variables.LocalProfileList, json.ToString());
                    Deleted = true;
                    Close();
                }
            }
            else
            {
                MessageBox.Show(_locRm.GetString("message.deleteprofilelast"), _locRm.GetString("message.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
