using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls;

namespace Luncher
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
         
        public static string minecraft = "";
        public static string lang = "uk";
        [STAThread]
        static void Main(string[] args)
        {
            ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Dark";
            if (args.Length != 0)
            {
                int argint = args.Length;
                while (argint != 0)
                {
                    argint = argint - 1;
                    if (args[argint].Contains("/directory="))
                    {
                        minecraft = args[argint].Split('=')[1];
                        minecraft = minecraft.Replace("$_", " ");
                    }
                }
            }
            else
            {
                minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
