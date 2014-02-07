using Nimbus.Model.ORM;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net;
using ServiceStack.OrmLite;
using System.Net.Http;
using System.Linq;


namespace Nimbus.Web.API.Controllers
{
    [NimbusAuthorize]
    public class CategoryController:NimbusApiController
    {
        /// <summary>
        /// método que lista todas as categorias existentes no nimbus
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Category> showAllCategory()
        {       
            try
            {
                List<Category> listCat = new List<Category>();
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    listCat = db.Select<Category>();
                }

                return listCat;
            }
            catch(Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

        }

        /// <summary>
        /// método que lista todas as categorias existentes no nimbus para a pagina de categorias
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Category> showCategoryToPage(int skip = 0)
        {
            try
            {
                List<Category> listCat = new List<Category>();
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    listCat = db.Select<Category>().Skip(skip).Take(10).ToList();
                }

                return listCat;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

        }

        /// <summary>
        /// Método que retorna a string da img de CAPA/TOPO do canal de acordo com a 
        /// categoria que ele pertence
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public string GetImgTopChannel(int id) 
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    string url;
                    url = db.SelectParam<Category>(img => img.Id == id).Select(i => i.ImageUrl).FirstOrDefault();
                    url = url.ToLower().Replace("/category", "/capachannel");
                    return url;
                }
            }
            catch (Exception ex)
            {
                 throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

    }
}