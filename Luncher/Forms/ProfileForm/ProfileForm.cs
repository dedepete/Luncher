using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telerik.WinControls.UI;

namespace Luncher.Forms.ProfileForm
{
    public partial class ProfileForm : RadForm
    {
        public ProfileForm()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            InitializeComponent();
        }

        private readonly ResourceManager _locRm = new ResourceManager("Luncher.Forms.ProfileForm.ProfileForm",
            typeof (ProfileForm).Assembly);

        private readonly string _minecraft = Program.Minecraft;
        private string _profile;

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            Size = !radButton4.Enabled ? MinimumSize : MaximumSize;
            _profile = ProfileName.Text;
            GetVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
            GetParams(ProfileName.Text);
        }

        private string _oldver;

        private void GetVersions(bool useexperementbuilds, bool usebetabuilds, bool usealphabuilds, bool useotherbuilds)
        {
            _oldver = Versions.Text;
            Versions.Items.Clear();
            Versions.Items.Add(_locRm.GetString("uselastversion"));
            var list = new List<string>();
            var json = JObject.Parse(File.ReadAllText(Variables.McFolder + "/versions/versions.json"));
            var jr = (JArray) json["versions"];
            for (var i = 0; i < jr.Count; i++)
            {
                if (json["versions"][i] == null) continue;
                var id = json["versions"][i]["id"].ToString();
                var type = json["versions"][i]["type"].ToString();
                list.Add(String.Format("{0} {1}", type, id));
                var ritem = new RadListDataItem {Text = type + " " + id, Tag = id};
                switch (type)
                {
                    case "snapshot":
                        if (useexperementbuilds) Versions.Items.Add(ritem);
                        break;
                    case "old_beta":
                        if (usebetabuilds) Versions.Items.Add(ritem);
                        break;
                    case "old_alpha":
                        if (usealphabuilds) Versions.Items.Add(ritem);
                        break;
                    case "release":
                        Versions.Items.Add(type + " " + id);
                        break;
                    default:
                        if (useotherbuilds) Versions.Items.Add(ritem);
                        break;
                }
            }
            foreach (var info in from b in Directory.GetDirectories(Variables.McVersions)
                where File.Exists(String.Format("{0}/{1}/{1}.json", Variables.McVersions,
                    new DirectoryInfo(b).Name))
                let add = list.All(a => !a.Contains(new DirectoryInfo(b).Name))
                where add
                select
                    JObject.Parse(
                        File.ReadAllText(String.Format("{0}/{1}/{1}.json", Variables.McVersions,
                            new DirectoryInfo(b).Name))))
                Versions.Items.Add(info["type"] + " " + info["id"]);
            try
            {
                if (_oldver != null & _oldver != String.Empty)
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
            catch
            {
                Versions.SelectedIndex = 0;
            }
        }

        private void GetParams(string pName)
        {
            var temp = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            dynamic json = JObject.Parse(temp["profiles"][pName].ToString());
            if (json.allowedReleaseTypes != null)
            { 
                if (json.allowedReleaseTypes.ToList().Contains("old_alpha"))
                    EnableAlpha.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                if (json.allowedReleaseTypes.ToList().Contains("snapshot"))
                    EnableExp.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                if (json.allowedReleaseTypes.ToList().Contains("old_beta")) 
                    EnableBeta.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            ResX.Text = json.resolution != null ? json.resolution.width : @"480";
            ResY.Text = json.resolution != null ? json.resolution.height : @"854";
            if (json.gameDir != null)
            {
                Gamedir.Text = json.gameDir; 
                UseDirectory.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            else
                Gamedir.Text = _minecraft;
            if (json.javaDir != null)
            {
                ExecJava.Text = json.javaDir;
                UseExec.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            else ExecJava.Text = Variables.JavaExe;
            if (json.javaArgs != null)
            {
                Args.Text = json.javaArgs;
                UseArgs.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
            }
            else
                Args.Text = @"-Xmx1G";
            var ver = json.lastVersionId ?? _locRm.GetString("uselastversion");
            try
            {
                var jsonVer =
                    JObject.Parse(File.ReadAllText(String.Format("{0}\\{1}\\{1}.json", Variables.McVersions, ver)));
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
            if (json.launcherVisibilityOnGameClose != null)
                switch ((string)json.launcherVisibilityOnGameClose)
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
            if (json.server != null)
            {
                ipTextBox.Text = json.server.ip;
                portTextBox.Text = json.server.port ?? String.Empty;
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
            }
            catch
            {
            }
        }

        public bool Canceled;

        private void radButton1_Click(object sender, EventArgs e)
        {
            Newprofilename = _profile;
            Canceled = true;
            Close();
        }

        public string Newprofilename;
        private bool _changed;

        private void radButton2_Click(object sender, EventArgs e)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            var json1 = (JObject) json["profiles"];
            if (_changed)
                if (json["profiles"].Cast<JProperty>().Any(peep => peep.Name == ProfileName.Text))
                {
                    Logging.Info(_locRm.GetString("message.invalidname"));
                    return;
                }
            if (Versions.SelectedItem == null)
            {
                Logging.Info(_locRm.GetString("message.errornull"));
                return;
            }
            json["profiles"] = Processing.Rename(json1, name => name == _profile ? ProfileName.Text : name);
            json["selectedProfile"] = ProfileName.Text;
            Newprofilename = ProfileName.Text;
            JsonProfile.ProfileServer server = null;
            if (ipTextBox.Text != String.Empty)
                server = new JsonProfile.ProfileServer
                {
                    ip = ipTextBox.Text,
                    port = portTextBox.Text != String.Empty ? portTextBox.Text : null
                };
            var resolution = new JsonProfile.ProfileResolution
            {
                height = ResY.Text,
                width = ResX.Text
            };
            var releasetypes = new List<string> {"release"};
            if (EnableExp.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                releasetypes.Add("snapshot");
            if (EnableAlpha.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                releasetypes.Add("old_alpha");
            if (EnableBeta.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                releasetypes.Add("old_beta");
            string gamedir = null;
            if (Gamedir.Text != String.Empty)
            {
                gamedir = Gamedir.Text;
                if (gamedir[Gamedir.Text.Length - 1].ToString() == "\\" ||
                    gamedir[Gamedir.Text.Length - 1].ToString() == "/")
                    gamedir = Gamedir.Text.Substring(0, Gamedir.Text.Length - 1);
            }
            var version = new JsonProfile.Profile
            {
                name = ProfileName.Text,
                lastVersionId =
                    !Versions.SelectedItem.Text.Contains(_locRm.GetString("uselastversion"))
                        ? ((string) Versions.SelectedItem.Tag ??
                           (Versions.SelectedItem.Text.Replace(
                               Versions.SelectedItem.Text.Split(' ')[0] + " ", String.Empty)))
                        : null,
                gameDir = UseDirectory.Checked ? gamedir : null,
                javaDir = UseExec.Checked ? ExecJava.Text : null,
                javaArgs = UseArgs.Checked ? Args.Text : null,
                server = server,
                resolution = resolution,
                allowedReleaseTypes = releasetypes.ToArray<string>(),
                launcherVisibilityOnGameClose = LState.SelectedItem.Tag.ToString()
            };
            json["profiles"][ProfileName.Text] = JObject.FromObject(version,
                new JsonSerializer {NullValueHandling = NullValueHandling.Ignore});
            File.WriteAllText(Variables.ProfileJsonFile, json.ToString());
            Close();
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

        public bool Deleted;

        private void radButton4_Click(object sender, EventArgs e)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            var json1 = (JObject) json["profiles"];
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