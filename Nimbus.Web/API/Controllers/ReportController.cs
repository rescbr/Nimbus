using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using System.Web.Http;
using Nimbus.Web.API.Models;
using System.Net;
using System.Net.Http;
using Nimbus.Model.ORM;

namespace Nimbus.Web.API.Controllers
{
    [NimbusAuthorize]
    public class ReportController: NimbusApiController
    {
        /// <summary>
        /// Método de denúncia:gravar o usuário e o tipo reportado
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [HttpPost]
        public ReportModel ReportComment(ReportModel dados )
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
                            //report comments
                            if (dados.typeReport.ToLower() == "comment")
                            {
                                var report = new Nimbus.Model.ORM.CommentReported
                                {
                                    UserReporterId = NimbusUser.UserId,
                                    UserReportedId = dados.userReported_id,
                                    Justification = dados.justification,
                                    CommentReportedId = dados.idReport
                                };
                                db.Save(report);
                                reportID = (int)db.GetLastInsertId(); //pega o id criado anteriormente
                            }
                            //report channel
                            else if (dados.typeReport.ToLower() == "channel")
                            {
                                var report = new Nimbus.Model.ORM.ChannelReported
                                {
                                    UserReporterId = NimbusUser.UserId,
                                    UserReportedId = dados.userReported_id,
                                    Justification = dados.justification,
                                    ChannelReportedId = dados.idReport
                                };
                                db.Save(report);
                                reportID = (int)db.GetLastInsertId(); //pega o id criado anteriormente
                            }
                            //report topic
                            else if (dados.typeReport.ToLower() == "topic")
                            {
                                var report = new Nimbus.Model.ORM.TopicReported
                                {
                                    UserReporterId = NimbusUser.UserId,
                                    UserReportedId = dados.userReported_id,
                                    Justification = dados.justification,
                                    TopicReportedId = dados.idReport
                                };
                                db.Save(report);
                                reportID = (int)db.GetLastInsertId(); //pega o id criado anteriormente
                            }
                            //report user
                            var userReported = new Nimbus.Model.ORM.UserReported
                            {
                                UserReportedId = dados.userReported_id,
                                UserReporterId = NimbusUser.UserId
                            };
                            db.Insert(userReported);
                            trans.Commit();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return dados;
        }

        /// <summary>
        /// Reporta o usuário
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [HttpPost]
        public UserReported ReportUser(UserReported dados)
        {
            if (dados != null)
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {                 
                    db.Insert<UserReported>(dados);

                    dados.Id = (int)db.GetLastInsertId();
                }
            }
            return dados;
        }
    }
}   

