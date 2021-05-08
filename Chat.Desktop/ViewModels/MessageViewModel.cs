using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Desktop.ViewModels
{
    public class MessageViewModel
    {
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public string From { get; set; }
        public string Room { get; set; }
        public string Avatar { get; set; }
        public string AvatarSrc => $"/Images/Avatars/{Avatar}";
    }
}
