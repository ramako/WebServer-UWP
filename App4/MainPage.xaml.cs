using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


// ACTIVAR CAPABILIDADES DE INTERNET appmanifest
namespace App4
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //Iniciar automaticamente el servidor
           // button_Click(new object(), new RoutedEventArgs());
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            var webServer = new WebServer();
            //NEED TO CHECK FIRST IF PORT 80 IS AVAILABLE
            await webServer.startServer("80");
        }

        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            var loadBalancer = new RoundRobinLoadBalancer();
            await loadBalancer.startLoadBalancer("80");

        }
    }
}
