using Nimbus.DB.ORM;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Net;
using ServiceStack.OrmLite;
using System.Net.Http;
using System.Linq;


namespace Nimbus.Web.API.Controllers
{
    public class CategoryController:NimbusApiController
    {
        /// <summary>
        /// método que lista todas as categorias existentes no nimbus
        /// </summary>
        /// <returns></returns>
        [Authorize]
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
        /// Método que retorna a string da img de CAPA/TOPO do canal de acordo com a 
        /// categoria que ele pertence
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public string GetImgTopChannel(int id) 
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    string url;
                    url = db.SelectParam<ImgTopChannel>(img => img.CategoryId == id).Select(i => i.UrlImg).FirstOrDefault();
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