using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models.Message
{
    /// <summary>
    /// Tipos de mensagens que são exibidas no perfil do usuário
    /// </summary>
    public enum TypeMessage 
    {
        received,
        send,
        deleted,        
    }

    /// <summary>
    /// Api para criar mensagem para o usuário dono do canal
    /// </summary>
    public class CreateMessageAPI
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Exibe as mensagens no perfil do usuário,
    /// mensagens vem dos canais.
    /// </summary>
    public class ReceiveMessageAPI
    {
        public int userSend_ID { get; set; }
        public string UrlImgUser { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string Message { get; set; }
        public DateTime DateMessage { get; set; }
        public bool Read { get; set; }

    }

    /// <summary>
    /// Envia a mensagem privada, criada em um canal, para o dono do canal
    /// </summary>
    public class SendMessageChannelAPI
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
        public string receiverName { get; set; }
    }




}