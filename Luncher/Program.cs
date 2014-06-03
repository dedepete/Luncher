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

        public static string Minecraft = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft";

        public static string Lang = "uk";

        public static string[] Arg;

        [STAThread]
        private static void Main(string[] args)
        {
            Arg = args;
            ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Dark";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
