﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.Models
{
    public class Subscribe
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int GroupId { get; set; }
    }
}
