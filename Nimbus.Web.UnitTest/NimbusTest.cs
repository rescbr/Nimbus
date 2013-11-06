using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nimbus.Web.API;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;

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
                db.DropAndCreateTables( new Type[]{
                    typeof(Category),
                    typeof(Ad), 
                    typeof(Organization),
                    typeof(User),
                    typeof(UserReported),
                    typeof(Channel),
                    typeof(ChannelReported),
                    typeof(ChannelUser),
                    typeof(OrganizationUser),
                    typeof(Topic),
                    typeof(TopicReported),
                    typeof(Comment),
                    typeof(CommentReported),
                    typeof(Message),
                    typeof(UserTopicFavorite)
                });
            }
        }

        /// <summary>
        /// Drop the Tables. This is made to clean all the database, including auto-generated ids.
        /// </summary>
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            /**/
            using (var db = controller.DatabaseFactory.OpenDbConnection())
            {
                db.DropTables(new Type[]{
                    typeof(UserTopicFavorite),
                    typeof(Message),
                    typeof(CommentReported),
                    typeof(Comment),
                    typeof(TopicReported),
                    typeof(Topic),
                    typeof(OrganizationUser),
                    typeof(ChannelUser),
                    typeof(ChannelReported),
                    typeof(Channel),
                    typeof(UserReported),
                    typeof(User),
                    typeof(Organization),
                    typeof(Ad),
                    typeof(Category)
                });
            }
            /**/
        }
    }
}
