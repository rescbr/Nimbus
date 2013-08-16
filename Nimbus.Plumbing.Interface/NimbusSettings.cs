using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing.Interface
{
    public class NimbusSettings
    {
        private bool _isDevelopment = false;
        public bool IsDevelopment
        {
            get { return _isDevelopment; }
        }

        private bool _isDebug = false;
        public bool IsDebug
        {
            get { return _isDebug; }
        }

        public NimbusSettings() { }
        public NimbusSettings(bool isDevelopment, bool isDebug)
        {
            _isDebug = isDebug;
            _isDevelopment = isDevelopment;
        }
    }
}
