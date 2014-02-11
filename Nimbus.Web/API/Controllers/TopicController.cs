using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using Nimbus.Web.API.Models;
using Nimbus.Model.Bags;
using Nimbus.Web.Utils;
using System.Web;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para os Tópicos
    /// </summary>
    [NimbusAuthorize]
    public class TopicController : NimbusApiController
    {
        #region IsOwner e IsManager
        /// <summary>
        /// Verifica se o usuário é dono do canalque possui o tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [NonAction]
        public bool IsOwner(int id, string tipo)
        {
            bool allow = false;
            int channelId = -1;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (tipo == "topic")
                    {
                        channelId = db.SelectParam<Topic>(tp => tp.Id == id && tp.Visibility == true).Select(tp => tp.ChannelId).FirstOrDefault();
                    }
                    else if (tipo == "channel")
                    {
                        channelId = id;
                    }

                    allow = db.SelectParam<Role>(own => own.UserId == NimbusUser.UserId && own.ChannelId == channelId)
                                                                        .Select(own => own.IsOwner).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return allow;
        }

        /// <summary>
        /// Verifica se o usuário é adm do canal que possui o topico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [NonAction]
        public bool IsManager(int id, string tipo)
        {
            bool allow = false;
            int channelId = -1;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (tipo == "topic")
                    {
                        channelId = db.SelectParam<Topic>(tp => tp.Id == id && tp.Visibility == true).Select(tp => tp.ChannelId).FirstOrDefault();
                    }
                    else if (tipo == "channel")
                    {
                        channelId = id;
                    }

                    allow = db.SelectParam<Role>(mg => mg.UserId == NimbusUser.UserId && mg.ChannelId == id)
                                                                          .Exists(mg => mg.ChannelMagager == true || mg.TopicManager == true);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return allow;
        }

        #endregion

        #region Criar e Mostrar tópico (Post e GetComment)
        /// <summary>
        /// Criar um novo tópico
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpPost]
        public Topic NewTopic(Topic topic)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        bool isOwner = IsOwner(topic.ChannelId, "channel");
                        bool isManager = IsManager(topic.ChannelId, "channel");
                        if (isOwner == true || isManager == true)
                        {
                            topic.AuthorId = NimbusUser.UserId;
                            if (string.IsNullOrEmpty(topic.ImgUrl))
                            {
                                int idCtg = db.SelectParam<Channel>(ch => ch.Id == topic.ChannelId).Select(ch => ch.CategoryId).FirstOrDefault();
                                topic.ImgUrl = db.SelectParam<Category>(ct => ct.Id == idCtg).Select(ct => ct.ImageUrl).FirstOrDefault();
                            }
                            topic.CreatedOn = DateTime.Now;
                            topic.LastModified = DateTime.Now;
                            topic.Visibility = true;

                            if (string.IsNullOrEmpty(topic.Price.ToString()))
                            {
                                topic.Price = 0;
                            }

                            topic.Description = topic.Description;
                            if (topic.Text != null)
                                topic.Text = HtmlSanitizer.SanitizeHtml(topic.Text);
                            topic.Title = topic.Title;

                            //tópicos do tipo file devem ter os arquivos armazenados apenas no nimbus.
                            if (topic.TopicType == Model.Enums.TopicType.file &&
                                !topic.UrlVideo.StartsWith("http://storage.portalnimbus.com.br/"))
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "arquivo fora da storage"));
                            }
                            topic.UrlVideo = topic.UrlVideo;

                            //Renato: comentado devido processo de remoção de htmlencode
                            //if (topic.TopicType == Model.Enums.TopicType.exam)
                            //{
                            //    foreach (var item in topic.Question)
                            //    {
                            //        item.TextQuestion = HttpUtility.HtmlEncode(item.TextQuestion);
                            //        //colocar encode nas opçoes
                            //    }
                            //}

                            db.Save(topic);
                            topic.Id = (int)db.GetLastInsertId();
                            trans.Commit();

                            var newTopicNotification = new Notifications.TopicNotification();
                            newTopicNotification.NewTopic(topic);

                            return topic;
                        }
                        else
                        {
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "erro ao criar item"));
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Método para editar um tópico
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpPost]
        public Topic EditTopic(Topic topic)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        bool isOwner = IsOwner(topic.ChannelId, "channel");
                        bool isManager = IsManager(topic.ChannelId, "channel");

                        if (isOwner == true || isManager == true)
                        {
                            Topic tpc = db.SelectParam<Topic>(tp => tp.Id == topic.Id).FirstOrDefault();
                            tpc.Description = topic.Description;

                            if (string.IsNullOrEmpty(topic.ImgUrl))
                            {
                                int idCtg = db.SelectParam<Channel>(ch => ch.Id == tpc.ChannelId).Select(ch => ch.CategoryId).FirstOrDefault();
                                tpc.ImgUrl = db.SelectParam<Category>(ct => ct.Id == 1).Select(ct => ct.ImageUrl).FirstOrDefault();
                            }
                            else
                            {
                                tpc.ImgUrl = topic.ImgUrl;
                            }
                            tpc.LastModified = DateTime.Now;

                            if (topic.Text != null)
                                tpc.Text = HtmlSanitizer.SanitizeHtml(topic.Text);
                            else
                                tpc.Text = null;

                            if (topic.TopicType == Model.Enums.TopicType.exam)
                            {
                                tpc.Question = topic.Question;
                            }

                            tpc.Title = topic.Title;
                            tpc.UrlCapa = topic.UrlCapa != null ? topic.UrlCapa : tpc.UrlCapa;
                            if (!string.IsNullOrEmpty(topic.UrlVideo))
                            {
                                //tópicos do tipo file devem ter os arquivos armazenados apenas no nimbus.
                                if (tpc.TopicType == Model.Enums.TopicType.file &&
                                    !topic.UrlVideo.StartsWith("http://storage.portalnimbus.com.br/"))
                                {
                                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "arquivo fora da storage"));
                                }
                                tpc.UrlVideo = topic.UrlVideo;
                            }
                            tpc.Visibility = true;
                            if (string.IsNullOrEmpty(topic.Price.ToString()))
                            {
                                tpc.Price = 0;
                            }
                            else
                            {
                                tpc.Price = topic.Price;
                            }


                            db.Update<Topic>(tpc);
                            trans.Commit();

                            //Renato: comentado devido processo de remoção de htmlencode
                            //topic.Title = HttpUtility.HtmlDecode(topic.Title);
                            //topic.Description = HttpUtility.HtmlDecode(topic.Description);
                            //topic.ImgUrl = HttpUtility.HtmlDecode(topic.ImgUrl);
                            //topic.Text = HttpUtility.HtmlDecode(topic.Text);
                            //topic.Title = HttpUtility.HtmlDecode(topic.Title);

                            var notification = new Notifications.TopicNotification();
                            notification.EditTopic(tpc);

                            return topic;
                        }
                        else
                        {
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "erro ao editar item"));
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }

            }
        }

        //TODO terminar essa funçao: parte de mostras os topicos relacionados
        /// <summary>
        /// carregar informações gerais de um tópico, chammar a função de comentarios e count favoritos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Topic ShowTopic(int id)
        {
            Topic topic = new Topic();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = ValidateShowTopic(id);

                    if (allow == true)
                    {
                        topic = db.SelectParam<Topic>(tp => tp.Id == id).FirstOrDefault();

                        //Renato: comentado devido processo de remoção de htmlencode
                        //topic.Title = HttpUtility.HtmlDecode(topic.Title);                       
                        //topic.Description = HttpUtility.HtmlDecode(topic.Description);
                        //topic.Text = HttpUtility.HtmlDecode(topic.Text);

                        if (topic.TopicType == Nimbus.Model.Enums.TopicType.exam)
                        {
                            #region exam
                            //verificar se o usuario já fez o exame
                            int ChannelID = db.SelectParam<Channel>(ch => ch.Id == topic.ChannelId).Select(ch => ch.OrganizationId).FirstOrDefault();
                            UserExam userExam = ValidateExam(id);

                            bool isPrivate = db.SelectParam<Channel>(ch => ch.Id == topic.ChannelId).Select(ch => ch.IsPrivate).FirstOrDefault();

                            if (userExam == null || isPrivate == false)
                            {
                                //se nunca tiver feito o exame, pode fazer. Canal privado = pode limitar. Canal free = sempre aberto 
                                //caso seja um teste free, o 'bool' já permite refazer - apagar as respostas

                                //Renato: comentado devido processo de remoção de htmlencode
                                //foreach (Nimbus.Model.Question item in topic.Question)
                                //{
                                //    item.TextQuestion = HttpUtility.HtmlDecode(item.TextQuestion);
                                //    //colocar p opçoes e arrumar p retornar esse
                                //}
                            }
                            #endregion
                        }
                        if (topic.TopicType == Nimbus.Model.Enums.TopicType.video)
                        {
                            #region video
                            string url = topic.UrlVideo;
                            //garantir que mesmo que o vídeo tenha sido salvo com string padrao diferente, irá retornar o param certo
                            if (url.Length >= 11)
                            {
                                string param = "";
                                if (url.IndexOf("/embed") <= 0)
                                {
                                    if (url.IndexOf("youtube.be/") > 0)
                                    {
                                        int posicao = url.IndexOf(".be/") + 4;
                                        param = url.Substring(posicao);
                                    }
                                    else if (url.IndexOf("youtube.com") > 0)
                                    {
                                        string querystring = new Uri(topic.UrlVideo).Query;
                                        var q = HttpUtility.ParseQueryString(querystring);
                                        param = q["v"];
                                    }
                                    topic.UrlVideo = "//www.youtube.com/embed/" + param + "?wmode=transparent";
                                }
                                else
                                {
                                    //já foi salvo da forma correta
                                    int posicao = url.IndexOf("http:") + 5;
                                    param = url.Substring(posicao);
                                    topic.UrlVideo = param + "?wmode=transparent";
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return topic;
        }

        /// <summary>
        /// carregar informações gerais de um tópico exposto na login page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public Topic ShowTopicToLoginPage(int id)
        {
            Topic topic = new Topic();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    topic = db.SelectParam<Topic>(tp => tp.Id == id).FirstOrDefault();

                    if (topic.TopicType == Nimbus.Model.Enums.TopicType.video)
                    {
                        #region video
                        string url = topic.UrlVideo;
                        //garantir que mesmo que o vídeo tenha sido salvo com string padrao diferente, irá retornar o param certo
                        if (url.Length >= 11)
                        {
                            string param = "";
                            if (url.IndexOf("/embed") <= 0)
                            {
                                if (url.IndexOf("youtube.be/") > 0)
                                {
                                    int posicao = url.IndexOf(".be/") + 4;
                                    param = url.Substring(posicao);
                                }
                                else if (url.IndexOf("youtube.com") > 0)
                                {
                                    string querystring = new Uri(topic.UrlVideo).Query;
                                    var q = HttpUtility.ParseQueryString(querystring);
                                    param = q["v"];
                                }
                                topic.UrlVideo = "//www.youtube.com/embed/" + param + "?wmode=transparent";
                            }
                            else
                            {
                                //já foi salvo da forma correta
                                int posicao = url.IndexOf("http:") + 5;
                                param = url.Substring(posicao);
                                topic.UrlVideo = param + "?wmode=transparent";
                            }
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return topic;
        }

        #endregion

        /// <summary>
        /// Trending Topics de todas as categorias
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<TopicBag> TrendingTopics(int skip = 0, int take = 12, int categoryId = 0)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 12;

            #region comentarios + exemplos
            /***************************************************************************
             *                           HOT SORT TÓPICO                               * 
             *        para ordenar TRENDING tópicos de acordo com o tempo              *
             * query baseada em:                                                       *
             * https://github.com/reddit/reddit/blob/master/sql/functions.sql          *
             * http://bibwild.wordpress.com/2012/05/08/reddit-story-ranking-algorithm/ *
             * http://technotes.iangreenleaf.com/posts/2013-12-09-reddits-empire-is-built-on-a-flawed-algorithm.html
             ***************************************************************************/
            #endregion

            List<TopicBag> tpcList = new List<TopicBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                List<Topic> trending = new List<Topic>();
                if (categoryId > 0)
                {
                    trending = db.Query<Topic>(
                    #region queryzinha 4: o terror contra-ataca
                        //este SQL utiliza o metodo de skip/take do sql server 2012 (que funciona no sql azure tbm).
                        //não irá funcionar em versoes antigas.
@"                
select [Id], [ImgUrl], [Title], [TopicType], [Description], [LastModified], [CreatedOn],[UserReadLater], [UserFavorited]
from (
	select [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description], [Topic].[LastModified], [Topic].[CreatedOn],
		   (select round(
					cast((log(case when abs([positive] - [negative]) > 1 then abs([positive] - [negative]) else 1 end) 
						* sign([positive] - [negative])) + (datediff(second, '20130101', [Topic].[CreatedOn]) / 45000)
					as numeric), 7) 
				as [hot_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score],
        (select count(*)
    		from [UserTopicReadLater]
            where [UserTopicReadLater].[TopicId] = [Topic].[Id] and [UserTopicReadLater].[UserId] = @idUser
        ) as [UserReadLater],
		
		(select count(*)
            from [UserTopicFavorite]		
            where [UserTopicFavorite].[TopicId] = [Topic].[Id] and [UserTopicFavorite].[UserId] = @idUser  
        ) as [UserFavorited]
	from [Topic]
    inner join [Channel] on [Topic].[ChannelId] = [Channel].[Id] and [Channel].[CategoryId] = @idCategory
    where [Topic].[Visibility] = 1
	order by score desc
    offset @skip rows fetch next @take rows only
) tmpScoreSort",
                    #endregion
 new { skip = skip * take, take = take, idCategory = categoryId, idUser = NimbusUser.UserId });
                }
                else
                {
                    trending = db.Query<Topic>(
                    #region queryzinha 3: a vingança do sql
                        //este SQL utiliza o metodo de skip/take do sql server 2012 (que funciona no sql azure tbm).
                        //não irá funcionar em versoes antigas.
    @"                
select [Id], [ImgUrl], [Title], [TopicType], [Description], [LastModified], [CreatedOn],[UserReadLater], [UserFavorited]
from (
	select [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description], [Topic].[LastModified], [Topic].[CreatedOn],
		   (select round(
					cast((log(case when abs([positive] - [negative]) > 1 then abs([positive] - [negative]) else 1 end) 
						* sign([positive] - [negative])) + (datediff(second, '20130101', [Topic].[CreatedOn]) / 45000)
					as numeric), 7) 
				as [hot_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score],
        (select count(*)
    		from [UserTopicReadLater]
            where [UserTopicReadLater].[TopicId] = [Topic].[Id] and [UserTopicReadLater].[UserId] = @idUser
        ) as [UserReadLater],
		
		(select count(*)
            from [UserTopicFavorite]		
            where [UserTopicFavorite].[TopicId] = [Topic].[Id] and [UserTopicFavorite].[UserId] = @idUser  
        ) as [UserFavorited]

	from [Topic]
    where [Topic].[Visibility] = 1
	order by score desc
    offset @skip rows fetch next @take rows only
) tmpScoreSort",
                    #endregion
 new { skip = skip * take, take = take, idUser = NimbusUser.UserId });
                }
                if (trending.Count > 0)
                {
                    foreach (var item in trending)
                    {
                        int count = db.SelectParam<ViewByTopic>(vt => vt.TopicId == item.Id).Select(vt => vt.CountView).FirstOrDefault();
                        TopicBag bag = new TopicBag()
                        {
                            Id = item.Id,
                            Description = item.Description,
                            Title = item.Title,
                            TopicType = item.TopicType,
                            ImgUrl = item.ImgUrl,
                            LastModified = item.LastModified,
                            Count = count,
                            UserReadLater = item.UserReadLater != null ? item.UserReadLater : false,
                            UserFavorited = item.UserFavorited != null ? item.UserFavorited : false
                        };
                        tpcList.Add(bag);
                    }
                }

                return tpcList;
            }
        }

        [HttpGet]
        public List<TopicBag> TopTopics(int skip = 0, int take = 12, int idCat = 0)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 12;
            #region comentarios e exemplos
            /************************************************************************
             *                      CONFIDENCE SORT TÓPICO                          * 
             *            para ordenar TOP tópicos de acordo com a votação          *
             * query baseada em:                                                    *
             * https://github.com/reddit/reddit/blob/master/r2/r2/lib/db/_sorts.pyx *
             * http://www.evanmiller.org/how-not-to-sort-by-average-rating.html     *
             ************************************************************************/
            #endregion

            List<TopicBag> tpcList = new List<TopicBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                List<Topic> top = new List<Topic>();
                if (idCat > 0)
                {
                    top = db.Query<Topic>(
                    #region queryzinha 5: destruição total
                        //este SQL utiliza o metodo de skip/take do sql server 2012 (que funciona no sql azure tbm).
                        //não irá funcionar em versoes antigas.
    @"
select [Id], [ImgUrl], [Title], [TopicType], [Description], [LastModified], [UserReadLater], [UserFavorited]
from (
	select [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description], [Topic].[LastModified],
	 (select ((([positive] + 1.9208) / ([positive] + [negative]) - 
			  1.96 * SQRT(([positive] * [negative]) / ([positive] + [negative]) + 0.9604) / 
			  ([positive] + [negative])) / (1 + 3.8416 / ([positive] + [negative]))) as [confidence_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score],
        (select count(*)
    		from [UserTopicReadLater]
            where [UserTopicReadLater].[TopicId] = [Topic].[Id] and [UserTopicReadLater].[UserId] = @idUser
        ) as [UserReadLater],
		
		(select count(*)
            from [UserTopicFavorite]		
            where [UserTopicFavorite].[TopicId] = [Topic].[Id] and [UserTopicFavorite].[UserId] = @idUser  
        ) as [UserFavorited]
	from [Topic]
	inner join [Channel] on [Topic].[ChannelId] = [Channel].[Id] and [Channel].[CategoryId] = @idCategory
    where [Topic].[Visibility] = 1
	order by score desc
    offset @skip rows fetch next @take rows only
) tmpScoreSort",
                    #endregion
 new { skip = skip * take, take = take, idCategory = idCat, idUser = NimbusUser.UserId });


                }
                else
                {
                    top = db.Query<Topic>(
                    #region queryzinha 2, o retorno
                        //este SQL utiliza o metodo de skip/take do sql server 2012 (que funciona no sql azure tbm).
                        //não irá funcionar em versoes antigas.
                       @"
select [Id], [ImgUrl], [Title], [TopicType], [Description], [LastModified],[UserReadLater], [UserFavorited]
from (
	select [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description], [Topic].[LastModified],
	 (select ((([positive] + 1.9208) / ([positive] + [negative]) - 
			  1.96 * SQRT(([positive] * [negative]) / ([positive] + [negative]) + 0.9604) / 
			  ([positive] + [negative])) / (1 + 3.8416 / ([positive] + [negative]))) as [confidence_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score],
        (select count(*)
    		from [UserTopicReadLater]
            where [UserTopicReadLater].[TopicId] = [Topic].[Id] and [UserTopicReadLater].[UserId] = @idUser
        ) as [UserReadLater],
		
		(select count(*)
            from [UserTopicFavorite]		
            where [UserTopicFavorite].[TopicId] = [Topic].[Id] and [UserTopicFavorite].[UserId] = @idUser  
        ) as [UserFavorited]
	from [Topic]
    where [Topic].[Visibility] = 1
	order by score desc
    offset @skip rows fetch next @take rows only
) tmpScoreSort",
                    #endregion
 new { skip = skip * take, take = take, idUser = NimbusUser.UserId });
                }
                if (top.Count > 0)
                {
                    foreach (var item in top)
                    {
                        int count = db.SelectParam<ViewByTopic>(vt => vt.TopicId == item.Id).Select(vt => vt.CountView).FirstOrDefault();
                        TopicBag bag = new TopicBag()
                        {
                            Id = item.Id,
                            Description = item.Description,
                            Title = item.Title,
                            TopicType = item.TopicType,
                            ImgUrl = item.ImgUrl,
                            LastModified = item.LastModified,
                            Count = count,
                            UserReadLater = item.UserReadLater != null ? item.UserReadLater : false,
                            UserFavorited = item.UserFavorited != null ? item.UserFavorited : false
                            //db.Where<UserTopicFavorite>(u => u.UserId == NimbusUser.UserId && u.TopicId == item.Id).Exists(u => u.Visible)
                        };
                        tpcList.Add(bag);
                    }
                }

                return tpcList;
            }
        }

        [HttpGet]
        public List<TopicBag> TopTopicsToLogin()
        {
            int skip = 0;
            int take = 4;
            /************************************************************************
             *                      CONFIDENCE SORT TÓPICO                          * 
             *            para ordenar TOP tópicos de acordo com a votação          *
             * query baseada em:                                                    *
             * https://github.com/reddit/reddit/blob/master/r2/r2/lib/db/_sorts.pyx *
             * http://www.evanmiller.org/how-not-to-sort-by-average-rating.html     *
             ************************************************************************/

            List<TopicBag> tpcList = new List<TopicBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var top = db.Query<Topic>(
                #region queryzinha 6: oráculo da visão
                    //este SQL utiliza o metodo de skip/take do sql server 2012 (que funciona no sql azure tbm).
                    //não irá funcionar em versoes antigas.
@"
select [Id], [ImgUrl], [Title], [TopicType], [Description], [LastModified]
from (
	select [Topic].[Id], [Topic].[ImgUrl], [Topic].[Title], [Topic].[TopicType], [Topic].[Description], [Topic].[LastModified],
	 (select ((([positive] + 1.9208) / ([positive] + [negative]) - 
			  1.96 * SQRT(([positive] * [negative]) / ([positive] + [negative]) + 0.9604) / 
			  ([positive] + [negative])) / (1 + 3.8416 / ([positive] + [negative]))) as [confidence_score]
			from (
				select 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 1 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [positive], 
					(select count([Visible]) from [UserLikeTopic] where [UserLikeTopic].[Visible] = 0 and [UserLikeTopic].[TopicId] = [Topic].[Id]) as [negative]
				) tmpCount
			where [positive] + [negative] > 0) as [score]
	from [Topic]
	order by score desc
    offset @skip rows fetch next @take rows only
) tmpScoreSort",
                #endregion
 new { skip = skip * take, take = take });

                if (top.Count > 0)
                {
                    foreach (var item in top)
                    {
                        int count = db.SelectParam<ViewByTopic>(vt => vt.TopicId == item.Id).Select(vt => vt.CountView).FirstOrDefault();
                        TopicBag bag = new TopicBag()
                        {
                            Id = item.Id,
                            Description = item.Description,
                            Title = item.Title,
                            TopicType = item.TopicType,
                            ImgUrl = item.ImgUrl,
                            LastModified = item.LastModified,
                            Count = count,
                            UserFavorited = false
                        };
                        tpcList.Add(bag);
                    }
                }

                return tpcList;
            }
        }

        public class TopicHtmlWrapper
        {
            public int Count { get; set; }
            public string Html { get; set; }
        }

        [HttpGet]
        public TopicHtmlWrapper AbstTopicHtml(int id = 0, string viewBy = null, int categoryID = 0, int skip = 0, string type = null)
        {
            var rz = new RazorTemplate();
            string html = "";
            List<TopicBag> topics = new List<TopicBag>();

            if (type == "abstopic")
            {
                topics = AbstTopic(id, viewBy, categoryID, skip);
            }
            else if (type == "marcado")
            {
                topics = showReadLaterTopic(id, skip);
            }
            else if (type == "favorited")
            {
                topics = TopicsFavoriteUsers(id, skip);
            }
            else if (type == "toptopic")
            {
                int take = id; //p/ nao ter que usar outra variavel, aproveitar a variavel id q/ é um inteiro não utilizado para mostrar os topTopics
                topics = TopTopics(skip, take, categoryID);
            }
            foreach (var topic in topics)
            {
                html += rz.ParseRazorTemplate<TopicBag>
                    ("~/Website/Views/ChannelPartials/TopicPartial.cshtml", topic);
            }
            return new TopicHtmlWrapper { Html = html, Count = topics.Count };
        }

        [HttpGet]
        public TopicHtmlWrapper TopTopicHtml(int take = 0, int categoryID = 0, int skip = 0)
        {
            var rz = new RazorTemplate();
            string html = "";
            List<TopicBag> topics = new List<TopicBag>();

            topics = TopTopics(skip, take, categoryID);

            foreach (var topic in topics)
            {
                html += rz.ParseRazorTemplate<TopicBag>
                    ("~/Website/Views/ChannelPartials/TopicPartial.cshtml", topic);
            }
            return new TopicHtmlWrapper { Html = html, Count = topics.Count };
        }

        [HttpGet]
        public TopicHtmlWrapper TrendingTopicHtml(int take = 0, int categoryID = 0, int skip = 0)
        {
            var rz = new RazorTemplate();
            string html = "";
            List<TopicBag> topics = new List<TopicBag>();

            topics = TrendingTopics(skip, take, categoryID);

            foreach (var topic in topics)
            {
                html += rz.ParseRazorTemplate<TopicBag>
                    ("~/Website/Views/ChannelPartials/TopicPartial.cshtml", topic);
            }
            return new TopicHtmlWrapper { Html = html, Count = topics.Count };
        }

        /// <summary>
        /// método de exibir tópicos em resumo, filtra por categoriam modificação e popularidade
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<TopicBag> AbstTopic(int channelID = 0, string viewBy = null, int categoryID = 0, int skip = 0)
        {
            List<TopicBag> tpcList = new List<TopicBag>();
            IEnumerable<int> idChannel;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                int idOrg = NimbusOrganization.Id;

                //verifica se o canal que esta tentando acessar é válido e existe
                if (channelID > 0)
                {
                    bool isValid = db.SelectParam<Channel>(ch => ch.Id == channelID && ch.Visible == true && ch.OrganizationId == idOrg)
                                                                .Exists(ch => ch.Id == channelID);
                    if (isValid)
                    {
                        idChannel = new List<int>();
                        (idChannel as List<int>).Add(channelID);
                    }
                    else
                    {
                        idChannel = null;
                    }
                }
                //busca todos os canais da categoria da organizacao
                else if (categoryID > 0)
                {

                    idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true && ch.CategoryId == categoryID)
                                                                        .Select(ch => ch.Id);
                }
                //busca todos os canais em geral da organizacao                      
                else
                {
                    idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true)
                                                                          .Select(ch => ch.Id);
                }

                if (idChannel.Count() > 0)
                {
                    List<Topic> topic = db.Where<Topic>(tp => tp.Visibility == true && idChannel.Contains(tp.ChannelId)).Skip(15 * skip).Take(15).ToList();


                    if (topic.Count > 0)
                    {
                        foreach (var item in topic)
                        {
                            int count = db.SelectParam<ViewByTopic>(vt => vt.TopicId == item.Id).Select(vt => vt.CountView).FirstOrDefault();
                            bool willRead = db.Where<UserTopicReadLater>(u => u.UserId == NimbusUser.UserId && u.TopicId == item.Id).Select(u => u.Visible).FirstOrDefault();
                            TopicBag bag = new TopicBag()
                            {
                                Id = item.Id,
                                Description = item.Description,
                                Title = item.Title,
                                TopicType = item.TopicType,
                                ImgUrl = item.ImgUrl,
                                LastModified = item.LastModified,
                                Count = count,
                                UserFavorited = db.Where<UserTopicFavorite>(u => u.UserId == NimbusUser.UserId && u.TopicId == item.Id).Exists(u => u.Visible),
                                UserReadLater = willRead != null ? willRead : false
                            };
                            tpcList.Add(bag);
                        }
                    }
                }
            }


            if (viewBy.ToLower() == "bymodified" || viewBy.ToLower() == "alltpc")
                return tpcList.OrderByDescending(tp => tp.LastModified).ToList();
            else if (viewBy.ToLower() == "bypopularity" || viewBy.ToLower() == "popular")
                return tpcList.OrderByDescending(tp => tp.Count).ToList();
            else
                return tpcList;
        }

        /// <summary>
        /// método de favoritar/desfavoritar o tópico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public bool TopicFavorite(int id)
        {
            bool flag = false;

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                UserTopicFavorite user = new UserTopicFavorite();
                user = db.SelectParam<UserTopicFavorite>(us => us.UserId == NimbusUser.UserId && us.TopicId == id).FirstOrDefault();

                if (user == null) //nunca favoritou
                {
                    UserTopicFavorite usertpv = new UserTopicFavorite()
                    {
                        FavoritedOn = DateTime.Now,
                        TopicId = id,
                        UserId = NimbusUser.UserId,
                        Visible = true
                    };
                    db.Insert<UserTopicFavorite>(usertpv);
                    flag = true;
                }
                else
                {
                    user.FavoritedOn = DateTime.Now;
                    user.Visible = (user.Visible == true) ? false : true;
                    db.Update<UserTopicFavorite>(user);
                    flag = user.Visible;
                }
            }
            return flag;
        }

        /// <summary>
        /// Retorna se o tópico visitado é favoritado ou não
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public bool TopicIsFavorite(int id)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                return db.Where<UserTopicFavorite>(t => t.TopicId == id && t.UserId == NimbusUser.UserId).Exists(t => t.Visible == true);
            }
        }

        /// <summary>
        /// Método que retorna todos os tópicos que o usuário favoritou
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        [HttpGet]
        public List<TopicBag> TopicsFavoriteUsers(int id, int skip)
        {
            List<TopicBag> listTpc = new List<TopicBag>();

            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var listUserTpc = db.SelectParam<UserTopicFavorite>(t => t.UserId == NimbusUser.UserId && t.Visible == true)
                                                             .Skip(15 * skip).Take(15);

                var listTopic = listUserTpc.Select(r => db.Where<Topic>(t => t.Visibility == true && t.Id == r.TopicId).FirstOrDefault()).ToList();

                foreach (var item in listTopic)
                {
                    TopicBag bag = new TopicBag
                    {
                        AuthorId = item.AuthorId,
                        ChannelId = item.ChannelId,
                        Id = item.Id,
                        ImgUrl = item.ImgUrl,
                        Title = item.Title,
                        TopicType = item.TopicType,
                        Visibility = item.Visibility,
                        UserReadLater = db.Where<UserTopicReadLater>(c => c.TopicId == item.Id && c.UserId == NimbusUser.UserId).Exists(c => c.Visible == true),
                        UserFavorited = db.Where<UserTopicFavorite>(c => c.Visible == true && c.TopicId == item.Id).Exists(c => c.UserId == NimbusUser.UserId)
                    };
                    listTpc.Add(bag);
                }
            }

            return listTpc;
        }

        /// <summary>
        /// Add/retirar tópico da lista de ler mais tarde 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="readOn"></param>
        /// <param name="willRead"></param>
        /// <returns></returns>
        [HttpPost]
        public bool ReadTopicLater(int id, DateTime? readOn = null, bool willRead = false)
        {
            bool operation = false;

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                UserTopicReadLater user = db.SelectParam<UserTopicReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.TopicId == id).FirstOrDefault();

                if (willRead == false)//retirar da lista
                {
                    user.Visible = false;
                    user.ReadOn = null;
                    db.Update<UserTopicReadLater>(user);
                    operation = true;
                }
                else if (willRead == true)//colocar na lista
                {
                    if (user == null)
                    {
                        UserTopicReadLater uTpL = new UserTopicReadLater
                        {
                            Visible = true,
                            ReadOn = DateTime.Now,
                            UserId = NimbusUser.UserId,
                            TopicId = id
                        };
                        db.Insert<UserTopicReadLater>(uTpL);
                        operation = true;
                    }
                    else
                    {
                        user.Visible = true;
                        readOn = DateTime.Now;
                        db.Update<UserTopicReadLater>(user);
                        operation = true;
                    }
                }
            }
            return operation;
        }

        /// <summary>
        /// Método para retornar os topicos que o usuário vai ler mais tarde
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<TopicBag> showReadLaterTopic(int id, int skip)
        {
            List<TopicBag> listTpc = new List<TopicBag>();

            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var listUserTpc = db.SelectParam<UserTopicReadLater>(t => t.UserId == NimbusUser.UserId && t.Visible == true)
                                                             .Skip(15 * skip).Take(15);

                var listTopic = listUserTpc.Select(r => db.Where<Topic>(t => t.Visibility == true && t.Id == r.TopicId).FirstOrDefault()).ToList();

                foreach (var item in listTopic)
                {
                    TopicBag bag = new TopicBag
                    {
                        AuthorId = item.AuthorId,
                        ChannelId = item.ChannelId,
                        Id = item.Id,
                        ImgUrl = item.ImgUrl,
                        Title = item.Title,
                        TopicType = item.TopicType,
                        Visibility = item.Visibility,
                        UserReadLater = listUserTpc.Where(c => c.UserId == NimbusUser.UserId && c.TopicId == item.Id).Select(c => c.Visible).FirstOrDefault(),
                        UserFavorited = db.Where<UserTopicFavorite>(c => c.Visible == true && c.TopicId == item.Id).Exists(c => c.UserId == NimbusUser.UserId)
                    };
                    listTpc.Add(bag);
                }
            }

            return listTpc;
        }



        /// <summary>
        /// Verifica se o tópico é privado ou pago e se o usuário possui permissão para visualizar o conteúdo
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [NonAction]
        public bool ValidateShowTopic(int id)
        {
            bool allow = false;
            //ver permissao p vizualizar => se é pago = ter pagado, se é privado = ser aceito
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                Topic tpc = db.SelectParam<Topic>(tp => tp.Id == id).FirstOrDefault();
                if (tpc.Visibility == false) return false;


                bool chnPrivate = db.SelectParam<Channel>(ch => ch.Id == tpc.ChannelId).Select(ch => ch.IsPrivate).FirstOrDefault();
                if (chnPrivate == false)
                {
                    allow = true;
                }
                else
                {
                    bool? pending = db.SelectParam<ChannelUser>(ch => ch.ChannelId == tpc.ChannelId && ch.UserId == NimbusUser.UserId)
                                                                                                    .Select(ch => ch.Accepted).FirstOrDefault();
                    if (pending == false && pending != null) //não esta pendente = já foi aceito
                    {
                        allow = true;
                    }
                    else
                    {
                        allow = false;
                    }
                }
                if (tpc.Price > 0)
                {
                    bool paid = db.SelectParam<RoleTopic>(tp => tp.ChannelId == tpc.ChannelId && tp.TopicId == id)
                                                                         .Select(tp => tp.Paid).FirstOrDefault();
                    if (paid == true)
                    {
                        allow = true;
                    }
                    else
                    {
                        allow = false;
                    }
                }
            }

            return allow;
        }

        /// <summary>
        /// Método de retornar o numero de favoritos de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public int CountFavorite(int id)
        {
            int count = 0;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    count = db.SelectParam<UserTopicFavorite>(fv => fv.TopicId == id && fv.Visible == true).Count();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return count;
        }

        /// <summary>
        /// Retorna o número de likes que o tópico possui
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public int CountLikes(int id)
        {
            int count = 0;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    count = db.SelectParam<UserLikeTopic>(fl => fl.TopicId == id && fl.Visible == true).Count();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return count;
        }

        /// <summary>
        /// Retorna o número de unlikes que o tópico possui
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public int CountUnLikes(int id)
        {
            int count = 0;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                count = db.SelectParam<UserLikeTopic>(fl => fl.TopicId == id && fl.Visible == false).Count();
            }
            return count;
        }

        /// <summary>
        /// Retornase o usuario deu like,unlike ou nenhum dos dois para um tópico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public bool? UserLiked(int id)
        {
            bool? liked = null;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var user = db.SelectParam<UserLikeTopic>(fl => fl.TopicId == id && fl.UserId == NimbusUser.UserId).FirstOrDefault();
                if (user == null)
                    liked = null;
                else
                    liked = user.Visible; //true = like ; false = unlike
            }
            return liked;
        }

        /// <summary>
        /// método para like e unlike de um topico
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public bool? LikeTopic(int id, string type)
        {
            bool? success = null;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var user = db.Where<UserLikeTopic>(u => u.UserId == NimbusUser.UserId && u.TopicId == id).FirstOrDefault();

                if (user == null)//novo
                {
                    UserLikeTopic userLike = new UserLikeTopic()
                    {
                        Visible = type == "like" ? true : false,
                        UserId = NimbusUser.UserId,
                        TopicId = id,
                        LikedOn = DateTime.Now
                    };
                    db.Insert<UserLikeTopic>(userLike);
                    success = userLike.Visible;
                }
                else //atualizar
                {
                    user.LikedOn = DateTime.Now;
                    if (type == "like")
                        user.Visible = true;
                    else
                        user.Visible = false;
                    db.Update<UserLikeTopic>(user, u => u.TopicId == user.TopicId && u.UserId == user.UserId);

                    success = user.Visible;

                }
            }
            return success;
        }

        /// <summary>
        /// Retorna as informações da categoria que o tópico pertence
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public CategoryBag CategoryTopic(int id)
        {
            CategoryBag category = new CategoryBag();

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                int channlId = db.SelectParam<Topic>(t => t.Id == id).Select(t => t.ChannelId).FirstOrDefault();
                if (channlId > 0)
                {
                    int catID = db.SelectParam<Channel>(ch => ch.Id == channlId && ch.Visible == true).Select(ch => ch.CategoryId).FirstOrDefault();
                    Category ctg = db.SelectParam<Category>(ct => ct.Id == catID).FirstOrDefault();
                    //category.ColorCode = ctg.ColorCode;
                    category.Id = ctg.Id;
                    category.ImageUrl = ctg.ImageUrl;
                    category.ImgTopChannel = ctg.ImageUrl.ToLower().Replace("category", "capachannel");
                    category.LocalizedName = ctg.LocalizedName;
                    category.Name = ctg.Name;
                }
            }


            return category;
        }

        /// <summary>
        /// Função para pegar os ads por categoria específica ou genérica.
        /// </summary>
        /// <param name="idCategory"></param>
        /// <returns></returns>
        [HttpGet]
        public List<Ad> ShowAds(int id = -1)
        {
            List<Ad> listAds = new List<Ad>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (id == -1) //não tem categoria, anuncio generico
                    {
                        listAds = db.SelectParam<Ad>(ad => ad.Visible == true);
                    }
                    else
                    {
                        listAds = db.SelectParam<Ad>(ad => ad.Visible == true && ad.CategoryId == id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return listAds;
        }

        //TODO: listar tópicos relacionados 
        /// <summary>
        /// Verifica se o usuario já fez o exame. Se já fez, retornar o objeto com data e nota
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [NonAction]
        [HttpGet]
        public UserExam ValidateExam(int id)
        {
            UserExam userExam = new UserExam();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    int idOrg = NimbusOrganization.Id;
                    userExam = db.SelectParam<UserExam>(ex => ex.ExamId == id && ex.UserId == NimbusUser.UserId
                                                                        && ex.OrganizationId == idOrg).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                userExam = null;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return userExam;
        }

        /// <summary>
        /// Add tags para os topicos
        /// </summary>
        /// <param name="topicID"></param>
        /// <param name="tagsList"></param>
        /// <returns></returns>
        [HttpPost]
        public List<string> AddTagsTopic(int id, List<string> tagsList)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool isOwner = IsOwner(id, "topic");
                            bool isManager = IsManager(id, "topic");
                            int channelID = db.SelectParam<Topic>(tp => tp.Id == id && tp.Visibility == true).Select(tp => tp.ChannelId).FirstOrDefault();

                            bool isPrivate = db.SelectParam<Channel>(ch => ch.Id == channelID).Select(p => p.IsPrivate).FirstOrDefault();
                            bool allOk = false;

                            if (isOwner == true || isManager == true)//usuario possui permissao
                            {
                                //colocar restrição para canal free
                                if (isPrivate == false)
                                {
                                    int countTag = db.SelectParam<TagTopic>(tp => tp.TopicId == id).Count();
                                    if (countTag <= 4)
                                    {
                                        tagsList = tagsList.Take(5 - (countTag + 1)).ToList();
                                        allOk = true;
                                    }
                                    else
                                    {
                                        allOk = false;
                                    }
                                }
                                else
                                {
                                    allOk = true;
                                }

                                //add as tags
                                if (allOk == true)
                                {
                                    List<Tag> tagsExist = new List<Tag>();
                                    tagsExist = ValidateTag(tagsList); //retorna as tags já existentes no sistema

                                    foreach (string item in tagsList)
                                    {
                                        if (tagsExist.Exists(tg => tg.TagName.ToLower() == item.ToLower()))
                                        {
                                            //já existe
                                            TagTopic tagChannel = new TagTopic
                                            {
                                                TopicId = id,
                                                TagId = tagsExist.Where(t => t.TagName.ToLower() == item.ToLower()).Select(t => t.Id).First(),
                                                Visible = true
                                            };
                                            db.Save(tagChannel);
                                        }
                                        else
                                        {
                                            //criar uma nova tag na tabela
                                            Tag tag = new Tag
                                            {
                                                TagName = item
                                            };
                                            db.Save(tag);

                                            TagTopic tagChannel = new TagTopic
                                            {
                                                TopicId = id,
                                                TagId = (int)db.GetLastInsertId(),
                                                Visible = true
                                            };
                                            db.Save(tagChannel);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "sem permissao"));
                            }
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
            return tagsList;
        }

        /// <summary>
        /// verifica se a tag já existe e valida a tag retirando o '#'
        /// </summary>
        /// <param name="listtag"></param>
        /// <returns>Lista de tags existentes</returns>
        [NonAction]
        [HttpGet]
        public List<Tag> ValidateTag(List<string> listtag)
        {
            List<Tag> returntags = new List<Tag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    string text = string.Empty;
                    foreach (string item in listtag)
                    {
                        int i = 0;
                        text = item;
                        while (text.StartsWith("#"))
                        {
                            text = text.Substring(i + 1);
                            i++;
                        }
                        Tag tag = new Tag();
                        tag = db.SelectParam<Tag>(tg => tg.TagName.ToLower() == text.ToLower()).FirstOrDefault();
                        if (tag != null)
                            returntags.Add(tag);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return returntags;
        }

        /// <summary>
        /// Classe criada para facilitar enviar as respostas do usuário
        /// </summary>
        public class UserAnswerExam
        {
            public List<int> Choice { get; set; }
            public int TopicId { get; set; }
        }

        /// <summary>
        /// Método que calcula a nota do usuário
        /// </summary>
        /// <param name="exam"></param>
        /// <returns></returns>
        public int FinishExam(UserAnswerExam exam)
        {
            int userGrade = 0;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var topic = db.Where<Topic>(t => t.Id == exam.TopicId && t.Visibility == true).Where(t => t != null).FirstOrDefault();

                var questions = topic.Question;

                var choiceUser = exam.Choice.ToList();

                for (int i = 0; i < questions.Count(); i++)
                {
                    var correct = questions[i].CorrectAnswer;

                    if (correct == choiceUser[i])
                    {
                        userGrade++;
                    }
                }

                //CHAMAR O ENVIAR MSG para o perfil do usuário
                var message = ClonedContextInstance<MessageController>();
                var channel = db.Where<Channel>(c => c.Id == topic.ChannelId && c.Visible == true).Where(c => c != null).FirstOrDefault();

                message.SendMessageUser(new Message()
                                            {
                                                ChannelId = channel.Id,
                                                Text =
                                                    "Você realizou a avaliação " + topic.Title + " em " +
                                                    DateTime.Now.ToString("dd/MM/yyyy") + " às " + DateTime.Now.ToString("HH:mm") + " horas, " +
                                                    "disponível no canal " + channel.Name + ".\n" +
                                                    "Sua nota foi " + userGrade + " com " + (((float)userGrade / (float)questions.Count()) * 100.0).ToString("F1") + "% de acerto.",
                                                SenderId = 1, //enviado pelo sistema
                                                Title = "Sua nota na avaliação " + topic.Title + " do canal " + channel.Name,
                                            }, NimbusUser.UserId);
            }

            return userGrade;
        }

        /// <summary>
        /// Método para deletar um tópico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public bool DeleteTopic(int id)
        {
            bool flag = false;

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                Topic topic = db.Where<Topic>(t => t.Id == id).FirstOrDefault();

                if (topic != null)
                {
                    bool allow = db.Where<Role>(r => r.UserId == NimbusUser.UserId && r.ChannelId == topic.ChannelId).Select(r => r.IsOwner).FirstOrDefault();

                    if (allow == true)
                    {
                        using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                db.Update<Comment>(new Comment { Visible = false }, cmt => cmt.TopicId == id);

                                topic.Visibility = false;
                                db.Update<Topic>(topic);

                                trans.Commit();

                                var notification = new Notifications.TopicNotification();
                                notification.DeleteTopic(topic);

                                flag = true;
                            }
                            catch (Exception)
                            {
                                trans.Rollback();
                                flag = false;
                                throw;
                            }
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }

            return flag;
        }
    }
}
