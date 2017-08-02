using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaReader
{
    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>

    public sealed partial class FlipView : Page
    {
        List<Manga> MangasG = new List<Manga>();
        Manga mangaG;
        Boolean flag = false, flagepisodio = true;
        Stopwatch sw = new Stopwatch();
        int paginasaux = 0, paginas = 0, episodios = 0, mangasterminados = 0;

        public FlipView()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            MangasG = new List<Manga>();
            MakeInvisible();
            List<Manga> Mangas = e.Parameter as List<Manga>;
            Manga manga = Mangas.ElementAt<Manga>(Mangas.ElementAt<Manga>(0).GetMangaActual());
            if (Mangas.ElementAt<Manga>(0).GetDirección() == 1)
            {
                flipView.FlowDirection = FlowDirection.RightToLeft;
                BtnClose.HorizontalAlignment = HorizontalAlignment.Right;
                BtnNext.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                flipView.FlowDirection = FlowDirection.LeftToRight;
            }
            mangaG = manga;
            MangasG = Mangas;
            await LoadFlipView();
            sw.Start();
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //  Debug.WriteLine("Index " + flipView.SelectedIndex +" todos "+ " contador " + flipView.Items.Count);
            if (flipView.SelectedIndex + 1 == flipView.Items.Count && flipView.Items.Count > 2 && flagepisodio)
            {
                paginas = paginas + paginasaux;
                episodios++;
                MakeVisible();
                if (mangaG.GetActual() < mangaG.GetEpisodes().Count() && mangaG.GetActual() >= mangaG.GetUltimoEpisodioLeido() && mangaG.GetUltimoEpisodioLeido() < mangaG.GetEpisodes().Count())
                {

                    mangaG.GetEpisodes().ElementAt<Episode>(mangaG.GetActual()).SetRead(true);
                    mangaG.SetUltimoEpisodioLeido(mangaG.GetActual() + 1);
                    mangaG.SetActual(mangaG.GetActual() + 1);
                    var t = Task.Run(() => Clases.XmlIO.Writefile(MangasG));
                }
                else if (mangaG.GetActual() < mangaG.GetEpisodes().Count())
                {
                    mangaG.SetActual(mangaG.GetActual() + 1);
                }
                flagepisodio = false;
            }
        }

        private void flipView_SingleTapped(object sender, TappedRoutedEventArgs e)
        {
            if (flag == false)
            {
                BtnFullScreen.Visibility = Visibility.Visible;
                BtnClose.Visibility = Visibility.Visible;
                flag = true;
            }
            else
            {
                MakeInvisible();
                flag = false;
            }
        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (mangaG.GetActual() < mangaG.GetEpisodes().Count)
            {
                try
                {
                    Debug.WriteLine("actual: " + mangaG.GetActual() + "ultimo leido: " + mangaG.GetUltimoEpisodioLeido());
                    flagepisodio = true;
                    await LoadFlipView();
                }
                catch (ArgumentOutOfRangeException e3)
                {
                    Debug.WriteLine(e3);
                    throw;
                }
            }
            else
            {
                BtnNext.Content = "End of Manga";
            }
        }

        private void scrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            var doubleTapPoint = e.GetPosition(scrollViewer);
            if (scrollViewer.ZoomFactor != 1)
            {
                scrollViewer.ChangeView(doubleTapPoint.X, doubleTapPoint.Y, 1);
            }
            else if (scrollViewer.ZoomFactor == 1)
            {
                scrollViewer.ChangeView(doubleTapPoint.X, doubleTapPoint.Y, 2);
            }
        }

        private async Task LoadFlipView()
        {
            loading.IsActive = true;
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            String Url, Url2;
            episode = Clases.Functions.LoadEpisode(mangaG.GetEpisodes().ElementAt<Episode>(mangaG.GetActual()).GetDirectory());
            int cont = 0;
            Pages = episode.GetPages();
            paginasaux = Pages.Count;
            Url = mangaG.GetDirectory() + @"\" + episode.GetDirectory();
            List<String> Completeurl = new List<string>();
            foreach (String value in Pages)
            {
                //  Debug.Write("Episodes " + value);
                Url2 = Url;
                Url2 = Url + @"\" + value;

                Completeurl.Add(Url2);
            }
            MakeInvisible();
            List<BitmapImage> episodeIm = await Clases.Functions.LoadEpisodeImageAsync(Completeurl);
            for (int i = 0; i < flipView.Items.Count - 1; i++)
                flipView.Items.RemoveAt(i);

            foreach (BitmapImage value in episodeIm)
            {
                cont++;
                flipView.Items.Add(value);
                if (cont == 5 && flipView.Items.Count > 5)
                    flipView.SelectedIndex += 1;
            }
            loading.IsActive = false;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            sw.Stop();

            var t = Task.Run(() => Clases.XmlIO.WriteStatistics(paginas, episodios, sw, mangasterminados));
            Frame.Navigate(typeof(MainPage), MangasG);
        }
        private void MakeVisible()
        {
            BtnNext.Visibility = Visibility.Visible;
            BtnClose.Visibility = Visibility.Visible;
            if ((mangaG.GetActual() + 2) <= mangaG.GetEpisodes().Count())
            {
                BtnNext.Content = "Ir a " + (mangaG.GetActual() + 2).ToString() + " de " + mangaG.GetEpisodes().Count().ToString();
            }
            else
            {
                mangasterminados++;
                BtnNext.Content = "Fin";
            }

        }

        private void MakeInvisible()
        {
            BtnFullScreen.Visibility = Visibility.Collapsed;
            BtnNext.Visibility = Visibility.Collapsed;
            BtnClose.Visibility = Visibility.Collapsed;
        }

        private void Fullscren(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }


    }
}