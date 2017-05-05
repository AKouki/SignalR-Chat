using Chat.Desktop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Chat.Desktop.Models
{
    public class MessageViewModel
    {
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Avatar { get; set; }

        public BitmapSource B64Source
        {
            get
            {
                return Utils.Base64ToBitmap(Avatar);
            }
        }

    }
}
