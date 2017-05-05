using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Models.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public RoomViewModel(Room room)
        {
            this.Id = room.Id;
            this.Name = room.Name;
        }
    }
}