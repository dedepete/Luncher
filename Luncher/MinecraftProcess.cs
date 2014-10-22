using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Luncher.Forms.Launcher;
using Newtonsoft.Json.Linq;
using Telerik.WinControls.UI;

namespace Luncher
{
    public class MinecraftProcess : McVariables
    {
        private object Root { get; set; }

        private RichTextBox Txt { get; set; }
        private RadButton KillButton { get; set; }
        private RadButton CloseTabButton { get; set; }
        private Process _client;

        private readonly int _launcherVisibilityOnGameClose;

        public MinecraftProcess(object mainForm, string assetsPath, string libraries, string assetsFileName, string jsonblock)
        {
            dynamic json = JObject.Parse(jsonblock);
            if (json.javaArgs != null)
                JavaArgs = json.javaArgs + " ";
            if (json.javaDir != null)
                JavaExec = json.javaDir;
            PName = json.name;
            GameDir = json.gameDir ?? Program.Minecraft;
            LastVersionId = json.lastVersionId ?? (json.allowedReleaseTypes.Contains("snapshot")
                ? Variables.LastSnapshot
                : Variables.LastRelease);
            if (json.launcherVisibilityOnGameClose != null)
                switch ((string)json.launcherVisibilityOnGameClose)
                {
                    case "close launcher when game starts":
                        _launcherVisibilityOnGameClose = 1;
                        break;
                    case "hide launcher and re-open when game closes":
                        _launcherVisibilityOnGameClose = 2;
                        break;
                }
            string ip = null, port = null;
            if (json.server != null)
            {
                ip = json.server.ip;
                port = json.server.port;
            }
            var nativesFolder = Path.Combine(Variables.McVersions, LastVersionId);
            dynamic profileSJson = JObject.Parse(File.ReadAllText(nativesFolder + @"\" + LastVersionId + ".json"));
            MainClass = profileSJson.mainClass;
            Arg = profileSJson.minecraftArguments +
                  (ip != null ? String.Format(" --server {0} --port {1}", ip, (port ?? "25565")) : String.Empty);
            libraries += String.Format(";{0}\\{1}.jar", nativesFolder, LastVersionId);
            if (_launcherVisibilityOnGameClose != 1)
            {
                var va = ((Launcher) mainForm).LogTab("Minecraft version: " + LastVersionId, PName);
                Txt = (RichTextBox) va[0];
                KillButton = (RadButton) va[1];
                CloseTabButton = (RadButton) va[2];
            }
            Root = mainForm;
            Assetspath = assetsPath;
            Libs = libraries;
            Assets = assetsFileName;
        }

        private void MLogG(string text, bool iserror)
        {
            if (Txt.InvokeRequired)
                Txt.Invoke(new Action<string, bool>(MLogG), new object[] {text, iserror});
            else
            {
                var color = iserror ? Color.Red : Color.DarkSlateGray;
                var launcher = Root as Launcher;
                var line = (launcher.UseGamePrefix.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On ? "[GAME]" : string.Empty) + text + "\n";
                var start = Txt.TextLength;
                Txt.AppendText(line);
                var end = Txt.TextLength;
                Txt.Select(start, end - start);
                Txt.SelectionColor = color;
                Txt.SelectionLength = 0;
                Txt.ScrollToCaret();
            }
        }

        private void t_reader()
        {
            var flood = 0;
            var last = string.Empty;
            while (true)
            {
                try
                {
                    var line = String.Empty;
                    while (line.Trim() == String.Empty)
                    {
                        try
                        {
                            line = _client.StandardOutput.ReadLine();
                            if (last == line)
                                flood++;
                            else
                            {
                                flood = 0;
                                last = line;
                            }
                            try
                            {
                                if (flood >= 3) continue;
                                MLogG(line, false);
                            }
                            catch
                            {
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
        private void e_reader()
        {
            var flood = 0;
            var last = string.Empty;
            while (true)
            {
                try
                {
                    var line = String.Empty;
                    while (line.Trim() == String.Empty)
                    {
                        try
                        {
                            line = _client.StandardError.ReadLine();
                            if (last == line)
                                flood++;
                            else
                            {
                                flood = 0;
                                last = line;
                            }
                            try
                            {
                                if (flood >= 3) continue;
                                MLogG(line, true);
                            }
                            catch
                            {
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

        private static Thread _reader,  _errorReader;

        public void Launch()
        {
            var mroot = Root as Launcher;
            _client = new Process();
            var proc = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = JavaExec
            };
            GameDir = GameDir.Replace("${AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            try
            {
                if (!Directory.Exists(GameDir))
                    Directory.CreateDirectory(GameDir);
            }
            catch (Exception ex)
            {
                MLogG(ex.Message, true);
            }
            proc.WorkingDirectory = GameDir;
            var nativespath = "-Djava.library.path=" + Program.Minecraft + "\\natives";
            if (Libs.Contains(" ")) Libs = "\"" + Libs + "\"";
            if (nativespath.Contains(" ")) nativespath = "\"" + nativespath + "\"";
            var re = new Regex(@"\$\{(\w+)\}", RegexOptions.IgnoreCase);
            var values = new Dictionary<string, string>
            {
                {"auth_player_name", Variables.UserName},
                {"version_name", PName},
                {"game_directory", GameDir},
                {"assets_root", Assetspath},
                {"game_assets", Assetspath},
                {"assets_index_name", Assets},
                {"auth_session", Variables.AccessToken},
                {"auth_access_token", Variables.AccessToken},
                {"auth_uuid", Variables.ClientToken},
                {"user_properties", "{\"luncher\":[1234]}"},
                {"user_type", "mojang"}
            };
            Arg = re.Replace(Arg,
                match =>
                    !values[match.Groups[1].Value].Contains(' ')
                        ? values[match.Groups[1].Value]
                        : String.Format("\"{0}\"", values[match.Groups[1].Value]));
            Arg = Arg.Replace("${AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            proc.Arguments = String.Format("{0}{1} -cp {2} {3} {4}", JavaArgs, nativespath, Libs, MainClass, Arg);
            proc.StandardErrorEncoding = Encoding.UTF8;
            _client.StartInfo = proc;
            Logging.Info(mroot.LocRm.GetString("launch.workingdir") + " " + GameDir);
            Logging.Info(mroot.LocRm.GetString("launch.command") + " " + proc.FileName + " " + proc.Arguments);
            try
            {
                mroot.LaunchButtonChange(mroot.LocRm.GetString("launch.launchtext"), true);
                mroot.GetSelectedVersion(mroot.SelectProfile.SelectedItem.Text);
                _client.EnableRaisingEvents = true;
                if (_launcherVisibilityOnGameClose != 1)
                {
                    KillButton.Click += Kill;
                    _client.Exited += Client_Exited;
                    if (mroot.EnableMinecraftLogging.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    {
                        _reader = new Thread(t_reader);
                        _reader.Start();
                    }
                    _errorReader = new Thread(e_reader);
                    _errorReader.Start();
                    MLogG("Игра запущена", false);
                }
                Variables.ImStillRunning++;
                _client.Start();
                switch (_launcherVisibilityOnGameClose)
                {
                    case 1:
                        Variables.ImStillRunning--;
                        Application.Exit();
                        break;
                    case 2:
                        mroot.WindowState = FormWindowState.Minimized;
                        break;
                }
            }
            catch (Exception ex)
            {
                Variables.ImStillRunning--;
                mroot.Invoke((MethodInvoker) delegate
                {
                    CloseTabButton.Enabled = true;
                    KillButton.Enabled = false;
                });
                MLogG(mroot.LocRm.GetString("launch.error") + "\n" + ex, true);
                Logging.Error(mroot.LocRm.GetString("launch.error") + "\n" + ex);
                _reader.Abort();
                _errorReader.Abort();
                _client.Dispose();
            }
        }

        private void Client_Exited(object sender, EventArgs e)
        {
            Variables.ImStillRunning--;
            var mroot = Root as Launcher;
            var proc = sender as Process;
            mroot.Invoke((MethodInvoker)delegate
            {
                mroot.CleanNatives();
                CloseTabButton.Enabled = true;
                KillButton.Enabled = false;
                if (proc != null)
                    MLogG(("Процесс был завершён с кодом " + proc.ExitCode + ". Сеанс с " + proc.StartTime.ToString("HH:mm:ss") + "(Всего" + (Math.Round(proc.StartTime.Subtract(DateTime.Now).TotalMinutes, 2)).ToString().Replace('-', ' ') + " min)"), false);
                if (_launcherVisibilityOnGameClose != 2 || mroot.WindowState != FormWindowState.Minimized) return;
                mroot.WindowState = FormWindowState.Normal;
                mroot.Activate();
            });
            _reader.Abort();
            _errorReader.Abort();
            proc.Dispose();
        }

        private void Kill(object sender, EventArgs e)
        {
            if (Processing.IsRunning(_client)) _client.Kill();
        }
    }

    public abstract class McVariables
    {
        protected string GameDir { get; set; }
        protected string Arg { get; set; }
        protected string PName { get; set; }
        protected string Assetspath { get; set; }
        protected string JavaExec = Variables.JavaExe;
        protected string Libs { get; set; }
        protected string JavaArgs { get; set; }
        protected string Assets { get; set; }
        protected string MainClass { get; set; }
        protected string LastVersionId { get; set; }
    }
}
