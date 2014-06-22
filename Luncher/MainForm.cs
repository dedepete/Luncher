using System.Linq;
using NDesk.Options;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Luncher
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            LogBox._LogBox = Log;
        }

        readonly Timer _t1 = new Timer
            {
                Interval = 1000,
                Enabled = false,
                Tag = null
            };
        int _tick;

        string _minecraft = "";

        private void MainForm_Load(object sender, EventArgs e)
        {
            Text = ProductName + " " + ProductVersion;
            Logging.Log("", false, false,  ProductName + " " + ProductVersion);
            Logging.Log("", false, false,  "");
            Logging.Log("", false, false,  "#System information:");
            try
            {
                var osName = (new Microsoft.VisualBasic.Devices.ComputerInfo()).OSFullName;
                Logging.Log("", false, false,  "Operating System: " + osName);
                Logging.Log("", false, false,  "Java Path: \"" + Processing.GetJavaInstallationPath() + "\"");
            }
            catch
            {
                Logging.Log("", false, false,  "OSFullName == Unavaiable");
            }
            Logging.Log("", false, false,  "");
            Logging.Log("", false, false,  "#Assembly information:");
            Logging.Log("", false, false,  "JSON.NET " + Variables.NetJsonVersion);
            Logging.Log("", false, false,  "DotNetZip " + Variables.NetZipVersion);
            Logging.Log("", false, false,  "NDesk.Options " + Variables.NdOptions);
            Logging.Log("", false, false,  "");
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
                    Logging.Log("", false, false,  "###########################");
                    Logging.Log("", false, false,  ex.ToString());
                    Program.Minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                }
                Logging.Log("", false, false,  "Setting Minecraft directory: " + Program.Minecraft);
            }
            else
            {
                Program.Minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
            }
            _minecraft = Program.Minecraft;
            if (!Directory.Exists(_minecraft))
            {
                Directory.CreateDirectory(_minecraft);
                Logging.Log("", false, false,  "Directory " + _minecraft + " created successful!");
            }
            try
            {
                Logging.Log("", false, false,  "Reading configuration file...");
                LoadConfiguration.LoadConfigurationFile();
            }
            catch (Exception ex)
            {
                Logging.Log("", false, false, "An error occurred while reading configuration file:\n" + ex);
            }
            Program.Lang = LoadConfiguration.Mainlang;
            var lang = "";
            if (Program.Lang == "")
            {
                lang = "ru-default(Русский)";
            }
            else
            {
                try
                {
                    foreach (var a in from a in Directory.GetFiles(Application.StartupPath + "\\" + Program.Lang + "\\") let fileName = Path.GetFileName(a) where fileName != null && fileName.Contains("name") select a)
                    {
                        lang = Program.Lang + "(" + Path.GetFileNameWithoutExtension(a) + ")";
                        break;
                    }
                }
                catch
                {
                    lang = Program.Lang + "(" + "Unknown" + ")";
                }
            }
            Logging.Log("", false, false,  "Loading setting for language: " + lang);
            var bgw = new BackgroundWorker();
            bgw.DoWork += CheckApplicationUpdate;
            bgw.RunWorkerAsync();
        }

        void CheckLauncherProfiles()
        {
            if (!File.Exists(Variables.LocalProfileList))
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
                File.WriteAllText(Variables.LocalProfileList, parsedjson.ToString());
            }
            var ljson = JObject.Parse(File.ReadAllText(Variables.LocalProfileList));
            try
            {
                if (ljson["luncher"].ToString() == "true")
                {
                    if (ljson["selectedProfile"] != null)
                    {
                        Logging.Log("", false, false,  "launcher_profiles.json загружен успешно");
                    }
                    else
                    {
                        throw new Exception("One of the important file is corrupted!");
                    }
                }
                else
                {
                    var newname = DateTime.Now.ToString("HHmmss") + ".json";
                    Logging.Log("", false, false,  "Creating backup of old launcher_profiles(launcher_profiles.bup." + newname + ")...");
                    File.Move(Variables.LocalProfileList, Variables.McFolder + "/launcher_profiles.bup." + newname);
                    var jsn = JObject.Parse(File.ReadAllText(Variables.McFolder + "/launcher_profiles.bup." + newname));
                    jsn.Add(new JProperty("luncher", "true"));
                    File.WriteAllText(Variables.LocalProfileList, jsn.ToString());
                }
            }
            catch
            {
                var newname = DateTime.Now.ToString("HHmmss") + ".json";
                Logging.Log("", false, false, "Creating backup of old launcher_profiles(launcher_profiles.bup." + newname + ")...");
                File.Move(Variables.LocalProfileList, Variables.McFolder + "/launcher_profiles.bup." + newname);
                var jsn = JObject.Parse(File.ReadAllText(Variables.McFolder + "/launcher_profiles.bup." + newname));
                jsn.Add(new JProperty("luncher", "true"));
                File.WriteAllText(Variables.LocalProfileList, jsn.ToString());
            }
        }
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            _t1.Enabled = false;
            Logging.Log("", false, false,  "Completed! Download time: " + _tick + "s");
            Logging.Log("", false, false,  "Rechecking versions.json...");
            _tick = 0;
            CheckVersions();
        }

        void t1_tick(object sender, EventArgs e)
        {
            _tick++;
        }

        void DownloadVersions()
        {
            var webc = new WebClient();
            if (!Directory.Exists(_minecraft + "/versions/"))
            {
                Directory.CreateDirectory(_minecraft + "/versions/");
            }
            Logging.Log("", false, false,  "Downloading versions.json...");
            webc.DownloadFileCompleted += Completed;
            webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json"), _minecraft + "/versions/versions.json");
            _t1.Tick += t1_tick;
            _t1.Enabled = true;
        }

        void CheckVersions()
        {
            if (!File.Exists(_minecraft + "\\versions/versions.json"))
            {
                DownloadVersions();
            }
            else
            {
                if (LoadConfiguration.Updaterupdateversions == "True")
                {
                    try
                    {
                        Logging.Log("", false, false,  "Checking version.json...");
                        var request =
                            (HttpWebRequest)
                                WebRequest.Create("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json");
                        var response = (HttpWebResponse) request.GetResponse();
                        var sr = new StreamReader(response.GetResponseStream());
                        var id = 0;
                        string latestsnaphot = null;
                        string latestrelease = null;
                        while (sr.Peek() >= 0)
                        {
                            var line = sr.ReadLine();
                            if (id != 2)
                            {
                                if (line != null && line.Contains("snapshot"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    var line1 = line.Split(':');
                                    latestsnaphot = line1[1];
                                    Logging.Log("", false, false,  "Latest snapshot: " + line1[1]);
                                    id++;
                                }
                                if (line != null && line.Contains("release"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    var line1 = line.Split(':');
                                    latestrelease = line1[1];
                                    Logging.Log("", false, false,  "Latest release: " + line1[1]);
                                    id++;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        var updatefound = false;
                        var id2 = 0;
                        foreach (var lines in File.ReadLines(_minecraft + "/versions/versions.json"))
                        {
                            if (id2 != 2)
                            {
                                var line = lines;
                                if (line.Contains("snapshot"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    var line1 = line.Split(':');
                                    if (latestsnaphot != line1[1])
                                    {
                                        if (LoadConfiguration.Updateralerts.Contains("True"))
                                        {
                                            var rd = new RadDesktopAlert
                                            {
                                                CaptionText = "A new version available",
                                                ContentText =
                                                    "A new Minecraft snapshot is avaible: " + latestsnaphot,
                                                ShowCloseButton = true,
                                                ShowOptionsButton = false,
                                                ShowPinButton = false,
                                                AutoClose = true,
                                                AutoCloseDelay = 10,
                                                ThemeName = "VisualStudio2012Dark",
                                                CanMove = false
                                            };
                                            rd.Show();
                                        }
                                        updatefound = true;
                                    }
                                    id2++;
                                }
                                if (line.Contains("release"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    var line1 = line.Split(':');
                                    if (latestrelease != line1[1])
                                    {
                                        if (LoadConfiguration.Updateralerts.Contains("True"))
                                        {
                                            var rd = new RadDesktopAlert
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
                                            };
                                            rd.Show();
                                        }
                                        updatefound = true;
                                    }
                                    id2++;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        var jo = JObject.Parse(File.ReadAllText(Variables.McVersions + "/versions.json"));
                        JObject njo;
                        using (var client = new WebClient())
                        {
                            if (!File.Exists(Variables.McVersions + "/versions.temp.json"))
                            {
                                client.DownloadFile(
                                    "https://s3.amazonaws.com/Minecraft.Download/versions/versions.json",
                                    Variables.McVersions + "/versions.temp.json");
                            }
                            njo = JObject.Parse(File.ReadAllText(Variables.McVersions + "/versions.temp.json"));
                        }
                        Logging.Log("", false, false,  "Local versions: " + ((JArray)jo["versions"]).Count + ". Remote versions: " + ((JArray)njo["versions"]).Count);
                        if (((JArray) jo["versions"]).Count != ((JArray) njo["versions"]).Count)
                        {
                            updatefound = true;
                        }
                        Variables.LastRelease = latestrelease;
                        Variables.LastSnapshot = latestsnaphot;
                        if (updatefound)
                        {
                            DownloadVersions();
                        }
                        if (!updatefound)
                        {
                            Logging.Log("", false, false,  "No update found.");
                            Launch();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Log("", false, false,  "An error occurred while checking versions.json:\n" + ex + "\n");
                        if (File.Exists(_minecraft + "\\versions/versions.json"))
                        {
                            Variables.WorkingOffline = true;
                            Logging.Log("", false, false,  "Загружаю локальный список версий...");
                            try
                            {
                                JObject json =
                                    JObject.Parse(File.ReadAllText(_minecraft + "/versions/versions.json"));
                                Variables.LastRelease = json["latest"]["release"].ToString();
                                Logging.Log("", false, false,  "Last local release: " + json["latest"]["release"]);
                                Variables.LastSnapshot = json["latest"]["snapshot"].ToString();
                                Logging.Log("", false, false,  "Last local snapshot: " + json["latest"]["snapshot"]);
                                Launch();
                            }
                            catch
                            {
                                CrashPanel.Visible = true;
                                Logging.Log("", false, false,  "Локальный versions.json повреждён. Поключите компьютер к Интернету и запустите лаунчер для загрузки этого списка или установите свой вручную.\nПродолжение работы лаунчера невозможно");
                            }
                        }
                        else if (!File.Exists(_minecraft + "\\versions/versions.json"))
                        {
                            CrashPanel.Visible = true;
                            Logging.Log("", false, false,  "Локальный versions.json отсутствует. Поключите компьютер к Интернету и запустите лаунчер для загрузки этого списка или установите свой вручную.\nПродолжение работы лаунчера невозможно");
                        }
                    }
                }
                else
                {
                    Logging.Log("", false, false,  "Проверка versions.json выключена пользователем");
                    Launch();
                }
            }
        }
        void CheckApplicationUpdate(object sender, EventArgs e)
        {
            if (LoadConfiguration.Updaterupdateprogram == "True")
            {
                    var mi1 = new MethodInvoker(() => Logging.Log("", false, false,  "Checking for update..."));
                    Invoke(mi1);
                    try
                    {
                        var request =
                            (HttpWebRequest) WebRequest.Create("http://file.ru-minecraft.ru/verlu.html");
                        var response = (HttpWebResponse) request.GetResponse();
                        var sr = new StreamReader(response.GetResponseStream());
                        var line = sr.ReadLine();
                        if (line == ProductVersion)
                        {
                            var mi2 = new MethodInvoker(delegate { Logging.Log("", false, false,  "No update found."); CheckVersions(); });
                            Invoke(mi2);
                        }
                        if (line != ProductVersion)
                        {
                            var mi2 = new MethodInvoker(() => Logging.Log("", false, false,  "Update avaible: " + line));
                            Invoke(mi2);
                            var dr = new RadMessageBoxForm
                            {
                                Text = "Найдено обновление",
                                MessageText =
                                    "<html>Найдено обновление лаунчера: <b>" + line + "</b>\nТекущая версия: <b>" +
                                    ProductVersion +
                                    "</b>\n Хотите ли вы пройти на страницу загрузки данного обновления?\n\nP.S. В противном случае, это уведомление будет появляться при каждом запуске лаунчера >:3",
                                StartPosition = FormStartPosition.CenterScreen,
                                ButtonsConfiguration = MessageBoxButtons.YesNo,
                                TopMost = true,
                                MessageIcon = null
                            }.ShowDialog();
                            if (dr == DialogResult.Yes)
                            {
                                Process.Start(
                                    @"https://docs.google.com/spreadsheet/ccc?key=0AlHr5lFJzStndHpHVEFORHBYUGd6eXEtQjQ2Y1ZIaWc&usp=sharing");
                                new MethodInvoker(Application.Exit).Invoke();
                            }
                            else
                            {
                                var mi = new MethodInvoker(CheckVersions);
                                Invoke(mi);
                            }
                        }
                        response.Close();
                    }
                    catch (Exception ex)
                    {
                        var mi = new MethodInvoker(delegate { Logging.Log("", false, false,  "Во время проверки обновлений возникла ошибка:\n" + ex); CheckVersions(); });
                        Invoke(mi);
                    }
                }
                else
                {
                    var mi = new MethodInvoker(delegate { Logging.Log("", false, false,  "Проверка наличия обновлений отлючена пользователем"); CheckVersions(); });
                    Invoke(mi);
                }
            }
        }

        void Launch()
        {
            try
            {
                var tempfilepath = Variables.McVersions + "/versions.temp.json";
                if (File.Exists(tempfilepath)) File.Delete(tempfilepath);
                var ln = new Launcher {Size = Size, Location = Location};
                CheckLauncherProfiles();
                Hide();
                Logging.Log("", false, false,  "Starting launcher...");
                ln.Log.Text = Log.Text;

                ln.AllowReconstruct.ToggleState = LoadConfiguration.Resoucersenablerebuilding.Contains("True") ? Telerik.WinControls.Enumerations.ToggleState.On : Telerik.WinControls.Enumerations.ToggleState.Off;

                ln.EnableMinecraftLogging.ToggleState = LoadConfiguration.Gamelogging.Contains("True") ? Telerik.WinControls.Enumerations.ToggleState.On : Telerik.WinControls.Enumerations.ToggleState.Off;

                ln.UseGamePrefix.ToggleState = LoadConfiguration.GameLoggingusegameprefix.Contains("True") ? Telerik.WinControls.Enumerations.ToggleState.On : Telerik.WinControls.Enumerations.ToggleState.Off;

                ln.AllowUpdateVersions.ToggleState = LoadConfiguration.Updaterupdateversions.Contains("True") ? Telerik.WinControls.Enumerations.ToggleState.On : Telerik.WinControls.Enumerations.ToggleState.Off;

                ln.EnableMinecraftUpdateAlerts.ToggleState = LoadConfiguration.Updateralerts.Contains("True") ? Telerik.WinControls.Enumerations.ToggleState.On : Telerik.WinControls.Enumerations.ToggleState.Off;

                ln.radCheckBox1.ToggleState = LoadConfiguration.Updaterupdateprogram == "True" ? Telerik.WinControls.Enumerations.ToggleState.On : Telerik.WinControls.Enumerations.ToggleState.Off;

                ln.ReconstructingIndex.Text = LoadConfiguration.Resoucerrebuildresource;
                ln.usingAssets.Text = LoadConfiguration.Resoucerassetspath;
                ln.RenameWindow.SelectedIndex = Convert.ToInt32(LoadConfiguration.Mainrenamewindow);
                ln.Show();
            }
            catch (Exception ex)
            {
                CrashPanel.Visible = true;
                Logging.Log("", false, false,  "Во время чтения файла профилей возникла ошибка:\n" + ex + "\n\n#######\nВозможное решение: Удалите " + Variables.ProfileJsonFile + ". Если у вас есть какая-либо ценная информация в этом файле, то сделайте бэкап");
            }
        }
    }
}
