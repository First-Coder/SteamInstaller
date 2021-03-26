using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SteamInstaller.Extensions
{
    public static class DownloadExtension
    {
        public static bool DownloadAsync(string requestUri, string filename)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri can not be null");
            }

            try
            {
                var client = new WebClient();
                client.DownloadFile(requestUri, filename);
                return true;
            }
            catch(WebException e)
            {

                return false;
            }
        }
    }
}
