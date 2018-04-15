using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
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
            try
            {
                if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Project1\data.txt"))
                {
                    var path = @"\Project1\data.txt";
                    var folder = ApplicationData.Current.LocalFolder;
                    String data2;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    StorageFile file = await folder.GetFileAsync(path);
                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read); using

                     (StreamReader reader = new StreamReader(stream.AsStream()))
                    {
                        data2 = reader.ReadToEnd().ToString();
                    }
                    String[] lines = data2.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
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
      
        public static async Task Writefile(List<Manga> Mangas)
        {           
            if (Mangas.Count > 0)
            {
                String data = null;
                foreach (Manga value in Mangas)
                {
                    data = data + value.GetUltimoEpisodioLeido().ToString() + "\n" + value.GetDirectory().ToString() + "\n" + value.GetName().ToString() + "\n" + value.GetDirección().ToString() + "\n";
                }

                byte[] encodedText = Encoding.ASCII.GetBytes(data);
                StorageFile sampleFile = await CrearSampleFileAsync();
                using (var writer = await sampleFile.OpenStreamForWriteAsync())
                {
                    await writer.WriteAsync(encodedText, 0, encodedText.Length);
                    writer.Dispose();
                }
            }
            if (Mangas.Count == 0)
            {
                StorageFile sampleFile = await CrearSampleFileAsync();
                await sampleFile.DeleteAsync();
            }
        }

        




        public static async Task<StorageFile> CrearSampleFileAsync()
        {
            StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
            var projectFolderName = "Project1";
            StorageFolder projectFolder = await rootFolder.CreateFolderAsync(projectFolderName, CreationCollisionOption.OpenIfExists);
            StorageFile sampleFile = await projectFolder.CreateFileAsync("data.txt", CreationCollisionOption.ReplaceExisting);
            return sampleFile;
        }

        public static async Task<String[]> ReadStatistics()
        {        
            try
            {
                if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Project1\statistics.txt"))
                {
                    var path = @"\Project1\statistics.txt";
                    var folder = ApplicationData.Current.LocalFolder;
                    String data2;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();                 
                    StorageFile file = await folder.GetFileAsync(path);
                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read); using

                     (StreamReader reader = new StreamReader(stream.AsStream()))
                    {
                        data2 = reader.ReadToEnd().ToString();
                    }
                    String[] lines = data2.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);                  
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
            TimeSpan tiempo;
            String[] previousdata = await ReadStatistics();
            if (previousdata != null)
            {
                Int32.TryParse(previousdata[0], out int paginas1);
                paginas = paginas + paginas1;
                Int32.TryParse(previousdata[1], out int episodios1);
                episodios = episodios + episodios1;
                Int32.TryParse(previousdata[3], out int mangasterminados1);
                mangasterminados = mangasterminados + mangasterminados1;
                tiempo = sw.Elapsed.Add(TimeSpan.Parse(previousdata[2]));
            }
            else
            {
                tiempo = sw.Elapsed;
            }
            String data = null;
            data = data + paginas.ToString() + "\n" + episodios.ToString() + "\n" + tiempo + "\n" + mangasterminados.ToString();
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

        public static async Task WriteJsonAsync(List<Manga> Mangas)
        {           
            foreach (Manga value1 in Mangas)
            {
              await   WriteMangaJsonAsync(value1, CreationCollisionOption.FailIfExists);
            }
        }

        public static async Task WriteJsonAsyncV2(List<Manga> Mangas)
        {
            List<String> directorios = new List<string>();
            List<List<String>> arreglo = new List<List<String>>();

            foreach (Manga value1 in Mangas)
            {

                foreach (Episode value in value1.GetEpisodes())
                {
                    directorios.Add(value.GetDirectory());
                }

                arreglo.Add(directorios);
                directorios = new List<string>();
            }

            string json = JsonConvert.SerializeObject(arreglo);

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("prueba.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);



        }

        public static async Task WriteMangaJsonAsync(Manga manga, CreationCollisionOption option)
        {
            List<String> directorios = new List<string>();
            List<List<String>> arreglo = new List<List<String>>();

            try
            {
                foreach (Episode value in manga.GetEpisodes())
                {
                    directorios.Add(value.GetDirectory());
                }
                string json = JsonConvert.SerializeObject(directorios);

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(manga.GetName() + ".json", option);
                await FileIO.WriteTextAsync(file, json);
            }
            catch (Exception)
            {

            }
            directorios = new List<string>();

        }

        public static List<List<String>> ReadJsonV2()
        {
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\" + "prueba.json"))
            {
                using (StreamReader r = File.OpenText((ApplicationData.Current.LocalFolder.Path + @"\" + "prueba.json")))
                {
                    string json = r.ReadToEnd();
                    List<List<String>> items = JsonConvert.DeserializeObject<List<List<String>>>(json);
                    return items;
                }
            }
            return null;
        }


        public static List<String> ReadJson(String Nombre)
        {
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\" + Nombre + ".json"))
            {
                using (StreamReader r = File.OpenText((ApplicationData.Current.LocalFolder.Path + @"\" + Nombre + ".json")))
                {
                    string json = r.ReadToEnd();
                    List<String> items = JsonConvert.DeserializeObject<List<String>>(json);
                    return items;
                }
            }            
            return null;
        }

        public static void DeleteJson(String Nombre)
        {
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\" + Nombre + ".json"))
            {
                File.Delete((ApplicationData.Current.LocalFolder.Path + @"\" + Nombre + ".json"));
            }
        }
    }
}