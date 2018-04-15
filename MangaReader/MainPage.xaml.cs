using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MangaReader
{

    public class MenuItem
    {
        public Uri IName
        {
            get; set;
        }
        public String Titulo
        {
            get; set;
        }
    }   

    public sealed partial class MainPage : Page
    {
        private static List<Manga> Mangas = new List<Manga>();

        Windows.Storage.ApplicationDataContainer localSettings =
        Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder =
        Windows.Storage.ApplicationData.Current.LocalFolder;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null)
            {
                if (e.Parameter is List<Manga> MangasParameter && Mangas != null)
                { 
                    LoadGrid();
                    UpdateItems();
                }
            }
            else
            {
                Mangas = new List<Manga>();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();          
        }   

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Main Folder:"+ApplicationData.Current.LocalFolder.Path);
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            InicializarConfiguraciones();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (Mangas.Count == 0)
            {
                Mangas = new List<Manga>();
                Manga Manga1 = new Manga();
                String[] lines = await Clases.XmlIO.Readfile();
              
                if (lines != null)
                {
                    int i = 0;
                    loading.IsActive = true;                   
                    while (i + 1 < lines.Length)
                    {                       
                        Windows.Storage.StorageFolder folder = await OpenFolder(lines[i + 1]); //segunda linea
                        if (folder != null)
                        {
                            Manga1 = Clases.Functions.LoadAll(folder, folder.Path, folder.Name, lines[i], lines[i + 3]);
                            if (Manga1 != null)
                            {
                                Mangas.Add(Manga1);
                            }
                        }
                        i = i + 4;
                    } 
                    //await Clases.XmlIO.WriteJsonAsync(Mangas);
                  //  await Clases.XmlIO.WriteJsonAsyncV2(Mangas);
                    loading.IsActive = false;
                }                
            }
            LoadGrid();
            watch.Stop();
            Debug.WriteLine("Tiempo apertura: " + watch.ElapsedMilliseconds);
            FullScreen_loaded();
            UpdateItems();
        }

        private void InicializarConfiguraciones()
        {
            if (localSettings.Values["readingDirection"] == null)
            {
                localSettings.Values["readingDirection"] = 0;
            }
            if (localSettings.Values["AjusteImagen"] == null)
            {
                localSettings.Values["AjusteImagen"] = 0;
            }
            if (localSettings.Values["FullScrenn"] == null)
            {
                localSettings.Values["FullScrenn"] = 0;
            }
        }

        private void ListTapped(object sender, TappedRoutedEventArgs e)
        {
            int numero = MangaImages.SelectedIndex;
            if (numero == 0)
            {
                AgregarManga();
            }
            else
            {
                numero--;
                if (Mangas.ElementAt(numero).GetUltimoEpisodioLeido() == 0)
                {
                    ContLectura.Text = "Comenzar lectura";
                }
                else
                {
                    String ContEpi = " (" + Mangas.ElementAt(numero).GetUltimoEpisodioLeido() + " de " + (Mangas.ElementAt(numero).GetEpisodes().Count) + ")";
                    ContLectura.Text = "Continuar lectura" + ContEpi;
                }

                NombreManga.Text = Mangas.ElementAt(numero).GetName();
                FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);
            }          
        }

        private async void FlyoutSeleccionarEpisodio(object sender, TappedRoutedEventArgs e)
        {
            int selectedEpi = await InputTextDialogAsync("Seleccionar capitulo de " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName());
            if (selectedEpi!=-1)
            {
                if (selectedEpi < Mangas.ElementAt(MangaImages.SelectedIndex-1).GetUltimoEpisodioLeido())
                {
                    await Clases.Functions.CreateMessageAsync("No se actualizará el ultimo episodio leído");
                }  
                Mangas.ElementAt(MangaImages.SelectedIndex-1).SetActual(selectedEpi);
                Mangas.ElementAt(0).SetMangaActual(MangaImages.SelectedIndex-1);
                Frame.Navigate(typeof(FlipView), Mangas);
            }
        }

        private void FlyoutContinuar(object sender, TappedRoutedEventArgs e)
        {
            ContinuarLectura();
        }       

        private async Task <int> InputTextDialogAsync(string title)
        {
            ContentDialog dialog = new ContentDialog();
            ComboBox cBox = new ComboBox();
            List<Episode> Episodes = new List<Episode>();
            Episodes = Mangas.ElementAt(MangaImages.SelectedIndex-1).GetEpisodes();
            cBox.HorizontalAlignment= HorizontalAlignment.Center;
            int i = 1;
            foreach (Episode value in Episodes)
            {
                cBox.Items.Add(i.ToString());
                i++;
            }
            cBox.SelectedIndex = 0;
            dialog.RequestedTheme = ElementTheme.Dark;
            dialog.Content = cBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ir";
            dialog.SecondaryButtonText = "Cancelar";
            if (cBox.SelectedIndex == -1)
            {
                return -1;
            }
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return cBox.SelectedIndex;
            else
                return -1;
        }

     
       

        private  void  LoadGrid()
        { 
            Episode episode = new Episode();
            ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>();
            try
            {
                items.Add(new MenuItem() { IName = new Uri("ms-appx:///Assets/Agregar.png"),Titulo="Agregar Nuevo" });               
                for (int i = 0; i < Mangas.Count(); i++)
                {
                    if(File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(i).GetName() + ".jpg"))
                    {
                        items.Add(new MenuItem() { IName = new Uri(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(i).GetName() + ".jpg", UriKind.RelativeOrAbsolute), Titulo = Mangas.ElementAt(i).GetName()});
                    }
                    else
                    {
                        items.Add(new MenuItem() { IName = new Uri("ms-appx:///Assets/Imagen.png"), Titulo = Mangas.ElementAt(i).GetName() });
                    }
                }         
            }
            catch (FileNotFoundException)
            {
                    
            }  
            MangaImages.ItemsSource = items;
        }

        private void FullScreen_loaded()
        {
            var ts = fullScreen;
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool syncStatus = view.IsFullScreenMode;
            DataContext = this;
            ts.IsOn = syncStatus;
            ts.Toggled += FullScreen_Toggled;
        }

        private async Task<Windows.Storage.StorageFolder> OpenFolder(String directorio)
        {
            try
            {              
                Windows.Storage.StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directorio);
                return (folder);
            }
            catch (Exception)
            {    
                if (await Clases.Functions.SiNoMensaje(("Ha ocurrido un error en la lectura de: " + directorio.Split('\\').Last() + "\n¿Desea eliminarlo de la lista de mangas?")) == 1)
                {
                    SaveData();
                    Clases.XmlIO.DeleteJson(directorio.Split('\\').Last());
                } 
                return null;
            }
        }

        private async void AgregarManga()
        {
            Boolean flag = false;
            if (Mangas == null)
            {
                Mangas = new List<Manga>();
            }
            Manga Manga1 = new Manga();           
            var picker = new Windows.Storage.Pickers.FolderPicker()
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads,
                SettingsIdentifier = "asd"
            };
            picker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();             
            if (folder != null)
            {
                MangaImages.IsEnabled = false;
                foreach (Manga value in Mangas)
                {
                    if (value.GetName().Equals(folder.Name))
                        flag = true;
                }               
                if (!flag)
                {
                    loadingLoadManga.IsActive = true;
                    await Task.Yield();
                    await Task.Delay(100);
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);
                    Manga1 = (Clases.Functions.LoadAll(folder, folder.Path, folder.Name, "0", "0"));                    
                    if (Manga1 != null)
                    {
                        loadingLoadManga.IsActive = false;
                        Mangas.Add(Manga1);
                        GuardarImagen();
                        SaveData();
                        await Clases.XmlIO.WriteJsonAsync(Mangas);
                        await Task.Delay(100);
                        Mangas = Mangas.OrderBy(o => o.GetName()).ToList();
                        LoadGrid();
                        MangaImages.IsEnabled = true;
                        await Clases.Functions.CreateMessageAsync("Se ha agregado existosamente: " + folder.Name);
                     
                    }
                    else
                    {
                        loadingLoadManga.IsActive = false;
                        await Clases.Functions.CreateMessageAsync("Ha ocurrido un error al agregar: " + folder.Name);
                    }
                    
                }
                else
                {
                    await Clases.Functions.CreateMessageAsync("Ya existe un manga con ese nombre");
                }
            }
            MangaImages.IsEnabled = true;
            loadingLoadManga.IsActive = false;
        }

        private async void GuardarImagen()
        {
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            BitmapImage image1 = new BitmapImage();
            String Url;
            episode = await Clases.Functions.LoadEpisodeAsync(Mangas.ElementAt(Mangas.Count() - 1).GetEpisodes().ElementAt(0).GetDirectory());
           
            if (episode != null && episode.GetPages().Count > 0)
            {
                Pages = episode.GetPages();
                Url = (Mangas.ElementAt(Mangas.Count-1).GetDirectory() + @"\" + episode.GetDirectory() + @"\" + episode.GetPages().ElementAt(0));
                StorageFile file = await StorageFile.GetFileFromPathAsync((Url));
                CopiarImagen(file, Mangas.ElementAt(Mangas.Count - 1).GetName());                              
            }  
        }

        private async void FlyoutCambiarImagen(object sender, TappedRoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            int selecteditem = MangaImages.SelectedIndex - 1;
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null && Mangas.Count > 0 && MangaImages.SelectedIndex - 1 != -1)
            {
                CopiarImagen(file, Mangas.ElementAt(selecteditem).GetName());
                MangaImages.SelectedIndex = selecteditem;
                LoadGrid();
               
            }
        }

        private async void  CopiarImagen(StorageFile file, String Nombre)
        {
            try
            {
                StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                StorageFolder projectFolder = await rootFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);
                // StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + @"\Images");
                Debug.WriteLine(projectFolder.Path + @"\" + Nombre + ".jpg");
                if (File.Exists(projectFolder.Path + @"\" + Nombre + ".jpg"))
                {
                    StorageFile OldFile = await StorageFile.GetFileFromPathAsync(projectFolder.Path + @"\" + Nombre + ".jpg");
                    await OldFile.DeleteAsync();
                }
            
                await file.CopyAsync(projectFolder,Nombre + ".jpg", NameCollisionOption.ReplaceExisting);
            }
            catch(FileNotFoundException)
            {
                await Clases.Functions.CreateMessageAsync("Ocurrió un error al cambiar la imagen");
            }
        }

        private async void ContinuarLectura()
        {
            try
            {
                if (MangaImages.SelectedIndex-1 != -1 && Mangas.ElementAt(MangaImages.SelectedIndex-1).GetUltimoEpisodioLeido() < Mangas.ElementAt(MangaImages.SelectedIndex-1).GetEpisodes().Count)
                {
                    String Title = "¿Desea continuar con el capítulo " + (Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetUltimoEpisodioLeido() + 1) + " de " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName() + "?";
                    if (Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetUltimoEpisodioLeido() == 0)
                    {
                        Title = "¿Desea comenzar la lectura de " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName()+"?";
                    }
                   
                    if (await Clases.Functions.SiNoMensaje(Title)==1 && MangaImages.SelectedIndex - 1 != -1 && Mangas.Count > 0) {
                        Mangas.ElementAt(0).SetMangaActual(MangaImages.SelectedIndex - 1);
                        Mangas.ElementAt(MangaImages.SelectedIndex - 1).SetActual(Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetUltimoEpisodioLeido());
                        if (localSettings.Values[Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName()] == null)
                        {
                            localSettings.Values[Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName()] = 0;
                        }
                        Frame.Navigate(typeof(FlipView), Mangas);
                    }
                }
                else
                {
                    await Clases.Functions.CreateMessageAsync("No hay más episodios, episodio actual: " + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetUltimoEpisodioLeido());
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga primero");
            }
        }

        private void UpdateItems()
        {
            if ((localSettings.Values["FullScrenn"].ToString() == "1"))
            {
                fullScreen.IsOn = true;
            }

            if ((localSettings.Values["readingDirection"].ToString().Length > 0))
            {
                DireccionLectura.SelectedIndex = Int32.Parse(localSettings.Values["readingDirection"].ToString());
            }
            else
            {
                DireccionLectura.SelectedIndex = 0;
            }

            if ((localSettings.Values["AjusteImagen"].ToString().Length > 0))
            {
                AjustarImagen.SelectedIndex = Int32.Parse(localSettings.Values["AjusteImagen"].ToString());
            }
            else
            {
                AjustarImagen.SelectedIndex = 0;
            }
        }

        private void SaveData()
        {
            var t = Task.Run(() => Clases.XmlIO.Writefile(Mangas));
            t.Wait();
        }

        private async void FlyoutEliminar(object sender, TappedRoutedEventArgs e)
        {
            if (MangaImages.SelectedIndex-1 != -1)
            {
               
                String Title = "¿Está seguro de que desea eliminar " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName() + "?";  
                if (await Clases.Functions.SiNoMensaje(Title) == 1)
                {
                    String directorio = Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetDirectory();
                    Clases.XmlIO.DeleteJson(Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName());
                    if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"))
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync((ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"));
                        await file.DeleteAsync();
                    }
                   
                    Mangas.RemoveAt(MangaImages.SelectedIndex-1);
                    SaveData();                   
                    Title = "¿Desea eliminar permanentemente los archivos locales?";
                    if (await Clases.Functions.SiNoMensaje(Title) == 1)
                    {
                        loadingLoadManga.IsActive = true;
                        MangaImages.IsEnabled = false;

                        try
                        {
                            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directorio);
                            await folder.DeleteAsync();
                        }
                        catch (Exception)
                        {
                            await Clases.Functions.CreateMessageAsync("Ocurrió un error al borrar los archivos");
                        }
                        MangaImages.IsEnabled = true;
                        await Clases.Functions.CreateMessageAsync("Se han elimnado los archivos locales");
                        loadingLoadManga.IsActive = false;
                    }
                    LoadGrid();
                }
               
            }
            else
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga primero");
            }           
        }

        private void FullScreen_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool isInFullScreenMode = view.IsFullScreenMode;
            if (isInFullScreenMode)
            {
                view.ExitFullScreenMode();
                localSettings.Values["FullScrenn"] = 0;
            }
            else
            {
                view.TryEnterFullScreenMode();
                localSettings.Values["FullScrenn"] = 1;
            }
        }

        private async void EstadisticasClick(object sender, RoutedEventArgs e)
        {
            String[] previousdata = await Clases.XmlIO.ReadStatistics();
            TimeSpan tiempo; 
            if (previousdata != null)
            {
                int PagT = Int32.Parse(previousdata[0]);
                int CapT = Int32.Parse(previousdata[1]);
                int ManT = Int32.Parse(previousdata[3]);
                tiempo = TimeSpan.Parse(previousdata[2]);
                FLyout.Text = "Páginas leidas: " + PagT.ToString("#,##0") + "\nCapítulos leídos: " + CapT.ToString("#,##0") + "\nMangas terminados: " + ManT.ToString("#,##0") + "\nTiempo total: " + tiempo.ToString(@"d\:hh\:mm");
            }
            else
            {
                FLyout.Text = "No ha leído nada aun :)";
            }
        }            

        private async void FlyoutRecargar(object sender, TappedRoutedEventArgs e)
        {
            if (MangaImages.SelectedIndex-1 != -1 && Mangas.Count > 0)
            {
                MangaImages.IsEnabled = false;
                String Title = " Esto agregará nuevos capitulos agregados en la carpeta, desea continuar? ";  
                if (await Clases.Functions.SiNoMensaje(Title) == 1)
                {                                  
                String directorio = Mangas.ElementAt(MangaImages.SelectedIndex-1).GetDirectory();
                loadingLoadManga.IsActive = true;                
                await Task.Yield();
                await Task.Delay(100);
                try
                {                      
                    string[] folders1 = System.IO.Directory.GetDirectories(directorio, "*", System.IO.SearchOption.AllDirectories);
                    int cantidadActual = Mangas.ElementAt(MangaImages.SelectedIndex-1).GetEpisodes().Count;
                    int cantidadNueva = folders1.Count();
                    if (cantidadNueva > cantidadActual)
                    {
                        for (int i = cantidadActual; i < cantidadNueva; i++)
                        {
                            Episode episode = new Episode();
                            episode.SetDirectory(folders1[i]);
                            Mangas.ElementAt(MangaImages.SelectedIndex-1).SetEpisode(episode);
                        }
                        await Clases.XmlIO.WriteMangaJsonAsync(Mangas.ElementAt(MangaImages.SelectedIndex-1), CreationCollisionOption.ReplaceExisting);
                        MangaImages.IsEnabled = true;
                        loadingLoadManga.IsActive = false;
                        await Clases.Functions.CreateMessageAsync("Se agregaron " + (cantidadNueva - cantidadActual) + " capítulos nuevos");
                    }
                    else
                    {
                        MangaImages.IsEnabled = true;
                        loadingLoadManga.IsActive = false;
                        await Clases.Functions.CreateMessageAsync("No hay nuevos capítulos que agregar");
                    }
                    loadingLoadManga.IsActive = false;

                    }
                    catch (Exception)
                    {
                        await Clases.Functions.CreateMessageAsync("Ha ocurrido un error al recargar el manga");
                    }
                }
                MangaImages.IsEnabled = true;

            }
            else
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga");
            }
        }

        private void AjustarImagen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AjustarImagen.SelectedIndex != -1)
            {
                localSettings.Values["AjusteImagen"] = AjustarImagen.SelectedIndex;
            }
            else
            {
                localSettings.Values["AjusteImagen"] = 0;
            }
        }     

        private void DireccionLectura_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DireccionLectura.SelectedIndex != -1)
            {
                localSettings.Values["readingDirection"] = DireccionLectura.SelectedIndex;
            }
            else
            {
                localSettings.Values["readingDirection"] = 0;
            }
        }
    }
}
