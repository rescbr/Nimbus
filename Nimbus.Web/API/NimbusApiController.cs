using Nimbus.Plumbing;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Nimbus.Web.API
{
    public class NimbusApiController : ApiController
    {
        private Model.ORM.Organization _nimbusOrg = null;
        public Model.ORM.Organization NimbusOrganization
        {
            get
            {
                if (_nimbusOrg != null)
                {
                    return _nimbusOrg;
                }
                else
                {
                    Model.ORM.Organization org;
                    string host = Request.Headers.Host.Split(':')[0]; //remove porta
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        org = (db.Where<Model.ORM.Organization>(o => o.Cname == host)
                            .FirstOrDefault() as Model.ORM.Organization);
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
        public T ClonedContextInstance<T>() where T : NimbusApiController, new()
        {
            T instance = new T();
            instance.ControllerContext = this.ControllerContext;
            instance.NimbusUser = this.NimbusUser;
            instance.NimbusOrganization = this.NimbusOrganization;

            return instance;
        }

    }
}