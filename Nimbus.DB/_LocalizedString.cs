using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//Baseado em http://stackoverflow.com/a/9264669
namespace Nimbus.DB
{

    public class LocalizedString : Dictionary<string, string>
    {
        private string _default;
        public string Default
        {
            get { return _default; }
            set { _default = value; }
        }

        public LocalizedString() : base() { }
        public LocalizedString(string defaultValue)
            : base()
        {
            _default = defaultValue;
        }
        public new string this[string key]
        {
            get { return base.ContainsKey(key) ? base[key] : _default; }
            set { base[key] = value; }
        }
    }
}
