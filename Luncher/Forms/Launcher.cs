using System;
using System.Collections.Generic;
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
            if (Variables.ImStillRunning != 0)
            {
                new RadMessageBoxForm
                            {
                                Text = @"Ошибка",
                                MessageText = "Не все процессы Minecraft завершены!",
                                StartPosition = FormStartPosition.CenterScreen,
                                ButtonsConfiguration = MessageBoxButtons.OK,
                                TopMost = true,
                                MessageIcon = Processing.GetRadMessageIcon(RadMessageIcon.Error),
                                Owner = this,
                                DetailsText = null,
                            }.ShowDialog();
                e.Cancel = true;
                return;
            }
            var mainObject = new JObject
            {
                {"lang", Program.Lang},
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
            try
            {
                foreach (var i in Directory.GetDirectories(Application.StartupPath))
                {
                    foreach (var a in Directory.GetFiles(i).Where(a =>
                    {
                        var fileName = Path.GetFileName(a);
                        return fileName != null && fileName.Contains("name");
                    }))
                        LangDropDownList.Items.Add(new RadListDataItem
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
            catch (Exception ex)
            {
                Logging.Error("Couldn't get localization files!\n" + ex.Data);
                LangDropDownList.SelectedIndex = 0;
            }
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
            string ver;
            if (json["profiles"][profile]["lastVersionId"] != null)
                ver = json["profiles"][profile]["lastVersionId"].ToString();
            else
            {
                var allowed = (JArray) json["profiles"][profile]["allowedReleaseTypes"];
                ver = allowed.ToString().Contains("snapshot") ? Variables.LastSnapshot : Variables.LastRelease;
            }
            var verJar = String.Format("{0}\\{1}\\{1}.jar", Variables.McVersions, ver);
            var verJson = String.Format("{0}\\{1}\\{1}.json", Variables.McVersions, ver);
            var state =
                LocRm.GetString(File.Exists(verJar) &&
                                File.Exists(verJson)
                    ? "launcherstate.readytoplay"
                    : "launcherstate.readytodownloadandplay");
            SelectedVersion.Text = String.Format("{0} {1} {2}", LocRm.GetString("launcherstate.readytext"), state, ver);
            if (!File.Exists(verJar) &&
                !File.Exists(verJson) &&
                Variables.WorkingOffline)
            {
                LaunchButton.Enabled = false;
                LaunchButton.Text = @"Недоступно";
            }
            else if (File.Exists(verJar) &&
                     File.Exists(verJson) &&
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
            if (Nickname.Text == String.Empty)
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
                            JObject.Parse(File.ReadAllText(String.Format("{0}\\versions\\{1}\\{1}.json", _minecraft, _ver)));
                        _nativesFolder = Path.Combine(Variables.McVersions, _ver);
                        Logging.Info(LocRm.GetString("lib.checking"));
                        var gsb = new StringBuilder(); // libs
                        var nsb = new StringBuilder(); // libs with natives
                        var missing = 0;
                        var all = 0;
                        var jr = (JArray)json["libraries"];
                        foreach (var t in jr)
                        {
                            all++;
                            var successfully = false;
                            var s = t["name"].ToString().Split(':');
                            if (t["rules"] != null)
                            {
                                var ja = (JArray)t["rules"];
                                if (ja.Count > 1)
                                    for (var j = 0; j < ja.Count; j++)
                                    {
                                        if (ja[j]["action"].ToString() == "allow")
                                        {
                                            if (ja[j]["os"] != null)
                                                if (ja[j]["os"]["name"].ToString() != "windows")
                                                    continue;
                                            j++;
                                        }
                                        if (ja[j] != null)
                                            if (ja[j]["action"].ToString() == "disallow")
                                            {
                                                if (ja[j]["os"] == null) continue;
                                                if (ja[j]["os"]["name"].ToString() == "windows") continue;
                                            }
                                        successfully = true;
                                    }
                                else
                                {
                                    switch (ja[0]["action"].ToString())
                                    {
                                        case "allow":
                                            if (ja[0]["os"] != null)
                                                if (ja[0]["os"]["name"].ToString() != "windows")
                                                    continue;
                                            break;
                                        case "disallow":
                                            if (ja[0]["os"] == null) continue;
                                            if (ja[0]["os"]["name"].ToString() == "windows") continue;
                                            break;
                                    }
                                    successfully = true;
                                }
                            }
                            else
                                successfully = true;
                            if (successfully == false) continue;
                            var natives = String.Empty;
                            if (t["natives"] != null)
                                if (t["natives"]["windows"] != null)
                                    natives = t["natives"]["windows"].ToString()
                                        .Replace("${arch}", IntPtr.Size == 8 ? "64" : "32");
                            var url = String.Empty;
                            if (t["url"] != null)
                                url = t["url"].ToString();
                            var lib =
                                String.Format(
                                    "{0}\\{1}\\{2}\\{1}-{2}" +
                                    (!String.IsNullOrEmpty(natives) ? "-" + natives : String.Empty) + ".jar",
                                    s[0].Replace('.', '\\'), s[1], s[2]);
                            if (natives == String.Empty)
                                gsb.AppendFormat("{0}\\libraries\\{1};", Variables.McFolder, lib);
                            else nsb.AppendLine(lib + ";");
                            var temppath = Path.Combine(_minecraft + "\\libraries", lib);
                            if (File.Exists(temppath))
                                continue;
                            missing++;
                            Logging.Error(string.Format("{0}, {1}", temppath, LocRm.GetString("lib.notfound")));
                            _librariesMissed.Add(lib, url);
                        }
                        Logging.Info(String.Format("{0} {1}. {2} {3}", LocRm.GetString("lib.completed1p"), all, LocRm.GetString("lib.completed2p"), missing));
                        _libs = gsb.ToString();
                        _nativelibs = nsb.ToString().Substring(0, nsb.ToString().Length - 1);
                        if (missing == 0)
                        {
                            step = 0;
                            continue;
                        }
                        progressBar1.Maximum = _librariesMissed.Keys.Count;
                        DownloadLibraries();
                    }
                        break;
                }
                break;
            }
        }

        private readonly Dictionary<string, string> _librariesMissed = new Dictionary<string, string>();
        private void DownloadLibraries()
        {
            var total = _librariesMissed.Keys.Count();
            var current = 0;
            var librariesWrongSize = 0;
            foreach (var a in _librariesMissed)
            {
                var filename = String.Format("{0}\\libraries\\{1}", Variables.McFolder, a.Key);
                var path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var client = new WebClient();
                client.DownloadFileCompleted += (sender, e) =>
                {
                    current++;
                    progressBar1.Value1 = current;
                    progressBar1.Text = String.Format("Downloading libraries [{0}\\{1}]", current, total);
                    if (new FileInfo(filename).Length <= 1)
                    {
                        Logging.Warning(String.Format("Library {0} downloaded with wrong size!", a.Key));
                        librariesWrongSize++;
                    }
                    else
                        Logging.Info(String.Format("Finished downloading {0}{1}", a.Key,
                            (a.Value != String.Empty ? " from custom repo " + a.Value : String.Empty)));
                    _librariesMissed.Remove(a.Key);
                    if (_librariesMissed.Keys.Count != 0) return;
                    Logging.Info("Done. Missed: " + librariesWrongSize);
                    LaunchButtonClicked(0);
                    progressBar1.Value1 = progressBar1.Maximum;
                };
                client.DownloadFileAsync(
                    new Uri((a.Value == String.Empty ? "https://libraries.minecraft.net/" : a.Value) + a.Key),
                    filename);
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
                _lastVersionId = json["lastVersionId"].ToString();
            else
            {
                var allowed = (JArray) json["allowedReleaseTypes"];
                _lastVersionId = allowed.ToString().Contains("snapshot") ? Variables.LastSnapshot : Variables.LastRelease;
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
            string ip = null;
            string port = null;
            if (json["server"] != null)
            {
                ip = json["server"]["ip"].ToString();
                port = json["server"]["port"] != null ? json["server"]["port"].ToString() : null;
            }
            _nativesFolder = Path.Combine(_minecraft + "\\versions", _lastVersionId);
            var profileSJson = File.ReadAllText(_nativesFolder + @"\" + _lastVersionId + ".json");
            var profilejsono = JObject.Parse(profileSJson);
            _mainClass = profilejsono["mainClass"].ToString();
            _arg = profilejsono["minecraftArguments"] + (ip != null ? String.Format(" --server {0} --port {1}", ip, (port ?? "25565")) : String.Empty);
            _libs = String.Format("{0}{1}\\{2}.jar", _libs, _nativesFolder, _lastVersionId);
            var natives = _nativelibs.Split(';');
            try
            {
                foreach (var a in natives)
                    using (var zip = ZipFile.Read(Variables.McFolder + "/libraries/" + a))
                        zip.ExtractAll(Variables.McFolder + "/natives/",
                            ExtractExistingFileAction.OverwriteSilently);
                Logging.Info(String.Format("Распаковка natives завершена. Всего {0} файлов", new DirectoryInfo(Variables.McFolder + "/natives/").GetFiles("*.dll",
                            SearchOption.AllDirectories).Length));
            }
            catch (Exception ex)
            {
                Logging.Error("Smth went wrong: " + ex.Data);
            }
        }

        private string _pName;
        private string _gameDir;
        private string _lastVersionId;
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
                var txt = Variables.McFolder + "/luncher/userprofiles.json";
                var jo = JObject.Parse(File.ReadAllText(txt));
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
                File.WriteAllText(txt, jo.ToString());
                var lselected = Nickname.Text;
                UpdateUserProfiles();
                Nickname.SelectedValue = lselected;
            }
            catch
            {
            }
            HideProgressBar();
            GetDetails(profileJson);
            var va = LogTab("Minecraft version: " + _lastVersionId, _pName);
            var mp = new MinecraftProcess(this, _gameDir, _arg, _pName, usingAssets.Text, _javaExec, _libs, _javaArgs, _assets,
                _lastVersionId, _mainClass) { Txt = (RichTextBox)va[0], KillButton = (RadButton)va[1], CloseTabButton = (RadButton)va[2] };
            mp.Launch();
        }

        #endregion

        private void CheckResourses(string index, int step)
        {
            while (true)
            {
                _assets = index;
                var webc = new WebClient();
                switch (step)
                {
                    case 0:
                    {
                        var jsonIndex = String.Format("{0}\\assets\\indexes\\{1}.json", _minecraft, index);
                        if (!File.Exists(jsonIndex))
                        {
                            var path = Path.GetDirectoryName(jsonIndex);
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            var text = String.Format("{0} {1}...",
                                LocRm.GetString("downloader.inprogress"), jsonIndex);
                            progressBar1.Text = text;
                            progressBar1.Maximum = 100;
                            Logging.Info(text);
                            webc.DownloadFileCompleted += (sender, e) =>
                            {
                                progressBar1.Text = String.Format("{0} {1} {2}",
                                    LocRm.GetString("downloadprocess.downloading"), jsonIndex,
                                    LocRm.GetString("downloading.completed"));
                                CheckResourses(index, 1);
                            };
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(String.Format("https://s3.amazonaws.com/Minecraft.Download/indexes/{0}.json",
                                    index)), jsonIndex);
                        }
                        else
                        {
                            step = 1;
                            continue;
                        }
                    }
                        break;
                    case 1:
                    {
                        var json = JObject.Parse(File.ReadAllText(String.Format("{0}\\assets\\indexes\\{1}.json", _minecraft, index)));
                        var all = ((JObject) json["objects"]).Count;
                        Logging.Info(LocRm.GetString("resources.checking"));
                        foreach (
                            var filename in
                                json["objects"].Cast<JProperty>()
                                    .Select(peep => json["objects"][peep.Name]["hash"].ToString())
                                    .Select(c => c[0].ToString() + c[1].ToString() + "\\" + c)
                                    .Where(
                                        filename => !File.Exists(Variables.McFolder + "\\assets\\objects\\" + filename))
                            )
                            _missedAssets.Add(filename);
                        Logging.Info(
                            String.Format("{0} {1}. {2} {3}", LocRm.GetString("resources.completed1p"), all, LocRm.GetString("resources.completed2p"), _missedAssets.Count));
                        if (_missedAssets.Count != 0)
                        {
                            progressBar1.Maximum = _missedAssets.Count;
                            DownloadAssets();
                        }
                        else
                            LaunchButtonClicked(1);
                    }
                        break;
                }
                break;
            }
        }
        private List<string> _missedAssets = new List<string>();
        private void DownloadAssets()
        {
            var missed = 0;
            var current = 0;
            var total = _missedAssets.Count();
            foreach (var a in _missedAssets)
            {
                var filename = String.Format("{0}\\assets\\objects\\{1}", Variables.McFolder, a);
                var path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var client = new WebClient();
                client.DownloadFileCompleted += (sender, e) =>
                {
                    current++;
                    progressBar1.Value1 = current;
                    progressBar1.Text = String.Format("Downloading resources [{0}\\{1}]", current, _missedAssets.Count());
                    if (new FileInfo(filename).Length <= 1)
                    {
                        missed++;
                        Logging.Warning(String.Format("Resource {0} downloaded with wrong size!", a));
                    }
                    else
                        Logging.Info(String.Format("Finished downloading {0}", a));
                    total--;
                    if (total != 0) return;
                    _missedAssets = new List<string>();
                    Logging.Info("Done. Missed: " + missed);
                    LaunchButtonClicked(1);
                };
                client.DownloadFileAsync(
                    new Uri("http://resources.download.minecraft.net/" + a),
                    filename);
            }
        }
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (progressBar1.Maximum != 100)
                progressBar1.Maximum = 100;
            progressBar1.Value1 = e.ProgressPercentage;
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
                var toparse = json1[SelectProfile.Text].ToString();
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
                    var jsonPath = String.Format("{0}\\assets\\indexes\\legacy.json", _minecraft);
                    var jsonDir = Path.GetDirectoryName(jsonPath);
                    if (!Directory.Exists(jsonDir)) Directory.CreateDirectory(jsonDir);
                    if (!File.Exists(jsonPath))
                        new WebClient().DownloadFile(
                            @"https://s3.amazonaws.com/Minecraft.Download/indexes/legacy.json", jsonPath);
                    Logging.Info(LocRm.GetString("resources.reconstructing"));
                    var json =
                        JObject.Parse(
                            File.ReadAllText(jsonPath));
                    foreach (JProperty peep in json["objects"])
                    {
                        all++;
                        var c = json["objects"][peep.Name]["hash"].ToString();
                        var filename = String.Format("{0}{1}\\{2}", c[0].ToString(), c[1].ToString(), json["objects"][peep.Name]["hash"]);
                        var newpath = String.Format("{0}\\assets\\objects\\{1}", _minecraft, filename);
                        var oldpath = String.Format("{0}\\assets\\{1}", _minecraft, peep.Name);
                        if (File.Exists(oldpath)) continue;
                        Logging.Info(String.Format("{0} -> {1}", newpath, oldpath));
                        var path = Path.GetDirectoryName(oldpath);
                        if (path != null && (!Directory.Exists(path))) Directory.CreateDirectory(path);
                        if (!File.Exists(newpath))
                            new WebClient().DownloadFile(
                                String.Format(@"http://resources.download.minecraft.net/{0}", filename),
                                oldpath);
                        else File.Copy(newpath, oldpath);
                        reconstructed++;
                    }
                    Logging.Info(String.Format("{0} {1}. {2} {3}", LocRm.GetString("resources.recostructionsuccestotal"), all, LocRm.GetString("resources.recostructionsuccestotalrecostructed"), reconstructed));
                }
                catch (Exception ex)
                {
                    Logging.Error(String.Format("{0}\n{1}", LocRm.GetString("resources.reconstructionerror"), ex.Data));
                }
            }
            else
            {
                Logging.Warning(LocRm.GetString("resources.reconstructioncanceled"));
            }
            LaunchButtonClicked(1);
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
            var path = String.Format("{0}\\natives", Variables.McFolder);
            if (!Directory.Exists(path)) return;
            Logging.Info("Очистка natives...");
            foreach (var file in Directory.GetFiles(path))
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

        private void radButton3_Click(object sender, EventArgs e)
        {
            AddUserProfile();
            UpdateUserProfiles();
        }
    }
}
