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
        /// Obtém a DatabaseFactory a partir das configurações no NimbusAppBus.
        /// </summary>
        public IDbConnectionFactory DatabaseFactory
        {
            get
            {
                if(_databaseFactory != null)
                    return _databaseFactory;
                else
                    return new OrmLiteConnectionFactory
                    (NimbusAppBus.Instance.Settings.DatabaseConnectionString,
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
        public T ClonedContextInstance<T>() where T : ApiController, new()
        {
            T instance = new T();
            instance.ControllerContext = this.ControllerContext;

            return instance;
        }

    }
}