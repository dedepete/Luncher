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
    public static class Processing
    {
        public static string GetProfileDetails(string profile)
        {
            string json2return = null;
            string jstring = File.ReadAllText(Variables.profileJSONFile);
            JObject json = JObject.Parse(jstring);
            json2return = json["profiles"][profile].ToString();
            return json2return;
        }
    }
}
