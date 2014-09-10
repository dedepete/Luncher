using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using NDesk.Options;
using Newtonsoft.Json.Linq;
using Telerik.WinControls;
using Telerik.WinControls.Enumerations;
using Telerik.WinControls.UI;

namespace Luncher.Forms.MainForm
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LogBox.ProductName = ProductName;
            LogBox.Box = Log;
        }

        string _minecraft = string.Empty;

        private void WriteLog(string message)
        {
            if (InvokeRequired)
                Invoke(new Action<string>(WriteLog), new object[] {message});
            else
                Logging.Info(message, new LoggingOptions {UseTimeAndStatePrefix = false});
        }
        private void WriteLog()
        {
            if (InvokeRequired)
                Invoke(new Action<string>(WriteLog));
            else
                Logging.Info(string.Empty, new LoggingOptions { UseTimeAndStatePrefix = false });
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            var ver = ProductVersion.Split('.');
            var finalver = string.Format("{0}.{1}.{2}-build{3}-{4}", ver[0], ver[1], ver[2], ver[3], "git");
            Text = string.Format("{0} {1}", ProductName, finalver);
            WriteLog(string.Format("{0} {1}", ProductName, finalver));
            WriteLog();
            WriteLog("#System information:");
            try
            {
                var osName = (new Microsoft.VisualBasic.Devices.ComputerInfo()).OSFullName;
                WriteLog("Operating System: " + osName);
            }
            catch
            {
                WriteLog("Operating System: Unavaiable");
            }
            try
            {
                WriteLog("Java Path: \"" + Processing.GetJavaInstallationPath() + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("The registry refers to a nonexistent Java Runtime Environment\n\n{0}", ex.Data), @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                WriteLog("Java Path: Missed");
            }
            WriteLog();
            WriteLog("#Assembly information:");
            WriteLog("JSON.NET " + Variables.NetJsonVersion);
            WriteLog("DotNetZip " + Variables.NetZipVersion);
            WriteLog("NDesk.Options " + Variables.NdOptions);
            WriteLog();
            if (Program.Arg.Length != 0)
            {
                var p = new OptionSet
                {
                    {
                        "d|directory=", "minecraft custom {PATH}.",
                        v => Program.Minecraft = v
                    },
                };
                try
                {
                    p.Parse(Program.Arg);
                }
                catch (Exception ex)
                {
                    WriteLog("###########################");
                    WriteLog(ex.ToString());
                    Program.Minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\.minecraft";
                }
                WriteLog("Setting Minecraft directory: " + Program.Minecraft);
            }
            else
                Program.Minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                    "\\.minecraft";
            _minecraft = Program.Minecraft;
            if (!Directory.Exists(_minecraft))
            {
                Directory.CreateDirectory(_minecraft);
                WriteLog("Directory " + _minecraft + " created successful!");
            }
            try
            {
                WriteLog("Reading configuration file...");
                Configuration.Load();
            }
            catch (Exception ex)
            {
                WriteLog("An error occurred while reading configuration file:\n" + ex);
            }
            Program.Lang = (string)Configuration.Main["lang"];
            var lang = String.Empty;
            if (Program.Lang == String.Empty)
                lang = "ru-default(Русский)";
            else
                try
                {
                    foreach (var a in from a in Directory.GetFiles(Application.StartupPath + "\\" + Program.Lang + "\\")
                        let fileName = Path.GetFileName(a)
                        where fileName != null && fileName.Contains("name")
                        select a)
                    {
                        lang = Program.Lang + "(" + Path.GetFileNameWithoutExtension(a) + ")";
                        break;
                    }
                }
                catch
                {
                    WriteLog(String.Format("Unknown language: {0}. Setting default language", Program.Lang));
                    Program.Lang = String.Empty;
                    lang = "ru-default(Русский)";
                }
            WriteLog("Loading settings for language: " + lang);
            _bgw = new Thread(CheckApplicationUpdate);
            _bgw.Start();
        }

        private Thread _bgw;

        void CheckLauncherProfiles()
        {
            if (!File.Exists(Variables.ProfileJsonFile))
            {
                const string json = @"{
  'profiles': {
    'Luncher': {
      'name': 'Luncher',
      'lastVersionId': '1.7.4',
      'allowedReleaseTypes': [
        'release'
      ],
      'launcherVisibilityOnGameClose': 'keep the launcher open'
    }
  },
  'selectedProfile': 'Luncher'
}";
                var parsedjson = JObject.Parse(json);
                parsedjson["profiles"]["Luncher"]["lastVersionId"] = Variables.LastRelease;
                File.WriteAllText(Variables.ProfileJsonFile, parsedjson.ToString());
            }
            var ljson = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
            try
            {
                if (ljson["luncher"].ToString() == "true")
                    if (ljson["selectedProfile"] != null)
                        WriteLog("launcher_profiles.json загружен успешно");
                    else
                        throw new Exception("One of the important file is corrupted!");
                else
                {
                    var newname = DateTime.Now.ToString("HHmmss") + ".json";
                    WriteLog("Creating backup of old launcher_profiles(launcher_profiles.bup." + newname + ")...");
                    File.Move(Variables.ProfileJsonFile, Variables.McFolder + "/launcher_profiles.bup." + newname);
                    var jsn = JObject.Parse(File.ReadAllText(Variables.McFolder + "/launcher_profiles.bup." + newname));
                    jsn.Add(new JProperty("luncher", "true"));
                    File.WriteAllText(Variables.ProfileJsonFile, jsn.ToString());
                }
            }
            catch
            {
                var newname = DateTime.Now.ToString("HHmmss") + ".json";
                WriteLog("Creating backup of old launcher_profiles(launcher_profiles.bup." + newname + ")...");
                File.Move(Variables.ProfileJsonFile, Variables.McFolder + "/launcher_profiles.bup." + newname);
                var jsn = JObject.Parse(File.ReadAllText(Variables.McFolder + "/launcher_profiles.bup." + newname));
                jsn.Add(new JProperty("luncher", "true"));
                File.WriteAllText(Variables.ProfileJsonFile, jsn.ToString());
            }
        }

        void DownloadVersions()
        {
            var webc = new WebClient();
            if (!Directory.Exists(_minecraft + "/versions/"))
                Directory.CreateDirectory(_minecraft + "/versions/");
            WriteLog("Downloading versions.json...");
            var sw = new Stopwatch();
            sw.Start();
            webc.DownloadFileCompleted += (sender, e) =>
            {
                sw.Stop();
                WriteLog("Completed! Download time: " + sw.ElapsedMilliseconds + "ms");
                if (new FileInfo(String.Format("{0}{1}", Variables.McVersions, "versions.json")).Length < 0)
                    WriteLog("version.json downloaded with wrong filesize!");
                Launch();
            };
            webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json"), _minecraft + "/versions/versions.json");
        }

        private async void CheckVersions()
        {
            if (InvokeRequired)
                Invoke(new Action(CheckVersions));
            else
            {
                if (!File.Exists(_minecraft + "\\versions\\versions.json"))
                    DownloadVersions();
                else
                {
                    if ((bool) Configuration.Updates["checkVersionsUpdate"])
                        try
                        {
                            WriteLog("Checking version.json...");
                            var latestsnapshot = String.Empty;
                            var latestrelease = String.Empty;
                            var jb =
                                JObject.Parse(await new WebClient().DownloadStringTaskAsync(
                                new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json")));
                            if (jb["latest"]["snapshot"] != null)
                            {
                                latestsnapshot = jb["latest"]["snapshot"].ToString();
                                WriteLog("Latest snapshot: " + latestsnapshot);
                            }
                            if (jb["latest"]["release"] != null)
                            {
                                latestrelease = jb["latest"]["release"].ToString();
                                WriteLog("Latest release: " + latestrelease);
                            }
                            var updatefound = false;
                            var localsnapshot = String.Empty;
                            var localrelease = String.Empty;
                            var ver = JObject.Parse(File.ReadAllText(_minecraft + "/versions/versions.json"));
                            if (ver["latest"]["snapshot"] != null) localsnapshot = ver["latest"]["snapshot"].ToString();
                            if (ver["latest"]["release"] != null) localrelease = ver["latest"]["release"].ToString();
                            if (latestsnapshot != localsnapshot || latestrelease != localrelease)
                            {
                                if ((bool) Configuration.Updates["enableMinecraftUpdateAlerts"])
                                {
                                    if (latestsnapshot != localsnapshot)
                                        new RadDesktopAlert
                                        {
                                            CaptionText = "A new version available",
                                            ContentText =
                                                "A new Minecraft snapshot is avaible: " + latestsnapshot,
                                            ShowCloseButton = true,
                                            ShowOptionsButton = false,
                                            ShowPinButton = false,
                                            AutoClose = true,
                                            AutoCloseDelay = 10,
                                            ThemeName = "VisualStudio2012Dark",
                                            CanMove = false
                                        }.Show();
                                    if (latestrelease != localrelease)
                                        new RadDesktopAlert
                                        {
                                            CaptionText = "A new version available",
                                            ContentText = "A new Minecraft release is avaible: " + latestrelease,
                                            ShowCloseButton = true,
                                            ShowOptionsButton = false,
                                            ShowPinButton = false,
                                            AutoClose = true,
                                            AutoCloseDelay = 10,
                                            ThemeName = "VisualStudio2012Dark",
                                            CanMove = false
                                        }.Show();
                                }
                                updatefound = true;
                            }
                            WriteLog("Local versions: " + ((JArray) jb["versions"]).Count + ". Remote versions: " +
                                     ((JArray) ver["versions"]).Count);
                            if (((JArray) jb["versions"]).Count != ((JArray) ver["versions"]).Count) updatefound = true;
                            Variables.LastRelease = latestrelease;
                            Variables.LastSnapshot = latestsnapshot;
                            if (updatefound)
                                DownloadVersions();
                            else
                            {
                                WriteLog("No update found.");
                                Launch();
                            }
                        }
                        catch (Exception ex)
                        {
                            WriteLog("An error occurred while checking versions.json:\n" + ex + "\n");
                            if (File.Exists(_minecraft + "\\versions\\versions.json"))
                            {
                                Variables.WorkingOffline = true;
                                WriteLog("Загружаю локальный список версий...");
                                try
                                {
                                    var json =
                                        JObject.Parse(File.ReadAllText(_minecraft + "/versions/versions.json"));
                                    Variables.LastRelease = json["latest"]["release"].ToString();
                                    WriteLog("Last local release: " + json["latest"]["release"]);
                                    Variables.LastSnapshot = json["latest"]["snapshot"].ToString();
                                    WriteLog("Last local snapshot: " + json["latest"]["snapshot"]);
                                    Launch();
                                }
                                catch
                                {
                                    CrashPanel.Visible = true;
                                    WriteLog(
                                        "Локальный versions.json повреждён. Поключите компьютер к Интернету и запустите лаунчер для загрузки этого списка или установите свой вручную.\nПродолжение работы лаунчера невозможно");
                                }
                            }
                            else if (!File.Exists(_minecraft + "/versions/versions.json"))
                            {
                                CrashPanel.Visible = true;
                                WriteLog(
                                    "Локальный versions.json отсутствует. Поключите компьютер к Интернету и запустите лаунчер для загрузки этого списка или установите свой вручную.\nПродолжение работы лаунчера невозможно");
                            }
                        }
                    else
                    {
                        WriteLog("Проверка versions.json выключена пользователем");
                        Launch();
                    }
                }
            }
        }

        private void CheckApplicationUpdate()
        {
            if ((bool)Configuration.Updates["checkProgramUpdate"])
            {
                WriteLog("Checking for update...");
                try
                {
                    var dver = new WebClient().DownloadString(new Uri("http://file.ru-minecraft.ru/verlu.html"));
                    if (dver == ProductVersion)
                    {
                        WriteLog("No update found.");
                        CheckVersions();
                    }
                    else if (dver != ProductVersion)
                    {
                        WriteLog("Update avaible: " + dver);
                        var mi2 = new MethodInvoker(() =>
                        {
                            var dr = new RadMessageBoxForm
                            {
                                Text = @"Найдено обновление",
                                MessageText =
                                    "<html>Найдено обновление лаунчера: <b>" + dver + "</b>\nТекущая версия: <b>" +
                                    ProductVersion +
                                    "</b>\n Хотите ли вы пройти на страницу загрузки данного обновления?\n\nP.S. В противном случае, это уведомление будет появляться при каждом запуске лаунчера >:3",
                                StartPosition = FormStartPosition.CenterScreen,
                                ButtonsConfiguration = MessageBoxButtons.YesNo,
                                TopMost = true,
                                MessageIcon = Processing.GetRadMessageIcon(RadMessageIcon.Info),
                                Owner = this,
                                DetailsText = null
                            }.ShowDialog();
                            if (dr == DialogResult.Yes)
                            {
                                Process.Start(
                                    @"https://docs.google.com/spreadsheet/ccc?key=0AlHr5lFJzStndHpHVEFORHBYUGd6eXEtQjQ2Y1ZIaWc&usp=sharing");
                                Application.Exit();
                            }
                            else
                                CheckVersions();
                        });
                        Invoke(mi2);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Во время проверки обновлений возникла ошибка:\n" + ex);
                    CheckVersions();
                }
            }
            else
            {
                WriteLog("Проверка наличия обновлений отлючена пользователем");
                CheckVersions();
            }
        }

        private void Launch()
        {
            WriteLog("Starting launcher...");
            try
            {
                var ln = new Launcher.Launcher {Size = Size, Location = Location};
                CheckLauncherProfiles();
                Hide();
                ln.Log.Text = Log.Text;

                ln.AllowReconstruct.ToggleState = (bool)Configuration.Resources["enableReconstruction"]
                    ? ToggleState.On
                    : ToggleState.Off;

                ln.EnableMinecraftLogging.ToggleState = (bool)Configuration.Updates["enableMinecraftUpdateAlerts"]
                    ? ToggleState.On
                    : ToggleState.Off;

                ln.UseGamePrefix.ToggleState = (bool)Configuration.Logging["useGamePrefix"]
                    ? ToggleState.On
                    : ToggleState.Off;

                ln.AllowUpdateVersions.ToggleState = (bool)Configuration.Updates["checkVersionsUpdate"]
                    ? ToggleState.On
                    : ToggleState.Off;

                ln.EnableMinecraftUpdateAlerts.ToggleState = (bool)Configuration.Updates["enableMinecraftUpdateAlerts"]
                    ? ToggleState.On
                    : ToggleState.Off;

                ln.radCheckBox1.ToggleState = (bool)Configuration.Updates["checkProgramUpdate"]
                    ? ToggleState.On
                    : ToggleState.Off;

                ln.usingAssets.Text = (string)Configuration.Resources["assetsDir"];
                ln.Show();
            }
            catch (Exception ex)
            {
                CrashPanel.Visible = true;
                WriteLog("Во время чтения файла профилей возникла ошибка:\n" + ex +
                         "\n\n#######\nВозможное решение: Удалите " + Variables.ProfileJsonFile +
                         ". Если у вас есть какая-либо ценная информация в этом файле, то сделайте бэкап");
            }
        }
    }
}
