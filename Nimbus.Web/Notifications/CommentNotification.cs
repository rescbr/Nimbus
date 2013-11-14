using Nimbus.Model.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Notifications
{
    public class CommentNotification : NimbusNotificationBase
    {
        
        public void NewComment(Comment comment)
        {
            string topicGroup = NimbusHub.GetTopicGroupName(comment.TopicId);
            NimbusHubContext.Clients.Group(topicGroup)
                .newTopicCommentNotification(new
                {
                    commentId = comment.Id,
                    parentId = comment.ParentId,
                    topicId = comment.TopicId
                });
        }
    }
}