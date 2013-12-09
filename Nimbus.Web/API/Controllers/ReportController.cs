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
        /// Reporta um canal
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [HttpPost]
        public ChannelReported ReportChannel(ChannelReported dados)
        {
            if (dados != null)
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    dados.UserReporterId = NimbusUser.UserId;
                    db.Insert<ChannelReported>(dados);
                    dados.Id = (int)db.GetLastInsertId();
                }
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
                    dados.UserReporterId = NimbusUser.UserId;
                    db.Insert<UserReported>(dados);

                    dados.Id = (int)db.GetLastInsertId();
                }
            }
            return dados;
        }

        /// <summary>
        /// Reporta um tópico
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [HttpPost]
        public TopicReported ReportTopic(TopicReported dados)
        {
            if (dados != null)
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    dados.UserReporterId = NimbusUser.UserId;
                    db.Insert<TopicReported>(dados);
                    dados.Id = (int)db.GetLastInsertId();
                }
            }
            return dados;
        }

        /// <summary>
        /// Reporta um comentário
        /// </summary>
        /// <param name="dados"></param>
        /// <returns></returns>
        [HttpPost]
        public CommentReported ReportComment(CommentReported dados)
        {
            if (dados != null)
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    dados.UserReporterId = NimbusUser.UserId;
                    db.Insert<CommentReported>(dados);
                    dados.Id = (int)db.GetLastInsertId();
                }
            }
            return dados;
        }
    }
}   

