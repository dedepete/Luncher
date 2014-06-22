using System.IO;

namespace Luncher
{
    public static class Variables
    {
        public static bool WorkingOffline;

        private static readonly string Minecraft = Program.Minecraft;
        // keep in mind these variables are available anywhere using Variables.varName
        public static readonly string LocalProfileList = Minecraft + "\\launcher_profiles.json";
        public static readonly string ProfileJsonFile = Minecraft + @"\launcher_profiles.json";
        public static string UserName;
        public static string ClientToken = "someInterestingClientToken";
        public static string AccessToken = "someInterestingAccessToken";
        public static string MainClass;
        public static readonly string McFolder = Minecraft;
        public static readonly string McVersions = Path.Combine(Minecraft, "versions");
        public static readonly string JavaExe = Processing.GetJavaInstallationPath() + @"\bin\java.exe";
        // custom versioning
        public const string NetJsonVersion = "6.0r3";
        public const string NetZipVersion = "1.9.2";
        public const string NdOptions = "0.2.1";
        // last versions
        public static string LastRelease;
        public static string LastSnapshot;
    }
}
