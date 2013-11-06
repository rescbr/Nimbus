using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class UserChannelReadLater : Nimbus.Model.UserChannelReadLater
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Channel))]
        public int ChannelId { get; set; }

    }
}
