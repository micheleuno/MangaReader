using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Power;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
using Windows.System;
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
        ObservableCollection<MenuItem> items = new ObservableCollection<MenuItem>();
        ApplicationDataContainer localSettings =
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
                    FullScreen_loaded();
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

        private  void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Main Folder:"+ApplicationData.Current.LocalFolder.Path);
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            InicializarConfiguraciones();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if (Mangas.Count == 0)
            {
                Mangas = Clases.Functions.CargarDatos();           
               
                FullScreen_loaded();
                LoadGrid();
                UpdateItems();
                watch.Stop();              
            }          
            Debug.WriteLine("Tiempo apertura: " + watch.ElapsedMilliseconds);
         
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

        private void  ListTapped(object sender, TappedRoutedEventArgs e)
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
            //getNumberOfEpisodes();
        }
        
        private async void getNumberOfEpisodes()
        {
            String directorio = Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetDirectory();    
           StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directorio);
            Debug.WriteLine("Hola"+ directorio);
            if (folder != null)
            {
                var sw = new Stopwatch();
                sw.Start();
                var queryOptions = new QueryOptions
                {
                    FolderDepth = FolderDepth.Deep,
                    IndexerOption = IndexerOption.UseIndexerWhenAvailable
                };
                var query = folder.CreateFileQueryWithOptions(queryOptions);
                var allFiles = await query.GetFilesAsync();               
                sw.Stop();
                Debug.WriteLine("Ellapsed time:"+sw.ElapsedMilliseconds+ "Archivos"+allFiles.Count);
            }
        }

        private async void FlyoutInfo(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var dialog = new ContentDialog()
            {
                Title = Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName(),
                RequestedTheme = ElementTheme.Dark,
            };

            // Setup Content
            var panel = new StackPanel();

         
            panel.Children.Add(new TextBlock
            {
                Text = "Directorio: " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetDirectory(),
                TextWrapping = TextWrapping.Wrap,
            });

            panel.Children.Add(new TextBlock
            {
                Text = "Capitulos: " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetEpisodes().Count,
                TextWrapping = TextWrapping.Wrap,
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Capitulos leídos: " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetUltimoEpisodioLeido(),
                TextWrapping = TextWrapping.Wrap,
            });



            var aggBattery = Battery.AggregateBattery;

            // Get report
            var report = aggBattery.GetReport();
            panel.Children.Add(new TextBlock
            {
                Text = "Charge rate (mW): " + report.ChargeRateInMilliwatts.ToString(),
                TextWrapping = TextWrapping.Wrap,
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Design energy capacity (mWh): " + report.DesignCapacityInMilliwattHours.ToString(),
                TextWrapping = TextWrapping.Wrap,
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Fully-charged energy capacity (mWh): " + report.FullChargeCapacityInMilliwattHours.ToString(),
                TextWrapping = TextWrapping.Wrap,
            });
            panel.Children.Add(new TextBlock
            {
                Text = "Remaining energy capacity (mWh): " + report.RemainingCapacityInMilliwattHours.ToString(),
                TextWrapping = TextWrapping.Wrap,
            });

            dialog.SecondaryButtonText = "Abrir directorio";
            dialog.SecondaryButtonClick += async delegate
            {
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetDirectory());
                    await Launcher.LaunchFolderAsync(folder);
                }
                catch (FileNotFoundException)
                {
                   await Clases.Functions.CreateMessageAsync("No se encuentra el directorio");
                   
                }
            };



            dialog.Content = panel;

            // Add Buttons
            dialog.PrimaryButtonText = "OK";
           
          await dialog.ShowAsync();
           
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
                MangaImages.IsEnabled = false;
                Mangas.ElementAt(MangaImages.SelectedIndex-1).SetActual(selectedEpi);
                Mangas.ElementAt(0).SetMangaActual(MangaImages.SelectedIndex-1);
                if (await VerificarDirectorio(MangaImages.SelectedIndex - 1))
                {
                    Frame.Navigate(typeof(FlipView), Mangas);
                }
                MangaImages.IsEnabled = true;
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
            items = new ObservableCollection<MenuItem>();
            try
            {
                items.Add(new MenuItem() { IName = new Uri("ms-appx:///Assets/Agregar.png"),Titulo="Agregar Nuevo" });
                int length = Mangas.Count();
                for (int i = 0; i < length; i++)
                {
                    if(File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(i).GetName() + ".jpg"))
                    {
                        items.Add(new MenuItem() { IName = new Uri(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(i).GetName() + ".jpg" + "?cache=" + new Random().Next()), Titulo = Mangas.ElementAt(i).GetName()});
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

        private void DeleteGrid(int posicion)
        {
            items.RemoveAt(posicion);         
        }

        private void InsertInPosition(String nombre)
        {
            int i = GetPosition(nombre);
            if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + nombre + ".jpg"))
            {
                items.Insert(i,new MenuItem() { IName = new Uri(ApplicationData.Current.LocalFolder.Path + @"\Images\" + nombre + ".jpg" + "?cache=" + new Random().Next()), Titulo = nombre });
            }
            else
            {
                items.Insert(i,new MenuItem() { IName = new Uri("ms-appx:///Assets/Imagen.png"), Titulo = nombre });
            }
        }

        private void FullScreen_loaded()
        {
            BtnCloseApp.Visibility = Visibility.Collapsed;
            var ts = fullScreen;
            ApplicationView view = ApplicationView.GetForCurrentView();
            bool syncStatus = view.IsFullScreenMode;
            DataContext = this;
            ts.IsOn = syncStatus;
            ts.Toggled += FullScreen_Toggled;

            if (view.IsFullScreenMode)
                BtnCloseApp.Visibility = Visibility.Visible;
        }

        private async Task<bool> VerificarDirectorio(int posicion)
        {
          //  var watch = System.Diagnostics.Stopwatch.StartNew();
            String directorio = Mangas.ElementAt(posicion).GetDirectory();
            try
            {  
                Windows.Storage.StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directorio);
                var items = await folder.GetItemsAsync(0, 1);
                if (items.Count == 0)
                {
                    await Clases.Functions.CreateMessageAsync("El Directorio de " + directorio.Split('\\').Last() + "esta vacio");
                    return false;
                }
               // watch.Stop();
              //  Debug.WriteLine("Tiempo revisión: " + watch.ElapsedMilliseconds);

                return (true);
            }
            catch (Exception)
            {    
                if (await Clases.Functions.SiNoMensaje(("Ha ocurrido un error en la lectura de: " + directorio.Split('\\').Last() + "\n¿Desea eliminarlo de la lista de mangas?")) == 1)
                {
                    Mangas.RemoveAt(posicion);
                    DeleteGrid(posicion+1);
                    SaveData();                   
                } 
                return false;
            }           
        }

        private async void AgregarManga()
        {           
            if (Mangas == null)
            {
                Mangas = new List<Manga>();
            }
           
            var picker = new Windows.Storage.Pickers.FolderPicker()
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads,
                SettingsIdentifier = "asd"
            };
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();           


               
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);                   

                loadingLoadManga.IsActive = true;
                MangaImages.IsEnabled = false;
                if (RevisarRepetido(folder))
                {
                    if (!await AgregarMangaFolder(folder))
                    {
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

        private async Task<bool> AgregarMangaFolder(StorageFolder folder)
        {
            Manga Manga1 = new Manga();
           
            await Task.Yield();
            
          
            Manga1 = await (Clases.Functions.LoadAllAsync(folder, folder.Path, folder.Name, "0", "0"));
            if (Manga1 != null)
            {              
                Mangas.Add(Manga1);
                GuardarImagen(folder.Path, folder.Name);               
                Mangas = Mangas.OrderBy(o => o.GetName()).ToList();
                SaveData();
                await Task.Delay(1200);
                InsertInPosition(folder.Name);             
                return true;
            }
            else
            {               
                return false;               
            }
          
        }

        private Boolean RevisarRepetido(StorageFolder folder)
        {
            bool flag = true;
            foreach (Manga value in Mangas)
            {
                if (value.GetName().Equals(folder.Name))
                    flag = false;
            }
            return flag; //true: no está False: ya existe
        }

        private async void DefaultFolderOnClick(object sender, RoutedEventArgs e)
        {
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
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);
                localSettings.Values["DefaultDireectory"] = folder.Path;
                await Clases.Functions.CreateMessageAsync("Se registró " + folder.Path + " como directorio raiz");
            }
        
        }


        private async void Reload_click(object sender, RoutedEventArgs e)
        {
            int error = 0, bien = 0;
           
            if (localSettings.Values["DefaultDireectory"] == null)
            {
                await Clases.Functions.CreateMessageAsync("Debe ingresar un directorio raiz en las configuraciones");
                return;
            }
            if (await Clases.Functions.SiNoMensaje("Se revisarán todos los mangas en la carpeta raiz, ¿desea continuar?") == 1)
            {            
                loadingLoadManga.IsActive = true;
                MangaImages.IsEnabled = false;
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(localSettings.Values["DefaultDireectory"].ToString());
                    IReadOnlyList<StorageFolder> directories = await folder.GetFoldersAsync();
                    foreach (StorageFolder value in directories)
                    {
                        if (RevisarRepetido(value))
                        {                            
                            if( await AgregarMangaFolder(value))
                            {
                                bien++;
                            }
                            else
                            {
                               error++;
                            }
                        }
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    await Clases.Functions.CreateMessageAsync("El directorio raiz no existe");
                    MangaImages.IsEnabled = true;
                    loadingLoadManga.IsActive = false;
                    return;
                }
                loadingLoadManga.IsActive = false;
                await Clases.Functions.CreateMessageAsync("Se agregaron correctamente "+ bien+" mangas\n"+"Hubo errores en "+ error+" carpetas");
              
            }
            MangaImages.IsEnabled = true;
            loadingLoadManga.IsActive = false;
        }


        private int GetPosition(String nombre)
        {
            int length = Mangas.Count;
            for (int i = 0; i < length; i++)
            {
                if (Mangas.ElementAt(i).GetName().Equals(nombre)){
                    return i+1;
                }
            }
            return Mangas.Count+1 ;
        }

        private async void GuardarImagen(String path, String name)
        {
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            BitmapImage image1 = new BitmapImage();
            String Url;
            episode = await Clases.Functions.LoadEpisodeAsync(Mangas.ElementAt(Mangas.Count() - 1).GetEpisodes().ElementAt(0).GetDirectory());
           
            if (episode != null && episode.GetPages().Count > 0)
            {
                Pages = episode.GetPages();
                Url = (episode.GetPages().ElementAt(0));
                StorageFile file = await StorageFile.GetFileFromPathAsync((Url));
                CopiarImagen(file, name);                              
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
                MangaImages.IsEnabled = false;
                CopiarImagen(file, Mangas.ElementAt(selecteditem).GetName());
                await Task.Delay(1200);
                MangaImages.SelectedIndex = selecteditem;                       
                DeleteGrid(selecteditem+1);
                InsertInPosition(Mangas.ElementAt(selecteditem).GetName());
                MangaImages.IsEnabled = true;                          
            }
        }

        private async void  CopiarImagen(StorageFile file, String Nombre)
        {
            try
            {
                StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                StorageFolder projectFolder = await rootFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists); 
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
                        MangaImages.IsEnabled = false;
                        Mangas.ElementAt(0).SetMangaActual(MangaImages.SelectedIndex - 1);
                        Mangas.ElementAt(MangaImages.SelectedIndex - 1).SetActual(Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetUltimoEpisodioLeido());
                        if (localSettings.Values[Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName()] == null)
                        {
                            localSettings.Values[Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName()] = 0;
                        }
                        if (await VerificarDirectorio(MangaImages.SelectedIndex - 1))
                        {                           
                            Frame.Navigate(typeof(FlipView), Mangas);
                        }
                        MangaImages.IsEnabled = true;
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

        private async void SaveData()
        {
         await Clases.XmlIO.WriteJsonEpisodios(Mangas);
         await Clases.XmlIO.WriteJsonData(Mangas);          
        }

        private async void FlyoutEliminar(object sender, TappedRoutedEventArgs e)
        {
            if (MangaImages.SelectedIndex-1 != -1)
            {
               
                String Title = "¿Está seguro de que desea eliminar " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName() + "?";  
                if (await Clases.Functions.SiNoMensaje(Title) == 1)
                {
                    String directorio = Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetDirectory();                   
                    if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"))
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync((ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"));
                        await file.DeleteAsync();
                     
                      
                    }
                    ApplicationData.Current.LocalSettings.Values.Remove(Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName());
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
                            loadingLoadManga.IsActive = false;
                            await Clases.Functions.CreateMessageAsync("Se han elimnado los archivos locales");
                          
                        }
                        catch (Exception)
                        {
                            await Clases.Functions.CreateMessageAsync("Ocurrió un error al borrar los archivos");
                        }
                        loadingLoadManga.IsActive = false;
                        MangaImages.IsEnabled = true;
                                       
                    }
                    
                    DeleteGrid(MangaImages.SelectedIndex);
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
                BtnCloseApp.Visibility = Visibility.Collapsed;
                view.ExitFullScreenMode();
                localSettings.Values["FullScrenn"] = 0;
                
            }
            else
            {
                BtnCloseApp.Visibility = Visibility.Visible;
                view.TryEnterFullScreenMode();
                localSettings.Values["FullScrenn"] = 1;
            }
        }

       

        private  void EstadisticasClick(object sender, RoutedEventArgs e)
        {
            List<string> previousdata =  Clases.XmlIO.ReadJsonEstaditicas();
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

        private async void CloseApp(object sender, RoutedEventArgs e)
        {
           
            if (await Clases.Functions.SiNoMensaje("¿Desea realmente salir?") == 1)
            {
                Application.Current.Exit();
            }
        }

        private async void FlyoutRecargar(object sender, TappedRoutedEventArgs e)
        {
            if (MangaImages.SelectedIndex-1 != -1 && Mangas.Count > 0)
            {
                MangaImages.IsEnabled = false;
                String Title = " Esto agregará nuevos capitulos agregados en la carpeta, ¿desea continuar? ";  
                if (await Clases.Functions.SiNoMensaje(Title) == 1)
                {                                  
                String directorio = Mangas.ElementAt(MangaImages.SelectedIndex-1).GetDirectory();
                loadingLoadManga.IsActive = true;                
                await Task.Yield();
                await Task.Delay(100);
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directorio);
                    IReadOnlyList<StorageFolder> fileList = await folder.GetFoldersAsync();
                    int cantidadActual = Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetEpisodes().Count;
                     
                    int cantidadNueva = fileList.Count;
                       // Debug.WriteLine("cant actual " + cantidadActual + " cantidad nueva " + fileList.ElementAt(0).Name + " manga actual " + directorio);
                        if (cantidadNueva > cantidadActual)
                        {
                            for (int i = cantidadActual; i < cantidadNueva; i++)
                            {
                                Episode episode = new Episode();
                                episode.SetDirectory(fileList.ElementAt(i).Path);
                                Mangas.ElementAt(MangaImages.SelectedIndex - 1).SetEpisode(episode);
                            }
                            SaveData();
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
                }
                    catch (Exception)
                    {
                        await Clases.Functions.CreateMessageAsync("Ha ocurrido un error al recargar el manga");
                    }
                }
                loadingLoadManga.IsActive = false;
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
