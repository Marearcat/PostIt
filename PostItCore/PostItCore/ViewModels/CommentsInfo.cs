using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class CommentsInfo
    {
        public string UserId { get; set; }
        public int Id { get; set; }
        public string UserNick { get; set; }
        public int PostId { get; set; }
        public string Desc { get; set; }
        public DateTime Date { get; set; }
        public int Rep { get; set; }
        public bool Favor { get; set; }
        public int Page { get; set; }
    }
}
