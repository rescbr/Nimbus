using Nimbus.Plumbing.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public class NimbusAppBus : Nimbus.Plumbing.Interface.INimbusAppBus
    {
        private NimbusSettings _settings;
        public NimbusSettings Settings
        {
            get { return _settings; }
        }

        private INimbusDebugAutoAttach _nimbusDebugAutoAttach;
        public INimbusDebugAutoAttach NimbusDebugAutoAttach
        {
            get { return _nimbusDebugAutoAttach; }
        }

        public NimbusAppBus(NimbusSettings settings)
        {
            _settings = settings;
            _nimbusDebugAutoAttach = new NimbusDebugAutoAttach(this);
        }
    }
}
