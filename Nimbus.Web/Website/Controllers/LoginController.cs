using Facebook;
using Nimbus.Plumbing;
using Nimbus.Web.API;
using Nimbus.Web.Security;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using System.Globalization;
using Nimbus.Web.Utils;
using System.Net;


namespace Nimbus.Web.Website.Controllers
{
    public class LoginController : NimbusWebController
    {
        DatabaseLogin.AuthenticationResult _tempAuthResult = DatabaseLogin.AuthenticationResult.GenericFail;
        // GET: /login
        [HttpGet]
        [ActionName("Index")]
        public ActionResult Get(string redirect = null, string errormessage = null)
        {
            if (redirect != null && Uri.IsWellFormedUriString(redirect, UriKind.Relative))
            {
                return View("Login", new LoginModel()
                {
                    RedirectURL = redirect,
                    ErrorMessage = errormessage
                });
            }
            else
            {
                return View(new LoginModel() { ErrorMessage = errormessage });
            }
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Post(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                DatabaseLogin dbLogin = new DatabaseLogin(DatabaseFactory);
                NimbusPrincipal loggedInUser;

                //verifica URL de redirecionamento
                if (String.IsNullOrWhiteSpace(login.RedirectURL) ||
                    !Uri.IsWellFormedUriString(login.RedirectURL, UriKind.Relative))
                {
                    login.RedirectURL = "/";
                }

                //faz login no banco
                if (dbLogin.Authenticate(login.Email, login.Password, out loggedInUser, out _tempAuthResult))
                {
                    //Usuário e senha corretos, criar token de autenticação
                    Guid token;

                    //Cria token com validade de 7 dias
                    string authToken = Token.GenerateToken(
                        new NSCInfo()
                        {
                            TokenGenerationDate = DateTime.Now.ToUniversalTime(),
                            TokenExpirationDate = DateTime.Now.AddDays(Const.CookieExpiryDays).ToUniversalTime(),
                            UserId = (loggedInUser.Identity as NimbusUser).UserId
                        },
                        out token);

                    //Lembre-se de expirar o cookie também
                    var loginCookie = new HttpCookie("nsc-session", authToken)
                    {
                        Expires = DateTime.Now.AddDays(Const.CookieExpiryDays)
                    };

                    //adiciona objeto do usuário logado à sessão
                    Session[Const.UserSession] = loggedInUser;

                    Response.Cookies.Add(loginCookie);
                    return Redirect(login.RedirectURL);

                }
                else
                {
                    //joga mensagem de erro
                    login.ErrorMessage = "Usuário ou senha inválidos.";
                }
            }

            login.Password = ""; //limpa a senha antes de enviar
            return View(login);
        }

        [HttpGet]
        [ActionName("Logout")]
        public ActionResult Logout()
        {
            var cookie = Request.Cookies["nsc-session"];
            cookie.Value = "";
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
            return Redirect("/");
        }

        [HttpGet]
        [ActionName("Facebook")]
        public ActionResult Facebook(string redirect)
        {
            string fbRedirectUrl = Request.Url.Scheme + "://" + Request.Url.Authority + "/login/FacebookCallback";
            if (redirect != null)
                fbRedirectUrl += "?redirect=" + Uri.EscapeUriString(redirect);
            var fb = new FacebookClient();
            var loginUrl = fb.GetLoginUrl(new
            {
                client_id = NimbusConfig.FacebookAppId,
                redirect_uri = fbRedirectUrl,
                response_type = "code",
                scope = "email,user_birthday" // Add other permissions as needed
            });

            return Redirect(loginUrl.AbsoluteUri);
        }

