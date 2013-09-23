using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API.Controllers;
using Nimbus.Web.API.Models.User;
using Nimbus.Plumbing;
using Nimbus.Plumbing.Interface;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.Http.Hosting;
using System.Net.Http;
using System.Net;
using System.Threading;
using ServiceStack.OrmLite;

namespace Nimbus.Web.UnitTest
{
    [TestClass]
    public class UserAPIControllerTest 
    {
        UserAPIController controller = new UserAPIController();

        [TestInitialize]
        public void UserAPIControllerTest_Initialize()
        {
            controller.DatabaseFactory = NimbusTest.controller.DatabaseFactory;
        }

        [TestMethod]
        public void User_RegisterTest()
        {
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

            var response = controller.createProfile(model);

            Assert.AreEqual(response, true);

        }
        
    }
}
