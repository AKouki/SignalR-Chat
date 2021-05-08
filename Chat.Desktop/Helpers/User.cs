using Chat.Desktop.ViewModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Chat.Desktop.Helpers
{
    public class User
    {
        public static CookieContainer AuthCookie { get; set; }
        public static RoomViewModel CurrentRoom { get; set; }
    }
}
