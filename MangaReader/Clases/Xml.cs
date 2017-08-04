using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;

namespace MangaReader.Clases
{

    class XmlIO
    {      
        public static async Task<String[]> Readfile()
        {
            // settings
            try
            {
                if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Project1\data.txt"))
                {
                    var path = @"\Project1\data.txt";
                    var folder = ApplicationData.Current.LocalFolder;
                   // Debug.WriteLine(folder.Path);
                    String data2;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    // acquire file
                    StorageFile file = await folder.GetFileAsync(path);
                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read); using

                     (StreamReader reader = new StreamReader(stream.AsStream()))
                    {
                        data2 = reader.ReadToEnd().ToString();
                    }
                    String[] lines = data2.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    /* Debug.Write("1 " + lines[0]);
                     Debug.Write("2 " + lines[1]);
                     Debug.Write("3 " + lines[2]);*/
                    stream.Dispose();
                    return lines;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog(e.ToString());
                await dialog.ShowAsync();
                throw;
            }
        }
    
        public static async Task Writefile( List<Manga> Mangas)
        {
         //   Manga manga = new Manga();
          //  manga = Mangas.ElementAt<Manga>(0);
            String data=null;
            foreach (Manga value in Mangas)
            {
                data = data + value.GetUltimoEpisodioLeido().ToString() + "\n" + value.GetDirectory().ToString() + "\n" + value.GetName().ToString() + "\n" + value.GetDirección().ToString() + "\n";
            }
          
            byte[] encodedText = Encoding.ASCII.GetBytes(data);
            StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
            var projectFolderName = "Project1";
            StorageFolder projectFolder = await rootFolder.CreateFolderAsync(projectFolderName, CreationCollisionOption.OpenIfExists);
            Debug.WriteLine(projectFolder.Path);
            StorageFile sampleFile = await projectFolder.CreateFileAsync("data.txt",CreationCollisionOption.ReplaceExisting);
            using (var writer = await sampleFile.OpenStreamForWriteAsync())
            {
                await writer.WriteAsync(encodedText, 0, encodedText.Length);
                writer.Dispose();
            }
           
            if (Mangas.Count == 0)
            {
               await sampleFile.DeleteAsync();
            }
        }

        public static async Task<String[]> ReadStatistics()
        {
            // settings
            try
            {
                if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Project1\statistics.txt"))
                {
                    var path = @"\Project1\statistics.txt";
                    var folder = ApplicationData.Current.LocalFolder;
                    // Debug.WriteLine(folder.Path);
                    String data2;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    // acquire file
                    StorageFile file = await folder.GetFileAsync(path);
                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read); using

                     (StreamReader reader = new StreamReader(stream.AsStream()))
                    {
                        data2 = reader.ReadToEnd().ToString();
                    }
                    String[] lines = data2.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    /* Debug.Write("1 " + lines[0]);
                     Debug.Write("2 " + lines[1]);
                     Debug.Write("3 " + lines[2]);*/
                    stream.Dispose();
                    return lines;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog(e.ToString());
                await dialog.ShowAsync();
                throw;
            }
        }

        public static async Task WriteStatistics(int paginas, int episodios, Stopwatch sw, int mangasterminados)
        {
            //   Manga manga = new Manga();
            //  manga = Mangas.ElementAt<Manga>(0);
            TimeSpan tiempo;
            String[] previousdata = await ReadStatistics();
            if (previousdata!=null)
            {
                int paginas1;
                Int32.TryParse(previousdata[0], out paginas1);
                paginas = paginas + paginas1;
                int episodios1;
                Int32.TryParse(previousdata[1], out episodios1);
                episodios = episodios + episodios1;
                int mangasterminados1;
                Int32.TryParse(previousdata[3], out mangasterminados1);
                mangasterminados = mangasterminados + mangasterminados1;
                tiempo = sw.Elapsed.Add(TimeSpan.Parse(previousdata[2]));
            }
            else
            {
                tiempo = sw.Elapsed;
              
            }
           

            String data = null;

            
                data = data + paginas.ToString() + "\n" +episodios.ToString() + "\n" +tiempo+"\n"+mangasterminados.ToString();
            
            byte[] encodedText = Encoding.ASCII.GetBytes(data);
            StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
            var projectFolderName = "Project1";
            StorageFolder projectFolder = await rootFolder.CreateFolderAsync(projectFolderName, CreationCollisionOption.OpenIfExists);
           // Debug.WriteLine(projectFolder.Path);
            StorageFile sampleFile = await projectFolder.CreateFileAsync("statistics.txt", CreationCollisionOption.ReplaceExisting);
            using (var writer = await sampleFile.OpenStreamForWriteAsync())
            {
                await writer.WriteAsync(encodedText, 0, encodedText.Length);
                writer.Dispose();
            }
        }
    }
}