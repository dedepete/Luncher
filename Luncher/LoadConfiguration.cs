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
                        string[] parsingvalue = a.Split("="[0]);
                        string pvalue = parsingvalue[1];
                        string[] parse = parsingvalue[0].Split('.');
                        string main = parse[0];
                        string token = parse[1];
                        string token2 = parse[2];
                        if (main == "luncher")
                        {
                            if (token == "main")
                            {
                                if (token2 == "lang")
                                {
                                    mainlang = pvalue;
                                }
                                if (token2 == "renamewindow")
                                {
                                    mainrenamewindow = pvalue;
                                }
                            }
                            else if (token == "gamelogging")
                            {
                                if (token2 == "enable")
                                {
                                    gamelogging = pvalue;
                                }
                                else if (token2 == "usegameprefix")
                                {
                                    gameloggingusegameprefix = pvalue;
                                }
                            }
                            else if (token == "updater")
                            {
                                if (token2 == "updateversions")
                                {
                                    updaterupdateversions = pvalue;
                                }
                                else if (token2 == "updateprogram")
                                {
                                    updaterupdateprogram = pvalue;
                                }
                                else if (token2 == "alerts")
                                {
                                    updateralerts = pvalue;
                                }
                            }
                            else if (token == "resources")
                            {
                                if (token2 == "enablerebuilding")
                                {
                                    resoucersenablerebuilding = pvalue;
                                }
                                else if (token2 == "rebuildresource")
                                {
                                    resoucerrebuildresource = pvalue;
                                }
                                else if (token2 == "assetspath")
                                {
                                    resoucerassetspath = pvalue;
                                }
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
                StringBuilder sb = new StringBuilder();
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
