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
                    (NimbusConfig.DatabaseConnection,
                    SqlServerDialect.Provider);
            using (var db = dbFactory.OpenDbConnection())
            {
                if (!db.TableExists("Organization"))
                {
                    using (var trans = db.OpenTransaction())
                    {
                        //criar tabelas
                        
                        db.CreateTable(false, typeof(Category));
                        db.CreateTable(false, typeof(Ad)); 
                        db.CreateTable(false, typeof(Organization));
                        db.CreateTable(false, typeof(User));
                        db.CreateTable(false, typeof(UserAds));
                        db.CreateTable(false, typeof(UserReported));
                        db.CreateTable(false, typeof(Channel));
                        db.CreateTable(false, typeof(ChannelReported));
                        db.CreateTable(false, typeof(ChannelUser));
                        db.CreateTable(false, typeof(OrganizationUser));
                        db.CreateTable(false, typeof(UserChannelReadLater));
                        db.CreateTable(false, typeof(Role));
                        db.CreateTable(false, typeof(OrganizationUser));
                       // db.CreateTable(false, typeof(RoleOrganization));
                        db.CreateTable(false, typeof(Topic));
                        db.CreateTable(false, typeof(TopicReported));
                        db.CreateTable(false, typeof(UserExam));
                        db.CreateTable(false, typeof(RoleTopic));
                        db.CreateTable(false, typeof(Tag));
                        db.CreateTable(false, typeof(TagChannel));
                        db.CreateTable(false, typeof(TagTopic));
                        db.CreateTable(false, typeof(Comment));
                        db.CreateTable(false, typeof(CommentReported));
                        db.CreateTable(false, typeof(Message));
                        db.CreateTable(false, typeof(UserTopicFavorite));
                        db.CreateTable(false, typeof(Premium));
                        db.CreateTable(false, typeof(PremiumUser));
                        db.CreateTable(false, typeof(Prices));
                        db.CreateTable(false, typeof(ReceiverMessage));
                        db.CreateTable(false, typeof(UserInfoPayment));
                        db.CreateTable(false, typeof(UserTopicFavorite));
                        db.CreateTable(false, typeof(ViewByTopic));
                        db.CreateTable(false, typeof(VoteChannel));

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
                        trans.Commit();
                    }
                    
                }
                
            }
        }
    }
}