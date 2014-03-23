using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Luncher
{
    public static class WebStuff
    {
        public static string Signout()
        {
            JObject JSON2POST = new JObject(
                new JProperty("accessToken", Variables.accessToken),
                new JProperty("clientToken", Variables.clientToken));
            string JSON = JSON2POST.ToString();
            File.Delete(Variables.userFile);
            Variables.userName = null;
            Variables.clientToken = null;
            Variables.accessToken = null;
            string response = WebStuff.PostHTTP(JSON, "invalidate");
            return response;
        }
        public static string GetJSONFromWeb(string url)
        {
            string toReturn = null;
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, "temp.json");
            }
            toReturn = File.ReadAllText("temp.json");
            File.Delete("temp.json");
            return toReturn;
        }
        public static string PostHTTP(string JSON, string whatDo)
        { // valid whatDo = authenticate, refresh, validate, signout, invalidate
            // details on operations at http://wiki.vg/Session
            string responseJSON = null;
            try
            {
                var req = (HttpWebRequest)WebRequest.Create(Variables.authServer + whatDo);
                req.ContentType = "application/json";
                req.Method = "POST";

                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    sw.Write(JSON);
                    sw.Flush();
                    sw.Close();

                    var resp = (HttpWebResponse)req.GetResponse();
                    using (var sr = new StreamReader(resp.GetResponseStream()))
                    {
                        responseJSON = sr.ReadToEnd();
                    }
                }
            }
            catch (WebException e)
            {
                if (whatDo == "authenticate")
                {
                    if (e.ToString().Contains("403"))
                    {
                        MessageBox.Show("Wrong username/password combination!\r\nMake sure you are using your migrated email if you migrated!");
                        return null;
                    }
                }
                MessageBox.Show(e.Message);
            }
            // MessageBox.Show(responseJSON);
            return responseJSON;
        }
    }
    public static class Processing
    {
        public static void CopyDirectory(string orig, string dirTo)
        {
            DirectoryInfo dir = new DirectoryInfo(orig);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory not found: " + orig);
            }
            if (!Directory.Exists(dirTo))
                Directory.CreateDirectory(dirTo);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temp = Path.Combine(dirTo, file.Name);
                file.CopyTo(temp, true);
            }
        }
        public static string GetProfileDetails(string profile)
        {
            string json2return = null;
            string jstring = File.ReadAllText(Variables.profileJSONFile);
            JObject json = JObject.Parse(jstring);
            //MessageBox.Show(profile);
            json2return = json["profiles"][profile].ToString();
            //MessageBox.Show(json2return);
            return json2return;
        }
        public static void SaveUserInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Variables.userName + "- This is the username");
            sb.AppendLine(Variables.clientToken + "- This is the clientToken");
            sb.AppendLine(Variables.accessToken + "- This is the accessToken");
            File.WriteAllText(Variables.userFile, sb.ToString());
        }
        public static void SaveSettings(bool option1)
        {
            bool origOption1 = CheckSettings1();
            XDocument doc = XDocument.Load(Variables.setFile);
            foreach (XElement settings in doc.Descendants("settings")
                .Where(list => Convert.ToBoolean(list.Element("origAppData").Value) != option1))
            {
                settings.Element("origAppData").Value = option1.ToString();
            }
            doc.Save(Variables.setFile);
        }
        public static bool CheckSettings1() // Check option #1 (should run in original %appdata%)
        {
            XDocument doc = XDocument.Load(Variables.setFile);
            var data = from item in doc.Descendants("settings")
                       select new
                       {
                           option1 = item.Element("origAppData").Value
                       };
            string temp;
            foreach (var poop in data)
            {
                temp = poop.ToString().Replace("{ option1 = ", "");
                temp = temp.Replace("}", "").Trim();
                try
                {
                    return Convert.ToBoolean(temp);
                }
                catch (FormatException x)
                {
                    MessageBox.Show(x.Message);
                }
                catch (InvalidCastException x)
                {
                    MessageBox.Show(x.Message);
                }
            }
            return false;
        }
    }
}
