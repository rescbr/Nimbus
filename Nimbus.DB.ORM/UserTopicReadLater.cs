using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class UserTopicReadLater : Nimbus.DB.UserTopicReadLater
    {

        [AutoIncrement]
        public override int Id { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }

        [References(typeof(Topic))]
        public override int TopicId { get; set; }

    }
}
