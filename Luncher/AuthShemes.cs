using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luncher
{
    class AuthShemes
    {
        public static string authserver = @"https://authserver.mojang.com";

        public static string authenticate = @"/authenticate";
        public static string refresh = @"/refresh";
        public static string validate = @"/validate";
        public static string signout = @"/signout";
        public static string invalidate = @"/invalidate";

        public static string authenticatesheme = @"{
  'agent': {
    'name': 'Minecraft',           
    'version': 1
  },
  'username': '${username}',
  'password': '${password}',
}";
        public static string signoutsheme = @"{
  'username': '${username}',
  'password': '${password}',
}";
    }
}
