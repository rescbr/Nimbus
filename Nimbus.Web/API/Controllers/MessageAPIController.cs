using Nimbus.Web.API.Models;
using Nimbus.Web.API.Models.Channel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models.Message;
using Nimbus.DB;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para as Mensagens
    /// </summary>
    public class MessageAPIController : NimbusApiController
    {              
        /// <summary>
        /// enviar mensagem através de um canal
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [Authorize]
        public string SendMessageChannel(SendMessageChannelAPI message)
        {
            AlertSendMessage alert = new AlertSendMessage();
            string msg = alert.ErrorMessage;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //Lembrar: se ownerID = 0, quando mostrar na view colocar: Nimbus
                            int ownerID = db.Select<int>("SELECT Role.UserId FROM Role " +
                                                         "WHERE Role.ChannelId ={0} AND Role.IsOwner = true", message.Channel_ID).FirstOrDefault();

                            List<int> listReceiver = db.Select<int>("SELECT Role.UserId FROM Role " +
                                                                     "WHERE Role.ChannelId ={0} AND Role.MessageManager = true", message.Channel_ID);

                            if (ownerID > 0)
                            {
                                listReceiver.Add(ownerID);
                            }

                            foreach (int item in listReceiver)
                            {
                                Message dadosMsg = new Message
                                {
                                    Sender_ID = NimbusUser.UserId,
                                    Receiver_ID = item,
                                    Channel_ID = message.Channel_ID,
                                    Date = DateTime.Now,
                                    ReadStatus = false,
                                    Status = Enums.MessageType.received,
                                    Text = message.Text,
                                    Title = message.Title,
                                    Visible = true,
                                    Receivers = null
                                };
                                db.Insert(dadosMsg);
                                db.Save(dadosMsg);
                            }
                            msg = alert.SuccessMessage;

                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            msg = alert.ErrorMessage;
                            throw ex;
 
                        }
                    }
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return msg;
        }
    
        /// <summary>
        /// mensangens recebidas
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public List<Nimbus.DB.Message> ReceivedMessages()
        {
            List<Nimbus.DB.Message> listMessage = new List<Nimbus.DB.Message>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    listMessage = db.SelectParam<Nimbus.DB.Message>(user => user.Receiver_ID == NimbusUser.UserId 
                                                                            && user.Status == DB.Enums.MessageType.received); 
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            return listMessage.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// lista as mensagens enviadas pelo usuário
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public List<Nimbus.DB.Message> SentMessages()
        {
            List<Nimbus.DB.Message> listMessage = new List<Nimbus.DB.Message>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    listMessage = db.SelectParam<Nimbus.DB.Message>(user => user.Receiver_ID == NimbusUser.UserId
                                                                            && user.Status == DB.Enums.MessageType.send);
                }
            }
            catch (Exception)
            {

                throw;
            }
            return listMessage.OrderBy(d => d.Date).ToList();
        }

       
        /// <summary>
        /// Troca a visibilidade das mensagens//excluir mensagem 
        /// </summary>
        /// <param name="listID"></param>
        /// <returns></returns>
        [Authorize]
        public string DeleteMessages(List<int> listID)
        {
            AlertGeneral alert = new AlertGeneral();
            string msg = alert.ErrorMessage;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    foreach (int item in listID)
                    {
                        db.Update<Nimbus.DB.Message>(new { Visible = false }, msgID => msgID.Id == item && 
                                                                              (msgID.Receiver_ID == NimbusUser.UserId ||
                                                                               msgID.Sender_ID == NimbusUser.UserId));
                    }
                    msg = alert.SuccessMessage;
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return msg; 
        }

        



    }
}
