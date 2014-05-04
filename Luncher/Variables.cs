using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Luncher
{
    public static class Variables
    {
        public static bool workingOffline = false;

        private static string minecraft = Program.minecraft;
        // keep in mind these variables are available anywhere using Variables.varName
        public static string localProfileList = minecraft + "\\launcher_profiles.json";
        public static string profileJSONFile = minecraft + @"\launcher_profiles.json";
        public static string profileNamesJSONFile = minecraft + @"\user_profiles.json";
        public static string userName = null;
        public static string clientToken = "someInterestingClientToken";
        public static string accessToken = "someInterestingAccessToken";
        public static string mainClass = null;
        public static string MCFolder = minecraft;
        public static string MCVersions = Path.Combine(minecraft, "versions");
        public static string javaExe = GetJavaInstallationPath() + @"\bin\java.exe";
        // custom versioning
        public static string netJsonVersion = "6.0r3";
        public static string netZipVersion = "1.9.2";
        public static string NDOptions = "0.2.1";
        // last versions
        public static string lastRelease = null;
        public static string lastSnapshot = null;
        //
        public static String GetJavaInstallationPath()
        {
            try
            {
                String javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
                {
                    String currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                    {
                        return homeKey.GetValue("JavaHome").ToString();
                    }
                }
            }
            catch
            {
                String javaKey = "SOFTWARE\\Wow6432Node\\JavaSoft\\Java Runtime Environment";
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
                {
                    String currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                    {
                        return homeKey.GetValue("JavaHome").ToString();
                    }
                }
            }
        }
    }
}
