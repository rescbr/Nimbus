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
using Nimbus.DB.ORM;

namespace Nimbus.Web.UnitTest
{
    [TestClass]
    public class UserControllerTest 
    {
        UserController controller = new UserController();

        /// <summary>
        /// Init the controller with a Database Factory that points to a Test Database
        /// </summary>
        [TestInitialize]
        public void UserControllerTest_Initialize()
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
            User model = new User();

            model.BirthDate = DateTime.Now;
            model.City = "Bauru";
            model.Country = "Brazil";
            model.Email = "contato@portalnimbus.com.br";
            model.FirstName = "Nimbus";
            model.LastName = "Portal";
            model.Password = "123123";
            model.State = "SP";

            var response = controller.createProfile(model);

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
         
            User model = new User();

            model.City = "Bauru";
            model.Country = "Brazil";
            model.State = "SP";
            model.About = "I'm just a nice geek guy";
            model.Experience = "Programmer and Project Manager";
            model.Interest = "Mobile and Java";
            model.Occupation = "Programmer on Nimbus Corp";
            model.AvatarUrl = "http://portalnimbus.com.br/images/logo_hotsite-01.png";
            model.BirthDate = DateTime.Now;

            var response = controller.editProfile(model, 1);

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

            var model = controller.showProfile();

        }

        /// <summary>
        /// Test a Successful user show
        /// </summary>
        [TestMethod]
        public void User_ShowWithId_Successful()
        {
            User_Register_Successful();

            var model = controller.showProfile(1);
            
        }

        #endregion


    }
}
