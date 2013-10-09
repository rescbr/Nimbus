using Nimbus.DB.ORM;
using Nimbus.Plumbing;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web
{
    public class DatabaseStartup
    {
        public static void CreateDatabaseIfNotThere()
        {
            var dbFactory = new OrmLiteConnectionFactory
                    (NimbusAppBus.Instance.Settings.DatabaseConnectionString,
                    SqlServerDialect.Provider);
            using (var db = dbFactory.OpenDbConnection())
            {
                if (!db.TableExists("Organization"))
                {
                    //criar tabelas
                    db.CreateTables(false, new Type[]{
                        typeof(Category),
                        typeof(Ad), 
                        typeof(Organization),
                        typeof(User),
                        typeof(UserReported),
                        typeof(Channel),
                        typeof(ChannelReported),
                        typeof(ChannelUser),
                        typeof(Role),
                        typeof(OrganizationUser),
                        typeof(Topic),
                        typeof(TopicReported),
                        typeof(Comment),
                        typeof(CommentReported),
                        typeof(Message),
                        typeof(UserTopicFavorite),
                        
                    });

                    var nimbusorg = new Nimbus.DB.ORM.Organization()
                    {
                        Cname = "dev.portalnimbus.com.br", 
                        Id = 1,
                        Name = "Portal Nimbus"
                    };
                    db.Save(nimbusorg);

                    var sysuser = new DB.ORM.User()
                    {
                        Id = 1,
                        FirstName = "System",
                        LastName = "Administrator",
                        Email = "sysop@portalnimbus.com.br",
                        Password = new Security.PlaintextPassword("local@adm1").Hash,
                        TOTPKey = Security.OneTimePassword.GenerateSecret(),
                        BirthDate = DateTime.Now
                    };
                    db.Save(sysuser);

                    var orguser = new DB.ORM.OrganizationUser()
                    {
                        OrganizationId = 1,
                        UserId = 1
                    };
                    db.Save(orguser);
                }
            }
        }
    }
}