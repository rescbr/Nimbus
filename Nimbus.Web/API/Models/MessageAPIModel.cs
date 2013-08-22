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
        send       
    }
    
    /// <summary>
    /// Envia a mensagem privada, criada em um canal, para o dono do canal
    /// </summary>
    public class SendMessageChannelAPI
    {
        public int Channel_ID { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public int receiver_ID{ get; set; }
    }




}