using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Web.Http;

namespace App4
{
    class Cache
    {
        StorageFolder cache;
        HttpClient cliente;
        public Cache()
        {
             initialization();
            cliente = new HttpClient();
        }

        public async void initialization()
        {
            cache = await KnownFolders.PicturesLibrary.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
            Debug.WriteLine("el directorio es " + cache.Path);
        }

        public async void getImage(Uri imageRequested)
        {
            Debug.WriteLine("imagen: "+ imageRequested);
            try {
                await cache.GetFileAsync("ubuntu-logo.png");
            }catch (Exception e)
            {
                downloadImage(imageRequested);
                Debug.Write(e);
            }
        }

        public async void downloadImage(Uri imageRequested)
        {
            StorageFile myfile = await cache.CreateFileAsync("ubuntu.jpg", CreationCollisionOption.ReplaceExisting);
            var response = await cliente.GetAsync(imageRequested);
         //   var image=await response.Content.ReadAsInputStreamAsync();
            var stream=await myfile.OpenAsync(FileAccessMode.ReadWrite);
            await response.Content.WriteToStreamAsync(stream);

        }

    }
}
