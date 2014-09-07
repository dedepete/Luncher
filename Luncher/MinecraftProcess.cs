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
using Newtonsoft.Json;
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

        private string _errors;
        private string _logs;

        private int _tflood;
        private string _tlast;
        private int _eflood;
        private string _elast;

        private readonly int _launcherVisibilityOnGameClose;

        public MinecraftProcess(object mainForm, string assetsPath, string libraries, string assetsFileName, string jsonblock)
        {
            var json = JsonConvert.DeserializeObject<JsonProfile.Profile>(jsonblock);
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
                switch (json.launcherVisibilityOnGameClose)
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
            var profileSJson = File.ReadAllText(nativesFolder + @"\" + LastVersionId + ".json");
            var profilejsono = JObject.Parse(profileSJson);
            MainClass = profilejsono["mainClass"].ToString();
            Arg = profilejsono["minecraftArguments"] +
                  (ip != null ? String.Format(" --server {0} --port {1}", ip, (port ?? "25565")) : String.Empty);
            libraries += String.Format(";{0}\\{1}.jar", nativesFolder, LastVersionId);
            var va = ((Launcher)mainForm).LogTab("Minecraft version: " + LastVersionId, PName);
            Txt = (RichTextBox) va[0];
            KillButton = (RadButton) va[1];
            CloseTabButton = (RadButton) va[2];
            Root = mainForm;
            Assetspath = assetsPath;
            Libs = libraries;
            Assets = assetsFileName;
        }

        private void MLogG(string text, bool iserror)
        {
            var color = iserror ? Color.Red : Color.DarkSlateGray;
            string line;
            var launcher = Root as Launcher;
            if (launcher != null &&
                launcher.UseGamePrefix.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                line = "[GAME]" + text + "\n";
            else
                line = text + "\n";
            var start = Txt.TextLength;
            Txt.AppendText(line);
            var end = Txt.TextLength;
            Txt.Select(start, end - start);
            Txt.SelectionColor = color;
            Txt.SelectionLength = 0;
            Txt.ScrollToCaret();
        }

        private void t_reader()
        {
            while (true)
            {
                var mroot = Root as Launcher;
                var line = "";
                try
                {
                    while (line.Trim() == "")
                    {
                        try
                        {
                            line = _client.StandardOutput.ReadLine();
                            if (_tlast == line)
                                _tflood++;
                            else
                            {
                                _tflood = 0;
                                _tlast = line;
                            }
                            try
                            {
                                if (_tflood >= 3) continue;
                                mroot.Invoke((MethodInvoker) (() => MLogG(line, false)));
                                _logs = _logs + "\n" + line;
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
            while (true)
            {
                var mroot = Root as Launcher;
                try
                {
                    var line = "";
                    while (line.Trim() == "")
                    {
                        try
                        {
                            line = _client.StandardError.ReadLine();
                            if (_elast == line)
                                _eflood++;
                            else
                            {
                                _eflood = 0;
                                _elast = line;
                            }
                            try
                            {
                                if (_eflood >= 3) continue;
                                mroot.Invoke((MethodInvoker) (() => MLogG(line, true)));
                                _errors = _errors + "\n" + line;
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

        private static Thread _reader;
        private static Thread _errorReader;

        public void Launch()
        {
            KillButton.Click += Kill;
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
            catch(Exception ex)
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
                    _client.Exited += Client_Exited;
                    if (mroot.EnableMinecraftLogging.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
                    {
                        _reader = new Thread(t_reader);
                        _reader.Start();
                    }
                    _errorReader = new Thread(e_reader);
                    _errorReader.Start();
                }
                Variables.ImStillRunning++;
                _client.Start();
                switch (_launcherVisibilityOnGameClose)
                {
                    case 1:
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
