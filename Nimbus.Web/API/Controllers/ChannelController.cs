using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;
using Nimbus.Model.ORM;
using Nimbus.Model.Bags;

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
                    bool allow = db.SelectParam<Role>(r => r.UserId == NimbusUser.UserId).Exists(r => r.IsOwner == true || r.ChannelMagager == true);
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
            List<int> idList = new List<int>();
            List<User> moderators = new List<User>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    idList = db.SelectParam<Role>(rl => rl.ChannelId == id).Where(rl => (rl.ChannelMagager == true ||
                                                                                        rl.MessageManager == true ||
                                                                                        rl.ModeratorManager == true ||
                                                                                        rl.TopicManager == true ||
                                                                                        rl.UserManager == true) && rl.IsOwner == false).Select(user => user.UserId).ToList();
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
                    idList = db.SelectParam<Role>(rl => rl.ChannelId == id).Where(rl => rl.ChannelMagager == true ||
                                                                                        rl.MessageManager == true).Select(user => user.UserId).ToList();
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
                    Role role = db.SelectParam<Role>(rl => rl.ChannelId == id && rl.UserId == NimbusUser.UserId).FirstOrDefault();
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

                    Channel channel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.Id == id).FirstOrDefault();

                    #region verifica permissão
                    if (channel != null)
                    {
                        if (channel.Price > 0 || channel.IsPrivate == true)
                        {
                            bool paid = Paid(id);
                            bool accepted = IsAccepted(id);

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
                        allow = true;
                    
                    } 
                    #endregion

                    if (allow == true)
                    {
                        int topidID = db.SelectParam<Topic>(tp => tp.ChannelId == id && tp.Visibility == true).Select(tp => tp.Id).FirstOrDefault();                    
                        List<Comment> listComments = db.SelectParam<Comment>(cm => cm.TopicId == topidID &&  cm.Visible == true);

                        int userComment = listComments.Where(us => us.UserId == NimbusUser.UserId).Count();
                        string firstName = db.SelectParam<User>(us => us.Id == channel.OwnerId).Select(us => us.FirstName).FirstOrDefault();
                        string lastName = db.SelectParam<User>(us => us.Id == channel.OwnerId).Select(us => us.LastName).FirstOrDefault();
                        showChannel.Name = channel.Name;
                        showChannel.CategoryId = channel.CategoryId;
                        showChannel.Id = channel.Id;
                        showChannel.countFollowers = channel.Followers.ToString();
                        showChannel.OrganizationId = channel.OrganizationId;
                        showChannel.OwnerId = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        showChannel.OpenToComments = channel.OpenToComments;
                        if (userComment > 0 && listComments.Count > 0)
                        {
                            showChannel.participationChannel = ((userComment * 100) / listComments.Count()).ToString();
                        }
                        else
                            showChannel.participationChannel = "0%";
                        showChannel.ImgUrl = channel.ImgUrl;
                        showChannel.OwnerName = firstName + " " + lastName;
                        showChannel.CountVotes = db.SelectParam<VoteChannel>(vt => vt.ChannelId == id).Select(vt => vt.Score).Count();
                        
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
        /// Retorna os canais encontrados para a palavra/tag buscada 
        /// </summary>
        /// <param name="q">query de pesquisa</param>
        /// <returns></returns>
        [HttpGet]
        public List<Channel> SearchChannel(string q)
        {
            List<Channel> channels = new List<Channel>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
                try
                {
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        //verificar se é tag
                        if (q.StartsWith("#"))
                        {
                            int i = 0;
                            while (q.StartsWith("#"))
                            {
                                q = q.Substring(i + 1);
                                i++;
                            }
                            int tagID = db.SelectParam<Tag>(tag => tag.TagName.ToLower() == q.ToLower()).Select(tag => tag.Id).FirstOrDefault();

                            List<int> idChannels = db.SelectParam<TagChannel>(tgc => tgc.TagId == tagID).Select(tgc => tgc.ChannelId).ToList();

                            foreach (int item in idChannels)
                            {
                                Channel chn = db.SelectParam<Channel>(ch => ch.Id == item && ch.Visible == true && ch.OrganizationId == idOrg).FirstOrDefault();
                                if (chn != null)
                                    channels.Add(chn);
                            }

                        }
                        else
                        {
                            //pegar canais da categoria
                            int idCat = db.SelectParam<Category>(ct => ct.Name.ToLower() == q.ToLower()).Select(ct => ct.Id).FirstOrDefault();
                            if (idCat > 0)
                            {
                                channels = db.SelectParam<Channel>(chn => (chn.Name.Contains(q) || chn.CategoryId == idCat)
                                                                        && chn.Visible == true && chn.OrganizationId == idOrg);
                            }
                            else
                            {
                                channels = db.SelectParam<Channel>(chn => chn.Name.Contains(q) && chn.Visible == true && chn.OrganizationId == idOrg);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + q + "'"));
            }
            return channels;
        }
        
       
                   
        /// <summary>
        /// visualizar 'meus canais'
        /// </summary>
        /// <returns></returns>        
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
        /// Método que retorna os canais pagos que o usuário comprou
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<Channel> UserChannelPaid(int id)
        {
            List<Channel> listChannel = new List<Channel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> idsChannel = db.SelectParam<Role>(rl => rl.Paid == true && rl.Accepted == true &&  rl.UserId == id).Select(rl => rl.ChannelId).ToList();

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
        [HttpGet]
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
        public List<Channel> FollowsChannel(int id)
        {
            List<Channel> listChannel = new List<Channel>();
            List<int> listUserChannel = new List<int>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {                    
                    listUserChannel = db.SelectParam<ChannelUser>(ch => ch.UserId == NimbusUser.UserId && ch.Visible == true && ch.Follow == true)
                                                                 .Select(ch => ch.ChannelId).ToList();
                    if (listUserChannel.Count > 0)
                    {
                        foreach (int item in listUserChannel)
                        {
                            Channel channel = new Channel();
                            channel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.OrganizationId == id && ch.Id == item).First();
                            listChannel.Add(channel);
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
        /// Método para retornar os channels que o usuário vai ler mais tarde
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<Channel> showReadLaterChannel(int id)
        {
            List<Channel> listChannel = new List<Channel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> listUserChannel = db.SelectParam<UserChannelReadLater>(ch => ch.UserId == NimbusUser.UserId && ch.Visible == true)
                                                                 .Select(ch => ch.ChannelId).ToList();
                    if (listUserChannel.Count > 0)
                    {
                        foreach (int item in listUserChannel)
                        {
                            Channel channel = new Channel();
                            channel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.OrganizationId == id && ch.Id == item).First();
                            listChannel.Add(channel);
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

        #endregion


        #region métodos de interação com o canal
               
        /// <summary>
        /// Caso o usuário deseje deletar o channel, ele perde a posse e o channel passa a ser do 'nimbus'
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpDelete]
        public bool DeleteChannel(int id)
        {
            bool flag = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    int idOwner = db.Select<int>("SELECT Channel.OwnerId FROM Channel "+
                                                  "INNER JOIN Role ON Channel.Id = Role.ChannelId "+
                                                  "WHERE Channel.Id = {0} AND Channel.UserId = {1} AND Channel.Visible = true AND Role.IsOwner = true",
                                                  id, NimbusUser.UserId).FirstOrDefault();

                    if (idOwner > 0)
                    {
                        db.UpdateOnly(new Channel { OwnerId = 0 }, chn => chn.OwnerId, chn => chn.Id == id);
                        flag = true;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return flag;
 
        }
                 
        /// <summary>
        /// seguir/ñ seguir canal
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
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
        [HttpPost]
        public UserBag AddModerator(Role userModerator)
        {
            //TODO: notificação
            UserBag bag = new UserBag();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    using(var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool prox = false;
                            bool allow = db.SelectParam<Role>(role => role.UserId == NimbusUser.UserId && role.ChannelId == userModerator.ChannelId)
                                                                                .Exists (role => role.IsOwner == true || role.ModeratorManager == true);


                            int countModerator = db.SelectParam<Role>(mdr => mdr.ChannelId == userModerator.ChannelId &&
                                                                                      (mdr.ModeratorManager == true || mdr.MessageManager == true
                                                                                       || mdr.ChannelMagager == true || mdr.TopicManager == true 
                                                                                       || mdr.UserManager == true)).Count();

                            //organizationId == 0 => nimbus, portanto free
                            int orgID = db.SelectParam<Channel>(ch => ch.Id == userModerator.ChannelId).Select(ch => ch.OrganizationId).FirstOrDefault();

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

                                    db.Update<Role>(role);                                   
                                }
                                else
                                {
                                    db.Save(userModerator);
                                }

                                bag.Id = user.Id;
                                bag.FirstName = user.FirstName;
                                bag.LastName = user.LastName;
                                bag.AvatarUrl = user.AvatarUrl;                                   

                                if (userModerator.ChannelMagager)
                                    bag.RoleInChannel = "";

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
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return bag;
        }

        /// <summary>
        /// Add/retirar channel da lista de ler mais tarde 
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="readOn"></param>
        /// <returns></returns>
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
        [HttpPost]
        public Tag AddTagsChannel(int id, string tag)
        {
            Tag newTag = new Tag();
            tag = tag.Trim('#');
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

                                        TagChannel tagChannel = new TagChannel
                                        {
                                            ChannelId = id,
                                            TagId = (int)db.GetLastInsertId(),
                                            Visible = true
                                        };
                                        db.Save(tagChannel);

                                        newTag.TagName = ntag.TagName;
                                        newTag.Id = ntag.Id;
                                    }                                   
                                }
                            }
                            else
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
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
        /// Troca a visibilidade (deleta) a tag escolhida
        /// </summary>
        /// <param name="channelID"></param>
        /// <param name="tagID></param>
        /// <returns></returns>
        [HttpDelete]
        public bool DeleteTagChannel(int id, int tagID)
        {
            bool isDelete = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {

                    bool isOWner = IsOwner(id);
                    bool isManager = IsManager(id);

                    if (isOWner == true || isManager == true)//usuario possui permissao
                    {
                        TagChannel dado = db.SelectParam<TagChannel>(ch => ch.ChannelId == id && ch.TagId == tagID).FirstOrDefault();
                        dado.Visible = false;

                        db.Update<TagChannel>(dado);
                        isDelete = true;
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
            return isDelete;
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
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {

                    bool isOWner = IsOwner(id);
                    bool isManager = db.SelectParam<Role>(r => r.ChannelId == id && r.UserId == NimbusUser.UserId).Exists(r => r.IsOwner == true || 
                                                                                                                              r.ChannelMagager == true ||
                                                                                                                              r.ModeratorManager == true);

                    if (isOWner == true || isManager == true)//usuario possui permissao
                    {
                        Role role = db.SelectParam<Role>(r => r.UserId == userID && r.ChannelId == id).FirstOrDefault();
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
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
                                channel.Id = channelID;
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
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool isOwner = IsOwner (editChannel.Id);

                    bool isManager = IsManager(editChannel.Id);
                                                                       
                    if (isOwner == true || isManager == true)
                    {
                        channel = db.SelectParam<Channel>(chn => chn.Id == editChannel.Id && chn.Visible == true).FirstOrDefault();

                        channel.Name = !string.IsNullOrEmpty(editChannel.Name) ? System.Web.HttpUtility.HtmlEncode(editChannel.Name) : channel.Name;
                        channel.CategoryId = editChannel.CategoryId > 0? editChannel.CategoryId : channel.CategoryId;
                        channel.Description = !string.IsNullOrEmpty(editChannel.Description)? System.Web.HttpUtility.HtmlEncode(editChannel.Description): channel.Description;
                        channel.ImgUrl = db.SelectParam<Category>(c => c.Id == editChannel.CategoryId).Select(c => c.ImageUrl).FirstOrDefault();
                        channel.IsCourse = editChannel.IsCourse ;
                        channel.IsPrivate = editChannel.IsPrivate;
                        channel.LastModification = DateTime.Now;
                        channel.OpenToComments = editChannel.OpenToComments;
                        channel.Price = editChannel.Price != -1? editChannel.Price : 0;
                        channel.Visible = editChannel.Visible;
                        
                        db.Update<Channel>(channel);
                        //TODO: Notificação
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

            return channel;
        }

        /// <summary>
        /// Método de adicionar/atualizar os scores de um canal
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
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
        [HttpPut]
        public ChannelBag AcceptUser(ChannelBag dados)
        {            
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
