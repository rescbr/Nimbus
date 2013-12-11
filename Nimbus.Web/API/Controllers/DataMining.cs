using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using System.Collections.Concurrent;
using System.Web.Http;

namespace Nimbus.Web.API.Controllers
{
    [NimbusAuthorize]
    public class DataMiningController : NimbusApiController
    {
        [HttpGet]
        public List<Channel> RelatedChannels(int id, int qtd = 5)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var allDifferentChannelsFromFollowers = db.Where<ChannelUser>(chu => chu.ChannelId == id)
                        .Select(s => db.Where<ChannelUser>(t => t.UserId == s.UserId && t.ChannelId != s.ChannelId)
                        .Select(u => u.ChannelId));

                Dictionary<int, int> relatedCounter = new Dictionary<int, int>();
                foreach (var neighborChannels in allDifferentChannelsFromFollowers)
                {
                    foreach (var neighborChannel in neighborChannels)
                    {
                        if (!relatedCounter.ContainsKey(neighborChannel))
                            relatedCounter[neighborChannel] = 1;
                        else
                            relatedCounter[neighborChannel] += 1;
                    }
                }

                var listRelatedChannelIds = relatedCounter.OrderByDescending(ctr => ctr.Value).Take(qtd).Select(chid => chid.Key).ToArray();

                var relatedChannels = db.Where<Channel>(ch => listRelatedChannelIds.Contains(ch.Id));
                foreach (var item in relatedChannels)
                {
                    item.ImgUrl = item.ImgUrl.ToLower().Replace("capachannel", "category");
                }

                return relatedChannels.ToList();
            }
        }
    }
}