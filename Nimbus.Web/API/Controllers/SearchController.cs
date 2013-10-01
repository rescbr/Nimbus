using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;
using System.Web.Http;
using Nimbus.DB.ORM;
using System.Net.Http;
using System.Net;

namespace Nimbus.Web.API.Controllers
{
    public class SearchController:NimbusApiController
    {

        /// <summary>
        /// Retorna os canais encontrados para a palavra/tag buscada 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<Channel> SearchInChannel(string text)
        {          
            List<Channel> channel = new List<Channel>();
            if (!string.IsNullOrEmpty(text))
            {
                int idOrg = NimbusOrganization.Id;
                try
                {
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        //verificar se é tag
                        int i = 0;
                        while (text.StartsWith("#"))
                        {
                            text = text.Substring(i + 1);
                            i++;
                        }
                        //pegar canais da categoria
                        int idCat = db.SelectParam<Category>(ct => ct.Name.ToLower() == text.ToLower()).Select(ct => ct.Id).FirstOrDefault();
                        if (idCat > 0)
                        {
                            channel = db.SelectParam<Channel>(chn => (chn.Name.Contains(text) || chn.CategoryId == idCat)
                                                                    && chn.Visible == true && chn.OrganizationId == idOrg);
                        }
                        else
                        {
                            channel = db.SelectParam<Channel>(chn => chn.Name.Contains(text) && chn.Visible == true && chn.OrganizationId == idOrg);
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,ex));
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent,"Nenhum registro encontrado para '"+text+"'"));
            }
            return channel;
        }

        /// <summary>
        /// Retorna os topicos encontrados para a palavra/tag buscada 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<Topic> SearchInTopic(string text)
        {
            List<Topic> topic = new List<Topic>();
            if (!string.IsNullOrEmpty(text))
            {
                int idOrg = NimbusOrganization.Id;
                try
                {
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        //verificar se é tag
                        int i = 0;
                        while (text.StartsWith("#"))
                        {
                            text = text.Substring(i + 1);
                            i++;
                        }
                        //pegar canais da categoria
                        int idCat = db.SelectParam<Category>(ct => ct.Name.ToLower() == text.ToLower()).Select(ct => ct.Id).FirstOrDefault();
                        List<int> idChannel = new List<int>();
                        if (idCat > 0)
                        {
                            //restringe a busca para o conteudo da organizacao  e com a categoria
                            idChannel = db.SelectParam<Channel>(ch => ch.CategoryId == idCat && ch.Visible == true && ch.OrganizationId == idOrg).Select(ch => ch.Id).ToList();

                            topic = db.SelectParam<Topic>(tp => (tp.Text.Contains(text) ||
                                                                 tp.Title.Contains(text) ||
                                                                 tp.Description.Contains(text) ||
                                                                 tp.Question.Exists(q => q.TextQuestion.Contains(text) || q.ChoicesAnswer.Values.Contains(text))
                                                                 ) && tp.Visibility == true && idChannel.Contains(tp.ChannelId));
                        }
                        else
                        {
                            //restringe a busca para o conteudo da organizacao
                            idChannel = db.SelectParam<Channel>(ch => ch.Visible == true && ch.OrganizationId == idOrg).Select(ch => ch.Id).ToList();

                            topic = db.SelectParam<Topic>(tp => (tp.Text.Contains(text) ||
                                                                 tp.Title.Contains(text) ||
                                                                 tp.Description.Contains(text) ||
                                                                 tp.Question.Exists(q => q.TextQuestion.Contains(text) || q.ChoicesAnswer.Values.Contains(text))
                                                                 ) && tp.Visibility == true && idChannel.Contains(tp.ChannelId));
                        }
                    }
                }
                catch(Exception ex)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                }
            }
            else
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NoContent, "Nenhum registro encontrado para '" + text + "'"));
            }
            return topic;
        }

        
      
    }
}