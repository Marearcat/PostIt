using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string State { get; set; }
    }
}
