using System;
using System.Drawing;
using System.Windows.Forms;
using Luncher;

namespace Luncher
{
    public static class Logging
    {
        public static void Log(int code, bool colored, string text)
        {
            var logBox = LogBox._LogBox as RichTextBox;
            string finalstring;
            var time = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
            var color = Color.Black;
            switch (code)
            {
                case 0:
                //Nothing TAG
                    finalstring = String.Format("{0}", text);
                    break;
                case 1:
                //Warning TAG
                    if (colored) color = Color.Orange;
                    finalstring = String.Format("[Luncher][WARNING][{0}] {1}", time, text);
                    break;
                case 2:
                //Error TAG
                    if (colored) color = Color.Red;
                    finalstring = String.Format("[Luncher][ERROR][{0}] {1}", time, text);
                    break;
                case 3:
                //Info TAG
                    finalstring = String.Format("[Luncher][INFO][{0}] {1}", time, text);
                    break;
            }
            if (logBox != null)
            {
                var start = logBox.TextLength;
                logBox.AppendText(String.Format("{0}\n", finalstring));
                var end = logBox.TextLength;
                logBox.Select(start, end - start);
                logBox.SelectionColor = color;
                logBox.SelectionLength = 0;
                logBox.ScrollToCaret();
            }
        }
    }

    public static class LogBox
    {
        public static object _LogBox;
    }
}
