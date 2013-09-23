using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Topic : Nimbus.DB.Topic
    {
        [AutoIncrement]
        public override int Id { get; set; }

        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

        [References(typeof(User))]
        public override int AuthorId { get; set; }
    }
}
