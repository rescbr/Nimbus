﻿using System;
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
            try
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
                                    topic.ImgUrl = db.SelectParam<Category>(ct => ct.Id == 1).Select(ct => ct.ImageUrl).FirstOrDefault();
                                }
                                topic.CreatedOn = DateTime.Now;
                                topic.LastModified = DateTime.Now;
                                topic.Visibility = true;

                                if (string.IsNullOrEmpty(topic.Price.ToString()))
                                {
                                    topic.Price = 0;
                                }

                                topic.Description = HttpUtility.HtmlEncode(topic.Description);
                                topic.Text = HttpUtility.HtmlEncode(topic.Text);
                                topic.Title = HttpUtility.HtmlEncode(topic.Title);
                                topic.UrlVideo = HttpUtility.HtmlEncode(topic.UrlVideo);

                                if (topic.TopicType == Model.Enums.TopicType.exam)
                                {
                                    foreach (var item in topic.Question)
                                    {
                                        item.TextQuestion = HttpUtility.HtmlEncode(item.TextQuestion);
                                        //colocar encode nas opçoes
                                    }
                                }

                                db.Save(topic);
                                topic.Id = (int)db.GetLastInsertId();
                                trans.Commit();
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
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
            try
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
                                    tpc.ImgUrl = "/" + db.SelectParam<Category>(ct => ct.Id == 1).Select(ct => ct.ImageUrl).FirstOrDefault();
                                }
                                tpc.LastModified = DateTime.Now;
                                //tpc.Question = topic.Question;
                                tpc.Text = topic.Text;
                                tpc.Title = topic.Title;
                                tpc.UrlCapa = topic.UrlCapa != null ? topic.UrlCapa : tpc.UrlCapa;
                                tpc.UrlVideo = topic.UrlVideo;
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
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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

                        topic.Title = HttpUtility.HtmlDecode(topic.Title);                       
                        topic.Description = HttpUtility.HtmlDecode(topic.Description);
                        topic.Text = HttpUtility.HtmlDecode(topic.Text);

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
                                foreach (Nimbus.Model.Question item in topic.Question)
                                {
                                    item.TextQuestion = HttpUtility.HtmlDecode(item.TextQuestion);
                                    //colocar p opçoes e arrumar p retornar esse
                                }
                            }
                            #endregion
                        }
                        if (topic.TopicType == Nimbus.Model.Enums.TopicType.video)
                        {                           
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
                                    topic.UrlVideo = "//www.youtube.com/embed/" + param;
                                }
                                else
                                {
                                    //já foi salvo da forma correta
                                    int posicao = url.IndexOf("http:") + 5;
                                    param = url.Substring(posicao);
                                    topic.UrlVideo = param;
                                }
                            }
                           

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return topic;
        }

        /// <summary>
        /// carregar informações gerais dos trending tópicos
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<TopicBag> ShowTTopic(int id)
        {
            List<TopicBag> topicList = new List<TopicBag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    ICollection<int> idChannel;
                    if (id == 0)
                    {
                        idChannel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.OrganizationId == NimbusOrganization.Id)
                                                            .Select(ch => ch.Id).ToList();
                    }
                    else
                    {
                        idChannel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.OrganizationId == NimbusOrganization.Id && ch.CategoryId == id)
                                                            .Select(ch => ch.Id).ToList();

                    }
                    //TODO: COLOCAR P APARECER DE DFATO SO OS TRENDING
                    ICollection<Topic> tpcs = db.SelectParam<Topic>(tp => tp.Visibility == true && idChannel.Contains(tp.ChannelId)).ToList();
                    foreach (var item in tpcs)
                    {
                        TopicBag topic = new TopicBag()
                        {
                            Title = HttpUtility.HtmlDecode(item.Title),
                            //Description = RemoveHTMLString.StripTagsCharArray(topic.Description); 
                            Description = HttpUtility.HtmlDecode(item.Description),
                            Text = HttpUtility.HtmlDecode(item.Text),
                            AuthorId = item.AuthorId,
                            ChannelId = item.ChannelId,
                            //Count
                            CreatedOn = item.CreatedOn,
                            Id = item.Id,
                            ImgUrl = item.ImgUrl,
                            LastModified = item.LastModified,
                            Price = item.Price,
                            Question = item.Question,
                            TopicType = item.TopicType,
                            UrlCapa = item.UrlCapa,
                            UrlVideo = item.UrlVideo,
                            Visibility = item.Visibility

                        };
                        topicList.Add(topic);
                    }

                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return topicList;
        }

        /// <summary>
        /// Search para Topicos
        /// </summary>
        /// <param name="q">Query de Pesquisa</param>
        /// <returns></returns>
        [HttpGet]
        public List<Topic> SearchTopic(string q)
        {
            List<Topic> topics = new List<Topic>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
                try
                {
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        List<int> idChannelTopic = new List<int>();
                        //restringe a busca para o conteudo da organizacao
                        idChannelTopic = db.SelectParam<Channel>(ch => ch.Visible == true && ch.OrganizationId == idOrg).Select(ch => ch.Id).ToList();

                        //verificar se é tag
                        if (q.StartsWith("#"))
                        {
                            int i = 0;
                            while (q.StartsWith("#"))
                            {
                                q = q.Substring(i + 1);
                                i++;
                            }
                            int tagID = db.SelectParam<Tag>(tag => tag.TagName.ToLower() == q.ToLower()).Select(tag => tag.Id).FirstOrDefault();
                            List<int> idTopics = db.SelectParam<TagTopic>(tgc => tgc.TagId == tagID).Select(tgc => tgc.TopicId).ToList();

                            foreach (int item in idTopics)
                            {
                                Topic tpc = db.SelectParam<Topic>(tp => idChannelTopic.Contains(tp.ChannelId) && tp.Visibility == true && idTopics.Contains(tp.Id)).FirstOrDefault();
                                if (tpc != null)
                                    topics.Add(tpc);
                            }
                        }
                        else
                        {
                            //pegar canais da categoria
                            int idCat = db.SelectParam<Category>(ct => ct.Name.ToLower() == q.ToLower()).Select(ct => ct.Id).FirstOrDefault();
                            if (idCat > 0)
                            {
                                //restringe a busca para o conteudo da organizacao MAS com a categoria
                                idChannelTopic = db.SelectParam<Channel>(ch => ch.CategoryId == idCat && ch.Visible == true && ch.OrganizationId == idOrg).Select(ch => ch.Id).ToList();

                                topics = db.SelectParam<Topic>(tp => (tp.Text.Contains(q) ||
                                                                     tp.Title.Contains(q) ||
                                                                     tp.Description.Contains(q) ||
                                                                     tp.Question.Exists(question => question.TextQuestion.Contains(q) || question.ChoicesAnswer.Values.Contains(q))
                                                                     ) && tp.Visibility == true && idChannelTopic.Contains(tp.ChannelId));
                            }
                            else
                            {
                                topics = db.SelectParam<Topic>(tp => (tp.Text.Contains(q) ||
                                                                     tp.Title.Contains(q) ||
                                                                     tp.Description.Contains(q) ||
                                                                     tp.Question.Exists(question => question.TextQuestion.Contains(q) || question.ChoicesAnswer.Values.Contains(q))
                                                                     ) && tp.Visibility == true && idChannelTopic.Contains(tp.ChannelId));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + q + "'"));
            }
            return topics;
        }

        #endregion

        /// <summary>
        /// método de exibir tópicos em resumo, filtra por categoriam modificação e popularidade
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<TopicBag> AbstTopic(string viewBy = null, int channelID = 0, int categoryID = 0)
        {
            List<TopicBag> tpcList = new List<TopicBag>();
            try
            {
                List<int> idChannel = new List<int>();
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
                            idChannel.Add(channelID);
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
                                                                            .Select(ch => ch.Id).ToList();
                    }
                    //busca todos os canais em geral da organizacao                      
                    else
                    {
                        idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true)
                                                                              .Select(ch => ch.Id).ToList();
                    }

                    if (idChannel.Count > 0)
                    {

                        List<Topic> topic = db.Where<Topic>(tp => tp.Visibility == true);
                        topic = topic.Where(t => idChannel.Contains(t.ChannelId)).ToList();

                        if (topic.Count > 0)
                        {
                            foreach (var item in topic)
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
                                    Count = count

                                };
                                tpcList.Add(bag);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            if (viewBy.ToLower() == "bymodified")
                return tpcList.OrderBy(tp => tp.LastModified).ToList();
            else if (viewBy.ToLower() == "bypopularity")
                return tpcList.OrderBy(tp => tp.Count).ToList();
            else
                return tpcList;
        }

        /// <summary>
        /// método de favoritar/desfavoritar o tópico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        public bool TopicFavorite(int id)
        {
            bool flag = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    UserTopicFavorite user = new UserTopicFavorite();
                    user = db.SelectParam<UserTopicFavorite>(us => us.UserId == NimbusUser.UserId && us.TopicId == id).FirstOrDefault();
                    UserTopicFavorite usrFavorite = new UserTopicFavorite();
                    if (user == null) //nunca favoritou
                    {
                        usrFavorite.UserId = NimbusUser.UserId;
                        usrFavorite.TopicId = id;
                        usrFavorite.FavoritedOn = DateTime.Now;
                        usrFavorite.Visible = true;
                        db.Insert(usrFavorite);
                        flag = true;
                    }
                    else
                    {
                        user.FavoritedOn = DateTime.Now;
                        user.Visible = (user.Visible == true) ? false : true;
                        db.Update(user);
                        flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                flag = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return flag;
        }

        /// <summary>
        /// Add/retirar tópico da lista de ler mais tarde 
        /// </summary>
        /// <param name="topicID"></param>
        /// <param name="readOn"></param>
        /// <returns></returns>
        [HttpPost]
        public bool ReadChannelLater(int id, DateTime? readOn = null)
        {
            bool operation = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //se ja existir = retirar//se não existir = criar
                    UserTopicReadLater user = db.SelectParam<UserTopicReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.TopicId == id).FirstOrDefault();
                    if (user != null)
                    {
                        user.Visible = false;
                        user.ReadOn = null;
                    }
                    else
                    {
                        user.Visible = true;
                        user.UserId = NimbusUser.UserId;
                        user.ReadOn = readOn;
                        user.TopicId = id;
                    }
                    db.Save(user);
                }
                operation = true;
            }
            catch (Exception ex)
            {
                operation = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return operation;
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
            try
            {
                //ver permissao p vizualizar => se é pago = ter pagado, se é privado = ser aceito
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Topic tpc = db.SelectParam<Topic>(tp => tp.Id == id).FirstOrDefault();
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
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return allow;
        }

        /// <summary>
        /// Método de retornar o numero de favoritos de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [HttpGet]
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
        /// Retorna as informações da categoria que o tópico pertence
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [HttpGet]
        public CategoryBag CategoryTopic(int id)
        {
            CategoryBag category = new CategoryBag();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    int channlId = db.SelectParam<Topic>(t => t.Id == id).Select(t => t.ChannelId).FirstOrDefault();
                    if (channlId > 0)
                    {
                        int catID = db.SelectParam<Channel>(ch => ch.Id == channlId && ch.Visible == true).Select(ch => ch.CategoryId).FirstOrDefault();
                        Category ctg = new Category();
                        ctg = db.SelectParam<Category>(ct => ct.Id == catID).FirstOrDefault();
                        //category.ColorCode = ctg.ColorCode;
                        category.Id = ctg.Id;
                        category.ImageUrl = ctg.ImageUrl;
                        category.ImgTopChannel = db.SelectParam<ImgTopChannel>(c => c.CategoryId == ctg.Id).Select(c => c.UrlImg).FirstOrDefault();
                        category.LocalizedName = ctg.LocalizedName;
                        category.Name = ctg.Name;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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


    }
}
