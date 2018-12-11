using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.Models
{
    public class Mail
    {
        [Key]
        public int Id { get; set; }
        public string DepId { get; set; }
        public string DestId { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
