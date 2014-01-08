using System;
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
                    userBag.State = user.State;
                    userBag.Age = (int)Math.Floor((DateTime.Now.Subtract(user.BirthDate).Days) / 365.25);


                    var roles = db.Where<Channel>(c => c.Visible == true && c.OrganizationId == NimbusOrganization.Id).Select(c => db.Where<Role>(rl => rl.UserId == id && rl.ChannelId == c.Id &&
                                                                                                                                  (rl.IsOwner == true || rl.ChannelMagager == true || rl.MessageManager == true
                                                                                                                                   || rl.ModeratorManager == true || rl.TopicManager == true
                                                                                                                                   || rl.UserManager == true)).FirstOrDefault()).Where(r => r != null);
                    int pointsChn = 0;
                    int pointsTpc = 0;
                    int pointsCmt = 0;
                    if (roles.Count() > 0)
                    {
                         pointsChn = ((roles.Select(c => c.UserId == id && c.IsOwner == true).Count() * 50)
                                       + (roles.Select(c => c.UserId == id && (c.ChannelMagager == true || c.MessageManager == true ||
                                                                               c.ModeratorManager == true || c.TopicManager == true || c.UserManager == true)).Count() * 25));


                         pointsTpc = roles.Where(r => r.UserId == id).Select(r => db.Where<Topic>(t => t.AuthorId == r.UserId &&
                                                                                                        t.ChannelId == r.ChannelId && t.Visibility == true))
                                                                        .FirstOrDefault().Count();

                        pointsCmt = roles.Where(r => r.UserId == id).Select(r => db.Where<Comment>(c => c.UserId == id && c.Visible == true && c.ChannelId == r.ChannelId))
                                                                        .Where(c => c != null).Count();
                    }
                     pointsChn = pointsChn * 30;
                     pointsCmt = pointsCmt * 1;
                     pointsTpc = pointsTpc * 50;

                    userBag.PointsForChannel = pointsChn;
                    userBag.PointsForComment = pointsCmt;
                    userBag.PontsForTopic = pointsTpc;
                    userBag.Interaction = pointsTpc + pointsCmt + pointsChn + 100; //100 = pq o usuário se cadastrou no nimbus

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
INNER JOIN [Role] ON [ChannelUser].[ChannelId] = [Role].[ChannelId]
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
                                AvatarUrl = x.AvatarUrl
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
                if (Token.VerifyToken(reset.Token, out tokenGuid, out info))
                {                   
                    if (reset.NewPassord == reset.ConfirmPassword)
                    {
                        using (var db = DatabaseFactory.OpenDbConnection())
                        {
                            idUser = info.UserId * -1; //vem negativo * -1 = positivo
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
        public bool sendTokenResetPassword()
        {
            MandrillApi apiMandrill = new MandrillApi("***REMOVED***");
            EmailMessage mensagem = new EmailMessage();
            mensagem.from_email = "resetPassword@portalnimbus.com.br";
            mensagem.from_name = "Portal Nimbus";
            mensagem.subject = "Redefinição de senha ";
            List<EmailAddress> address = new List<EmailAddress> ();
            address.Add(new EmailAddress("***REMOVED***"));
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
                                     "<a href=\"http://localhost:50011/resetpassword?reset=" + Uri.EscapeDataString(tokeString) + "\">Redefinir senha</a>" +
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
    }




}
