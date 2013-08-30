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
                            //Lembrar: se owner = true, quando mostrar na view colocar: Nimbus
                                                        
                            List<Message.Receiver> listReceiver = db.Select<Message.Receiver>("SELECT Role.UserId, Role.IsOwner, User.Name " +
                                                                                              "FROM Role INNER JOIN User ON Role.UserId = User.Id" +
                                                                                              "WHERE Role.ChannelId ={0} AND " +
                                                                                              "(Role.MessageManager = true OR Role.IsOwner = true)",
                                                                                              message.Channel_ID);

                                //add a  msg                                                 
                                Message dadosMsg = new Message
                                {
                                    Sender_ID = NimbusUser.UserId,
                                    Channel_ID = message.Channel_ID,
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
                                    if (item.UserID == NimbusUser.UserId)
                                    {
                                        db.Insert(new ReceiverMessage
                                          { IsOwner = item.IsOwner, MessageID = idMesg, 
                                            UserID = item.UserID, NameUser = item.Name,
                                            Status = Enums.MessageType.send
                                          });
                                    }
                                    else
                                    {
                                        db.Insert(new ReceiverMessage
                                        {
                                            IsOwner = item.IsOwner,
                                            MessageID = idMesg,
                                            UserID = item.UserID,
                                            NameUser = item.Name,
                                            Status = Enums.MessageType.received
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
                    listMessage = db.Select<Message>("SELECT *  "+
                                                     "FROM Message"+
                                                     "INNER JOIN ReceiverMessage ON  Message.Id = ReceiverMessage.MessageID" +
                                                     "WHERE ReceiverMessage.UserID = {0} AND Message.Visible = true AND ReceiverMessage.Status = {1}",
                                                     NimbusUser.UserId, Enums.MessageType.received);
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
                    listMessage = db.Select<Message>("SELECT *  " +
                                                     "FROM Message" +
                                                     "INNER JOIN ReceiverMessage ON  Message.Id = ReceiverMessage.MessageID" +
                                                     "WHERE ReceiverMessage.UserID = {0} AND Message.Visible = true AND ReceiverMessage.Status = {1}",
                                                      NimbusUser.UserId, Enums.MessageType.send);

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
                        db.Query<Nimbus.DB.Message>("UPDATE Message SET Message.Visible = false " +
                                                     "INNER JOIN ReceiverMessage ON Message.Id = ReceiverMessage.MessageID " +
                                                     "WHERE Message.Id = @msgID  AND ReceiverMessage.UserID = @userID ",
                                                      new{ msgID = item, userID = NimbusUser.UserId});
                        
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
