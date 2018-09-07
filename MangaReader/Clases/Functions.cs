using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaReader.Clases
{
    class Functions
    {
        public static Manga LoadAll(Windows.Storage.StorageFolder folder, String path, String name, String Actual, String direccion)
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
                    int lenght = folders1.Count();
                    for (int i = 0; i < lenght; i++)
                    {
                        Episode episode = new Episode();
                        episode.SetDirectory(folders1.ElementAt(i));
                        manga.SetEpisode(episode);
                    }
                    return manga;
                

            }
            return null;
        }

        public static async Task<Episode> LoadEpisodeAsync(String path)
        {
            try
            {
                Episode episode = new Episode();
                string[] pages = System.IO.Directory.GetFiles(path).Select(Path.GetFullPath).ToArray();
                string[] extensions = new[] { ".png", ".jpg", ".tiff" };
                DirectoryInfo dInfo = new DirectoryInfo(path);
                int lenght = pages.Length;
                for (int i = 0; i < lenght; i++)
                {
                    episode.AddPage(pages[i]);                    
                }
                DirectoryInfo info = new DirectoryInfo(path);
                episode.SetDirectory(info.Name);
                return episode;
            }
            catch (DirectoryNotFoundException)
            {
                await CreateMessageAsync("Ocurrió un error al leer el archivo: " + path);
                return null;
            }
        }

        public static List<Manga> CargarDatos()
        {
            List<Manga> Mangas = new List<Manga>();
            List<List<String>> arregloMangas =  Clases.XmlIO.ReadJsonEpisodios();
            List<List<String>> arregloDatos = Clases.XmlIO.ReadJsonData();
            int cont=0;
            if (arregloMangas!=null && arregloDatos != null)
            {
                foreach (List<String> value in arregloDatos)
                {
                    Manga manga = new Manga();
                    manga.SetDirectory(value.ElementAt(1));
                    manga.SetName(value.ElementAt(2));
                    Int32.TryParse(value.ElementAt(0), out int result);
                    manga.SetUltimoEpisodioLeido(result);
                    Int32.TryParse(value.ElementAt(3), out result);
                    manga.SetDirección(result);
                    foreach (String value2 in arregloMangas.ElementAt(cont))
                    {
                        Episode episode = new Episode();
                        episode.SetDirectory(value2);
                        manga.SetEpisode(episode);
                    }
                    cont++;

                    Mangas.Add(manga);
                }
            }            
            return Mangas;
        }

        public static async Task  CheckPagesNumber(Episode episode)
        {
            int cont=1;
            Boolean flag = true;
            try
            {            
                if (episode.GetPages().Count > 0 && episode != null)
                {
                    foreach (String value in episode.GetPages())
                    {                       
                        if (value.Length >= 7)
                        {
                            int numero = Int32.Parse(value.Substring(value.Length - 7, 3));
                            if (numero == 0)
                                cont = 0;
                            if (numero != cont)
                            {
                                Debug.WriteLine("numero " + numero);
                                flag = false;
                                break;
                            }

                            cont++;
                        }
                        else
                        {
                            flag = true;
                            break;
                        }
                       
                    }

                    if (!flag)
                    {
                        await CreateMessageAsync("Puede que falten páginas");
                        
                    }
                }
            }
            catch (FormatException)
            {

            }
           
        }
        
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };


        public static async Task<List<BitmapImage>> LoadEpisodeImageAsync(List<String> Completeurl)
        {
            List<BitmapImage> images = new List<BitmapImage>();
            BitmapImage image = new BitmapImage();           
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
                        await CreateMessageAsync("Ocurrió un error al leer el archivo: " + value);
                        var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                        image.UriSource = imageUriForlogo;
                        images.Add(image);
                    }
                                     
                }
            }
            catch (Exception)
            {
                await CreateMessageAsync("Ocurrió un error al leer el archivo");
                var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                image.UriSource = imageUriForlogo;
                images.Add(image);
            }        
            return images;
        }

        public static async Task CreateMessageAsync(String mensaje)
        {
            ContentDialog dialog = new ContentDialog
            {
                RequestedTheme = ElementTheme.Dark,
                Title = mensaje,
                PrimaryButtonText = "Aceptar"
            };
            await dialog.ShowAsync();
        }

        public static async Task<int> SiNoMensaje(string title)
        {
            ContentDialog dialog = new ContentDialog
            {
                RequestedTheme = ElementTheme.Dark,
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Sí",
                SecondaryButtonText = "No"
            };
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return 1;
            }
            else
            {
                return 0;
            }
            
        }
    }

}
