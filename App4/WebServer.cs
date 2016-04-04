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
namespace App4
{
    //http://www.dzhang.com/blog/2012/09/18/a-simple-in-process-http-server-for-windows-8-metro-apps
    class WebServer : Server
    {
        //https://msdn.microsoft.com/en-us/library/windows/apps/windows.networking.sockets.streamsocketlistener.aspx
        //https://code.msdn.microsoft.com/windowsapps/StreamSocket-Sample-8c573931/sourcecode?fileId=58542&pathId=2049390160
        //https://ms-iot.github.io/content/en-US/win10/samples/BlinkyWebServer.htm
        IReadOnlyList<Windows.Networking.HostName> ipAdresses;

        public WebServer()
        {
            listener =  new StreamSocketListener();
            listener.ConnectionReceived += Listener_ConnectionReceived;
            ipAdresses=NetworkInformation.GetHostNames();
       

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task startServer()
        {
            string port = ConfigData.getSettings("port");
            Debug.WriteLine(port);
            rootDirectory = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync(@"www/");
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


         

            var t =  Task.Run(
                    async () =>
                    {

                        Task.CurrentId.ToString();
                        
                        DataReader reader = new DataReader(args.Socket.InputStream);
                        reader.InputStreamOptions = InputStreamOptions.Partial;
                        var bytesAvailable = await reader.LoadAsync(1000);
                        var byteArray = new byte[bytesAvailable];
                        reader.ReadBytes(byteArray);
                        if (getHttpMethod(byteArray) != "GET")
                        {
                            throw new Exception("Metodo HTTP no soportado");
                        }

                    

                        string fileRequested = getPathToFile(byteArray);


                        Byte[] bSendData;
                        var fileExtension = Path.GetExtension(fileRequested);
                        if (fileExtension==".png" || fileExtension == ".jpg")
                        {
                            bSendData =await getBinaryFile(fileRequested);

                          
                        }
                        else
                        {
                            bSendData = await getHtmlFileAsBytes(fileRequested);
                        }
                        DataWriter writer = new DataWriter(args.Socket.OutputStream);
                        writer.WriteBytes(bSendData);
                        await writer.StoreAsync();
                        
                        await writer.FlushAsync();
    
                         args.Socket.Dispose();
                       reader.Dispose();
                        writer.Dispose();
                               });
            t.Wait();

        }



    }//CLASS

}
