using System;
using System.Windows.Forms;
using Telerik.WinControls;

namespace Luncher
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>

        public static string minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";

        public static string lang = "uk";

        public static string[] arg;

        [STAThread]
        private static void Main(string[] args)
        {
            arg = args;
            ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Dark";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
