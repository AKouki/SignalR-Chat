using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Chat.Desktop.Helpers
{
    class Utils
    {
        public static BitmapSource Base64ToBitmap(string base64String)
        {
            try
            {
                if (base64String.StartsWith("data:image/false;base64,"))
                    base64String = base64String.Replace("data:image/false;base64,", "");
                else if (base64String.StartsWith("data:image/png;base64,"))
                    base64String = base64String.Replace("data:image/png;base64,", "");

                var bytes = Convert.FromBase64String(base64String);

                using (var stream = new MemoryStream(bytes))
                {
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
