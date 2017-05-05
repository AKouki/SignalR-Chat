using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Chat.Web.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; }

        public string Timestamp { get; set; }

        public virtual ApplicationUser FromUser { get; set; }

        [Required]
        public virtual Room ToRoom { get; set; }
    }
}