using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Desktop.Helpers
{
    class User
    {
        public static CookieContainer AuthCookie { get; set; }
        public static string CurrentRoom { get; set; }
    }
}