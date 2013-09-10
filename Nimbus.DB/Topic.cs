using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Topic
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Channel))]
        public int ChannelId { get; set; }

        [References(typeof(User))]
        public int AuthorId { get; set; }

        public string ImgUrl { get; set; }
        public string UrlCapa {get;set;}
        public string Title { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public Enums.TopicType TopicType { get; set; }
        public bool? Visibility { get; set; }
        public string Text { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string UrlVideo { get; set; }
        public AllQuestion Question { get; set; }
    }

    public class AllQuestion
    {
        public List<Question> Questions { get; set; }
    }

    public class Question
    {
        public string TextQuestion { get; set; }
        public string CorrectAnswer { get; set; }
        public List<Answer> ChoicesAnswer { get; set; }
    }

    public class Answer
    {
        public string TextAnswer { get; set; }
    }

}
