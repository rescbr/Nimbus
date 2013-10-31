using Nimbus.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Security
{
    public class SessionManagement
    {
        public NimbusPrincipal GetPrincipalFromSession(Guid sessionToken)
        {
            throw new NotImplementedException("Use GetNimbusPrincipal");
        }

        public void InvalidateSession(Guid sessionToken)
        {
            throw new NotImplementedException("Favor me implementar =)");
        }
        public NimbusPrincipal GetNimbusPrincipal(int userId)
        {
            var dblogin = new DatabaseLogin();
            return dblogin.GetNimbusPrincipal(userId);
        }
    }
}