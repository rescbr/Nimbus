using Nimbus.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;

namespace Nimbus.Web.API.Controllers
{
    public class SearchAPIController:NimbusApiController
    {
        /// <summary>
        /// método para pegar o nome da organização pelo host e saber se o usuário esta no portal 'nimbus' ou da org
        /// </summary>
        /// <returns></returns>
        public int CurrentOrgID()
        {
            int idOrg = 0;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                try                    
                {
                    string name = Request.Headers.Host;
                    idOrg = db.SelectParam<Organization>(org => org.Cname == name).Select(o => o.Id).FirstOrDefault();
                }
                catch (Exception)
                {
                    idOrg = 1; //nimbus
                }
            }
            return idOrg;
        }

        public SearchAPIModel Search(SearchType typeSearch)
        {
            SearchAPIModel search = new SearchAPIModel ();


            return search;
        }



    }
}