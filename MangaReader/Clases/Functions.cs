﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaReader.Clases
{
    class Functions
    {
        public static Manga LoadAll(Windows.Storage.StorageFolder folder, String path, String name, String Actual,String direccion)
        {
           // Debug.WriteLine("direccion:" + direccion);            
           
            List<String> folders = Clases.XmlIO.ReadJson(name);
            if (folders!=null)
            {               
                Manga manga = new Manga();
                manga.SetDirectory(path);
                manga.SetName(name);
                Int32.TryParse(Actual, out int result);
                manga.SetUltimoEpisodioLeido(result);
                Int32.TryParse(direccion, out result);
                manga.SetDirección(result);
                for (int i = 0; i < folders.Count; i++)
                {
                    Episode episode = new Episode();
                    episode.SetDirectory(folders.ElementAt(i));
                    manga.SetEpisode(episode);
                }
                return manga;
            }
            else
            {               
                string[] folders1 = System.IO.Directory.GetDirectories(folder.Path, "*", System.IO.SearchOption.AllDirectories);
                if (folders1.Count() > 0)
                {
                    Manga manga = new Manga();
                    manga.SetDirectory(path);
                    manga.SetName(name);
                    Int32.TryParse(Actual, out int result);
                    manga.SetUltimoEpisodioLeido(result);
                    Int32.TryParse(direccion, out result);
                    manga.SetDirección(result);
                    for (int i = 0; i < folders1.Count(); i++)
                    {
                        Episode episode = new Episode();
                        episode.SetDirectory(folders1.ElementAt(i));
                        manga.SetEpisode(episode);
                    }
                    return manga;
                }
                
            }
            return null;
        }

        public static async Task<Episode> LoadEpisodeAsync(String path)
        {
            try
            {
                Episode episode = new Episode();
                string[] pages = System.IO.Directory.GetFiles(path).Select(Path.GetFileName).ToArray();
                string[] extensions = new[] { ".png", ".jpg", ".tiff" };
                DirectoryInfo dInfo = new DirectoryInfo(path);
                for (int i = 0; i < pages.Length; i++)
                {
                    episode.AddPage(pages[i]);
                }
                DirectoryInfo info = new DirectoryInfo(path);
                episode.SetDirectory(info.Name);
                return episode;
            }
            catch (DirectoryNotFoundException)
            {
                await CreateMessageAsync("Ocurrión un error al leer el archivo: "+path);
                return null;
            }
        }

        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

        public static async Task<List<BitmapImage>> LoadEpisodeImageAsync(List<String> Completeurl)
        {
            
             List<BitmapImage> images = new List<BitmapImage>();
            BitmapImage image;
            //   Stopwatch sw = new Stopwatch();          
            //sw.Start();
            try
            {
                foreach (String value in Completeurl)
                {

                    if (ImageExtensions.Contains(Path.GetExtension(value).ToUpperInvariant()))
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync((value));
                        IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        image = new BitmapImage();
                        await image.SetSourceAsync(fileStream);
                 

                        images.Add(image);
                    }
                    else
                    {
                        await CreateMessageAsync("Ocurrión un error al leer el archivo: " + value);

                    }
                    // sw.Stop();
                    // Debug.WriteLine("TIempo lectura imagenes: " + sw.Elapsed);
                }
            }
            catch (Exception)
            {
                await CreateMessageAsync("Ocurrión un error al leer el archivo:");
            }        
            return images;
        }

        public static async Task CreateMessageAsync(String mensaje)
        {
            var dialog = new MessageDialog(mensaje);
            await dialog.ShowAsync();
        }
     
    }
   
}
