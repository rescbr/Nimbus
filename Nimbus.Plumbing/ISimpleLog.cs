using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    /// <summary>
    /// ISimpleLog é o logger utilizado na inicialização do NimbusAppBus e deve ser criado pelo Host.
    /// </summary>
    public interface ISimpleLog
    {
        void Log(string origin, string message);
    }
}
