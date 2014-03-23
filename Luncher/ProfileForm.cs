using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Luncher
{
    public partial class ProfileForm : Telerik.WinControls.UI.RadForm
    {
        public ProfileForm()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.lang);
            InitializeComponent();
        }

        ResourceManager LocRM = new ResourceManager("Luncher.ProfileForm", typeof(ProfileForm).Assembly);

        string minecraft = Program.minecraft;
        string profile;

        private void ProfileForm_Load(object sender, EventArgs e)
        {
            if (radButton4.Enabled == false)
            {
                this.Size = this.MinimumSize;
            }
            else
            {
                this.Size = this.MaximumSize;
            }
            profile = ProfileName.Text;
            getVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
            getParams(ProfileName.Text);
        }

        string oldver;
        void getVersions(bool useexperementbuilds, bool usebetabuilds, bool usealphabuilds, bool useotherbuilds)
        {

            oldver = Versions.Text;
            Versions.Items.Clear();
            string ver = null;
            Versions.Items.Add(LocRM.GetString("uselastversion"));
            List<string> list = new List<string>();
            JObject json = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/versions/versions.json"));
            JArray jr = (JArray) json["versions"];
            for (int i = 0; i < jr.Count; i++)
            {
                string id = "null";
                string type = "null";
                try
                {
                    id = json["versions"][i]["id"].ToString();
                    type = json["versions"][i]["type"].ToString();

                }
                catch
                {
                }
                list.Add(type + " " + id);
                if (type == "snapshot" & useexperementbuilds == true)
                {
                    Versions.Items.Add(type + " " + id);
                }
                else if (type == "old_beta" & usebetabuilds == true)
                {
                    Versions.Items.Add(type + " " + id);
                }
                else if (type == "old_alpha" & usealphabuilds == true)
                {
                    Versions.Items.Add(type + " " + id);
                }
                else if (type == "release")
                {
                    Versions.Items.Add(type + " " + id);
                }
                else if (type != "release" & type != "snapshot" & type != "old_alpha" & type != "old_beta" &
                         useotherbuilds == true)
                {
                    Versions.Items.Add(type + " " + id);
                }
            }
            foreach (string b in Directory.GetDirectories(minecraft + "\\versions\\"))
            {
                bool add = true;
                string type = "null";
                foreach (string a in list)
                {
                    string[] splitted = a.Split(' ');
                    if (a.Contains(new DirectoryInfo(b).Name))
                    {
                        type = splitted[0];
                        add = false;
                        break;
                    }
                }
                if (add == true)
                {
                    JObject info = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/versions/" + new DirectoryInfo(b).Name + "/" + new DirectoryInfo(b).Name + ".json"));
                    Versions.Items.Add(info["type"] + " " + info["id"]);
                }
            }
            try
            {
                if (oldver != null & oldver != "")
                {
                    bool founded = false;
                    foreach (RadListDataItem a in Versions.Items)
                    {
                        if (a.Text.Contains(oldver))
                        {
                            Versions.SelectedItem = a;
                            founded = true;
                            break;
                        }
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

        void getParams(string pName)
        {
            JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            try
            {
                JArray allowedVersions = (JArray)json["profiles"][pName]["allowedReleaseTypes"];
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
                    Gamedir.Text = minecraft;
                }
            }
            catch { Gamedir.Text = minecraft; }
            try
            {
                if (json["profiles"][pName]["javaDir"].ToString() != null)
                {
                    ExecJava.Text = json["profiles"][pName]["javaDir"].ToString();
                    UseExec.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ExecJava.Text = Variables.javaExe;
                }
            }
            catch { ExecJava.Text = Variables.javaExe; }
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
            string useversion = null;
            string ver = null;
            try
            {
                ver = json["profiles"][profile]["lastVersionId"].ToString();
            }
            catch
            {
                useversion = LocRM.GetString("uselastversion");
            }
            try
            {
                JObject jsonVer = JObject.Parse(File.ReadAllText(minecraft + "\\versions\\" + ver + "\\" + ver + ".json"));
                Versions.SelectedItem = Versions.FindItemExact(jsonVer["type"].ToString() + " " + ver, true);
            }
            catch
            {
                if (ver != null & ver != "")
                {
                    bool founded = false;
                    foreach (RadListDataItem a in Versions.Items)
                    {
                        if (a.Text.Contains(ver))
                        {
                            Versions.SelectedItem = a;
                            founded = true;
                            break;
                        }
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
                if (json["profiles"][pName]["launcherVisibilityOnGameClose"].ToString() == "close launcher when game starts")
                {
                    LState.SelectedIndex = 2;
                }
                else if (json["profiles"][pName]["launcherVisibilityOnGameClose"].ToString() ==
                         "hide launcher and re-open when game closes")
                {
                    LState.SelectedIndex = 1;
                }
                else if (json["profiles"][pName]["launcherVisibilityOnGameClose"].ToString() == "keep the launcher open")
                {
                    LState.SelectedIndex = 0;
                }
            }
            catch { LState.SelectedIndex = 0; }
            changed = false;
        }

        private void EnableExp_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            getVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
        }

        private void EnableBeta_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            getVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
        }

        private void EnableAlpha_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            getVersions(EnableExp.Checked, EnableBeta.Checked, EnableAlpha.Checked, EnableOther.Checked);
        }

        private void UseDirectory_ToggleStateChanged(object sender, Telerik.WinControls.UI.StateChangedEventArgs args)
        {
            if (UseDirectory.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                Gamedir.Enabled = true;
            }
            else
            {
                Gamedir.Enabled = false;
            }
        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Gamedir.Text);
            }catch{}
        }

        public bool canceled = false;
        private void radButton1_Click(object sender, EventArgs e)
        {
            newprofilename = profile;
            canceled = true;
            this.Close();
        }

        public string newprofilename = null;
        bool changed = false;
        private void radButton2_Click(object sender, EventArgs e)
        {
            string error = null;
            JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            JObject json1 = (JObject)json["profiles"];
            JObject curprofile = (JObject)json1[profile];
            bool allowed = true;
            if (changed == true)
            {
                foreach (JProperty peep in json["profiles"])
                {
                    if (peep.Name == ProfileName.Text)
                    {
                        error = LocRM.GetString("message.invalidname");
                        allowed = false;
                        break;
                    }
                    else
                    {
                        allowed = true;
                    }
                }
            }
            if (Versions.SelectedItem == null)
            {
                error = LocRM.GetString("message.errornull");
                allowed = false;
            }
            if (allowed == true)
            {
                json["profiles"] = Launcher.Rename(json1, name => name == profile ? ProfileName.Text : name);
                json["selectedProfile"] = ProfileName.Text;
                newprofilename = ProfileName.Text;
                if (!Versions.SelectedItem.Text.Contains(LocRM.GetString("uselastversion")))
                {
                    string[] lastversionid = Versions.SelectedItem.Text.Split(' ');
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
                    JArray item = new JArray();
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
                File.WriteAllText(Variables.localProfileList, json.ToString());
                this.Close();
            }
            else
            {
                MessageBox.Show(error, LocRM.GetString("message.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProfileName_TextChanged(object sender, EventArgs e)
        {
            if (profile == ProfileName.Text)
            {
                changed = false;
            }
            else
            {
                changed = true;
            }
        }

        private void UseExec_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            if (UseExec.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                ExecJava.Enabled = true;
            }
            else
            {
                ExecJava.Enabled = false;
            }
        }

        private void UseArgs_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            if (UseArgs.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                Args.Enabled = true;
            }
            else
            {
                Args.Enabled = false;
            }
        }

        private void Versions_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {

        }

        public bool deleted = false;
        private void radButton4_Click(object sender, EventArgs e)
        {
            JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            JObject json1 = (JObject)json["profiles"];
            if (json1.Count - 1 != 0)
            {
                DialogResult dr = MessageBox.Show(LocRM.GetString("message.deleteprofiletext"), LocRM.GetString("message.deleteprofile"),
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    json1.Property(profile).Remove();
                    File.WriteAllText(Variables.localProfileList, json.ToString());
                    deleted = true;
                    this.Close();
                }
            }
            else
            {
                MessageBox.Show(LocRM.GetString("message.deleteprofilelast"), LocRM.GetString("message.error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
