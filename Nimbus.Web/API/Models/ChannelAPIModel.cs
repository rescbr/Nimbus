using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models.Channel
{
  
    /// <summary>
    /// Exibe todas as informações sobre um canal.
    /// </summary>
    public class showChannelAPIModel
    {
        public int Organization_ID { get; set; }
        public string ChannelName { get; set; }
        public int owner_ID { get; set; }
        public string CountFollowers { get; set; }
        public string RankingChannel { get; set; }
        public string ParticipationChannel { get; set; }
        public string MessageAlert { get; set; }
        public List<Nimbus.DB.ChannelTag> TagList { get; set; }
        public decimal Price {get;set;}
        public string UrlImgChannel { get; set; }
        public bool isMember { get; set; }
        public bool isPrivate { get; set; }
        //public List<RelatedChannelAPI> RelatedChannelList { get; set; }
    }

    /// <summary>
    /// Exibe informações basicas para os canais relacionados ao canal aberto
    /// </summary>
    public class RelatedChannelAPI 
    {
        public int Organization_ID { get; set; }
        public int channel_ID { get; set; }
        public string ChannelName { get; set; }
        public string UrlImgChannel { get; set; }
    }

    /// <summary>
    /// Exibe informações básicas para listar canais e patrocínios
    /// </summary>
    public class AbstractChannelAPI
    {
        public int Organization_ID { get; set; }
        public int channel_ID { get; set; }
        public string ChannelName { get; set; }
        public string UrlImgChannel { get; set; }
    }

    /// <summary>
    /// Lista os moderadores do canal
    /// <para>(int)moderator_ID, (string)ModeratorName</para>
    /// </summary>
    public class ChannelModaretorAPI
    {
        public int moderator_ID { get; set; }
        public string ModeratorName { get; set; }
    }

    /// <summary>
    /// Lista todas as palavras chaves que representa o canal.
    /// <para>(int)tag_ID, (string)TagName</para>
    /// </summary>
    public class ChannelTagAPI
    {
        public int tag_ID { get; set; }
        public string TagName { get; set; }
    }

    /// <summary>
    /// informações necessárias para se criar um canal
    /// </summary>
    public class NewChannelAPI
    {
        public int Owner_ID { get; set; }
        public int Category_ID { get; set; }
        public int Organization_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool OpenToComments { get; set; }
        public bool IsPrivate { get; set; }
        public bool Visible { get; set; }
        public bool IsCourse { get; set; }
        public decimal Price { get; set; }
        public int Ranking { get; set; }
        public int Followers { get; set; }
    }

    /// <summary>
    /// informações necessárias para editar um canal
    /// </summary>
    public class EditChannelAPI
    {
        public int Channel_ID { get; set; }
        public int Category_ID { get; set; }
        public int Organization_ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public bool OpenToComments { get; set; }
        public bool IsPrivate { get; set; }
        public bool Visible { get; set; }
        public bool IsCourse { get; set; }
        public decimal Price { get; set; }
    }
}