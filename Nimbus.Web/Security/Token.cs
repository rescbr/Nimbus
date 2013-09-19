using Nimbus.Plumbing.Interface;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Nimbus.Web.Security
{
    public class Token
    {

        private static readonly byte[] NSC_BYTES = { 0x6e, 0x73, 0x63 }; // "nsc"
        private static readonly int NSC_VERSION_1 = 1;

        /// <summary>
        /// Gera um Token nsc:1:
        /// </summary>
        /// <param name="nimbusAppBus">NimbusAppBus</param>
        /// <param name="info">dados do usuário NSCInfo</param>
        /// <param name="tokenGuid">saída da GUID gerada (Session ID)</param>
        /// <returns>string nsc:1: contendo o token</returns>
        public static string GenerateToken(INimbusAppBus nimbusAppBus, NSCInfo info, out Guid tokenGuid)
        {

            Guid token = Guid.NewGuid();

            byte[] bNscInfo;
            byte[] bGuidToken = token.ToByteArray();

            using (MemoryStream ms = new MemoryStream())
            {
                TypeSerializer.SerializeToStream<NSCInfo>(info, ms);
                bNscInfo = ms.ToArray();
            }

            byte[] bHmacMsg = NSC_BYTES.Concat(BitConverter.GetBytes(NSC_VERSION_1)).Concat(bGuidToken).Concat(bNscInfo).ToArray();

            HMACSHA512 hmac = new HMACSHA512(nimbusAppBus.Settings.Cryptography.CookieHMACKey);
            hmac.ComputeHash(bHmacMsg);

            string b64info = Convert.ToBase64String(bNscInfo);
            string b64token = Convert.ToBase64String(bGuidToken);
            string b64hmac = Convert.ToBase64String(hmac.Hash);


            StringBuilder sb = new StringBuilder();
            sb.Append("nsc:"); sb.Append(NSC_VERSION_1.ToString()); sb.Append(':');
            sb.Append(b64token); sb.Append(':');
            sb.Append(b64info); sb.Append(':');
            sb.Append(b64hmac);


            tokenGuid = token;
            return sb.ToString();
        }

        /// <summary>
        /// Verifica Token de autenticação (nsc:1:)
        /// </summary>
        /// <param name="nimbusAppBus">NimbusAppBus</param>
        /// <param name="token">string do token nsc:1:</param>
        /// <param name="tokenGuid">saída contendo a GUID do token (Session ID)</param>
        /// <param name="info">saída contendo NSCInfo com dados do usuário</param>
        /// <returns></returns>
        public static bool VerifyToken(INimbusAppBus nimbusAppBus, string token, out Guid tokenGuid, out NSCInfo info)
        {
            tokenGuid = Guid.Empty;
            info = null;

            string[] splitToken = token.Split(':');
            try
            {
                // Verifica condições
                // Token começa com nsc
                if (splitToken[0] != "nsc") return false;

                // Para token versão 1:
                if (splitToken[1] == "1")
                {
                    int nscVersion = 1;
                    //obtem as partes
                    byte[] bToken = Convert.FromBase64String(splitToken[2]);
                    byte[] bInfo = Convert.FromBase64String(splitToken[3]);
                    byte[] bHmac = Convert.FromBase64String(splitToken[4]);

                    IEnumerable<byte> bHmacMsg = NSC_BYTES.Concat(BitConverter.GetBytes(nscVersion)).Concat(bToken).Concat(bInfo);

                    //calcula HMAC
                    HMACSHA512 hmac = new HMACSHA512(nimbusAppBus.Settings.Cryptography.CookieHMACKey);
                    hmac.ComputeHash(bHmacMsg.ToArray());

                    //se HMACs baterem, token é valido
                    if (bHmac.SequenceEqual(hmac.Hash))
                    {
                        //desserializa informações
                        Guid t = new Guid(bToken);
                        NSCInfo i;
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ms.Write(bInfo, 0, bInfo.Length);
                            ms.Seek(0, SeekOrigin.Begin);
                            i = TypeSerializer.DeserializeFromStream<NSCInfo>(ms);
                        }
                        tokenGuid = t;
                        info = i;
                        return true;
                    }
                }
                else return false;
            }
            catch (Exception ex) { return false; }

            return false;
        }


    }
    [Serializable]
    public class NSCInfo
    {
        public int UserId { get; set; }
        public DateTime TokenGenerationDate { get; set; }
        public DateTime TokenExpirationDate { get; set; }
    }
}