using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
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
        }

        Timer t1 = new Timer()
            {
                Interval = 1000,
                Enabled = false,
                Tag = null
            };
        int tick = 0;
        string latest = null;

        string minecraft = Program.minecraft;

        private void MainForm_Load(object sender, EventArgs e)
        {
            MLog(ProductName + " " + ProductVersion);
            MLog("");
            try
            {
                var OSName = (new Microsoft.VisualBasic.Devices.ComputerInfo()).OSFullName;
                MLog("(Microsoft.VisualBasic.Devices.ComputerInfo()).OSFullName == " + OSName);
                MLog("Variables.GetJavaInstallationPath() == \"" + Variables.GetJavaInstallationPath() + "\"");
            }
            catch
            {
                MLog("(Microsoft.VisualBasic.Devices.ComputerInfo()).OSFullName == Unavaiable");
            }
            MLog("JSON.NET " + Variables.netJsonVersion);
            MLog("DotNetZip " + Variables.netZipVersion);
            MLog("NDesk.Options " + Variables.NDOptions);
            MLog("");
            if (!Directory.Exists(minecraft))
            {
                Directory.CreateDirectory(minecraft);
                MLog("Папка " + minecraft + " создана успешно!");
            }
            try
            {
                MLog("Чтение файла конфигурации...");
                LoadConfiguration.LoadConfigurationFile();
            }
            catch (Exception ex)
            {
                MLog("Во время чтения файла конфигрурации возникла ошибка:\n" + ex.ToString());
            }
            Program.lang = LoadConfiguration.mainlang;
            string lang = "";
            if (Program.lang == "")
            {
                lang = "ru-default(Русский)";
            }
            else
            {
                try
                {
                    foreach (string a in Directory.GetFiles(Application.StartupPath + "\\" + Program.lang + "\\"))
                    {
                        if (Path.GetFileName(a).Contains("name"))
                        {
                            lang = Program.lang + "(" + Path.GetFileNameWithoutExtension(a) + ")";
                            break;
                        }
                    }
                }
                catch
                {
                    lang = Program.lang + "(" + "Unknown" + ")";
                }
            }
            MLog("Загружены языковые параметры для языка " + lang);
            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(CheckApplicationUpdate);
            bgw.RunWorkerAsync();
        }

        void CheckLauncherProfiles()
        {
            if (!File.Exists(Variables.localProfileList))
            {
                string json = @"{
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
                JObject parsedjson = JObject.Parse(json);
                parsedjson["profiles"]["Luncher"]["lastVersionId"] = Variables.lastRelease;
                File.WriteAllText(Variables.localProfileList, parsedjson.ToString());
            }
            JObject ljson = JObject.Parse(File.ReadAllText(Variables.localProfileList));
            try
            {
                if (ljson["luncher"].ToString() == "true")
                {
                    if (ljson["selectedProfile"] != null)
                    {
                        MLog("launcher_profiles.json загружен успешно");
                    }
                    else
                    {
                        throw new Exception("Один из важных разделов файла повреждён!");
                    }
                }
                else
                {
                    string newname = DateTime.Now.ToString("HHmmss") + ".json";
                    MLog("Создание бэк-апа старого файла профилей(launcher_profiles.bup." + newname + ")...");
                    File.Move(Variables.localProfileList, Variables.MCFolder + "/launcher_profiles.bup." + newname);
                    JObject jsn = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/launcher_profiles.bup." + newname));
                    jsn.Add(new JProperty("luncher", "true"));
                    File.WriteAllText(Variables.localProfileList, jsn.ToString());
                }
            }
            catch
            {
                string newname = DateTime.Now.ToString("HHmmss") + ".json";
                MLog("Создание бэк-апа старого файла профилей(launcher_profiles.bup." + newname + ")...");
                File.Move(Variables.localProfileList, Variables.MCFolder + "/launcher_profiles.bup." + newname);
                JObject jsn = JObject.Parse(File.ReadAllText(Variables.MCFolder + "/launcher_profiles.bup." + newname));
                jsn.Add(new JProperty("luncher", "true"));
                File.WriteAllText(Variables.localProfileList, jsn.ToString());
            }
        }
        void MLog(string text)
        {
            Log.AppendText(text + "\n");
            Log.ScrollToCaret();
        }
        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            t1.Enabled = false;
            MLog("Завершено. Время загрузки: " + tick + "s");
            MLog("Запуск повторной проверки versions.json...");
            tick = 0;
            CheckVersions();
        }

        void t1_tick(object sender, EventArgs e)
        {
            tick++;
        }

        void DownloadVersions()
        {
            WebClient webc = new WebClient();
            if (!Directory.Exists(minecraft + "/versions/"))
            {
                Directory.CreateDirectory(minecraft + "/versions/");
            }
            MLog("Загружаем versions.json...");
            webc.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            webc.DownloadFileAsync(new Uri("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json"), minecraft + "/versions/versions.json");
            t1.Tick += new EventHandler(t1_tick);
            t1.Enabled = true;
        }

        void CheckVersions()
        {
            if (!File.Exists(minecraft + "\\versions/versions.json"))
            {
                DownloadVersions();
            }
            else
            {
                if (LoadConfiguration.updaterupdateversions == "True")
                {
                    try
                    {
                        MLog("Проверяем version.json...");
                        HttpWebRequest request =
                            (HttpWebRequest)
                                WebRequest.Create("https://s3.amazonaws.com/Minecraft.Download/versions/versions.json");
                        HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                        StreamReader sr = new StreamReader(response.GetResponseStream());
                        int id = 0;
                        string latestsnaphot = null;
                        string latestrelease = null;
                        while (sr.Peek() >= 0)
                        {
                            string line = sr.ReadLine();
                            if (id != 2)
                            {
                                if (line.Contains("snapshot"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    string[] line1 = line.Split(':');
                                    latestsnaphot = line1[1];
                                    MLog("Последний снапшот: " + line1[1]);
                                    id++;
                                }
                                if (line.Contains("release"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    string[] line1 = line.Split(':');
                                    latestrelease = line1[1];
                                    MLog("Последний релиз: " + line1[1]);
                                    id++;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        bool updatefound = false;
                        int id2 = 0;
                        foreach (string lines in File.ReadLines(minecraft + "/versions/versions.json"))
                        {
                            if (id2 != 2)
                            {
                                string line = lines;
                                if (line.Contains("snapshot"))
                                {
                                    line = line.Replace(" ", String.Empty);
                                    line = line.Replace(",", String.Empty);
                                    line = line.Replace("\"", String.Empty);
                                    string[] line1 = line.Split(':');
                                    if (latestsnaphot != line1[1])
                                    {
                                        if (LoadConfiguration.updateralerts.Contains("True"))
                                        {
                                            RadDesktopAlert rd = new RadDesktopAlert
                                            {
                                                CaptionText = "Доступна новая версия",
                                                ContentText =
                                                    "Доступна новая предварительная версия Minecraft: " + latestsnaphot,
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
                                    string[] line1 = line.Split(':');
                                    if (latestrelease != line1[1])
                                    {
                                        if (LoadConfiguration.updateralerts.Contains("True"))
                                        {
                                            RadDesktopAlert rd = new RadDesktopAlert
                                            {
                                                CaptionText = "Доступна новая версия",
                                                ContentText = "Доступна новая версия Minecraft: " + latestrelease,
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
                        JObject jo = JObject.Parse(File.ReadAllText(Variables.MCVersions + "/versions.json"));
                        JObject njo;
                        using (WebClient client = new WebClient())
                        {
                            if (!File.Exists(Variables.MCVersions + "/versions.temp.json"))
                            {
                                client.DownloadFile(
                                    "https://s3.amazonaws.com/Minecraft.Download/versions/versions.json",
                                    Variables.MCVersions + "/versions.temp.json");
                            }
                            njo = JObject.Parse(File.ReadAllText(Variables.MCVersions + "/versions.temp.json"));
                        }
                        MLog("Локальных версий: " + ((JArray)jo["versions"]).Count + ". Версий на удалённом сервере: " + ((JArray)njo["versions"]).Count);
                        if (((JArray) jo["versions"]).Count != ((JArray) njo["versions"]).Count)
                        {
                            updatefound = true;
                        }
                        Variables.lastRelease = latestrelease;
                        Variables.lastSnapshot = latestsnaphot;
                        if (updatefound == true)
                        {
                            DownloadVersions();
                        }
                        if (updatefound == false)
                        {
                            MLog("Обновления не найдены.");
                            Launch();
                        }
                    }
                    catch (Exception ex)
                    {
                        MLog("Во время проверки обновлений versions.json возникла ошибка:\n" + ex.ToString() + "\n");
                        if (File.Exists(minecraft + "\\versions/versions.json"))
                        {
                            Variables.workingOffline = true;
                            MLog("Загружаю локальный список версий...");
                            try
                            {
                                JObject json =
                                    JObject.Parse(File.ReadAllText(minecraft + "/versions/versions.json"));
                                Variables.lastRelease = json["latest"]["release"].ToString();
                                MLog("Последний локальный релиз: " + json["latest"]["release"].ToString());
                                Variables.lastSnapshot = json["latest"]["snapshot"].ToString();
                                MLog("Последний локальный снапшот: " + json["latest"]["snapshot"].ToString());
                                Launch();
                            }
                            catch
                            {
                                CrashPanel.Visible = true;
                                MLog("Локальный versions.json повреждён. Поключите компьютер к Интернету и запустите лаунчер для загрузки этого списка или установите свой вручную.\nПродолжение работы лаунчера невозможно");
                            }
                        }
                        else if (!File.Exists(minecraft + "\\versions/versions.json"))
                        {
                            CrashPanel.Visible = true;
                            MLog("Локальный versions.json отсутствует. Поключите компьютер к Интернету и запустите лаунчер для загрузки этого списка или установите свой вручную.\nПродолжение работы лаунчера невозможно");
                        }
                    }
                }
                else
                {
                    MLog("Проверка versions.json выключена пользователем");
                    Launch();
                }
            }
        }
        void CheckApplicationUpdate(object sender, EventArgs e)
        {
            if (LoadConfiguration.updaterupdateprogram == "True")
            {
                MethodInvoker mi1 = new MethodInvoker(delegate() { this.MLog("Проверяем наличие обновлений..."); });
                this.Invoke(mi1);
                try
                {
                    HttpWebRequest request =
                        (HttpWebRequest) WebRequest.Create("http://file.ru-minecraft.ru/verlu.html");
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string line = sr.ReadLine();
                    if (line == ProductVersion)
                    {
                        MethodInvoker mi2 = new MethodInvoker(delegate() { this.MLog("Обновления не найдены."); });
                        this.Invoke(mi2);
                        MethodInvoker mi = new MethodInvoker(delegate() { this.CheckVersions(); });
                        this.Invoke(mi);
                    }
                    if (line != ProductVersion)
                    {
                        MethodInvoker mi2 = new MethodInvoker(delegate() { this.MLog("Найдено обновление: " + line); });
                        this.Invoke(mi2);
                        DialogResult dr =
                            RadMessageBox.Show(
                                "<html>Найдено обновление лаунчера: <b>" + line + "</b>\nТекущая версия: <b>" +
                                ProductVersion +
                                "</b>\n Хотите ли вы пройти на страницу загрузки данного обновления?\n\nP.S. В противном случае, это уведомление будет появляться при каждом запуске лаунчера >:3",
                                "Найдено обновление", MessageBoxButtons.YesNo, RadMessageIcon.Info);
                        if (dr == System.Windows.Forms.DialogResult.Yes)
                        {
                            Process.Start(
                                @"https://docs.google.com/spreadsheet/ccc?key=0AlHr5lFJzStndHpHVEFORHBYUGd6eXEtQjQ2Y1ZIaWc&usp=sharing");
                            MethodInvoker mi = new MethodInvoker(delegate() { Application.Exit(); });
                            this.Invoke(mi);
                        }
                        else
                        {
                            MethodInvoker mi = new MethodInvoker(delegate() { this.CheckVersions(); });
                            this.Invoke(mi);
                        }
                    }
                    response.Close();
                }
                catch (Exception ex)
                {
                    MethodInvoker mi2 =
                        new MethodInvoker(
                            delegate() { this.MLog("Во время проверки обновлений возникла ошибка:\n" + ex.ToString()); });
                    this.Invoke(mi2);
                    MethodInvoker mi = new MethodInvoker(delegate() { this.CheckVersions(); });
                    this.Invoke(mi);
                }
            }
            else if(LoadConfiguration.updaterupdateprogram == "False")
            {
                MethodInvoker mi1 = new MethodInvoker(
                    delegate() { MLog("Проверка наличия обновлений отлючена пользователем"); });
                this.Invoke(mi1);
                MethodInvoker mi = new MethodInvoker(delegate() { this.CheckVersions(); });
                this.Invoke(mi);
            }
        }
        void Launch()
        {
            try
            {
                try
                {
                    File.Delete(Variables.MCVersions + "/versions.temp.json");
                }
                catch
                {
                }
                Launcher ln = new Launcher();
                ln.Size = this.Size;
                ln.Location = this.Location;
                CheckLauncherProfiles();
                this.Hide();
                MLog("Запуск...");
                ln.Log.Text = this.Log.Text;
                if (LoadConfiguration.resoucersenablerebuilding.Contains("True"))
                {
                    ln.AllowReconstruct.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ln.AllowReconstruct.ToggleState = Telerik.WinControls.Enumerations.ToggleState.Off;
                }
                if (LoadConfiguration.gamelogging.Contains("True"))
                {
                    ln.EnableMinecraftLogging.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ln.EnableMinecraftLogging.ToggleState = Telerik.WinControls.Enumerations.ToggleState.Off;
                }
                if (LoadConfiguration.gameloggingusegameprefix.Contains("True"))
                {
                    ln.UseGamePrefix.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ln.UseGamePrefix.ToggleState = Telerik.WinControls.Enumerations.ToggleState.Off;
                }
                if (LoadConfiguration.updaterupdateversions.Contains("True"))
                {
                    ln.AllowUpdateVersions.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ln.AllowUpdateVersions.ToggleState = Telerik.WinControls.Enumerations.ToggleState.Off;
                }
                if (LoadConfiguration.updateralerts.Contains("True"))
                {
                    ln.EnableMinecraftUpdateAlerts.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ln.EnableMinecraftUpdateAlerts.ToggleState = Telerik.WinControls.Enumerations.ToggleState.Off;
                }
                if (LoadConfiguration.updaterupdateprogram == "True")
                {
                    ln.radCheckBox1.ToggleState = Telerik.WinControls.Enumerations.ToggleState.On;
                }
                else
                {
                    ln.radCheckBox1.ToggleState = Telerik.WinControls.Enumerations.ToggleState.Off;
                }
                ln.ReconstructingIndex.Text = LoadConfiguration.resoucerrebuildresource;
                ln.usingAssets.Text = LoadConfiguration.resoucerassetspath;
                ln.RenameWindow.SelectedIndex = Convert.ToInt32(LoadConfiguration.mainrenamewindow);
                ln.Show();
            }
            catch (Exception ex)
            {
                CrashPanel.Visible = true;
                MLog("Во время чтения файла профилей возникла ошибка:\n" + ex.ToString() + "\n\n#######\nВозможное решение: Удалите " + Variables.profileJSONFile + ". Если у вас есть какая-либо ценная информация в этом файле, то сделайте бэкап");
            }
        }
    }
}
