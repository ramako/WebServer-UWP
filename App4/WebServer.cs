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
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;


//TODO Refactorizar para que quede ordenado mostrar imagenes y html
//TODO Excepcion si el METODO HTTP no es GET
//TODO Un balanceador de carga basico.
namespace App4
{
    //http://www.dzhang.com/blog/2012/09/18/a-simple-in-process-http-server-for-windows-8-metro-apps
    class WebServer
    {
        //https://msdn.microsoft.com/en-us/library/windows/apps/windows.networking.sockets.streamsocketlistener.aspx
        //https://code.msdn.microsoft.com/windowsapps/StreamSocket-Sample-8c573931/sourcecode?fileId=58542&pathId=2049390160
        //https://ms-iot.github.io/content/en-US/win10/samples/BlinkyWebServer.htm
        StreamSocketListener listener;
        IReadOnlyList<Windows.Networking.HostName> ipAdresses;

        public WebServer()
        {
            listener =  new StreamSocketListener();
            listener.ConnectionReceived += Listener_ConnectionReceived;
            ipAdresses=NetworkInformation.GetHostNames();

        }
        public async Task startServer(string port)
        {
            try {
                
                foreach(var item in ipAdresses)
                {
                    await new MessageDialog(item.ToString()).ShowAsync();
                }
                Debug.WriteLine("Initiating webserver...");
                await listener.BindServiceNameAsync(port);
            }
            catch (Exception)
            {
                var portErrorDialog = new MessageDialog("Port " + port + " is being used by another application.");
                await portErrorDialog.ShowAsync();
            }
        }


        //http://stackoverflow.com/questions/3670057/does-console-writeline-block
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {

           DataReader reader = new DataReader(args.Socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;

            
               var t = Task.Run(
                    async () =>
                    {
                                // estas 3 lineas en una funcion : var byteArray= loadHttpRequest()
                        var bytesAvailable = await reader.LoadAsync(1000);
                        var byteArray = new byte[bytesAvailable];
                        reader.ReadBytes(byteArray);

                        DataWriter writer = new DataWriter(args.Socket.OutputStream);

                        var rootDirectory =  await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"www/");
                        string fileRequested = getPathToFile(byteArray);
                        Debug.WriteLine("File requested is " + fileRequested);

                        if(fileRequested.Contains(".png"))
                        {
                            //LEER FICHERO Y ENVIARLO
                           var fileBinaryRequested= await rootDirectory.GetFileAsync(fileRequested);

                           IBuffer binaryFileStream= await FileIO.ReadBufferAsync(fileBinaryRequested);
                            Byte[] bytes = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(binaryFileStream);
                            writer.WriteBytes(bytes);
                            //fileBinaryRequested.

                        }
                        //FIRST READ AND TRY TO SEND THE FILE...THEN FIGURE IT OUT HOW TO DESIGN IT PROPERLY
                        else { 
                            try {
                                string response = await getFileFormattedHtml(rootDirectory,fileRequested);
                                Debug.WriteLine("after getfileformattedhtml");
                                Byte[] bSendData = Encoding.UTF8.GetBytes(response);
                                writer.WriteBytes(bSendData);
                            }
                            catch (Exception e)
                            {
                            
                               string response= await fileNotFound(rootDirectory);
                                Byte[] bSendData = Encoding.UTF8.GetBytes(response);
                                writer.WriteBytes(bSendData);
                                Debug.WriteLine(e);
                            }
                        }

                        await writer.StoreAsync();
                        await writer.FlushAsync();
    
                         args.Socket.Dispose();
                                });

            
    

        }

        private string getPathToFile(Byte [] byteArray)
        {
            const int positionOfPathRequested = 1;
            string request = System.Text.Encoding.UTF8.GetString(byteArray);
            string [] pathRequested = request.Split(' ');

            return  pathRequested[positionOfPathRequested].TrimStart('/').Replace('/','\\');
        }


        private async Task <string> getFileFormattedHtml(Windows.Storage.StorageFolder rootDirectory, string fileRequested)
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

        private async Task<string>  fileNotFound(Windows.Storage.StorageFolder rootDirectory)
        {
    
            Windows.Storage.StorageFile htmlErrorFile= await rootDirectory.GetFileAsync("404.html");
            var properties = await htmlErrorFile.GetBasicPropertiesAsync();
            string htmlErrorPage = "HTTP/1.0 404 Not Found\r\n" +
                 "Content-Lenght:" + properties.Size + "\r\n" +
                 "Content-type: text/html\r\n" +
                 "Connection: Close\r\n\r\n";
            
            htmlErrorPage+= await Windows.Storage.FileIO.ReadTextAsync(htmlErrorFile);
            return htmlErrorPage;
        }

    }//CLASS

}
