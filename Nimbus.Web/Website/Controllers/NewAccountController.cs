﻿using Nimbus.Web.API;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;


namespace Nimbus.Web.Website.Controllers
{
    public class NewAccountController : NimbusWebController
    {
        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get() {
            return View("NewAccount", null);
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(NewAccountModel newAccount)
        {
            if (ModelState.IsValid)
            {
                //TODO: verificar data de nascimento

                //verifica se a senha está correta
                if (newAccount.Password == newAccount.ConfirmPassword)
                {             
       
                    /* Quando for usar uma API internamente, faça o clone antes. */
                    var userapi = ClonedContextInstance<API.Controllers.UserController>();
                    var login = new LoginController();
                    login.NimbusOrganization = this.NimbusOrganization;
                    login.Session = this.Session;
                    login.Response = this.Response;

                    var newUser = new Nimbus.Model.ORM.User()
                    {
                        City = newAccount.City,
                        Country = newAccount.Country,
                        Email = newAccount.Email,
                        FirstName = newAccount.FirstName,
                        LastName = newAccount.LastName,
                        Password = newAccount.Password,
                        State = newAccount.State,
                        AvatarUrl = "/images/av130x130/person_icon.png",
                        BirthDate = newAccount.BirthDate != null ? newAccount.BirthDate : DateTime.Now
                    };
                     userapi.CreateProfile(newUser);
                    //aqui deveria redirecionar
                     return login.Post(new LoginModel() { Email = newAccount.Email, Password = newAccount.Password, RedirectURL = "/" });
                    }
                else //senha confirmada está incorreta.
                {
                    //aqui deveria passar mensagem de erro
                }
            }

            return Redirect("/login"); //null => model de errro
        }
    }
}