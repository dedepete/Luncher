using Ionic.Zip;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Threading.Tasks;
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
        }

        [DllImport("user32.dll")]
        static extern int SetWindowText(IntPtr hWnd, string text);

        ResourceManager LocRM = new ResourceManager("Luncher.Launcher", typeof(Launcher).Assembly);

        string minecraft = Program.minecraft;

        private void Launcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
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
            MLog(LocRM.GetString("program.started"));
        }

        void GetTranslations()
        {
            foreach (string i in Directory.GetDirectories(Application.StartupPath))
            {
                foreach (string a in Directory.GetFiles(i))
                {
                    if (Path.GetFileName(a).Contains("name"))
                    {
                        LangDropDownList.Items.Add(new RadListDataItem
                        {
                            Text = Path.GetFileNameWithoutExtension(a) + " (" + i.Substring(i.LastIndexOf(Path.DirectorySeparatorChar) + 1) + ")",
                            Tag = i.Substring(i.LastIndexOf(Path.DirectorySeparatorChar) + 1)
                        });
                    }
                }
            }
            int index = -1;
            foreach (RadListDataItem i in LangDropDownList.Items)
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

        public void MLogG(string text, bool iserror)
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
            if (UseGamePrefix.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                line = "[GAME]" + text + "\n";
            }
            else
            {
                line = text + "\n";
            }
            int start = Log.TextLength;
            Log.AppendText(line);
            int end = Log.TextLength;
            Log.Select(start, end - start);
            {
                Log.SelectionColor = color;
            }
            Log.SelectionLength = 0;
            Log.ScrollToCaret();
        }

        public void WLog(string text)
        {
            int start = Log.TextLength;
            Log.AppendText("[Luncher][WARNING][" + DateTime.Now.ToString("dd-MM-yy HH:mm:ss") + "] " + text + "\n");
            int end = Log.TextLength;
            Log.Select(start, end - start);
            {
                Log.SelectionColor = Color.Orange;
            }
            Log.SelectionLength = 0; 
            Log.ScrollToCaret();
        }

        public void ELog(string text)
        {
            int start = Log.TextLength;
            Log.AppendText("[Luncher][ERROR][" + DateTime.Now.ToString("dd-MM-yy HH:mm:ss") + "] " + text + "\n");
            int end = Log.TextLength;
            Log.Select(start, end - start);
            {
                Log.SelectionColor = Color.Red;
            }
            Log.SelectionLength = 0;
            Log.ScrollToCaret();
        }
        #endregion

        void ShowReport(string text, string profilename)
        {
            RadPageViewPage report = new RadPageViewPage();
            RadButton closebutton = new RadButton();
            RadPanel panel = new RadPanel();
            RichTextBox reportbox = new RichTextBox();
            panel.Text = "Text";
            panel.Dock = DockStyle.Top;
            panel.Size = new Size(panel.Size.Width, 54);
            closebutton.Text = "Закрыть";
            closebutton.Location = new Point(panel.Size.Width - (closebutton.Size.Width + 5), 5);
            closebutton.Anchor = (AnchorStyles.Right | AnchorStyles.Top);
            closebutton.Click += new EventHandler(closetab);
            closebutton.Tag = report;
            report.Text = "Log: " + profilename;
            reportbox.Dock = DockStyle.Fill;
            reportbox.ReadOnly = true;
            reportbox.Text = text;
            panel.Controls.Add(closebutton);
            report.Controls.Add(reportbox);
            report.Controls.Add(panel);
            radPageView1.Pages.Add(report);
            radPageView1.SelectedPage = report;
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
                JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
                json["selectedProfile"] = SelectProfile.SelectedItem.Text;
                File.WriteAllText(Variables.localProfileList, json.ToString());
            }
            catch { }
        }

        private void GetSelectedVersion(string profile)
        {
            JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            string state;
            string ver1 = null;
            try
            {
                ver1 = json["profiles"][profile]["lastVersionId"].ToString();
            }
            catch
            {
                JArray allowed = (JArray)json["profiles"][profile]["allowedReleaseTypes"];
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
            JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
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
                if (NewsBrowser.CanGoBack == true)
                {
                    radButton2.Enabled = true;
                }
                else
                {
                    radButton2.Enabled = false;
                }
                if (NewsBrowser.CanGoForward == true)
                {
                    radButton1.Enabled = true;
                }
                else
                {
                    radButton1.Enabled = false;
                }
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
            string profileJSON = Processing.GetProfileDetails(this.SelectProfile.SelectedItem.Text);
            if (step == 0)
            {
                Variables.userName = Nickname.Text;
                string index = null;
                try
                {
                    string ver = null;
                    JObject obj = JObject.Parse(profileJSON);
                    try
                    {
                        ver = obj["lastVersionId"].ToString();
                    }
                    catch
                    {
                        JArray allowed = (JArray)obj["lastVersionId"].ToString();
                        if (allowed.ToString().Contains("snapshot"))
                        {
                            ver = Variables.lastSnapshot;
                        }
                        else
                        {
                            ver = Variables.lastRelease;
                        }
                    }
                    JObject verJSON = JObject.Parse(File.ReadAllText(minecraft + "\\versions\\" + ver + "\\" + ver + ".json"));
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
            else if (step == 1)
            {
                GetVersions();
                Launch(profileJSON);
            }
            else if (step == 2)
            {
                WebClient webc = new WebClient();
                JObject obj = JObject.Parse(profileJSON); 
                try
                {
                    ver = obj["lastVersionId"].ToString();
                }
                catch
                {
                    JArray allowed = (JArray)obj["allowedReleaseTypes"];
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
                    webc.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedVerJar);
                    webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/" + ver + "/" + ver + ".jar"), minecraft + "/versions/" + ver + "/" + ver + ".jar");
                }
                else
                {
                    LaunchButtonClicked(3);
                }
            }
            else if (step == 3)
            {
                WebClient webc = new WebClient();
                JObject obj = JObject.Parse(profileJSON);
                try
                {
                    ver = obj["lastVersionId"].ToString();
                }
                catch
                {
                    JArray allowed = (JArray)obj["allowedReleaseTypes"];
                    if (allowed.ToString().Contains("snapshot"))
                    {
                        ver = Variables.lastSnapshot;
                    }
                    else
                    {
                        ver = Variables.lastRelease;
                    }
                }
                if (!File.Exists(minecraft + "/versions/" + ver + "/" + ver + ".json"))
                {
                    try { Directory.CreateDirectory(Path.GetDirectoryName(minecraft + "/versions/" + ver + "/" + ver + ".json")); }
                    catch { }
                    progressBar1.Text = LocRM.GetString("downloader.inprogress") + " " + minecraft + "/versions/" + ver + "/" + ver + ".json" + "...";
                    MLog(LocRM.GetString("downloader.inprogress") + " " + minecraft + "/versions/" + ver + "/" + ver + ".json" + "...");
                    webc.DownloadFileCompleted += new AsyncCompletedEventHandler(CompletedVerJson);
                    webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/" + ver + "/" + ver + ".json"), minecraft + "/versions/" + ver + "/" + ver + ".json");
                }
                else
                {
                    LaunchButtonClicked(4);
                }
            }
            else if (step == 4)
            {
                JObject json = JObject.Parse(File.ReadAllText(minecraft + "/versions/" + ver + "/" + ver + ".json"));
                nativesFolder = Path.Combine(minecraft + "/versions", ver);
                string profileSJson = File.ReadAllText(nativesFolder + @"\" + ver + ".json");
                MLog(LocRM.GetString("lib.checking"));
                StringBuilder sb = new StringBuilder();
                StringBuilder gsb = new StringBuilder();
                StringBuilder nsb = new StringBuilder();
                string[] temp2 = new string[2];
                string finalPath;
                string libFileName = "";
                int missing = 0;
                int all = 0;
                JArray jr = (JArray)json["libraries"];
                for (int i = 0; i < jr.Count; i++)
                {
                    all++;
                    temp2[0] = json["libraries"][i]["name"].ToString().Split(':')[0]; // will be net.sf.jopt-simple
                    temp2[1] = json["libraries"][i]["name"].ToString().Split(':')[1] + @"\" + json["libraries"][i]["name"].ToString().Split(':')[2]; // will be jopt-simple\4.5
                    if (json["libraries"][i]["natives"] != null)
                    {
                        if (json["libraries"][i]["natives"]["windows"] != null)
                        {
                            libFileName = temp2[1].Replace(@"\", "-") + "-" +json["libraries"][i]["natives"]["windows"] + ".jar";
                            if (IntPtr.Size == 8)
                            {
                                libFileName = libFileName.Replace("${arch}", "64");
                            }
                            else
                            {
                                libFileName = libFileName.Replace("${arch}", "32");
                            }
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
                        nsb.Append(finalPath + ";");
                    }
                    if (!File.Exists(Path.Combine(minecraft + "/libraries", temp2[0], temp2[1], libFileName)))
                    {
                        missing++;
                        ELog(Path.Combine(minecraft + "/libraries", temp2[0], temp2[1], libFileName) + ", " + LocRM.GetString("lib.notfound"));
                        sb.Append(finalPath + ";");
                    }
                }
                MLog(LocRM.GetString("lib.completed1p") + " " + all + ". " + LocRM.GetString("lib.completed2p") + " " + missing);
                string libfinal = sb.ToString();
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
        void DownloadLibs()
        {
            string filename = libstodownload[lcur];
            WebClient webc = new WebClient();
            if (Directory.Exists(minecraft) == false)
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
                webc.DownloadFileCompleted += new AsyncCompletedEventHandler(LibCompleted);
                webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedLib);
                MLog(LocRM.GetString("downloadprocess.downloading") + " " + filename + "...");
                webc.DownloadFileAsync(new Uri("https://libraries.minecraft.net/" + filename), minecraft + "\\libraries\\" + filename);
            }
            catch (Exception ex)
            {
                ELog(LocRM.GetString("downloader.error") + "\n" + ex.ToString());
            }
        }
        private void ProgressChangedLib(object sender, DownloadProgressChangedEventArgs e)
        {
            string filename = libstodownload[lcur];
            string downloaded = string.Format("{0} MB's / {1} MB's", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            string speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
            progressBar1.Text = LocRM.GetString("downloadprocess.downloading") + " \\libraries\\" + filename + "...  [" + speed + " | " + downloaded + "]";
        }
        private void LibCompleted(object sender, AsyncCompletedEventArgs e)
        {
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
            foreach (string s in Directory.GetDirectories(Variables.MCFolder + "/versions/"))
            {
                string versionname = new DirectoryInfo(s).Name;
                if (File.Exists(Variables.MCFolder + "/versions/" + versionname + "/" + versionname + ".jar") & File.Exists(Variables.MCFolder + "/versions/" + versionname + "/" + versionname + ".json"))
                {
                    JObject json = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/versions/" + versionname + "/" + versionname + ".json"));
                    string id = "null";
                    string type = "null";
                    string time = "null";
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

        private int tflood = 0;
        string tlast = null;
        private int eflood = 0;
        string elast = null;

        private void t_reader()
        {
            while (true)
            {
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
                            if (line.Contains("Attempting early MinecraftForge initialization") & rnw == true)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    rnw = false;
                                    this.MLog("[Forge]Инициализация Minecraft Forge...");
                                });
                            }
                            if (line.Contains("Sound engine started") & rnw == false)
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    rnw = true;
                                    this.MLog("[Forge]Инициализация Minecraft Forge закончена");
                                });
                            }
                            if (tflood < 3)
                            {
                                this.Invoke((MethodInvoker) delegate
                                {
                                    this.MLogG(line, false);
                                });
                            }
                            if (rnw == true)
                            {
                                if (RenameWindow.SelectedIndex == 0)
                                {
                                    SetWindowText(Client.MainWindowHandle,
                                        "Minecraft - " + lastVersionID + " - " + ProductName + " " + ProductVersion);
                                }
                                else if (RenameWindow.SelectedIndex == 2)
                                {
                                    SetWindowText(Client.MainWindowHandle, "Minecraft");
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
                try
                {
                    string line = "";
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
                            this.Invoke((MethodInvoker)delegate
                            {
                                rnw = false;
                                this.MLog("[Forge]Инициализация Minecraft Forge...");
                            });
                        }
                        if (line.Contains("Sound engine started"))
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                rnw = true;
                                this.MLog("[Forge]Инициализация Minecraft Forge закончена");
                            });
                        }
                        try
                        {
                            if (eflood < 3)
                            {
                                this.Invoke((MethodInvoker) delegate
                                {
                                    this.MLogG(line, true);
                                });
                            }
                            if (rnw == true)
                            {
                                if (RenameWindow.SelectedIndex == 0)
                                {
                                    SetWindowText(Client.MainWindowHandle,
                                        "Minecraft - " + lastVersionID + " - " + ProductName + " " + ProductVersion);
                                }
                                else if (RenameWindow.SelectedIndex == 2)
                                {
                                    SetWindowText(Client.MainWindowHandle, "Minecraft");
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
        private static Process Client;
        void GetDetails(string jsonraw)
        {
            JObject json = JObject.Parse(jsonraw);
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
                JArray allowed = (JArray)json["allowedReleaseTypes"];
                if (allowed.ToString().Contains("snapshot"))
                {
                    lastVersionID = Variables.lastSnapshot;
                }
                else
                {
                    lastVersionID = Variables.lastRelease;
                }
            }
            if (json["allowedReleaseTypes"] != null)
            {
                foreach (JValue releaseType in json["allowedReleaseTypes"])
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
            string profileSJson = File.ReadAllText(nativesFolder + @"\" + lastVersionID + ".json");
            JObject profilejsono = JObject.Parse(profileSJson);
            Variables.mainClass = profilejsono["mainClass"].ToString();
            arg = profilejsono["minecraftArguments"].ToString();
            libs = libs + nativesFolder + @"\" + lastVersionID + ".jar";
            string[] natives = nativelibs.Split(';');
            try
            {
                for (int e = 0; e < natives.Length; e++)
                {
                    using (ZipFile zip = ZipFile.Read(Variables.MCFolder + "/libraries/" + natives[e]))
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
        string pName = null;
        string gameDir = null;
        string lastVersionID = null;
        List<string> allowedReleaseTypes = new List<string>();
        string javaArgs = "-Xmx1G "; // default
        string nativesFolder = Variables.MCVersions;
        List<string> reqLibs = new List<string>();
        string binNatives = Path.Combine(Variables.MCFolder, "bin");
        string libs = null;
        string nativelibs = null;
        string arg = null;
        string ver = null;
        string javaExec = Variables.javaExe;
        string assets = "1.7.4";
        bool hl = false;
        bool cl = false;
        void Launch(string profileJSON)
        {
            HideProgressBar();
            Client = new Process();
            GetDetails(profileJSON);
            MLog(LocRM.GetString("launch.text1p") + " " + lastVersionID + " (" + LocRM.GetString("launch.text2p") + " " + pName + ")");
            //Processing.CopyDirectory(binNatives, nativesFolder);
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.UseShellExecute = false;
            proc.RedirectStandardOutput = true;
            proc.RedirectStandardError = true;
            proc.WindowStyle = ProcessWindowStyle.Hidden;
            proc.CreateNoWindow = true;
            proc.FileName = javaExec;
            proc.WorkingDirectory = gameDir;
            string assetspath = usingAssets.Text;
            string nativespath = "-Djava.library.path=" + minecraft + "/natives";
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
            arg = arg.Replace("${auth_session}", "token:luncher");
            arg = arg.Replace("${auth_access_token}", "token:luncher");
            arg = arg.Replace("${auth_uuid}", "1111174");
            arg = arg.Replace("${user_properties}", "{\"luncher\":[1234]}");
            arg = arg.Replace("${AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            arg = arg.Replace("\\\"", "\"");
            string token = Variables.accessToken + ":" + Variables.clientToken;
            proc.Arguments = javaArgs + nativespath + " -cp " + libs + " " + Variables.mainClass + " " + arg;
            proc.StandardErrorEncoding = Encoding.UTF8;
            Client.StartInfo = proc;
            MLog(LocRM.GetString("launch.workingdir")+" " + gameDir);
            MLog(LocRM.GetString("launch.command")+" " + proc.FileName + " " + proc.Arguments);
            try
            {
                LaunchButtonChange(LocRM.GetString("launch.launchtext"), true);
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
                Client.EnableRaisingEvents = true;
                Client.Exited += new EventHandler(Client_Exited);
                Client.Start();
                if (EnableMinecraftLogging.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                {
                    Reader = new Thread(new ThreadStart(t_reader));
                    Reader.Start();
                }
                ErrorReader = new Thread(new ThreadStart(e_reader));
                ErrorReader.Start();
                if (hl == true)
                {
                    this.WindowState = FormWindowState.Minimized;
                }
                if (cl == true)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                ELog(LocRM.GetString("launch.error") + "\n" + ex.ToString());
            }
        }
        #endregion
        private void Client_Exited(object sender, EventArgs e)
        {
            var proc = sender as Process;
            this.Invoke((MethodInvoker)delegate
            {
                CleanNatives();
                this.MLogG(("Процесс был завершён с кодом " + proc.ExitCode + ". Сеанс с " + proc.StartTime.ToString("HH:mm:ss") + "(Всего" + (Math.Round(proc.StartTime.Subtract(DateTime.Now).TotalMinutes, 2)).ToString().Replace('-', ' ') + " min)"), false);
                if (this.hl == true)
                {
                    this.WindowState = FormWindowState.Normal;
                    hl = false;
                }
            });
        }
        public static JToken Rename(JToken json, Dictionary<string, string> map)
        {
            return Rename(json, name => map.ContainsKey(name) ? map[name] : name);
        }

        public static JToken Rename(JToken json, Func<string, string> map)
        {
            JProperty prop = json as JProperty;
            if (prop != null)
            {
                return new JProperty(map(prop.Name), Rename(prop.Value, map));
            }

            JArray arr = json as JArray;
            if (arr != null)
            {
                var cont = arr.Select(el => Rename(el, map));
                return new JArray(cont);
            }

            JObject o = json as JObject;
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
            WebClient webc = new WebClient();
            if (step == 0)
            {
                if (!File.Exists(minecraft + "/assets/indexes/" + index + ".json"))
                {
                    try { Directory.CreateDirectory(minecraft + "/assets/indexes/"); }
                    catch { }
                    progressBar1.Text = LocRM.GetString("downloader.inprogress") + " " + minecraft + "/assets/indexes/" + index + ".json" + "...";
                    MLog(LocRM.GetString("downloader.inprogress") + " " + minecraft + "/assets/indexes/" + index + ".json" + "...");
                    indexcont = index;
                    progressBar1.Maximum = 100;
                    webc.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                    webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                    webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/indexes/" + index + ".json"), minecraft + "/assets/indexes/" + index + ".json");
                }
                else
                {
                    CheckResourses(index, 1);
                }
            }
            else if (step == 1)
            {
                assetstodownload = null;
                total = 0;
                cur = 0;
                indexcont = index;
                int all = 0;
                int missing = 0;
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
            WebClient webc = new WebClient();
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
                webc.DownloadFileCompleted += new AsyncCompletedEventHandler(ResCompleted);
                webc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChangedRes);
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
                string filename = assetstodownload[cur];
                string downloaded = string.Format("{0} MB's / {1} MB's",(e.BytesReceived / 1024d / 1024d).ToString("0.00"),(e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
                string speed = string.Format("{0} kb/s", (e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));
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

        void LaunchButtonChange(string text, bool enablestate)
        {
            LaunchButton.Text = text;
            LaunchButton.Enabled = enablestate;
        }

        private void AddProfile_Click(object sender, EventArgs e)
        {
            try
            {
                string newprofilename = DateTime.Now.ToString("HH:mm:ss");
                JObject json = JObject.Parse(File.ReadAllText(Variables.localProfileList));
                JObject json1 = (JObject)json["profiles"];
                string toparse = "" + json1[SelectProfile.Text];
                JObject curprofile = JObject.Parse(toparse);
                Console.WriteLine(newprofilename);
                newprofilename = "Copy of " + curprofile["name"] + "(" + newprofilename + ")";
                MLog(LocRM.GetString("profile.createcopy") + " " + this.SelectProfile.Text + "(" + newprofilename + ")" + "...");
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
                ELog(LocRM.GetString("profile.createerror") + "\n" + ex.ToString());
            }
        }

        void ChangeProgile(bool isediting)
        {
            MLog(LocRM.GetString("profile.editing") + " " + this.SelectProfile.Text + "...");
            ProfileForm pf = new ProfileForm();
            pf.ProfileName.Text = this.SelectProfile.Text;
            pf.radButton4.Enabled = isediting;
            pf.ShowDialog();
            if (pf.deleted == false)
            {
                SelectProfile.Items.Add(pf.newprofilename);
                SelectProfile.SelectedItem = SelectProfile.FindItemExact(pf.newprofilename, true);
                GetItems();
                GetSelectedVersion(SelectProfile.SelectedItem.Text);
                if (pf.canceled != true)
                {
                    MLog(LocRM.GetString("profile.edited.complete1p") + " " + this.SelectProfile.Text + " " + LocRM.GetString("profile.edited.complete2p"));
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
                int all = 0;
                int reconstructed = 0;
                try
                {
                    MLog(LocRM.GetString("resources.reconstructing"));
                    JObject json =
                        JObject.Parse(
                            File.ReadAllText(minecraft + "/assets/indexes/" + ReconstructingIndex.Text + ".json"));
                    foreach (JProperty peep in json["objects"])
                    {
                        all++;
                        string c = json["objects"][peep.Name]["hash"].ToString();
                        char с1 = c[0];
                        char с2 = c[1];
                        string filename = с1.ToString() + с2.ToString() + "\\" + json["objects"][peep.Name]["hash"];
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

        private void openModList_Click(object sender, EventArgs e)
        {
            new ModList().ShowDialog();
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
            if (loadedlang == true)
            {
                RadMessageBox.Show(LocRM.GetString("lang.changemessage"), "Language changed", MessageBoxButtons.OK,
                    RadMessageIcon.Info);
                WLog(LocRM.GetString("lang.changemessage"));
            }
        }

        void CleanNatives()
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
    }
}
