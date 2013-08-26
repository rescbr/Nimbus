using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using System.Web.Http;
using Nimbus.Web.API.Models;

namespace Nimbus.Web.API.Controllers
{
    public class ReportAPIController: NimbusApiController
    {
        /// <summary>
        /// Método de denúncia:gravar o usuário e o tipo reportado
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [Authorize]
        public bool ReportComment(ReportAPIModel dados )
        {

            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            int reportID = -1;
                            //report comment
                            if (dados.typeReport == Models.reportType.comment)
                            {
                                var report = new Nimbus.DB.CommentReported
                                {
                                    UserReporter_ID = NimbusUser.UserId,
                                    UserReported_ID = dados.userReported_id,
                                    Justification = dados.Justification,
                                    CommentReported_ID = dados.idReport
                                };
                                db.Save(report);
                                reportID = (int)db.GetLastInsertId(); //pega o id criado anteriormente
                            }
                            //report channel
                            else if (dados.typeReport == Models.reportType.channel)
                            {
                                var report = new Nimbus.DB.ChannelReported
                                {
                                    UserReporter_ID = NimbusUser.UserId,
                                    UserReported_ID = dados.userReported_id,
                                    Justification = dados.Justification,
                                    ChannelReported_ID = dados.idReport
                                };
                                db.Save(report);
                                reportID = (int)db.GetLastInsertId(); //pega o id criado anteriormente
                            }
                            //report topic
                            else if (dados.typeReport == Models.reportType.topic)
                            {
                                var report = new Nimbus.DB.TopicReported
                                {
                                    UserReporter_ID = NimbusUser.UserId,
                                    UserReported_ID = dados.userReported_id,
                                    Justification = dados.Justification,
                                    TopicReported_ID = dados.idReport
                                };
                                db.Save(report);
                                reportID = (int)db.GetLastInsertId(); //pega o id criado anteriormente
                            }
                            //report user
                            var userReported = new Nimbus.DB.UserReported
                            {
                                UserReported_ID = dados.userReported_id,
                                UserReporter_ID = NimbusUser.UserId,
                                Report_ID = reportID,
                                Type = DB.Enums.ReportType.comment
                            };
                            db.Insert(userReported);
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return false;
        }

    }
}   

