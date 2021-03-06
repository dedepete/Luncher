﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace Luncher.YaDra4il
{
    public class AuthManager
    {
        [JsonProperty("email")]
        public string Email;
        [JsonProperty("password")]
        public string Password;

        [JsonProperty("username")]
        public string Username;
        [JsonProperty("uuid")]
        public string Uuid;
        [JsonProperty("sessionToken")]
        public string SessionToken;
        [JsonProperty("accessToken")]
        public string AccessToken;

        [JsonProperty("demo")]
        public bool IsDemo;
        [JsonProperty("legacy")]
        public bool IsLegacy;

        public JArray UserProperties;

        public Authenticate Login()
        {
            var auth = Login(Email, Password);
            SessionToken = auth.accessToken;
            AccessToken = auth.clientToken;
            Username = auth.selectedProfile.name;
            Uuid = auth.selectedProfile.id;
            UserProperties = (JArray)auth.user["properties"];
            return auth;
        }

        private Authenticate Login(string email, string password)
        {
            var auth = new Authenticate(email, password);
            auth = (Authenticate)auth.DoPost();
            return auth;
        }

        public void Logout()
        {
            Logout(Email, Password);
        }

        private static void Logout(string email, string password)
        {
            var signout = new Signout(email, password);
            signout.DoPost();
        }

        public Refresh Refresh()
        {
            var refresh = new Refresh(SessionToken, AccessToken);
            refresh = (Refresh) refresh.DoPost();
            SessionToken = refresh.accessToken;
            AccessToken = refresh.clientToken;
            return refresh;
        }

        public bool CheckSessionToken()
        {
            var valid = CheckSessionToken(SessionToken);
            return valid;
        }

        private static bool CheckSessionToken(string sessionToken)
        {
            var check = new AuthentificationCheck(sessionToken);
            return ((AuthentificationCheck)check.DoPost()).valid;
        }

        public string GetUsernameByUUID()
        {
            Username = new Username
            {
                Uuid = Uuid
            }.GetUsernameByUuid();
            return Username;
        }

        public UserInfo GetUserInfo()
        {
            var inform = GetUserInfo(Username);
            Uuid = inform.id;
            return inform;
        }
        public static UserInfo GetUserInfo(string username)
        {
            var inform = new UserInfo(username);
            inform = (UserInfo)inform.DoPost();
            return inform;
        }
    }

    public class Authenticate : Request
    {
        public string accessToken;
        public string clientToken;
        public UserInfo selectedProfile;
        public JObject user;
        public Authenticate(string email, string password)
        {
            Url = AuthLinks.Authenticate;
            ToPost = new JObject
            {
                {
                    "agent", new JObject
                    {
                        {"name", "Minecraft"},
                        {"version", 1}
                    }
                },
                {"username", email},
                {"password", password},
                {"requestUser", true}
            }.ToString();
        }
    }

    public class Signout : Request
    {
        public Signout(string email, string password)
        {
            Url = AuthLinks.Signout;
            ToPost = new JObject
            {
                {"username", email},
                {"password", password},
            }.ToString();
        }
        public override Request Parse(string json)
        {
            return null;
        }
    }

    public class Refresh : Request
    {
        public string accessToken;
        public string clientToken;
        public Refresh(string accessToken, string clientToken)
        {
            Url = AuthLinks.Refresh;
            ToPost = new JObject
            {
                {"accessToken", accessToken},
                {"clientToken", clientToken},
            }.ToString();
        }
    }

    public class UserInfo : Request
    {
        public string id;
        public string name;

        public UserInfo(string username)
        {
            Url = "https://api.mojang.com/profiles/minecraft";
            ToPost = "[\"" + username + "\"]";
        }

        public override Request Parse(string json)
        {
            json = json.Trim('[', ']');
            return base.Parse(json);
        }
    }
    public class AuthentificationCheck : Request
    {
        public bool valid;
        public AuthentificationCheck(string session)
        {
            Url = AuthLinks.Validate;
            ToPost = "{\"accessToken\":\"" + session + "\"}";
        }
        public override Request DoPost()
        {
            try
            {
                base.DoPost();
                valid = true;
            }
            catch
            {
                valid = false;
            }
            return this;
        }
        public override Request Parse(string json)
        {
            return null;
        }
    }
    public abstract class Request
    {
        public string Url;
        public string ToPost;

        public virtual Request DoPost()
        {
            var body = Encoding.UTF8.GetBytes(ToPost);
            var request = (HttpWebRequest) WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = body.Length;
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(ToPost);
                streamWriter.Flush();
                streamWriter.Close();
            }
            return Parse(new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd());
        }

        public virtual Request Parse(string json)
        {
            return (Request)JsonConvert.DeserializeObject(json, GetType());
        }
    }
    public class Username
    {
        public string Uuid { private get; set; }
        public string GetUsernameByUuid()
        {
            var res =
                new WebClient().DownloadString("https://sessionserver.mojang.com/session/minecraft/profile/" + Uuid);
            var jo = JObject.Parse(res);
            return jo["name"].ToString();
        }
    }
    internal static class AuthLinks
    {
        private const string Authserver = @"https://authserver.mojang.com";

        public const string Authenticate = Authserver + @"/authenticate";
        public const string Refresh = Authserver + @"/refresh";
        public const string Validate = Authserver + @"/validate";
        public const string Signout = Authserver + @"/signout";
        public const string Invalidate = Authserver + @"/invalidate";
    }
}
