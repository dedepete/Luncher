using System.IO;

namespace Luncher
{
    public static class Variables
    {
        public static bool WorkingOffline;
        public static int ImStillRunning;

        // basic variables
        public static string McFolder = Program.Minecraft;
        public static string McVersions = Path.Combine(McFolder, "versions\\");
        public static string ProfileJsonFile = string.Format("{0}\\launcher_profiles.json", McFolder);
        public static string JavaExe = string.Format("{0}\\bin\\java.exe", Processing.GetJavaInstallationPath());

        // minecraft basic settings
        public static string UserName;
        public static string ClientToken = "11i1111i11ii11iii1i1i11iiii11iii";
        public static string AccessToken = "1i1ii1i111ii1i1i1i1i1ii1ii1ii111";
        
        // custom versioning
        public const string NetJsonVersion = "6.0r8";
        public const string NetZipVersion = "1.9.3";
        public const string NdOptions = "0.2.1";

        // last versions
        public static string LastRelease;
        public static string LastSnapshot;
    }
}
