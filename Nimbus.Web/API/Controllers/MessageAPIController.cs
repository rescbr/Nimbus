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
using Nimbus.DB.ORM;

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
        [HttpPost]
        public string SendMessageChannel(Message message)
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
                            //Lembrar: se owner = true, quando mostrar na view colocar: Nimbus     
                            List<Nimbus.DB.Receiver> listReceiver = db.Select<Nimbus.DB.Receiver>("SELECT Role.UserId, Role.IsOwner, User.Name " +
                                                                                                  "FROM Role INNER JOIN User ON Role.UserId = User.Id" +
                                                                                                  "WHERE Role.ChannelId ={0} AND " +
                                                                                                  "(Role.MessageManager = true OR Role.IsOwner = true)",
                                                                                                   message.ChannelId);
                            //add a  msg                                                 
                                Message dadosMsg = new Message
                                {
                                    SenderId = NimbusUser.UserId,
                                    ChannelId = message.ChannelId,
                                    Date = DateTime.Now,
                                    ReadStatus = false,
                                    Text = message.Text,
                                    Title = message.Title,
                                    Visible = true,
                                    Receivers = listReceiver
                                };
                                db.Save(dadosMsg);

                                int idMesg = (int)db.GetLastInsertId();

                                foreach (var item in listReceiver)
                                {
                                    if (item.UserId == NimbusUser.UserId)
                                    {
                                        db.Insert(new ReceiverMessage
                                          { IsOwner = item.IsOwner, MessageId = idMesg, 
                                            UserId = item.UserId, NameUser = item.Name,
                                            Status = Nimbus.DB.Enums.MessageType.send
                                          });
                                    }
                                    else
                                    {
                                        db.Insert(new ReceiverMessage
                                        {
                                            IsOwner = item.IsOwner,
                                            MessageId = idMesg,
                                            UserId = item.UserId,
                                            NameUser = item.Name,
                                            Status = Nimbus.DB.Enums.MessageType.received
                                        });
                                    }                                                                      
                                }
                                trans.Commit();
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
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return msg;
        }
    
        /// <summary>
        /// mensangens recebidas
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<Message> ReceivedMessages()
        {
            List<Message> listMessage = new List<Message>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> listIdMsg = new List<int>();
                    listIdMsg = db.SelectParam<ReceiverMessage>(r => r.Status == DB.Enums.MessageType.received 
                                                                               && r.UserId == NimbusUser.UserId)
                                                                               .Select(r => r.MessageId).ToList();

                    listMessage = db.SelectParam<Message>(m => m.Visible == true && listIdMsg.Contains(m.Id));                  
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listMessage.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// lista as mensagens enviadas pelo usuário
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<Message> SentMessages()
        {
            List<Message> listMessage = new List<Message>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> msgSend = new List<int>();
                    msgSend = db.SelectParam<ReceiverMessage>(rm => rm.Status == Nimbus.DB.Enums.MessageType.send 
                                                                   && rm.UserId == NimbusUser.UserId)
                                                                   .Select(rm => rm.MessageId).ToList();

                    listMessage = db.SelectParam<Message>(m => m.Visible == true && (msgSend.Contains(m.Id) || m.SenderId == NimbusUser.UserId));                
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
                        Message message = new Message();
                        //visible= false  quando o usuario mandou ou recebeu a msg
                        db.Update<Message>(message.Visible = false, m => m.Id == item  
                                                                             && ( m.Receivers.Exists(r => r.UserId == NimbusUser.UserId) 
                                                                                  || m.SenderId == NimbusUser.UserId)
                                                                                  );
                        db.Save(message);
                    }
                    msg = alert.SuccessMessage;
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return msg; 
        }

        
    }
}
