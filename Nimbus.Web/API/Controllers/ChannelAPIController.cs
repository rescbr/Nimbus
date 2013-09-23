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
using Nimbus.Web.API.Models.User;

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
                    AlertChannelPay alert = new AlertChannelPay();
                    bool allow = false;
                    Nimbus.DB.Channel channel2 = db.Query<Nimbus.DB.Channel>("SELECT Channel.Price, Channel.IsPrivate" +
                                                                             "FROM Channel" +
                                                                             "WHERE Channel.Id ={0} AND Channel.Visible = true", channelID).FirstOrDefault();
                    if (channel2.Price > 0)
                    {
                        bool paid = db.SelectParam<Nimbus.DB.Role>(ch => ch.ChannelId == channelID && ch.UserId == NimbusUser.UserId)
                                                                        .Select(ch => ch.Paid).FirstOrDefault();
                        if (paid == true)
                        { 
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                            showChannel.MessageAlert = alert.AlertPay;
                        }
                    }
                    else if (channel2.IsPrivate == true)
                    {
                        bool? pending = db.SelectParam<Nimbus.DB.ChannelUser>(ch => ch.ChannelId == channelID && ch.UserId == NimbusUser.UserId)
                                                                                  .Select(ch => ch.Pending).FirstOrDefault();
                        if (pending == false && pending != null)
                        {
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                            showChannel.MessageAlert = alert.AlertPrivate;
                        }
                    }
                    if (allow == true)
                    {
                        Nimbus.DB.Channel channel = db.SelectParam<Nimbus.DB.Channel>(chn => chn.Id == channelID && chn.Visible == true).FirstOrDefault();
                        List<ChannelTag> tagList = db.SelectParam<Nimbus.DB.ChannelTag>(tg => tg.ChannelID == channelID && tg.Visible == true).ToList();
                        
                        List<InteractionAPI> listComments = db.Select<InteractionAPI>("SELECT Comment.UserId FROM Comment" +
                                                         "INNER JOIN Topic ON Comment.TopicId = Topic.Id" +
                                                         "WHERE Topic.ChannelId = {0} AND Comment.Visible = true AND Topic.Visible = true",
                                                         channelID);

                        int userComment = listComments.Select(c => c.UserID == NimbusUser.UserId).Count();
                        int allComment = listComments.Count();

                        showChannel.ChannelName = channel.Name;
                        showChannel.CountFollowers = channel.Followers.ToString();
                        showChannel.Organization_ID = channel.OrganizationId;
                        showChannel.owner_ID = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        showChannel.TagList = tagList;
                        showChannel.ParticipationChannel = ((userComment * 100) / allComment).ToString();
                        showChannel.RankingChannel = channel.Ranking.ToString();
                        showChannel.UrlImgChannel = channel.ImgUrl;
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
        public List<AbstractChannelAPI> abstractAllChannel(int categoryID)
        {
            List<AbstractChannelAPI> listChannel = new List<AbstractChannelAPI>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<Nimbus.DB.Channel> chnList = new List<Channel>(); 
                    if (categoryID > 0)
                    {
                        chnList = db.Select<Nimbus.DB.Channel>("SELECT Channel.OrganizationId, Channel.Id, Channel.Name, Channel.ImgUrl " +
                                                                                 "FROM Channel WHERE Channel.Visible = true AND Channel.CategoryId = {0}", categoryID);
                    }
                    else
                    {
                        chnList = db.Select<Nimbus.DB.Channel>("SELECT Channel.OrganizationId, Channel.Id, Channel.Name, Channel.ImgUrl " +
                                                                                 "FROM Channel WHERE Channel.Visible = true");
                    }
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
        {//TODO : notificação
            bool follow = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Nimbus.DB.Channel channel2 = db.Query<Nimbus.DB.Channel>("SELECT Channel.Price, Channel.IsPrivate" +
                                                                                "FROM Channel" +
                                                                                "WHERE Channel.Id ={0} AND Channel.Visible = true", channelID).FirstOrDefault();
                    bool pending = true;
                    if (channel2.IsPrivate == false)
                    {
                        pending = false;
                    }
                        Nimbus.DB.ChannelUser user = db.SelectParam<Nimbus.DB.ChannelUser>(chn => chn.UserId == NimbusUser.UserId &&
                                                                                                  chn.ChannelId == channelID).FirstOrDefault();
                        if (user == null)//novato
                        {
                            db.Insert<Nimbus.DB.ChannelUser>(new ChannelUser
                            {
                                ChannelId = channelID,
                                Interaction = 0,
                                Follow = true,
                                Vote = false,
                                UserId = NimbusUser.UserId,
                                Pending = pending
                            });
                            follow = true;
                        }
                        else //já existia
                        {
                            if (user.Follow == false)
                            {
                                db.UpdateOnly(new ChannelUser { Follow = true, Pending = true }, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId);
                                follow = true;
                            }
                            else if (user.Follow == true)
                            {
                                db.UpdateOnly(new ChannelUser { Follow = false, Pending = null }, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId);
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
        /// add ou retira as permissões dos moderadores para o canal
        /// </summary>
        /// <param name="userModerator"></param>
        /// <returns></returns>
        [Authorize]
        public string addModerator(List<Role> userModerator, int idChannel)
        {//TODO: notificação
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
                            bool prox = false;
                            bool allow = db.SelectParam<Nimbus.DB.Role>(role => role.UserId == NimbusUser.UserId && role.ChannelId == idChannel)
                                                                                .Exists (role => role.IsOwner == true || role.ModeratorManager == true);


                            int countModerator = db.SelectParam<Nimbus.DB.Role>(mdr => mdr.ChannelId == idChannel &&
                                                                                      (mdr.ModeratorManager == true || mdr.MessageManager == true
                                                                                       || mdr.ChannelMagager == true || mdr.TopicManager == true 
                                                                                       || mdr.UserManager == true)).Count();

                            //organizationId == 0 => nimbus, portanto free
                            int orgID = db.SelectParam<Nimbus.DB.Channel>(ch => ch.Id == idChannel).Select(ch => ch.OrganizationId).FirstOrDefault();

                            if (allow == true)
                            {
                                if (orgID == 0) //canais free, portanto permite apenas 5 moderadores
                                {
                                    if (countModerator < 5)
                                        prox = true;
                                    else
                                        prox = false;
                                }
                                else if (orgID != 0)
                                {
                                    prox = true;
                                }
                            }
                            else
                            {
                                prox = false;
                            }
                            

                            if (prox == true)
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
                        user.ChannelId = channelID;
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

        /// <summary>
        /// Criar um canal
        /// </summary>
        /// <param name="newChannel"></param>
        /// <returns></returns>
        [Authorize]
        public bool newChannel(NewChannelAPI newChannel)
        {//TODO:Notificação
            bool created = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = false; //zero = padrao Nimbus
                    if (newChannel.Organization_ID != 0)
                    {
                        int idUser = db.SelectParam<Nimbus.DB.OrganizationUser>(us => us.UserId == NimbusUser.UserId
                                                                                   && us.OrganizationId == newChannel.Organization_ID).Select(us => us.UserId).FirstOrDefault();

                        bool isManager = db.SelectParam<Nimbus.DB.Role>(us => us.UserId == idUser)
                                                                        .Exists(us => us.IsOwner == true || us.ChannelMagager == true);
                        if (isManager == true)
                        {
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                        }
                    }
                    else
                    {
                        allow = true;
                    }

                    if (allow == true)
                    {
                        using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                Nimbus.DB.Channel channel = new Nimbus.DB.Channel()
                                {
                                    CategoryId = newChannel.Category_ID,
                                    CreatedOn = DateTime.Now,
                                    LastModification = DateTime.Now,
                                    Description = newChannel.Description,
                                    Followers = 0,
                                    ImgUrl = newChannel.ImgUrl,
                                    IsCourse = newChannel.IsCourse,
                                    IsPrivate = newChannel.IsPrivate,
                                    Name = newChannel.Name,
                                    OpenToComments = newChannel.OpenToComments,
                                    OrganizationId = newChannel.Organization_ID,
                                    OwnerId = NimbusUser.UserId,
                                    Price = newChannel.IsPrivate == true ? newChannel.Price : 0,
                                    Ranking = 0,
                                    Visible = true
                                };
                                db.Insert(channel);
                                int channelID = (int)db.GetLastInsertId();

                                Nimbus.DB.VoteChannel vote = new VoteChannel
                                {
                                    Channel_ID = channelID,
                                    Score = 0
                                };
                                db.Insert(vote);
                                trans.Commit();
                                created = true;
                            }
                            catch (Exception ex)
                            {
                                created = false;
                                trans.Rollback();
                                throw ex;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                created = false;
                throw ex;
            }
            return created;
        }
        
        /// <summary>
        /// Méetodo para editar as informações de um canal
        /// </summary>
        /// <param name="editChannel"></param>
        /// <returns></returns>
        [Authorize]
        public bool editChannel(EditChannelAPI editChannel)
        {
            bool edit = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool isOwner = db.SelectParam<Nimbus.DB.Role>(own => own.UserId == NimbusUser.UserId && own.ChannelId == editChannel.Channel_ID)
                                                                        .Select(own => own.IsOwner).FirstOrDefault();
                    bool isManager = db.SelectParam<Nimbus.DB.Role>(mg => mg.UserId == NimbusUser.UserId && mg.ChannelId == editChannel.Channel_ID)
                                                                        .Select(mg => mg.ChannelMagager).FirstOrDefault();
                    if (isOwner == true || isManager == true)
                    {
                        Channel channel = db.SelectParam<Nimbus.DB.Channel>(chn => chn.Id == editChannel.Channel_ID && chn.Visible == true).FirstOrDefault();
                        channel.Name = editChannel.Name;
                        channel.CategoryId = editChannel.Category_ID;
                        channel.Description = editChannel.Description;
                        channel.ImgUrl = editChannel.ImgUrl;
                        channel.IsCourse = editChannel.IsCourse;
                        channel.IsPrivate = editChannel.IsPrivate;
                        channel.LastModification = DateTime.Now;
                        channel.OpenToComments = editChannel.OpenToComments;
                        channel.OrganizationId = editChannel.Organization_ID;
                        channel.Price = editChannel.Price;
                        channel.Visible = editChannel.Visible;

                        db.Update(channel);
                        edit = true;
                        //TODO: Notificação
                    }
                    else 
                    {
                        edit = false;
                    }
                }
            }
            catch (Exception ex)
            {
                edit = false;
                throw ex;
            }

            return edit;
        }

        /// <summary>
        /// Método de adicionar/atualizar os scores de um canal
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        [Authorize]
        public int voteChannel(VoteChannelAPI vote)
        {
            int score = -1;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            var voted = db.SelectParam<Nimbus.DB.ChannelUser>(vt => vt.UserId == NimbusUser.UserId && vt.ChannelId == vote.Channel_ID && vt.Pending == false)
                                                                                     .Select(vt => vt.Vote).FirstOrDefault();
                            if (voted == null)
                            {
                                VoteChannel vtChannel = new VoteChannel
                                {
                                    Score = vote.Score
                                };
                                int nota = db.Query<int>("UPDATE VoteChannel SET VoteChannel.Score = VoteChannel.Score + @score OUTPUT INSERTED.Score " +
                                                                "WHERE VoteChannel.Channel_ID = @channelID",
                                                                new { score = vote.Score, channelID = vote.Channel_ID }).FirstOrDefault();

                                ChannelUser chnUser = new ChannelUser { Vote = true, Score = vote.Score };
                                db.Update<Nimbus.DB.ChannelUser>(chnUser, chn => chn.UserId == NimbusUser.UserId && chn.ChannelId == vote.Channel_ID);

                                score = nota;
                                trans.Commit();
                            }
                            else if (voted == true)
                            {
                                VoteChannel vtChannel = new VoteChannel
                                {
                                    Score = vote.Score
                                };

                                int notaVelha = db.SelectParam<Nimbus.DB.ChannelUser>(vt => vt.ChannelId == vote.Channel_ID && vt.UserId == NimbusUser.UserId)
                                                                                           .Select(vt => vt.Score).FirstOrDefault();

                                int nota = db.Query<int>("UPDATE VoteChannel SET VoteChannel.Score = VoteChannel.Score + @score OUTPUT INSERTED.Score " +
                                                                "INNER JOIN ChannelUser ON VoteChannel.Channel_ID = ChannelUser.ChannelId " +
                                                                "WHERE VoteChannel.Channel_ID = @channelID AND ChannelUser.UserId = @user ",
                                                                new { score = (vote.Score - notaVelha), channelID = vote.Channel_ID, user = NimbusUser.UserId }).FirstOrDefault();

                                ChannelUser chnUser = new ChannelUser { Score = vote.Score };
                                db.Update<Nimbus.DB.ChannelUser>(chnUser, chn => chn.UserId == NimbusUser.UserId && chn.ChannelId == vote.Channel_ID);

                                trans.Commit();
                            }
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

            return score;
        }

        /// <summary>
        /// Lista os usuários que desejam entrar para o canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        public List<abstractProfile> ListPendingUSer(int channelID)
        {
            List<abstractProfile> list = new List<abstractProfile>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    int allow = db.SelectParam<Nimbus.DB.Role>(role => role.UserId == NimbusUser.UserId && 
                                                                       (role.IsOwner == true || role.ChannelMagager == true || role.UserManager == true)).Count();
                    if (allow > 0)
                    {
                        List<int> idUsers = db.SelectParam<Nimbus.DB.ChannelUser>(ch => ch.ChannelId == channelID && ch.Pending == true).Select(ch => ch.UserId).ToList();
                        foreach (int item in idUsers)
                        {
                            User user = db.Select<Nimbus.DB.User>("SELECT User.FirstName, User.LastName, User.AvatarUrl " +
                                                                             "FROM User WHERE User.Id = {0}", item).FirstOrDefault();
                            if(user != null)
                            {
                                abstractProfile profile = new abstractProfile
                                {
                                    AvatarUrl = user.AvatarUrl,
                                    idUser = item,
                                    Name = user.FirstName + " " + user.LastName
                                };
                                list.Add(profile);
                            }
                        }
                    }
                    else
                    {
                        list = null;
                    }
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }

            return list;
        }

        /// <summary>
        /// Método para aceitar/recusar um usuário que se inscreveu para o canal
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [Authorize]
        public bool AcceptUser(AcceptUserAPI dados)
        {
            bool accept = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = db.SelectParam<Nimbus.DB.Role>(ch => ch.ChannelId == dados.ChannelID && ch.UserId == NimbusUser.UserId)
                                                                        .Exists(ch => ch.IsOwner == true || ch.UserManager == true ||
                                                                                ch.ChannelMagager == true);
                    if (allow == true)
                    {
                        if (dados.isAccept == false)//recusou = sai da lista 
                            dados.isAccept = null;

                        ChannelUser channel = new ChannelUser
                        {
                            Pending = dados.isAccept
                        };
                        db.Update<Nimbus.DB.ChannelUser>(channel, ch => ch.UserId == dados.IdUser && ch.ChannelId == dados.ChannelID);

                        accept = true;
                    }
                    else
                    {
                        accept = false;
                    }
                }                
            }
            catch (Exception ex)
            {
                accept = false;
                throw ex;
            }

            return accept;
        }

        #endregion

    }



}
