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
                    //inserir mensagem  enviada para o remetente e recebida para o destinatario 
                   var dados = new[]
                    {
                        new Nimbus.DB.Message
                        {
                            Receiver_ID = message.receiver_ID,
                            Sender_ID = NimbusUser.UserId,
                            Channel_ID = message.Channel_ID,
                            Title = message.Title,
                            Message = message.Text,
                            ReadStatus = false,
                            Status = Nimbus.DB.Enums.MessageType.received, //add p pessoa que vai receber a msg
                            Date = DateTime.Now,
                            Visible = true
                        },

                        new Nimbus.DB.Message
                        {
                            Receiver_ID = message.receiver_ID,
                            Sender_ID = NimbusUser.UserId,
                            Channel_ID = message.Channel_ID,
                            Title = message.Title,
                            Message = message.Text,
                            ReadStatus = true,
                            Status = Nimbus.DB.Enums.MessageType.send, //add p pessoa que enviou como registro/histórico
                            Date = DateTime.Now,
                            Visible = true
                        }
                    };
                   db.Insert(dados);
                   db.Save(dados);
                   msg = alert.SuccessMessage;
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
