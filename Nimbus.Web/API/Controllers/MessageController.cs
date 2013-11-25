using Nimbus.Web.API.Models;
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
                            SenderId = NimbusUser.UserId,
                            ChannelId = message.ChannelId,
                            Date = DateTime.Now,
                            ReadStatus = false,
                            Text = HttpUtility.HtmlEncode(message.Text),
                            Title = HttpUtility.HtmlEncode(message.Title),
                            Visible = true,
                            Receivers = listReceiver
                        };
                        db.Save(dadosMsg);

                        int idMesg = (int)db.GetLastInsertId();
                        message.Id = idMesg;
                        foreach (var item in listReceiver)
                        {
                            if (item.UserId == NimbusUser.UserId)
                            {
                                db.Insert(new ReceiverMessage
                                {
                                    IsOwner = item.IsOwner,
                                    MessageId = idMesg,
                                    UserId = item.UserId,
                                    NameUser = item.Name,
                                    Status = Nimbus.Model.Enums.MessageType.send
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
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
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
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> listIdMsg = new List<int>();

                    listIdMsg = db.SelectParam<ReceiverMessage>(r => r.UserId == NimbusUser.UserId).Skip(15*skip).Take(15)
                                                                               .Select(r => r.MessageId).ToList();
                    //TODO colocar restricao p/ mostrar somente type.received
                    foreach (int item in listIdMsg)
                    {
                        MessageBag bag = new MessageBag();
                        Message msg = db.SelectParam<Message>(m => m.Visible == true && m.Id == item).First();
                        bag.ChannelId = msg.ChannelId;
                        bag.Date = msg.Date;
                        bag.Id = msg.Id;
                        bag.ReadStatus = msg.ReadStatus;
                        bag.Receivers = msg.Receivers;
                        bag.SenderId = msg.SenderId;
                        bag.Text = msg.Text;
                        bag.Title = msg.Title;
                        bag.Visible = msg.Visible;
                        bag.UserName = db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.FirstName).FirstOrDefault()
                                       + " " + db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.LastName).FirstOrDefault();
                        bag.AvatarUrl = db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.AvatarUrl).FirstOrDefault();

                        listMessage.Add(bag);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listMessage.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// Método que retorna as mensagems enviadas para o canal
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<MessageBag> ChannelReceivedMessages(int id = 0)
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
                        ICollection<int> listIdMsg = db.SelectParam<ReceiverMessage>(rv => rv.UserId == NimbusUser.UserId).Select(rv => rv.MessageId).ToList();
                        //TODO colocar restricao p/ mostrar só type.received

                        foreach (int item in listIdMsg)
                        {
                            MessageBag bag = new MessageBag();
                            Message msg = db.Where<Message>(m => m.Visible == true && m.Id == item).First();
                            bag.ChannelId = msg.ChannelId;
                            bag.Date = msg.Date;
                            bag.Id = msg.Id;
                            bag.ReadStatus = msg.ReadStatus;
                            bag.Receivers = msg.Receivers; //TODO colocar htmlencode
                            bag.SenderId = msg.SenderId;
                            bag.Text = HttpUtility.HtmlDecode(msg.Text);
                            bag.Title = HttpUtility.HtmlDecode(msg.Title);
                            bag.Visible = msg.Visible;
                            bag.UserName = db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.FirstName).FirstOrDefault()
                                           + " " + db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.LastName).FirstOrDefault();
                            bag.AvatarUrl = db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.AvatarUrl).FirstOrDefault();

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
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> msgSend = new List<int>();
                    msgSend = db.SelectParam<ReceiverMessage>(rm => rm.Status == Nimbus.Model.Enums.MessageType.send
                                                                   && rm.UserId == NimbusUser.UserId).Skip(15 * skip).Take(15)
                                                                   .Select(rm => rm.MessageId).ToList();

                    ICollection<Message> messages = db.SelectParam<Message>(m => m.Visible == true && (msgSend.Contains(m.Id) || m.SenderId == NimbusUser.UserId));
                    foreach (var item in messages)
                    {
                        MessageBag msg = new MessageBag()
                        {
                            ChannelId = item.ChannelId,
                            Date = item.Date,
                            Id = item.Id,
                            ReadStatus = item.ReadStatus,
                            Receivers = item.Receivers,
                            SenderId = item.SenderId,
                            Text = HttpUtility.HtmlDecode(item.Text),
                            Title = HttpUtility.HtmlDecode(item.Title),
                            Visible = item.Visible
                        };
                        listMessage.Add(msg);
                    }
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
