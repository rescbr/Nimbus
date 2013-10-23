using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Nimbus.Plumbing;

namespace Nimbus.Web.Notifications
{
    public class NimbusHub : Hub
    {

        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, GetGroupName());
            return base.OnConnected();
        }

        /// <summary>
        /// Obtém a key do grupo SignalR do usuário logado.
        /// </summary>
        /// <returns>groupName</returns>
        public string GetGroupName()
        {
            if (Context.User.Identity.AuthenticationType == "NimbusUser")
            {
                var u = (Context.User.Identity as NimbusUser);
                return GetGroupName(u.UserId);
            }
            else throw new Exception("Wrong user authentication type. Shouldn't happen.");
        }

        /// <summary>
        /// Obtém um groupName SignalR para um usuário.
        /// </summary>
        /// <param name="userId">O UserId do usuário</param>
        /// <returns>groupName</returns>
        public static string GetGroupName(int userId)
        {
            return "user" + userId.ToString();
        }
    }

    
}