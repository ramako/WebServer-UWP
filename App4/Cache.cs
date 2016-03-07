using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
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

        public async Task<byte[]> getImage(Uri imageRequested)
        {
            Byte[] bytes;
            string fileNameRequested=imageRequested.Segments.Last(); // ultimo segmento ubuntu-logo.png
            StorageFile image;

            Debug.WriteLine("imagen: "+ fileNameRequested);
            try {
                 image = await cache.GetFileAsync(fileNameRequested);

            }catch (Exception e)
            {
                Debug.WriteLine("excepcionr");
                await downloadImage(imageRequested,fileNameRequested);
                 image = await cache.GetFileAsync(fileNameRequested);
            }

            IBuffer binaryFileStream = await FileIO.ReadBufferAsync(image);
            bytes = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(binaryFileStream);
            return bytes;
        }

        public async Task downloadImage(Uri imageRequested, string fileNameRequested)
        {
            try { 
            StorageFile myfile = await cache.CreateFileAsync(fileNameRequested, CreationCollisionOption.ReplaceExisting);
            var response = await cliente.GetAsync(imageRequested);
            var stream=await myfile.OpenAsync(FileAccessMode.ReadWrite);
            await response.Content.WriteToStreamAsync(stream);
            response.Dispose();
            stream.Dispose();
            } catch (Exception e)
            {
                Debug.WriteLine("Error");
            }

        }

    }
}
