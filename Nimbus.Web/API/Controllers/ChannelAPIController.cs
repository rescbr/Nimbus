using Nimbus.Web.API.Models.Channel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;

namespace Nimbus.Web.API.Controllers
{

    /// <summary>
    /// Controle sobre todas as funções realizadas para os Canais.
    /// </summary>
    public class ChannelAPIController : NimbusApiController
    {
        #region métodos de visualização
        /// <summary>
        /// carregar informações gerais do canal
        /// </summary>
        /// <returns></returns>
        public showChannelAPIModel showChannel(int channelID)
        {
            showChannelAPIModel showChannel = new showChannelAPIModel();
            try
            {

                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    
                    bool ismember = (db.SelectParam<Nimbus.DB.ChannelUser>(usr => usr.UserId == NimbusUser.UserId &&
                                                                                        usr.ChannelId == channelID).Count()) > 0 ? true : false;

                    if (ismember == true)
                    {
                        Nimbus.DB.Channel channel = db.SelectParam<Nimbus.DB.Channel>(chn => chn.Id == channelID && chn.Visible == true).FirstOrDefault();
                        showChannel.ChannelName = channel.Name;
                        showChannel.CountFollowers = channel.Followers.ToString();
                        showChannel.Organization_ID = channel.OrganizationId;
                        showChannel.owner_ID = channel.OwnerId;
                        showChannel.Price = channel.Price;
                        //showChannel.ParticipationChannel = 
                        showChannel.RankingChannel = channel.Ranking.ToString();
                        showChannel.UrlImgChannel = channel.ImgUrl;
                    }
                    else
                    {
                        AlertChannelPay alert = new AlertChannelPay ();
                        Nimbus.DB.Channel channel2 = db.Query<Nimbus.DB.Channel>("SELECT Channel.Price, Channel.IsPrivate" +
                                                                                 "FROM Channel"+
                                                                                 "WHERE Channel.Id ={0} AND Channel.Visible = true", channelID).FirstOrDefault();
                        if (channel2.Price > 0)
                        {
                            showChannel.MessageAlert = alert.AlertPay ;
                        }
                        else if (channel2.IsPrivate == true)
                        {
                            showChannel.MessageAlert = alert.AlertPrivate;
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return showChannel;
        }

        //visualizar 'meus canais'
        //visualizar canais moderados
        //visualizar resumo do  canal
        #endregion


        #region métodos de interação com o canal
        //criar canal
        //editar canal
        //add moderadores para o canal
        //deletar moderadores
        //seguir/ñ seguir canal
        //enviar mensagem para o canal/dono)
        //editar/add tags do canal
        //ver mais tarde o canal ou retirar da lista de ver mais tarde
        #endregion




    }



}
