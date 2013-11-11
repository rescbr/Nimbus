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

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para as Mensagens
    /// </summary>
    [NimbusAuthorize]
    public class MessageController : NimbusApiController
    {              
        /// <summary>
        /// enviar mensagem através de um canal
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        public Message SendMessageChannel(Message message)
        {           
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //Lembrar: se owner = true, quando mostrar na view colocar: Nimbus     
                            List<Nimbus.Model.Receiver> listReceiver = new List<Model.Receiver>();

                            var roles = db.SelectParam<Role>(r => r.ChannelId == message.ChannelId && (r.MessageManager == true || r.IsOwner == true));

                            foreach (var item in roles)
                            {
                                Nimbus.Model.Receiver receiver = new Model.Receiver();
                                receiver.IsOwner = item.IsOwner;
                                receiver.UserId = item.UserId;
                                receiver.Name = db.SelectParam<User>(u => u.Id == item.UserId).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault();
                                listReceiver.Add(receiver);
                            }
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
                                try
                                {
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
                                    notification.NewMessage(dadosMsg.Title, listReceiver.Select(l => l.UserId).ToList());

                                }
                                catch (Exception ex)
                                {
                                    trans.Rollback();
                                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex)); 
                                }
                                
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex)); 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return message;
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
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //Lembrar: se owner = true, quando mostrar na view colocar: Nimbus   

                            List<Nimbus.Model.Receiver> listReceiver = new List<Nimbus.Model.Receiver>();         
                    
                                Nimbus.Model.Receiver receiver = new Model.Receiver();
                                receiver.IsOwner = true; //a msg é enviada para o perfil, logo não importa esse item
                                receiver.UserId = id;
                                receiver.Name = db.SelectParam<User>(u => u.Id == id).Select(s => s.FirstName + " " + s.LastName).FirstOrDefault();
                                listReceiver.Add(receiver);

                            //add quem enviou para os 'receivers', pois ele deve ter controle sobre o que ele enviou tbm.
                                Nimbus.Model.Receiver sendReceiver = new Model.Receiver();
                                sendReceiver.IsOwner = true;
                                sendReceiver.UserId = NimbusUser.UserId;
                                sendReceiver.Name = NimbusUser.FirstName + " " + NimbusUser.LastName;
                                listReceiver.Add(sendReceiver);

                            //add a  msg                                                 
                            Message dadosMsg = new Message
                            {
                                SenderId = NimbusUser.UserId,
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
                            try
                            {
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
                                notification.NewMessage(dadosMsg.Title, listReceiver.Where(l =>l.UserId != NimbusUser.UserId).Select(l => l.UserId).ToList());

                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                            }

                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return message;
        }
    
        /// <summary>
        /// mostra todas as mensagens que o usuário recebeu, mostra no perfil do usuário
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<MessageBag> ReceivedMessages()
        {
            List<MessageBag> listMessage = new List<MessageBag>();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> listIdMsg = new List<int>();

                    listIdMsg = db.SelectParam<ReceiverMessage>(r => r.UserId == NimbusUser.UserId)
                                                                               .Select(r => r.MessageId).ToList();
                    //TODO colocar restricao p/ mostrar somente type.received
                    foreach (int item in listIdMsg)
                    {
                        MessageBag bag = new MessageBag();
                        Message msg  = db.SelectParam<Message>(m => m.Visible == true && m.Id == item).First();
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
                                       + " " + db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.LastName).FirstOrDefault() ;
                        bag.AvatarUrl = db.SelectParam<User>(u => u.Id == msg.SenderId).Select(u => u.AvatarUrl).FirstOrDefault() ;
                        
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
        public List<Message> SentMessages()
        {
            List<Message> listMessage = new List<Message>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<int> msgSend = new List<int>();
                    msgSend = db.SelectParam<ReceiverMessage>(rm => rm.Status == Nimbus.Model.Enums.MessageType.send 
                                                                   && rm.UserId == NimbusUser.UserId)
                                                                   .Select(rm => rm.MessageId).ToList();

                    ICollection<Message> messages = db.SelectParam<Message>(m => m.Visible == true && (msgSend.Contains(m.Id) || m.SenderId == NimbusUser.UserId));
                    foreach (var item in messages)
                    {
                        Message msg = new Message()
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
