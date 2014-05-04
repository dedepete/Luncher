using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Luncher
{
    public class Auth
    {
        public string user { get; set; }
        public string password { get; set; }

        public String Authenticate()
        {
            string json = JObject.Parse(AuthShemes.authenticatesheme).ToString();
            json = json.Replace("${username}", user).Replace("${password}", password);
            try
            {
                JObject jo = JObject.Parse(MakePOST.mPOSTJSON(AuthShemes.authserver + AuthShemes.authenticate, json));
                Username uname = new Username()
                {
                    uuid = jo["selectedProfile"]["id"].ToString()
                };
                return uname.GetUsernameByUUID() + ":" + jo["accessToken"].ToString() + ":" + jo["clientToken"].ToString() + ":" + jo["selectedProfile"]["id"].ToString();
            }
            catch (Exception ex)
            {
                return "Error occupied while autheticating.\n" + ex.ToString();
            }
        }
        public String Logout()
        {
            string json = JObject.Parse(AuthShemes.signoutsheme).ToString();
            json = json.Replace("${username}", user).Replace("${password}", password);
            if (MakePOST.mPOSTJSON(AuthShemes.authserver + AuthShemes.signout, json) == string.Empty)
            {
                return "Successful";
            }
            else
            {
                return "Unsuccessful";
            }
        }
    }

    public class Username
    {
        public string uuid { get; set; }

        public String GetUsernameByUUID()
        {
            JObject jo = JObject.Parse(new WebClient().DownloadString("https://sessionserver.mojang.com/session/minecraft/profile/" + uuid));
            return jo["name"].ToString();
        }
    }
    public static class MakePOST
    {
        public static String mPOSTJSON(string httpreq, string topost)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(topost);
                var request = (HttpWebRequest) WebRequest.Create(httpreq);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = body.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(body, 0, body.Length);
                    stream.Close();
                }
                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception av)
            {
                return "Error\n" + av.ToString();
            }
        }
    }
}
