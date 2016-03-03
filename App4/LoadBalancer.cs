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

        HttpClient cliente;
      static  Dictionary<String, String> ipClientList;

        public LoadBalancer()
        {
            ipClientList = new Dictionary<string, string>();
            listener = new StreamSocketListener();
            listener.ConnectionReceived += Listener_ConnectionReceived;
            cliente = new HttpClient();
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
       
        public static void getServersHealth()
        {
           // Debug.WriteLine("dubidubi du du dubidu!");
         
        }


        //TODO WinRT information: A connection with the server could not be established
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {

            foreach (var item in ipClientList)
                Debug.WriteLine(item);
            string value = "";
      //      Debug.Write(ipClientList.TryGetValue(args.Socket.Information.RemoteAddress, out value));
            DataReader reader = new DataReader(args.Socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;

           
            string serverUri = ""; //servidor que le toca, hacer metodo getServer() que devuelva el serverUri necesario
            if (ipClientList.TryGetValue(args.Socket.Information.RemoteAddress.ToString(), out  value)==true)
            {
                serverUri = value;
       //         Debug.WriteLine("Ha encontrado mi ip");
            } else
            {
                serverUri = balanceRequests();
          //      Debug.Write(ipClientList.TryGetValue(args.Socket.Information.RemoteAddress, out value));
                ipClientList.Add(args.Socket.Information.RemoteAddress.ToString(), serverUri);
      
                Debug.WriteLine("No ha encontrado mi IP");
            }

     //       Debug.WriteLine("server uri: " + serverUri);

            var bytesAvailable = await reader.LoadAsync(1000);
              var byteArray = new byte[bytesAvailable];
              reader.ReadBytes(byteArray);

            if(getHttpMethod(byteArray)!="GET")
            {
                throw new Exception ("Metodo HTTP no soportado");
            }
            string fileRequested = getPathToFile(byteArray);

           

            var uriRequest = new Uri(serverUri + fileRequested);


            var fileExtension = Path.GetExtension(fileRequested);

                var respuesta=await cliente.GetAsync(uriRequest); // en try catch ?
               await respuesta.Content.WriteToStreamAsync(args.Socket.OutputStream);
            
            args.Socket.Dispose();
        }

        public abstract string balanceRequests();


    }//CLASS

}
