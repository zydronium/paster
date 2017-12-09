using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Paster.Models
{
    public class Pastes
    {
        [Key]
        public string Ident { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
        public string MimeType { get; set; }
        public string Ip { get; set; }
        public DateTime Expires { get; set; }
    }
}