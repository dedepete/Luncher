namespace Luncher
{
    static class AuthShemes
    {
        private const string Authserver = @"https://authserver.mojang.com";

        public const string Authenticate = Authserver + @"/authenticate";
        public const string Refresh = Authserver + @"/refresh";
        public const string Validate = Authserver + @"/validate";
        public const string Signout = Authserver + @"/signout";
        public const string Invalidate = Authserver + @"/invalidate";

        public const string AuthenticateSheme = @"{
  'agent': {
    'name': 'Minecraft',           
    'version': 1
  },
  'username': '${username}',
  'password': '${password}',
}";
        public const string SignoutSheme = @"{
  'username': '${username}',
  'password': '${password}',
}";
    }
}
