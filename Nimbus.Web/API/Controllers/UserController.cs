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

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Usuários.
    /// </summary>
    [NimbusAuthorize]
    public class UserController : NimbusApiController
    {

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
                    userBag.Age =(int)Math.Floor((DateTime.Now.Subtract(user.BirthDate).Days)/365.25);
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
                using(var db = DatabaseFactory.OpenDbConnection())
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
