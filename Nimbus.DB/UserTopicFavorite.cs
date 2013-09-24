using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserTopicFavorite
    {
        public virtual int UserId { get; set; }

        public virtual int TopicId { get; set; }


        public DateTime FavoritedOn { get; set; }
        public bool Visible { get; set; }
    }
}
