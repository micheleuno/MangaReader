using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaReader.Clases
{
    class Functions
    {
        public static Manga LoadAll(Windows.Storage.StorageFolder folder, String path, String name, String Actual,String direccion)
        {
            string[] folders = System.IO.Directory.GetDirectories(folder.Path, "*", System.IO.SearchOption.AllDirectories);
            Manga manga = new Manga();
            int result;
            manga.SetDirectory(path);
            manga.SetName(name);
            Int32.TryParse(Actual, out result);
            manga.SetUltimoEpisodioLeido(result);
            Int32.TryParse(direccion, out result);
            manga.SetDirección(result);
            for (int i = 0; i < folders.Length; i++)
            {
                Episode episode = new Episode();
                episode.SetDirectory(folders[i]);
                manga.SetEpisode(episode);
            }
            return manga;
        }

        public static Episode LoadEpisode(String path)
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

        public static async Task<List<BitmapImage>> LoadEpisodeImageAsync(List<String> Completeurl)
        {
            List<BitmapImage> images = new List<BitmapImage>();
            BitmapImage image;
            foreach (String value in Completeurl)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync((value));
                IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                image = new BitmapImage();
                await image.SetSourceAsync(fileStream);
                images.Add(image);
            }
            return images;
        }
    }
}
