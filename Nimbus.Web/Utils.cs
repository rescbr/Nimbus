using Nimbus.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using ServiceStack.OrmLite;

namespace Nimbus.Web
{
    public class Utils
    {
        /// <summary>
        /// The method create a Base64 encoded string from a normal string.
        /// </summary>
        /// <param name="toEncode">The String containing the characters to encode.</param>
        /// <returns>The Base64 encoded string.</returns>
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes
                  = System.Text.Encoding.Unicode.GetBytes(toEncode);
            string returnValue
                  = System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        /// <summary>
        /// The method to Decode your Base64 strings.
        /// </summary>
        /// <param name="encodedData">The String containing the characters to decode.</param>
        /// <returns>A String containing the results of decoding the specified sequence of bytes.</returns>
        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes
                = System.Convert.FromBase64String(encodedData);
            string returnValue =
               System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);
            return returnValue;
        }

        public static JavaScriptSerializer B64JavaScriptSerializerFactory
        {
            get
            {
                var jss = new JavaScriptSerializer();
                jss.RegisterConverters(new JavaScriptConverter[] { new ByteArrayBase64Converter() });
                return jss;
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
}
