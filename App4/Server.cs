using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace App4
{
   public abstract class Server
    {
        protected StreamSocketListener listener;
        protected StorageFolder rootDirectory;


        protected async Task<byte[]> getHtmlFileAsBytes(string fileRequested)
        {
            Byte[] bSendData;
            try
            {
                string response = await getFileFormattedHtml(fileRequested);
                Debug.WriteLine("after getfileformattedhtml");
                bSendData = Encoding.UTF8.GetBytes(response);
            }
            catch (Exception e)
            {

                string response = await fileNotFound();
                bSendData = Encoding.UTF8.GetBytes(response);

                Debug.WriteLine(e);
            }

            return bSendData;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        protected string getPathToFile(Byte[] byteArray)
        {
            const int positionOfPathRequested = 1;
            string request = System.Text.Encoding.UTF8.GetString(byteArray);
            string[] pathRequested = request.Split(' ');

            return pathRequested[positionOfPathRequested].TrimStart('/').Replace('/', '\\');
        }
        /// <summary>
        /// Method to find out which HTTP Method the client requested.
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns> A string that contains the name of the HTTP method requested by the client.</returns>
        protected string getHttpMethod(Byte[] byteArray)
        {
            const int positionOfMethod = 0;
            string request = System.Text.Encoding.UTF8.GetString(byteArray);
            string[] pathRequested = request.Split(' ');

            return pathRequested[positionOfMethod];
        }

        protected async Task<string> getFileFormattedHtml(string fileRequested)
        {
            Windows.Storage.StorageFile htmlPageRequested = await rootDirectory.GetFileAsync(fileRequested);
            var properties = await htmlPageRequested.GetBasicPropertiesAsync();
            string htmlPage = "HTTP/1.0 200 OK\r\n" +
                 "Content-Lenght:" + properties.Size + "\r\n" +
                 "Content-type: text/html\r\n" +
                 "Connection: Close\r\n\r\n";

            htmlPage += await Windows.Storage.FileIO.ReadTextAsync(htmlPageRequested);

            return htmlPage;

        }

        protected async Task<byte[]> getBinaryFile(string fileRequested)
        {
            var fileBinaryRequested = await rootDirectory.GetFileAsync(fileRequested);

            IBuffer binaryFileStream = await FileIO.ReadBufferAsync(fileBinaryRequested);
            Byte[] bytes = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(binaryFileStream);
            return bytes;
        }


        protected async Task<string> fileNotFound()
        {

            Windows.Storage.StorageFile htmlErrorFile = await rootDirectory.GetFileAsync("404.html");
            var properties = await htmlErrorFile.GetBasicPropertiesAsync();
            string htmlErrorPage = "HTTP/1.0 404 Not Found\r\n" +
                 "Content-Lenght:" + properties.Size + "\r\n" +
                 "Content-type: text/html\r\n" +
                 "Connection: Close\r\n\r\n";

            htmlErrorPage += await Windows.Storage.FileIO.ReadTextAsync(htmlErrorFile);
            return htmlErrorPage;
        }
    }
}
