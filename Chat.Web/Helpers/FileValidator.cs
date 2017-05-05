using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Chat.Web.Helpers
{
    public class FileValidator
    {
        public static bool ValidSize(int size)
        {
            // Maximum file size allowed: 500KB
            return (size < 500000);
        }

        public static bool ValidType(string fileName)
        {
            var extenstion = Path.GetExtension(fileName).ToLowerInvariant();

            if (extenstion.Equals(".jpg") || extenstion.Equals(".png"))
                return true;

            return false;
        }
    }
}