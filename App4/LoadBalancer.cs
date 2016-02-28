using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Web.Http;

namespace App4
{
    //https://msdn.microsoft.com/library/hh273122%28v=vs.100%29.aspx
    public class LoadBalancer
    {
        StreamSocketListener listener;
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



        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            DataReader reader = new DataReader(args.Socket.InputStream);
            reader.InputStreamOptions = InputStreamOptions.Partial;

            /*  var bytesAvailable = await reader.LoadAsync(1000);
              var byteArray = new byte[bytesAvailable];
              reader.ReadBytes(byteArray);
              //extract URI?
              System.Text.Encoding.UTF8.GetString(byteArray);*/

            var respuesta=await cliente.GetAsync(new Uri("http://169.254.13.167/"));
            var respuestaString=await respuesta.Content.ReadAsStringAsync();
            Debug.WriteLine(respuestaString);

            DataWriter writer = new DataWriter(args.Socket.OutputStream);
            writer.WriteBytes(Encoding.UTF8.GetBytes(respuestaString));
            await writer.StoreAsync();
            await writer.FlushAsync();

            args.Socket.Dispose();
        }
    }
}
