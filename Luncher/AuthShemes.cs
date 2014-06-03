namespace Luncher
{
    class AuthShemes
    {
        public const string Authserver = @"https://authserver.mojang.com";

        public const string Authenticate = @"/authenticate";
        public const string Refresh = @"/refresh";
        public const string Validate = @"/validate";
        public const string Signout = @"/signout";
        public const string Invalidate = @"/invalidate";

        public const string Authenticatesheme = @"{
  'agent': {
    'name': 'Minecraft',           
    'version': 1
  },
  'username': '${username}',
  'password': '${password}',
}";
        public const string Signoutsheme = @"{
  'username': '${username}',
  'password': '${password}',
}";
    }
}
