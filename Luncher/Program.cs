using System;
using System.Windows.Forms;
using Luncher.Forms;
using Telerik.WinControls;

namespace Luncher
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>

        public static string Minecraft = String.Format("{0}\\.minecraft", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public static string Lang = "";

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
