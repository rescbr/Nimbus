using Nimbus.Web.API.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Usuários.
    /// </summary>
    public class UserAPIController : NimbusApiController
    {

        #region métodos de exibir informações do perfil
        ///<summary>
        ///exibir informações perfil visitado
        /// </summary>
        public ShowProfile friendProfile(int idUser)
        {

            return showProfile(idUser);
        }

        ///<summary>
        ///exibir informações perfil usuário logado
        ///</summary>
        public ShowProfile userProfile()
        {
            return showProfile(0);
        }

        ///<sumary>
        ///método padrão de exibir perfil
        /// </sumary>
        public ShowProfile showProfile(int idUser)
        {
            ShowProfile profile = new ShowProfile();
            try
            {                
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var user = db.SelectParam<Nimbus.DB.User>(usr => usr.Id == idUser).FirstOrDefault();
                    profile.user_ID = user.Id;
                    profile.UrlImg = user.AvatarUrl;
                    profile.Name = user.Name;
                    profile.BirthDate = (DateTime.Now - user.BirthDate).ToString();
                    profile.City = user.City;
                    profile.State = user.State;
                    profile.Country = user.Country;
                    profile.Occupation = user.Occupation;
                    profile.Interest = user.Interest;
                    profile.Experience = user.Experience;
                    profile.About = user.About;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return profile;
        }

        #endregion

        /// <summary>
        /// editar informações perfil
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>bool</returns>
        public bool editProfile(EditUserAPIModel profile)
        {
            bool success = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    var user = db.Update<Nimbus.DB.User>(new { Occupation = profile.Occupation, 
                                                               Interest = profile.Interest,
                                                               Experience = profile.Experience,
                                                               AvatarUrl = profile.UrlImg,
                                                               About = profile.About,
                                                               City = profile.City,
                                                               State = profile.State,
                                                               Country = profile.Country 
                                                             }, usr => usr.Id == 0 );
                }
                success = true;
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return success; 
        }
          
        //TO DO
        //reportar usuários (terminar a parte que salva no banco
        public bool reportUser(ReportUserAPIModel user)
        {
            bool success = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                //db.Insert(new Report{userReported_ID = user.userReported_ID, Justification = user.Justification, user_ID = 0});
                }
                success = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return success;
        }


        //deletar conta
        



    }



}
