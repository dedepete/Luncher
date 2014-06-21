using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace Luncher
{
    public class Auth
    {
        public string User { private get; set; }
        public string Password { private get; set; }

        public String Authenticate()
        {
            Logging.Log("", true, false,  "Authenticating...");
            var json = JObject.Parse(AuthShemes.Authenticatesheme).ToString();
            json = json.Replace("${username}", User).Replace("${password}", Password);
            var response = MakePost.MPostjson(AuthShemes.Authserver + AuthShemes.Authenticate, json);
            try
            {
                var jo = JObject.Parse(response);
                var uname = new Username
                {
                    Uuid = jo["selectedProfile"]["id"].ToString()
                };
                return uname.GetUsernameByUuid() + ":" + jo["accessToken"] + ":" + jo["clientToken"] + ":" + jo["selectedProfile"]["id"];
            }
            catch
            {
                string cause = null;
                if (response == "ProtocolError")
                {
                    cause = "Invalid credentials";
                }
                return cause;
            }
        }
        public String Logout()
        {
            string json = JObject.Parse(AuthShemes.Signoutsheme).ToString();
            json = json.Replace("${username}", User).Replace("${password}", Password);
            return MakePost.MPostjson(AuthShemes.Authserver + AuthShemes.Signout, json) == string.Empty ? "Successful" : "Unsuccessful";
        }
    }

    public class Username
    {
        public string Uuid { private get; set; }

        public String GetUsernameByUuid()
        {
            var jo = JObject.Parse(new WebClient().DownloadString("https://sessionserver.mojang.com/session/minecraft/profile/" + Uuid));
            return jo["name"].ToString();
        }
    }
    public static class MakePost
    {
        public static String MPostjson(string httpreq, string topost)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(topost);
                var request = (HttpWebRequest) WebRequest.Create(httpreq);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = body.Length;
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(body, 0, body.Length);
                    stream.Close();
                }
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (WebException av)
            {
                return av.Status.ToString();
            }
        }
    }
}
