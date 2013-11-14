using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using System.Collections.Concurrent;

namespace Nimbus.Web.API.DataMining
{
    public class ChannelMining : NimbusApiController
    {
        void RelatedChannels(int channelId)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var allDifferentChannelsFromFollowers = db.Where<ChannelUser>(chu => chu.ChannelId == channelId)
                        .Select(s => db.Where<ChannelUser>(t => t.UserId == s.UserId && t.ChannelId != s.ChannelId)
                        .Select(u => u.ChannelId));

                Dictionary<int, int> relatedCounter = new Dictionary<int, int>();
                foreach (var neighborChannels in allDifferentChannelsFromFollowers)
                {
                    foreach (var neighborChannel in neighborChannels)
                    {
                        relatedCounter[neighborChannel] += 1;
                    }
                }


            }
        }

    }
}