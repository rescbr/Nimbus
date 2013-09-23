using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API;
using ServiceStack.OrmLite;

namespace Nimbus.Web.UnitTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class NimbusTest
    {
        public static NimbusApiController controller = new NimbusApiController();

        /// <summary>
        /// Create the Tables
        /// </summary>
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            controller.DatabaseFactory = new OrmLiteConnectionFactory
                    ("Data Source=.\\SQLEXPRESS;Initial Catalog=Nimbus_Test;Integrated Security=True;MultipleActiveResultSets=True",
                    SqlServerDialect.Provider);

            using (var db = controller.DatabaseFactory.OpenDbConnection())
            {
                db.DropAndCreateTables(new Type[]{
                    typeof(Nimbus.DB.Category),
                    typeof(Nimbus.DB.Ad), 
                    typeof(Nimbus.DB.Organization),
                    typeof(Nimbus.DB.User),
                    typeof(Nimbus.DB.UserReported),
                    typeof(Nimbus.DB.Channel),
                    typeof(Nimbus.DB.ChannelReported),
                    typeof(Nimbus.DB.ChannelUser),
                    typeof(Nimbus.DB.OrganizationUser),
                    typeof(Nimbus.DB.Topic),
                    typeof(Nimbus.DB.TopicReported),
                    typeof(Nimbus.DB.Comment),
                    typeof(Nimbus.DB.CommentReported),
                    typeof(Nimbus.DB.Message),
                    typeof(Nimbus.DB.UserTopicFavorite)
                });
            }
        }

        /// <summary>
        /// Drop the Tables. This is made to clean all the database, including auto-generated ids.
        /// </summary>
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            using (var db = controller.DatabaseFactory.OpenDbConnection())
            {
                db.DropTables(new Type[]{
                    typeof(Nimbus.DB.UserTopicFavorite),
                    typeof(Nimbus.DB.Message),
                    typeof(Nimbus.DB.CommentReported),
                    typeof(Nimbus.DB.Comment),
                    typeof(Nimbus.DB.TopicReported),
                    typeof(Nimbus.DB.Topic),
                    typeof(Nimbus.DB.OrganizationUser),
                    typeof(Nimbus.DB.ChannelUser),
                    typeof(Nimbus.DB.ChannelReported),
                    typeof(Nimbus.DB.Channel),
                    typeof(Nimbus.DB.UserReported),
                    typeof(Nimbus.DB.User),
                    typeof(Nimbus.DB.Organization),
                    typeof(Nimbus.DB.Ad), 
                    typeof(Nimbus.DB.Category)
                });
            }
        }
    }
}
