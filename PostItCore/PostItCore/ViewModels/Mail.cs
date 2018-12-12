using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostItCore.ViewModels
{
    public class Mail
    {
        public string Text { get; set; }
        public int Page { get; set; }
        public string DepId { get; set; }
        public string DepName { get; set; }
    }
}
