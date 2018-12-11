using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class CreateComment
    {
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string Desc { get; set; }
    }
}
