using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
        public string Desc { get; set; }
        public int Rep { get; set; }
        public DateTime Date { get; set; }
    }
}
