using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;
using Nimbus.DB.Bags;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Usuários.
    /// </summary>
    public class UserController : NimbusApiController
    {

        #region métodos de exibir informações do perfil

        ///<summary>
        ///exibir informações perfil usuário logado
        ///</summary>
        [Authorize]
        [HttpGet]
        public UserBag showProfile()
        {
            return showProfile(NimbusUser.UserId);
        }

        ///<sumary>
        ///método padrão de exibir perfil
        /// </sumary>
        //[Authorize]
        [HttpGet]
        public UserBag showProfile(int id)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var user = db.SelectParam<UserBag>(usr => usr.Id == id).FirstOrDefault();                    
                    user.Age = (int)Math.Floor((DateTime.Now.Subtract(user.BirthDate).Days)/365.25);
                    user.Interaction = 0; //TODO: arrumar p valor certo - pensar nas regras

                    //throw http exception
                    if (user == null)
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, 
                            "this item does not exist"));
                    }

                    user.Password = "";
                    return user;
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
        [Authorize]
        [HttpPut]
        public User EditProfile(User user, int id)
        {
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    db.Update<User>(user, usr => usr.Id == id );
                    db.Save(user);
                }

                return user;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }
         

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
        [Authorize]
        [HttpPost]
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
