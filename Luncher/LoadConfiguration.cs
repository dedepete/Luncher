using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Luncher
{
    public static class LoadConfiguration
    {
        public static string gamelogging = "True";
        public static string gameloggingusegameprefix = "False";

        public static string updaterupdateversions = "True";
        public static string updaterupdateprogram = "True";
        public static string updateralerts = "True";

        public static string resoucersenablerebuilding = "True"; 
        public static string resoucerrebuildresource = "1.7.4";
        public static string resoucerassetspath = "${AppData}\\.minecraft\\assets\\";

        public static string mainlang = "";
        public static string mainrenamewindow = "1";

        public static void LoadConfigurationFile()
        {
            if (File.Exists(Program.minecraft + "\\luncher\\configuration.cfg"))
            {
                foreach (string a in File.ReadAllLines(Program.minecraft + "\\luncher\\configuration.cfg"))
                {
                    if (!String.IsNullOrEmpty(a) && !a.Contains('#'))
                    {
                        var parsingvalue = a.Split("="[0]);
                        var pvalue = parsingvalue[1];
                        var parse = parsingvalue[0].Split('.');
                        var main = parse[0];
                        var token = parse[1];
                        string token2 = parse[2];
                        if (main == "luncher")
                        {
                            switch (token)
                            {
                                case "main":
                                    switch (token2)
                                    {
                                        case "lang":
                                            mainlang = pvalue;
                                            break;
                                        case "renamewindow":
                                            mainrenamewindow = pvalue;
                                            break;
                                    }
                                    break;
                                case "gamelogging":
                                    switch (token2)
                                    {
                                        case "enable":
                                            gamelogging = pvalue;
                                            break;
                                        case "usegameprefix":
                                            gameloggingusegameprefix = pvalue;
                                            break;
                                    }
                                    break;
                                case "updater":
                                    switch (token2)
                                    {
                                        case "updateversions":
                                            updaterupdateversions = pvalue;
                                            break;
                                        case "updateprogram":
                                            updaterupdateprogram = pvalue;
                                            break;
                                        case "alerts":
                                            updateralerts = pvalue;
                                            break;
                                    }
                                    break;
                                case "resources":
                                    switch (token2)
                                    {
                                        case "enablerebuilding":
                                            resoucersenablerebuilding = pvalue;
                                            break;
                                        case "rebuildresource":
                                            resoucerrebuildresource = pvalue;
                                            break;
                                        case "assetspath":
                                            resoucerassetspath = pvalue;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(Program.minecraft + "\\luncher\\");
                }
                catch { }
                var sb = new StringBuilder();
                sb.AppendLine("#Luncher configuration file");
                sb.AppendLine("#");
                sb.AppendLine();
                sb.AppendLine("luncher.main.lang=");
                sb.AppendLine("luncher.main.renamewindow=1");
                sb.AppendLine();
                sb.AppendLine("luncher.gamelogging.enable=True");
                sb.AppendLine("luncher.gamelogging.usegameprefix=False");
                sb.AppendLine();
                sb.AppendLine("luncher.updater.updateversions=True");
                sb.AppendLine("luncher.updater.updateprogram=True");
                sb.AppendLine("luncher.updater.alerts=True");
                sb.AppendLine();
                sb.AppendLine("luncher.resources.enablerebuilding=True");
                sb.AppendLine("luncher.resources.rebuildresource=1.7.4");
                sb.AppendLine("luncher.resources.assetspath=${AppData}\\.minecraft\\assets\\");
                File.WriteAllText(Program.minecraft + "\\luncher\\configuration.cfg", sb.ToString());
            }
        }
    }
}
