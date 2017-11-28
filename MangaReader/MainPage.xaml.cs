using Microsoft.Toolkit.Uwp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace MangaReader
{
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
                    PopulateCBoxManga();
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
            if (localSettings.Values["readingDirection"] == null)
            {
                localSettings.Values["readingDirection"] = 1;
            }
            if (localSettings.Values["AjusteImagen"] == null)
            {
                localSettings.Values["AjusteImagen"] = 1;
            }
            if (Mangas.Count == 0)
            {
                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 500));

                Mangas = new List<Manga>();
                Manga Manga1 = new Manga();
                String[] lines = await Clases.XmlIO.Readfile();

                if (lines != null)
                {
                    int i = 0;
                    loading.IsActive = true;
                    var watch = System.Diagnostics.Stopwatch.StartNew();
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
                    Debug.WriteLine("Tiempo lectura: " + watch.ElapsedMilliseconds);
                    PopulateCBoxManga();
                    UpdateItems();
                    //LlenarGridview();


                   await Clases.XmlIO.WriteJsonAsync(Mangas);
                    loading.IsActive = false;
                }

            }
            if (Mangas.Count == 0)
            {
                var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                image.Source = new BitmapImage(imageUriForlogo);
            }

            FullScreen_loaded();
        }

        private async void LlenarGridview()
        {
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            String Url;
            if (Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().Count > 0)
            {
                try
                {
                    episode = await Clases.Functions.LoadEpisodeAsync(Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().ElementAt(0).GetDirectory());
                    if (episode != null)
                    {
                        Pages = episode.GetPages();
                        Url = (Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetDirectory() + @"\" + episode.GetDirectory() + @"\" + episode.GetPages().ElementAt(0));
                        BitmapImage image1 = new BitmapImage();
                        StorageFile file = await StorageFile.GetFileFromPathAsync((Url));
                        IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        image1 = new BitmapImage();
                        await image1.SetSourceAsync(fileStream);
                        image.Source = image1;
                        ObservableCollection<BitmapImage> listItems = new ObservableCollection<BitmapImage>();
                        itemListView.Items.Add(image1);
                    }

                }
                catch (FileNotFoundException)
                {

                }
            }


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
                await Clases.Functions.CreateMessageAsync("Ha ocurrido un error en la lectura de: " + directorio.Split('\\').Last() + "\nVerifique el directorio");
                SaveData();
                Clases.XmlIO.DeleteJson(directorio.Split('\\').Last());
                return null;
            }
        }

        private async void BtnOpenFile(object sender, RoutedEventArgs e)
        {

            if (Mangas == null)
            {
                Mangas = new List<Manga>();
            }
            Manga Manga1 = new Manga();
            loadingLoadManga.IsActive = true;
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
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);
                    Manga1 = (Clases.Functions.LoadAll(folder, folder.Path, folder.Name, "0", "0"));
                    if (Manga1 != null)
                    {
                        loadingLoadManga.IsActive = false;
                        Mangas.Add(Manga1);
                        await Clases.Functions.CreateMessageAsync("Se ha agregado existosamente: " + folder.Name);
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
                    PopulateCBoxManga();                   
                    UpdateItems();
                }
                else
                {
                    await Clases.Functions.CreateMessageAsync("Ya existe un manga con ese nombre");
                }
            }
            loadingLoadManga.IsActive = false;
        }

        private void PopulateCBoxManga()
        {
            ComboBoxManga.Items.Clear();
            foreach (Manga value in Mangas)
            {
                ComboBoxManga.Items.Add(value.GetName());
            }
        }

        private void ComboBoxManga_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ComboBoxManga.SelectedIndex != -1 && Mangas.Count > 0)
            {
                ActualizarComboboxManga();

                LoadImage();
            }

        }

        private void ActualizarComboboxManga()
        {
            try
            {
                List<Episode> Episodes = new List<Episode>();
                Episodes = Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes();
                Mangas.ElementAt(0).SetMangaActual(ComboBoxManga.SelectedIndex);
                int i = 1;
                ComboBoxEpisode.Items.Clear();
                foreach (Episode value in Episodes)
                {
                    ComboBoxEpisode.Items.Add(i.ToString());
                    i++;
                }
                contEpisode.Text = Mangas.ElementAt(Mangas.ElementAt(0).GetMangaActual()).GetUltimoEpisodioLeido().ToString() + " de "
              + Mangas.ElementAt(Mangas.ElementAt(0).GetMangaActual()).GetEpisodes().Count().ToString();
                ComboBoxEpisode.SelectedIndex = Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido();
            }
            catch (ArgumentException)
            {
                ComboBoxEpisode.SelectedIndex = 0;
            }

        }
        private async void LoadImage()
        {
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            String Url;

            if (Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().Count > 0)
            {

                try
                {
                    episode = await Clases.Functions.LoadEpisodeAsync(Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().ElementAt(0).GetDirectory());
                    if (episode != null && episode.GetPages().Count > 0)
                    {
                        Pages = episode.GetPages();
                        Url = (Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetDirectory() + @"\" + episode.GetDirectory() + @"\" + episode.GetPages().ElementAt(0));
                        BitmapImage image1 = new BitmapImage();
                        StorageFile file = await StorageFile.GetFileFromPathAsync((Url));
                        IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                        image1 = new BitmapImage();
                        await image1.SetSourceAsync(fileStream);
                        image.Source = image1;
                    }
                    else
                    {
                        var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                        image.Source = new BitmapImage(imageUriForlogo);
                    }

                }
                catch (FileNotFoundException)
                {

                }
            }

        }

        private async void ButtonView_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxManga.SelectedIndex != -1 && ComboBoxEpisode.SelectedIndex != -1 && Mangas.Count > 0)
            {

                if (ComboBoxEpisode.SelectedIndex < Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido())
                {
                    await Clases.Functions.CreateMessageAsync("No se actualizará el ultimo episodio leído");
                }
                localSettings.Values["MangaActual"] = ComboBoxManga.SelectedIndex;
                GuardarDireccion();
                GuardarAjusteImagen();
                Mangas.ElementAt(ComboBoxManga.SelectedIndex).SetActual(ComboBoxEpisode.SelectedIndex);
                Frame.Navigate(typeof(FlipView), Mangas);

            }
            else
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga primero");
            }

        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            ContinuarLectura();
        }
        private async void ContinuarLectura()
        {
            try
            {
                if (ComboBoxManga.SelectedIndex != -1 && ComboBoxEpisode.SelectedIndex != -1 && Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido() < Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().Count)
                {
                    MessageDialog showDialog = new MessageDialog("¿Desea continuar con el capítulo " + (Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido() + 1) + " de " + Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetName() + "?");
                    showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                    showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                    showDialog.DefaultCommandIndex = 0;
                    showDialog.CancelCommandIndex = 1;
                    var result = await showDialog.ShowAsync();
                    if ((int)result.Id == 0 && ComboBoxManga.SelectedIndex != -1 && Mangas.Count > 0)
                    {

                        localSettings.Values["MangaActual"] = ComboBoxManga.SelectedIndex;
                        GuardarDireccion();
                        GuardarAjusteImagen();
                        Mangas.ElementAt(ComboBoxManga.SelectedIndex).SetActual(Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido());
                        if (localSettings.Values[Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetName()] == null)
                        {
                            localSettings.Values[Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetName()] = 0;
                        }
                        Frame.Navigate(typeof(FlipView), Mangas);
                    }
                }
                else
                {
                    await Clases.Functions.CreateMessageAsync("No hay más episodios, episodio actual: " + Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido());
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga primero");
            }
        }

        private void ImageTapped(object sender, RoutedEventArgs e)
        {
            ContinuarLectura();
        }

        private void UpdateItems()
        {
            if (Mangas.Count > 0)
            {
                int mangaactual = Mangas.ElementAt(0).GetMangaActual();


                if (localSettings.Values["MangaActual"] != null)
                {
                    try
                    {
                        Int32.TryParse(localSettings.Values["MangaActual"].ToString(), out mangaactual);
                        ComboBoxManga.SelectedIndex = mangaactual;
                    }
                    catch (Exception)
                    {
                        ComboBoxManga.SelectedIndex = 0;
                        Mangas.ElementAt(0).SetActual(0);
                        mangaactual = Mangas.ElementAt(0).GetMangaActual();
                    }
                }
                else
                {
                    ComboBoxManga.SelectedIndex = mangaactual;
                }

                ComboBoxEpisode.SelectedIndex = 0;


                contEpisode.Text = Mangas.ElementAt(mangaactual).GetUltimoEpisodioLeido().ToString() + " de "
                    + Mangas.ElementAt(mangaactual).GetEpisodes().Count().ToString();

                if ((localSettings.Values["readingDirection"].ToString() == "1"))
                {
                    toggleSwitch.IsOn = true;
                }
                else
                {
                    toggleSwitch.IsOn = false;
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

        private async void BtnEliminar(object sender, RoutedEventArgs e)
        {
            if (ComboBoxManga.SelectedIndex != -1)
            {
                MessageDialog showDialog = new MessageDialog("Está seguro de que desea eliminar " + Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetName() + "?");
                showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();

                if ((int)result.Id == 0)
                {
                    Clases.XmlIO.DeleteJson(Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetName());
                    Mangas.RemoveAt(ComboBoxManga.SelectedIndex);
                    SaveData();
                    PopulateCBoxManga();
                    ActualizarComboboxManga();
                    if (ComboBoxManga.Items.Count > 0)
                    {
                        ComboBoxManga.SelectedIndex = 0;
                        Mangas.ElementAt(0).SetMangaActual(0);
                    }
                    else
                    {
                        ComboBoxEpisode.Items.Clear();
                        var imageUriForlogo = new Uri("ms-appx:///Assets/Imagen.png");
                        image.Source = new BitmapImage(imageUriForlogo);
                        contEpisode.Text = "de";
                    }
                }
            }
            else
            {
                await Clases.Functions.CreateMessageAsync("Debe agregar un manga primero");
            }

        }

        private void GuardarDireccion()
        {
            if (toggleSwitch.IsOn == true)
            {
                localSettings.Values["readingDirection"] = 1;
                // Mangas.ElementAt(0).SetDirección(1);
            }
            else
            {
                localSettings.Values["readingDirection"] = 0;
                Mangas.ElementAt(0).SetDirección(0);
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
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            String[] previousdata = await Clases.XmlIO.ReadStatistics();
            TimeSpan tiempo;
           

            if (previousdata != null)
            {
                tiempo = TimeSpan.Parse(previousdata[2]);
                FLyout.Text = "Páginas leidas: " + previousdata[0] + "\nCapítulos leídos: " + previousdata[1] + "\nMangas terminados: " + previousdata[3] + "\nTiempo total: " + tiempo.ToString(@"d\:hh\:mm");
            }
            else
            {
                FLyout.Text = "No ha leído nada aun :)";
            }
        }

        private async void BtnRecargar(object sender, RoutedEventArgs e)
        {
            if (ComboBoxManga.SelectedIndex != -1 && Mangas.Count > 0)
            {
                MessageDialog showDialog = new MessageDialog(" Esto agregará nuevos capitulos agregados en la carpeta, desea continuar? ");
                showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
                showDialog.Commands.Add(new UICommand("No") { Id = 1 });
                showDialog.DefaultCommandIndex = 0;
                showDialog.CancelCommandIndex = 1;
                var result = await showDialog.ShowAsync();
                if ((int)result.Id == 0 )
                {                                  
                    String directorio = Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetDirectory();
                    try
                    {
                        loading.IsActive = true;
                        string[] folders1 = System.IO.Directory.GetDirectories(directorio, "*", System.IO.SearchOption.AllDirectories);
                        int cantidadActual = Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().Count;
                        int cantidadNueva = folders1.Count();
                        if (cantidadNueva > cantidadActual)
                        {
                            for (int i = cantidadActual; i < cantidadNueva; i++)
                            {
                                Episode episode = new Episode();
                                episode.SetDirectory(folders1[i]);
                                Mangas.ElementAt(ComboBoxManga.SelectedIndex).SetEpisode(episode);
                            }
                            await Clases.XmlIO.WriteMangaJsonAsync(Mangas.ElementAt(ComboBoxManga.SelectedIndex), CreationCollisionOption.ReplaceExisting);
                            ActualizarComboboxManga();
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
