using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Desktop.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string CurrentRoom { get; set; }
        public string Avatar { get; set; }
        public string Device { get; set; }
        public string AvatarSrc => $"Images/Avatars/{Avatar}";
    }
}
