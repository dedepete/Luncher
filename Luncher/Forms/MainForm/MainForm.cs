using System;
using System.Diagnostics;
using System.Drawing;
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
            LoggingConfiguration.ProductName = ProductName;
            LoggingConfiguration.LoggingBox = Log;
        }

        string _minecraft = string.Empty;

        private void WriteLog(string message)
        {
            Logging.Info(message, new LoggingOptions {UseTimeAndStatePrefix = false});
        }
        private void WriteLog()
        {
            Logging.Info(string.Empty, new LoggingOptions {UseTimeAndStatePrefix = false});
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
            var jpath = Processing.GetJavaInstallationPath();
            WriteLog("Java Path: \"" + (jpath ?? "MISSED") + "\"");
            if (jpath == null)
                MessageBox.Show(
                    Localization.Localization_MainForm.JavaNotFoundError,
                    @"Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            WriteLog();
            WriteLog(string.Format("#Assembly information:\nJSON.NET {0}\nDotNetZip {1}\nNDesk.Options {2}",
                Variables.NetJsonVersion, Variables.NetZipVersion, Variables.NdOptions));
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
            var lang = string.Empty;
            if (Program.Lang == string.Empty)
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
                    WriteLog(string.Format("Unknown language: {0}. Setting default language", Program.Lang));
                    Program.Lang = string.Empty;
                    lang = "ru-default(Русский)";
                }
            WriteLog("Loading settings for language: " + lang);
            _bgw = new Thread(CheckApplicationUpdate);
            _bgw.Start();
        }

        private Thread _bgw;

        void CheckLauncherProfiles()
        {
            try
            {
                var profile = JObject.Parse(File.ReadAllText(Variables.ProfileJsonFile));
                if (profile["profiles"] == null)
                    throw new Exception();
                if (profile["selectedProfile"] == null)
                    throw new Exception();
            }
            catch
            {
                File.Delete(Variables.ProfileJsonFile);
            }
            if (File.Exists(Variables.ProfileJsonFile)) return;
            var jsonProfile = new JObject
            {
                {
                    "profiles", new JObject
                    {
                        {
                            ProductName, new JObject
                            {
                                {"name", ProductName},
                                {"lastVersionId", Variables.LastRelease},
                                {
                                    "allowedReleaseTypes", new JArray
                                    {
                                        "release"
                                    }
                                },
                                {"launcherVisibilityOnGameClose", "keep the launcher open"}
                            }
                        }
                    }
                },
                {"selectedProfile", ProductName}
            };
            File.WriteAllText(Variables.ProfileJsonFile, jsonProfile.ToString());
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
                var path = string.Format("{0}{1}", Variables.McVersions, "versions.json");
                if (new FileInfo(path).Length < 0)
                    WriteLog("version.json downloaded with wrong filesize!");
                else
                {
                    var verFile = JObject.Parse(File.ReadAllText(path));
                    if (verFile["latest"]["snapshot"] != null) Variables.LastSnapshot = verFile["latest"]["snapshot"].ToString();
                    if (verFile["latest"]["release"] != null) Variables.LastRelease = verFile["latest"]["release"].ToString();
                }
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
                            var latestsnapshot = string.Empty;
                            var latestrelease = string.Empty;
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
                            var localsnapshot = string.Empty;
                            var localrelease = string.Empty;
                            var ver = JObject.Parse(File.ReadAllText(_minecraft + "/versions/versions.json"));
                            if (ver["latest"]["snapshot"] != null) localsnapshot = ver["latest"]["snapshot"].ToString();
                            if (ver["latest"]["release"] != null) localrelease = ver["latest"]["release"].ToString();
                            if (latestsnapshot != localsnapshot || latestrelease != localrelease)
                            {
                                if ((bool) Configuration.Updates["enableMinecraftUpdateAlerts"])
                                {
                                    if (latestsnapshot != localsnapshot)
                                        Processing.ShowAlert("A new version is available",
                                            "A new Minecraft snapshot is avaible: " + latestsnapshot);
                                    if (latestrelease != localrelease)
                                        Processing.ShowAlert("A new version is available",
                                            "A new Minecraft release is avaible: " + latestrelease);
                                }
                                updatefound = true;
                            }
                            WriteLog("Local versions: " + ((JArray) jb["versions"]).Count + ". Remote versions: " +
                                     ((JArray) ver["versions"]).Count);
                            if (((JArray) jb["versions"]).Count != ((JArray) ver["versions"]).Count) updatefound = true;
                            Variables.LastRelease = latestrelease;
                            Variables.LastSnapshot = latestsnapshot;
                            if (updatefound)
                            {
                                DownloadVersions();
                                return;
                            }
                            WriteLog("No update found.");
                        }
                        catch (Exception ex)
                        {
                            WriteLog("An error occurred while checking versions.json:\n" + ex + "\n");
                            if (File.Exists(_minecraft + "\\versions\\versions.json"))
                            {
                                Variables.WorkingOffline = true;
                                WriteLog("Loading local versions.json...");
                                try
                                {
                                    var json =
                                        JObject.Parse(File.ReadAllText(_minecraft + "/versions/versions.json"));
                                    Variables.LastRelease = json["latest"]["release"].ToString();
                                    WriteLog("Last local release: " + json["latest"]["release"]);
                                    Variables.LastSnapshot = json["latest"]["snapshot"].ToString();
                                    WriteLog("Last local snapshot: " + json["latest"]["snapshot"]);
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
                        WriteLog("Проверка versions.json выключена пользователем");
                    Launch();
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
                        WriteLog("No update found.");
                    else if (dver != ProductVersion)
                    {
                        WriteLog("Update avaible: " + dver);
                        var mi2 = new MethodInvoker(() =>
                        {
                            var alert = new RadDesktopAlert
                            {
                                CaptionText = @"Найдено обновление",
                                ContentText = "<html>Найдено обновление лаунчера: <b>" + dver + "</b>\nТекущая версия: <b>" +
                                    ProductVersion +
                                    "</b>\n Хотите ли вы пройти на страницу загрузки данного обновления?\n\nP.S. В противном случае, это уведомление будет появляться при каждом запуске лаунчера >:3",
                                ShowCloseButton = false,
                                ShowOptionsButton = false,
                                ShowPinButton = false,
                                AutoClose = false,
                                CanMove = false,
                                AutoCloseDelay = 5,
                                FixedSize = new Size(329, 235),
                                ThemeName = "VisualStudio2012Dark"
                            };
                            var openUrlButton = new RadButtonElement("Получить обновление");
                            openUrlButton.Click += delegate
                            {
                                Process.Start(
                                    @"https://docs.google.com/spreadsheet/ccc?key=0AlHr5lFJzStndHpHVEFORHBYUGd6eXEtQjQ2Y1ZIaWc&usp=sharing");
                                alert.Hide();
                                Application.Exit();
                            };
                            var ignoreButton = new RadButtonElement("Игнорировать");
                            ignoreButton.Click += delegate { alert.Hide(); };
                            alert.ButtonItems.Add(openUrlButton);
                            alert.ButtonItems.Add(ignoreButton);
                            alert.Show();
                        });
                        Invoke(mi2);
                    }
                    CheckVersions();
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
                LoggingConfiguration.LoggingBox = Log;
                CrashPanel.Visible = true;
                WriteLog("Во время чтения файла профилей возникла ошибка:\n" + ex +
                         "\n\n#######\nВозможное решение: Удалите " + Variables.ProfileJsonFile +
                         ". Если у вас есть какая-либо ценная информация в этом файле, то сделайте бэкап");
            }
        }
    }
}
