using Ionic.Zip;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Luncher
{
    public partial class Launcher : RadForm
    {
        public Launcher()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.lang);
            InitializeComponent();
            var openVer = new RadMenuItem {Text = LocRM.GetString("contextver.open")};
            openVer.Click += openVer_Clicked;
            VerContext.Items.Add(openVer);
            var VerS = new RadMenuSeparatorItem();
            VerContext.Items.Add(VerS);
            var delVer = new RadMenuItem {Text = LocRM.GetString("contextver.del")};
            delVer.Click += delVer_Clicked;
            VerContext.Items.Add(delVer);
        }

        public ResourceManager LocRM = new ResourceManager("Luncher.Launcher", typeof(Launcher).Assembly);

        string minecraft = Program.minecraft;
        private void openVer_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(radListView1.SelectedItem[0].ToString()))
                {
                    Process.Start(Variables.MCVersions + "/" + radListView1.SelectedItem[0] + "/");
                }
            }
            catch { }
        }

        private void delVer_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(radListView1.SelectedItem[0].ToString()))
                {
                    var dr =
                        RadMessageBox.Show(
                            LocRM.GetString("contextver.del.a") + "(" + radListView1.SelectedItem[0] + ")?",
                            LocRM.GetString("contextver.del.b"),
                            MessageBoxButtons.YesNo, RadMessageIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        MLog(LocRM.GetString("contextver.del.progress") + " " + radListView1.SelectedItem[0] + "...");
                        try
                        {
                            Directory.Delete(Variables.MCVersions + "/" + radListView1.SelectedItem[0] + "/", true);
                            GetVersions();
                            GetSelectedVersion(SelectProfile.SelectedItem.Text);
                        }
                        catch (Exception ex)
                        {
                            ELog(LocRM.GetString("contextver.del.error") + "\n" + ex);
                        }
                    }
                }
            }
            catch { }
        }

        private Auth auth;

        private void Launcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("#Luncher "+ProductVersion+" configuration file");
            sb.AppendLine("#");
            sb.AppendLine();
            sb.AppendLine("luncher.main.lang=" + Program.lang);
            sb.AppendLine("luncher.main.renamewindow=" + RenameWindow.SelectedIndex);
            sb.AppendLine();
            sb.AppendLine("luncher.gamelogging.enable=" + EnableMinecraftLogging.Checked.ToString());
            sb.AppendLine("luncher.gamelogging.usegameprefix=" + UseGamePrefix.Checked.ToString());
            sb.AppendLine();
            sb.AppendLine("luncher.updater.updateversions=" + AllowUpdateVersions.Checked.ToString());
            sb.AppendLine("luncher.updater.updateprogram=" + radCheckBox1.Checked.ToString());
            sb.AppendLine("luncher.updater.alerts=" + EnableMinecraftUpdateAlerts.Checked.ToString());
            sb.AppendLine();
            sb.AppendLine("luncher.resources.enablerebuilding=" + AllowReconstruct.Checked.ToString());
            sb.AppendLine("luncher.resources.rebuildresource=" + ReconstructingIndex.Text);
            sb.AppendLine("luncher.resources.assetspath=" + usingAssets.Text);
            File.WriteAllText(Program.minecraft + "\\luncher\\configuration.cfg", sb.ToString());
            Application.Exit();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            UpdateUserProfiles();
            CleanNatives();
            GetTranslations();
            GetVersions();
            AboutVersion.Text = ProductVersion;
            Log.ScrollToCaret();
            if (File.Exists(Variables.localProfileList))
            {
                GetItems();
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
            }
            if (File.Exists(Variables.MCFolder + "/lastlogin"))
            {
                var nickname = File.ReadAllText(Variables.MCFolder + "/lastlogin");
                Nickname.Text = nickname;
            }
            MLog(LocRM.GetString("program.started"));
        }

        void GetTranslations()
        {
            foreach (var i in Directory.GetDirectories(Application.StartupPath))
            {
                foreach (var a in Directory.GetFiles(i).Where(a => Path.GetFileName(a).Contains("name")))
                {
                    LangDropDownList.Items.Add(new RadListDataItem
                    {
                        Text = Path.GetFileNameWithoutExtension(a) + " (" + i.Substring(i.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ")",
                        Tag = i.Substring(i.LastIndexOf(Path.DirectorySeparatorChar) + 1)
                    });
                }
            }
            var index = -1;
            foreach (var i in LangDropDownList.Items)
            {
                index++;
                if (i.Text.Contains(Program.lang))
                {
                    Console.WriteLine(i.Tag + " " + Program.lang);
                    LangDropDownList.SelectedIndex = index;
                    break;
                }
            }
            loadedlang = true;
        }

        #region Logging
        public void MLog(string text)
        {
            Log.AppendText("[Luncher][INFO][" + DateTime.Now.ToString("dd-MM-yy HH:mm:ss") + "] " + text + "\n");
            Log.ScrollToCaret();
        }

        public void WLog(string text)
        {
            var start = Log.TextLength;
            Log.AppendText("[Luncher][WARNING][" + DateTime.Now.ToString("dd-MM-yy HH:mm:ss") + "] " + text + "\n");
            var end = Log.TextLength;
            Log.Select(start, end - start);
            {
                Log.SelectionColor = Color.Orange;
            }
            Log.SelectionLength = 0; 
            Log.ScrollToCaret();
        }

        public void ELog(string text)
        {
            var start = Log.TextLength;
            Log.AppendText("[Luncher][ERROR][" + DateTime.Now.ToString("dd-MM-yy HH:mm:ss") + "] " + text + "\n");
            var end = Log.TextLength;
            Log.Select(start, end - start);
            {
                Log.SelectionColor = Color.Red;
            }
            Log.SelectionLength = 0;
            Log.ScrollToCaret();
        }
        #endregion

        void AddUserProfile()
        {
            var lf = new LoginDialog();
            lf.ShowDialog();
            MLog(lf.result);
        }

        void UpdateUserProfiles()
        {
            if (File.Exists(minecraft + "/luncher/userprofiles.json"))
            {
                Nickname.Items.Clear();
                var userprofiles = JObject.Parse(File.ReadAllText(minecraft + "/luncher/userprofiles.json"));
                foreach (JProperty peep in userprofiles["profiles"])
                {
                    Nickname.Items.Add(peep.Name);
                }
            }
            else
            {
                var templ = @"{
  'profiles': {
  }
}";
                File.WriteAllText(minecraft + "/luncher/userprofiles.json", templ);
            }
        }
        RichTextBox ShowReport(string text, string profilename)
        {
            var report = new RadPageViewPage();
            var closebutton = new RadButton();
            //var showerrorsonly = new RadToggleButton();
            //var shownormalonly = new RadToggleButton();
            var panel = new RadPanel();
            var reportbox = new RichTextBox();
            panel.Text = text;
            panel.Dock = DockStyle.Top;
            panel.Size = new Size(panel.Size.Width, 60);
            closebutton.Text = LocRM.GetString("close.text");
            closebutton.Location = new Point(panel.Size.Width - (closebutton.Size.Width + 5), 5);
            closebutton.Anchor = (AnchorStyles.Right | AnchorStyles.Top);
            closebutton.Enabled = false;
            closebutton.Click += closetab;
            closebutton.Tag = report;
            //showerrorsonly.Text = "Только ошибки";
            //showerrorsonly.Location = new Point(closebutton.Location.X - (showerrorsonly.Width+ 5), 5);
            //showerrorsonly.Anchor = (AnchorStyles.Right | AnchorStyles.Top);
            //shownormalonly.Text = "Только лог";
            //shownormalonly.Location = new Point(closebutton.Location.X - (showerrorsonly.Width + 5), 10 + shownormalonly.Height);
            //shownormalonly.Anchor = (AnchorStyles.Right | AnchorStyles.Top);
            report.Text = "Log: " + profilename;
            reportbox.Dock = DockStyle.Fill;
            reportbox.ReadOnly = true;
            reportbox.Tag = closebutton;
            panel.Controls.Add(closebutton);
            //panel.Controls.Add(showerrorsonly);
            //panel.Controls.Add(shownormalonly);
            report.Controls.Add(reportbox);
            report.Controls.Add(panel);
            radPageView1.Pages.Add(report);
            radPageView1.SelectedPage = report;
            return reportbox;
        }
        void closetab(object sender, EventArgs e)
        {
            var rb = sender as RadButton;
            var page = rb.Tag as RadPageViewPage;
            radPageView1.Pages.Remove(page);
        }

        private void SelectProfile_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            try
            {
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
                var json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
                json["selectedProfile"] = SelectProfile.SelectedItem.Text;
                File.WriteAllText(Variables.localProfileList, json.ToString());
            }
            catch { }
        }

        public void GetSelectedVersion(string profile)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            string state;
            string ver1 = null;
            try
            {
                ver1 = json["profiles"][profile]["lastVersionId"].ToString();
            }
            catch
            {
                var allowed = (JArray)json["profiles"][profile]["allowedReleaseTypes"];
                if (allowed.ToString().Contains("snapshot"))
                {
                    ver1 = Variables.lastSnapshot;
                }
                else
                {
                    ver1 = Variables.lastRelease;
                }
            }
            if (Directory.Exists(minecraft + "/versions/" + ver1))
            {
                state = LocRM.GetString("launcherstate.readytoplay");
            }
            else
            {
                state = LocRM.GetString("launcherstate.readytodownloadandplay");
            }
            SelectedVersion.Text = LocRM.GetString("launcherstate.readytext")+ " " + state + " " + ver1;
            if (!File.Exists(Variables.MCVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                !File.Exists(Variables.MCVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                Variables.workingOffline == true)
            {
                LaunchButton.Enabled = false;
                LaunchButton.Text = "Недоступно";
            }
            else if (File.Exists(Variables.MCVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                     File.Exists(Variables.MCVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                     Variables.workingOffline == true)
            {
                LaunchButton.Enabled = true;
                LaunchButton.Text = "Запуск";
            }
        }

        void GetItems()
        {
            SelectProfile.Items.Clear();
            //ProfileList.Rows.Clear();
            var json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            foreach (JProperty peep in json["profiles"])
            {
                SelectProfile.Items.Add(peep.Name);
            }
            try
            {
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(json["selectedProfile"].ToString(), true);
            }
            catch
            {
                SelectProfile.SelectedIndex = 0;
            }
        }
        private void NewsBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (NewsBrowser.Url != new Uri("http://mcupdate.tumblr.com/"))
            {
                radButton2.Enabled = NewsBrowser.CanGoBack;
                radButton1.Enabled = NewsBrowser.CanGoForward;
                radPanel2.Text = NewsBrowser.Url.ToString();
                radPanel2.Visible = true;
            }
            else
            {
                radPanel2.Visible = false;
            }
        }

        private void radButton2_Click(object sender, EventArgs e)
        {
            NewsBrowser.GoBack();
        }

        private void radButton1_Click(object sender, EventArgs e)
        {
            NewsBrowser.GoForward();
        }

        private void EditProfile_Click(object sender, EventArgs e)
        {
            ChangeProgile(true);
        }

        private void Launch_Click(object sender, EventArgs e)
        {
            if (Nickname.Text == null | Nickname.Text == "")
            {
                Nickname.Text = "Player" + DateTime.Now.ToString("HHmmss");
            }
            SetNullProgressBar();
            ShowProgressBar();
            LaunchButtonChange(LocRM.GetString("launcher.wait"), false);
            try
            {
                File.WriteAllText(Variables.MCFolder + "/lastlogin", Nickname.Text);
            }
            catch
            {
            }
            radPageView1.SelectedPage = ConsolePage;
            LaunchButtonClicked(2);
        }

        void LaunchButtonClicked(int step)
        {
            var profileJSON = Processing.GetProfileDetails(SelectProfile.SelectedItem.Text);
            switch (step)
            {
                case 0:
                {
                    Variables.userName = Nickname.Text;
                    string index = null;
                    try
                    {
                        string ver = null;
                        var obj = JObject.Parse(profileJSON);
                        try
                        {
                            ver = obj["lastVersionId"].ToString();
                        }
                        catch
                        {
                            var allowed = (JArray)obj["lastVersionId"].ToString();
                            if (allowed.ToString().Contains("snapshot"))
                            {
                                ver = Variables.lastSnapshot;
                            }
                            else
                            {
                                ver = Variables.lastRelease;
                            }
                        }
                        var verJSON = JObject.Parse(File.ReadAllText(minecraft + "\\versions\\" + ver + "\\" + ver + ".json"));
                        index = verJSON["assets"].ToString();
                    }
                    catch { }
                    if (index != null)
                    {
                        CheckResourses(index, 0);
                    }
                    else
                    {
                        ReformatAssets();
                    }
                }
                    break;
                case 1:
                    GetVersions();
                    Launch(profileJSON);
                    break;
                case 2:
                {
                    var webc = new WebClient();
                    var obj = JObject.Parse(profileJSON); 
                    try
                    {
                        ver = obj["lastVersionId"].ToString();
                    }
                    catch
                    {
                        var allowed = (JArray)obj["allowedReleaseTypes"];
                        if (allowed.ToString().Contains("snapshot"))
                        {
                            ver = Variables.lastSnapshot;
                        }
                        else
                        {
                            ver = Variables.lastRelease;
                        }
                    }
                    if (!File.Exists(minecraft + "/versions/" + ver + "/" + ver + ".jar"))
                    {
                        try { Directory.CreateDirectory(Path.GetDirectoryName(minecraft + "/versions/" + ver + "/" + ver + ".jar")); }
                        catch { }
                        progressBar1.Text = LocRM.GetString("downloader.inprogress") + " " + minecraft + "/versions/" + ver + "/" + ver + ".jar" + "...";
                        MLog(LocRM.GetString("downloader.inprogress") + " " + minecraft + "/versions/" + ver + "/" + ver + ".jar" + "...");
                        webc.DownloadFileCompleted += CompletedVerJar;
                        webc.DownloadProgressChanged += ProgressChanged;
                        webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/" + ver + "/" + ver + ".jar"), minecraft + "/versions/" + ver + "/" + ver + ".jar");
                    }
                    else
                    {
                        LaunchButtonClicked(3);
                    }
                }
                    break;
                case 3:
                {
                    var webc = new WebClient();
                    var obj = JObject.Parse(profileJSON);
                    try
                    {
                        ver = obj["lastVersionId"].ToString();
                    }
                    catch
                    {
                        var allowed = (JArray)obj["allowedReleaseTypes"];
                        ver = allowed.ToString().Contains("snapshot") ? Variables.lastSnapshot : Variables.lastRelease;
                    }
                    if (!File.Exists(minecraft + "/versions/" + ver + "/" + ver + ".json"))
                    {
                        try { Directory.CreateDirectory(Path.GetDirectoryName(minecraft + "/versions/" + ver + "/" + ver + ".json")); }
                        catch { }
                        progressBar1.Text = LocRM.GetString("downloader.inprogress") + " " + minecraft + "/versions/" + ver + "/" + ver + ".json" + "...";
                        MLog(LocRM.GetString("downloader.inprogress") + " " + minecraft + "/versions/" + ver + "/" + ver + ".json" + "...");
                        webc.DownloadFileCompleted += CompletedVerJson;
                        webc.DownloadProgressChanged += ProgressChanged;
                        webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/" + ver + "/" + ver + ".json"), minecraft + "/versions/" + ver + "/" + ver + ".json");
                    }
                    else
                    {
                        LaunchButtonClicked(4);
                    }
                }
                    break;
                case 4:
                {
                    var json = JObject.Parse(File.ReadAllText(minecraft + "/versions/" + ver + "/" + ver + ".json"));
                    nativesFolder = Path.Combine(minecraft + "/versions", ver);
                    string profileSJson = File.ReadAllText(nativesFolder + @"\" + ver + ".json");
                    MLog(LocRM.GetString("lib.checking"));
                    var sb = new StringBuilder();
                    var gsb = new StringBuilder();
                    var nsb = new StringBuilder();
                    var temp2 = new string[2];
                    string finalPath;
                    var libFileName = "";
                    var missing = 0;
                    var all = 0;
                    var jr = (JArray)json["libraries"];
                    for (var i = 0; i < jr.Count; i++)
                    {
                        all++;
                        string url = null;
                        temp2[0] = json["libraries"][i]["name"].ToString().Split(':')[0];
                        temp2[1] = json["libraries"][i]["name"].ToString().Split(':')[1] + @"\" + json["libraries"][i]["name"].ToString().Split(':')[2];
                        try { url = json["libraries"][i]["url"].ToString(); } catch { }
                        if (json["libraries"][i]["natives"] != null)
                        {
                            if (json["libraries"][i]["natives"]["windows"] != null)
                            {
                                libFileName = temp2[1].Replace(@"\", "-") + "-" +json["libraries"][i]["natives"]["windows"] + ".jar";
                                libFileName = libFileName.Replace("${arch}", IntPtr.Size == 8 ? "64" : "32");
                            }
                        }
                        else
                        {
                            libFileName = temp2[1].Replace(@"\", "-") + ".jar";
                        }
                        temp2[0] = temp2[0].Replace(".", @"\");
                        finalPath = Path.Combine(temp2[0], temp2[1], libFileName);
                        if (!finalPath.Contains("natives"))
                        {
                            if (json["libraries"][i]["rules"] != null)
                            {
                                if (json["libraries"][i]["rules"].Count() < 2)
                                {
                                    if (json["libraries"][i]["rules"][0]["action"].ToString() == "allow" &&
                                        json["libraries"][i]["rules"][0]["os"]["name"].ToString() == "osx")
                                    {
                                    }
                                    else if (json["libraries"][i]["rules"][0]["action"].ToString() == "allow" &&
                                             json["libraries"][i]["rules"][0]["os"]["name"].ToString() == "windows")
                                    {
                                        gsb.Append(Variables.MCFolder + "\\libraries\\" + finalPath + ";");
                                    }
                                }
                                else
                                {
                                    gsb.Append(Variables.MCFolder + "\\libraries\\" + finalPath + ";");
                                }
                            }
                            else
                            {
                                gsb.Append(Variables.MCFolder + "\\libraries\\" + finalPath + ";");
                            }
                        }
                        else
                        {
                            if (json["libraries"][i]["rules"] != null)
                            {
                                if (json["libraries"][i]["rules"].Count() < 2)
                                {
                                    if (json["libraries"][i]["rules"][0]["action"].ToString() == "allow" &&
                                        json["libraries"][i]["rules"][0]["os"]["name"].ToString() == "osx")
                                    {
                                    }
                                    else if (json["libraries"][i]["rules"][0]["action"].ToString() == "allow" &&
                                             json["libraries"][i]["rules"][0]["os"]["name"].ToString() == "windows")
                                    {
                                        nsb.Append(finalPath + ";");
                                    }
                                }
                                else
                                {
                                    nsb.Append(finalPath + ";");
                                }
                            }
                            else
                            {
                                nsb.Append(finalPath + ";");
                            }
                        }
                        if (!File.Exists(Path.Combine(minecraft + "/libraries", temp2[0], temp2[1], libFileName)))
                        {
                            missing++;
                            ELog(Path.Combine(minecraft + "/libraries", temp2[0], temp2[1], libFileName) + ", " + LocRM.GetString("lib.notfound"));
                            if (url == null)
                            {
                                sb.Append(finalPath + ";");
                            }
                            else
                            {
                                sb.Append(finalPath + "@" + url + ";");
                            }
                        }
                    }
                    MLog(LocRM.GetString("lib.completed1p") + " " + all + ". " + LocRM.GetString("lib.completed2p") + " " + missing);
                    var libfinal = sb.ToString();
                    libs = gsb.ToString();
                    nativelibs = nsb.ToString().Substring(0, nsb.ToString().Length - 1);
                    if (missing == 0)
                    {
                        LaunchButtonClicked(0);
                    }
                    else
                    {
                        libstodownload = libfinal.Substring(0, libfinal.Length - 1).Split(';');
                        ltotal = missing;
                        progressBar1.Maximum = ltotal + 1;
                        DownloadLibs();
                    }
                }
                    break;
            }
        }

        private void CompletedVerJar(object sender, AsyncCompletedEventArgs e)
        {
            SetNullProgressBar();
            LaunchButtonClicked(3);
        }

        private void CompletedVerJson(object sender, AsyncCompletedEventArgs e)
        {
            SetNullProgressBar();
            LaunchButtonClicked(4);
        }

        string[] libstodownload = null;
        int ltotal = 0;
        int lcur = 0;
        Stopwatch lsw = new Stopwatch();

        private void DownloadLibs()
        {
            string filename;
            string url = null;
            if (libstodownload[lcur].Contains('@'))
            {
                
                filename = libstodownload[lcur].Split('@')[0];
                url = libstodownload[lcur].Split('@')[1];
            }
            else
            {
                filename = libstodownload[lcur];
            }
            var webc = new WebClient();
            if (!Directory.Exists(minecraft))
            {
                Directory.CreateDirectory(minecraft);
            }
            try
            {
                try
                {
                    Directory.CreateDirectory(minecraft + "\\libraries\\" + filename.Replace(Path.GetFileName(filename), String.Empty));
                }
                catch { }
                sw.Start();
                webc.DownloadFileCompleted += LibCompleted;
                webc.DownloadProgressChanged += ProgressChangedLib;
                MLog(LocRM.GetString("downloadprocess.downloading") + " " + filename + "...");
                webc.DownloadFileAsync(
                    url == null ? new Uri("https://libraries.minecraft.net/" + filename) : new Uri(url + filename),
                    minecraft + "\\libraries\\" + filename);
            }
            catch (Exception ex)
            {
                ELog(LocRM.GetString("downloader.error") + "\n" + ex);
            }
        }
        private void ProgressChangedLib(object sender, DownloadProgressChangedEventArgs e)
        {
            var filename = libstodownload[lcur];
            var downloaded = string.Format("{0} MB's / {1} MB's", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            var speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            progressBar1.Text = LocRM.GetString("downloadprocess.downloading") + " \\libraries\\" + filename + "...  [" + speed + " | " + downloaded + "]";
        }
        private void LibCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var lname = libstodownload[lcur];
            var size = new FileInfo(Variables.MCFolder + "\\libraries\\" + lname).Length.ToString();
            var completedtext = LocRM.GetString("lib.downloadingcomplete");
            completedtext = completedtext.Replace("{0}", lname);
            completedtext = completedtext.Replace("{1}", size);
            MLog(completedtext);
            lsw.Reset();
            lcur++;
            if (lcur == ltotal)
            {
                lcur = 0;
                ltotal = 0;
                libstodownload = null;
                LaunchButtonClicked(0);
                progressBar1.Value1 = progressBar1.Maximum;
            }
            else
            {
                try
                {
                    progressBar1.Value1 = lcur;
                }
                catch { }
                DownloadLibs();
            }
        }

        void GetVersions()
        {
            radListView1.Items.Clear();
            foreach (var s in Directory.GetDirectories(Variables.MCFolder + "/versions/"))
            {
                var versionname = new DirectoryInfo(s).Name;
                if (File.Exists(Variables.MCFolder + "/versions/" + versionname + "/" + versionname + ".jar") & File.Exists(Variables.MCFolder + "/versions/" + versionname + "/" + versionname + ".json"))
                {
                    var json = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/versions/" + versionname + "/" + versionname + ".json"));
                    var id = "null";
                    var type = "null";
                    var time = "null";
                    try
                    {
                        id = json["id"].ToString();
                        type = json["type"].ToString();
                        time = json["releaseTime"].ToString();
                    }
                    catch(Exception ex)
                    {
                        ELog(LocRM.GetString("getversions.error") + "\n" + ex.ToString());
                    }
                    radListView1.Items.Add(id, type, time);
                }
            }
        }

        #region Launch
        void GetDetails(string jsonraw)
        {
            var json = JObject.Parse(jsonraw);
            if (json["javaArgs"] != null)
            {
                javaArgs = json["javaArgs"].ToString() + " ";
            }
            if (json["javaDir"] != null)
            {
                javaExec = json["javaDir"].ToString();
            }
            pName = json["name"].ToString();
            try
            {
                gameDir = json["gameDir"].ToString();
            }
            catch
            {
                gameDir = minecraft;
            }
            try
            {
                lastVersionID = json["lastVersionId"].ToString();
            }
            catch
            {
                var allowed = (JArray)json["allowedReleaseTypes"];
                lastVersionID = allowed.ToString().Contains("snapshot") ? Variables.lastSnapshot : Variables.lastRelease;
            }
            if (json["allowedReleaseTypes"] != null)
            {
                foreach (var releaseType in json["allowedReleaseTypes"])
                {
                    allowedReleaseTypes.Add(releaseType.ToString());
                }
            }
            try
            {
                if (json["launcherVisibilityOnGameClose"].ToString() == "close launcher when game starts")
                {
                    cl = true;
                }
                else if (json["launcherVisibilityOnGameClose"].ToString() ==
                         "hide launcher and re-open when game closes")
                {
                    hl = true;
                }
            }
            catch
            {
            }
            nativesFolder = Path.Combine(minecraft + "/versions", lastVersionID);
            var profileSJson = File.ReadAllText(nativesFolder + @"\" + lastVersionID + ".json");
            var profilejsono = JObject.Parse(profileSJson);
            Variables.mainClass = profilejsono["mainClass"].ToString();
            arg = profilejsono["minecraftArguments"].ToString();
            libs = libs + nativesFolder + @"\" + lastVersionID + ".jar";
            var natives = nativelibs.Split(';');
            try
            {
                foreach (var a in natives)
                {
                    using (var zip = ZipFile.Read(Variables.MCFolder + "/libraries/" + a))
                    {
                        zip.ExtractAll(Variables.MCFolder + "/natives/temp/", ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                foreach (
                    var a in
                        new DirectoryInfo(Variables.MCFolder + "\\natives\\temp\\").GetFiles("*.dll",
                            SearchOption.AllDirectories))
                {
                    MLog("Перемещаю " + a.Name + " в " + Variables.MCFolder + "\\natives\\");
                    File.Move(a.FullName, Variables.MCFolder + "\\natives\\" + a.Name);
                }
                MLog("Удаляю временную папку...");
                Directory.Delete(Variables.MCFolder + "\\natives\\temp\\", true);
            }
            catch (Exception ex)
            {
                ELog(ex.ToString());
            }
        }
        public string pName = null;
        public string gameDir = null;
        public string lastVersionID = null;
        public List<string> allowedReleaseTypes = new List<string>();
        public string javaArgs = "-Xmx1G "; // default
        public string nativesFolder = Variables.MCVersions;
        public string libs = null;
        public string nativelibs = null;
        public string arg = null;
        public string ver = null;
        public string javaExec = Variables.javaExe;
        public string assets = "1.7.4";
        public bool hl = false;
        public bool cl = false;
        void Launch(string profileJSON)
        {
            try
            {
                var add = true;
                var jo = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/luncher/userprofiles.json"));
                var profiles = (JObject)jo["profiles"];
                foreach (JProperty peep in jo["profiles"].Cast<JProperty>().Where(peep => peep.Name == Nickname.Text))
                {
                    add = false;
                    if (jo["profiles"][peep.Name]["type"].ToString() == "pirate")
                    {
                        Variables.accessToken = "someInterestingAccessToken";
                        Variables.clientToken = "someInterestingClientToken";
                    }
                    else
                    {
                        var topost = new JObject();
                        topost.Add(new JProperty("accessToken", jo["profiles"][peep.Name]["accessToken"]));
                        var resp = MakePOST.mPOSTJSON(AuthShemes.authserver + AuthShemes.validate, topost.ToString());
                        if (resp.Contains("Error"))
                        {
                            var topost1 = new JObject();
                            var part = new JObject();
                            topost1.Add(new JProperty("accessToken", jo["profiles"][peep.Name]["accessToken"]));
                            topost1.Add(new JProperty("clientToken", jo["profiles"][peep.Name]["clientToken"]));
                            part.Add(new JProperty("id", jo["profiles"][peep.Name]["UUID"]));
                            part.Add("name", peep.Name);
                            topost1.Add("selectedProfile", part);
                            var response = MakePOST.mPOSTJSON(AuthShemes.authserver + AuthShemes.validate,
                                topost1.ToString());
                            if (!response.Contains("Error"))
                            {
                                var jo1 = JObject.Parse(response);
                                jo["profiles"][peep.Name]["accessToken"] = jo1["accessToken"];
                            }
                        }
                        Variables.accessToken = jo["profiles"][peep.Name]["accessToken"].ToString();
                        Variables.clientToken = jo["profiles"][peep.Name]["UUID"].ToString();
                    }
                    break;
                }
                if (add)
                {
                    var jo1 = new JObject();
                    jo1.Add(new JProperty("type", "pirate"));
                    profiles.Add(Nickname.Text, jo1);
                }
                File.WriteAllText(Variables.MCFolder + "/luncher/userprofiles.json", jo.ToString());
                var lselected = Nickname.Text;
                UpdateUserProfiles();
                Nickname.SelectedValue = lselected;
            }
            catch { }
            HideProgressBar();
            GetDetails(profileJSON);
            var mp = new MinecraftProcess()
            {
                arg = arg,
                assetspath = usingAssets.Text,
                libs = libs,
                pName = pName,
                javaExec = javaExec,
                root = this,
                javaArgs = javaArgs,
                gameDir = gameDir,
                assets = assets,
                lastVersionID = lastVersionID,
                txt = ShowReport("Minecraft version: " + lastVersionID, pName)
            };
            mp.Launch();
        }
        #endregion
        public static JToken Rename(JToken json, Dictionary<string, string> map)
        {
            return Rename(json, name => map.ContainsKey(name) ? map[name] : name);
        }

        public static JToken Rename(JToken json, Func<string, string> map)
        {
            var prop = json as JProperty;
            if (prop != null)
            {
                return new JProperty(map(prop.Name), Rename(prop.Value, map));
            }

            var arr = json as JArray;
            if (arr != null)
            {
                var cont = arr.Select(el => Rename(el, map));
                return new JArray(cont);
            }

            var o = json as JObject;
            if (o != null)
            {
                var cont = o.Properties().Select(el => Rename(el, map));
                return new JObject(cont);
            }

            return json;
        }

        string todownload = null;

        void CheckResourses(string index, int step)
        {
            assets = index;
            var webc = new WebClient();
            switch (step)
            {
                case 0:
                    if (!File.Exists(minecraft + "/assets/indexes/" + index + ".json"))
                    {
                        try { Directory.CreateDirectory(minecraft + "/assets/indexes/"); }
                        catch { }
                        progressBar1.Text = LocRM.GetString("downloader.inprogress") + " " + minecraft + "/assets/indexes/" + index + ".json" + "...";
                        MLog(LocRM.GetString("downloader.inprogress") + " " + minecraft + "/assets/indexes/" + index + ".json" + "...");
                        indexcont = index;
                        progressBar1.Maximum = 100;
                        webc.DownloadFileCompleted += Completed;
                        webc.DownloadProgressChanged += ProgressChanged;
                        webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/indexes/" + index + ".json"), minecraft + "/assets/indexes/" + index + ".json");
                    }
                    else
                    {
                        CheckResourses(index, 1);
                    }
                    break;
                case 1:
                {
                    assetstodownload = null;
                    total = 0;
                    cur = 0;
                    indexcont = index;
                    var all = 0;
                    var missing = 0;
                    MLog(LocRM.GetString("resources.checking"));
                    JObject json = JObject.Parse(File.ReadAllText(minecraft + "/assets/indexes/" + index + ".json"));
                    foreach (JProperty peep in json["objects"])
                    {
                        all++;
                        string c = json["objects"][peep.Name]["hash"].ToString();
                        char с1 = c[0];
                        char с2 = c[1];
                        string filename = с1.ToString() + с2.ToString() + "\\" + json["objects"][peep.Name]["hash"];
                        if (File.Exists(minecraft + "/assets/objects/" + filename))
                        {

                        }
                        else
                        {
                            missing++;
                            ELog("\\assets\\objects\\" + filename + ", " + LocRM.GetString("lib.notfound"));
                            todownload = todownload + filename + ";";
                        }
                    }
                    MLog(LocRM.GetString("resources.completed1p") + " " + all + ". " + LocRM.GetString("resources.completed2p") + " " + missing);
                    if (missing != 0)
                    {
                        assetstodownload = todownload.Substring(0, todownload.Length - 1).Split(';');
                        total = missing;
                        progressBar1.Maximum = total + 1;
                        DownloadResourses1();
                    }
                    else
                    {
                        LaunchButtonClicked(1);
                    }
                }
                    break;
            }
        }
        string indexcont = null;
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            //SetNullProgressBar();
            progressBar1.Text = LocRM.GetString("downloadprocess.downloading") + " " + minecraft + "/assets/indexes/" + indexcont + ".json" + " " + LocRM.GetString("downloading.completed");
            CheckResourses(indexcont, 1);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                progressBar1.Maximum = 100;
                progressBar1.Value2 = e.ProgressPercentage;
            }
            catch
            {
            }
        }

        string[] assetstodownload = null;
        int total = 0;
        int cur = 0;
        Stopwatch sw = new Stopwatch();
        void DownloadResourses1()
        {
            string filename = assetstodownload[cur];
            var webc = new WebClient();
            if (Directory.Exists(minecraft) == false)
            {
                Directory.CreateDirectory(minecraft);
            }
            try
            {
                try
                {
                    Directory.CreateDirectory(minecraft + "\\assets\\objects\\" + filename.Replace(Path.GetFileName(filename), String.Empty));
                }
                catch { }
                sw.Start();
                webc.DownloadFileCompleted += ResCompleted;
                webc.DownloadProgressChanged += ProgressChangedRes;
                MLog(LocRM.GetString("downloadprocess.downloading") + " \\assets\\objects\\" + filename + "...");
                webc.DownloadFileAsync(new Uri("http://resources.download.minecraft.net/" + filename), minecraft + "\\assets\\objects\\" + filename);
            }
            catch (Exception ex)
            {
                ELog(ex.ToString());
            }
        }
        private void ProgressChangedRes(object sender, DownloadProgressChangedEventArgs e)
        {
                var filename = assetstodownload[cur];
                var downloaded = string.Format("{0} MB's / {1} MB's",(e.BytesReceived / 1024d / 1024d).ToString("0.00"),(e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
                var speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
                progressBar1.Text = LocRM.GetString("downloadprocess.downloading") + " \\assets\\objects\\" + filename + "...  [" + speed + " | " + downloaded + "]";
        }
        private void ResCompleted(object sender, AsyncCompletedEventArgs e)
        {
            sw.Reset();
            cur++;
            if (cur == total)
            {
                cur = 0;
                total = 0;
                assetstodownload = null;
                CheckResourses(indexcont, 1);
                progressBar1.Value1 = progressBar1.Maximum;
            }
            else
            {
                try
                {
                    progressBar1.Value1 = cur;
                }
                catch { }
                DownloadResourses1();
            }
        }

        void SetNullProgressBar()
        {
            progressBar1.Value1 = 0;
            progressBar1.Value2 = 0;
        }

        void ShowProgressBar()
        {
            progressBar1.Visible = true;
        }

        void HideProgressBar()
        {
            progressBar1.Text = "";
            progressBar1.Visible = false;
        }

        public void LaunchButtonChange(string text, bool enablestate)
        {
            LaunchButton.Text = text;
            LaunchButton.Enabled = enablestate;
        }

        private void AddProfile_Click(object sender, EventArgs e)
        {
            try
            {
                var newprofilename = DateTime.Now.ToString("HH:mm:ss");
                var json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
                var json1 = (JObject)json["profiles"];
                var toparse = "" + json1[SelectProfile.Text];
                var curprofile = JObject.Parse(toparse);
                Console.WriteLine(newprofilename);
                newprofilename = "Copy of " + curprofile["name"] + "(" + newprofilename + ")";
                MLog(LocRM.GetString("profile.createcopy") + " " + SelectProfile.Text + "(" + newprofilename + ")" + "...");
                curprofile["name"] = newprofilename;
                Console.WriteLine();
                json1.Add(new JProperty(newprofilename, curprofile));
                File.WriteAllText(Variables.localProfileList, json.ToString());
                GetItems();
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(newprofilename, true);
                ChangeProgile(false);
            }
            catch(Exception ex)
            {
                ELog(LocRM.GetString("profile.createerror") + "\n" + ex);
            }
        }

        void ChangeProgile(bool isediting)
        {
            MLog(LocRM.GetString("profile.editing") + " " + SelectProfile.Text + "...");
            var pf = new ProfileForm
            {
                ProfileName = {Text = SelectProfile.Text},
                radButton4 = {Enabled = isediting}
            };
            pf.ShowDialog();
            if (pf.deleted == false)
            {
                SelectProfile.Items.Add(pf.newprofilename);
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(pf.newprofilename, true);
                GetItems();
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
                if (pf.canceled != true)
                {
                    MLog(LocRM.GetString("profile.edited.complete1p") + " " + SelectProfile.Text + " " + LocRM.GetString("profile.edited.complete2p"));
                }
                else
                {
                    MLog(LocRM.GetString("profile.delete.canceled"));
                }
            }
            else
            {
                MLog(LocRM.GetString("profile.delete.succes"));
                GetItems();
                SelectProfile.SelectedIndex = 0;
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            Process.Start("http://vk.com/sesmc");
        }

        private void label5_Click(object sender, EventArgs e)
        {
            Process.Start("http://ru-minecraft.ru");
        }

        void ReformatAssets()
        {
            if (AllowReconstruct.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                var all = 0;
                var reconstructed = 0;
                try
                {
                    MLog(LocRM.GetString("resources.reconstructing"));
                    var json =
                        JObject.Parse(
                            File.ReadAllText(minecraft + "/assets/indexes/" + ReconstructingIndex.Text + ".json"));
                    foreach (JProperty peep in json["objects"])
                    {
                        all++;
                        var c = json["objects"][peep.Name]["hash"].ToString();
                        char с1 = c[0];
                        char с2 = c[1];
                        var filename = с1.ToString() + с2.ToString() + "\\" + json["objects"][peep.Name]["hash"];
                        if (!File.Exists(minecraft + "/assets/" + peep.Name))
                        {
                            MLog(minecraft + "/assets/objects/" + filename + " -> " + minecraft + "/assets/" + peep.Name);
                            try
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(minecraft + "/assets/" + peep.Name));
                            }
                            catch
                            {
                            }
                            File.Copy(minecraft + "/assets/objects/" + filename, minecraft + "/assets/" + peep.Name);
                            reconstructed++;
                        }
                    }
                    MLog(LocRM.GetString("resources.recostructionsuccestotal") + " " + all + ". " + LocRM.GetString("resources.recostructionsuccestotalrecostructed") + " " +
                         reconstructed);
                }
                catch (Exception ex)
                {
                    ELog(LocRM.GetString("resources.reconstructionerror") + "\n" + ex.ToString());
                }
            }
            else
            {
                WLog(LocRM.GetString("resources.reconstructioncanceled"));
            }
            LaunchButtonClicked(1);
        }

        private void AllowReconstruct_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            if (AllowReconstruct.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                ReconstructingIndex.Enabled = true;
            }
            else
            {
                ReconstructingIndex.Enabled = false;
            }
        }

        private void EnableMinecraftLogging_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            if (EnableMinecraftLogging.ToggleState == Telerik.WinControls.Enumerations.ToggleState.Off)
            {
                UseGamePrefix.Enabled = false;
            }
            else
            {
                UseGamePrefix.Enabled = true;
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            Process.Start("http://mcoffline.ru");
        }

        bool loadedlang = false;
        private void radDropDownList1_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            if (LangDropDownList.SelectedItem.Text.Contains("ru"))
            {
                Program.lang = "";
            }
            else
            {
                Program.lang = LangDropDownList.SelectedItem.Tag.ToString();
            }
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.lang);
            if (loadedlang)
            {
                RadMessageBox.Show(LocRM.GetString("lang.changemessage"), "Language changed", MessageBoxButtons.OK,
                    RadMessageIcon.Info);
                WLog(LocRM.GetString("lang.changemessage"));
            }
        }

        public void CleanNatives()
        {
            if (Directory.Exists(Variables.MCFolder + "/natives"))
            {
                MLog("Очистка natives...");
                foreach (string file in Directory.GetFiles(Variables.MCFolder + "/natives"))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        ELog(ex.ToString());
                    }
                }
            }
        }

        private void radListView1_ItemMouseClick(object sender, ListViewItemEventArgs e)
        {
            radListView1.SelectedItem = e.Item;
        }

        private void label10_Click(object sender, EventArgs e)
        {
            Process.Start(@"https://github.com/Ilan321/MCLauncher");
        }

        private void radButton3_Click(object sender, EventArgs e)
        {
            AddUserProfile();
            UpdateUserProfiles();
        }
    }

    public class MinecraftProcess
    {
        public object root { get; set; }
        public string gameDir { get; set; }
        public string arg { get; set; }
        public string pName { get; set; }
        public string assetspath { get; set; }
        public string javaExec { get; set; }
        public string libs { get; set; }
        public string javaArgs { get; set; }
        public string assets { get; set; }

        public RichTextBox txt { get; set; }

        public string lastVersionID { get; set; }

        private Process Client;

        private string errors;
        private string logs;

        private int tflood = 0;
        string tlast = null;
        private int eflood = 0;
        string elast = null;

        [DllImport("user32.dll")]
        public static extern int SetWindowText(IntPtr hWnd, string text);

        public void MLogG(string text, bool iserror, RichTextBox txt)
        {
            Color color;
            if (iserror)
            {
                color = Color.Red;
            }
            else
            {
                color = Color.DarkSlateGray;
            }
            string line;
            if ((root as Launcher).UseGamePrefix.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                line = "[GAME]" + text + "\n";
            }
            else
            {
                line = text + "\n";
            }
            int start = txt.TextLength;
            txt.AppendText(line);
            int end = txt.TextLength;
            txt.Select(start, end - start);
            {
                txt.SelectionColor = color;
            }
            txt.SelectionLength = 0;
            txt.ScrollToCaret();
        }

        private void t_reader()
        {
            while (true)
            {
                var mroot = root as Launcher;
                try
                {
                    string line = "";
                    while (line.Trim() == "")
                    {
                        line = Client.StandardOutput.ReadLine();
                        if (tlast == line)
                        {
                            tflood++;
                        }
                        else
                        {
                            tflood = 0;
                            tlast = line;
                        }
                        try
                        {
                            if (line.Contains("Attempting early MinecraftForge initialization") & rnw)
                            {
                                mroot.Invoke((MethodInvoker)delegate
                                {
                                    rnw = false;
                                    MLogG("[Forge]Инициализация Minecraft Forge...", false, txt);
                                });
                            }
                            if (line.Contains("Sound engine started") & rnw == false)
                            {
                                mroot.Invoke((MethodInvoker)delegate
                                {
                                    rnw = true;
                                    MLogG("[Forge]Инициализация Minecraft Forge закончена", false, txt);
                                });
                            }
                            if (tflood < 3)
                            {
                                mroot.Invoke((MethodInvoker)(() => MLogG(line, false, txt)));
                                logs = logs + "\n" + line;
                            }
                            if (rnw)
                            {
                                switch (mroot.RenameWindow.SelectedIndex)
                                {
                                    case 0:
                                        SetWindowText(Client.MainWindowHandle,
                                            "Minecraft - " + mroot.lastVersionID + " - " + mroot.ProductName + " " + mroot.ProductVersion);
                                        break;
                                    case 2:
                                        SetWindowText(Client.MainWindowHandle, "Minecraft");
                                        break;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    break;
                }
            }
        }

        bool rnw = true;

        void e_reader()
        {
            while (true)
            {
                var mroot = root as Launcher;
                try
                {
                    var line = "";
                    while (line.Trim() == "")
                    {
                        line = Client.StandardError.ReadLine();
                        if (elast == line)
                        {
                            eflood++;
                        }
                        else
                        {
                            eflood = 0;
                            elast = line;
                        }
                        if (line.Contains("Attempting early MinecraftForge initialization"))
                        {
                            mroot.Invoke((MethodInvoker)delegate
                            {
                                rnw = false;
                                MLogG("[Forge]Инициализация Minecraft Forge...", false, txt);
                            });
                        }
                        if (line.Contains("Sound engine started"))
                        {
                            mroot.Invoke((MethodInvoker)delegate
                            {
                                rnw = true;
                                MLogG("[Forge]Инициализация Minecraft Forge закончена", false, txt);
                            });
                        }
                        try
                        {
                            if (eflood < 3)
                            {
                                mroot.Invoke((MethodInvoker)(() => MLogG(line, true, txt)));
                                errors = errors + "\n" + line;
                            }
                            if (rnw)
                            {
                                switch (mroot.RenameWindow.SelectedIndex)
                                {
                                    case 0:
                                        SetWindowText(Client.MainWindowHandle,
                                            "Minecraft - " + lastVersionID + " - " + mroot.ProductName + " " + mroot.ProductVersion);
                                        break;
                                    case 2:
                                        SetWindowText(Client.MainWindowHandle, "Minecraft");
                                        break;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                catch (NullReferenceException) { break; }
            }
        }

        private static Thread Reader;
        private static Thread ErrorReader;
        public void Launch()
        {
            var mroot = root as Launcher;
            Client = new Process();
            var proc = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = javaExec
            };
            gameDir = gameDir.Replace("${AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            proc.WorkingDirectory = gameDir;
            string nativespath = "-Djava.library.path=" + Program.minecraft + "/natives";
            if (gameDir.Contains(" "))
            {
                gameDir = "\"" + gameDir + "\"";
            }
            if (libs.Contains(" "))
            {
                libs = "\"" + libs + "\"";
            }
            if (assetspath.Contains(" "))
            {
                assetspath = "\"" + assetspath + "\"";
            }
            if (nativespath.Contains(" "))
            {
                nativespath = "\"" + nativespath + "\"";
            }
            arg = arg.Replace("${auth_player_name}", Variables.userName);
            arg = arg.Replace("${version_name}", pName);
            arg = arg.Replace("${game_directory}", gameDir);
            arg = arg.Replace("${assets_root}", assetspath);
            arg = arg.Replace("${game_assets}", assetspath);
            arg = arg.Replace("${assets_index_name}", assets);
            arg = arg.Replace("${auth_session}", Variables.accessToken);
            arg = arg.Replace("${auth_access_token}", Variables.accessToken);
            arg = arg.Replace("${auth_uuid}", Variables.clientToken);
            arg = arg.Replace("${user_properties}", "{\"luncher\":[1234]}");
            arg = arg.Replace("${AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            arg = arg.Replace("${user_type}", "mojang");
            arg = arg.Replace("\\\"", "\"");
            proc.Arguments = javaArgs + nativespath + " -cp " + libs + " " + Variables.mainClass + " " + arg;
            proc.StandardErrorEncoding = Encoding.UTF8;
            Client.StartInfo = proc;
            mroot.MLog(mroot.LocRM.GetString("launch.workingdir") + " " + gameDir);
            mroot.MLog(mroot.LocRM.GetString("launch.command") + " " + proc.FileName + " " + proc.Arguments);
            try
            {
                mroot.LaunchButtonChange(mroot.LocRM.GetString("launch.launchtext"), true);
                mroot.GetSelectedVersion(mroot.SelectProfile.SelectedItem.Text);
                Client.EnableRaisingEvents = true;
                Client.Exited += Client_Exited;
                Client.Start();
                if (mroot.EnableMinecraftLogging.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    Reader = new Thread(t_reader);
                    Reader.Start();
                }
                ErrorReader = new Thread(e_reader);
                ErrorReader.Start();
                if (mroot.hl)
                {
                    mroot.WindowState = FormWindowState.Minimized;
                }
                if (mroot.cl)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                mroot.ELog(mroot.LocRM.GetString("launch.error") + "\n" + ex.ToString());
            }
        }
        private void Client_Exited(object sender, EventArgs e)
        {
            var mroot = root as Launcher;
            var proc = sender as Process;
            mroot.Invoke((MethodInvoker)delegate
            {
                var radButton = txt.Tag as RadButton;
                radButton.Enabled = true;
                mroot.CleanNatives();
                MLogG(("Процесс был завершён с кодом " + proc.ExitCode + ". Сеанс с " + proc.StartTime.ToString("HH:mm:ss") + "(Всего" + (Math.Round(proc.StartTime.Subtract(DateTime.Now).TotalMinutes, 2)).ToString().Replace('-', ' ') + " min)"), false, txt);
                if (mroot.hl)
                {
                    mroot.WindowState = FormWindowState.Normal;
                    mroot.hl = false;
                }
            });
        }
    }
}
