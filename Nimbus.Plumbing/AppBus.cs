using Nimbus.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public class NimbusAppBus
    {
        private static NimbusAppBus _instance = null;
        public static NimbusAppBus Instance
        {
            get
            {
                if (_instance == null) throw new Exception("Run Init() first.");
                return _instance;
            }
        }

        private NimbusAppBus() { }

        private NimbusSettings _settings;
        public NimbusSettings Settings
        {
            get { return _settings; }
            private set { _settings = value; }
        }

        public static void Init(NimbusSettings settings)
        {
            if (_instance != null) throw new Exception("NimbusAppBus was already initialized.");
            _instance = new NimbusAppBus();
            _instance.Settings = settings;
        }
    }
}
