using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API.Controllers;
using Nimbus.Web.API.Models.User;

namespace Nimbus.Web.UnitTest
{
    [TestClass]
    public class UserAPIControllerTest
    {
        [TestMethod]
        public void User_RegisterTest()
        {
            UserAPIController userAPI = new UserAPIController();
            CreateUserAPIModel model = new CreateUserAPIModel();

            model.BirthDate = DateTime.Now;
            model.City = "Bauru";
            model.ConfirmPassword = "123123";
            model.Country = "Brazil";
            model.Email = "contato@portalnimbus.com.br";
            model.FirstName = "Nimbus";
            model.LastName = "Portal";
            model.Password = "123123";
            model.State = "SP";

            userAPI.createProfile(model);
        }
    }
}
