using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ApplicationUser Admin { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
