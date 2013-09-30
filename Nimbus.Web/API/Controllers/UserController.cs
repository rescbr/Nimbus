using Nimbus.Web.API.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;

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
        public User showProfile()
        {
            return showProfile(NimbusUser.UserId);
        }

        ///<sumary>
        ///método padrão de exibir perfil
        /// </sumary>
        //[Authorize]
        [HttpGet]
        public User showProfile(int id)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var user = db.SelectParam<User>(usr => usr.Id == id).FirstOrDefault();

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
        [HttpPost]
        public User editProfile(User user, int id)
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
                throw ex;
            }
        }
         

        [HttpPut]
        public User createProfile(User user)
        {
            bool login = false;
            try
            {
                string passwordHash = new Security.PlaintextPassword(user.Password).Hash;
                user.Password = passwordHash;

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    db.Insert(user);
                    login = true;
                }

                return user;
            }
            catch (Exception ex)
            {
                login = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

        }


    }



}
