using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PostItCore.Models
{
    public class User : IdentityUser
    {
        public string Nick { get; set; }
        public int Rep { get; set; }
    }
}
