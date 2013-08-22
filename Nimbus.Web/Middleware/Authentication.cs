using Microsoft.Owin;
using Nimbus.Plumbing.Interface;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nimbus.Web.Middleware
{
    /// <summary>
    /// Middleware responsável pela autenticação do Nimbus
    /// </summary>
    public class Authentication : OwinMiddleware
    {

        private static readonly byte[] NSC_BYTES = { 0x6e, 0x73, 0x63 }; // "nsc"
        private static readonly int NSC_VERSION_1 = 1;

        private INimbusAppBus _nimbusAppBus;
        public Authentication(OwinMiddleware next, INimbusAppBus nimbusAppBus)
            : base(next)
        { _nimbusAppBus = nimbusAppBus; }

        public override async Task Invoke(OwinRequest request, OwinResponse response)
        {
            //Caso o WebAPI informe que é necessário autenticar...
            request.OnSendingHeaders(state =>
            {
                var resp = (OwinResponse)state;

                if (resp.StatusCode == 401)
                    FailLoginRequest(ref resp); //TODO: Pegar path de retorno do login
            }, response);


            //Lê Cookie
            var cookies = request.GetCookies();
            string sessionToken = null;
            try
            {
                sessionToken = Uri.UnescapeDataString(cookies["nsc-session"]);
            }
            catch (Exception)
            {
                FailLoginRequest(ref response);
            }

            if (sessionToken != null)
            {
                Guid tokenGuid;
                NSCInfo info;
                if (VerifyToken(sessionToken, out tokenGuid, out info))
                {
                    //Token é válido, continuar verificando se o usuário pode ser logado
                    if (info.TokenGenerationDate.AddDays(7.0) > DateTime.Now.ToUniversalTime())
                    {
                        //TODO!
                        //pega o NimbusUser
                        //algo como NimbusAppBus.Cache.SessionCache etc
                        var identity = new NimbusUser() { 
                            AvatarUrl = "avatar.png",
                            Name = "Eusébio Testoso",
                            UserId = 1,
                            IsAuthenticated = true //sempre!
                        };
                        //request.User = new ClaimsPrincipal(identity);
                        request.User = (IPrincipal)(new NimbusPrincipal(identity));
                    }
                    else FailLoginRequest(ref response); //token velho
                }
                else FailLoginRequest(ref response); //token inválido
            }
            else FailLoginRequest(ref response); //token = null

            await Next.Invoke(request, response);
        }

        private void FailLoginRequest(ref OwinResponse response)
        {
            //TODO: Colocar endereço de retorno
            response.Redirect("/login");
        }

        public string GenerateToken(int userId, out Guid tokenGuid)
        {
            NSCInfo info = new NSCInfo()
            {
                UserId = userId,
                TokenGenerationDate = DateTime.Now.ToUniversalTime(),
            };
            return GenerateToken(_nimbusAppBus, info, out tokenGuid);
        }

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


        public bool VerifyToken(string token, out Guid tokenGuid, out NSCInfo info)
        {
            return VerifyToken(_nimbusAppBus, token, out tokenGuid, out info);
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

        [Serializable]
        public class NSCInfo
        {
            public int UserId { get; set; }
            public DateTime TokenGenerationDate { get; set; }
        }
    }
}