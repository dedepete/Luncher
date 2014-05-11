using System.IO;
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
