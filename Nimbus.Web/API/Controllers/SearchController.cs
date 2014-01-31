using Nimbus.Model.Bags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net.Http;
using ServiceStack.OrmLite;
using System.Net;
using Nimbus.Model;
using Nimbus.Extensions;

namespace Nimbus.Web.API.Controllers
{
    [NimbusAuthorize]
    public class SearchController:NimbusApiController
    {

        /// <summary>
        /// Search para Usuarios
        /// </summary>
        /// <param name="q">Query de Pesquisa</param>
        /// <returns></returns>
        [HttpGet]
        public List<SearchBag> SearchUser(string q)
        {
            List<SearchBag> usersFound = new List<SearchBag>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
                using (var db = DatabaseFactory.OpenDbConnection())
                {//restringir a busca pela organizaçao

                    var users = db.Query<Model.ORM.User>(
                          @"
                            SELECT [User].[Id], [User].[FirstName], [User].[LastName],
                                   [User].[Occupation], [User].[Interest], [User].[AvatarUrl]
                            FROM [User]
                            WHERE ((([User].[FirstName] LIKE @strQuery) OR 
			                            ([User].[LastName] LIKE @strQuery) OR 
			                            ([User].[Occupation] LIKE @strQuery) OR 
			                            ([User].[Interest] LIKE @strQuery))) ",
                          new { strQuery = "%" + q + "%" });

                    foreach (var item in users)
                    {
                        string description = !string.IsNullOrEmpty(item.Interest) ? item.Interest : item.Occupation;
                        description = !string.IsNullOrEmpty(description) ? description : item.Experience;
                        description = !string.IsNullOrEmpty(description) ? description : "Sem informações adicionais.";

                        SearchBag bag = new SearchBag()
                        {
                            Description = description,
                            IdItem = item.Id,
                            Title = item.FirstName + " " + item.LastName,
                            UrlImage = item.AvatarUrl,
                            TypeSearch = "user",
                            ItemPageUrl = "userprofile"
                        };
                        usersFound.Add(bag);
                    }
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + q + "'"));
            }
            return usersFound;
        }
        
        /// <summary>
        /// Search para Topicos
        /// </summary>
        /// <param name="q">Query de Pesquisa</param>
        /// <returns></returns>
        [HttpGet]
        public List<SearchBag> SearchTopic(string q)
        {
            List<Topic> topics = new List<Topic>();
            List<SearchBag> topicsFound = new List<SearchBag>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //restringe a busca para o conteudo da organizacao
                    IEnumerable<int> idChannelTopic = db.Where<Channel>(ch => ch.Visible == true && ch.OrganizationId == idOrg).Select(ch => ch.Id);
                    
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

                            topics = db.Where<Topic>(tp => (tp.Text.Contains(q) ||
                                                                 tp.Title.Contains(q) ||
                                                                 tp.Description.Contains(q))
                                                                 && tp.Visibility == true && idChannelTopic.Contains(tp.ChannelId));
                        }
                        else
                        {
                            topics = db.Where<Topic>(tp => (tp.Text.Contains(q) ||
                                                                 tp.Title.Contains(q) ||
                                                                 tp.Description.Contains(q))
                                                                 && tp.Visibility == true && idChannelTopic.Contains(tp.ChannelId));
                        }
                    }
                }
                foreach (var item in topics)
                {
                    SearchBag bag = new SearchBag()
                    {
                        Description = item.Description,
                        IdItem = item.Id,
                        Title = item.Title,
                        TypeSearch = "topic",
                        UrlImage = item.ImgUrl,
                        ItemPageUrl = "topic"
                    };
                    topicsFound.Add(bag);
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + q + "'"));
            }
            return topicsFound;
        }

        /// <summary>
        /// Retorna os canais encontrados para a palavra/tag buscada 
        /// </summary>
        /// <param name="q">query de pesquisa</param>
        /// <returns></returns>
        [HttpGet]
        public List<SearchBag> SearchChannel(string q)
        {
            List<SearchBag> channelsFound = new List<SearchBag>();
            List<Channel> channels = new List<Channel>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
              
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
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

                            var idChannels = db.Where<TagChannel>(tgc => tgc.TagId == tagID).Select(tgc => tgc.ChannelId);

                            channels = idChannels.Select(ch => db.Where<Channel>(c => c.Visible == true && c.OrganizationId == idOrg && c.Id == ch)
                                                                .FirstOrDefault()).Where(ch => ch != null).ToList();
                        }
                        else
                        {
                            //pegar canais da categoria
                            int idCat = db.SelectParam<Category>(ct => ct.Name.ToLower() == q.ToLower()).Select(ct => ct.Id).FirstOrDefault();
                            if (idCat > 0)
                            {
                                channels = db.SelectParam<Channel>(chn => (chn.Name.Contains(q) || chn.CategoryId == idCat)
                                                                        && chn.Visible == true && chn.OrganizationId == idOrg);
                            }
                            else
                            {
                                channels = db.SelectParam<Channel>(chn => chn.Name.Contains(q) && chn.Visible == true && chn.OrganizationId == idOrg);
                            }
                        }
                        foreach (var channel in channels)
                        {
                            SearchBag bag = new SearchBag();
                            bag.IdItem = channel.Id;
                            bag.Title = channel.Name;
                            bag.Description = channel.Description;
                            bag.UrlImage = channel.ImgUrl.ToLower().Replace("/capachannel/", "/category/");
                            bag.TypeSearch = "channel";
                            bag.ItemPageUrl = "channel";
                            channelsFound.Add(bag);
                        }
                    }                
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + q + "'"));
            }
            return channelsFound;
        }

        /// <summary>
        /// Método de buscar td
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        [HttpGet]
        public List<SearchBag> SearchAll(string q )
        {
            List<SearchBag> listUser = SearchUser(q);
            List<SearchBag> listTopic = SearchTopic(q);
            List<SearchBag> listChannel = SearchChannel(q);

            List<SearchBag> listFinal = new List<SearchBag>();
            listFinal.AddRange(listChannel);
            listFinal.AddRange(listTopic);
            listFinal.AddRange(listUser);

            listFinal.Shuffle();
            return listFinal;
        }
    }
}