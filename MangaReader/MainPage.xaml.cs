using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Popups;
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
                  //  PopulateCBoxManga();
                    UpdateItems();
                    LoadGrid();
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
            Debug.WriteLine("asdasdasda:"+ApplicationData.Current.LocalFolder.Path);
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if (localSettings.Values["readingDirection"] == null)
            {
                localSettings.Values["readingDirection"] = 0;
            }
            if (localSettings.Values["AjusteImagen"] == null)
            {
                localSettings.Values["AjusteImagen"] = 1;
            }
            if (localSettings.Values["FullScrenn"] == null)
            {
                localSettings.Values["FullScrenn"] = 0;
            }
            if (Mangas.Count == 0)
            {              
               
                FullScreen_loaded();
                Mangas = new List<Manga>();
                Manga Manga1 = new Manga();
                String[] lines = await Clases.XmlIO.Readfile();
                var watch = System.Diagnostics.Stopwatch.StartNew();
                if (lines != null)
                {
                    int i = 0;
                    loading.IsActive = true;
                   
                    while (i + 1 < lines.Length)
                    {
                        foreach (String value in lines)
                        {
                            //  Debug.WriteLine(value);
                        }
                        // Debug.WriteLine(lines[i + 1]);
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
                    watch.Stop();
                  
                    
                   // PopulateCBoxManga();
                    UpdateItems();
                   

                    await Clases.XmlIO.WriteJsonAsync(Mangas);
                    loading.IsActive = false;
                }
                LoadGrid();
                Debug.WriteLine("Tiempo apertura: " + watch.ElapsedMilliseconds);
            }          

           
            if ((localSettings.Values["FullScrenn"].ToString() == "1"))
            {
                fullScreen.IsOn = true;
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

                localSettings.Values["MangaActual"] = MangaImages.SelectedIndex-1;
                GuardarDireccion();
                GuardarAjusteImagen();
                Mangas.ElementAt(MangaImages.SelectedIndex-1).SetActual(selectedEpi);
                Mangas.ElementAt(0).SetMangaActual(MangaImages.SelectedIndex-1);
                Frame.Navigate(typeof(FlipView), Mangas);

            }

        }

        private void FlyoutContinuar(object sender, TappedRoutedEventArgs e)
        {
            ContinuarLectura();
        }

        private async void FlyoutCambiarImagen(object sender, TappedRoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            String Nombre;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            int selecteditem = MangaImages.SelectedIndex-1;
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                try
                {
                    if (Mangas.Count>0 && MangaImages.SelectedIndex-1 != -1)
                    {
                       
                        StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                        StorageFolder projectFolder = await rootFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

                        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + @"\Images");
                        Nombre = file.Name;
                        Debug.WriteLine("nombre archivo"+Nombre);
                        if(File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images" + @"\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"))
                        {
                            StorageFile OldFile = await StorageFile.GetFileFromPathAsync((ApplicationData.Current.LocalFolder.Path + @"\Images" + @"\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"));

                            await OldFile.DeleteAsync();
                          
                        }

                        await file.CopyAsync(folder, Mangas.ElementAt(selecteditem).GetName() + ".jpg", NameCollisionOption.ReplaceExisting);
                    

                    }
                   
                }
                catch (FileNotFoundException)
                {


                }             
                MangaImages.SelectedIndex = selecteditem;             
                LoadGrid();
            }
          





        }
        private async Task<int> InputTextDialogAsync(string title)
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
            BitmapImage image1 = new BitmapImage();
           
            try
            {
                items.Add(new MenuItem() { IName = new Uri("ms-appx:///Assets/Agregar.png"),Titulo="Agregar Nuevo" });
               
                for (int i = 0; i < Mangas.Count(); i++)
                    {
                        if(File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(i).GetName() + ".jpg"))
                        {
                            items.Add(new MenuItem() { IName = new Uri(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(i).GetName() + ".jpg"), Titulo = Mangas.ElementAt(i).GetName()});
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

        private async void SaveData1()
        {
            List<String> Directorios = new List<string>();
            var helper = new LocalObjectStorageHelper();
            foreach (Manga value in Mangas)
            {
                Directorios.Add(value.GetDirectory());
            }

            await helper.SaveFileAsync("Directorios", Directorios);

        }
        private async void ReadData()
        {

            List<String> DirectoriosR = new List<string>();
            var helper = new LocalObjectStorageHelper();
            if (await helper.FileExistsAsync("Directorios"))
            {
                foreach (String value in helper.Read<List<String>>("Directorios"))
                {
                    // Debug.WriteLine("Directorios almacenados: " + value);
                }

            }
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

                MessageDialog showDialog = new MessageDialog("Ha ocurrido un error en la lectura de: " + directorio.Split('\\').Last() + "\n¿Desea eliminarlo de la lista de mangas?");
                showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();
                if ((int)result.Id == 0)
                {
                    SaveData();
                    Clases.XmlIO.DeleteJson(directorio.Split('\\').Last());

                }                
               
                return null;
            }
        }

        private async void AgregarManga()
        {

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

            Boolean flag = false;
           
          
            if (folder != null)
            {
                foreach (Manga value in Mangas)
                {
                    if (value.GetName().Equals(folder.Name))
                        flag = true;
                }
                //loadingLoadManga.IsActive = false;
                if (!flag)
                {
                    loadingLoadManga.IsActive = true;
                    await Task.Yield();
                    await PutTaskDelay();

                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);
                    Manga1 = (Clases.Functions.LoadAll(folder, folder.Path, folder.Name, "0", "0"));
                    
                    if (Manga1 != null)
                    {
                        loadingLoadManga.IsActive = false;
                        Mangas.Add(Manga1);
                        await Clases.Functions.CreateMessageAsync("Se ha agregado existosamente: " + folder.Name);
                        GuardarImagen();
                    }
                    else
                    {
                        loadingLoadManga.IsActive = false;
                        await Clases.Functions.CreateMessageAsync("Ha ocurrido un error al agregar: " + folder.Name);
                    }

                    SaveData();
                    await Clases.XmlIO.WriteJsonAsync(Mangas);
                    Mangas.ElementAt(0).SetMangaActual(Mangas.Count()-1);
                    localSettings.Values["MangaActual"] = Mangas.Count-1;
                  
                    await PutTaskDelay();                   
                         
                    UpdateItems();
                    LoadGrid();
                }
                else
                {
                    await Clases.Functions.CreateMessageAsync("Ya existe un manga con ese nombre");
                }
            }
            loadingLoadManga.IsActive = false;
        }
        private async void GuardarImagen()
        {
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            BitmapImage image1 = new BitmapImage();
            String Url;
            episode = await Clases.Functions.LoadEpisodeAsync(Mangas.ElementAt(Mangas.Count() - 1).GetEpisodes().ElementAt(0).GetDirectory());
            try
            {
                if (episode != null && episode.GetPages().Count > 0)
                {
                    Pages = episode.GetPages();
                    Url = (Mangas.ElementAt(Mangas.Count-1).GetDirectory() + @"\" + episode.GetDirectory() + @"\" + episode.GetPages().ElementAt(0));
                    StorageFile file = await StorageFile.GetFileFromPathAsync((Url));
                    StorageFolder rootFolder = ApplicationData.Current.LocalFolder;
                    StorageFolder projectFolder = await rootFolder.CreateFolderAsync("Images", CreationCollisionOption.OpenIfExists);

                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path + @"\Images");

                    await file.CopyAsync(folder, Mangas.ElementAt(Mangas.Count - 1).GetName() + ".jpg", NameCollisionOption.ReplaceExisting);
                   /* file = await StorageFile.GetFileFromPathAsync((ApplicationData.Current.LocalFolder.Path + @"\Images" + @"\" + episode.GetPages().ElementAt(0)));
                    await file.RenameAsync(Mangas.ElementAt(Mangas.Count - 1).GetName()+".jpg");*/
                }
            }
            catch (FileNotFoundException)
            {

               
            }
         
        }

     
        private async void ContinuarLectura()
        {
            try
            {
                if (MangaImages.SelectedIndex-1 != -1 && Mangas.ElementAt(MangaImages.SelectedIndex-1).GetUltimoEpisodioLeido() < Mangas.ElementAt(MangaImages.SelectedIndex-1).GetEpisodes().Count)
                {

                    ContentDialog dialog = new ContentDialog();                 
                  
                    dialog.RequestedTheme = ElementTheme.Dark;                   
                    dialog.Title = "¿Desea continuar con el capítulo " + (Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetUltimoEpisodioLeido() + 1) + " de " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName() + "?";
                    dialog.IsSecondaryButtonEnabled = true;
                    dialog.PrimaryButtonText = "Sí";
                    dialog.SecondaryButtonText = "No";

                    if (await dialog.ShowAsync() == ContentDialogResult.Primary && MangaImages.SelectedIndex - 1 != -1 && Mangas.Count > 0) {

                        GuardarDireccion();
                        GuardarAjusteImagen();
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
            if (Mangas.Count > 0)
            {
                int mangaactual = Mangas.ElementAt(0).GetMangaActual();             
                

                if ((localSettings.Values["readingDirection"].ToString().Length>0))
                {
                    DireccionLectura.SelectedIndex = Int32.Parse(localSettings.Values["readingDirection"].ToString());
                }
                else
                {
                    DireccionLectura.SelectedIndex = 0;
                }
               

                if ((localSettings.Values["AjusteImagen"].ToString() == "1"))
                {
                    SwitchAjustar.IsOn = true;
                }
                else
                {
                    SwitchAjustar.IsOn = false;
                }
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




                ContentDialog dialog = new ContentDialog();

                dialog.RequestedTheme = ElementTheme.Dark;
                dialog.Title = "Está seguro de que desea eliminar " + Mangas.ElementAt(MangaImages.SelectedIndex - 1).GetName() + "?";
                dialog.IsSecondaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Sí";
                dialog.SecondaryButtonText = "No";          

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    Clases.XmlIO.DeleteJson(Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName());
                    if (File.Exists(ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"))
                    {
                        StorageFile file = await StorageFile.GetFileFromPathAsync((ApplicationData.Current.LocalFolder.Path + @"\Images\" + Mangas.ElementAt(MangaImages.SelectedIndex-1).GetName() + ".jpg"));
                        await file.DeleteAsync();
                    }
                   
                    Mangas.RemoveAt(MangaImages.SelectedIndex-1);
                    SaveData();
                    LoadGrid();
                }
            }
            else
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga primero");
            }           
        }

        private void GuardarDireccion()
        {
            if (DireccionLectura.SelectedIndex!=-1)
            {
                localSettings.Values["readingDirection"] = DireccionLectura.SelectedIndex;
            }
            else
            {
                localSettings.Values["readingDirection"] = 0;
            }     
        }

        private void GuardarAjusteImagen()
        {
            if (SwitchAjustar.IsOn == true)
            {
                localSettings.Values["AjusteImagen"] = 1;
            }
            else
            {
                localSettings.Values["AjusteImagen"] = 0;
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            String[] previousdata = await Clases.XmlIO.ReadStatistics();
            TimeSpan tiempo;
            int PagT = Int32.Parse(previousdata[0]);
            int CapT = Int32.Parse(previousdata[1]);
            int ManT = Int32.Parse(previousdata[3]);
                     

            if (previousdata != null)
            {
                tiempo = TimeSpan.Parse(previousdata[2]);
                FLyout.Text = "Páginas leidas: " + PagT.ToString("#,##0") + "\nCapítulos leídos: " + CapT.ToString("#,##0") + "\nMangas terminados: " + ManT.ToString("#,##0") + "\nTiempo total: " + tiempo.ToString(@"d\:hh\:mm");
            }
            else
            {
                FLyout.Text = "No ha leído nada aun :)";
            }
        }
        async Task PutTaskDelay()
        {
            await Task.Delay(100);
        }

        private async void FlyoutRecargar(object sender, TappedRoutedEventArgs e)
        {
            if (MangaImages.SelectedIndex-1 != -1 && Mangas.Count > 0)
            {


                ContentDialog dialog = new ContentDialog();

                dialog.RequestedTheme = ElementTheme.Dark;
                dialog.Title = " Esto agregará nuevos capitulos agregados en la carpeta, desea continuar? ";
                dialog.IsSecondaryButtonEnabled = true;
                dialog.PrimaryButtonText = "Sí";
                dialog.SecondaryButtonText = "No";
         
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                    {                                  
                    String directorio = Mangas.ElementAt(MangaImages.SelectedIndex-1).GetDirectory();

                    loading.IsActive = true;
                    await Task.Yield();
                   await PutTaskDelay();


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
                            loading.IsActive = false;
                            await Clases.Functions.CreateMessageAsync("Se agregaron " + (cantidadNueva - cantidadActual) + " capítulos nuevos");
                        }
                        else
                        {
                            loading.IsActive = false;
                            await Clases.Functions.CreateMessageAsync("No hay nuevos capítulos que agregar");
                        }
                        loading.IsActive = false;

                    }
                    catch (Exception)
                    {
                        await Clases.Functions.CreateMessageAsync("Ha ocurrido un error al recargar el manga");
                    }
                }             

            }
            else
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga");
            }
        }

       
    }
}
