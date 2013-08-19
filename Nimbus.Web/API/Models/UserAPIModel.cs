using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.API.Models.User
{
    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    public class CreateUserAPIModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public bool Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Denuncia um usuário
    /// </summary>
    public class ReportUserAPIModel
    {
        public int userReported_ID { get; set; }
        public string Justification { get; set; }
    }
    
    /// <summary>
    /// Listar as imagens dos canais pagos adquiridos pelo usuário
    /// </summary>
    public class ChannelPayAPI
    {
        public int channel_ID { get; set; }
        public string ChannelName { get; set; }
        public string UrlImgChannel { get; set; }
    }


}



