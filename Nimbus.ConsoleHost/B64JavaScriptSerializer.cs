using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Nimbus
{
    public static class B64JavaScriptSerializerFactory
    {
        public static JavaScriptSerializer JSS
        {
            get
            {
                var jss = new JavaScriptSerializer();
                jss.RegisterConverters(new JavaScriptConverter[] { new ByteArrayBase64Converter() });
                return jss;
            }
        }
    }

    public class ByteArrayBase64Converter : JavaScriptConverter
        {
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                return Convert.FromBase64String((string)dictionary["b64"]);
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                return new Dictionary<string, object> { { "b64", Convert.ToBase64String((byte[])obj) } };
            }

            public override IEnumerable<Type> SupportedTypes
            {
                get { return new[] { typeof(byte[]) }; }
            }
        }
}
