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
        int paginasaux = 0, paginas = 0, episodios = 0, mangasterminados = 0, contPag = 1, contPagAnt = 0, cantPag = 0;


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

            }
            else
            {
                flipView.FlowDirection = FlowDirection.LeftToRight;
            }
            if (localSettings.Values["readingDirection"].ToString() == "2")
            {
                flipView.ItemsPanel = Resources["vertical"] as ItemsPanelTemplate;
            }
            loading.IsActive = true;
            mangaG = manga;
            MangasG = Mangas;
           

            try
            {
                episodeG = await Clases.Functions.LoadEpisodeAsync(mangaG.GetEpisodes().ElementAt(mangaG.GetActual()).GetDirectory());
                await Clases.Functions.CheckPagesNumber(episodeG);
                await CargarBitmap(-1, false);
                LoadFlipView();
                if (localSettings.Values[mangaG.GetName()] != null && !localSettings.Values[mangaG.GetName()].ToString().Equals("0") && mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido())
                {
                    MoverPagina();
                }

                if (localSettings.Values["AjusteImagen"].ToString() == "1")
                {
                    flipView.ItemTemplate = Resources["AjustarAncho"] as DataTemplate;
                }

                if (localSettings.Values["AjusteImagen"].ToString() == "0")
                {
                    flipView.ItemTemplate = Resources["NoAjustar"] as DataTemplate;
                }
                ActualizarInfo();
                CargarCBox();
                loading.IsActive = false;
                sw.Start();
            }
            catch (Exception)
            {

                loading.IsActive = false;
                var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                BitmapImage image = new BitmapImage
                {
                    UriSource = imageUriForlogo
                };
                flipView.Items.Add(image);
                EpisodeConter.Visibility = Visibility.Visible;
            }

        }

        private void CargarCBox()
        {
            List<String> Pages = new List<String>();
            Pages = episodeG.GetPages();              
            cantPag = Pages.Count();
            SelectedPage.Items.Clear();
            int lenght = cantPag;
            for (int i = 0; i < lenght; i++)
            {
                SelectedPage.Items.Add(i + 1);
            }
            if (SelectedPage.Items.Count == 0)
            {
                //   SelectedPage.SelectedIndex = 0;
            }

        }

        private async void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cargaBitmap) //No hacer nada si se está moviendo de episodio
            {
                if (contPagAnt < flipView.SelectedIndex) //Solo cargar si no está en el flipview
                {
                    contPag++;

                    if (flagepisodio && contPag + 1 >= flipView.Items.Count())
                    {
                        loadingBackgorund.IsActive = true;
                        await CargarBitmap(contPag + 1, false);
                        LoadFlipView();
                        loadingBackgorund.IsActive = false;

                    }
                    contPagAnt = flipView.SelectedIndex;

                }
                else if (contPagAnt > flipView.SelectedIndex)
                {
                    contPag--;
                    contPagAnt = flipView.SelectedIndex;
                }
                ActualizarInfo();
                // Debug.WriteLine(" flipView.SelectedIndex " + (flipView.SelectedIndex + 1) + "  flipView.Items.Count " + flipView.Items.Count);
                if (flipView.Items.Count > 2 && flipView.SelectedIndex + 1 == flipView.Items.Count)
                {
                    SiguienteEpisodio();
                }
                
            }

        }

        private void SiguienteEpisodio()
        {
            MakeVisible();
            if (flagepisodio) //si llegó al final del capitulo actualizar datos y mostrar botones
            {
                paginas = paginas + paginasaux;
                episodios++;

                if (mangaG.GetActual() < mangaG.GetEpisodes().Count() && mangaG.GetActual() >= mangaG.GetUltimoEpisodioLeido() && mangaG.GetUltimoEpisodioLeido() < mangaG.GetEpisodes().Count())
                {
                    mangaG.GetEpisodes().ElementAt<Episode>(mangaG.GetActual()).SetRead(true);
                    mangaG.SetUltimoEpisodioLeido(mangaG.GetActual() + 1);
                    mangaG.SetActual(mangaG.GetActual() + 1);
                    var t = Task.Run(() => Clases.XmlIO.WriteJsonData(MangasG));
                }
                else if (mangaG.GetActual() < mangaG.GetEpisodes().Count())
                {
                    mangaG.SetActual(mangaG.GetActual() + 1);
                }
                flagepisodio = false;
            }
        }

        private async void MoverPagina()
        {
            Int32.TryParse(localSettings.Values[mangaG.GetName()].ToString(), out int pagina);
            if (pagina < cantPag)
            {
                // var watch = System.Diagnostics.Stopwatch.StartNew();
                if (await Clases.Functions.SiNoMensaje(("¿Desea continuar el capitulo en la página " + (pagina + 1) + "?")) == 1)
                {
                    CargarPaginaFlipview(pagina);
                }
                //  watch.Stop();
                // Debug.WriteLine("Tiempo movimiento: " + watch.ElapsedMilliseconds);
            }
        }

        private async void CargarPaginaFlipview(int pagina)
        {
            cargaBitmap = true;
            loading.IsActive = true;
            flipView.IsEnabled = false;
            for (int i = 0; i < pagina; i++)
            {
                contPag++;
                await CargarBitmap(contPag + 1, false);
                LoadFlipView();
            }
            loading.IsActive = false;
            contPag = pagina + 1;
            contPagAnt = pagina;
            MoverPaginaFlipView(pagina);            
            flipView.IsEnabled = true;
            cargaBitmap = false;
        }

        private void MoverPaginaFlipView(int pagina)
        {
            int selectedindex = flipView.SelectedIndex;
            if (selectedindex < pagina)
            {
                for (int i = selectedindex; i <= pagina; i++)
                {
                    if (flipView.Items.Count > i)
                    {
                        flipView.SelectedIndex = i;
                    }
                }
            }
            else
            {
                for (int i = selectedindex; i >= pagina; i--)
                {
                    if (flipView.Items.Count > i)
                    {
                        flipView.SelectedIndex = i;
                    }
                }
            }
            ActualizarInfo();
        }

        private void SelectedPageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedPage.Items.Count > 0)
            {
                if (SelectedPage.SelectedIndex >= flipView.Items.Count)
                {
                    CargarPaginaFlipview(SelectedPage.SelectedIndex);
                }
                else
                {
                    MoverPaginaFlipView(SelectedPage.SelectedIndex);
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
            BtnNext.IsEnabled = false;

            if (flagepisodio) //no ha llegado al final
            {
                if ((mangaG.GetActual() + 1 >= mangaG.GetEpisodes().Count))
                {
                    await Clases.Functions.CreateMessageAsync("No hay más cápítulos");
                }
                else if (!flagepisodio || await Clases.Functions.SiNoMensaje("Aun no termina el capítulo, ¿Desea continuar?") == 1)
                {
                    if (mangaG.GetActual() >= mangaG.GetUltimoEpisodioLeido() && mangaG.GetUltimoEpisodioLeido() < mangaG.GetEpisodes().Count()) //ver si actualizar el ultimo cap leido
                    {
                        mangaG.SetUltimoEpisodioLeido(mangaG.GetActual() + 1);
                    }
                    paginas = paginas + flipView.SelectedIndex + 1;
                    mangaG.SetActual(mangaG.GetActual() + 1);
                    CargarCapitulo();
                } 
            }
            else //Llegó al final del capitulo
            {
                if ((mangaG.GetActual() < mangaG.GetEpisodes().Count && mangaG.GetEpisodes().Count > 1))//si hay mas capitulos
                {
                    CargarCapitulo();
                }
                else
                {
                    await Clases.Functions.CreateMessageAsync("No hay más cápítulos");
                }
            }                                  
            BtnNext.IsEnabled = true;
        }

        private async void CargarCapitulo()
        {
            cargaBitmap = true;

            var t = Task.Run(() => Clases.XmlIO.WriteJsonData(MangasG));
            try
            {
                loading.IsActive = true;
                flipView.Items.Clear();
                MakeInvisible();
                episodeG = await Clases.Functions.LoadEpisodeAsync(mangaG.GetEpisodes().ElementAt(mangaG.GetActual()).GetDirectory());
                flagepisodio = true;
                await CargarBitmap(-1, false);
                LoadFlipView();
                contPag = 1;
                contPagAnt = 0;
            }
            catch (ArgumentOutOfRangeException)
            {
                loading.IsActive = false;
            }
            cargaBitmap = false;
            await Clases.Functions.CheckPagesNumber(episodeG);
            CargarCBox();
            loading.IsActive = false;
        }

        private async void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            BtnPrevous.IsEnabled = false;
            int reduccion = 1;
            Debug.WriteLine(mangaG.GetActual());
            if (mangaG.GetActual() >= 1)
            {
                if (await Clases.Functions.SiNoMensaje("¿Desea ir al capítulo anterior?") == 1)
                {
                    if (!flagepisodio)
                    {
                        reduccion = 2;
                    }
                    paginas = paginas + flipView.SelectedIndex + 1;
                    mangaG.SetActual(mangaG.GetActual() - reduccion);                   
                    CargarCapitulo();
                }
            }
            else
            {
                await Clases.Functions.CreateMessageAsync("No hay más cápítulos");
            }
            ActualizarInfo();            
            BtnPrevous.IsEnabled = true;
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

        private async Task CargarBitmap(int pagina, Boolean flag)
        {
            int inicio = 0, fin = 0;
            if (pagina == -1)
            {
                pagina = 0;
                fin = 3;
            }
            else
            {
                fin = pagina + 1;
            }
            if (flag == true)
            {
                fin = pagina + 1;
                pagina = 3;
            }
            episodeIm = new List<BitmapImage>();
            List<String> Pages = new List<String>();

            if (episodeG.GetPages().Count > 0 && episodeG != null)
            {
                Pages = episodeG.GetPages();
                if (pagina < Pages.Count())
                {
                    paginasaux = cantPag = Pages.Count();
                    List<String> Completeurl = new List<string>();
                    for (inicio = pagina; inicio < fin; inicio++)
                    {
                        if (Pages.Count() == 1 && inicio >= Pages.Count())
                        {
                            SiguienteEpisodio();
                            break;
                        }
                        if (inicio == cantPag)
                            break;
                        Completeurl.Add(Pages[inicio]);
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
                if (await Clases.Functions.SiNoMensaje("Marcar " + mangaG.GetName() + " como terminado?") == 1)
                {
                    mangasterminados++;
                }
            }

            if (flipView.SelectedIndex != 0 && contPag < cantPag && mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido() && flagepisodio)
            {
                await Clases.Functions.CreateMessageAsync("Su progreso será guardado");
                localSettings.Values[mangaG.GetName()] = contPag - 1;
            }
            else if (mangaG.GetActual() == mangaG.GetUltimoEpisodioLeido())
            {
                localSettings.Values[mangaG.GetName()] = 0;
            }
            episodeIm = new List<BitmapImage>();
            flipView.Items.Clear();
            System.GC.Collect();
            var t = Task.Run(() => Clases.XmlIO.WriteJsonStatistics(paginas, episodios, sw, mangasterminados));
            Frame.Navigate(typeof(MainPage), MangasG);
        }

        private void MakeVisible()
        {
            ActualizarInfo();
            BtnClose.Visibility = Visibility.Visible;
            EpisodeConter.Visibility = Visibility.Visible;
            ChapterConter.Visibility = Visibility.Visible;
            BtnNext.Visibility = Visibility.Visible;
            BtnPrevous.Visibility = Visibility.Visible;
            SelectedPage.Visibility = Visibility.Visible;
            flag = true;
            /* if ((mangaG.GetActual() + 1) <= mangaG.GetEpisodes().Count())
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
             }*/
        }
        private void ActualizarInfo()
        {
            int episodioactual = mangaG.GetActual();
            string episodios = mangaG.GetEpisodes().Count().ToString();
            if (flipView.Items.Count != 0)
                EpisodeConter.Content = "Página " + contPag + " de " + cantPag;
            if (!flagepisodio)
                episodioactual--;
            ChapterConter.Content = " Capítulo " + (episodioactual + 1).ToString() + " de " + episodios;        
            

        }

        private void MakeInvisible()
        {
            BtnPrevous.Visibility = Visibility.Collapsed;
            BtnNext.Visibility = Visibility.Collapsed;
            BtnClose.Visibility = Visibility.Collapsed;
            EpisodeConter.Visibility = Visibility.Collapsed;
            ChapterConter.Visibility = Visibility.Collapsed;
            SelectedPage.Visibility = Visibility.Collapsed;
        }
    }
}