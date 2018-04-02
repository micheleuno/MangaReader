﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=234238

namespace MangaReader
{
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
        Episode episodeG;
        int paginasaux = 0, paginas = 0, episodios = 0, mangasterminados = 0, contPag=1, contPagAnt=0, cantPag=0;

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

            mangaG = manga;
            MangasG = Mangas; 
            episodeG = await Clases.Functions.LoadEpisodeAsync(mangaG.GetEpisodes().ElementAt(mangaG.GetActual()).GetDirectory());
            Clases.Functions.CheckPagesNumber(episodeG);
            loading.IsActive = true;
            try {             
                await CargarBitmap(mangaG.GetActual(),-1,false);
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
                if (localSettings.Values["AjusteImagen"].ToString() == "0")
                {
                    flipView.ItemTemplate = Resources["NoAjustar"] as DataTemplate;
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
     
        private async void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {          
            if (!cargaBitmap) //No hacer nada si se está moviendo de episodio
            {            
                if (contPagAnt < flipView.SelectedIndex) //Solo cargar si no está en el flipview
                {
                    contPag++;
                    EpisodeConter.Content = (contPag).ToString() + " de " + cantPag;
                    if (flagepisodio && contPag+1 >= flipView.Items.Count())
                    {
                        loadingBackgorund.IsActive = true;
                        await CargarBitmap(mangaG.GetActual(), contPag + 1,false);
                        LoadFlipView();
                        loadingBackgorund.IsActive = false;

                    }
                    contPagAnt = flipView.SelectedIndex;

                }
                else if (contPagAnt>flipView.SelectedIndex)
                {                
                    contPag--;
                    EpisodeConter.Content = (contPag).ToString() + " de " + cantPag;
                    contPagAnt = flipView.SelectedIndex;
                }
                     

                if (flipView.Items.Count > 2 && flipView.SelectedIndex + 1 == flipView.Items.Count)
                {
                    SiguienteEpisodio();  
                }
              
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
            if (pagina < cantPag)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                MessageDialog showDialog = new MessageDialog("Desea continuar el capitulo en la página " + (pagina + 1) + "?");
                showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    cargaBitmap = true;
                    loading.IsActive = true;                
                    for (int i = 0; i < pagina; i++)
                    {                   
                        contPag++;
                        await CargarBitmap(mangaG.GetActual(), contPag + 1, false);
                        LoadFlipView();      
                    }                  
                    loading.IsActive = false;
                    contPag = pagina+1;                 
                    for(int i = 1; i <= pagina; i++)
                    {
                        flipView.SelectedIndex = i;
                    }
                  
                    cargaBitmap = false;
                }
                watch.Stop();
                Debug.WriteLine("Tiempo movimiento: " + watch.ElapsedMilliseconds);
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
                cargaBitmap = true;
                BtnNext.IsEnabled = false;
                try
                {
                    loading.IsActive = true;
                    episodeG = await Clases.Functions.LoadEpisodeAsync(mangaG.GetEpisodes().ElementAt(mangaG.GetActual()).GetDirectory());
                    flipView.Items.Clear();                    
                    flagepisodio = true;
                    flipView.Items.Clear();                  
                    await CargarBitmap(mangaG.GetActual(), -1, false);                   
                    LoadFlipView();
                    contPag = 1;
                    contPagAnt = 0;
                    MakeInvisible();
                }
                catch (ArgumentOutOfRangeException)
                {                
                    loading.IsActive = false;
                }
                loading.IsActive = false;
                cargaBitmap = false;
            }
            else
            {
                BtnNext.Content = "Fin del manga";
            }
            BtnNext.IsEnabled = true;  
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
            foreach (BitmapImage value in episodeIm)
            {       
               flipView.Items.Add(value);
            }
           episodeIm = new List<BitmapImage>();
        }

        private async Task CargarBitmap(int capitulo,int pagina,Boolean flag)
        {          
            int inicio = 0, fin = 0;         
            if (pagina == -1)
            {
                pagina = 0;
                fin = 3;
            }
            else
            {               
                fin = pagina+1;
            }
            if (flag == true)
            {
                fin = pagina + 1;
                pagina = 3;
            }          
            episodeIm = new List<BitmapImage>();
            List<String> Pages = new List<String>();          
            String Url = "", Url2;          
            if (episodeG.GetPages().Count > 0 && episodeG != null)
            {
                Pages = episodeG.GetPages();
                if (pagina < Pages.Count())
                {
                    paginasaux = cantPag = Pages.Count();
                    Url = mangaG.GetDirectory() + @"\" + episodeG.GetDirectory();
                    List<String> Completeurl = new List<string>();                  
                    for (inicio = pagina; inicio < fin; inicio++)
                    {                      
                        if (Pages.Count()==1&&inicio >= Pages.Count())
                        {                           
                            SiguienteEpisodio();                          
                            break;
                        }
                        Url2 = Url;
                        Url2 = Url + @"\" + Pages[inicio];
                        Completeurl.Add(Url2);                   
                    }
                    episodeIm = await Clases.Functions.LoadEpisodeImageAsync(Completeurl);
                }
            }
            else
            {
                SiguienteEpisodio();
                await Clases.Functions.CreateMessageAsync("El capitulo no contiene imágenes");
            }
        }

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            sw.Stop();
            if ((mangaG.GetActual() + 1) > mangaG.GetEpisodes().Count())
            {
                if (await Clases.Functions.SiNoMensaje("Marcar " + mangaG.GetName() + " como terminado?") ==1)
                {
                    mangasterminados++;
                }
            }    
           
            if (flipView.SelectedIndex != 0 && contPag  < cantPag && mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido() && flagepisodio)
            {
                await Clases.Functions.CreateMessageAsync("Su progreso será guardado");
                localSettings.Values[mangaG.GetName()] = contPag-1;
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
                EpisodeConter.Content =contPag+ " de " + cantPag;
         
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