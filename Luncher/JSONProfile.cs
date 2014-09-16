#region Profile Deserialization and Serialization

namespace JsonProfile
{
    public class Profile
    {
        public string[] allowedReleaseTypes;
        public ProfileResolution resolution;
        public ProfileServer server;
        public string name;
        public string gameDir;
        public string javaDir;
        public string javaArgs;
        public string lastVersionId;
        public string launcherVisibilityOnGameClose;
    }

    public class ProfileResolution
    {
        public string height;
        public string width;
    }

    public class ProfileServer
    {
        public string ip;
        public string port;
    }
}
#endregion
