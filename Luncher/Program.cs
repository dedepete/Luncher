using Luncher.Properties;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls;

namespace Luncher
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>

        public static string minecraft = "";

        public static string lang = "uk";

        [STAThread]
        private static void Main(string[] args)
        {
            ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Dark";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length != 0)
            {
                var p = new OptionSet()
                {
                    {
                        "d|directory=", "minecraft custom {PATH}.",
                        v => minecraft = v
                    },
                };
                try
                {
                    p.Parse(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("###########################");
                    Console.WriteLine(ex.ToString());
                    minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                }
                Application.Run(new MainForm());
            }
            else
            {
                minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
                Application.Run(new MainForm());
            }
        }
    }
}
