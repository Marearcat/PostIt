using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class UsersIndex
    {
        public int Page { get; set; }
        public int GroupId { get; set; }
        public List<PostItCore.Models.User> Users { get; set; }
    }
}
