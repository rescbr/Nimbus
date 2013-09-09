﻿using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserTopicFavorite
    {      
        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Topic))]
        public int TopicId { get; set; }

        public DateTime FavoritedOn { get; set; }
        public bool Visible { get; set; }
    }
}
