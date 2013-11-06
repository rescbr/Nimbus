using Nimbus.Model.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Nimbus.Model.Bags;

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
                            Ad ad = new Ad
                            {
                                CategoryId = adDados.CategoryId,
                                ChannelId = adDados.ChannelId,
                                ImgUrl = adDados.ImgUrl,
                                TypeAd = adDados.TypeAd,
                                Url = adDados.Url,
                                Visible = false
                            };
                            db.Insert(ad);

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

        /// <summary>
        /// Método para pegar os ADs do usuario e mostrar todas as informações
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<UserAdsBag> ShowInfo()
        {
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    List<UserAds> info = db.SelectParam<UserAds>(us => us.UserId == NimbusUser.UserId && us.IsPaid == true).ToList();

                    List<UserAdsBag> listAds = new List<UserAdsBag>();
                    foreach (UserAds item in info)
                    {
                        UserAdsBag bag = new UserAdsBag
                        {
                            Ads = db.SelectParam<Ad>(ad => ad.Id == item.AdsId).FirstOrDefault(),
                            AdsId = item.AdsId,
                            CountClick = item.CountClick,
                            CountDay = item.CountDay,
                            CountLeft = item.CountLeft,
                            Credits = item.Credits,
                            DatePayment = item.DatePayment,
                            IsPaid = item.IsPaid,
                            UserId = item.UserId
                        };
                        listAds.Add(bag);
                    }
                    return listAds;
                }
            }
            catch (Exception ex)
            {                
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }
    }
}