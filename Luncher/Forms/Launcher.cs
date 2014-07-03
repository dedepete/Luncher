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
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using Luncher.Properties;
using Newtonsoft.Json.Linq;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Data;

namespace Luncher.Forms
{
    public partial class Launcher : RadForm
    {
        public Launcher()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            InitializeComponent();
            LogBox.Box = Log;
            var openVer = new RadMenuItem {Text = LocRm.GetString("contextver.open")};
            openVer.Click += (sender, e) =>
            {
                try
                {
                    if (String.IsNullOrEmpty(radListView1.SelectedItem[0].ToString())) return;
                    Process.Start(Variables.McVersions + "/" + radListView1.SelectedItem[0] + "/");
                }
                catch
                {
                }
            };
            VerContext.Items.Add(openVer);
            var verS = new RadMenuSeparatorItem();
            VerContext.Items.Add(verS);
            var delVer = new RadMenuItem {Text = LocRm.GetString("contextver.del")};
            delVer.Click += (sender, e) =>
            {
                try
                {
                    if (String.IsNullOrEmpty(radListView1.SelectedItem[0].ToString())) return;
                    var dr =
                        RadMessageBox.Show(
                            string.Format("{0}({1})?", LocRm.GetString("contextver.del.a"), radListView1.SelectedItem[0]),
                            LocRm.GetString("contextver.del.b"),
                            MessageBoxButtons.YesNo, RadMessageIcon.Question);
                    if (dr != DialogResult.Yes) return;
                    Logging.Info(String.Format("{0} {1}...", LocRm.GetString("contextver.del.progress"), radListView1.SelectedItem[0]));
                    try
                    {
                        Directory.Delete(String.Format("{0}/{1}/", Variables.McVersions, radListView1.SelectedItem[0]), true);
                        GetVersions();
                        GetSelectedVersion(SelectProfile.SelectedItem.Text);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error(String.Format("{0}\n{1}", LocRm.GetString("contextver.del.error"), ex));
                    }
                }
                catch
                {
                }
            };
            VerContext.Items.Add(delVer);
        }

        public readonly ResourceManager LocRm = new ResourceManager("Luncher.Forms.Launcher", typeof (Launcher).Assembly);

        private readonly string _minecraft = Program.Minecraft;

