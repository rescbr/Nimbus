﻿using Nimbus.Web.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using Nimbus.Model.Bags;
using System.Web;
using Nimbus.Web.Utils;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para as Mensagens
    /// </summary>
    [NimbusAuthorize]
    public class MessageController : NimbusApiController
    {
        public class MessageHtmlWrapper
        {
            public int Count { get; set; }
            public string Html { get; set; }
        }

        [HttpGet]
        public MessageHtmlWrapper MessagesHtml(string viewBy = null, int skip = 0)
        {
            List<MessageBag> message = new List<MessageBag>();
                       
            if (viewBy == "messageSend")
                message = SentMessages(skip);
            if (viewBy == "messageReceived")
                message = ReceivedMessages(skip);
          

            var rz = new RazorTemplate();
            string html = "";

            foreach (var msg in message)
            {
                html += rz.ParseRazorTemplate<MessageBag>
                    ("~/Website/Views/MessagePartials/MessagePartial.cshtml", msg);
            }

            return new MessageHtmlWrapper { Html = html, Count = message.Count };
        }
                           


        [NonAction]
        internal Message SendMessageToList(Message message, List<Model.Receiver> listReceiver)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        //add a msg para o 'sender'
                        Nimbus.Model.Receiver sender = new Model.Receiver();
                        sender.IsOwner = db.SelectParam<Role>(r => r.ChannelId == message.ChannelId && r.UserId == NimbusUser.UserId)
                                                                   .Select(c => c.IsOwner).FirstOrDefault();
                        sender.UserId = NimbusUser.UserId;
                        sender.Name = NimbusUser.FirstName + " " + NimbusUser.LastName;
                        sender.AvatarUrl = NimbusUser.AvatarUrl;
                        listReceiver.Add(sender);

                        //add a  msg                                                 
                        Message dadosMsg = new Message
                        {
                            SenderId = message.SenderId > 0 ? message.SenderId : NimbusUser.UserId,
                            ChannelId = message.ChannelId,
                            Date = DateTime.Now,
                            Text = message.Text,
                            Title = message.Title,
                            Visible = true,
                            Receivers = listReceiver
                        };
                        db.Save(dadosMsg);

                        int idMesg = (int)db.GetLastInsertId();
                        message.Id = idMesg;
                        dadosMsg.Id = idMesg;

                        foreach (var item in listReceiver.Distinct())
                        {
                            if (item.UserId == NimbusUser.UserId) //qm está enviando
                            {
                                db.Insert(new ReceiverMessage
                                {
                                    IsOwner = item.IsOwner,
                                    MessageId = idMesg,
                                    UserId = item.UserId,
                                    NameUser = item.Name,
                                    UserReadStatus = true,
                                    Status = Nimbus.Model.Enums.MessageType.send
                                });
                            }
                            else
                            { //qm vai receber
                                db.Insert(new ReceiverMessage
                                {
                                    IsOwner = item.IsOwner,
                                    MessageId = idMesg,
                                    UserId = item.UserId,
                                    NameUser = item.Name,
                                    UserReadStatus = false,
                                    Status = Nimbus.Model.Enums.MessageType.received
                                });
                            }
                        }
                        trans.Commit();

                        //Notificação
                        var notification = new Notifications.MessageNotification();
                        notification.NewMessage(dadosMsg);

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }

            return message;
        }
        /// <summary>
        /// enviar mensagem através de um canal
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        
        [HttpPost]
        public Message SendMessageChannel(Message message)
        {
            //Lembrar: se owner = true, quando mostrar na view colocar: Nimbus     
            List<Nimbus.Model.Receiver> listReceiver = new List<Model.Receiver>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var roles = db.SelectParam<Role>(r => r.ChannelId == message.ChannelId && (r.MessageManager == true || r.IsOwner == true));

                //add todos os destinatarios dono + moderadores do canal
                foreach (var item in roles)
                {
                    Nimbus.Model.Receiver receiver = new Model.Receiver();
                    var user = db.SelectParam<User>(u => u.Id == item.UserId).FirstOrDefault();
                    receiver.IsOwner = item.IsOwner;
                    receiver.UserId = item.UserId;
                    receiver.Name = user.FirstName + " " + user.LastName;
                    receiver.AvatarUrl = user.AvatarUrl;

                    listReceiver.Add(receiver);
                }
            }
            return SendMessageToList(message, listReceiver);
        }

        /// <summary>
        /// método que envia uma mensagem pelo profile do usuario
        /// </summary>
        /// <param name="message"></param>
        /// <param name="receiverId"></param>
        /// <returns></returns>
        [HttpPost]
        public Message SendMessageUser(Message message, int id)
        {
            List<Nimbus.Model.Receiver> listReceiver = new List<Nimbus.Model.Receiver>();
            if (id != 0)
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Nimbus.Model.Receiver receiver = new Model.Receiver();
                    var user = db.SelectParam<User>(u => u.Id == id).FirstOrDefault();
                    receiver.IsOwner = true; //a msg é enviada para o perfil, logo não importa esse item
                    receiver.UserId = id;
                    receiver.Name = user.FirstName + " " + user.LastName;
                    receiver.AvatarUrl = user.AvatarUrl;

                    listReceiver.Add(receiver);
                }
            }
            return SendMessageToList(message, listReceiver);
        }

        /// <summary>
        /// mostra todas as mensagens que o usuário recebeu, mostra no perfil do usuário
        /// </summary>
        /// <returns></returns>
        [HttpGet] 
        public List<MessageBag> ReceivedMessages(int skip)
        {
            List<MessageBag> listMessage = new List<MessageBag>();
            
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    try
                    {
                        var listIdMsg = db.Where<ReceiverMessage>(r => r.UserId == NimbusUser.UserId && r.Status == Model.Enums.MessageType.received)
                                                            .Skip(15 * skip).Take(15)
                                                            .Select(r => db.Where<Message>(m => m.Id == r.MessageId && m.Visible == true).FirstOrDefault());

                        if (listIdMsg.Count() > 0)
                        {
                            foreach (var msg in listIdMsg)
                            {
                                MessageBag bag = new MessageBag();
                                User user = db.SelectParam<User>(u => u.Id == msg.SenderId).FirstOrDefault();
                                bag.ChannelId = msg.ChannelId;
                                bag.Date = msg.Date;
                                bag.Id = msg.Id;
                                bag.Receivers = msg.Receivers;
                                bag.SenderId = msg.SenderId;
                                bag.Text = msg.Text;
                                bag.Title = msg.Title;
                                bag.Visible = msg.Visible;
                                bag.UserName = user.FirstName + " " + user.LastName;
                                bag.AvatarUrl = user.AvatarUrl;
                                bag.UserReadStatus = db.Where<ReceiverMessage>(r => r.UserId == NimbusUser.UserId && r.MessageId == msg.Id)
                                                       .Select(r => r.UserReadStatus).FirstOrDefault();

                                listMessage.Add(bag);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                    }
                }

                return listMessage.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// Método que retorna as mensagems enviadas para o canal
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet] 
        public List<MessageBag> ChannelReceivedMessages(int id = 0, int skip = 0)
        {

            List<MessageBag> listMessage = new List<MessageBag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    bool idsAllow = db.SelectParam<Role>(r => r.ChannelId == id &&
                                                              (r.IsOwner == true || r.ChannelMagager == true || r.MessageManager == true))
                                                              .Exists(r => r.UserId == NimbusUser.UserId);

                    if (idsAllow == true)
                    {
                       var listIdMsg = db.Where<ReceiverMessage>(r => r.Status == Model.Enums.MessageType.received)
                                                             .Skip(15 * skip).Take(15)
                                                             .Select(r => db.Where<Message>(m => m.Id == r.MessageId && m.Visible == true && m.ChannelId == id)
                                                                            .FirstOrDefault()).Where(m => m != null);
                     
                        foreach (var msg in listIdMsg)
                        {
                            MessageBag bag = new MessageBag();
                            User user = db.SelectParam<User>(u => u.Id == msg.SenderId).FirstOrDefault();
                            bag.ChannelId = msg.ChannelId;
                            bag.Date = msg.Date;
                            bag.Id = msg.Id;
                            bag.Receivers = msg.Receivers;
                            bag.SenderId = msg.SenderId;
                            bag.Text = msg.Text;
                            bag.Title = msg.Title;
                            bag.Visible = msg.Visible;
                            bag.UserName = user.FirstName + " " + user.LastName;
                            bag.AvatarUrl = user.AvatarUrl;
                            bag.UserReadStatus = db.Where<ReceiverMessage>(r => r.MessageId == msg.Id)
                                                       .Select(r => r.UserReadStatus).FirstOrDefault();

                            listMessage.Add(bag);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            if (listMessage.Count > 0)
                return listMessage.OrderBy(d => d.Date).ToList();
            else
                return null;
        }

        /// <summary>
        /// lista as mensagens enviadas pelo usuário
        /// </summary>
        /// <returns></returns>
        [HttpGet] 
        public List<MessageBag> SentMessages(int skip)
        {
            List<MessageBag> listMessage = new List<MessageBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
                {
                    try
                    {
                        var msgSend = db.Where<ReceiverMessage>(r => r.UserId == NimbusUser.UserId && r.Status == Model.Enums.MessageType.send)
                                                             .Skip(15 * skip).Take(15)
                                                             .Select(r => db.Where<Message>(m => m.Id == r.MessageId && m.Visible == true).FirstOrDefault());

                        if (msgSend.Count() > 0)
                        {
                            foreach (var item in msgSend)
                            {
                                var user = db.SelectParam<User>(u => u.Id == item.SenderId).FirstOrDefault();
                                MessageBag msg = new MessageBag()
                                {
                                    ChannelId = item.ChannelId,
                                    Date = item.Date,
                                    Id = item.Id,
                                    Receivers = item.Receivers,
                                    SenderId = item.SenderId,
                                    Text = item.Text,
                                    Title = item.Title,
                                    Visible = item.Visible,
                                    UserName =  user.FirstName + " " + user.LastName,
                                    AvatarUrl = user.AvatarUrl
                                };
                                listMessage.Add(msg);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                    }
            }
            return listMessage.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// Troca a visibilidade das mensagens//excluir mensagem 
        /// </summary>
        /// <param name="listID"></param>
        /// <returns></returns>
        [HttpDelete]
        public List<int> DeleteMessages(List<int> listID)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    foreach (int item in listID)
                    {
                        Message message = new Message();
                        //visible= false  quando o usuario mandou ou recebeu a msg
                        db.Update<Message>(message.Visible = false, m => m.Id == item
                                                                             && (m.Receivers.Exists(r => r.UserId == NimbusUser.UserId)
                                                                                  || m.SenderId == NimbusUser.UserId)
                                                                                  );
                        db.Save(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listID;
        }

    }
}
