using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;
using System.Collections.Concurrent;

namespace Nimbus.Web.API.DataMining
{
    public class ChannelMining:NimbusApiController
    {
        public List<int> ChannelExists()
        {
            IEnumerable<int> idsChn;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    //pega todos os canais existentes dentro da org e retorna todos os ids
                    idsChn = db.SelectParam<Channel>(cn => cn.Visible == true && cn.OrganizationId == NimbusOrganization.Id).Select(cn => cn.Id);

                    //pega todos os canais que os usuarios seguem daquela organizaçao
                    IEnumerable<ChannelUser> users = db.SelectParam<ChannelUser>(chnu => chnu.Visible == true && chnu.Follow == true && idsChn.Contains(chnu.ChannelId));
                    ConcurrentDictionary<int, int> conjuntoItem = new ConcurrentDictionary<int,int>();

                    foreach (int idChn in idsChn)
                    {
                        foreach (ChannelUser item in users)
                        {
                            if (idChn == item.ChannelId)
                            {
                                //verifica se já existe na lista de resultados
                                conjuntoItem.AddOrUpdate(item.ChannelId, 1, (id, count) => ++count);
                            }
                        }
                    }  
     
                    //gerar restrição (precisa de 30% de likes p poder ser sugestão)
                    int support = Convert.ToInt32(idsChn.Count() * 0.3);
                 
                    foreach (var item in conjuntoItem)
                    {
                        if (item.Value < support)
                        {
                           int x;
                           conjuntoItem.TryRemove(item.Key, out x);
                        }
                    }
                    //nessa altura a conjuntoItem possui apenas os ids dos canais mais vistos até o momento (individualmente)
                    ConcurrentDictionary<int, int> elemX = new ConcurrentDictionary<int, int>();
                    ConcurrentDictionary<int, int> elemY = new ConcurrentDictionary<int, int>();

                }
            }
            catch (Exception ex)
            {                
                throw;
            }
            //arrumar
            return idsChn.ToList();
        }




    }
}