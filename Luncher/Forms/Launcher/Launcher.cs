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
using System.Threading;
using System.Windows.Forms;
using Ionic.Zip;
using Luncher.Properties;
using Luncher.YaDra4il;
using Newtonsoft.Json.Linq;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using Telerik.WinControls.UI.Data;

namespace Luncher.Forms.Launcher
{
    public partial class Launcher : RadForm
    {
        public Launcher()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            InitializeComponent();
            LoggingConfiguration.LoggingBox = Log;
            var openVer = new RadMenuItem {Text = LocRm.GetString("contextver.open")};
            openVer.Click += (sender, e) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(radListView1.SelectedItem[0].ToString())) return;
                    Process.Start(Variables.McVersions + "/" + radListView1.SelectedItem[0] + "/");
                }
                catch
                {
                }
            };
            VerContext.Items.Add(openVer);
            VerContext.Items.Add(new RadMenuSeparatorItem());
            var delVer = new RadMenuItem {Text = LocRm.GetString("contextver.del")};
            delVer.Click += (sender, e) =>
            {
                try
                {
                    if (string.IsNullOrEmpty(radListView1.SelectedItem[0].ToString())) return;
                    var dr =
                        RadMessageBox.Show(
                            string.Format("{0}({1})?", LocRm.GetString("contextver.del.a"), radListView1.SelectedItem[0]),
                            LocRm.GetString("contextver.del.b"),
                            MessageBoxButtons.YesNo, RadMessageIcon.Question);
                    if (dr != DialogResult.Yes) return;
                    Logging.Info(string.Format("{0} {1}...", LocRm.GetString("contextver.del.progress"), radListView1.SelectedItem[0]));
                    try
                    {
                        Directory.Delete(string.Format("{0}/{1}/", Variables.McVersions, radListView1.SelectedItem[0]), true);
                        GetVersions();
                        GetSelectedVersion(SelectProfile.SelectedItem.Text);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error(string.Format("{0}\n{1}", LocRm.GetString("contextver.del.error"), ex));
                    }
                }
                catch
                {
                }
            };
            VerContext.Items.Add(delVer);
        }

        public readonly ResourceManager LocRm = new ResourceManager("Luncher.Forms.Launcher.Launcher", typeof (Launcher).Assembly);

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
            var jo = new JObject
            {
                {
                    "main", new JObject
                    {
                        {"lang", Program.Lang},
                    }
                },
                {
                    "logging", new JObject
                    {
                        {"enableGameLogging", EnableMinecraftLogging.Checked},
                        {"useGamePrefix", UseGamePrefix.Checked}
                    }
                },
                {
                    "updates", new JObject
                    {
                        {"checkVersionsUpdate", AllowUpdateVersions.Checked},
                        {"checkProgramUpdate", radCheckBox1.Checked},
                        {"enableMinecraftUpdateAlerts", EnableMinecraftUpdateAlerts.Checked}
                    }
                },
                {
                    "resources", new JObject
                    {
                        {"enableReconstruction", AllowReconstruct.Checked},
                        {"assetsDir", usingAssets.Text}
                    }
                }
            };
            File.WriteAllText(string.Format("{0}\\luncher\\configuration.cfg", Program.Minecraft), jo.ToString());
            Application.Exit();
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            var a = ProductVersion.Split('.');
            Text = string.Format("{0} {1}.{2}.{3}", ProductName, a[0], a[1], a[2]);
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
            var lastlogin =
                JObject.Parse(File.ReadAllText(string.Format("{0}/luncher/userprofiles.json", Program.Minecraft)));
            if (lastlogin["selectedUsername"] != null)
                Nickname.SelectedIndex = Nickname.FindString(lastlogin["selectedUsername"].ToString());
            Logging.Info(LocRm.GetString("program.started"));
        }

        private void GetTranslations()
        {
            try
            {
                foreach (var i in Directory.GetDirectories(Application.StartupPath))
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
                var index = -1;
                foreach (var i in LangDropDownList.Items)
                {
                    index++;
                    if (!i.Text.Contains(Program.Lang)) continue;
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

        private void AddUserProfile()
        {
            var lf = new LoginDialog.LoginDialog();
            lf.ShowDialog();
            Logging.Info(lf.Result);
            lf.Dispose();
            UpdateUserProfiles();
            var lastlogin =
                JObject.Parse(File.ReadAllText(string.Format("{0}/luncher/userprofiles.json", Program.Minecraft)));
            if (lastlogin["selectedUsername"] != null)
                Nickname.SelectedIndex = Nickname.FindString(lastlogin["selectedUsername"].ToString());
        }

        private void UpdateUserProfiles()
        {
            var filename = string.Format("{0}/luncher/userprofiles.json", Program.Minecraft);
            if (File.Exists(filename))
            {
                Nickname.Items.Clear();
                var userprofiles = JObject.Parse(File.ReadAllText(filename));
                foreach (JProperty peep in userprofiles["profiles"])
                    Nickname.Items.Add(peep.Name);
            }
            else
                File.WriteAllText(filename, new JObject
                {
                    {"selectedUsername", null},
                    {"profiles", new JObject()}
                }.ToString());
            var lastlogin =
                JObject.Parse(File.ReadAllText(string.Format("{0}/luncher/userprofiles.json", Program.Minecraft)));
            if (lastlogin["selectedUsername"] != null)
                Nickname.SelectedIndex = Nickname.FindString(lastlogin["selectedUsername"].ToString());
        }

        public Dictionary<int, object> LogTab(string text, string profilename)
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
            var verJar = string.Format("{0}\\{1}\\{1}.jar", Variables.McVersions, ver);
            var verJson = string.Format("{0}\\{1}\\{1}.json", Variables.McVersions, ver);
            var state =
                LocRm.GetString(File.Exists(verJar) &&
                                File.Exists(verJson)
                    ? "launcherstate.readytoplay"
                    : "launcherstate.readytodownloadandplay");
            SelectedVersion.Text = string.Format("{0} {1} {2}", LocRm.GetString("launcherstate.readytext"), state, ver);
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
                radPanel2.Visible = false;
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
            if (Nickname.Text == string.Empty)
                Nickname.Text = string.Format("Player{0}", DateTime.Now.ToString("HHmmss"));
            SetNullProgressBar();
            ShowProgressBar();
            LaunchButtonChange(LocRm.GetString("launcher.wait"), false);
            var path = string.Format("{0}/luncher/userprofiles.json", Program.Minecraft);
            var lastlogin =
                JObject.Parse(File.ReadAllText(path));
            lastlogin["selectedUsername"] = Nickname.Text;
            File.WriteAllText(path, lastlogin.ToString());
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
                                var allowed = (JArray) obj["allowedReleaseTypes"];
                                ver = allowed.ToString().Contains("snapshot")
                                    ? Variables.LastSnapshot
                                    : Variables.LastRelease;
                            }
                            var verJson =
                                JObject.Parse(
                                    File.ReadAllText(string.Format("{0}\\versions\\{1}\\{1}.json", Program.Minecraft, ver)));
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
                        var jarPath = string.Format("{0}\\versions\\{1}\\{1}.jar", Program.Minecraft, _ver);
                        if (!File.Exists(jarPath))
                        {
                            var path = Path.GetDirectoryName(jarPath);
                            if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
                            progressBar1.Text = string.Format("{0} {1}...",
                                LocRm.GetString("downloader.inprogress"), jarPath);
                            Logging.Info(string.Format("{0} {1}...", LocRm.GetString("downloader.inprogress"), jarPath));
                            webc.DownloadFileCompleted += (sender, e) =>
                            {
                                SetNullProgressBar();
                                LaunchButtonClicked(3);
                            };
                            progressBar1.Maximum = 100;
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(string.Format(
                                    "https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.jar", _ver)),
                                string.Format("{0}/versions/{1}/{1}.jar", Program.Minecraft, _ver));
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
                            var allowed = (JArray) obj["allowedReleaseTypes"];
                            _ver = allowed.ToString().Contains("snapshot")
                                ? Variables.LastSnapshot
                                : Variables.LastRelease;
                        }
                        var jsonPath = string.Format("{0}\\versions\\{1}\\{1}.json", Program.Minecraft, _ver);
                        if (!File.Exists(jsonPath))
                        {
                            var path =
                                Path.GetDirectoryName(jsonPath);
                            if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
                            progressBar1.Text = string.Format("{0} {1}...",
                                LocRm.GetString("downloader.inprogress"), jsonPath);
                            Logging.Info(string.Format("{0} {1}...", LocRm.GetString("downloader.inprogress"), jsonPath));
                            webc.DownloadFileCompleted += (sender, e) =>
                            {
                                SetNullProgressBar();
                                LaunchButtonClicked(4);
                            };
                            progressBar1.Maximum = 100;
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(
                                    string.Format("https://s3.amazonaws.com/Minecraft.Download/versions/{0}/{0}.json",
                                        _ver)), jsonPath);
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
                            JObject.Parse(
                                File.ReadAllText(string.Format("{0}\\versions\\{1}\\{1}.json", Program.Minecraft, _ver)));
                        Logging.Info(LocRm.GetString("lib.checking"));
                        var templibs = new List<string>(); // libs
                        var tempnatives = new List<string>(); // libs with natives
                        int missing = 0, all = 0;
                        var librariesMissed = new Dictionary<string, string>();
                        using (var thr = new BackgroundWorker())
                        {
                            thr.DoWork += delegate
                            {
                                var jr = (JArray) json["libraries"];
                                foreach (var t in jr)
                                {
                                    all++;
                                    var successfully = false;
                                    var s = t["name"].ToString().Split(':');
                                    if (t["rules"] != null)
                                    {
                                        var ja = (JArray) t["rules"];
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
                                    var natives = string.Empty;
                                    if (t["natives"] != null)
                                        if (t["natives"]["windows"] != null)
                                            natives = t["natives"]["windows"].ToString()
                                                .Replace("${arch}", IntPtr.Size == 8 ? "64" : "32");
                                    var url = string.Empty;
                                    if (t["url"] != null)
                                        url = t["url"].ToString();
                                    var lib =
                                        string.Format(
                                            "{0}\\{1}\\{2}\\{1}-{2}" +
                                            (!string.IsNullOrEmpty(natives) ? "-" + natives : string.Empty) + ".jar",
                                            s[0].Replace('.', '\\'), s[1], s[2]);
                                    if (natives == string.Empty)
                                        templibs.Add(Variables.McFolder + "\\libraries\\" + lib);
                                    else tempnatives.Add(lib);
                                    var temppath = Path.Combine(Program.Minecraft + "\\libraries", lib);
                                    if (File.Exists(temppath))
                                        continue;
                                    Invoke(new Action(() =>
                                    {
                                        missing++;
                                        Logging.Error(string.Format("{0}, {1}", lib,
                                            LocRm.GetString("lib.notfound")));
                                        librariesMissed.Add(lib, url);
                                    }));
                                }
                                Logging.Info(string.Format("{0} {1}. {2} {3}", LocRm.GetString("lib.completed1p"),
                                    all,
                                    LocRm.GetString("lib.completed2p"), missing));
                                Invoke(new Action(() =>
                                {
                                    _libs = templibs;
                                    _nativelibs = tempnatives;
                                }));
                            };
                            thr.RunWorkerCompleted += delegate
                            {
                                if (missing == 0)
                                {
                                    LaunchButtonClicked(0);
                                    return;
                                }
                                progressBar1.Maximum = librariesMissed.Keys.Count;
                                DownloadLibraries(librariesMissed);
                            };
                            thr.RunWorkerAsync();
                        }
                    }
                        break;
                }
                break;
            }
        }

        private void DownloadLibraries(Dictionary<string, string> librariesMissed)
        {
            int librariesWrongSize = 0, total = librariesMissed.Keys.Count(), current = 0;
            foreach (var a in librariesMissed)
            {
                var filename = string.Format("{0}\\libraries\\{1}", Variables.McFolder, a.Key);
                var path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var client = new WebClient();
                var a1 = a.Key;
                var a2 = a.Value;
                client.DownloadFileCompleted += (sender, e) =>
                {
                    current++;
                    progressBar1.Value1 = current;
                    progressBar1.Text = string.Format("Downloading libraries [{0}\\{1}]", current, total);
                    if (new FileInfo(filename).Length <= 1)
                    {
                        Logging.Warning(string.Format("Library {0} downloaded with wrong size!", a1));
                        librariesWrongSize++;
                    }
                    else
                        Logging.Info(string.Format("Finished downloading {0}{1}", a1,
                            (a2 != string.Empty ? " from custom repo " + a1 : string.Empty)));
                    librariesMissed.Remove(a1);
                    if (librariesMissed.Keys.Count != 0) return;
                    Logging.Info("Done. Missed: " + librariesWrongSize);
                    LaunchButtonClicked(0);
                    progressBar1.Value1 = progressBar1.Maximum;
                };
                client.DownloadFileAsync(
                    new Uri((a.Value == string.Empty ? "https://libraries.minecraft.net/" : a.Value) + a.Key),
                    filename);
            }
        }

        private void GetVersions()
        {
            radListView1.Items.Clear();
            Processing.GetVersions(radListView1);
        }

        #region Launch

        private List<string> _libs = new List<string>(), _nativelibs = new List<string>();
        private string _ver, _assets = "1.7.4";

        private void Launch(string profileJson)
        {
            var properties = new JObject();
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
                        properties.Add(new JProperty("luncher", new JArray("228apasna")));
                    }
                    else
                    {
                        var a = new AuthManager
                        {
                            SessionToken = jo["profiles"][peep.Name]["accessToken"].ToString(),
                            Uuid = jo["profiles"][peep.Name]["UUID"].ToString()
                        };
                        var b = a.CheckSessionToken();
                        if (!b)
                        {
                            a.AccessToken = jo["profiles"][peep.Name]["clientToken"].ToString();
                            a.Refresh();
                            jo["profiles"][peep.Name]["accessToken"] = a.SessionToken;
                        }
                        Variables.AccessToken = jo["profiles"][peep.Name]["accessToken"].ToString();
                        Variables.ClientToken = jo["profiles"][peep.Name]["UUID"].ToString();
                        foreach (JObject prop in jo["profiles"][peep.Name]["properties"])
                            properties.Add(new JProperty(prop["name"].ToString(), new JArray(prop["value"])));
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
            catch(Exception ex)
            {
                new RadMessageBoxForm
                {
                    Text = @"Authorization required",
                    MessageText = "Client token or smth else isn't valid. Authorization required?",
                    StartPosition = FormStartPosition.CenterScreen,
                    ButtonsConfiguration = MessageBoxButtons.OK,
                    TopMost = true,
                    MessageIcon = Processing.GetRadMessageIcon(RadMessageIcon.Info),
                    Owner = this,
                    DetailsText = ex.Message,
                }.ShowDialog();
                Variables.AccessToken = "1i1ii1i111ii1i1i1i1i1ii1ii1ii111";
                Variables.ClientToken = "11i1111i11ii11iii1i1i11iiii11iii";
                properties.Add(new JProperty("luncher", new JArray("228apasna")));
            }
            HideProgressBar();
            using (var thr = new BackgroundWorker())
            {
                thr.DoWork += delegate
                {
                    var total = 0;
                    foreach (var a in _nativelibs)
                        try
                        {
                            using (var zip = ZipFile.Read(Variables.McFolder + "\\libraries\\" + a))
                                foreach (var e in zip.Where(e => e.FileName.EndsWith(".dll")))
                                {
                                    total++;
                                    e.Extract(Variables.McFolder + "\\natives\\",
                                        ExtractExistingFileAction.OverwriteSilently);
                                }
                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Smth went wrong due unpacking " + a + ": " + ex.Message);
                        }
                    Logging.Info(string.Format("Распаковка natives завершена. Всего извлечено {0} файлов", total));
                };
                thr.RunWorkerCompleted += delegate
                {
                    var finallibraries = _libs.Aggregate(string.Empty,
                        (current, a) => current + (a + ";"));
                    finallibraries = finallibraries.Substring(0, finallibraries.Length - 1);
                    var mp = new MinecraftProcess(this, usingAssets.Text, finallibraries, _assets, profileJson, properties);
                    mp.Launch();
                };
                thr.RunWorkerAsync();
            }
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
                        var jsonIndex = string.Format("{0}\\assets\\indexes\\{1}.json", Program.Minecraft, index);
                        if (!File.Exists(jsonIndex))
                        {
                            var path = Path.GetDirectoryName(jsonIndex);
                            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                            var text = string.Format("{0} {1}...",
                                LocRm.GetString("downloader.inprogress"), jsonIndex);
                            progressBar1.Text = text;
                            progressBar1.Maximum = 100;
                            Logging.Info(text);
                            webc.DownloadFileCompleted += (sender, e) =>
                            {
                                progressBar1.Text = string.Format("{0} {1} {2}",
                                    LocRm.GetString("downloadprocess.downloading"), jsonIndex,
                                    LocRm.GetString("downloading.completed"));
                                CheckResourses(index, 1);
                            };
                            progressBar1.Maximum = 100;
                            webc.DownloadProgressChanged += ProgressChanged;
                            webc.DownloadFileAsync(
                                new Uri(string.Format("https://s3.amazonaws.com/Minecraft.Download/indexes/{0}.json",
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
                        new Thread(() =>
                        {
                            var json =
                                JObject.Parse(
                                    File.ReadAllText(string.Format("{0}\\assets\\indexes\\{1}.json", Program.Minecraft,
                                        index)));
                            var all = ((JObject) json["objects"]).Count;
                            Logging.Info(LocRm.GetString("resources.checking"));
                            var missedAssets =
                                json["objects"].Cast<JProperty>()
                                    .Select(peep => json["objects"][peep.Name]["hash"].ToString())
                                    .Select(c => c[0].ToString() + c[1].ToString() + "\\" + c)
                                    .Where(
                                        filename =>
                                            !File.Exists(Variables.McFolder + "\\assets\\objects\\" + filename))
                                    .ToList();
                            Logging.Info(
                                    string.Format("{0} {1}. {2} {3}", LocRm.GetString("resources.completed1p"), all,
                                        LocRm.GetString("resources.completed2p"), missedAssets.Count));
                            Invoke(new Action(() =>
                            {
                                if (missedAssets.Count != 0)
                                {
                                    progressBar1.Maximum = missedAssets.Count;
                                    DownloadAssets(missedAssets);
                                }
                                else
                                    LaunchButtonClicked(1);
                            }));
                        }).Start();
                    }
                        break;
                }
                break;
            }
        }
        private void DownloadAssets(List<string> missedAssets)
        {
            int missed = 0, current = 0, total = missedAssets.Count();
            foreach (var a in missedAssets)
            {
                var filename = string.Format("{0}\\assets\\objects\\{1}", Variables.McFolder, a);
                var path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var client = new WebClient();
                var a1 = a;
                client.DownloadFileCompleted += (sender, e) =>
                {
                    current++;
                    progressBar1.Value1 = current;
                    progressBar1.Text = string.Format("Downloading resources [{0}\\{1}]", current, missedAssets.Count());
                    if (new FileInfo(filename).Length <= 1)
                    {
                        missed++;
                        Logging.Warning(string.Format("Resource {0} downloaded with wrong size!", a1));
                    }
                    else
                        Logging.Info(string.Format("Finished downloading {0}", a1));
                    total--;
                    if (total != 0) return;
                    missedAssets = new List<string>();
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
            progressBar1.Text = String.Empty;
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
                var newProfileName = DateTime.Now.ToString("HH:mm:ss");
                var json = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
                var jsonProfile = (JObject) json["profiles"][SelectProfile.Text];
                newProfileName = string.Format("Copy of {0}({1})", jsonProfile["name"], newProfileName);
                Logging.Info(string.Format("{0} {1}({2})" + "...", LocRm.GetString("profile.createcopy"),
                    SelectProfile.Text, newProfileName));
                var newProfile = new JObject(jsonProfile);
                newProfile["name"] = newProfileName;
                ((JObject) json["profiles"]).Add(new JProperty(newProfileName, newProfile));
                File.WriteAllText(Variables.ProfileJsonFile, json.ToString());
                GetItems();
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(newProfileName, true);
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
            var pf = new ProfileForm.ProfileForm
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
                    ? string.Format("{0} {1} {2}", LocRm.GetString("profile.edited.complete1p"), SelectProfile.Text,
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

        private void ReformatAssets()
        {
            if (AllowReconstruct.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                var all = 0;
                var reconstructed = 0;
                try
                {
                    var jsonPath = string.Format("{0}\\assets\\indexes\\legacy.json", Program.Minecraft);
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
                        var filename = string.Format("{0}{1}\\{2}", c[0].ToString(), c[1].ToString(),
                            json["objects"][peep.Name]["hash"]);
                        var newpath = string.Format("{0}\\assets\\objects\\{1}", Program.Minecraft, filename);
                        var oldpath = string.Format("{0}\\assets\\{1}", Program.Minecraft, peep.Name);
                        if (File.Exists(oldpath)) continue;
                        Logging.Info(string.Format("{0} -> {1}", newpath, oldpath));
                        var path = Path.GetDirectoryName(oldpath);
                        if (path != null && (!Directory.Exists(path))) Directory.CreateDirectory(path);
                        if (!File.Exists(newpath))
                            new WebClient().DownloadFile(
                                string.Format(@"http://resources.download.minecraft.net/{0}", filename),
                                oldpath);
                        else File.Copy(newpath, oldpath);
                        reconstructed++;
                    }
                    Logging.Info(string.Format("{0} {1}. {2} {3}", LocRm.GetString("resources.recostructionsuccestotal"),
                        all, LocRm.GetString("resources.recostructionsuccestotalrecostructed"), reconstructed));
                }
                catch (Exception ex)
                {
                    Logging.Error(string.Format("{0}\n{1}", LocRm.GetString("resources.reconstructionerror"), ex.Data));
                }
            }
            else
                Logging.Warning(LocRm.GetString("resources.reconstructioncanceled"));
            LaunchButtonClicked(1);
        }

        private void EnableMinecraftLogging_ToggleStateChanged(object sender, StateChangedEventArgs args)
        {
            UseGamePrefix.Enabled = EnableMinecraftLogging.ToggleState !=
                                    Telerik.WinControls.Enumerations.ToggleState.Off;
        }
        #region Links
        private void label3_Click(object sender, EventArgs e)
        {
            Process.Start("http://vk.com/sesmc");
        }
        private void label5_Click(object sender, EventArgs e)
        {
            Process.Start("http://ru-minecraft.ru");
        }
        private void label6_Click(object sender, EventArgs e)
        {
            Process.Start("http://vk.com/mcoffline");
        }
        #endregion

        private bool _loadedlang;

        private void LangSelection_Changed(object sender,
            PositionChangedEventArgs e)
        {
            Program.Lang = LangDropDownList.SelectedItem.Text.Contains("ru")
                ? string.Empty
                : LangDropDownList.SelectedItem.Tag.ToString();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Program.Lang);
            if (!_loadedlang) return;
            RadMessageBox.Show(LocRm.GetString("lang.changemessage"), "Language changed", MessageBoxButtons.OK,
                RadMessageIcon.Info);
            Logging.Warning(LocRm.GetString("lang.changemessage"));
        }

        public void CleanNatives()
        {
            var path = string.Format("{0}\\natives", Variables.McFolder);
            if (!Directory.Exists(path) || !Directory.GetFiles(path).Any()) return;
            Logging.Info("Очистка natives...");
            foreach (var file in Directory.GetFiles(path))
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Logging.Error(ex.Data.ToString());
                }
        }

        private void VersionList_ItemClick(object sender, ListViewItemEventArgs e)
        {
            radListView1.SelectedItem = e.Item;
        }

        private void AddUserProfile_Click(object sender, EventArgs e)
        {
            AddUserProfile();
            UpdateUserProfiles();
        }
        private void KeyPressed(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                LaunchButton.PerformClick();
        }
    }
}
