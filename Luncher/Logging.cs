using System;
using System.Drawing;

namespace Luncher
{
    public static class Logging
    {
        private enum ErrorState { WARNING, ERROR, INFO }
        private static void Log(ErrorState state, bool showprefix, bool colored, string text)
        {
            var logBox = LogBox.Box;
            var time = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
            var color = state != ErrorState.INFO && colored
                ? (state == ErrorState.ERROR ? Color.Red : Color.Orange)
                : Color.Black;
            var finalstring = string.Format(showprefix ? "[{0}][{1}][{2}] {3}" : "{3}", LogBox.ProductName, state, time,
                text);
            Console.WriteLine(finalstring);
            if (logBox == null) return;
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = color;
            logBox.AppendText(string.Format(string.IsNullOrEmpty(logBox.Text) ? "{0}" : "\n{0}", finalstring));
            logBox.SelectionColor = logBox.ForeColor;
            logBox.ScrollToCaret();
        }

        private static void Processing(string message, ErrorState t, params string[] args)
        {
            bool colored = false, pfx = true;
            if (args != null)
                foreach (var a in args)
                {
                    var name = a.Split(':')[0];
                    var value = a.Split(':')[1];
                    switch (name)
                    {
                        case "c": // is colored
                            colored = value == "true";
                            break;
                        case "pfx": // prefix
                            pfx = value == "true";
                            break;
                        default:
                            continue;
                    }
                }
            Log(t, pfx, colored, message);
        }

        public static void Info(string message, params string[] args)
        {
            Processing(message, 0, args);
        }

        public static void Info(string message)
        {
            Processing(message, ErrorState.INFO, "c:false");
        }

        public static void Error(string message, params string[] args)
        {
            Processing(message, ErrorState.ERROR, args);
        }

        public static void Error(string message)
        {
            Processing(message, ErrorState.ERROR, "c:true", "pfx:true");
        }

        public static void Warning(string message, params string[] args)
        {
            Processing(message, ErrorState.WARNING, args);
        }

        public static void Warning(string message)
        {
            Processing(message, ErrorState.WARNING, "c:true", "pfx:true");
        }
    }

    public static class LogBox
    {
        public static dynamic Box;
        public static string ProductName;
    }
}
