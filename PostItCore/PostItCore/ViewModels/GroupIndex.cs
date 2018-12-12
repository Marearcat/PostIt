using PostItCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class GroupIndex
    {
        public int Page { get; set; }
        public string UserId { get; set; }
        public List<Group> Groups { get; set; }
    }
}
