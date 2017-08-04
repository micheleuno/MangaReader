﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        private bool loaded = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null)
            {
                List<Manga> MangasParameter = e.Parameter as List<Manga>;
                if (MangasParameter != null && Mangas != null)
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
            if (Mangas.Count == 0)
            {
                Mangas = new List<Manga>();

                String[] lines = await Clases.XmlIO.Readfile();
                if (lines != null)
                {
                        int i = 0;
                        loading.IsActive = true;
                        while (i + 1 < lines.Length)
                        {
                            foreach (String value in lines) {
                                //  Debug.WriteLine(value);
                            }
                            // Debug.WriteLine(lines[i + 1]);
                            Windows.Storage.StorageFolder folder = await OpenFolder(lines[i + 1]); //segunda linea
                            if (folder != null)
                            {
                                Mangas.Add(Clases.Functions.LoadAll(folder, folder.Path, folder.Name, lines[i], lines[i + 2]));//primera                               
                            }
                            i = i + 4;

                        }
                        PopulateCBoxManga();
                        UpdateItems();
                        loading.IsActive = false;
                        loaded = true;
                    
                }
            }

        }

        private async Task <Windows.Storage.StorageFolder> OpenFolder(String directorio)
        {
            try
            {
                Windows.Storage.StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(directorio);
                return (folder);
            }           
            catch (FileNotFoundException)
            {
                var dialog = new MessageDialog("Ha ocurrido un error en la lectura de: " +directorio.Split('\\').Last()+"\nVerifique el directorio");
                await dialog.ShowAsync();
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                var dialog = new MessageDialog("Ha ocurrido un error en la lectura de: " + directorio.Split('\\').Last() + "\nVerifique el directorio");
                await dialog.ShowAsync();
                return null;
            }
           
        }

        private async void BtnOpenFile(object sender, RoutedEventArgs e)
        {
            if (Mangas == null)
            {
                Mangas = new List<Manga>();
            }
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            picker.SettingsIdentifier = "asd";
            picker.FileTypeFilter.Add("*");
            Windows.Storage.StorageFolder folder = await picker.PickSingleFolderAsync();
            Boolean flag = false;
            
            if (folder != null)
            {
                loading.IsActive = true;
                foreach (Manga value in Mangas)
                {
                    if (value.GetName().Equals(folder.Name))
                        flag = true;
                }
                loading.IsActive = false;
                if (!flag)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(folder.Name, folder);
                    Mangas.Add(Clases.Functions.LoadAll(folder, folder.Path, folder.Name, "0", "0"));
                    loading.IsActive = false;
                    var dialog = new MessageDialog("Se ha agregado existosamente: " + folder.Name);
                    await dialog.ShowAsync();
                    SaveData();
                    PopulateCBoxManga();
                    UpdateItems();
                }
                else
                {
                    var dialog = new MessageDialog("Ya existe un manga con ese nombre");
                    await dialog.ShowAsync();
                }
               
            }

           

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
           
            if (ComboBoxManga.SelectedIndex != -1&&Mangas.Count>0)
            {
                try
                {
                    List<Episode> Episodes = new List<Episode>();
                    Episodes = Mangas.ElementAt<Manga>(ComboBoxManga.SelectedIndex).GetEpisodes();
                    Mangas.ElementAt<Manga>(0).SetMangaActual(ComboBoxManga.SelectedIndex);
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
                catch(ArgumentException)
                {

                }

                loadImage();
            }
           
        }      
        private async void loadImage()
        {
            List<String> Pages = new List<String>();
            Episode episode = new Episode();
            String Url;
            episode = Clases.Functions.LoadEpisode(Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().ElementAt(0).GetDirectory());
            Pages = episode.GetPages();
            Url = (Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetDirectory() + @"\" + episode.GetDirectory() +@"\"+ episode.GetPages().ElementAt(0));
            BitmapImage image1 = new BitmapImage();
            StorageFile file = await StorageFile.GetFileFromPathAsync((Url));
            IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
            image1 = new BitmapImage();
            await image1.SetSourceAsync(fileStream);
            image.Source = image1;

        }

        private async void  ButtonView_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxManga.SelectedIndex != -1 && ComboBoxEpisode.SelectedIndex != -1)
            {
                if (ComboBoxEpisode.SelectedIndex< Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido())
                {
                    var dialog = new MessageDialog("No se actualizará el ultimo episodio leído");
                    await dialog.ShowAsync();
                }
                guardarDireccion();
                Mangas.ElementAt(ComboBoxManga.SelectedIndex).SetActual(ComboBoxEpisode.SelectedIndex);
                Frame.Navigate(typeof(FlipView), Mangas);
            }
            else
            {
                var dialog = new MessageDialog("Debe agregar un manga primero");
                await dialog.ShowAsync();
            }        

        }

        private async void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (ComboBoxManga.SelectedIndex != -1 && ComboBoxEpisode.SelectedIndex != -1 && Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido()< Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().Count)
            {
              // Debug.WriteLine("actual "+ Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetActual()+"capitulos " + Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetEpisodes().Count);
                guardarDireccion();
                Mangas.ElementAt(ComboBoxManga.SelectedIndex).SetActual(Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido());
                Frame.Navigate(typeof(FlipView), Mangas);
            }
            else
            {

                try
                {
                    var dialog = new MessageDialog("No hay más episodios, episodio actual: " + Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetUltimoEpisodioLeido());
                    await dialog.ShowAsync();
                }
                catch (ArgumentOutOfRangeException)
                {
                    var dialog = new MessageDialog("Debe agregar un manga primero");
                    await dialog.ShowAsync();
                }
            }
        }
        private void UpdateItems()
        {
          
                ComboBoxManga.SelectedIndex = Mangas.ElementAt(0).GetMangaActual();
                ComboBoxEpisode.SelectedIndex = 0;
            
           
            contEpisode.Text = Mangas.ElementAt(Mangas.ElementAt(0).GetMangaActual()).GetUltimoEpisodioLeido().ToString()+" de "
                + Mangas.ElementAt(Mangas.ElementAt(0).GetMangaActual()).GetEpisodes().Count().ToString();
            
            if ((Mangas.ElementAt(0).GetDirección())==1)
            {
                toggleSwitch.IsOn = true;
               
            }
            else
            {
                toggleSwitch.IsOn = false;
            }
        }

        private void SaveData()
        {
            if (Mangas.Count > 0)
            {
                var t = Task.Run(() => Clases.XmlIO.Writefile(Mangas));
                t.Wait();
            }
           
        }

        private async void BtnEliminar(object sender, RoutedEventArgs e)
        {
            MessageDialog showDialog = new MessageDialog("Está seguro de que desea eliminar " + Mangas.ElementAt(ComboBoxManga.SelectedIndex).GetName() + "?");
            showDialog.Commands.Add(new UICommand("Si") { Id = 0 });
            showDialog.Commands.Add(new UICommand("No") { Id = 1 });
            showDialog.DefaultCommandIndex = 0;
            showDialog.CancelCommandIndex = 1;
            var result = await showDialog.ShowAsync();

            if ((int)result.Id == 0&& ComboBoxManga.SelectedIndex != -1)
            {
               
                Mangas.RemoveAt(ComboBoxManga.SelectedIndex);
                SaveData();
                PopulateCBoxManga();
                if(ComboBoxManga.Items.Count > 0)
                    ComboBoxManga.SelectedIndex = 0;
            }
          

        }

        private void guardarDireccion()
        {
            if (toggleSwitch.IsOn == true)
            {
                Mangas.ElementAt(0).SetDirección(1);
            }
            else
            {
                Mangas.ElementAt(0).SetDirección(0);
            }
            var t = Task.Run(() => Clases.XmlIO.Writefile(Mangas));
        }

        private void fullScreen_Toggled(object sender, RoutedEventArgs e)
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
            if (previousdata != null)
            {
                FLyout.Text = "Páginas leidas: " + previousdata[0] + "\nCapítulos leídos: " + previousdata[1] +"\nMangas terminados: " + previousdata[3] + "\nTiempo total: " + previousdata[2].Split('.').First();
            }
            else
            {
                FLyout.Text = "No ha leído nada aun :)";
            }
          
        }
    }
}
    