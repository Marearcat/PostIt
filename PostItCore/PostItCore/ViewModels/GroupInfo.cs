using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class GroupInfo
    {
        public bool Sub { get; set; }
        public Models.Group Group { get; set; }
        public string UserId { get; set; }
        public string AdminNick { get; set; }
    }
}
