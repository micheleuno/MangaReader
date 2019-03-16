using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace MangaReader.Clases
{

    class XmlIO
    {

        public static async Task WriteJsonData(List<Manga> Mangas)
        {
            List<String> datosManga = new List<string>();
            List<List<String>> arregloMangas = new List<List<String>>();
            if (Mangas.Count > 0)
            {
                foreach (Manga value in Mangas)
                {
                    datosManga.Add( value.GetUltimoEpisodioLeido().ToString());
                    datosManga.Add(value.GetDirectory().ToString());
                    datosManga.Add(value.GetName().ToString());
                    datosManga.Add(value.GetDirección().ToString());
                    arregloMangas.Add(datosManga);
                    datosManga = new List<string>();
                }
            }
            string json = JsonConvert.SerializeObject(arregloMangas);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("Datos.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }

        public static List<List<String>> ReadJsonData()
        {
            if (!File.Exists(ApplicationData.Current.LocalFolder.Path + @"\" + "Datos.json"))
            {
                return null;
            }
            using (StreamReader r = File.OpenText((ApplicationData.Current.LocalFolder.Path + @"\" + "Datos.json")))
            {
                string json = r.ReadToEnd();
                List<List<String>> items = JsonConvert.DeserializeObject<List<List<String>>>(json);
                return items;
            }            
        }

        public static async Task WriteJsonStatistics(int paginas, int episodios, Stopwatch sw, int mangasterminados)
        {
            List<String> estadisticas = new List<string>();
            TimeSpan tiempo;
            List<String> previousdata = ReadJsonEstaditicas();
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
            estadisticas.Add(paginas.ToString());
            estadisticas.Add(episodios.ToString());
            estadisticas.Add(tiempo.ToString());
            estadisticas.Add(mangasterminados.ToString());

            string json = JsonConvert.SerializeObject(estadisticas);
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("estadisticas.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, json);
        }

        public static List<String> ReadJsonEstaditicas()
        {
            if (!File.Exists(ApplicationData.Current.LocalFolder.Path + @"\" + "estadisticas.json"))
            {
               return null;
            }
            using (StreamReader r = File.OpenText((ApplicationData.Current.LocalFolder.Path + @"\" + "estadisticas.json")))
            {
                string json = r.ReadToEnd();
                List<String> items = JsonConvert.DeserializeObject<List<String>>(json);
                return items;
            }            
        }

        public static async Task WriteJsonEpisodios(List<Manga> Mangas)
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

        public static List<List<String>> ReadJsonEpisodios()
        {
            if (!File.Exists(ApplicationData.Current.LocalFolder.Path + @"\" + "prueba.json"))
            {
                return null;
            }
            using (StreamReader r = File.OpenText((ApplicationData.Current.LocalFolder.Path + @"\" + "prueba.json")))
            {
                string json = r.ReadToEnd();
                List<List<String>> items = JsonConvert.DeserializeObject<List<List<String>>>(json);
                return items;
            }
           
        }
    }
}