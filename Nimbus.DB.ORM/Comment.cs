using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Comment : Nimbus.DB.Comment
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Comment))]
        public int? ParentId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Channel))]
        public int ChannelId { get; set; }

        [References(typeof(Topic))]
        public int TopicId { get; set; }
        
    }
}
