using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserTopicReadLater
    {

        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Topic))]
        public int TopicId { get; set; }

        public DateTime Date { get; set; }
        public DateTime ReadOn { get; set; }
    }
}
