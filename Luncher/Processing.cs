using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Telerik.WinControls.UI;

namespace Luncher
{
    public static class Processing
    {

        public static string GetProfileDetails(string profile)
        {
            var jstring = File.ReadAllText(Variables.ProfileJsonFile);
            var json = JObject.Parse(jstring);
            var json2Return = json["profiles"][profile].ToString();
            return json2Return;
        }
        public static void GetVersions(RadListView list)
        {
            foreach (var s in Directory.GetDirectories(Variables.McFolder + "/versions/"))
            {
                var versionname = new DirectoryInfo(s).Name;
                if (
                    !(File.Exists(Variables.McFolder + "/versions/" + versionname + "/" + versionname + ".jar") &
                      File.Exists(Variables.McFolder + "/versions/" + versionname + "/" + versionname + ".json")))
                    continue;
                var json = JObject.Parse(File.ReadAllText(Variables.McFolder + "/versions/" + versionname + "/" + versionname + ".json"));
                var id = "null";
                var type = "null";
                var time = "null";
                try
                {
                    id = json["id"].ToString();
                    type = json["type"].ToString();
                    time = json["releaseTime"].ToString();
                }
                catch (Exception ex)
                {
                    Logging.Error(ex.ToString());
                }
                list.Items.Add(id, type, time);
            }
        }
        public static JToken Rename(JToken json, Func<string, string> map)
        {
            var prop = json as JProperty;
            if (prop != null)
            {
                return new JProperty(map(prop.Name), Rename(prop.Value, map));
            }

            var arr = json as JArray;
            if (arr != null)
            {
                var cont = arr.Select(el => Rename(el, map));
                return new JArray(cont);
            }

            var o = json as JObject;
            if (o != null)
            {
                var cont = o.Properties().Select(el => Rename(el, map));
                return new JObject(cont);
            }

            return json;
        }
        public static String GetJavaInstallationPath()
        {
            try
            {
                const string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
                using (
                    var baseKey =
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
                {
                    if (baseKey != null)
                    {
                        var currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                        using (var homeKey = baseKey.OpenSubKey(currentVersion))
                        {
                            if (homeKey != null) return homeKey.GetValue("JavaHome").ToString();
                        }
                    }
                }
            }
            catch
            {
                const string javaKey = "SOFTWARE\\Wow6432Node\\JavaSoft\\Java Runtime Environment";
                using (
                    var baseKey =
                        RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
                {
                    if (baseKey == null) return null;
                    var currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                    {
                        if (homeKey != null) return homeKey.GetValue("JavaHome").ToString();
                    }
                }
            }
            return null;
        }
        public static bool IsRunning(Process process)
        {
            if (process == null) return false;
            try
            {
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }
    }
}
