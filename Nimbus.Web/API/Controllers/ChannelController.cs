﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;
using Nimbus.Model.ORM;
using Nimbus.Model.Bags;
using Nimbus.Web.Utils;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Canais.
    /// </summary>
    [NimbusAuthorize]
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
                    allow = db.SelectParam<Role>(mg => mg.UserId == NimbusUser.UserId && mg.ChannelId == id && mg.Accepted == true)
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
        /// verifica se a tag já existe e valida a tag retirando o '#'
        /// </summary>
        /// <param name="listtag"></param>
        /// <returns>Lista de tags existentes</returns>
        [NonAction]
        [HttpGet]
        public Tag ValidateTag(string tag)
        {
            Tag returntag = new Tag();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    string text = string.Empty;
                    text = tag;
                    int pos = text.IndexOf("#");
                    if(pos != -1)
                    {
                        text = text.Substring(pos + 1);
                    }
                    returntag = db.SelectParam<Tag>(tg => tg.TagName.ToLower() == text.ToLower()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return returntag;
        }


        /// <summary>
        /// Método de retornar todas as tags relacionadas ao canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpGet]
        public List<Tag> ShowTagChannel(int id = 0)
        {
            List<int> tagList = new List<int>();
            List<Tag> tagChannel = new List<Tag> ();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    tagList = db.SelectParam<TagChannel>(tg => tg.ChannelId == id && tg.Visible == true).Select(tg => tg.TagId).ToList();
                    foreach (int item in tagList)
                    {		               
                        Tag tag = db.SelectParam<Tag>(tg => tg.Id == item).FirstOrDefault();
                        if(tag != null)
                        {
                            tagChannel.Add(tag);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return tagChannel;
        }

        /// <summary>
        /// Método que lista as tags de um channel caso o usuário tenha permissao para editá-las
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<Tag> ShowTagChannelEdit(int id = 0)
        {
            
            List<Tag> tagChannel = new List<Tag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = db.SelectParam<Role>(r => r.UserId == NimbusUser.UserId).Exists(r => r.IsOwner == true || (r.ChannelMagager == true && r.Accepted == true));
                    if (allow == true)
                    {
                        ICollection<int> tagList = db.SelectParam<TagChannel>(tg => tg.ChannelId == id && tg.Visible == true).Select(tg => tg.TagId).ToList();
                        foreach (int item in tagList)
                        {
                            Tag tag = db.SelectParam<Tag>(tg => tg.Id == item).FirstOrDefault();
                            if (tag != null)
                            {
                                tagChannel.Add(tag);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return tagChannel;
        }

        /// <summary>
        /// Método retorna todos os moderadores do canal
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<User> ShowModerators(int id = 0)
        {
            List<Role> idList = new List<Role>();
            List<User> moderators = new List<User>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    idList = db.Where<Role>(rl => rl.ChannelId == id && rl.IsOwner == false && ((rl.ChannelMagager == true ||
                                                                                                rl.MessageManager == true ||
                                                                                                rl.ModeratorManager == true ||
                                                                                                rl.TopicManager == true ||
                                                                                                rl.UserManager == true) && rl.Accepted != false)).ToList();
                    foreach (var item in idList)
                    {                        
                        User user = db.SelectParam<User>(us => us.Id == item.UserId).FirstOrDefault();
                        if (user != null)
                        {
                            if (item.Accepted == null)
                                user.LastName = user.LastName + " (pendente)";
                            moderators.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return moderators;
        }

        /// <summary>
        /// Método que retornar os moderadores de um canal e uma string referente a permissão que eles tem, 
        /// deve ser usado para exibir em tela de edição
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<UserBag> ShowModeratorsEdit(int id = 0)
        {
            List<UserBag> moderators = new List<UserBag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    ICollection<Role> roleList = db.SelectParam<Role>(rl => rl.ChannelId == id).Where(rl => (rl.ChannelMagager == true ||
                                                                                        rl.MessageManager == true ||
                                                                                        rl.ModeratorManager == true ||
                                                                                        rl.TopicManager == true ||
                                                                                        rl.UserManager == true) && rl.IsOwner == false).ToList();
                    foreach (Role item in roleList)
                    {
                        User user = db.SelectParam<User>(us => us.Id == item.UserId).FirstOrDefault();
                        if (user != null)
                        {
                            UserBag bag = new UserBag();
                            bag.FirstName = user.FirstName;

                            if(item.Accepted == null)
                            bag.LastName = user.LastName + " (pendente)";
                            else
                            bag.LastName = user.LastName;

                            bag.Id = user.Id;
                            bag.AvatarUrl = user.AvatarUrl;

                            if (item.ChannelMagager == true)
                                bag.RoleInChannel = "Todas";
                            else if (item.MessageManager == true)
                                bag.RoleInChannel = "Moderar mensagens";
                            else if (item.ModeratorManager == true)
                                bag.RoleInChannel = "Moderar moderadores";
                            else if (item.TopicManager == true)
                                bag.RoleInChannel = "Moderar tópicos";
                            else if (item.UserManager == true)
                                bag.RoleInChannel = "Moderar usuários";

                            moderators.Add(bag);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return moderators;
        }

        /// <summary>
        /// Método que retorna os usuarios que moderam o canal com permissao p/ ver/responder as mensagens
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<User> GetMessageModerators(int id = 0)
        {
            List<int> idList = new List<int>();
            List<User> moderators = new List<User>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    idList = db.SelectParam<Role>(rl => rl.ChannelId == id).Where(rl => rl.Accepted == true && (rl.ChannelMagager == true ||
                                                                                        rl.MessageManager == true)).Select(user => user.UserId).ToList();
                    foreach (int item in idList)
                    {
                        User user = db.SelectParam<User>(us => us.Id == item).FirstOrDefault();
                        if (user != null)
                        {
                            moderators.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return moderators;
        }

        /// <summary>
        /// método para retornar em string as permissoes do current user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<string> ReturnRolesUser(int id = 0)
        {
            List<string> roles = new List<string>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Role role = db.SelectParam<Role>(rl => rl.ChannelId == id && rl.UserId == NimbusUser.UserId && rl.Accepted == true).FirstOrDefault();
                    if (role != null)
                    {
                        if (role.ChannelMagager == true)
                            roles.Add("channelmanager");
                        if (role.MessageManager == true)
                            roles.Add("messagemanager");
                        if (role.ModeratorManager == true)
                            roles.Add("moderatormanager");
                        if (role.TopicManager == true)
                            roles.Add("topicmanager");
                        if (role.UserManager == true)
                            roles.Add("usermanager");
                    }
                      
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return roles;
        }

        #region métodos de visualização
        /// <summary>
        /// carregar informações gerais do canal
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ChannelBag ShowChannel(int id = 0)
        {
            ChannelBag showChannel = new ChannelBag();
            try
            {

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    AlertChannelPay alert = new AlertChannelPay();
                    bool allow = false;
                    bool accepted = false;
                    Channel channel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.Id == id).FirstOrDefault();
                    if (channel == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }

                    #region verifica permissão
                    if (channel != null)
                    {
                        if (channel.Price > 0 || channel.IsPrivate == true)
                        {
                            bool paid = Paid(id);
                            accepted = IsAccepted(id);

                            if (paid == true || accepted == true)
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
                                else if (accepted == false)
                                {
                                    showChannel.messageAlert = alert.AlertPrivate;
                                }
                            }
                        }
                        else
                        {
                            allow = true;
                            accepted = true;
                        }
                    } 
                    #endregion

                    if (allow == true)
                    {
                        int topidID = db.SelectParam<Topic>(tp => tp.ChannelId == id && tp.Visibility == true).Select(tp => tp.Id).FirstOrDefault();                    
                        List<Comment> listComments = db.SelectParam<Comment>(cm => cm.TopicId == topidID &&  cm.Visible == true);

                        int userComment = listComments.Where(us => us.UserId == NimbusUser.UserId).Count();
                        string firstName = db.SelectParam<User>(us => us.Id == channel.OwnerId).Select(us => us.FirstName).FirstOrDefault();
                        string lastName = db.SelectParam<User>(us => us.Id == channel.OwnerId).Select(us => us.LastName).FirstOrDefault();

                        var channelUser = db.Where<ChannelUser>(c => c.ChannelId == id && c.UserId == NimbusUser.UserId && c.Visible == true).Where(c => c != null).FirstOrDefault();
                      
                        showChannel.Name = channel.Name;
                        showChannel.Description = channel.Description;
                        showChannel.CategoryId = channel.CategoryId;
                        showChannel.Id = channel.Id;
                        showChannel.countFollowers = db.Where<ChannelUser>(c => c.ChannelId == id && c.Follow == true && c.Visible == true).Count().ToString();
                        showChannel.OrganizationId = channel.OrganizationId;
                        showChannel.OwnerId = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        showChannel.OpenToComments = channel.OpenToComments;
                        showChannel.UserFollow = channelUser != null ? channelUser.Follow : false; 
                        if (userComment > 0 && listComments.Count > 0)
                        {
                            showChannel.participationChannel = ((userComment * 100) / listComments.Count()).ToString();
                        }
                        else
                            showChannel.participationChannel = "0%";
                        showChannel.ImgUrl = channel.ImgUrl;
                        showChannel.OwnerName = firstName + " " + lastName;
                        showChannel.CountVotes = db.SelectParam<VoteChannel>(vt => vt.ChannelId == id).Select(vt => vt.Score).FirstOrDefault();
                        showChannel.isAccept = accepted;
                        showChannel.UserVoteChannel = (channelUser != null && channelUser.Vote != null)? channelUser.Score : 0;
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
        /// carregar informações basicas de um canal para exibir na login page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ChannelBag InfoChannelToLoginPage(int id = 0)
        {
            ChannelBag showChannel = new ChannelBag();
            try
            {

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    AlertChannelPay alert = new AlertChannelPay();
                    bool allow = false;
                    bool accepted = false;
                    Channel channel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.Id == id).FirstOrDefault();
                    if (channel == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                    #region verifica permissão
                    if (channel != null)
                    {
                        if (channel.Price == 0 || channel.IsPrivate == false)
                        {
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                        }
                    }
                    #endregion

                    if (allow == true)
                    {                                                               
                        showChannel.Name = channel.Name;
                        showChannel.Description = channel.Description;
                        showChannel.CategoryId = channel.CategoryId;
                        showChannel.countFollowers = db.Where<ChannelUser>(c => c.ChannelId == id && c.Follow == true && c.Visible == true).Count().ToString(); 
                        showChannel.CountVotes = db.SelectParam<VoteChannel>(vt => vt.ChannelId == id).Select(vt => vt.Score).FirstOrDefault();
                    }
                }
            }

            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return showChannel;
        }

        public class ChannelHtmlWrapper
        {
            public int Count { get; set; }
            public string Html { get; set; }
        }

        [HttpGet]
        public ChannelHtmlWrapper AbstChannelHtml(int? id = 0, string viewBy = null, int categoryID = 0, int skip = 0)
        {
            List<Channel> channels = new List<Channel>();

            int idUser = NimbusUser.UserId;

            if( viewBy == "myChannels")
                channels = MyChannel(id, skip);
            if(viewBy == "channelsFollow")
                channels = FollowsChannel(idUser, skip);
            if (viewBy == "myChannelsMannager")
                channels = ModeratorChannel(idUser, skip); 
            
            var rz = new RazorTemplate();
            string html = "";

            foreach (var channel in channels)
            {
                html += rz.ParseRazorTemplate<Channel>
                    ("~/Website/Views/ChannelPartials/ChannelPartial.cshtml", channel);
            }

            return new ChannelHtmlWrapper { Html = html, Count = channels.Count };
        }


        /// <summary>
        /// visualizar 'meus canais' = criados pelo usuário
        /// </summary>
        /// <returns></returns>        
        [HttpGet]
        public List<Channel> MyChannel(int? id = 0 , int skip = 0)
        {
            List<Channel> listChannel;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                if (id == null)
                {
                    id = NimbusUser.UserId;
                }
                var idsRole = db.Where<Role>(rl => rl.IsOwner == true && rl.UserId == id).Skip(9 * skip).Take(9).Select(rl => rl.ChannelId);

                listChannel = idsRole.Select(rl => db.Where<Channel>(ch => ch.Visible == true && ch.Id == rl)
                                        .FirstOrDefault()).Where(ch => ch != null).ToList();

                foreach (var item in listChannel)
                {
                    if (item.OrganizationId == 1) // quando o cara não é org pagante, nao pode mudar a capa do channel, logo no abstract iria ficar uma 'cor solida feia'
                    {
                        item.ImgUrl = item.ImgUrl.ToLower().Replace("/capachannel/", "/category/");
                    }
                }
            } 
           
            return listChannel;
        }

        /// <summary>
        /// Método que retorna os canais pagos que o usuário comprou
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<Channel> UserChannelPaid(int id)
        {
            List<Channel> listChannel = new List<Channel> ();            
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    listChannel = db.Where<Role>(rl => rl.Paid == true && rl.Accepted == true && rl.UserId == id)
                                    .Select(role => db.Where<Channel>(chn => chn.Visible == true && chn.Id == role.ChannelId).FirstOrDefault())
                                    .Where(ch => ch != null).ToList();
                  
                }
           
            return listChannel;
        }

       /// <summary>
       ///visualizar canais moderados
       /// </summary>
       /// <returns></returns>
        [HttpGet]
        public List<Channel> ModeratorChannel(int? id = 0, int skip = 0)
        {
            List<Channel> listChannel = new List<Channel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (id == null)
                        id = NimbusUser.UserId;

                    List<int> idsChannel = db.SelectParam<Role>(rl => rl.IsOwner == false && ((rl.ModeratorManager == true ||
                                                                                              rl.ChannelMagager == true ||
                                                                                              rl.MessageManager == true ||
                                                                                              rl.TopicManager == true ||
                                                                                              rl.UserManager == true) && rl.Accepted == true)&& rl.UserId == id)
                                                                        .Skip(9 * skip).Take(9)
                                                                        .Select(rl => rl.ChannelId).ToList();
                    List<Category> listCategory = db.Select<Category>();
                    foreach (int item in idsChannel)
                    {
                        Channel channel = (from chn in db.SelectParam<Channel>(chn => chn.Visible == true && chn.Id == item)
                                       select new Channel()
                                       {
                                           OrganizationId = chn.OrganizationId,
                                           Id = chn.Id,
                                           Name = chn.Name,
                                           ImgUrl = listCategory.Where(c => c.Id == chn.CategoryId).Select(c => c.ImageUrl).FirstOrDefault()
                                       }).FirstOrDefault();

                        listChannel.Add(channel);                        
                    }
                    foreach (var item in listChannel)
                    {
                        if (item.OrganizationId == 1) // quando o cara não é org pagante, nao pode mudar a capa do channel, logo no abstract iria ficar uma 'cor solida feia'
                        {
                            item.ImgUrl = item.ImgUrl.ToLower().Replace("/capachannel/", "/category/");
                        }
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
        [HttpGet]
        public List<Channel> AllChannel(int id)
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

        /// <summary>
        /// retorna uma lista com todos os canais follow do usuário dentro da org
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Channel> FollowsChannel(int id, int skip)
        {
            List<Channel> listChannel = new List<Channel>();                      
                using (var db = DatabaseFactory.OpenDbConnection())
                {                    
                   var listUserChannel = db.SelectParam<ChannelUser>(ch => ch.UserId == NimbusUser.UserId && ch.Visible == true && ch.Follow == true)
                                                                 .Skip(15 * skip).Take(15)
                                                                 .Select(ch => ch.ChannelId);

                   listChannel = listUserChannel.Select(rl => db.Where<Channel>(ch => ch.Visible == true && ch.Id == rl)
                                                .FirstOrDefault()).Where(ch => ch != null).ToList();

                   foreach (var item in listChannel)
                   {
                       if (item.OrganizationId == 1) // quando o cara não é org pagante, nao pode mudar a capa do channel, logo no abstract iria ficar uma 'cor solida feia'
                       {
                           item.ImgUrl = item.ImgUrl.ToLower().Replace("/capachannel/", "/category/");
                       }
                   }
                }                
           
            return listChannel;
        }
         
        #endregion


        #region métodos de interação com o canal
               
        /// <summary>
        /// Caso o usuário deseje deletar o channel, ele perde a posse e o channel passa a ser do 'nimbus'
        /// </summary>
        /// <param name="id"></param>
        ///<param name="idUser"></param>
        /// <returns></returns>
        [HttpDelete]
        public string DeleteChannel(int id)
        {
            string message = null; 
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        Role roleOwner = db.Where<Role>(r => (r.ChannelId == id && r.Accepted == true) && r.UserId == NimbusUser.UserId && r.IsOwner == true)
                                       .FirstOrDefault();
                                              
                        var topics = db.Where<Topic>(t => t.ChannelId == id && t.Visibility == true);

                        int countFollowers = db.Where<ChannelUser>(c => c.Visible == true && c.Id == id && c.Follow == true).Count();

                        if (roleOwner != null)
                        {
                            if (countFollowers > 0) //possui seguidores
                            {
                                if (topics.Count > 0)
                                {
                                    DeleteChannelWithTopics(id, roleOwner);
                                }
                                else
                                {
                                    DeleteChannelWithoutTopics(id, roleOwner);
                                    //retirar este channel da lista de canais seguidos
                                    var listUserChannel = db.Where<ChannelUser>(c => c.ChannelId == id && c.Visible == true && c.Follow == true);
                                    foreach (var item in listUserChannel)
                                    {
                                        item.Follow = false;
                                        item.Visible = false;
                                        db.Update<ChannelUser>(item, c => c.ChannelId == id);
                                    }
                                }
                            }
                            else
                            {
                                //não possui seguidores = pode deletar inclusive os tópicos
                                if (topics.Count > 0)
                                {                                    
                                    foreach (var item in topics)
                                    {
                                        db.SqlScalar<int>(@"UPDATE [Comment]
                                                            SET [Comment].[Visible] = 0
                                                            WHERE [Comment].[ChannelId] = @channelId", new{channelId = item.ChannelId});
                                    }
                                    db.SqlScalar<int>(@"UPDATE [Topic]
                                                            SET [Topic].[Visibility] = 0
                                                            WHERE [Topic].[ChannelId] = @channelId", new { channelId = id });

                                    message = DeleteChannelWithoutTopics(id, roleOwner);
                                }
                                else 
                                {
                                   message = DeleteChannelWithoutTopics(id, roleOwner);
                                }

                            }

                        }

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }

            }
            return message;
 
        }

        public string DeleteChannelWithTopics(int id, Role roleOwner) 
        {
            string message;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var moderator = db.Where<Role>(r => (r.ChannelId == id && r.Accepted == true) && r.IsOwner == false && r.ChannelMagager == true).ToList();
                        
                        if (moderator.Count > 0)
                        {
                            if (moderator.Count > 1)
                            {
                                var moderatorBest = db.Where<ChannelUser>(c => c.ChannelId == id && moderator.Exists(m => m.UserId == c.UserId)).OrderBy(c => c.Interaction).Max();
                                int newIdOwner = moderator.Where(m => m.UserId == moderatorBest.UserId).Select(m => m.UserId).FirstOrDefault();

                                moderator[0] = moderator.Where(c => c.UserId == newIdOwner).FirstOrDefault();
                            }
                            
                        }
                        else
                        {
                            Role role = new Role ()
                            {
                                UserId = 1, //nimbus 
                                ChannelId = id
                            };
                            moderator.Add(role); 
                        }

                        message = "Este canal não pode ser apagado pois existem seguidores. O canal foi transferido para o moderador: " +
                              db.Where<User>(u => u.Id == roleOwner.UserId).Select(u => new { Name = u.FirstName + " " + u.LastName }).FirstOrDefault().Name;

                        //atualizando/retirando permissao do dono do canal
                        roleOwner.IsOwner = false;
                        roleOwner.ChannelMagager = false;
                        roleOwner.ModeratorManager = false;
                        roleOwner.MessageManager = false;
                        roleOwner.TopicManager = false;
                        roleOwner.UserManager = false;
                        db.Update<Role>(roleOwner, r => r.UserId == roleOwner.UserId && r.ChannelId == id);

                        //dando permissao para o moderador
                        moderator[0].IsOwner = true;
                        moderator[0].ChannelMagager = true;
                        moderator[0].ModeratorManager = true;
                        moderator[0].MessageManager = true;
                        moderator[0].TopicManager = true;
                        moderator[0].UserManager = true;
                        db.Update<Role>(moderator[0], m => m.UserId == moderator[0].UserId && m.ChannelId == id);
                                             

                        //atualiza no channel quem é o novo dono  
                        Channel channel = db.Where<Channel>(c => c.Id == id && c.Visible == true).FirstOrDefault();
                        channel.OwnerId = moderator[0].UserId;
                        db.Update<Channel>(channel, c=> c.Id == id);
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }
                }
            }
            return message;
        }

        public string DeleteChannelWithoutTopics(int id, Role roleOwner) 
        {
            string message;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                       db.SqlScalar<int>(@"UPDATE [ChannelUser]
                                                  SET [ChannelUser].[Visible] = 0
                                                  WHERE [ChannelUser].[ChannelId] = @chnId AND [ChannelUser].[Visible]= 1", new { chnId = id });                       
                   
                        db.SqlScalar<int>(@"UPDATE [Role]
                                                  SET [Role].[IsOwner] = 0, [Role].[ChannelMagager] = 0,[Role].[MessageManager] = 0,[Role].[Accepted] = 0,
                                                      [Role].[ModeratorManager] = 0,[Role].[TopicManager] = 0,[Role].[UserManager] = 0
                                                  WHERE [Role].[ChannelId] = @chnId", new { chnId = id });

                        Channel channelDelete = db.Where<Channel>(c => c.Id == id && c.Visible == true).FirstOrDefault();
                        channelDelete.Visible = false;
                        db.Update<Channel>(channelDelete, ch => ch.Id == id);
                        message = "/userprofile/index/" + NimbusUser.UserId;

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return message;
        }

        /// <summary>
        /// seguir/ñ seguir canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpPost]
        public ChannelUser FollowChannel(int id)
        {
            //TODO : notificação
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    ChannelUser channelUser = new ChannelUser();
                    Channel channel = db.SelectParam<Channel>(c => c.Id == id && c.Visible == true).FirstOrDefault();
                    bool accepted = false;
                    if (channel.IsPrivate == false)
                    {
                        accepted = true;
                    }
                     ChannelUser user = db.SelectParam<ChannelUser>(chn => chn.UserId == NimbusUser.UserId &&
                                                                           chn.ChannelId == id).FirstOrDefault();
                     if (user == null)//novato
                     {

                         channelUser.ChannelId = id;
                         channelUser.Interaction = 0;
                         channelUser.Follow = true;
                         channelUser.Vote = false;
                         channelUser.UserId = NimbusUser.UserId;
                         channelUser.Accepted = accepted;
                         channelUser.Visible = true;
                         db.Insert<ChannelUser>(channelUser);

                     }
                     else //já existia
                     {
                         if (user.Follow == false)
                         {
                             db.UpdateOnly(new ChannelUser { Follow = true }, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId && usr.ChannelId == id);                             
                             channelUser.Follow = true;
                         }
                         else if (user.Follow == true)
                         {
                             db.UpdateOnly(new ChannelUser { Follow = false }, usr => usr.Follow, usr => usr.UserId == NimbusUser.UserId && usr.ChannelId == id);
                             channelUser.Follow = false;
                             //verificar se é moderador e retirar da tabela de moderar
                             using(var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                             {
                                 try
                                 {
                                     var moderator = db.Where<Role>(c => c.ChannelId == id && c.UserId == NimbusUser.UserId 
                                                                                            && (c.IsOwner == false && (c.ChannelMagager == true || c.MessageManager == true
                                                                                                                        ||c.ModeratorManager == true || c.TopicManager == true
                                                                                                                        || c.UserManager == true))).FirstOrDefault();
                                     if (moderator != null && moderator.UserId > 1)
                                     {
                                         //é moderador = tirar as permissões
                                         moderator.ChannelMagager = false;
                                         moderator.MessageManager = false;
                                         moderator.ModeratorManager = false;
                                         moderator.TopicManager = false;
                                         moderator.UserManager = false;
                                         db.Update<Role>(moderator, c => c.UserId == NimbusUser.UserId && c.ChannelId == id);
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
                         channelUser.ChannelId = id;
                         channelUser.Interaction = user.Interaction;                         
                         channelUser.Vote = user.Vote;
                         channelUser.UserId = NimbusUser.UserId;
                         channelUser.Accepted = user.Accepted;
                         channelUser.Visible = true;
                     }
                     return channelUser;
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }
                    
        /// <summary>
        /// add ou retira as permissões dos moderadores para o canal
        /// </summary>
        /// <param name="userModerator"></param>
        /// <returns></returns>
        [HttpPost]
        public UserBag AddModerator(Role userModerator)
        {
            //TODO: notificação
            UserBag bag = new UserBag();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        bool prox = false;
                        bool allow = db.SelectParam<Role>(role => role.UserId == NimbusUser.UserId && role.ChannelId == userModerator.ChannelId)
                                                                            .Exists(role => (role.IsOwner == true || role.ModeratorManager == true) && role.Accepted == true);


                        int countModerator = db.SelectParam<Role>(mdr => mdr.ChannelId == userModerator.ChannelId && ((mdr.ModeratorManager == true || mdr.MessageManager == true
                                                                                                                         || mdr.ChannelMagager == true || mdr.TopicManager == true
                                                                                                                         || mdr.UserManager == true) && mdr.Accepted == true)).Count();

                        var channel = db.Where<Channel>(ch => ch.Id == userModerator.ChannelId).FirstOrDefault();
                        //organizationId == 0 => nimbus, portanto free
                        int orgID = channel.OrganizationId;

                        if (allow == true)
                        {
                            if (orgID == 1) //canais free, portanto permite apenas 5 moderadores
                            {
                                if (countModerator < 5)
                                    prox = true;
                                else
                                    prox = false;
                            }
                            else if (orgID != 1)
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
                            Role role = db.SelectParam<Role>(r => r.ChannelId == userModerator.ChannelId && r.UserId == userModerator.UserId).FirstOrDefault();
                            User user = db.SelectParam<User>(u => u.Id == userModerator.UserId).FirstOrDefault();

                            //verifica se já existe e havia sido 'deletado'
                            if (role != null)
                            {
                                role.ChannelMagager = userModerator.ChannelMagager;
                                role.MessageManager = userModerator.MessageManager;
                                role.ModeratorManager = userModerator.ModeratorManager;
                                role.TopicManager = userModerator.TopicManager;
                                role.UserManager = userModerator.UserManager;
                                role.Accepted = null; //está pendente
                                db.Update<Role>(role, r=> r.UserId == role.UserId && r.ChannelId == role.ChannelId);
                            }
                            else
                            {
                                userModerator.Accepted = null; //o usuário não aceitou ainda = pendente
                                db.Insert<Role>(userModerator);                              
                            }
                            var modNotif = new Notifications.ModeratorNotification();
                            var currentUser = db.Where<User>(u => u.Id == NimbusUser.UserId).FirstOrDefault();
                            modNotif.CreateModeratorInvite(channel, currentUser, user);

                            bag.Id = user.Id;
                            bag.FirstName = user.FirstName;
                            bag.LastName = user.LastName + " (pendente)";
                            bag.AvatarUrl = user.AvatarUrl;

                            if (userModerator.ChannelMagager)
                                bag.RoleInChannel = "Todos";

                            if (userModerator.MessageManager)
                                bag.RoleInChannel = "Moderar mensagens";

                            if (userModerator.ModeratorManager)
                                bag.RoleInChannel = "Moderar moderadores";

                            if (userModerator.TopicManager)
                                bag.RoleInChannel = "Moderar tópicos";

                            if (userModerator.UserManager)
                                bag.RoleInChannel = "Moderar usuários";

                            if (userModerator.ChannelMagager == true && userModerator.MessageManager == true && userModerator.ModeratorManager == true
                                && userModerator.TopicManager == true && userModerator.UserManager == true)
                                bag.RoleInChannel = "Todas";

                        }
                        else
                        {
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "limite de moderador completo"));
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return bag;
        }

        /// <summary>
        /// Método de aceitar/recusar ser moderador de um canal, retorna o sucesso da operação = finalizada ou interrompida
        /// </summary>
        /// <param name="id">ChannelId</param>
        /// <param name="accepted"></param>
        /// <param name="guid">Guid da notificação</param>
        /// <returns></returns>
        [HttpPost]
        public bool AcceptOrNotBeModerator(int id, bool accepted, Guid guid)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    Role roleUser = db.Where<Role>(c => c.ChannelId == id && c.UserId == NimbusUser.UserId).FirstOrDefault();
                    if (roleUser != null)
                    {
                        roleUser.Accepted = accepted;   
                    }

                    Notification<string> not = db.Where<Notification<string>>(n => n.Id == guid).FirstOrDefault();
                    try
                    {
                        db.Update<Role>(roleUser, r=> r.UserId == NimbusUser.UserId && r.ChannelId == id);
                        db.Delete<Notification<string>>(not);
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return accepted;
        }

        /// <summary>
        /// Add/retirar channel da lista de ler mais tarde 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="readOn"></param>
        /// <param name="willRead"></param>
        /// <returns></returns>   
        [HttpPost]
        public bool ReadChannelLater(int id, DateTime? readOn = null, bool willRead = false)
        {
            bool operation = false;

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                UserChannelReadLater user = db.SelectParam<UserChannelReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.ChannelId == id).FirstOrDefault();
                if (willRead == false)//retirar da lista
                {
                    user.Visible = false;
                    user.ReadOn = null;
                    db.Update<UserChannelReadLater>(user);
                    operation = true;
                }
                else if (willRead == true)//colocar na lista
                {
                    user.Visible = true;
                    readOn = DateTime.Now;
                    if (user == null)
                    {
                        user.UserId = NimbusUser.UserId;
                        user.ChannelId = id;
                        db.Insert<UserChannelReadLater>(user);
                        operation = true;
                    }
                    else
                    {
                        db.Update<UserChannelReadLater>(user);
                        operation = true;
                    }
                }
            }
            return operation;
        }

           
        /// <summary>
        /// Add tags para os canais
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="tagsList"></param>
        /// <returns></returns>
        [HttpPost]
        public Tag AddTagsChannel(int id, string tag)
        {
            Tag newTag = new Tag();
            tag = tag.Trim('#');

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
                                    int countTag = db.SelectParam<TagChannel>(ch => ch.ChannelId == id && ch.Visible == true).Count();
                                    if (countTag <= 4)
                                    {
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
                                    Tag tagsExist = new Tag();
                                    tagsExist = ValidateTag(tag); //retorna as tags já existentes no sistema

                                   if(tagsExist != null)
                                   {
                                        //já existe
                                        TagChannel tagChannel = new TagChannel
                                        {
                                            ChannelId = id,
                                            TagId = tagsExist.Id,
                                            Visible = true
                                        };
                                        db.Save(tagChannel);

                                        newTag.Id = tagsExist.Id;
                                        newTag.TagName = tagsExist.TagName;
                                    }
                                    else
                                    {
                                        //criar uma nova tag na tabela
                                        Tag ntag = new Tag
                                        {
                                            TagName = tag
                                        };
                                        db.Save(ntag);

                                        int idTag = (int)db.GetLastInsertId();
                                        TagChannel tagChannel = new TagChannel
                                        {
                                            ChannelId = id,
                                            TagId = idTag,
                                            Visible = true
                                        };
                                        db.Save(tagChannel);

                                        newTag.TagName = ntag.TagName;
                                        newTag.Id = idTag;
                                    }                                   
                                }
                            }
                            else
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                            }
                            trans.Commit();
                        }
                        catch
                        {
                            trans.Rollback();
                            throw;
                        }
                    }
                }            
            return newTag;
        }

        /// <summary>
        /// Método que muda a permissao de um moderador
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        [HttpPost]
        public string EditPermissionModerator(int id, int userId, int permission)
        {
            string newPermission = "";
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    Role currentRole = db.SelectParam<Role>(r => r.ChannelId == id && r.UserId == userId).FirstOrDefault();
                    if (currentRole != null)
                    {
                        if (permission == 0)
                        {
                            currentRole.ChannelMagager = true;
                            currentRole.TopicManager = false;
                            currentRole.MessageManager = false;
                            currentRole.ModeratorManager = false;
                            currentRole.UserManager = false;
                            newPermission = "Todas";
                        }
                        else if (permission == 1)
                        {
                            currentRole.ChannelMagager = false;
                            currentRole.TopicManager = false;
                            currentRole.MessageManager = true;
                            currentRole.ModeratorManager = false;
                            currentRole.UserManager = false;
                            newPermission = "Moderar mensagens";
                        }
                        else if (permission == 2)
                        {
                            currentRole.ChannelMagager = false;
                            currentRole.TopicManager = false;
                            currentRole.MessageManager = false;
                            currentRole.ModeratorManager = true;
                            currentRole.UserManager = false;
                            newPermission = "Moderar moderadores";
                        }
                        else if (permission == 3)
                        {
                            currentRole.ChannelMagager = false;
                            currentRole.TopicManager = true;
                            currentRole.MessageManager = false;
                            currentRole.ModeratorManager = false;
                            currentRole.UserManager = false;
                            newPermission = "Moderar tópicos";
                        }
                        else if (permission == 4)
                        {
                            currentRole.ChannelMagager = false;
                            currentRole.TopicManager = false;
                            currentRole.MessageManager = false;
                            currentRole.ModeratorManager = false;
                            currentRole.UserManager = true;
                            newPermission = "Moderar usuários";
                        }

                        db.Update<Role>(currentRole);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return newPermission;
        }

        /// <summary>
        /// Class criada para facilitar quando tiver q retornar um objeto ao deletar uma tag
        /// </summary>
        public class CurrentCountTag 
        {
            public int Count { get; set; }
            public bool isDelete { get; set; }
        }

        /// <summary>
        /// Troca a visibilidade (deleta) a tag escolhida
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="tagID></param>
        /// <returns></returns>
        [HttpDelete]
        public CurrentCountTag DeleteTagChannel(int id, int tagID)
        {
            CurrentCountTag current = new CurrentCountTag();
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool isOWner = IsOwner(id);
                    bool isManager = IsManager(id);

                    if (isOWner == true || isManager == true)//usuario possui permissao
                    {

                        var tags = db.SelectParam<TagChannel>(ch => ch.ChannelId == id && ch.Visible == true);
                        
                        TagChannel dado = tags.Where(ch => ch.TagId == tagID).FirstOrDefault();

                        dado.Visible = false;
                        db.Update<TagChannel>(dado);

                        current.isDelete = true;
                        current.Count = (tags.Count()- 1);
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                    }
                }
            
            return current;
        }

        /// <summary>
        /// Método que retira a permissão do usuário para moderar o canal
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [HttpDelete]
        public bool DeleteModeratorChannel(int id, int userID)
        {
            bool isDelete = false;
            using (var db = DatabaseFactory.OpenDbConnection())
            {

                bool isOWner = IsOwner(id);
                bool isManager = db.SelectParam<Role>(r => r.ChannelId == id && r.UserId == NimbusUser.UserId).Exists(r =>(r.IsOwner == true ||
                                                                                                                          r.ChannelMagager == true ||
                                                                                                                          r.ModeratorManager == true) && r.Accepted == true);

                if (isOWner == true || isManager == true)//usuario possui permissao
                {
                    Role role = db.SelectParam<Role>(r => r.UserId == userID && r.ChannelId == id).FirstOrDefault();
                    if (role == null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "usuário não é moderador"));
                    role.Accepted = false;
                    role.ChannelMagager = false;
                    role.MessageManager = false;
                    role.ModeratorManager = false;
                    role.TopicManager = false;
                    role.UserManager = false;

                    db.Update<Role>(role);
                    isDelete = true;
                }
                else
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                }
            }

            return isDelete;
        }

        /// <summary>
        /// Criar um canal
        /// </summary>
        /// <param name="newChannel"></param>
        /// <returns></returns>
        [HttpPost]
        public Channel NewChannel(Channel channel)
        {
            //TODO:Notificação
                channel.CreatedOn = DateTime.Now;
                channel.Followers = 0;
                channel.LastModification = DateTime.Now;
                channel.Description = channel.Description.Length > 200 ? channel.Description.Substring(0, 200): channel.Description;
                channel.Name = channel.Name.Length > 100 ? channel.Name.Substring(0, 100) : channel.Name;
             
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = false; //um = padrao Nimbus
                    if (channel.OrganizationId != 1)
                    {
                        int idUser = db.SelectParam<OrganizationUser>(us => us.UserId == NimbusUser.UserId
                                                                                   && us.OrganizationId == channel.OrganizationId).Select(us => us.UserId).FirstOrDefault();

                        //TODO: verificar se é role ou roleORganization
                        bool isManager = db.SelectParam<Role>(us => us.UserId == idUser)
                                                                        .Exists(us => us.IsOwner == true || (us.ChannelMagager == true && us.Accepted == true));
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
                                channel.Id = channelID;
                                Role role = new Role { 
                                    Accepted = true,
                                    ChannelId= channelID,
                                    ChannelMagager = true,
                                    IsOwner = true,
                                    MessageManager = true,
                                    ModeratorManager = true,
                                    Paid = true,
                                    TopicManager = true,
                                    UserId = NimbusUser.UserId,
                                    UserManager = true
                                };
                                db.Insert<Role>(role);

                                VoteChannel vote = new VoteChannel
                                {
                                    ChannelId = channelID,
                                    Score = 0
                                };
                                db.Insert<VoteChannel>(vote);
                                trans.Commit();

                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw;
                            }
                        }
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                    }
                }          
            return channel;
        }
        
        /// <summary>
        /// Méetodo para editar as informações de um canal
        /// </summary>
        /// <param name="editChannel"></param>
        /// <returns></returns>
        [HttpPost]
        public Channel EditChannel(Channel editChannel)
        {
            Channel channel = new Channel();
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool isOwner = IsOwner (editChannel.Id);

                    bool isManager = IsManager(editChannel.Id);
                                                                       
                    if (isOwner == true || isManager == true)
                    {
                        using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                channel = db.SelectParam<Channel>(chn => chn.Id == editChannel.Id && chn.Visible == true).FirstOrDefault();

                                string titleChn = !string.IsNullOrEmpty(editChannel.Name) ? editChannel.Name : channel.Name;
                                string description = !string.IsNullOrEmpty(editChannel.Description) ? editChannel.Description : channel.Description;
                                string chnImgUrl = db.SelectParam<Category>(c => c.Id == editChannel.CategoryId).Select(c => c.ImageUrl).FirstOrDefault();

                                channel.Name = titleChn.Length > 100 ? titleChn.Substring(0, 100) : titleChn;
                                channel.CategoryId = editChannel.CategoryId > 0 ? editChannel.CategoryId : channel.CategoryId;
                                channel.Description = description.Length > 200 ? description.Substring(0, 200) : description;                                
                                channel.ImgUrl = chnImgUrl.ToLower().Replace("category", "capachannel");
                                channel.IsCourse = editChannel.IsCourse;
                                channel.IsPrivate = editChannel.IsPrivate;
                                channel.LastModification = DateTime.Now;
                                channel.OpenToComments = editChannel.OpenToComments;
                                channel.Price = editChannel.Price != -1 ? editChannel.Price : 0;
                                channel.Visible = channel.Visible;

                                db.Update<Channel>(channel);

                                List<Topic> tpcs = db.Where<Topic>(t => t.ChannelId == channel.Id);
                                foreach (var item in tpcs)
                                {
                                    item.ImgUrl = chnImgUrl;
                                    db.Update<Topic>(item, i => i.Id == item.Id);
                                }
                                trans.Commit();
                            }
                            catch(Exception ex)
                            {
                                trans.Rollback();
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                            }
                        }
                    }
                    else 
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                    }
                }
           
            return channel;
        }

        /// <summary>
        /// Método de adicionar/atualizar os scores de um canal
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        [HttpPost]
        public int VoteChannel(int id,int vote)
        {
            int score = -1;
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool? voted  = null;
                            ChannelUser userChannel = db.Where<ChannelUser>(vt => vt.UserId == NimbusUser.UserId && vt.ChannelId == id && vt.Visible == true).FirstOrDefault();
                            if (userChannel != null)
                                voted = userChannel.Vote;
                            
                            VoteChannel vtChannel = db.Where<VoteChannel>(ch => ch.ChannelId == id).FirstOrDefault();
                            if (vtChannel == null) //criando score no banco
                            {
                                VoteChannel votechn = new VoteChannel
                                {
                                    ChannelId = id,
                                    Score = 0
                                };
                                db.Insert<VoteChannel>(votechn);
                                vtChannel = votechn;
                            }


                            if (voted == null || voted == false)                                
                            {                               
                                int notaChannel = vtChannel.Score;

                                if (userChannel != null)
                                {
                                    userChannel.Vote = true;
                                    userChannel.Score = vote;
                                    db.Update<ChannelUser>(userChannel, c => c.ChannelId == id && c.UserId == NimbusUser.UserId);
                                }
                                else //cria o usuario na tabela do channelUser
                                {
                                    ChannelUser newUser = new ChannelUser()
                                    {
                                        Accepted = NimbusOrganization.Id == 1 ? true : false,
                                        ChannelId = id,
                                        Follow = false,
                                        Interaction = 0,
                                        Score = vote,
                                        UserId = NimbusUser.UserId,
                                        Visible = true,
                                        Vote = true
                                    };
                                    db.Insert<ChannelUser>(newUser);
                                }
                                score = notaChannel + vote;

                                vtChannel.Score = score;
                                db.Update<VoteChannel>(vtChannel);

                                trans.Commit();
                            }
                            else if (voted == true) //já votou, está votando outro valor
                            {
                                int oldVoteUser = userChannel.Score;

                                userChannel.Score = vote;
                                db.Update<ChannelUser>(userChannel, c=> c.UserId == NimbusUser.UserId && c.ChannelId == id);

                                score = (vtChannel.Score - oldVoteUser) + vote;
                                vtChannel.Score = score;

                                db.Update<VoteChannel>(vtChannel);
                                trans.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ;
                        }
                    }
                }
           
            return score;
        }

        /// <summary>
        /// Lista os usuários que desejam entrar para o canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpGet]
        public List<User> ListPendingUSer(int id)
        {
            List<User> list = new List<User>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = db.SelectParam<Role>(role => role.UserId == NimbusUser.UserId)
                                                    .Exists(role => role.IsOwner == true || ((role.ChannelMagager == true || role.UserManager == true) && role.Accepted == true));
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
        [HttpPut]
        public ChannelBag AcceptUser(ChannelBag dados)
        {            
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = db.SelectParam<Role>(ch => ch.ChannelId == dados.Id && ch.UserId == NimbusUser.UserId)
                                                                        .Exists(ch => ch.IsOwner == true || ((ch.UserManager == true ||
                                                                                                             ch.ChannelMagager == true) && ch.Accepted == true));
                    if (allow == true)
                    {
                        ChannelUser channel = new ChannelUser
                        {
                           Accepted = dados.isAccept
                        };
                        db.Update<ChannelUser>(channel, ch => ch.UserId == dados.userID && ch.ChannelId == dados.Id);                       
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return dados;
        }

        /// <summary>
        /// Retorna a posição do channel no ranking 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
