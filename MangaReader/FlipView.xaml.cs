using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaReader
{
    /// <summary>
    /// Una página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    /// 
    public abstract class DataTemplateSelector : ContentControl
    {
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return null;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

    public sealed partial class FlipView : Page
    {
        Windows.Storage.ApplicationDataContainer localSettings =
        Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder =
        Windows.Storage.ApplicationData.Current.LocalFolder;
        List<Manga> MangasG = new List<Manga>();
        Manga mangaG;
        Boolean flag = false, flagepisodio = true, cargaBitmap = false;
        Stopwatch sw = new Stopwatch();
        List<BitmapImage> episodeIm;
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
            // if (Mangas.ElementAt<Manga>(0).GetDirección() == 1)

            if (localSettings.Values["readingDirection"].ToString() == "1")
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
            loading.IsActive = true;

            try { 
            
                await CargarBitmap(mangaG.GetActual());
                LoadFlipView();
                if (localSettings.Values[mangaG.GetName()] != null && !localSettings.Values[mangaG.GetName()].ToString().Equals("0") && mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido())
                {
                    MoverPagina();
                }
                loading.IsActive = false;
                if (localSettings.Values["AjusteImagen"].ToString() == "1")
                {
                    flipView.ItemTemplate = Resources["AjustarAncho"] as DataTemplate;
                }
               
                if (localSettings.Values["readingDirection"].ToString() == "2")
                {
                    flipView.ItemsPanel = Resources["vertical"] as ItemsPanelTemplate;
                    BtnClose.HorizontalAlignment = HorizontalAlignment.Center;
                    BtnClose.VerticalAlignment = VerticalAlignment.Top;
                    BtnNext.HorizontalAlignment = HorizontalAlignment.Center;
                    BtnNext.VerticalAlignment = VerticalAlignment.Bottom;
                    BtnFullScreen.HorizontalAlignment = HorizontalAlignment.Right;
                    BtnFullScreen.VerticalAlignment = VerticalAlignment.Center;
                    EpisodeConter.HorizontalAlignment = HorizontalAlignment.Left;
                    EpisodeConter.VerticalAlignment = VerticalAlignment.Center;
                    
                }
                    sw.Start();

            }
            catch (Exception)
            {
                loading.IsActive = false;
                var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                BitmapImage image = new BitmapImage();
                image.UriSource = imageUriForlogo;
                flipView.Items.Add(image);
                EpisodeConter.Visibility = Visibility.Visible;
            }

        }


       /* public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }*/

     
        private async void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //  Debug.WriteLine("Index " + flipView.SelectedIndex +" todos "+ " contador " + flipView.Items.Count);
            EpisodeConter.Content = (flipView.SelectedIndex + 1).ToString() + " de " + flipView.Items.Count.ToString();

           // await Task.Delay(100);

            if (flipView.Items.Count > 2 && flipView.SelectedIndex + 1 == flipView.Items.Count)
            {
                SiguienteEpisodio();  
            }
            await Task.Delay(100);
            if (flipView.Items.Count == 1)
            {
                SiguienteEpisodio();
            }

            if (cargaBitmap == false && flipView.SelectedIndex > (flipView.Items.Count) / 2 && mangaG.GetActual() < (mangaG.GetEpisodes().Count - 1))
            {
                loadingBackgorund.IsActive = true;
                cargaBitmap = true;
                await CargarBitmap(mangaG.GetActual() + 1);
                loadingBackgorund.IsActive = false;
            }

        }

        private void SiguienteEpisodio()
        {
            if (flagepisodio) //si llegó al final del capitulo actualizar datos y mostrar botones
            {
                paginas = paginas + paginasaux;
                episodios++;

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
            MakeVisible();
        } 



    private async void MoverPagina()
        {
            Int32.TryParse(localSettings.Values[mangaG.GetName()].ToString(), out int pagina);
            if (pagina < flipView.Items.Count)
            {
                MessageDialog showDialog = new MessageDialog("Desea continuar el capitulo en la página " + (pagina + 1) + "?");
                showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    flipView.SelectedIndex = pagina;
                }
            }
        }

        private void FlipView_SingleTapped(object sender, TappedRoutedEventArgs e)
        {
            if (flag == false)
            {
                MakeVisible();
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

                if (episodeIm.Count == 0)
                {
                   loading.IsActive = true;
                    cargaBitmap = true;
                    await CargarBitmap(mangaG.GetActual());
                   loading.IsActive = false;
                }

                cargaBitmap = false;
                    try
                    {
                        //  Debug.WriteLine("actual: " + mangaG.GetActual() + "ultimo leido: " + mangaG.GetUltimoEpisodioLeido());
                        flagepisodio = true;
                        LoadFlipView();
                        MakeInvisible();
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        //  Debug.WriteLine(e3);
                        throw;
                    }
                }
                else
                {
                    BtnNext.Content = "End of Manga";
                }

            

           

        }

        private async void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await Task.Delay(1);
            var scrollViewer = sender as ScrollViewer;
            var doubleTapPoint = e.GetPosition(scrollViewer);
            if (scrollViewer.ZoomFactor != 1)
            {
                scrollViewer.ChangeView(doubleTapPoint.X, doubleTapPoint.Y, 1, false);
            }
            else if (scrollViewer.ZoomFactor == 1)
            {
                scrollViewer.ChangeView(doubleTapPoint.X, doubleTapPoint.Y, 2, false);
            }
        }



        private void LoadFlipView()
        {
            int cont = 0;
            flipView.Items.Clear();
            foreach (BitmapImage value in episodeIm)
            {
                cont++;
                flipView.Items.Add(value);
            }
            episodeIm = new List<BitmapImage>();
            System.GC.Collect();
        }

        public FrameworkElement SearchVisualTree(DependencyObject targetElement, string elementName)
        {
            FrameworkElement res = null;
            var count = VisualTreeHelper.GetChildrenCount(targetElement);
            if (count == 0)
                return res;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(targetElement, i);
                if ((child as FrameworkElement).Name == elementName)
                {
                    res = child as FrameworkElement;
                    return res;
                }
                else
                {
                    res = SearchVisualTree(child, elementName);
                    if (res != null)
                        return res;
                }
            }
            return res;
        }



        private async Task CargarBitmap(int capitulo)
        {
            // Debug.WriteLine("Cargando episodio: " + capitulo);
            episodeIm = new List<BitmapImage>();
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            String Url = "", Url2;
            episode = await Clases.Functions.LoadEpisodeAsync(mangaG.GetEpisodes().ElementAt(capitulo).GetDirectory());
            if (episode.GetPages().Count > 0 && episode != null)
            {                
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

                episodeIm = await Clases.Functions.LoadEpisodeImageAsync(Completeurl);              
            }
            else
            {
                await Clases.Functions.CreateMessageAsync("El capitulo no contiene imágenes");
            }

        }


        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            sw.Stop();
            if ((mangaG.GetActual() + 1) > mangaG.GetEpisodes().Count())
            {
                MessageDialog showDialog = new MessageDialog("Marcar " + mangaG.GetName() + " como terminado?");

                showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    mangasterminados++;
                }
            }
            Debug.WriteLine("selected index " + flipView.SelectedIndex + " total " + flipView.Items.Count);
            if (flipView.SelectedIndex != 0 && flipView.SelectedIndex + 1 < flipView.Items.Count && mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido() && flagepisodio)
            {
                await Clases.Functions.CreateMessageAsync("Su progreso será guardado");

                localSettings.Values[mangaG.GetName()] = flipView.SelectedIndex;
            }
            else if (mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido())
            {
                localSettings.Values[mangaG.GetName()] = 0;
            }
            episodeIm = new List<BitmapImage>();
            flipView.Items.Clear();
            System.GC.Collect();
            var t = Task.Run(() => Clases.XmlIO.WriteStatistics(paginas, episodios, sw, mangasterminados));
            Frame.Navigate(typeof(MainPage), MangasG);
        }

        private void MakeVisible()
        {
            int episodioactual = mangaG.GetActual();
            string episodios = mangaG.GetEpisodes().Count().ToString();
            BtnFullScreen.Visibility = Visibility.Visible;
            BtnClose.Visibility = Visibility.Visible;
            if (flipView.Items.Count != 0)
                EpisodeConter.Content =(flipView.SelectedIndex + 1).ToString() + " de " + flipView.Items.Count.ToString();
          //  Windows.Devices.Power.Battery.AggregateBattery.ReportUpdated += AggregateBatteryOnReportUpdated;
            EpisodeConter.Visibility = Visibility.Visible;
            flag = true;
            if ((mangaG.GetActual() + 1) <= mangaG.GetEpisodes().Count())
            {
                BtnNext.Content = "Ir a " + (episodioactual + 1).ToString() + " de " + episodios;
            }
            else
            {
                BtnNext.Content = "Fin";
            }
            if (!flagepisodio)
            {
                BtnNext.Visibility = Visibility.Visible;
            }
           

        }

        private async void  AggregateBatteryOnReportUpdated(Windows.Devices.Power.Battery sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // await UpdatePercentage(sender);  
                var details = sender.GetReport();
                var getPercentage = (details.RemainingCapacityInMilliwattHours.Value / (double)details.FullChargeCapacityInMilliwattHours.Value);
                var getStatus = details.Status;
                string per = getPercentage.ToString("##%");
                string asd="";
                if (per.Length > 0)
                    asd = "Batería restante ";
              //  Battery.Content = (asd+per);
            });
           

        }

        private void MakeInvisible()
        {
            BtnFullScreen.Visibility = Visibility.Collapsed;
            BtnNext.Visibility = Visibility.Collapsed;
            BtnClose.Visibility = Visibility.Collapsed;
            EpisodeConter.Visibility = Visibility.Collapsed;
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