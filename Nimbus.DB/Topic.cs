using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class Topic
    {
        public virtual int Id { get; set; }

        public virtual int ChannelId { get; set; }
        public virtual int AuthorId { get; set; }


        public string ImgUrl { get; set; }
        public string UrlCapa { get; set; }
        public string Title { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public Enums.TopicType TopicType { get; set; }
        public bool? Visibility { get; set; }
        public string Text { get; set; }
        public decimal? Price { get; set; }
        public string Description { get; set; }
        public string UrlVideo { get; set; }
        public List<Question> Question { get; set; }
    }

    public class Question
    {
        public string TextQuestion { get; set; }
        public int CorrectAnswer { get; set; }
        public Dictionary<int, string> ChoicesAnswer { get; set; }
    }

}
