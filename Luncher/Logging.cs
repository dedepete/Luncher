using System;
using System.Drawing;
using System.Windows.Forms;

namespace Luncher
{
    public static class Logging
    {
        private static void Log(int type, bool showprefix, bool colored, string text)
        {
            var logBox = LogBox.Box as RichTextBox;
            string finalstring;
            var time = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
            var color = Color.Black;
            switch (type)
            {
                case 1:
                    if (colored) color = Color.Orange;
                    finalstring = String.Format(showprefix ? "[{0}][WARNING][{1}] {2}" : "{2}", LogBox.ProductName, time,
                        text);
                    break;
                case 2:
                    if (colored) color = Color.Red;
                    finalstring = String.Format(showprefix ? "[{0}][ERROR][{1}] {2}" : "{2}", LogBox.ProductName, time,
                        text);
                    break;
                default:
                    finalstring = String.Format(showprefix ? "[{0}][INFO][{1}] {2}" : "{2}", LogBox.ProductName, time,
                        text);
                    break;
            }
            Console.WriteLine(finalstring);
            if (logBox == null) return;
            logBox.SelectionColor = color;
            var start = 0;
            if (colored) start = logBox.TextLength;
            logBox.AppendText(String.Format(String.IsNullOrEmpty(logBox.Text) ? "{0}" : "\n{0}", finalstring));
            if (colored)
            {
                var end = logBox.TextLength;
                logBox.Select(start, end - start);
                logBox.SelectionColor = color;
                logBox.SelectionLength = 0;
            }
            logBox.ScrollToCaret();
        }

        private static void Processing(string message, int t, params string[] args)
        {
            var colored = false;
            var pfx = true;
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
            Processing(message, 0, "c:false");
        }

        public static void Error(string message, params string[] args)
        {
            Processing(message, 2, args);
        }

        public static void Error(string message)
        {
            Processing(message, 2, "c:true", "pfx:true");
        }

        public static void Warning(string message, params string[] args)
        {
            Processing(message, 1, args);
        }

        public static void Warning(string message)
        {
            Processing(message, 1, "c:true", "pfx:true");
        }
    }

    public static class LogBox
    {
        public static object Box;

        public static string ProductName;
    }
}
