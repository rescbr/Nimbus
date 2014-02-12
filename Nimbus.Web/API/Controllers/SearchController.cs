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
using Nimbus.Web.Utils;

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
                            WHERE ((([User].[FirstName] COLLATE Latin1_general_CI_AI LIKE @search COLLATE Latin1_general_CI_AI) OR 
			                            ([User].[LastName] COLLATE Latin1_general_CI_AI LIKE @search COLLATE Latin1_general_CI_AI) OR 
			                            ([User].[Occupation] COLLATE Latin1_general_CI_AI LIKE @search COLLATE Latin1_general_CI_AI) OR 
			                            ([User].[Interest] COLLATE Latin1_general_CI_AI LIKE @search COLLATE Latin1_general_CI_AI))) ",
                          new { search = "%" + q + "%" });

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
            var topics = new List<Model.ORM.Topic>();
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
                        q = q.TrimStart('#');
                        topics = db.Query<Model.ORM.Topic>(@"
                            select distinct [Topic].*
                            from [Tag], [TagChannel], [Topic], [Channel]
                            where [Tag].TagName collate Latin1_general_CI_AI like @search collate Latin1_general_CI_AI
                                  and [Tag].[Id] = [TagChannel].TagId and [Topic].ChannelId = [TagChannel].ChannelId and [Topic].Visibility = 1
                                  and [Channel].[Id] = [TagChannel].ChannelId and [Channel].Visible = 1 and [Channel].OrganizationId = @organization",
                            new { search = "%" + q + "%", organization = idOrg });
                        
                    }
                    else
                    {
                        topics = db.Query<Model.ORM.Topic>(
@"select distinct [Topic].*
from [Topic], [Channel]
where ([Topic].[Text] collate Latin1_general_CI_AI like @search collate Latin1_general_CI_AI
        or [Topic].[Title] collate Latin1_general_CI_AI like @search collate Latin1_general_CI_AI
	    or [Topic].[Description] collate Latin1_general_CI_AI like @search collate Latin1_general_CI_AI)
	    and [Topic].[Visibility] = 1 and [Channel].[Id] = [Topic].ChannelId and [Channel].[OrganizationId] = @organization",
     new { search = "%" + q + "%", organization = idOrg });
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
            var channels = new List<Model.ORM.Channel>();
            if (!string.IsNullOrEmpty(q))
            {
                int idOrg = NimbusOrganization.Id;
              
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        //verificar se é tag
                        if (q.StartsWith("#"))
                        {
                            q = q.TrimStart('#');
                            channels = db.Query<Model.ORM.Channel>(@"
                                select distinct [Channel].*
                                from [Tag], [TagChannel], [Channel]
                                where [Tag].TagName collate Latin1_general_CI_AI like @search collate Latin1_general_CI_AI
                                        and [Tag].[Id] = [TagChannel].TagId and [Channel].[Id] = [TagChannel].ChannelId 
		                                and [Channel].Visible = 1 and [Channel].OrganizationId = @organization",
                                new { search = "%" + q + "%", organization = idOrg });
                        }
                        else
                        {
                            channels = db.Query<Model.ORM.Channel>(@"
                                select distinct [Channel].*
                                from [Channel]
                                where [Channel].Name collate Latin1_general_CI_AI like @search collate Latin1_general_CI_AI
		                              and [Channel].Visible = 1 and [Channel].OrganizationId = @organization",
                                new { search = "%" + q + "%", organization = idOrg });
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

        public class ChnByCategoryHtmlWrapper
        {
            public int Count { get; set; }
            public string Html { get; set; }
        }

        [HttpGet]
        public ChnByCategoryHtmlWrapper AbstChannelHtml(int id, string nameCat)
        {
            var rz = new RazorTemplate();
            string html = "";
            List<Nimbus.Model.ORM.Channel> channel = new List<Nimbus.Model.ORM.Channel>();

                channel = ChannelByCategory(id, nameCat);
           
            foreach (var item in channel)
            {
                html += rz.ParseRazorTemplate<Nimbus.Model.ORM.Channel>
                    ("~/Website/Views/ChannelPartials/ChannelPartial.cshtml", item);
            }
            return new ChnByCategoryHtmlWrapper { Html = html, Count = channel.Count };
        }


        /// <summary>
        /// Retorna os canais encontrados para a categoria escolhida 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Nimbus.Model.ORM.Channel> ChannelByCategory(int id, string nameCat)
        {
            List<Nimbus.Model.ORM.Channel> channels = new List<Nimbus.Model.ORM.Channel>();

            if (id > 0)
            {
                int idOrg = NimbusOrganization.Id;

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //pegar canais da categoria
                    bool idCatValid = db.Where<Category>(ct => ct.Id == id).Exists(c => c.Id == id);
                    if (idCatValid == true)
                    {
                        channels = db.SelectParam<Nimbus.Model.ORM.Channel>(chn => chn.Visible == true && chn.OrganizationId == idOrg && chn.CategoryId == id);
                        foreach (var item in channels)
                        {
                            item.ImgUrl = item.ImgUrl.ToLower().Replace("capachannel","category");
                        }
                    }
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + nameCat + "'"));
            }
            return channels;
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