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
                        List<ChannelTag> tagList = db.SelectParam<Nimbus.DB.ChannelTag>(tg => tg.ChannelID ==channelID  && tg.Visible == true).ToList();

                        showChannel.ChannelName = channel.Name;
                        showChannel.CountFollowers = channel.Followers.ToString();
                        showChannel.Organization_ID = channel.OrganizationId;
                        showChannel.owner_ID = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        showChannel.TagList = tagList;
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
            
        /// <summary>
        /// addou retira as permissões dos moderadores para o canal
        /// </summary>
        /// <param name="userModerator"></param>
        /// <returns></returns>
        [Authorize]
        public string managerModerator(List<Role> userModerator, int idChannel)
        {
            AlertGeneral alert = new AlertGeneral();
            string msg = alert.ErrorMessage;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    using(var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool  isOwner = db.SelectParam<Nimbus.DB.Role>(role => role.UserId == NimbusUser.UserId && role.IsOwner == true)
                                               .Select(s => s.IsOwner).FirstOrDefault();

                            bool isManager = db.SelectParam<Nimbus.DB.Role>(role => role.UserId == NimbusUser.UserId && role.ModeratorManager == true)
                                                 .Select(s => s.ModeratorManager).FirstOrDefault();
                            if (isOwner == true || isManager == true)
                            {
                                foreach (Role item in userModerator)
                                {
                                    db.Save(item);
                                }
                                msg = alert.SuccessMessage;
                            }
                            else
                            {
                                msg = alert.NotAllowed;
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();  
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return msg;
        }

        /// <summary>
        /// Add/retirar channel da lista de ler mais tarde 
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="readOn"></param>
        /// <returns></returns>
        [Authorize]
        public bool ReadChannelLater(int channelID, DateTime readOn)
        {
            bool operation = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //se ja existir = retirar//se não existir = criar
                    UserChannelReadLater user = db.SelectParam<Nimbus.DB.UserChannelReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.ChannelId == channelID).FirstOrDefault();
                    if (user != null)
                    {
                        user.Visible = false;
                        user.ReadOn = null;
                    }
                    else
                    {
                        user.Visible = true;
                        user.UserId = NimbusUser.UserId;
                        user.ReadOn = readOn;
                        user.Date = DateTime.Now;
                    }
                    db.Save(user);
                }
                operation = true;
            }
            catch (Exception ex)
            {
                operation = false;
                throw;
            }

            return operation;
        }
           
        /// <summary>
        /// Add tags para os canais
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="tagsList"></param>
        /// <returns></returns>
        [Authorize]
        public bool TagsChannel(int channelID, List<string> tagsList)
        {
            bool isOk = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool allow = db.SelectParam<Nimbus.DB.Role>(user => user.UserId == NimbusUser.UserId).Select(us => us.IsOwner).FirstOrDefault();
                            bool isPrivate = db.SelectParam<Nimbus.DB.Channel>(ch => ch.Id == channelID).Select(p => p.IsPrivate).FirstOrDefault();
                            bool allOk = false;

                            if (allow == true)//usuario possui permissao
                            {
                                //colocar restrição apra canal free
                                if (isPrivate == false)
                                {
                                    int countTag = db.SelectParam<Nimbus.DB.ChannelTag>(ch => ch.ChannelID == channelID).Count();
                                    if (countTag <= 4)
                                    {
                                        tagsList = tagsList.Take(5 - (countTag + 1)).ToList();
                                        allOk = true;
                                    }
                                    else
                                    {
                                        allOk = false;
                                    }
                                }
                                else
                                {
                                    allOk = true;
                                }
                                if (allOk == true)
                                {
                                    foreach (string item in tagsList)
                                    {
                                        ChannelTag tag = new ChannelTag
                                        {
                                            ChannelID = channelID,
                                            TagName = item,
                                            Visible = true 
                                        };
                                        db.Save(tag);
                                    }
                                    isOk = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            isOk = false;
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isOk = false;
                throw;
            }
            return isOk;
        }

        /// <summary>
        /// Troca a visibilidade (deleta) a tag escolhida
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="tagList"></param>
        /// <returns></returns>
        [Authorize]
        public bool DeleteTagChannel(int channelID, List<int> tagList)
        {
            bool isOk = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool allow = db.SelectParam<Nimbus.DB.Role>(user => user.UserId == NimbusUser.UserId).Select(us => us.IsOwner).FirstOrDefault();

                            if (allow == true)//usuario possui permissao
                            {

                                foreach (int item in tagList)
                                {
                                    var dado = new Nimbus.DB.ChannelTag() { Visible = false };
                                    db.Update<Nimbus.DB.ChannelTag>(dado, ch => ch.ChannelID == item);
                                    db.Save(dado);
                                }
                                isOk = true;
                            }
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            isOk = false;
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isOk = false;
                throw;
            }
            return isOk;
        }


        //criar canal

        //editar canal

      
        #endregion

    }



}
