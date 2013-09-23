using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models.User
{
    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    public class CreateUserAPIModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
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
    /// Listar os dados dos canais pagos adquiridos pelo usuário
    /// </summary>
    public class ChannelUserPaidAPI
    {
        public int channel_ID { get; set; }
        public string ChannelName { get; set; }
        public string UrlImgChannel { get; set; }
    }

    /// <summary>
    /// Edita informações básicas do perfil do usuário
    /// </summary>
    public class EditUserAPIModel
    {
        public string UrlImg { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Occupation { get; set; }
        public string Interest { get; set; }
        public string Experience { get; set; }
        public string About { get; set; }
        public DateTime BirthDate { get; set; }
    }

    ///<sumary>
    ///Exibir as informações de um perfil
    ///</sumary>
    public class ShowProfile
    {
        public int user_ID { get; set; }
        public string UrlImg { get; set; }
        public string Name { get; set; }
        public DateTime BirthDate { get; set; }
        public string Occupation { get; set; }
        public string Interest { get; set; }
        public string Experience { get; set; }
        public string About { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class abstractProfile
    {
        public int idUser { get; set; }
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
    }

}



