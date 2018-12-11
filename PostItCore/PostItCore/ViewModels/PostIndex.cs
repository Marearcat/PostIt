using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class PostIndex
    {
        public int Page { get; set; }
        public List<Models.Post> Posts { get; set; }
    }
}
