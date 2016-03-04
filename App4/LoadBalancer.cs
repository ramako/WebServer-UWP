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
    //TODO Crear "tabla hash" donde almacenar la IP origen -> servidor que le corresponde, cuando recibo conexion comprobar si existe esa ip en la tabla
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
       
        //PING
        public async static void getServersHealth()
        {
            string ConnectionAttemptInformation = "";
            HostName server2 = new HostName("192.168.1.30");
            try
            {
                using (var tcpClient = new StreamSocket())
                {
                    await tcpClient.ConnectAsync(
                        server2,
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

        public string dealServer(StreamSocketListenerConnectionReceivedEventArgs args)
        {
            string serverUri = "";
            string value = "";
            if (ipClientList.TryGetValue(args.Socket.Information.RemoteAddress.ToString(), out value) == true)
            {
                serverUri = value;
     //           Debug.WriteLine("IP found in hash table.");
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

            string serverUri = dealServer(args);
          
            var bytesAvailable = await reader.LoadAsync(1000);
              var byteArray = new byte[bytesAvailable];
              reader.ReadBytes(byteArray);

            if(getHttpMethod(byteArray)!="GET")
            {
                throw new Exception ("Metodo HTTP no soportado");
            }
            string fileRequested = getPathToFile(byteArray);

            //if(cache.getImage()==false) y guardar la imagen .

            var uriRequest = new Uri(serverUri + fileRequested);


            var fileExtension = Path.GetExtension(fileRequested);
            if(fileExtension ==".png" | fileExtension == ".jpg")
            {
                cacheServer.getImage(uriRequest);
            }

            try {
                var respuesta = await cliente.GetAsync(uriRequest); 
                await respuesta.Content.WriteToStreamAsync(args.Socket.OutputStream);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
               
            
            args.Socket.Dispose();
        }

        public abstract string balanceRequests();


    }//CLASS

}
