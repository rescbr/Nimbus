using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ChannelTag
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Channel))]
        public int ChannelID { get; set; }

        public string TagName { get; set; }
    }
}
