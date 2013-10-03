using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API.Controllers;
using Nimbus.DB.ORM;
using Nimbus.Plumbing;

namespace Nimbus.Web.UnitTest.API.Controllers
{
    [TestClass]
    public class ChannelControllerTest
    {
        ChannelController controller = new ChannelController();

        /// <summary>
        /// Init the controller with a Database Factory that points to a Test Database
        /// </summary>
        [TestInitialize]
        public void ChannelControllerTest_Initialize()
        {
            controller.DatabaseFactory = NimbusTest.controller.DatabaseFactory;
        }

        [TestMethod]
        public void Channel_New_Successful()
        {
            UserControllerTest userTest = new UserControllerTest();
            userTest.User_Register_Successful();

            NimbusUser nimbusUser = new NimbusUser();
            nimbusUser.UserId = 1;
            controller.NimbusUser = nimbusUser;

            Channel model = new Channel();

            model.CategoryId = 0;
            model.OrganizationId = 0;
            
            model.Visible = true;
            model.Description = "Canal HAHAHA";
            model.Followers = 0;
            model.ImgUrl = "Bazinga!";
            model.IsCourse = false;
            model.IsPrivate = false;
            model.Name = "Canal de Teste";
            model.OpenToComments = true;

            model.Price = 0;

            controller.NewChannel(model);
            
        }
    }
}
