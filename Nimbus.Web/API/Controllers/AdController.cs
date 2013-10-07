using Nimbus.DB.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Nimbus.DB.Bags;

namespace Nimbus.Web.API.Controllers
{
    public class AdController:NimbusApiController
    {
        /// <summary>
        /// Método de adicionar/comprar um ads 
        /// </summary>
        /// <param name="adDados"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public AdsBag SaleAd(AdsBag adDados)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {

                        if (db.Exists<UserInfoPayment>(NimbusUser.UserId))
                        {
                            db.Insert(adDados);

                            int idAds = (int)db.GetLastInsertId();
                            double priceAd = db.SelectParam<Prices>(p => p.Id == adDados.PriceId).Select(p => p.Price).FirstOrDefault();

                            UserAds userAd = new UserAds();
                            userAd.AdsId = idAds;
                            userAd.CountClick = 0;
                            userAd.CountDay = 0;
                            //quantidade de cliques ou dias restantes
                            userAd.CountLeft = (int)((adDados.Credits) / priceAd);
                            userAd.Credits = adDados.Credits;
                            userAd.DatePayment = null;
                            userAd.IsPaid = false;
                            userAd.UserId = NimbusUser.UserId;

                            db.Save(userAd);
                            trans.Commit();
                        }
                        else
                        {
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "registro incompleto"));
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                    }
                }
            }
            
            return adDados;
        }


    }
}