        private void Launcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            var mainObject = new JObject
            {
                {"lang", Program.Lang},
                {"renameWindow", RenameWindow.SelectedIndex}
            };
            var loggingObject = new JObject
            {
                {"enableGameLogging", EnableMinecraftLogging.Checked},
                {"useGamePrefix", UseGamePrefix.Checked}
            };
            var updatesObject = new JObject
            {
                {"checkVersionsUpdate", AllowUpdateVersions.Checked},
                {"checkProgramUpdate", radCheckBox1.Checked},
                {"enableMinecraftUpdateAlerts", EnableMinecraftUpdateAlerts.Checked}
            };
            var resourcesObject = new JObject
            {
                {"enableReconstruction", AllowReconstruct.Checked},
                {"reconstructionSourceFile", ReconstructingIndex.Text},
                {"assetsDir", usingAssets.Text}
            };
            var jo = new JObject
            {
                {"main", mainObject},
                {"logging", loggingObject},
                {"updates", updatesObject},
                {"resources", resourcesObject}
            };
            File.WriteAllText(String.Format("{0}\\luncher\\configuration.cfg", Program.Minecraft), jo.ToString());
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
            if (File.Exists(Variables.ProfileJsonFile))
            {
                GetItems();
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
            }
            if (File.Exists(Variables.McFolder + "/lastlogin"))
            {
                var nickname = File.ReadAllText(Variables.McFolder + "/lastlogin");
                Nickname.Text = nickname;
            }
            Logging.Info(LocRm.GetString("program.started"));
        }

        private void GetTranslations()
        {
            foreach (var i in Directory.GetDirectories(Application.StartupPath))
            {
                foreach (var a in Directory.GetFiles(i).Where(a =>
                {
                    var fileName = Path.GetFileName(a);
                    return fileName != null && fileName.Contains("name");
                })) LangDropDownList.Items.Add(new RadListDataItem
                    {
                        Text =
                            Path.GetFileNameWithoutExtension(a) + " (" +
                            i.Substring(i.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ")",
                        Tag = i.Substring(i.LastIndexOf(Path.DirectorySeparatorChar) + 1)
                    });
            }
            var index = -1;
            foreach (var i in LangDropDownList.Items)
            {
                index++;
                if (!i.Text.Contains(Program.Lang)) continue;
                Console.WriteLine(i.Tag + @" " + Program.Lang);
                LangDropDownList.SelectedIndex = index;
                break;
            }
            _loadedlang = true;
        }

        private static void AddUserProfile()
        {
            var lf = new LoginDialog();
            lf.ShowDialog();
            Logging.Info(lf.Result);
        }

        private void UpdateUserProfiles()
        {
            var filename = String.Format("{0}/luncher/userprofiles.json", _minecraft);
            if (File.Exists(filename))
            {
                Nickname.Items.Clear();
                var userprofiles = JObject.Parse(File.ReadAllText(filename));
                foreach (JProperty peep in userprofiles["profiles"])
                {
                    Nickname.Items.Add(peep.Name);
                }
            }
            else
            {
                const string templ = @"{
  'profiles': {
  }
}";
                File.WriteAllText(filename, templ);
            }
        }

        private Dictionary<int, object> LogTab(string text, string profilename)
        {
            var report = new RadPageViewPage {Text = @"Log: " + profilename};
            var killprocess = new RadButton { Text = Resources.Launcher_ShowLogTab_Завершить, Anchor = (AnchorStyles.Right | AnchorStyles.Top) };
            var panel = new RadPanel {Text = text, Dock = DockStyle.Top};
            panel.Size = new Size(panel.Size.Width, 60);
            var closebutton = new RadButton
            {
                Text = LocRm.GetString("close.text"),
                Anchor = (AnchorStyles.Right | AnchorStyles.Top),
                Enabled = false
            };
            var reportbox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true};
            closebutton.Location = new Point(panel.Size.Width - (closebutton.Size.Width + 5), 5);
            closebutton.Click += (sender, e) =>
            {
                var rb = sender as RadButton;
                if (rb == null) return;
                radPageView1.Pages.Remove(report);
            };
            killprocess.Location = new Point(panel.Size.Width - (killprocess.Size.Width + 5), closebutton.Location.Y + closebutton.Size.Height + 5);
            panel.Controls.Add(closebutton);
            panel.Controls.Add(killprocess);
            report.Controls.Add(reportbox);
            report.Controls.Add(panel);
            radPageView1.Pages.Add(report);
            radPageView1.SelectedPage = report;
            reportbox.LinkClicked += (sender, e) => Process.Start(e.LinkText);
            return new Dictionary<int,object>
            {
                {0, reportbox},
                {1, killprocess},
                {2, closebutton}
            };
        }

        private void SelectProfile_SelectedIndexChanged(object sender,
            PositionChangedEventArgs e)
        {
            try
            {
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
                var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
                json["selectedProfile"] = SelectProfile.SelectedItem.Text;
                File.WriteAllText(Variables.ProfileJsonFile, json.ToString());
            }
            catch
            {
            }
        }

        public void GetSelectedVersion(string profile)
        {
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            string ver1;
            try
            {
                ver1 = json["profiles"][profile]["lastVersionId"].ToString();
            }
            catch
            {
                var allowed = (JArray) json["profiles"][profile]["allowedReleaseTypes"];
                ver1 = allowed.ToString().Contains("snapshot") ? Variables.LastSnapshot : Variables.LastRelease;
            }
            var state =
                LocRm.GetString(Directory.Exists(_minecraft + "/versions/" + ver1)
                    ? "launcherstate.readytoplay"
                    : "launcherstate.readytodownloadandplay");
            SelectedVersion.Text = String.Format("{0} {1} {2}", LocRm.GetString("launcherstate.readytext"), state, ver1);
            if (!File.Exists(Variables.McVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                !File.Exists(Variables.McVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                Variables.WorkingOffline)
            {
                LaunchButton.Enabled = false;
                LaunchButton.Text = @"Недоступно";
            }
            else if (File.Exists(Variables.McVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                     File.Exists(Variables.McVersions + "/" + ver1 + "/" + ver1 + ".jar") &&
                     Variables.WorkingOffline)
            {
                LaunchButton.Enabled = true;
                LaunchButton.Text = @"Запуск";
            }
        }

        private void GetItems()
        {
            SelectProfile.Items.Clear();
            var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            foreach (JProperty peep in json["profiles"])
                SelectProfile.Items.Add(peep.Name);
            if (json["selectedProfile"] != null)
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(json["selectedProfile"].ToString(), true);
            else SelectProfile.SelectedIndex = 0;
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

        private void BackButton_Click(object sender, EventArgs e)
        {
            NewsBrowser.GoBack();
        }

        private void ForwardButton_Click(object sender, EventArgs e)
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
                Nickname.Text = String.Format("Player{0}", DateTime.Now.ToString("HHmmss"));
            }
            SetNullProgressBar();
            ShowProgressBar();
            LaunchButtonChange(LocRm.GetString("launcher.wait"), false);
            try
            {
                File.WriteAllText(Variables.McFolder + "/lastlogin", Nickname.Text);
            }
            catch
            {
            }
            radPageView1.SelectedPage = ConsolePage;
            LaunchButtonClicked(2);
        }

        private void LaunchButtonClicked(int step)
        {
            while (true)
            {
                var profileJson = Processing.GetProfileDetails(SelectProfile.SelectedItem.Text);
                switch (step)
                {
                    case 0:
                    {
                        Variables.UserName = Nickname.Text;
                        string index = null;
                        try
                        {
                            string ver;
                            var obj = JObject.Parse(profileJson);
                            if (obj["lastVersionId"] != null)
                                ver = obj["lastVersionId"].ToString();
                            else
                            {
                                var allowed = (JArray)obj["allowedReleaseTypes"];
                                ver = allowed.ToString().Contains("snapshot")
                                    ? Variables.LastSnapshot
                                    : Variables.LastRelease;
                            }
                            var verJson =
                                JObject.Parse(
                                    File.ReadAllText(String.Format("{0}\\versions\\{1}\\{1}.json", _minecraft, ver)));
                            index = verJson["assets"].ToString();
                        }
                        catch
                        {
                        }
                        if (index != null)
                            CheckResourses(index, 0);
                        else
                            ReformatAssets();
                    }
                        break;
                    case 1:
                        GetVersions();
                        Launch(profileJson);
                        break;
                    case 2:
                    {
                        var webc = new WebClient();
                        var obj = JObject.Parse(profileJson);
                        if (obj["lastVersionId"] != null)
                            _ver = obj["lastVersionId"].ToString();
                        else
                        {
                            var allowed = (JArray) obj["allowedReleaseTypes"];
                            _ver = allowed.ToString().Contains("snapshot")
                                ? Variables.LastSnapshot
                                : Variables.LastRelease;
                        }
                        var jarPath = String.Format("{0}\\versions\\{1}\\{1}.jar", _minecraft, _ver);
                        if (!File.Exists(jarPath))
                        {
                            var path = Path.GetDirectoryName(jarPath);
                            if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
                            progressBar1.Text = String.Format("{0} {1}...",
                                LocRm.GetString("downloader.inprogress"), jarPath);
                            Logging.Info(String.Format("{0} {1}...", LocRm.GetString("downloader.inprogress"), jarPath));
                            webc.DownloadFileCompleted += (sender, e) =>
                            {
                                SetNullProgressBar();
                                LaunchButtonClicked(3);
                            };
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(String.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.jar", _ver)), String.Format("{0}/versions/{1}/{1}.jar", _minecraft, _ver));
                        }
                        else
                        {
                            step = 3;
                            continue;
                        }
                    }
                        break;
                    case 3:
                    {
                        var webc = new WebClient();
                        var obj = JObject.Parse(profileJson);
                        if (obj["lastVersionId"] != null)
                            _ver = obj["lastVersionId"].ToString();
                        else
                        {
                            var allowed = (JArray)obj["allowedReleaseTypes"];
                            _ver = allowed.ToString().Contains("snapshot")
                                ? Variables.LastSnapshot
                                : Variables.LastRelease;
                        }
                        var jsonPath = String.Format("{0}\\versions\\{1}\\{1}.json", _minecraft, _ver);
                        if (!File.Exists(jsonPath))
                        {
                            var path =
                                Path.GetDirectoryName(jsonPath);
                            if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
                            progressBar1.Text = String.Format("{0} {1}...",
                                LocRm.GetString("downloader.inprogress"), jsonPath);
                            Logging.Info(String.Format("{0} {1}...", LocRm.GetString("downloader.inprogress"), jsonPath));
                            webc.DownloadFileCompleted += (sender, e) =>
                            {
                                SetNullProgressBar();
                                LaunchButtonClicked(4);
                            };
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(String.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.json", _ver)), jsonPath);
                        }
                        else
                        {
                            step = 4;
                            continue;
                        }
                    }
                        break;
                    case 4:
                    {
                        var json =
                            JObject.Parse(File.ReadAllText(_minecraft + "/versions/" + _ver + "/" + _ver + ".json"));
                        _nativesFolder = Path.Combine(Variables.McVersions, _ver);
                        Logging.Info(LocRm.GetString("lib.checking"));
                        var sb = new StringBuilder();
                        var gsb = new StringBuilder();
                        var nsb = new StringBuilder();
                        var missing = 0;
                        var all = 0;
                        var jr = (JArray) json["libraries"];
                        for (var i = 0; i < jr.Count; i++)
                        {
                            all++;
                            var temp = json["libraries"][i]["name"].ToString().Split(':');
                            var temp2 = new[]
                            {
                                temp[0], temp[1] + @"\" + temp[2]
                            };
                            var url = json["libraries"][i]["url"];
                            string libFileName;
                            if (json["libraries"][i]["natives"] != null &&
                                json["libraries"][i]["natives"]["windows"] != null)
                            {
                                libFileName = temp2[1].Replace(@"\", "-") + "-" +
                                              json["libraries"][i]["natives"]["windows"] + ".jar";
                                libFileName = libFileName.Replace("${arch}", IntPtr.Size == 8 ? "64" : "32");
                            }
                            else
                            {
                                libFileName = temp2[1].Replace(@"\", "-") + ".jar";
                            }
                            temp2[0] = temp2[0].Replace(".", @"\");
                            var finalPath = Path.Combine(temp2[0], temp2[1], libFileName);
                            if (!finalPath.Contains("natives"))
                            {
                                if (json["libraries"][i]["rules"] != null)
                                {
                                    if (json["libraries"][i]["rules"].Count() < 2 &&
                                        json["libraries"][i]["rules"][0]["action"].ToString() == "allow" &&
                                        json["libraries"][i]["rules"][0]["os"]["name"].ToString() == "windows")
                                    {
                                        gsb.Append(Variables.McFolder + "\\libraries\\" + finalPath + ";");
                                    }
                                    else
                                    {
                                        gsb.Append(Variables.McFolder + "\\libraries\\" + finalPath + ";");
                                    }
                                }
                                else
                                {
                                    gsb.Append(Variables.McFolder + "\\libraries\\" + finalPath + ";");
                                }
                            }
                            else
                            {
                                if (json["libraries"][i]["rules"] != null)
                                {
                                    if (json["libraries"][i]["rules"].Count() < 2 &&
                                        json["libraries"][i]["rules"][0]["action"].ToString() == "allow" &&
                                        json["libraries"][i]["rules"][0]["os"]["name"].ToString() == "windows")
                                    {
                                        gsb.Append(Variables.McFolder + "\\libraries\\" + finalPath + ";");
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
                            var temppath = Path.Combine(_minecraft + "\\libraries", temp2[0], temp2[1], libFileName);
                            if (File.Exists(temppath))
                                continue;
                            missing++;
                            Logging.Error(string.Format("{0}, {1}", temppath, LocRm.GetString("lib.notfound")));
                            sb.Append(url == null ? finalPath + ";" : finalPath + "@" + url + ";");
                        }
                        Logging.Info(String.Format("{0} {1}. {2} {3}", LocRm.GetString("lib.completed1p"), all, LocRm.GetString("lib.completed2p"), missing));
                        var libfinal = sb.ToString();
                        _libs = gsb.ToString();
                        _nativelibs = nsb.ToString().Substring(0, nsb.ToString().Length - 1);
                        if (missing == 0)
                        {
                            step = 0;
                            continue;
                        }
                        _libstodownload = libfinal.Substring(0, libfinal.Length - 1).Split(';');
                        _ltotal = missing;
                        progressBar1.Maximum = _ltotal + 1;
                        DownloadLibs();
                    }
                        break;
                }
                break;
            }
        }

        private string[] _libstodownload;
        private int _ltotal;
        private int _lcur;
        private readonly Stopwatch _lsw = new Stopwatch();

        private void DownloadLibs()
        {
            var temp = _libstodownload[_lcur].Contains('@') ? _libstodownload[_lcur].Split('@') : null;
            var filename = temp == null ? _libstodownload[_lcur] : temp[0];
            var url = temp != null ? temp[1] : null;
            var webc = new WebClient();
            if (!Directory.Exists(_minecraft))
                Directory.CreateDirectory(_minecraft);
            try
            {
                var path = _minecraft + "\\libraries\\" +
                           filename.Replace(Path.GetFileName(filename), String.Empty);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                _sw.Start();
                webc.DownloadFileCompleted += LibCompleted;
                webc.DownloadProgressChanged += ProgressChangedLib;
                Logging.Info(String.Format("{0} {1}...", LocRm.GetString("downloadprocess.downloading"), filename));
                webc.DownloadFileAsync(
                    url == null ? new Uri(String.Format("https://libraries.minecraft.net/{0}", filename)) : new Uri(url + filename),
                    String.Format("{0}\\libraries\\{1}", _minecraft, filename));
            }
            catch (Exception ex)
            {
                Logging.Error(LocRm.GetString("downloader.error") + "\n" + ex);
            }
        }

        private void ProgressChangedLib(object sender, DownloadProgressChangedEventArgs e)
        {
            var filename = _libstodownload[_lcur];
            var downloaded = String.Format("{0} MB's / {1} MB's", (e.BytesReceived/1024d/1024d).ToString("0.00"),
                (e.TotalBytesToReceive/1024d/1024d).ToString("0.00"));
            var speed = String.Format("{0} kb/s", (e.BytesReceived/1024d/_sw.Elapsed.TotalSeconds).ToString("0.00"));
            progressBar1.Text = String.Format("{0} \\libraries\\{1}... [{2} | {3}]",
                LocRm.GetString("downloadprocess.downloading"), filename, speed, downloaded);
        }

        private void LibCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var lname = _libstodownload[_lcur];
            var size = new FileInfo(Variables.McFolder + "\\libraries\\" + lname).Length;
            var completedtext = LocRm.GetString("lib.downloadingcomplete");
            if (completedtext != null)
            {
                completedtext = completedtext.Replace("{0}", lname).Replace("{1}", size.ToString());
                Logging.Info(completedtext);
            }
            if (size <= 1) Logging.Warning("Wrong downloaded library size!");
            _lsw.Reset();
            _lcur++;
            if (_lcur == _ltotal)
            {
                _lcur = 0;
                _ltotal = 0;
                _libstodownload = null;
                LaunchButtonClicked(0);
                progressBar1.Value1 = progressBar1.Maximum;
            }
            else
            {
                try
                {
                    progressBar1.Value1 = _lcur;
                }
                catch
                {
                }
                DownloadLibs();
            }
        }

        private void GetVersions()
        {
            radListView1.Items.Clear();
            Processing.GetVersions(radListView1);
        }

        #region Launch

        private void GetDetails(string jsonraw)
        {
            var json = JObject.Parse(jsonraw);
            if (json["javaArgs"] != null)
                _javaArgs = json["javaArgs"] + " ";
            if (json["javaDir"] != null)
                _javaExec = json["javaDir"].ToString();
            _pName = json["name"].ToString();
            _gameDir = json["gameDir"] != null ? json["gameDir"].ToString() : _minecraft;
            if (json["lastVersionId"] != null)
                LastVersionId = json["lastVersionId"].ToString();
            else
            {
                var allowed = (JArray) json["allowedReleaseTypes"];
                LastVersionId = allowed.ToString().Contains("snapshot") ? Variables.LastSnapshot : Variables.LastRelease;
            }
            if (json["allowedReleaseTypes"] != null)
                foreach (var releaseType in json["allowedReleaseTypes"])
                {
                    _allowedReleaseTypes.Add(releaseType.ToString());
                }
            if (json["launcherVisibilityOnGameClose"] != null)
                switch (json["launcherVisibilityOnGameClose"].ToString())
                {
                    case "close launcher when game starts":
                        Cl = true;
                        break;
                    case "hide launcher and re-open when game closes":
                        Hl = true;
                        break;
                }
            _nativesFolder = Path.Combine(_minecraft + "\\versions", LastVersionId);
            var profileSJson = File.ReadAllText(_nativesFolder + @"\" + LastVersionId + ".json");
            var profilejsono = JObject.Parse(profileSJson);
            _mainClass = profilejsono["mainClass"].ToString();
            _arg = profilejsono["minecraftArguments"].ToString();
            _libs = _libs + _nativesFolder + @"\" + LastVersionId + ".jar";
            var natives = _nativelibs.Split(';');
            try
            {
                foreach (var a in natives)
                {
                    using (var zip = ZipFile.Read(Variables.McFolder + "/libraries/" + a))
                    {
                        zip.ExtractAll(Variables.McFolder + "/natives/",
                            ExtractExistingFileAction.OverwriteSilently);
                    }
                }
                Logging.Info(String.Format("Распаковка natives завершена. Всего {0} файлов", new DirectoryInfo(Variables.McFolder + "/natives/").GetFiles("*.dll",
                            SearchOption.AllDirectories).Length));
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Data.ToString());
            }
        }

        private string _pName;
        private string _gameDir;
        public string LastVersionId;
        private readonly List<string> _allowedReleaseTypes = new List<string>();
        private string _javaArgs = "-Xmx1G "; // default
        private string _nativesFolder = Variables.McVersions;
        private string _libs;
        private string _nativelibs;
        private string _arg;
        private string _ver;
        private string _javaExec = Variables.JavaExe;
        private string _assets = "1.7.4";
        private string _mainClass;
        public bool Hl;
        public bool Cl;

        private void Launch(string profileJson)
        {
            try
            {
                var add = true;
                var jo = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
                var profiles = (JObject)jo["profiles"];
                foreach (JProperty peep in jo["profiles"].Cast<JProperty>().Where(peep => peep.Name == Nickname.Text))
                {
                    add = false;
                    if (jo["profiles"][peep.Name]["type"].ToString() == "pirate")
                    {
                        Variables.AccessToken = "1i1ii1i111ii1i1i1i1i1ii1ii1ii111";
                        Variables.ClientToken = "11i1111i11ii11iii1i1i11iiii11iii";
                    }
                    else
                    {
                        var resp = MakePost.MPostjson(AuthShemes.Authserver + AuthShemes.Validate, new JObject { new JProperty("accessToken", jo["profiles"][peep.Name]["accessToken"]) }.ToString());
                        if (resp.Contains("Error"))
                        {
                            var topost = new JObject
                            {
                                new JProperty("accessToken", jo["profiles"][peep.Name]["accessToken"]),
                                new JProperty("clientToken", jo["profiles"][peep.Name]["clientToken"]),
                                {
                                    "selectedProfile", new JObject
                                    {
                                        new JProperty("id", jo["profiles"][peep.Name]["UUID"]),
                                        {"name", peep.Name}
                                    }
                                }
                            };
                            var response = MakePost.MPostjson(AuthShemes.Authserver + AuthShemes.Validate,
                                topost.ToString());
                            if (!response.Contains("Error"))
                            {
                                var jo1 = JObject.Parse(response);
                                jo["profiles"][peep.Name]["accessToken"] = jo1["accessToken"];
                            }
                        }
                        Variables.AccessToken = jo["profiles"][peep.Name]["accessToken"].ToString();
                        Variables.ClientToken = jo["profiles"][peep.Name]["UUID"].ToString();
                    }
                    break;
                }
                if (add)
                    profiles.Add(Nickname.Text, new JObject { new JProperty("type", "pirate") });
                File.WriteAllText(Variables.McFolder + "/luncher/userprofiles.json", jo.ToString());
                var lselected = Nickname.Text;
                UpdateUserProfiles();
                Nickname.SelectedValue = lselected;
            }
            catch
            {
            }
            HideProgressBar();
            GetDetails(profileJson);
            var va = LogTab("Minecraft version: " + LastVersionId, _pName);
            var mp = new MinecraftProcess(this, _gameDir, _arg, _pName, usingAssets.Text, _javaExec, _libs, _javaArgs, _assets,
                LastVersionId, _mainClass) { Txt = (RichTextBox)va[0], KillButton = (RadButton)va[1], CloseTabButton = (RadButton)va[2] };
            mp.Launch();
        }

        #endregion

        private string _todownload;

        private void CheckResourses(string index, int step)
        {
            while (true)
            {
                _assets = index;
                var webc = new WebClient();
                switch (step)
                {
                    case 0:
                        if (!File.Exists(_minecraft + "/assets/indexes/" + index + ".json"))
                        {
                            var path = _minecraft + "/assets/indexes/";
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            progressBar1.Text = String.Format("{0} {1}{2}.json...",
                                LocRm.GetString("downloader.inprogress"), path, index);
                            Logging.Info(String.Format("{0} {1}{2}.json" + "...", LocRm.GetString("downloader.inprogress"), path, index));
                            _indexcont = index;
                            progressBar1.Maximum = 100;
                            webc.DownloadFileCompleted += Completed;
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(String.Format("https://s3.amazonaws.com/Minecraft.Download/indexes/{0}.json", index)),
                                String.Format("{0}{1}.json", path, index));
                        }
                        else
                        {
                            step = 1;
                            continue;
                        }
                        break;
                    case 1:
                    {
                        _assetstodownload = null;
                        _total = 0;
                        _cur = 0;
                        _indexcont = index;
                        var all = 0;
                        var missing = 0;
                        Logging.Info(LocRm.GetString("resources.checking"));
                        var json = JObject.Parse(File.ReadAllText(_minecraft + "/assets/indexes/" + index + ".json"));
                        foreach (JProperty peep in json["objects"])
                        {
                            all++;
                            var c = json["objects"][peep.Name]["hash"].ToString();
                            char с1 = c[0];
                            char с2 = c[1];
                            var filename = с1.ToString() + с2.ToString() + "/" + json["objects"][peep.Name]["hash"];
                            if (File.Exists(_minecraft + "/assets/objects/" + filename)) continue;
                            missing++;
                            Logging.Warning(
                                String.Format("\\assets\\objects\\{0}, {1}", filename, LocRm.GetString("lib.notfound")));
                            _todownload = _todownload + filename + ";";
                        }
                        Logging.Info(
                            String.Format("{0} {1}. {2} {3}", LocRm.GetString("resources.completed1p"), all, LocRm.GetString("resources.completed2p"), missing));
                        if (missing != 0)
                        {
                            _assetstodownload = _todownload.Substring(0, _todownload.Length - 1).Split(';');
                            _total = missing;
                            progressBar1.Maximum = _total + 1;
                            DownloadResourses1();
                        }
                        else
                            LaunchButtonClicked(1);
                    }
                        break;
                }
                break;
            }
        }

        private string _indexcont;

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            progressBar1.Text = String.Format("{0} {1}/assets/indexes/{2}.json {3}",
                LocRm.GetString("downloadprocess.downloading"), _minecraft, _indexcont,
                LocRm.GetString("downloading.completed"));
            CheckResourses(_indexcont, 1);
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                progressBar1.Maximum = 100;
                progressBar1.Value1 = e.ProgressPercentage;
            }
            catch
            {
            }
        }

        private string[] _assetstodownload;
        private int _total;
        private int _cur;
        private readonly Stopwatch _sw = new Stopwatch();

        private void DownloadResourses1()
        {
            var filename = _assetstodownload[_cur];
            var webc = new WebClient();
            if (!Directory.Exists(_minecraft)) Directory.CreateDirectory(_minecraft);
            try
            {
                var mdir = _minecraft + "\\assets\\objects\\" +
                           filename.Replace(Path.GetFileName(filename), String.Empty);
                if (!Directory.Exists(mdir)) Directory.CreateDirectory(mdir);
                _sw.Start();
                webc.DownloadFileCompleted += ResCompleted;
                webc.DownloadProgressChanged += ProgressChangedRes;
                Logging.Info(
                    String.Format("{0} \\assets\\objects\\{1}...", LocRm.GetString("downloadprocess.downloading"), filename));
                webc.DownloadFileAsync(new Uri(String.Format("http://resources.download.minecraft.net/{0}", filename)),
                    String.Format("{0}\\assets\\objects\\{1}", _minecraft, filename));
            }
            catch (Exception ex)
            {
                Logging.Error(ex.ToString());
            }
        }

        private void ProgressChangedRes(object sender, DownloadProgressChangedEventArgs e)
        {
            var filename = _assetstodownload[_cur];
            var downloaded = string.Format("{0} MB's / {1} MB's", (e.BytesReceived/1024d/1024d).ToString("0.00"),
                (e.TotalBytesToReceive/1024d/1024d).ToString("0.00"));
            var speed = string.Format("{0} kb/s", (e.BytesReceived/1024d/_sw.Elapsed.TotalSeconds).ToString("0.00"));
            progressBar1.Text = String.Format("{0} \\assets\\objects\\{1}... [{2} | {3}]",
                LocRm.GetString("downloadprocess.downloading"), filename, speed, downloaded);
        }

        private void ResCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _sw.Reset();
            _cur++;
            if (_cur == _total)
            {
                _cur = 0;
                _total = 0;
                _assetstodownload = null;
                CheckResourses(_indexcont, 1);
                progressBar1.Value1 = progressBar1.Maximum;
            }
            else
            {
                try
                {
                    progressBar1.Value1 = _cur;
                }
                catch
                {
                }
                DownloadResourses1();
            }
        }

        private void SetNullProgressBar()
        {
            progressBar1.Value1 = 0;
            progressBar1.Value2 = 0;
        }

        private void ShowProgressBar()
        {
            progressBar1.Visible = true;
        }

        private void HideProgressBar()
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
                var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
                var json1 = (JObject) json["profiles"];
                var toparse = "" + json1[SelectProfile.Text];
                var curprofile = JObject.Parse(toparse);
                Console.WriteLine(newprofilename);
                newprofilename = "Copy of " + curprofile["name"] + "(" + newprofilename + ")";
                Logging.Info(String.Format("{0} {1}({2})" + "...", LocRm.GetString("profile.createcopy"), SelectProfile.Text, newprofilename));
                curprofile["name"] = newprofilename;
                Console.WriteLine();
                json1.Add(new JProperty(newprofilename, curprofile));
                File.WriteAllText(Variables.ProfileJsonFile, json.ToString());
                GetItems();
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(newprofilename, true);
                ChangeProgile(false);
            }
            catch (Exception ex)
            {
                Logging.Error(LocRm.GetString("profile.createerror") + "\n" + ex);
            }
        }

        private void ChangeProgile(bool isediting)
        {
            Logging.Info(LocRm.GetString("profile.editing") + " " + SelectProfile.Text + "...");
            var pf = new ProfileForm
            {
                ProfileName = {Text = SelectProfile.Text},
                radButton4 = {Enabled = isediting}
            };
            pf.ShowDialog();
            if (!pf.Deleted)
            {
                SelectProfile.Items.Add(pf.Newprofilename);
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(pf.Newprofilename, true);
                GetItems();
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
                Logging.Info(pf.Canceled != true
                    ? String.Format("{0} {1} {2}", LocRm.GetString("profile.edited.complete1p"), SelectProfile.Text,
                        LocRm.GetString("profile.edited.complete2p"))
                    : LocRm.GetString("profile.delete.canceled"));
            }
            else
            {
                Logging.Info(LocRm.GetString("profile.delete.succes"));
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

        private void ReformatAssets()
        {
            if (AllowReconstruct.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                var all = 0;
                var reconstructed = 0;
                try
                {
                    Logging.Info(LocRm.GetString("resources.reconstructing"));
                    var json =
                        JObject.Parse(
                            File.ReadAllText(String.Format("{0}/assets/indexes/{1}.json", _minecraft, ReconstructingIndex.Text)));
                    foreach (JProperty peep in json["objects"])
                    {
                        all++;
                        var c = json["objects"][peep.Name]["hash"].ToString();
                        var filename = c[0].ToString() + c[1].ToString() + "\\" + json["objects"][peep.Name]["hash"];
                        if (File.Exists(String.Format("{0}/assets/{1}", _minecraft, peep.Name))) continue;
                        Logging.Info(String.Format("{0}/assets/objects/{1} -> {0}/assets/{2}", _minecraft, filename, peep.Name));
                        var path = Path.GetDirectoryName(String.Format("{0}/assets/{1}", _minecraft, peep.Name));
                        if (path != null && (!Directory.Exists(path))) Directory.CreateDirectory(path);
                        File.Copy(String.Format("{0}/assets/objects/{1}", _minecraft, filename), String.Format("{0}/assets/{1}", _minecraft, peep.Name));
                        reconstructed++;
                    }
                    Logging.Info(String.Format("{0} {1}. {2} {3}", LocRm.GetString("resources.recostructionsuccestotal"), all, LocRm.GetString("resources.recostructionsuccestotalrecostructed"), reconstructed));
                }
                catch (Exception ex)
                {
                    Logging.Error(String.Format("{0}\n{1}", LocRm.GetString("resources.reconstructionerror"), ex));
                }
            }
            else
            {
                Logging.Warning(LocRm.GetString("resources.reconstructioncanceled"));
            }
            LaunchButtonClicked(1);
        }

        private void AllowReconstruct_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            ReconstructingIndex.Enabled = AllowReconstruct.ToggleState ==
                                          Telerik.WinControls.Enumerations.ToggleState.On;
        }

        private void EnableMinecraftLogging_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            UseGamePrefix.Enabled = EnableMinecraftLogging.ToggleState !=
                                    Telerik.WinControls.Enumerations.ToggleState.Off;
        }

        private void label6_Click(object sender, EventArgs e)
        {
            Process.Start("http://vk.com/mcoffline");
        }

        private bool _loadedlang;

        private void radDropDownList1_SelectedIndexChanged(object sender,
            PositionChangedEventArgs e)
        {
            Program.Lang = LangDropDownList.SelectedItem.Text.Contains("ru")
                ? ""
                : LangDropDownList.SelectedItem.Tag.ToString();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            if (!_loadedlang) return;
            RadMessageBox.Show(LocRm.GetString("lang.changemessage"), "Language changed", MessageBoxButtons.OK,
                RadMessageIcon.Info);
            Logging.Warning(LocRm.GetString("lang.changemessage"));
        }

        public void CleanNatives()
        {
            if (!Directory.Exists(Variables.McFolder + "/natives")) return;
            Logging.Info("Очистка natives...");
            foreach (var file in Directory.GetFiles(Variables.McFolder + "/natives"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logging.Error(ex.Data.ToString());
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
}
