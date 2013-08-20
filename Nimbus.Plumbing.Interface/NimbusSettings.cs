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

        private string _dbConnString;
        public string DatabaseConnectionString
        {
            get { return _dbConnString; }
        }

        public NimbusSettings() { }
        public NimbusSettings(bool isDevelopment, bool isDebug, string dbConnString)
        {
            _isDebug = isDebug;
            _isDevelopment = isDevelopment;
            _dbConnString = dbConnString;
        }

    }
}
