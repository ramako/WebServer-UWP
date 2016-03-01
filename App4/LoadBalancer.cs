using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public LoadBalancer()
        {
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

        public 



        //TODO WinRT information: A connection with the server could not be established
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;

              var bytesAvailable = await reader.LoadAsync(1000);
              var byteArray = new byte[bytesAvailable];
              reader.ReadBytes(byteArray);

            if(getHttpMethod(byteArray)!="GET")
            {
                throw new Exception ("Metodo HTTP no soportado");
            }
            string fileRequested = getPathToFile(byteArray);

            string serverUri = balanceRequests();

            var uriRequest = new Uri(serverUri + fileRequested);


            var fileExtension = Path.GetExtension(fileRequested);

                var respuesta=await cliente.GetAsync(uriRequest); // en try catch ?
               await respuesta.Content.WriteToStreamAsync(args.Socket.OutputStream);
                balanceRequests();
            
            args.Socket.Dispose();
        }

        public abstract string balanceRequests();


    }//CLASS

}
