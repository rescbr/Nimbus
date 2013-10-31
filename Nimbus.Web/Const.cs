using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web
{
    public static class Const
    {
        public const string UserSession = "user";

        public static class Azure
        {
            public const string AvatarContainer = "avatarupload";
            
        }

        public static class Auth
        {
            public const string PerformAuthLater = "nimbus:PerformAuthLater";
            public const string RequestPerformance = "nimbus:RequestPerfomance";
        }
    }
}