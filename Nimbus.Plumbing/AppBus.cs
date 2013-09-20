using Nimbus.Plumbing;
using Nimbus.Plumbing.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public class NimbusAppBus
    {
        #region Singleton
        private static NimbusAppBus _instance = null;
        public static NimbusAppBus Instance
        {
            get
            {
                if (_instance == null) throw new Exception("Run Init() first.");
                return _instance;
            }
        }

        public static void Init(NimbusSettings settings)
        {
            if (_instance != null) throw new Exception("NimbusAppBus was already initialized.");
            _instance = new NimbusAppBus();
            _instance._settings = settings;

            /* Inicializa Cache utilizando LocalCache */
            _instance._cache = new NimbusCache();
            _instance._cache.Initialize<LocalCache>();
        }

        private NimbusAppBus() { }

        #endregion

        private NimbusSettings _settings;
        public NimbusSettings Settings
        {
            get { return _settings; }
            private set { _settings = value; }
        }

        private NimbusCache _cache;
        public NimbusCache Cache
        {
            get { return _cache; }
        }

    }
}
