using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;

namespace MangaReader
{

    class Manga
    {
        private List<Episode> Episodes = new List<Episode>();
        private List<BitmapImage> FullEpisode = new List<BitmapImage>();
        private int actual=0;
        private int UltimoEpisodioLeido = 0;
        private int ActualManga = 0;
        private int Direccion = 0;
        private String Directory = null;
        private String Name;

        public void SetDirectory(String directory)
        {
            this.Directory = directory;
        }
        public String GetDirectory()
        {
            return this.Directory;
        }

        public void SetName(String name)
        {
            this.Name = name;
        }
        public String GetName()
        {
            return this.Name;
        }
        public void SetEpisode(Episode Directory)
        {
            this.Episodes.Add(Directory);
        }
        public List<Episode> GetEpisodes()
        {
            return this.Episodes;
        }
        public void SetActual(int actual)
        {
            this.actual = actual;
        }
        public int GetActual()
        {
            return this.actual;
        }
        public void SetMangaActual(int Manga)
        {
            this.ActualManga = Manga;
        }
        public int GetMangaActual()
        {
            return this.ActualManga;
        }
        public void SetUltimoEpisodioLeido(int ultimo)
        {
            this.UltimoEpisodioLeido = ultimo;
        }
        public int GetUltimoEpisodioLeido()
        {
            return this.UltimoEpisodioLeido;
        }
        public void SetDirección(int direccion)
        {
            this.Direccion = direccion;
        }
        public int GetDirección()
        {
            return this.Direccion;
        }





    }
}
