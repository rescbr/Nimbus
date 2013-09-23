using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API.Controllers;
using Nimbus.Web.API.Models.Channel;

namespace Nimbus.Web.UnitTest.API.Controllers
{
    [TestClass]
    public class ChannelAPIControllerTest
    {
        ChannelAPIController controller = new ChannelAPIController();

        /// <summary>
        /// Init the controller with a Database Factory that points to a Test Database
        /// </summary>
        [TestInitialize]
        public void UserAPIControllerTest_Initialize()
        {
            controller.DatabaseFactory = NimbusTest.controller.DatabaseFactory;
        }

        [TestMethod]
        public void Channel_New_Successful()
        {
            /*
            NewChannelAPI model = new NewChannelAPI();

            model.Visible = true;

            controller.newChannel(model);
            */
        }
    }
}
