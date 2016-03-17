using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace App4
{
    static class ConfigData
    {
       static List<TextBox> serverIps = new List<TextBox>();
        static ApplicationDataContainer AppSettings = ApplicationData.Current.LocalSettings;

      public  static void addIp(TextBox ip)
        {
            serverIps.Add(ip);
        }

       public static void saveSettings(string key, string value)
        {
            AppSettings.Values[key] = value;
        }
        
      public  static string getSettings(string key)
        {
            return (string)AppSettings.Values[key];
        }

        public static List<TextBox> getIps()
        {
            return serverIps;
        }

    }
}
