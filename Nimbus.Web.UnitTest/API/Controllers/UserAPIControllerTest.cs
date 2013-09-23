using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API.Controllers;
using Nimbus.Web.API.Models.User;
using Nimbus.Plumbing;
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

        /// <summary>
        /// Init the controller with a Database Factory that points to a Test Database
        /// </summary>
        [TestInitialize]
        public void UserAPIControllerTest_Initialize()
        {
            controller.DatabaseFactory = NimbusTest.controller.DatabaseFactory;
        }

        #region Successful Test Operations

        /// <summary>
        /// Test a Successful user registration
        /// </summary>
        [TestMethod]
        public void User_Register_Successful()
        {
            CreateUserAPIModel model = new CreateUserAPIModel();

            model.BirthDate = DateTime.Now;
            model.City = "Bauru";
            model.Country = "Brazil";
            model.Email = "contato@portalnimbus.com.br";
            model.FirstName = "Nimbus";
            model.LastName = "Portal";
            model.Password = "123123";
            model.ConfirmPassword = "123123";
            model.State = "SP";

            var response = controller.createProfile(model);

            Assert.AreEqual(true, response);
        }
        
        /// <summary>
        /// Test a Successful user edit
        /// </summary>
        [TestMethod]
        public void User_Edit_Successful()
        {
            User_Register_Successful();

            NimbusUser nimbusUser = new NimbusUser();
            nimbusUser.UserId = 1;
            controller.NimbusUser = nimbusUser;
         
            EditUserAPIModel model = new EditUserAPIModel();

            model.City = "Bauru";
            model.Country = "Brazil";
            model.State = "SP";
            model.About = "I'm just a nice geek guy";
            model.Experience = "Programmer and Project Manager";
            model.Interest = "Mobile and Java";
            model.Occupation = "Programmer on Nimbus Corp";
            model.UrlImg = "http://portalnimbus.com.br/images/logo_hotsite-01.png";
            model.BirthDate = DateTime.Now;

            var response = controller.editProfile(model);

            Assert.AreEqual(true, response);
        }

        /// <summary>
        /// Test a Successful user show
        /// </summary>
        [TestMethod]
        public void User_Show_Successful()
        {
            User_Register_Successful();

            NimbusUser nimbusUser = new NimbusUser();
            nimbusUser.UserId = 1;
            controller.NimbusUser = nimbusUser;

            ShowProfile model = controller.showProfile();

            Assert.AreEqual("Bauru", model.City);
        }

        /// <summary>
        /// Test a Successful user show
        /// </summary>
        [TestMethod]
        public void User_ShowWithId_Successful()
        {
            User_Register_Successful();

            ShowProfile model = controller.showProfile(1);
            
            Assert.AreEqual("Bauru", model.City);
        }

        #endregion


    }
}
