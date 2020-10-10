using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chat.Web.Helpers
{
    public class BasicEmojis
    {
        public static string ParseEmojis(string content)
        {
            content = content.Replace(":)", Img("emoji1.png"));
            content = content.Replace(":P", Img("emoji2.png"));
            content = content.Replace(":O", Img("emoji3.png"));
            content = content.Replace(":-)", Img("emoji4.png"));
            content = content.Replace("B|", Img("emoji5.png"));
            content = content.Replace(":D", Img("emoji6.png"));
            content = content.Replace("<3", Img("emoji7.png"));

            return content;
        }

        private static string Img(string imageName)
        {
            return ("<img class=\"emoji\" src=\"/images/emojis/" + imageName + "\">");
        }
    }
}
