using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing.Cache
{
    public class LocalCache : INimbusCacheProvider
    {
        private ConcurrentDictionary<string, object> _objectStore = new ConcurrentDictionary<string,object>();

        public object Get(string key)
        {
            return _objectStore[key];
        }

        public void Store(string key, object value)
        {
            _objectStore[key] = value;
        }

        public void StoreAndReplicate(string key, object value)
        {
            _objectStore[key] = value;
        }

        public object DeleteAndReplicate(string key, object value)
        {
            object obj;
            _objectStore.TryRemove(key, out obj);
            return obj;
        }
    }
}
