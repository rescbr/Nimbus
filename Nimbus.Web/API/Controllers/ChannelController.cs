﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;
using Nimbus.DB.ORM;
using Nimbus.DB.Bags;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Canais.
    /// </summary>
    public class ChannelController : NimbusApiController
    {
        /// <summary>
        /// Verifica se o usuário pagou para ser membro do canal (quando este possui preço maior 0 )
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [NonAction]
        public bool Paid(int id)
        {
            bool flag = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    flag = db.SelectParam<Role>(ch => ch.ChannelId == id && ch.UserId == NimbusUser.UserId).Select(ch => ch.Paid).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {                
                throw;
            }
            return flag;
        }

        /// <summary>
        /// Verifica se o usuário foi aceito no canal (quando este é privado)
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [NonAction]
        public bool IsAccepted (int id)
        {
            bool flag = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    flag = db.SelectParam<ChannelUser>(ch => ch.ChannelId == id && ch.UserId == NimbusUser.UserId).Select(ch => ch.Accepted).FirstOrDefault();   
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return flag;
        }

        /// <summary>
        /// Verifica se o usuário é dono do canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [NonAction]
        public bool IsOwner(int id)
        {
            bool allow = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    allow = db.SelectParam<Role>(own => own.UserId == NimbusUser.UserId && own.ChannelId == id)
                                                                        .Select(own => own.IsOwner).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                
                throw;
            }
            return allow;
        }

        /// <summary>
        /// Verifica se o usuário é adm do canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [NonAction]
        public bool IsManager(int id)
        {
            bool allow = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    allow = db.SelectParam<Role>(mg => mg.UserId == NimbusUser.UserId && mg.ChannelId == id)
                                                                         .Select(mg => mg.ChannelMagager).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return allow;
        }

        /// <summary>
        /// Método de retornar todas as tags relacionadas ao canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<TagChannel> ShowTagChannel(int id)
        {
            List<TagChannel> tagList = new List<TagChannel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    tagList = db.SelectParam<TagChannel>(tg => tg.ChannelId == id && tg.Visible == true).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return tagList;
        }

        #region métodos de visualização
        /// <summary>
        /// carregar informações gerais do canal
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public ChannelBag ShowChannel(int id)
        {
            ChannelBag showChannel = new ChannelBag();
            try
            {

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    AlertChannelPay alert = new AlertChannelPay();
                    bool allow = false;

                    Channel channel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.Id == id).FirstOrDefault();

                    #region verifica permissão
                    if (channel.Price > 0 || channel.IsPrivate == true)
                    {
                        bool paid = Paid(id);
                        bool accepted = IsAccepted(id);

                        if (paid == true || accepted == true ) 
                        { 
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                            if (paid == false)
                            {
                                showChannel.messageAlert = alert.AlertPay;
                            }
                            else if( accepted == false)
                            {
                                showChannel.messageAlert = alert.AlertPrivate;               
                            }
                        }
                    }
                    #endregion

                    if (allow == true)
                    {
                        int topidID = db.SelectParam<Topic>(tp => tp.ChannelId == id && tp.Visibility == true).Select(tp => tp.Id).FirstOrDefault();                    
                        List<Comment> listComments = db.SelectParam<Comment>(cm => cm.TopicId == topidID &&  cm.Visible == true);

                        int userComment = listComments.Where(us => us.UserId == NimbusUser.UserId).Count();

                        showChannel.Name = channel.Name;
                        showChannel.countFollowers = channel.Followers.ToString();
                        showChannel.OrganizationId = channel.OrganizationId;
                        showChannel.OwnerId = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        showChannel.participationChannel = ((userComment * 100) / listComments.Count()).ToString();
                        showChannel.ImgUrl = channel.ImgUrl;
                    }
                }
            }

            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return showChannel;
        }
               
        /// <summary>
        /// visualizar 'meus canais'
        /// </summary>
        /// <returns></returns>        
        [Authorize]
        [HttpGet]
        public List<Channel> MyChannel()
        {
            List<Channel> listChannel = new List<Channel>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                   List<int> idsChannel = db.SelectParam<Role>(rl => rl.IsOwner == true && rl.UserId == NimbusUser.UserId).Select(rl => rl.ChannelId).ToList();
                    
                    foreach (int item in idsChannel)
                    {
                        Channel channel = (from chn in db.SelectParam<Channel>(chn => chn.Visible == true && chn.Id == item) 
                                           select new Channel()
                                            {
                                                Id = chn.Id,
                                                Name = chn.Name,
                                                OrganizationId = chn.OrganizationId,
                                                ImgUrl = chn.ImgUrl
                                            }).FirstOrDefault();
                        listChannel.Add(channel);
                    }                       
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listChannel;
        }

       /// <summary>
       ///visualizar canais moderados
       /// </summary>
       /// <returns></returns>
        public List<Channel> ModeratorChannel()
        {
            List<Channel> listChannel = new List<Channel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> idsChannel = db.SelectParam<Role>(rl => rl.IsOwner == false && rl.ModeratorManager == true && rl.UserId == NimbusUser.UserId)
                                                                        .Select(rl => rl.ChannelId).ToList();

                    foreach (int item in idsChannel)
                    {
                        Channel channel = (from chn in db.SelectParam<Channel>(chn => chn.Visible == true && chn.Id == item)
                                       select new Channel()
                                       {
                                           OrganizationId = chn.OrganizationId,
                                           Id = chn.Id,
                                           Name = chn.Name,
                                           ImgUrl = chn.ImgUrl
                                       }).FirstOrDefault();

                        listChannel.Add(channel);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listChannel;
        }

        /// <summary>
        /// retorna uma lista com os resumos de todos os canais disponíveis no nimbus 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<Channel> AbstractAllChannel(int id)
        {
            List<Channel> listChannel = new List<Channel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (id > 0)
                    {
                        listChannel = db.SelectParam<Channel>(ch => ch.CategoryId == id && ch.Visible == true);

                    }
                    else
                    {
                        listChannel = db.SelectParam<Channel>(ch => ch.Visible == true);
                    }
                  
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
        [HttpDelete]
        public string DeleteChannel(int id)
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
                                                  id, NimbusUser.UserId).FirstOrDefault();

                    if (idOwner != null && idOwner > 0)
                    {
                        db.UpdateOnly(new Channel { OwnerId = 0 }, chn => chn.OwnerId, chn => chn.Id == id);
                        message = alert.SuccessMessage;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return message;
 
        }
                 
        /// <summary>
        /// seguir/ñ seguir canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        public bool FollowChannel(int id)
        {
            //TODO : notificação
            bool follow = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Channel channel = db.Query<Channel>("SELECT Channel.Price, Channel.IsPrivate" +
                                                                                "FROM Channel" +
                                                                                "WHERE Channel.Id ={0} AND Channel.Visible = true", id).FirstOrDefault();
                    bool accepted = true;
                    if (channel.IsPrivate == false)
                    {
                        accepted = true;
                    }
                     ChannelUser user = db.SelectParam<ChannelUser>(chn => chn.UserId == NimbusUser.UserId &&
                                                                           chn.ChannelId == id).FirstOrDefault();
                        if (user == null)//novato
                        {
                            db.Insert<ChannelUser>(new ChannelUser
                            {
                                ChannelId = id,
                                Interaction = 0,
                                Follow = true,
                                Vote = false,
                                UserId = NimbusUser.UserId,
                                Accepted = accepted
                            });
                            follow = true;
                        }
                        else //já existia
                        {
                            if (user.Follow == false)
                            {
                                db.UpdateOnly(new ChannelUser { Follow = true}, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId);
                                follow = true;
                            }
                            else if (user.Follow == true)
                            {
                               db.UpdateOnly(new ChannelUser { Follow = false}, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId);
                               follow = false;
                            }
                        }
                    
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return follow;
        }
                    
        /// <summary>
        /// add ou retira as permissões dos moderadores para o canal
        /// </summary>
        /// <param name="userModerator"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public string AddModerator(List<Role> userModerator, int id)
        {
            //TODO: notificação
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
                            bool allow = db.SelectParam<Role>(role => role.UserId == NimbusUser.UserId && role.ChannelId == id)
                                                                                .Exists (role => role.IsOwner == true || role.ModeratorManager == true);


                            int countModerator = db.SelectParam<Role>(mdr => mdr.ChannelId == id &&
                                                                                      (mdr.ModeratorManager == true || mdr.MessageManager == true
                                                                                       || mdr.ChannelMagager == true || mdr.TopicManager == true 
                                                                                       || mdr.UserManager == true)).Count();

                            //organizationId == 0 => nimbus, portanto free
                            int orgID = db.SelectParam<Channel>(ch => ch.Id == id).Select(ch => ch.OrganizationId).FirstOrDefault();

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
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
        [HttpPut]
        public bool ReadChannelLater(int id, DateTime readOn)
        {
            bool operation = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //se ja existir = retirar//se não existir = criar
                    UserChannelReadLater user = db.SelectParam<UserChannelReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.ChannelId == id).FirstOrDefault();
                    if (user != null)
                    {
                        //retirando
                        user.Visible = false;
                        user.ReadOn = null;
                    }
                    else
                    {
                        user.Visible = true;
                        user.UserId = NimbusUser.UserId;
                        user.ReadOn = readOn;
                        user.Date = DateTime.Now;
                        user.ChannelId = id;
                    }
                    db.Save(user);
                }
                operation = true;
            }
            catch (Exception ex)
            {
                operation = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
        [HttpPost]
        public bool AddTagsChannel(int id, List<string> tagsList)
        {
            bool flag = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool isOwner = IsOwner(id);
                            bool isManager = IsManager(id);

                            bool isPrivate = db.SelectParam<Channel>(ch => ch.Id == id).Select(p => p.IsPrivate).FirstOrDefault();
                            bool allOk = false;

                            if (isOwner == true || isManager == true)//usuario possui permissao
                            {
                                //colocar restrição para canal free
                                if (isPrivate == false)
                                {
                                    int countTag = db.SelectParam<TagChannel>(ch => ch.ChannelId == id).Count();
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

                                //add as tags
                                if (allOk == true)
                                {
                                    string text = string.Empty;
                                    foreach (string item in tagsList)
                                    {
                                        int i = 0;
                                        text = item;
                                        while (text.StartsWith("#"))
                                        {
                                            text = text.Substring(i + 1);
                                            i++;
                                        }
                                        Tag tag = new Tag
                                        {
                                            TagName = text
                                        };
                                        db.Save(tag);

                                        TagChannel tagChannel = new TagChannel
                                        {
                                            ChannelId = id,
                                            TagId = (int)db.GetLastInsertId(),
                                            Visible = true
                                        };
                                        db.Save(tagChannel);
                                    }
                                    flag = true;
                                }
                            }
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            flag = false;
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                flag = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return flag;
        }

        /// <summary>
        /// Troca a visibilidade (deleta) a tag escolhida
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="tagList"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        public bool DeleteTagChannel(int id, List<int> tagList)
        {
            bool flag = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool isOWner = IsOwner(id);
                            bool isManager = IsManager(id);

                            if (isOWner == true || isManager == true)//usuario possui permissao
                            {
                                foreach (int item in tagList)
                                {
                                    var dado = new TagChannel() { Visible = false };
                                    db.Update<TagChannel>(dado, ch => ch.ChannelId == item);
                                    db.Save(dado);
                                }
                                flag = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            flag = false;
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                flag = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return flag;
        }

        /// <summary>
        /// Criar um canal
        /// </summary>
        /// <param name="newChannel"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public Channel NewChannel(Channel channel)
        {
            //TODO:Notificação
            try
            {
                channel.CreatedOn = DateTime.Now;
                channel.Followers = 0;
                channel.LastModification = DateTime.Now;

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = false; //zero = padrao Nimbus
                    if (channel.OrganizationId != 0)
                    {
                        int idUser = db.SelectParam<OrganizationUser>(us => us.UserId == NimbusUser.UserId
                                                                                   && us.OrganizationId == channel.OrganizationId).Select(us => us.UserId).FirstOrDefault();

                        bool isManager = db.SelectParam<Role>(us => us.UserId == idUser)
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
                                db.Insert(channel);
                                int channelID = (int)db.GetLastInsertId();

                                VoteChannel vote = new VoteChannel
                                {
                                    ChannelId = channelID,
                                    Score = 0
                                };
                                db.Insert(vote);
                                trans.Commit();
                               
                            }
                            catch (Exception ex)
                            {                             
                                trans.Rollback();
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return channel;
        }
        
        /// <summary>
        /// Méetodo para editar as informações de um canal
        /// </summary>
        /// <param name="editChannel"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        public Channel EditChannel(Channel editChannel)
        {
            Channel channel = new Channel();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool isOwner = IsOwner (editChannel.Id);

                    bool isManager = IsManager(editChannel.Id);
                                                                       
                    if (isOwner == true || isManager == true)
                    {
                        channel = db.SelectParam<Channel>(chn => chn.Id == editChannel.Id && chn.Visible == true).FirstOrDefault();

                        channel.Name = editChannel.Name;
                        channel.CategoryId = editChannel.CategoryId;
                        channel.Description = editChannel.Description;
                        channel.ImgUrl = editChannel.ImgUrl;
                        channel.IsCourse = editChannel.IsCourse;
                        channel.IsPrivate = editChannel.IsPrivate;
                        channel.LastModification = DateTime.Now;
                        channel.OpenToComments = editChannel.OpenToComments;
                        channel.OrganizationId = editChannel.OrganizationId;
                        channel.Price = editChannel.Price;
                        channel.Visible = editChannel.Visible;
                        
                        db.Update(channel);
                        db.Save(channel);
                        //TODO: Notificação
                    }
                    else 
                    {
                        channel = null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return channel;
        }

        /// <summary>
        /// Método de adicionar/atualizar os scores de um canal
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        public int VoteChannel(int vote, int id)
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
                            bool? voted = db.SelectParam<ChannelUser>(vt => vt.UserId == NimbusUser.UserId && vt.ChannelId == id && vt.Accepted == true)
                                                                                     .Select(vt => vt.Vote).FirstOrDefault();
                            if (voted == null)
                            {
                                VoteChannel vtChannel = new VoteChannel
                                {
                                    Score = vote
                                };
                                int nota = db.Query<int>("UPDATE VoteChannel SET VoteChannel.Score = VoteChannel.Score + @score OUTPUT INSERTED.Score " +
                                                                "WHERE VoteChannel.Channel_ID = @channelID",
                                                                new { score = vote, channelID = id }).FirstOrDefault();

                                ChannelUser chnUser = new ChannelUser { Vote = true, Score = vote };
                                db.Update<ChannelUser>(chnUser, chn => chn.UserId == NimbusUser.UserId && chn.ChannelId == id);

                                score = nota;
                                trans.Commit();
                            }
                            else if (voted == true)
                            {
                                VoteChannel vtChannel = new VoteChannel
                                {
                                    Score = vote
                                };

                                int notaVelha = db.SelectParam<ChannelUser>(vt => vt.ChannelId == id && vt.UserId == NimbusUser.UserId)
                                                                                           .Select(vt => vt.Score).FirstOrDefault();

                                int nota = db.Query<int>("UPDATE VoteChannel SET VoteChannel.Score = VoteChannel.Score + @score OUTPUT INSERTED.Score " +
                                                                "INNER JOIN ChannelUser ON VoteChannel.Channel_ID = ChannelUser.ChannelId " +
                                                                "WHERE VoteChannel.Channel_ID = @channelID AND ChannelUser.UserId = @user ",
                                                                new { score = (vote - notaVelha), channelID = id, user = NimbusUser.UserId }).FirstOrDefault();

                                ChannelUser chnUser = new ChannelUser { Score = nota };
                                db.Update<ChannelUser>(chnUser, chn => chn.UserId == NimbusUser.UserId && chn.ChannelId == id);

                                score = nota;
                                trans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return score;
        }

        /// <summary>
        /// Lista os usuários que desejam entrar para o canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<User> ListPendingUSer(int id)
        {
            List<User> list = new List<User>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = db.SelectParam<Role>(role => role.UserId == NimbusUser.UserId)
                                                    .Exists(role => role.IsOwner == true || role.ChannelMagager == true || role.UserManager == true);
                    if (allow == true)
                    {
                        List<int> idUsers = db.SelectParam<ChannelUser>(ch => ch.ChannelId == id && (ch.Accepted == false && ch.Visible == true))
                                                                             .Select(ch => ch.UserId).ToList();
                        foreach (int item in idUsers)
                        {
                            User user = db.Select<User>("SELECT User.FirstName, User.LastName, User.AvatarUrl, User.Id " +
                                                                             "FROM User WHERE User.Id = {0}", item).FirstOrDefault();
                            if(user != null)
                            {                                  
                                list.Add(user);
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
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return list;
        }

        /// <summary>
        /// Método para aceitar/recusar um usuário que se inscreveu para o canal
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        public bool AcceptUser(ChannelBag dados)
        {
            bool accept = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = db.SelectParam<Role>(ch => ch.ChannelId == dados.Id && ch.UserId == NimbusUser.UserId)
                                                                        .Exists(ch => ch.IsOwner == true || ch.UserManager == true ||
                                                                                ch.ChannelMagager == true);
                    if (allow == true)
                    {
                        ChannelUser channel = new ChannelUser
                        {
                           Accepted = dados.isAccept
                        };
                        db.Update<ChannelUser>(channel, ch => ch.UserId == dados.userID && ch.ChannelId == dados.Id);

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
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return accept;
        }

        /// <summary>
        /// Retorna a posição do channel no ranking 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public int RankingChannel(int id)
        {
            int ranking = 0;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    int score = db.SelectParam<VoteChannel>(vt => vt.ChannelId == id).Select(vt => vt.Score).FirstOrDefault();
                    ranking = db.Select<int>("WITH Rankings AS (SELECT VoteChannel.ChannelId, Ranking = Dense_Rank() OVER(ORDER BY VoteChannel.Score DESC) " +
                                                                "FROM VoteChannel) "+
                                              "SELECT Ranking FROM Rankings "+
                                              "Where VoteChannel.ChannelId= {0}", id).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return ranking;
        }

        #endregion

    }



}
