using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Nimbus.Plumbing;

namespace Nimbus.Web.Notifications
{
    //[Authorize]
    public class NimbusHub : Hub
    {
        public void RegisterTopicCommentNotifications(int topicId)
        {
            Groups.Add(Context.ConnectionId, GetTopicGroupName(topicId));
        }

        public void RegisterMessageNotifications()
        {
            Groups.Add(Context.ConnectionId, GetMessageGroupName(UserId));
        }

        public override Task OnConnected()
        {
            Groups.Add(Context.ConnectionId, GetUserGroupName(UserId));
            return base.OnConnected();
        }

        /// <summary>
        /// Obtém a key do grupo SignalR do usuário logado.
        /// </summary>
        /// <returns>groupName</returns>
        public int UserId
        {
            get
            {
                if (Context.User.Identity.AuthenticationType == "NimbusUser")
                {
                    var u = (Context.User.Identity as NimbusUser);
                    return u.UserId;
                }
                else throw new Exception("Wrong user authentication type. Shouldn't happen.");
            }
        }

        /// <summary>
        /// Obtém um groupName SignalR para um usuário.
        /// </summary>
        /// <param name="userId">O UserId do usuário</param>
        /// <returns>groupName</returns>
        public static string GetUserGroupName(int userId)
        {
            return "user" + userId.ToString();
        }

        public static string GetTopicGroupName(int topicId)
        {
            return "topic" + topicId.ToString();
        }

        public static string GetMessageGroupName(int userId)
        {
            return "message" + userId.ToString();
        }

        public static string GetFollowerGroupName(int userId)
        {
            return "message" + userId.ToString();
        }
    }

    
}