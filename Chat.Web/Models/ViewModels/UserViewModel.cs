﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }

        public UserViewModel(ApplicationUser applicationUser)
        {
            this.Username = applicationUser.UserName;
            this.DisplayName = applicationUser.DisplayName;
            this.Avatar = applicationUser.Avatar;
            this.CurrentRoom = "";
            this.Device = "Unknown";
        }
    }
}