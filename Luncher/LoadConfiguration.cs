using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Luncher
{
    public static class LoadConfiguration
    {
        public static string Gamelogging = "True";
        public static string GameLoggingusegameprefix = "False";

        public static string Updaterupdateversions = "True";
        public static string Updaterupdateprogram = "True";
        public static string Updateralerts = "True";

        public static string Resoucersenablerebuilding = "True"; 
        public static string Resoucerrebuildresource = "1.7.4";
        public static string Resoucerassetspath = "${AppData}\\.minecraft\\assets\\";

        public static string Mainlang = "";
        public static string Mainrenamewindow = "1";

        public static void LoadConfigurationFile()
        {
            if (File.Exists(Path.Combine(Program.Minecraft, "luncher", "configuration.cfg")))
            {
                foreach (var a in File.ReadAllLines(Program.Minecraft + "\\luncher\\configuration.cfg"))
                {
                    if (String.IsNullOrEmpty(a) || a.Contains('#')) continue;
                    var parsingvalue = a.Split("="[0]);
                    var pvalue = parsingvalue[1];
                    var parse = parsingvalue[0].Split('.');
                    var main = parse[0];
                    var token = parse[1];
                    var token2 = parse[2];
                    if (main != "luncher") continue;
                    switch (token)
                    {
                        case "main":
                            switch (token2)
                            {
                                case "lang":
                                    Mainlang = pvalue;
                                    break;
                                case "renamewindow":
                                    Mainrenamewindow = pvalue;
                                    break;
                            }
                            break;
                        case "logging":
                            switch (token2)
                            {
                                case "enable":
                                    Gamelogging = pvalue;
                                    break;
                                case "usegameprefix":
                                    GameLoggingusegameprefix = pvalue;
                                    break;
                            }
                            break;
                        case "updater":
                            switch (token2)
                            {
                                case "updateversions":
                                    Updaterupdateversions = pvalue;
                                    break;
                                case "updateprogram":
                                    Updaterupdateprogram = pvalue;
                                    break;
                                case "alerts":
                                    Updateralerts = pvalue;
                                    break;
                            }
                            break;
                        case "resources":
                            switch (token2)
                            {
                                case "enablerebuilding":
                                    Resoucersenablerebuilding = pvalue;
                                    break;
                                case "rebuildresource":
                                    Resoucerrebuildresource = pvalue;
                                    break;
                                case "assetspath":
                                    Resoucerassetspath = pvalue;
                                    break;
                            }
                            break;
                    }
                }
            }
            else
            {
                var path = Path.Combine(Program.Minecraft, "luncher");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var sb = new StringBuilder();
                sb.AppendLine("#Luncher configuration file");
                sb.AppendLine("#");
                sb.AppendLine();
                sb.AppendLine("luncher.main.lang=");
                sb.AppendLine("luncher.main.renamewindow=1");
                sb.AppendLine();
                sb.AppendLine("luncher.logging.enable=True");
                sb.AppendLine("luncher.logging.usegameprefix=False");
                sb.AppendLine();
                sb.AppendLine("luncher.updater.updateversions=True");
                sb.AppendLine("luncher.updater.updateprogram=True");
                sb.AppendLine("luncher.updater.alerts=True");
                sb.AppendLine();
                sb.AppendLine("luncher.resources.enablerebuilding=True");
                sb.AppendLine("luncher.resources.rebuildresource=1.7.4");
                sb.AppendLine("luncher.resources.assetspath=${AppData}\\.minecraft\\assets\\");
                File.WriteAllText(Program.Minecraft + "\\luncher\\configuration.cfg", sb.ToString());
            }
        }
    }
}