        [HttpGet]
        [ActionName("FacebookCallback")]
        public ActionResult FacebookCallback(string redirect)
        {
            var fb = new FacebookClient();
            FacebookOAuthResult fbResult;
            if (fb.TryParseOAuthCallbackUrl(Request.Url, out fbResult))
            {
                if (fbResult.IsSuccess)
                {
                    //pega nome, id, email e 3rd party id do facebook
                    dynamic tokenResult = fb.Post("oauth/access_token", new
                    {
                        client_id = NimbusConfig.FacebookAppId,
                        client_secret = NimbusConfig.FacebookAppSecret,
                        redirect_uri = Request.Url.AbsoluteUri, //eh bizarro mas precisa disso
                        code = fbResult.Code
                    });
                    string accessToken = tokenResult.access_token;
                    fb.AccessToken = accessToken;
                    dynamic fbUserInfo = fb.Get("me?fields=first_name,last_name,id,email,third_party_id,birthday");
                    string fbEmail = fbUserInfo.email;

                    //tenta achar o usuario no BD
                    Model.ORM.User nimbusUser;
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        nimbusUser = db.Where<Model.ORM.User>(u => u.Email == fbEmail).FirstOrDefault();
                        if (nimbusUser == null)
                        {
                            //usuário sem conta nimbus, cria uma conta pra ele
                            DateTime fbBirthday = Convert.ToDateTime(fbUserInfo.birthday, new DateTimeFormatInfo() { ShortDatePattern = "MM/dd/yyyy" });
                            string pathAvatar;
                            dynamic fbAvatar = fb.Get("me/picture?redirect=0&type=large");
                            string fbAvatarUrl = fbAvatar.data.url; //precisa dessas idas e vindas por causa do fbAvatar ser dynamic
                            if (fbAvatar.data.is_silhouette == true)
                                pathAvatar = "/images/av130x130/person_icon.png";
                            else
                            {
                                var req = HttpWebRequest.Create(fbAvatarUrl);
                                var respStream = req.GetResponse().GetResponseStream();
                                var img = new ImageManipulation(respStream);

                                #region Redimensiona avatar do FB e sobe no azure
                                img.FitSize(200, 200); //muito embora a url é av130x130, o tamanho do avatar é 200x200.

                                HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
                                var nomeImgAvatar = "avatar-" + fbUserInfo.third_party_id;
                                md5.ComputeHash(Encoding.Unicode.GetBytes(nomeImgAvatar));
                                var nomeHashExt = Base32.ToString(md5.Hash).ToLower() + ".jpg";
                                nomeImgAvatar = "av130x130/" + nomeHashExt;
                                var nomeImgAvatar35x35 = "av35x35/" + nomeHashExt;
                                var nomeImgAvatar60x60 = "av60x60/" + nomeHashExt;

                                var blob = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar);
                                blob.UploadStreamToAzure(img.SaveToJpeg());

                                //envia as imagens redimensionadas
                                img.FitSize(60, 60);
                                var blob60x60 = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar60x60);
                                blob60x60.UploadStreamToAzure(img.SaveToJpeg());

                                img.FitSize(35, 35);
                                var blob35x35 = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar35x35);
                                blob35x35.UploadStreamToAzure(img.SaveToJpeg());

                                pathAvatar = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://").Replace("***REMOVED***", "storage.portalnimbus.com.br");
                                #endregion
                            }

                            var newUser = new Model.ORM.User()
                            {
                                Email = fbEmail,
                                FirstName = fbUserInfo.first_name,
                                LastName = fbUserInfo.last_name,
                                BirthDate = fbBirthday,
                                AvatarUrl = pathAvatar
                            };
                            db.Insert(newUser);
                            newUser.Id = (int)db.GetLastInsertId();

                            nimbusUser = newUser;
                        }
                    }

                    //usuário já possui conta Nimbus, entao faz login pra ele
                    Guid token;
                    //Cria token com validade de 7 dias
                    string authToken = Token.GenerateToken(
                        new NSCInfo()
                        {
                            TokenGenerationDate = DateTime.Now.ToUniversalTime(),
                            TokenExpirationDate = DateTime.Now.AddDays(Const.CookieExpiryDays).ToUniversalTime(),
                            UserId = nimbusUser.Id
                        },
                        out token);

                    //Lembre-se de expirar o cookie também
                    var loginCookie = new HttpCookie("nsc-session", authToken)
                    {
                        Expires = DateTime.Now.AddDays(Const.CookieExpiryDays)
                    };

                    //adiciona objeto do usuário logado à sessão
                    Session[Const.UserSession] = DatabaseLogin.GetNimbusPrincipal(nimbusUser);
                    Response.Cookies.Add(loginCookie);
                    return Redirect(redirect);
                   
                } //fim if (fbResult.IsSuccess)
                else
                {
                    //erro no login do face
                    if (fbResult.ErrorReason == "user_denied")
                    {
                        return Redirect("/login?errormessage=" + Uri.EscapeDataString("É necessário autorizar o login pelo Facebook."));
                    }
                    else
                    {
                        return Redirect("/login?errormessage=" + Uri.EscapeDataString("Ocorreu um erro ao fazer login pelo Facebook."));
                    }
                }
            } else {
                //fim if (fb.TryParseOAuthCallbackUrl
                //usuario quis brincar com callback do facebook, redirect pra login sem nem mostrar erro
                return Redirect("/login");
            }
        }

        [NonAction]
        private string DerivateFacebookPassword(string thirdPartyId)
        {
            HMACSHA512 hmac = new HMACSHA512(NimbusConfig.GeneralHMACKey);
            hmac.ComputeHash(Encoding.Unicode.GetBytes(thirdPartyId));
            string b64Hash = Convert.ToBase64String(hmac.Hash);

            return b64Hash;
        }
    }
}