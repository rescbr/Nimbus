using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public interface INimbusCacheProvider
    {
        /// <summary>
        /// Obtém o objeto no cache.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// Armazena um objeto no cache.
        /// Retorna mesmo se os dados ainda não forem replicados para todos os nós.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Store(string key, object value);

        /// <summary>
        /// Armazena um objeto no cache e aguarda replicação para todos os nós.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void StoreAndReplicate(string key, object value);

        /// <summary>
        /// Remove um objeto armazenado no cache, aguardando replicação.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>Objeto armazenado</returns>
        object DeleteAndReplicate(string key, object value);
        
    }
}
