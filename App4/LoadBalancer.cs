using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace App4
{
    //TODO Implementar health status (comrpobar si el servidor de detras esta vivo)
    //https://msdn.microsoft.com/library/hh273122%28v=vs.100%29.aspx
    public abstract class LoadBalancer : Server
    {

        Cache cacheServer;
        HttpClient cliente;
      static  Dictionary<String, String> ipClientList;

        public LoadBalancer()
        {
            ipClientList = new Dictionary<string, string>();
            listener = new StreamSocketListener();
            listener.ConnectionReceived += Listener_ConnectionReceived;
            cliente = new HttpClient();
            cacheServer = new Cache();
        }

        //dhcpcd eth0 to make it work ubuntu server
        public async Task startLoadBalancer(string port) { 
            try
            {
                Debug.WriteLine("Initiating loadbalancer...");
                await listener.BindServiceNameAsync(port);
            }
            catch (Exception)
            {
                var portErrorDialog = new MessageDialog("Port " + port + " is being used by another application.");
                await portErrorDialog.ShowAsync();
            }
        }

        //https://github.com/christophwille/winrt-vasily/blob/facd1741a7ca278ecd99e7ce9336adea408b4917/Source/Vasily/ViewModels/MainPageViewModel.cs

            // Retrieve all IPS.
        public async static void getServersHealth()
        {
            string ConnectionAttemptInformation = "";
            foreach (var item in ConfigData.getIps()) {

                HostName server = new HostName(item.Text);
            try
            {
                using (var tcpClient = new StreamSocket())
                {
                    await tcpClient.ConnectAsync(
                       server,
                        "80",
                        SocketProtectionLevel.PlainSocket);

                    var localIp = tcpClient.Information.LocalAddress.DisplayName;
                    var remoteIp = tcpClient.Information.RemoteAddress.DisplayName;

                    ConnectionAttemptInformation = String.Format("Success, remote server contacted at IP address {0}",
                                                                 remoteIp);
                    tcpClient.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147014836)
                {
                    ConnectionAttemptInformation = "Error: Timeout when connecting (check hostname and port)";
                }
                else
                {
                    ConnectionAttemptInformation = "Error: Exception returned from network stack: " + ex.Message;
                }
            }

                Debug.WriteLine(ConnectionAttemptInformation);
            }


        }

        public string dealServer(StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string serverUri = "";
            string value = "";
            if (ipClientList.TryGetValue(args.Socket.Information.RemoteAddress.ToString(), out value) == true)
            {
                serverUri = value;
                Debug.WriteLine("IP found in hash table.");
            }
            else
            {
                serverUri = balanceRequests();
                ipClientList.Add(args.Socket.Information.RemoteAddress.ToString(), serverUri);

       //         Debug.WriteLine("IP not found in hash table.");
            }

            return serverUri;
        }


        //TODO WinRT information: A connection with the server could not be established
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {

      

            foreach (var item in ipClientList)
                Debug.WriteLine(item);

            DataReader reader = new DataReader(args.Socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;

           
          
            var bytesAvailable = await reader.LoadAsync(1000);
              var byteArray = new byte[bytesAvailable];
              reader.ReadBytes(byteArray);

            if (getHttpMethod(byteArray) != "GET")
            {
                Debug.WriteLine("Metodo HTTP no soportado");
                return;
            }

            string serverUri = dealServer(args);
            string fileRequested = getPathToFile(byteArray);

            var uriRequest = new Uri(serverUri + fileRequested);


            var fileExtension = Path.GetExtension(fileRequested);
            if(fileExtension ==".png" | fileExtension == ".jpg")
            {
               var test= await cacheServer.getImage(uriRequest);
                DataWriter writer = new DataWriter(args.Socket.OutputStream);
                writer.WriteBytes(test);
                await writer.StoreAsync();
                writer.Dispose();

            } else { 

            try {
                var respuesta = await cliente.GetAsync(uriRequest);
                await respuesta.Content.WriteToStreamAsync(args.Socket.OutputStream);
                   
                }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            }
            //               args.Socket.OutputStream.WriteAsync()
          
            args.Socket.Dispose();
        }

        public abstract string balanceRequests();


    }//CLASS

}
