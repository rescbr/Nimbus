using Nimbus.Web.API;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using WebApiContrib.Formatting.Html;

namespace Nimbus.Web.Website.Controllers
{
    public class NewAccountController : NimbusApiController
    {
        public View Get() {
            return new View("NewAccount", new NewAccountModel());
        }

        public View Post(NewAccountModel newAccount)
        {
            if (ModelState.IsValid)
            {
                //TODO: verificar data de nascimento

                //verifica se a senha está correta
                if (newAccount.Password == newAccount.ConfirmPassword)
                {             
       
                    /* Quando for usar uma API internamente, faça o clone antes. */
                    var userapi = ClonedContextInstance<API.Controllers.UserController>();
                    var newUser = new Nimbus.DB.ORM.User()
                    {
                        City = newAccount.City,
                        Country = newAccount.Country,
                        Email = newAccount.Email,
                        FirstName = newAccount.FirstName,
                        LastName = newAccount.LastName,
                        Password = newAccount.Password,
                        State = newAccount.State

                        ,BirthDate = DateTime.Now
                    };
                    userapi.createProfile(newUser);
                    
                    //aqui deveria redirecionar
                    return new View("NewAccount", new NewAccountModel());
                }
                else //senha confirmada está incorreta.
                {
                    //aqui deveria passar mensagem de erro
                }
            }

            return new View("NewAccount", null); //null => model de errro
        }
    }
}