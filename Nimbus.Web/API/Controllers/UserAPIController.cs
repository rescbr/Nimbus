﻿using Nimbus.Web.API.Models.User;
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
    public class UserAPIController : NimbusApiController
    {

        #region métodos de exibir informações do perfil

        ///<summary>
        ///exibir informações perfil usuário logado
        ///</summary>
        [Authorize]
        [HttpGet]
        public ShowProfile showProfile()
        {
            return showProfile(NimbusUser.UserId);
        }

        ///<sumary>
        ///método padrão de exibir perfil
        /// </sumary>
        [Authorize]
        [HttpGet]
        public ShowProfile showProfile(int idUser)
        {
            ShowProfile profile = new ShowProfile();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var user = db.SelectParam<User>(usr => usr.Id == idUser).FirstOrDefault();
                    profile.user_ID = user.Id;
                    profile.UrlImg = user.AvatarUrl;
                    profile.Name = user.FirstName + " " + user.LastName;
                    profile.BirthDate = user.BirthDate;
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
        [Authorize]
        [HttpPost]
        public bool editProfile(EditUserAPIModel profile)
        {
            bool success = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    User user = new User()
                    {
                        Occupation = profile.Occupation,
                        Interest = profile.Interest,
                        Experience = profile.Experience,
                        AvatarUrl = profile.UrlImg,
                        About = profile.About,
                        City = profile.City,
                        State = profile.State,
                        Country = profile.Country,
                        BirthDate = profile.BirthDate
                    };

                    db.Update<User>(user, usr => usr.Id == NimbusUser.UserId );
                    db.Save(user);
                    success = true;
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return success; 
        }
         

        [HttpPut]
        public bool createProfile(CreateUserAPIModel newUser)
        {
            bool login = false;
            try
            {
                string passwordHash = new Security.PlaintextPassword(newUser.Password).Hash;

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    User user = new User()
                    {
                        
                        FirstName = newUser.FirstName,
                        LastName = newUser.LastName,
                        BirthDate= newUser.BirthDate,
                        City = newUser.City,
                        Country = newUser.Country,
                        Email = newUser.Email,
                        State = newUser.State,
                        Password = passwordHash
                        
                    };

                    db.Insert(user);
                    login = true;
                }
            }
            catch (Exception ex)
            {
                login = false;
                throw ex;
            }

            return login;
        }


    }



}
