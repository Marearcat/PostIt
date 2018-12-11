using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string AdminId { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public int Rep { get; set; }
    }
}
