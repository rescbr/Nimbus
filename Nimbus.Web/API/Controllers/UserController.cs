﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using Nimbus.Model.Bags;
using System.Web;
using System.Diagnostics;
using Mandrill;
using Nimbus.Web.Security;
using Nimbus.Web.Website.Models;
using Nimbus.Web.API.Models;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Usuários.
    /// </summary>
    [NimbusAuthorize]
    public class UserController : NimbusApiController
    {

        public class UserAutoCompleteResponse
        {
            public class UserData
            {
                public int Id { get; set; }
                public string AvatarUrl { get; set; }
            }
            public string value { get; set; }
            public UserData data { get; set; }
        }

        public class SuggestionWrapper
        {
            public List<UserAutoCompleteResponse> suggestions { get; set; }
        }
        
        #region métodos de exibir informações do perfil

        ///<summary>
        ///exibir informações perfil usuário logado
        ///</summary>
        [HttpGet]
        public UserBag showProfile()
        {
            return showProfile(NimbusUser.UserId);
        }

        ///<sumary>
        ///método padrão de exibir perfil
        /// </sumary>
        [HttpGet]
        public UserBag showProfile(int? id)
        {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (id == null)
                        id = NimbusUser.UserId;

                    var user = db.SelectParam<User>(usr => usr.Id == id).FirstOrDefault();
                    if (user == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.NotFound);
                    }
                    UserBag userBag = new UserBag();
                    userBag.Id = user.Id;
                    userBag.About = user.About;
                    userBag.AvatarUrl = user.AvatarUrl;
                    userBag.BirthDate = user.BirthDate;
                    userBag.City = user.City;
                    userBag.Country = user.Country;
                    userBag.Experience = user.Experience;
                    userBag.FirstName = user.FirstName;
                    userBag.Interest = user.Interest;
                    userBag.LastName = user.LastName;
                    userBag.Occupation = user.Occupation;
                    userBag.IsUserFacebook = user.Password.StartsWith("nsp") ? false : true;
                    userBag.State = user.State;
                    userBag.Age = (int)Math.Floor((DateTime.Now.Subtract(user.BirthDate).Days) / 365.25);


                    var roles = db.Where<Channel>(c => c.Visible == true && c.OrganizationId == NimbusOrganization.Id)
                                                        .Select(c => db.Where<Role>(rl => rl.UserId == id && rl.ChannelId == c.Id &&
                                                                                     (rl.IsOwner == true || rl.ChannelMagager == true || rl.MessageManager == true
                                                                                       || rl.ModeratorManager == true || rl.TopicManager == true
                                                                                       || rl.UserManager == true)).FirstOrDefault()).Where(r => r != null);
                    int pointsChn = 0; int hundredFollowers = 0;
                    int pointsTpc = 0; int fiftyFollowers = 0;
                    int pointsCmt = 0; 
                    if (roles.Count() > 0)
                    {
                         pointsChn = roles.Select(c => c.UserId == id && c.IsOwner == true).Count() * 50;

                        pointsChn = pointsChn + (roles.Where(c => c.IsOwner == false && c.UserId == id && c.Accepted == true )
                                                      .Select(c => c.ChannelMagager == true || c.MessageManager == true ||
                                                                   c.ModeratorManager == true || c.TopicManager == true || c.UserManager == true)
                                                      .Count() * 25);


                         pointsTpc = roles.Sum(r => db.Where<Topic>(t => t.AuthorId == r.UserId &&
                                                                            t.ChannelId == r.ChannelId && t.Visibility == true).Count());

                        pointsCmt =  db.Where<Comment>(c => c.UserId == id && c.Visible == true).Count();
                       
                     }

                     pointsCmt = pointsCmt * 1;
                     pointsTpc = pointsTpc * 30;


                      fiftyFollowers = db.SqlScalar<int>(@"SELECT COUNT(*) AS [value]
                                                            FROM [Channel] AS [CH]
                                                        WHERE (((
                                                            SELECT COUNT(*)
                                                            FROM [ChannelUser] AS [CS]
                                                            WHERE ([CS].[ChannelId] = [CH].[Id]) AND ([CS].[Follow] = 1) AND ([CS].[Accepted] = 1) AND ([CS].[Visible] = 1)
                                                            )) > 50) AND ([CH].[OwnerId] = @OwnerId) AND ([CH].[Visible] = 1)", new { OwnerId = id });

                      hundredFollowers = db.SqlScalar<int>(@"SELECT COUNT(*) AS [value]
                                                            FROM [Channel] AS [CH]
                                                        WHERE (((
                                                            SELECT COUNT(*)
                                                            FROM [ChannelUser] AS [CS]
                                                            WHERE ([CS].[ChannelId] = [CH].[Id]) AND ([CS].[Follow] = 1) AND ([CS].[Accepted] = 1) AND ([CS].[Visible] = 1)
                                                            )) > 100) AND ([CH].[OwnerId] = @OwnerId) AND ([CH].[Visible] = 1)", new { OwnerId = id });



                      if (fiftyFollowers > 0)
                          fiftyFollowers = fiftyFollowers * 50;
                      if (hundredFollowers >0)
                          hundredFollowers = hundredFollowers * 100;
 
                    userBag.PointsForChannel = pointsChn;
                    userBag.PointsForComment = pointsCmt;
                    userBag.PontsForTopic = pointsTpc;
                    userBag.Interaction = pointsTpc + pointsCmt + pointsChn + fiftyFollowers + hundredFollowers + 100; //100 = pq o usuário se cadastrou no nimbus

                    //throw http exception
                    if (userBag == null)
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                            "this item does not exist"));
                    }

                    user.Password = "";
                    return userBag;
                }            
        }
                                        
        /// <summary>
        /// Método que retorna os usuarios existentes excluindo os que já sao moderadores do canal
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public SuggestionWrapper SearchNewModerador(int id, string q)
        {
            //List<Model.ORM.User> users = new List<Model.ORM.User>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
                using (var db = DatabaseFactory.OpenDbConnection())
                {

                   
                    
                   var query = db.Query<Model.ORM.User>(
                   #region Queryzinha
@"
SELECT [tUser].[Id], [tUser].[FirstName], [tUser].[LastName], [tUser].[AvatarUrl]
FROM [ChannelUser]
INNER JOIN [Role] ON [ChannelUser].[ChannelId] = [Role].[ChannelId] AND [ChannelUser].[UserId] <> [Role].[UserId] AND [Role].[Accepted] = 1
OUTER APPLY (

    SELECT TOP (1) 1 AS [test], [User].[Id], [User].[Occupation], [User].[Interest], [User].[FirstName], [User].[LastName], [User].[AvatarUrl]
    FROM [User]
    WHERE ([User].[Id] = [ChannelUser].[UserId]) AND 
			(([User].[FirstName] LIKE @strQuery) OR 
			([User].[LastName] LIKE @strQuery) OR 
			([User].[Occupation] LIKE @strQuery) OR 
			([User].[Interest] LIKE @strQuery))
    ) AS [tUser]
	
WHERE ([tUser].[test] IS NOT NULL) AND 
	(((NOT ([Role].[IsOwner] = 1)) AND 
	(NOT ([Role].[ChannelMagager] = 1)) AND 
	(NOT ([Role].[MessageManager] = 1)) AND 
	(NOT ([Role].[ModeratorManager] = 1)) AND 
	(NOT ([Role].[TopicManager] = 1)) AND 
	(NOT ([Role].[UserManager] = 1))) OR 
	([Role].[UserId] <> [ChannelUser].[UserId])) AND 
	([ChannelUser].[Visible] = 1) AND 
	([ChannelUser].[ChannelId] = @chnIdParam)
",
                   #endregion
                   new { chnIdParam = id, strQuery = "%" + q + "%" });

                   if (query.Count() > 0)
                    {
                        var resp = query.Select(x => new UserAutoCompleteResponse
                        {
                            value = x.FirstName + " " + x.LastName,
                            data = new UserAutoCompleteResponse.UserData
                            {
                                Id = x.Id,
                                AvatarUrl = x.AvatarUrl.Replace("/av130x130", "/av35x35")
                            }
                        });

                        return new SuggestionWrapper { suggestions = resp.ToList() };
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + q + "'"));
                    }
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Query em branco"));
            }

        }

        #endregion

        /// <summary>
        /// editar informações perfil
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>bool</returns>        
        [AllowAnonymous]
        [HttpPost]
        public User EditProfile(User user)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    User currentUser = db.SelectParam<User>(us => us.Id == NimbusUser.UserId).FirstOrDefault();
                    if (currentUser != null)
                    {
                        currentUser.FirstName = user.FirstName;
                        currentUser.LastName = user.LastName;
                        currentUser.City = user.City;
                        currentUser.State = user.State;
                        currentUser.Country = user.Country;
                        currentUser.Interest = user.Interest;
                        currentUser.Occupation = user.Occupation;
                        currentUser.Experience = user.Experience;
                        currentUser.About = user.About;
                        currentUser.BirthDate = currentUser.BirthDate;

                        db.Save(currentUser);
                        user.Id = currentUser.Id;
                    }
                }
                return user;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public User CreateProfile(User user)
        {
            try
            {
                string passwordHash = new Security.PlaintextPassword(user.Password).Hash;
                user.Password = passwordHash;

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    db.Insert(user);
                    user.Id = (int)db.GetLastInsertId();
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// Método para completar as informações do usuário para que ele possa comprar/vender itens
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>

        [HttpPost]
        [AllowAnonymous]
        public UserInfoPayment CreateInfoUser(UserInfoPayment user)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    db.Insert(user);
                    return user;
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        #region password
        [HttpPost]
        [AllowAnonymous]
        public int ResetPassword(ResetPasswordModel reset)
        {
            int idUser = 0;
            if (reset.Token != null)
            {
                Guid tokenGuid;
                NSCInfo info;
                bool valid = false;
                if (reset.Token == "logado")
                {
                    valid = true;
                    idUser = NimbusUser.UserId;
                }
                else
                {
                    valid = Token.VerifyToken(reset.Token, out tokenGuid, out info);
                    idUser = info.UserId * -1; //vem negativo * -1 = positivo
                }

                if (valid == true)
                {                   
                    if (reset.NewPassord == reset.ConfirmPassword)
                    {
                        using (var db = DatabaseFactory.OpenDbConnection())
                        {                           
                            User user = db.Where<User>(u => u.Id == idUser).FirstOrDefault();
                            if (user != null)
                            {
                                string passwordHash = new Security.PlaintextPassword(reset.NewPassord).Hash;
                                user.Password = passwordHash;

                                db.Update<User>(user, u => u.Id == idUser);
                            }
                        }
                    }
                }
                else
                {//token invalido
                }
              

            }
            return idUser;
        }

        [HttpGet]
        [AllowAnonymous]
        public bool checkValidEmail(string email)
        {
            //verificar se email existe
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                bool exist = db.SelectParam<User>(u => u.Email == email).Exists(u  => u.Email == email);
                if (exist == true)
                    return sendTokenResetPassword(email);
                else 
                    return false;
            }            
        }

        [HttpGet]
        [AllowAnonymous]
        public bool sendTokenResetPassword(string userEmail = null)
        {
            MandrillApi apiMandrill = new MandrillApi(NimbusConfig.MandrillToken);
            EmailMessage mensagem = new EmailMessage();
            mensagem.from_email = "ResetPassword@portalnimbus.com.br";
            mensagem.from_name = "Portal Nimbus";
            mensagem.subject = "Redefinição de senha";
            if (userEmail == null)
                userEmail = NimbusUser.Email;
            List<EmailAddress> address = new List<EmailAddress> ();
            address.Add(new EmailAddress(userEmail));
            mensagem.to = address;
            
            NSCInfo infoToken = new NSCInfo 
            {
                TokenGenerationDate = DateTime.Now,
                TokenExpirationDate =  DateTime.Now.AddDays(1),
                UserId = -1 * NimbusUser.UserId
            };

            Guid token;
            string tokeString = Token.GenerateToken(infoToken, out token);

            mensagem.html=" <body style=\"width:80p0x; background-color:#ffffff;\">" +
                                "<div>"+
                                    "<img src=\"https://***REMOVED***/imgnimbus/topBar.png\" style=\"width:800px;\" />"+
                                "</div>"+
                                "<div style=\"margin:30px 10% 40px; height:250px;\">" +
                                     "Olá " + NimbusUser.FirstName + " " + NimbusUser.LastName + ", <br/>"+
                                     "para redefinir sua senha, acesse: " +
                                     "<a href=\"http://www.portalnimbus.com.br/resetpassword?reset=" + Uri.EscapeDataString(tokeString) + "\">Redefinir senha</a><br/>" +
                                     "*Esse link é válido no período de 1 (um) dia. <br/><br/>" +

                                "</div>"+
                                "<div>"+
                                    "<img src=\"https://***REMOVED***/imgnimbus/bottomBar.png\" style=\"width:800px;\" />" +
                                "</div>"+
                            "</body>";
                            

            var result = apiMandrill.SendMessage(mensagem);
            if (result[0].Status == EmailResultStatus.Sent)
            {
                return true;
            }
            else
                return false;
            
        }

        #endregion

        [HttpPost]
        public bool DeleteUserAccount() 
        {
            bool delete = false;

            using(var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted)) 
                {
                    try
                    {
                        #region//remover os canais + tópicos
                        IEnumerable<int> idsChannelsOWner = db.Where<Channel>(ch => ch.OwnerId == NimbusUser.UserId && ch.Visible == true).Select(c => c.Id).ToList();
                        if (idsChannelsOWner.Count() > 0)
                        {
                            ChannelController apiChn = ClonedContextInstance<ChannelController>();
                            foreach (int item in idsChannelsOWner)
                            {
                                apiChn.DeleteChannel(item);
                            }
                        }
                        #endregion

                        #region//remover os votos dados aos canais e canais seguidos
                        db.SqlScalar<int>(@"UPDATE [ChannelUser]
                                                  SET [ChannelUser].[Accepted] = 0, [ChannelUser].[Follow] = 0,[ChannelUser].[Vote] = 0,
                                                      [ChannelUser].[Visible] = 0
                                                  WHERE [ChannelUser].[UserId] = @userId AND 
                                                  ([ChannelUser].[Follow] = 1  OR [ChannelUser].[Accepted] = 1 OR [ChannelUser].[Vote] = 1 OR
                                                   [ChannelUser].[Visible] = 1)", new { userId = NimbusUser.UserId });
                        #endregion

                        #region//remover da tabela moderador                      
                        db.SqlScalar<int>(@"UPDATE [Role]
                                                  SET [Role].[Accepted] = 0,[Role].[ChannelMagager] = 0, [Role].[IsOwner] = 0,
                                                      [Role].[MessageManager] = 0,[Role].[ModeratorManager] = 0,[Role].[Paid] = 0,
                                                      [Role].[TopicManager] = 0,[Role].[UserManager] = 0
                                                  WHERE [Role].[UserId] = @userId AND 

                                                  ([Role].[Accepted] = 1  OR [Role].[ChannelMagager] = 1 OR [Role].[IsOwner] = 1 OR
                                                   [Role].[MessageManager] = 1 OR [Role].[ModeratorManager] = 1 OR [Role].[Paid]= 1 OR
                                                   [Role].[TopicManager] = 1 OR [Role].[UserManager] = 1)", new { userId = NimbusUser.UserId });                        
                        #endregion

                        #region//remover comentários
                        IEnumerable<Comment> comments = db.Where<Comment>(c => c.Visible == true && c.UserId == NimbusUser.UserId).ToList();
                        if (comments.Count() > 0) 
                        {
                            CommentController apiCmt = ClonedContextInstance<CommentController>();
                            foreach (var item in comments)
                            {
                                apiCmt.DeleteComment(item);  
                            }
                        }
                        #endregion

                        #region //remover da tabela de topicos favoritos    
                        db.SqlScalar<int>(@"UPDATE [UserTopicFavorite]
                                                  SET [UserTopicFavorite].[Visible] = 0
                                                  WHERE [UserTopicFavorite].[UserId] = @userId AND [UserTopicFavorite].[Visible] = 1", new { userId = NimbusUser.UserId });
                        #endregion

                        #region//removre os likes dados aos tópicos
                        db.SqlScalar<int>(@"UPDATE [UserLikeTopic]
                                                  SET [UserLikeTopic].[Visible] = 0
                                                  WHERE [UserLikeTopic].[UserId] = @userId AND [UserLikeTopic].[Visible]= 1", new { userId = NimbusUser.UserId });
                        #endregion

                        #region//remover usuário
                        var userCurrent = db.Where<User>(u => u.Id == NimbusUser.UserId).FirstOrDefault();
                        userCurrent.Password = null;

                        db.Update<User>(userCurrent, u => u.Id == NimbusUser.UserId);
                        #endregion

                        trans.Commit();
                        delete = true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw ;  
                    }
                }
            }

            return delete;
        }

        [HttpGet]
        public ParticipationModel CountToParticipation(int id = 0)
        {
            ParticipationModel model = new ParticipationModel();
            model.CountChannelManager = 0; model.CountChannelOwner = 0; model.CountComments = 0; model.CountTopics = 0;
            model.HundredFollowers = 0; model.FiftyFollowers = 0;

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                if (id == 0)
                    id = NimbusUser.UserId;

                model.CountChannelOwner = db.Where<Role>(r => r.UserId == id && r.IsOwner == true).Count();

                model.CountChannelManager = db.Where<Role>(r => r.IsOwner == false && r.UserId == id && r.Accepted == true)
                                              .Select(c => c.ChannelMagager == true || c.MessageManager == true ||
                                                           c.ModeratorManager == true || c.TopicManager == true || c.UserManager == true)
                                                      .Count();

                model.CountTopics = db.Where<Topic>(t => t.Visibility == true && t.AuthorId == id).Count();

                model.CountComments = db.Where<Comment>(c => c.UserId == id && c.Visible == true).Where(c => c != null).Count();

                model.FiftyFollowers = db.SqlScalar<int>(@"SELECT COUNT(*) AS [value]
                                                            FROM [Channel] AS [CH]
                                                        WHERE (((
                                                            SELECT COUNT(*)
                                                            FROM [ChannelUser] AS [CS]
                                                            WHERE ([CS].[ChannelId] = [CH].[Id]) AND ([CS].[Follow] = 1) AND ([CS].[Accepted] = 1) AND ([CS].[Visible] = 1)
                                                            )) > 50) AND ([CH].[OwnerId] = @OwnerId) AND ([CH].[Visible] = 1)", new { OwnerId = id });

                model.HundredFollowers = db.SqlScalar<int>(@"SELECT COUNT(*) AS [value]
                                                            FROM [Channel] AS [CH]
                                                        WHERE (((
                                                            SELECT COUNT(*)
                                                            FROM [ChannelUser] AS [CS]
                                                            WHERE ([CS].[ChannelId] = [CH].[Id]) AND ([CS].[Follow] = 1) AND ([CS].[Accepted] = 1) AND ([CS].[Visible] = 1)
                                                            )) > 100) AND ([CH].[OwnerId] = @OwnerId) AND ([CH].[Visible] = 1)", new { OwnerId = id });

            }
            return model;
        }

        [HttpPost]
        public bool SendFeedback(int type, string message)
        {
            MandrillApi mandril = new MandrillApi(NimbusConfig.MandrillToken);
            EmailMessage mensagem = new EmailMessage();
            List<EmailAddress> address = new List<EmailAddress>();


            try
            {
                if (type == 1)
                    mensagem.subject = "[feedbackPositivo]";
                else if (type == 0)
                    mensagem.subject = "[feedbackNegativo]";

                string nameUser = NimbusUser.FirstName != null? NimbusUser.FirstName : "nulo";
                nameUser = nameUser + " " + NimbusUser.LastName != null? NimbusUser.LastName : "nulo";
                mensagem.from_email = "contato@portalnimbus.com.br";
                mensagem.from_name = "Feedback";
                mensagem.text = "Tipo: " + mensagem.subject.Replace("feedback", "") + " \n" +
                                "Usuário: " + nameUser +"\n"+ 
                                "Mensagem: " + message + "\n\n\n\n";

                address.Add(new EmailAddress("***REMOVED***"));
                mensagem.to = address;

                var result = mandril.SendMessage(mensagem);
                if (result[0].Status == EmailResultStatus.Sent)
                {
                    return true;
                }
                else
                {

                    return false; //tem q arrumar
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
