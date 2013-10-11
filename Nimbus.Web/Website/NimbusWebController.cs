using Nimbus.Plumbing;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website
{
    public class NimbusWebController : Controller
    {
        private DB.ORM.Organization _nimbusOrg = null;
        public DB.ORM.Organization NimbusOrganization
        {
            get
            {
                if (_nimbusOrg != null)
                {
                    return _nimbusOrg;
                }
                else
                {
                    DB.ORM.Organization org;
                    string host = Request.Headers["Host"].Split(':')[0]; //remove porta
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        org = (db.Where<DB.ORM.Organization>(o => o.Cname == host)
                            .FirstOrDefault() as DB.ORM.Organization);
                        if(org == null)
                            org = db.Where<DB.ORM.Organization>(o => o.Id == 1).FirstOrDefault();
                    }
                    
                    return org;
                }
            }

            set
            {
                _nimbusOrg = value;
            }
        }


        /// <summary>
        /// For test purposes only!
        /// </summary>
        private NimbusUser _nimbusUser = null;
        /// <summary>
        /// Obtém o NimbusUser da requisição atual.
        /// </summary>
        public NimbusUser NimbusUser
        {
            get
            {
                if (_nimbusUser != null)
                {
                    return _nimbusUser;
                }
                else
                {
                    if (User.Identity.AuthenticationType == "NimbusUser")
                    {
                        return ((User.Identity) as NimbusUser);
                    }
                    else throw new Exception("User.Identity.AuthenticationType is not NimbusUser.");
                }

            }

            set
            {
                _nimbusUser = value;
            }
        }

        /// <summary>
        /// For test purposes only!
        /// </summary>
        private IDbConnectionFactory _databaseFactory = null;
        /// <summary>
        /// Obtém a DatabaseFactory a partir das configurações.
        /// </summary>
        public IDbConnectionFactory DatabaseFactory
        {
            get
            {
                if (_databaseFactory != null)
                    return _databaseFactory;
                else
                    return new OrmLiteConnectionFactory
                    (NimbusConfig.DatabaseConnection,
                    SqlServerDialect.Provider);
            }
            set
            {
                _databaseFactory = value;
            }
        }

        /// <summary>
        /// Clona o contexto da requisição atual e cria uma nova instância da ApiController T.
        /// Utilize para efetuar chamadas à API sem realizar RPC.
        /// </summary>
        /// <typeparam name="T">Tipo da nova instância ApiController</typeparam>
        /// <returns>Nova instância da ApiController T com o mesmo contexto da requisição atual.</returns>
        [NonAction]
        public T ClonedContextInstance<T>() where T : System.Web.Http.ApiController, new()
        {
            T instance = new T();
            //instance.ControllerContext = this.ControllerContext;
            instance.ControllerContext = new System.Web.Http.Controllers.HttpControllerContext();

            return instance;
        }

        protected internal new ViewResult View()
        {
            //retorna view default com nome do controller
            return base.View(GetViewFromType(this.GetType()));
        }

        protected internal new ViewResult View(object model)
        {
            return base.View(GetViewFromType(model.GetType()), model);
        }
        protected internal new ViewResult View(string viewName)
        {
            return base.View(String.Format("{0}/{1}.cshtml",
                GetNamespacePathFromType(this.GetType()), viewName));
        }
        protected internal new ViewResult View(string viewName, object model)
        {
            if (model == null) return View(viewName);

            return base.View(String.Format("{0}/{1}.cshtml",
                GetNamespacePathFromType(this.GetType()), viewName), model);
        }

        internal string GetViewFromType(Type objType)
        {
            if (objType == null) return null;
            if (!objType.Namespace.StartsWith("Nimbus.Web"))
                return "~/SharedViews/" + objType.Name + ".cshtml";

            string viewName;

            string modelSuffix = "Model";
            string controllerSuffix = "Controller";

            if (objType.Name.EndsWith(modelSuffix))
                viewName = objType.Name.Remove(objType.Name.Length - modelSuffix.Length);
            else if (objType.Name.EndsWith(controllerSuffix))
                viewName = objType.Name.Remove(objType.Name.Length - controllerSuffix.Length);
            else
                viewName = objType.Name;

            return String.Format("{0}/{1}.cshtml", GetNamespacePathFromType(objType), viewName);

        }

        internal string GetNamespacePathFromType(Type objType)
        {
            List<string> sepType = objType.Namespace.Split(Type.Delimiter).ToList();
            string viewNamespace = sepType[2]; //Nimbus.Web.XXXX

            return String.Format("~/{0}/Views", viewNamespace);
        }
    }
}