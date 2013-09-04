using Nimbus.Plumbing.Interface;
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
        /// Obtém o NimbusAppBus contexto OwinApp.
        /// </summary>
        public INimbusAppBus NimbusAppBus
        {
            get
            {
                return base.Configuration.Properties["NimbusAppBus"] as INimbusAppBus;
            }
        }

        /// <summary>
        /// Obtém o NimbusUser da requisição atual.
        /// </summary>
        public NimbusUser NimbusUser
        {
            get
            {
                if (User.Identity.AuthenticationType == "NimbusUser")
                {
                    return ((User.Identity) as NimbusUser);
                }
                else throw new Exception("User.Identity.AuthenticationType is not NimbusUser.");
            }
        }

        /// <summary>
        /// Obtém a DatabaseFactory a partir das configurações no NimbusAppBus.
        /// </summary>
        public IDbConnectionFactory DatabaseFactory
        {
            get
            {
                return new OrmLiteConnectionFactory
                    (NimbusAppBus.Settings.DatabaseConnectionString,
                    SqlServerDialect.Provider);
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