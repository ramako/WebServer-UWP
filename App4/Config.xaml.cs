using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App4
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Config : Page
    {
       
        public Config()
        {
            this.InitializeComponent();
            var listaIps= ConfigData.getIps();
            Debug.WriteLine(mainConfigStackPanel.Children.Count);
            foreach (var item in listaIps)
            {
                item.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                item.Visibility = Visibility.Visible;

                mainConfigStackPanel.Children.Add(item);
            }
        }

        private void saveConfig_Click(object sender, RoutedEventArgs e) {
            ConfigData.saveSettings("port", webServerPort.Text);
         
          
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            var listaIps = ConfigData.getIps();
            foreach (var item in listaIps)
            {
                mainConfigStackPanel.Children.Remove(item);
            }
            this.Frame.Navigate(typeof(MainPage));
        }

        private void addServer_Click(object sender, RoutedEventArgs e)
        {
            
            var test = new TextBox();
            ConfigData.addIp(test);
            test.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            test.Visibility = Visibility.Visible;
            mainConfigStackPanel.Children.Add(test);
            Debug.WriteLine(ConfigData.getIps()[0].Text);
        }
    }
}
