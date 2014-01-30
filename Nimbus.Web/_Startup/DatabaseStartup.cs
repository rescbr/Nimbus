using Nimbus.Model.ORM;
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
                        db.CreateTable(false, typeof(ImgTopChannel));
                        db.CreateTable(false, typeof(Ad)); 
                        db.CreateTable(false, typeof(Organization));
                        db.CreateTable(false, typeof(User));
                        db.CreateTable(false, typeof(UserAds));
                        db.CreateTable(false, typeof(Channel));
                        db.CreateTable(false, typeof(ChannelReported));
                        db.CreateTable(false, typeof(ChannelUser));
                        db.CreateTable(false, typeof(OrganizationUser));
                        db.CreateTable(false, typeof(UserChannelReadLater));
                        db.CreateTable(false, typeof(Role));
                        db.CreateTable(false, typeof(OrganizationUser));
                       // db.CreateTable(false, typeof(RoleOrganization)); //tabela em branco
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
                        db.CreateTable(false, typeof(UserLikeTopic));
                        db.CreateTable(false, typeof(UserReported));
                        db.CreateTable(false, typeof(ViewByTopic));
                        db.CreateTable(false, typeof(VoteChannel));
                        db.CreateTable(false, typeof(Notification<object>));
                        db.CreateTable(false, typeof(StorageUpload));
                        db.CreateTable(false, typeof(UserTopicReadLater));
                        //badge: tabela em branco   
                        //userbadge: tabela em branco
                        //log_user: tabela em branco

                        var nimbusorg = new Nimbus.Model.ORM.Organization()
                        {
                            Cname = "www.portalnimbus.com.br",
                            Id = 1,
                            Name = "Portal Nimbus"
                        };
                        db.Save(nimbusorg);

                        var sysuser = new Model.ORM.User()
                        {
                            Id = 1,
                            FirstName = "System",
                            LastName = "Administrator",
                            Email = "sysop@portalnimbus.com.br",
                            Password = new Security.PlaintextPassword("local@adm1").Hash,
                            TOTPKey = Security.OneTimePassword.GenerateSecret(),
                            BirthDate = DateTime.Now,
                            AvatarUrl = "/images/av130x130/person_icon.png"
                        };
                        db.Save(sysuser);

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 1,
                            ImageUrl = "/images/category/saude.png",
                            Name = "Saúde",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 2,
                            ImageUrl = "/images/category/animacao.png",
                            Name = "Animação",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 3,
                            ImageUrl = "/images/category/artes.png",
                            Name = "Artes",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 4,
                            ImageUrl = "/images/category/artesanato.png",
                            Name = "Artesanato",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 5,
                            ImageUrl = "/images/category/astronomia.png",
                            Name = "Astronomia",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 6,
                            ImageUrl = "/images/category/certificacao.png",
                            Name = "Certificação",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 8,
                            ImageUrl = "/images/category/concursos.png",
                            Name = "Concursos",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 9,
                            ImageUrl = "/images/category/culinaria.png",
                            Name = "Culinária",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 10,
                            ImageUrl = "/images/category/esportes.png",
                            Name = "Esportes",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 11,
                            ImageUrl = "/images/category/filosofia.png",
                            Name = "Filosofia",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 12,
                            ImageUrl = "/images/category/fotografia.png",
                            Name = "Fotografia",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 13,
                            ImageUrl = "/images/category/jardinagem.png",
                            Name = "Jardinagem",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 14,
                            ImageUrl = "/images/category/idiomas.png",
                            Name = "Idiomas",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 15,
                            ImageUrl = "/images/category/literatura.png",
                            Name = "Literatura",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 16,
                            ImageUrl = "/images/category/musica.png",
                            Name = "Música",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 17,
                            ImageUrl = "/images/category/computacao.png",
                            Name = "Computação",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 18,
                            ImageUrl = "/images/category/televisao.png",
                            Name = "Televisão",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 19,
                            ImageUrl = "/images/category/vestibular.png",
                            Name = "Vestibular",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 20,
                            ImageUrl = "/images/category/redacao.png",
                            Name = "Redação",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 21,
                            ImageUrl = "/images/category/ciencias.png",
                            Name = "Ciências",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 22,
                            ImageUrl = "/images/category/biologia.png",
                            Name = "Biologia",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 23,
                            ImageUrl = "/images/category/business.png",
                            Name = "Business",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 24,
                            ImageUrl = "/images/category/empreendedorismo.png",
                            Name = "Empreendedorismo",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 25,
                            ImageUrl = "/images/category/fisica.png",
                            Name = "Física",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 26,
                            ImageUrl = "/images/category/geografia.png",
                            Name = "Geografia",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 27,
                            ImageUrl = "/images/category/historia.png",
                            Name = "História",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 28,
                            ImageUrl = "/images/category/matematica.png",
                            Name = "Matemática",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 29,
                            ImageUrl = "/images/category/portugues.png",
                            Name = "Português",
                        });

                        db.Save(new Model.ORM.Category()
                        {
                            Id = 30,
                            ImageUrl = "/images/category/quimica.png",
                            Name = "Química",
                        });

                        trans.Commit();
                    }
                    
                }
                
            }
        }
    }
}