using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class PostInfo
    {
        public string UserId { get; set; }
        public string UserNick { get; set; }
        public int Id { get; set; }
        public string Head { get; set; }
        public string Desc { get; set; }
        public int Rep { get; set; }
        public DateTime Date { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}
