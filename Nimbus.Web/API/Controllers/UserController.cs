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
            try
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
                    userBag.Interaction = 0;//TODO: arrumar p valor certo - pensar nas regras

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
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Search para Usuarios
        /// </summary>
        /// <param name="q">Query de Pesquisa</param>
        /// <returns></returns>
        [HttpGet]
        public List<User> SearchUser(string q)
        {
            List<User> users = new List<User>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
                try
                {
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        users = db.SelectParam<User>(usr => (usr.FirstName.Contains(q) ||
                                                                usr.LastName.Contains(q) ||
                                                                usr.Occupation.Contains(q) ||
                                                                usr.Interest.Contains(q)));


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
            return users;
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
SELECT [tUser].[Id], [tUser].[Email], [tUser].[Password], [tUser].[TOTPKey], [tUser].[BirthDate], [tUser].[Occupation], [tUser].[Interest], [tUser].[Experience], [tUser].[FirstName], [tUser].[LastName], [tUser].[AvatarUrl], [tUser].[About], [tUser].[City], [tUser].[State], [tUser].[Country]
FROM [ChannelUser]
INNER JOIN [Role] ON [ChannelUser].[ChannelId] = [Role].[ChannelId]
OUTER APPLY (

    SELECT TOP (1) 1 AS [test], [User].[Id], [User].[Email], [User].[Password], [User].[TOTPKey], [User].[BirthDate], [User].[Occupation], [User].[Interest], [User].[Experience], [User].[FirstName], [User].[LastName], [User].[AvatarUrl], [User].[About], [User].[City], [User].[State], [User].[Country]
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
                        currentUser.FirstName = HttpUtility.HtmlEncode(user.FirstName);
                        currentUser.LastName = HttpUtility.HtmlEncode(user.LastName);
                        currentUser.City = HttpUtility.HtmlEncode(user.City);
                        currentUser.State = HttpUtility.HtmlEncode(user.State);
                        currentUser.Country = HttpUtility.HtmlEncode(user.Country);
                        currentUser.Interest = HttpUtility.HtmlEncode(user.Interest);
                        currentUser.Occupation = HttpUtility.HtmlEncode(user.Occupation);
                        currentUser.Experience = HttpUtility.HtmlEncode(user.Experience);
                        currentUser.About = HttpUtility.HtmlEncode(user.About);
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
    }



}
