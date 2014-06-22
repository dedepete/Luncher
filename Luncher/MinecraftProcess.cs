using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace Luncher
{
    public class MinecraftProcess
    {
        private object Root { get; set; }
        private string GameDir { get; set; }
        private string Arg { get; set; }
        private string PName { get; set; }
        private string Assetspath { get; set; }
        private string JavaExec { get; set; }
        private string Libs { get; set; }
        private string JavaArgs { get; set; }
        private string Assets { get; set; }

        private RichTextBox Txt { get; set; }

        private string LastVersionId { get; set; }

        private Process _client;

        private string _errors;
        private string _logs;

        private int _tflood;
        string _tlast;
        private int _eflood;
        string _elast;

        public MinecraftProcess(object mainForm, string gameDirectory, string arguments, string profileName, string assetsPath, string javaExec, string libraries, string javaArguments, string assetsFileName, string lastVersionId, RichTextBox txt)
        {
            Root = mainForm;
            GameDir = gameDirectory;
            Arg = arguments;
            PName = profileName;
            Assetspath = assetsPath;
            JavaExec = javaExec;
            Libs = libraries;
            JavaArgs = javaArguments;
            Assets = assetsFileName;
            LastVersionId = lastVersionId;
            Txt = txt;
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowText(IntPtr hWnd, string text);

        private void MLogG(string text, bool iserror, RichTextBox txt)
        {
            var color = iserror ? Color.Red : Color.DarkSlateGray;
            string line;
            var launcher = Root as Launcher;
            if (launcher != null && launcher.UseGamePrefix.ToggleState == Telerik.WinControls.Enumerations.ToggleState.On)
            {
                line = "[GAME]" + text + "\n";
            }
            else
            {
                line = text + "\n";
            }
            var start = txt.TextLength;
            txt.AppendText(line);
            var end = txt.TextLength;
            txt.Select(start, end - start);
            {
                txt.SelectionColor = color;
            }
            txt.SelectionLength = 0;
            txt.ScrollToCaret();
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
                            {
                                _tflood++;
                            }
                            else
                            {
                                _tflood = 0;
                                _tlast = line;
                            }
                            try
                            {
                                if (line.Contains("Attempting early MinecraftForge initialization") && _rnw)
                                {
                                    mroot.Invoke((MethodInvoker) delegate
                                    {
                                        _rnw = false;
                                        MLogG("[Forge]Инициализация Minecraft Forge...", false, Txt);
                                    });
                                }
                                if (line.Contains("Sound engine started") && _rnw == false)
                                {
                                    mroot.Invoke((MethodInvoker) delegate
                                    {
                                        _rnw = true;
                                        MLogG("[Forge]Инициализация Minecraft Forge закончена", false, Txt);
                                    });
                                }
                                if (_tflood < 3)
                                {
                                    mroot.Invoke((MethodInvoker) (() => MLogG(line, false, Txt)));
                                    _logs = _logs + "\n" + line;
                                }
                                if (!_rnw) continue;
                                switch (mroot.RenameWindow.SelectedIndex)
                                {
                                    case 0:
                                        SetWindowText(_client.MainWindowHandle,
                                            "Minecraft - " + mroot.LastVersionId + " - " + mroot.ProductName + " " +
                                            mroot.ProductVersion);
                                        break;
                                    case 2:
                                        SetWindowText(_client.MainWindowHandle, "Minecraft");
                                        break;
                                }
                            }
                            catch
                            {
                            }
                        }
                        catch { }


                    }

                }
                catch (NullReferenceException)
                {
                    break;
                }
            }
        }

        bool _rnw = true;
        void e_reader()
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
                            {
                                _eflood++;
                            }
                            else
                            {
                                _eflood = 0;
                                _elast = line;
                            }
                            if (line.Contains("Attempting early MinecraftForge initialization"))
                            {
                                mroot.Invoke((MethodInvoker) delegate
                                {
                                    _rnw = false;
                                    MLogG("[Forge]Инициализация Minecraft Forge...", false, Txt);
                                });
                            }
                            if (line.Contains("Sound engine started"))
                            {
                                mroot.Invoke((MethodInvoker) delegate
                                {
                                    _rnw = true;
                                    MLogG("[Forge]Инициализация Minecraft Forge закончена", false, Txt);
                                });
                            }
                            try
                            {
                                if (_eflood < 3)
                                {
                                    mroot.Invoke((MethodInvoker) (() => MLogG(line, true, Txt)));
                                    _errors = _errors + "\n" + line;
                                }
                                if (!_rnw) continue;
                                switch (mroot.RenameWindow.SelectedIndex)
                                {
                                    case 0:
                                        SetWindowText(_client.MainWindowHandle,
                                            "Minecraft - " + LastVersionId + " - " + mroot.ProductName + " " +
                                            mroot.ProductVersion);
                                        break;
                                    case 2:
                                        SetWindowText(_client.MainWindowHandle, "Minecraft");
                                        break;
                                }
                            }
                            catch
                            {
                            }
                        }
                        catch { }
                    }
                }
                catch (NullReferenceException) { break; }
            }
        }

        private static Thread _reader;
        private static Thread _errorReader;
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
            proc.WorkingDirectory = GameDir;
            var nativespath = "-Djava.library.path=" + Program.Minecraft + "/natives";
            if (GameDir.Contains(" ")) GameDir = "\"" + GameDir + "\"";
            if (Libs.Contains(" ")) Libs = "\"" + Libs + "\"";
            if (Assetspath.Contains(" ")) Assetspath = "\"" + Assetspath + "\"";
            if (nativespath.Contains(" ")) nativespath = "\"" + nativespath + "\"";
            Arg = Arg.Replace("${auth_player_name}", Variables.UserName);
            Arg = Arg.Replace("${version_name}", PName);
            Arg = Arg.Replace("${game_directory}", GameDir);
            Arg = Arg.Replace("${assets_root}", Assetspath);
            Arg = Arg.Replace("${game_assets}", Assetspath);
            Arg = Arg.Replace("${assets_index_name}", Assets);
            Arg = Arg.Replace("${auth_session}", Variables.AccessToken);
            Arg = Arg.Replace("${auth_access_token}", Variables.AccessToken);
            Arg = Arg.Replace("${auth_uuid}", Variables.ClientToken);
            Arg = Arg.Replace("${user_properties}", "{\"luncher\":[1234]}");
            Arg = Arg.Replace("${AppData}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            Arg = Arg.Replace("${user_type}", "mojang");
            Arg = Arg.Replace("\\\"", "\"");
            proc.Arguments = JavaArgs + nativespath + " -cp " + Libs + " " + Variables.MainClass + " " + Arg;
            proc.StandardErrorEncoding = Encoding.UTF8;
            _client.StartInfo = proc;
            if (mroot == null) return;
            Logging.Info(mroot.LocRm.GetString("launch.workingdir") + " " + GameDir);
            Logging.Info(mroot.LocRm.GetString("launch.command") + " " + proc.FileName + " " + proc.Arguments);
            try
            {
                mroot.LaunchButtonChange(mroot.LocRm.GetString("launch.launchtext"), true);
                mroot.GetSelectedVersion(mroot.SelectProfile.SelectedItem.Text);
                _client.EnableRaisingEvents = true;
                if (!mroot.Cl)
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
                _client.Start();
                if (mroot.Hl) mroot.WindowState = FormWindowState.Minimized;
                if (mroot.Cl) Application.Exit();
            }
            catch (Exception ex)
            {
                Logging.Error(mroot.LocRm.GetString("launch.error") + "\n" + ex);
            }
        }
        private void Client_Exited(object sender, EventArgs e)
        {
            var mroot = Root as Launcher;
            var proc = sender as Process;
            if (mroot != null)
                mroot.Invoke((MethodInvoker)delegate
                {
                    var radButton = Txt.Tag as RadButton;
                    if (radButton != null) radButton.Enabled = true;
                    mroot.CleanNatives();
                    if (proc != null)
                        MLogG(("Процесс был завершён с кодом " + proc.ExitCode + ". Сеанс с " + proc.StartTime.ToString("HH:mm:ss") + "(Всего" + (Math.Round(proc.StartTime.Subtract(DateTime.Now).TotalMinutes, 2)).ToString().Replace('-', ' ') + " min)"), false, Txt);
                    if (!mroot.Hl || mroot.WindowState != FormWindowState.Minimized) return;
                    mroot.WindowState = FormWindowState.Normal;
                    mroot.Hl = false;
                    mroot.Activate();
                });
        }
    }
}
