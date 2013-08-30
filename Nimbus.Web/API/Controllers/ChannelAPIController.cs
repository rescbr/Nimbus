using Nimbus.Web.API.Models.Channel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;
using Nimbus.DB;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Canais.
    /// </summary>
    public class ChannelAPIController : NimbusApiController
    {
        #region métodos de visualização
        /// <summary>
        /// carregar informações gerais do canal
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public showChannelAPIModel showChannel(int channelID)
        {
            showChannelAPIModel showChannel = new showChannelAPIModel();
            try
            {

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    
                    bool ismember = (db.SelectParam<Nimbus.DB.ChannelUser>(usr => usr.UserId == NimbusUser.UserId &&
                                                                                  usr.ChannelId == channelID &&
                                                                                  usr.Follow == true).Count()) > 0 ? true : false;

                    if (ismember == true)
                    {
                        Nimbus.DB.Channel channel = db.SelectParam<Nimbus.DB.Channel>(chn => chn.Id == channelID && chn.Visible == true).FirstOrDefault();
                        showChannel.ChannelName = channel.Name;
                        showChannel.CountFollowers = channel.Followers.ToString();
                        showChannel.Organization_ID = channel.OrganizationId;
                        showChannel.owner_ID = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        //showChannel.ParticipationChannel = 
                        showChannel.RankingChannel = channel.Ranking.ToString();
                        showChannel.UrlImgChannel = channel.ImgUrl;
                    }
                    else
                    {
                        AlertChannelPay alert = new AlertChannelPay ();
                        Nimbus.DB.Channel channel2 = db.Query<Nimbus.DB.Channel>("SELECT Channel.Price, Channel.IsPrivate" +
                                                                                 "FROM Channel"+
                                                                                 "WHERE Channel.Id ={0} AND Channel.Visible = true", channelID).FirstOrDefault();
                        if (channel2.Price > 0)
                        {
                            showChannel.MessageAlert = alert.AlertPay ;
                        }
                        else if (channel2.IsPrivate == true)
                        {
                            showChannel.MessageAlert = alert.AlertPrivate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return showChannel;
        }
               
        /// <summary>
        /// visualizar 'meus canais'
        /// </summary>
        /// <returns></returns>        
        [Authorize]
        public List<AbstractChannelAPI> myChannel()
        {
            List<AbstractChannelAPI> listChannel = new List<AbstractChannelAPI>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> listID = db.Select<int>("SELECT ChannelUser.ChannelId FROM ChannelUser" +
                                                      "INNER JOIN Role ON ChannelUser.UserId = Role.UserId "+
                                                      "WHERE ChannelUser.UserId = {0} AND Role.IsOwner = true AND ChannelUser.Follow = true",
                                                       NimbusUser.UserId);
                    foreach (int item in listID)
                    {
                        Nimbus.DB.Channel chn = db.Select<Nimbus.DB.Channel>("SELECT Channel.Organization, Channel.Id, Channel.Name, Channel.ImgUrl "+
                                                                                 "FROM Channel WHERE Channel.Id = {0} AND Channel.Visible = true", item).FirstOrDefault();
                        AbstractChannelAPI channel = new AbstractChannelAPI();
                        channel.channel_ID = chn.Id;
                        channel.ChannelName = chn.Name;
                        channel.Organization_ID = chn.OrganizationId;
                        channel.UrlImgChannel = chn.ImgUrl;
                        listChannel.Add(channel);
                    }                       
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return listChannel;
        }

       /// <summary>
       ///visualizar canais moderados
       /// </summary>
       /// <returns></returns>
        public List<AbstractChannelAPI> moderatorChannel()
        {
            List<AbstractChannelAPI> listChannel = new List<AbstractChannelAPI>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> listID = db.Select<int>("SELECT ChannelUser.ChannelId FROM ChannelUser" +
                                                      "INNER JOIN Role ON ChannelUser.UserId = Role.UserId " +
                                                      "WHERE ChannelUser.UserId = {0} AND Role.IsOwner = false AND ChannelUser.Follow = true ",
                                                       NimbusUser.UserId);
                    foreach (int item in listID)
                    {
                        Nimbus.DB.Channel chn = db.Select<Nimbus.DB.Channel>("SELECT Channel.Organization, Channel.Id, Channel.Name, Channel.ImgUrl " +
                                                                                 "FROM Channel WHERE Channel.Id = {0} AND Channel.Visible = true",item).FirstOrDefault();
                        if (chn != null)
                        {
                            AbstractChannelAPI channel = new AbstractChannelAPI();
                            channel.channel_ID = chn.Id;
                            channel.ChannelName = chn.Name;
                            channel.Organization_ID = chn.OrganizationId;
                            channel.UrlImgChannel = chn.ImgUrl;
                            listChannel.Add(channel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listChannel;
        }

        /// <summary>
        /// retorna uma lista com os resumos de todos os canais disponíveis no nimbus 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public List<AbstractChannelAPI> abstractAllChannel()
        {
            List<AbstractChannelAPI> listChannel = new List<AbstractChannelAPI>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {

                    List<Nimbus.DB.Channel> chnList = db.Select<Nimbus.DB.Channel>("SELECT Channel.OrganizationId, Channel.Id, Channel.Name, Channel.ImgUrl " +
                                                                             "FROM Channel WHERE Channel.Visible = true");
                    foreach (var item in chnList)
                    {
                        AbstractChannelAPI absChannel = new AbstractChannelAPI
                        {
                            channel_ID = item.Id,
                            ChannelName = item.Name,
                            Organization_ID = item.OrganizationId,
                            UrlImgChannel = item.ImgUrl
                        };
                        listChannel.Add(absChannel);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return listChannel;
        }

        #endregion


        #region métodos de interação com o canal
               
        /// <summary>
        /// Caso o usuário deseje deletar o channel, ele perde a posse e o channel passa a ser do 'nimbus'
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        public string DeleteChannel(int channelID)
        {
            AlertGeneral alert = new AlertGeneral();
            string message = alert.ErrorMessage;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    int idOwner = db.Select<int>("SELECT Channel.OwnerId FROM Channel "+
                                                                             "INNER JOIN Role ON Channel.Id = Role.ChannelId "+
                                                                             "WHERE Channel.Id = {0} AND Channel.UserId = {1} AND Channel.Visible = true AND Role.IsOwner = true",
                                                                              channelID, NimbusUser.UserId).FirstOrDefault();
                    if (idOwner != null && idOwner > 0)
                    {
                        db.UpdateOnly(new Channel { OwnerId = 0 }, chn => chn.OwnerId, chn => chn.Id == channelID);
                        message = alert.SuccessMessage;
                    }
                    
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return message;
 
        }
                
        /// <summary>
        /// seguir/ñ seguir canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        public bool followChannel(int channelID)
        {
            bool follow = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    Nimbus.DB.ChannelUser user = db.SelectParam<Nimbus.DB.ChannelUser>(chn => chn.UserId == NimbusUser.UserId &&
                                                                                              chn.ChannelId == channelID).FirstOrDefault();
                    if (user == null)//novato
                    {
                        db.Insert<Nimbus.DB.ChannelUser>(new ChannelUser { ChannelId = channelID, Interaction = 0, Follow=true, 
                                                                           Vote = false, UserId = NimbusUser.UserId });
                        follow = true;
                    }
                    else //já existia
                    {
                        if (user.Follow == false)
                        {
                            db.UpdateOnly(new ChannelUser { Follow = true }, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId);
                            follow = true;
                        }
                        else if (user.Follow == true)
                        {
                            db.UpdateOnly(new ChannelUser { Follow = false }, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId);
                            follow = false;
                        }                                                                                                                                                                                                                                                                                                                                                                                                                                                            
                    }
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return follow;
        }


        //apagar permissoes moderadores
        //add moderadores para o canal


        //criar canal
        //editar canal

        //enviar mensagem para o canal/dono)
        //editar/add tags do canal
        //ver mais tarde o canal ou retirar da lista de ver mais tarde
        #endregion

    }



}
