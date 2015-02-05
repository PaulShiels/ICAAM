using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for Music.xaml
    /// </summary>
    public partial class Music
    {
        private const string musicFolder = "C:\\Music\\";
        private List<MediaPlayer> songs = new List<MediaPlayer>();
        private MediaPlayer s = new MediaPlayer();
        public static bool mediaPlayerIsPlaying = false, mediaPaused = false, mediaStopped=false;
        private TimeSpan pausedPosition;
        private bool sliderBeingDragged = false;
        private List<Track> lstTracks = new List<Track>();
        private List<Track> lstSelectedTracks = new List<Track>();
        private List<Track> lstOrderedTracks = new List<Track>();
        private string selectedArtistName; //This is needed to ensure the correct track is select from the list of artists tracks
        private MediaElement mePlayer = new MediaElement();
        private DispatcherTimer timer = new DispatcherTimer(), sliderChanging = new DispatcherTimer();
        private Track trackPlaying;
        private enum MakeVisible { None, AllSongs, AlbumsGrid, AlbumTracks, Artists, ArtistsTracks, NowPlaying, PlayControls };
        //private MakeVisible viewToMakeVisible = MakeVisible.AllSongs;
        //private enum MusicPlayerReturnToWindow { AllSongs, AlbumsGrid, AlbumTracks, Artists, ArtistsTracks, NowPlaying };
        //private List<MakeVisible> returnToWindow = new List<MakeVisible>();//List is used to keep the history of the last 2 views
        private List<MakeVisible> _returnToWindow = new List<MakeVisible>();
        private bool removeRtwIndexOne = false;
        private List<MakeVisible> returnToWindow {
            get
            {
                if (_returnToWindow.Count > 1 && removeRtwIndexOne && _returnToWindow[0].ToString() == _returnToWindow[1].ToString())
                {
                    {
                        _returnToWindow.RemoveAt(1);
                    }
                }
                
                if (_returnToWindow.Count>2)
                {
                    _returnToWindow.RemoveRange(2, _returnToWindow.Count - 2);
                    
                }

                removeRtwIndexOne = true; ;
                return _returnToWindow;
            }
            set
            {
                
            }
        }        
        //<MediaElement Name="mePlayer" Grid.Row="1" LoadedBehavior="Manual" Stretch="None" />

        public Music()
        {
            InitializeComponent();                      
            
            //items.Add(new Song() { songName = "John Doe", col2 = 42, col3 = "john@doe-family.com" });
            //lvSongs.ItemsSource = items;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            imgHomeIcon.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\Home_Icon.png"));
            //songName.Header = "Name";
            //artist.Header = "Artist";            
            //album.Header = "Album";
            //lNowShowing.Content = "TRACKS";
            //songName.Width = lvSongs.ActualWidth * 0.7;
            //artist.Width = lvSongs.ActualWidth * 0.3;
            //album.Width = lvSongs.ActualWidth / 3.6;
            
            txtsongTimeLeft.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(TimeSpan.Zero.TotalSeconds));
            txtsongRunningTime.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(TimeSpan.Zero.TotalSeconds));

            //Set up buttons
            btnPlay.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnPlay.Content = "Play";

            sldrTrack.IsMoveToPointEnabled = true;

            sliderChanging.Interval = TimeSpan.FromMilliseconds(10);
            sliderChanging.Tick += new EventHandler(SliderMoving);
            timer.Interval = TimeSpan.FromMilliseconds(1000); //one second
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
                        
            //int id = 0;
            //Check if the music folder contains any directories
            if (Directory.GetDirectories(musicFolder).Count() > 0)
            {
                //if it does check if this directory has any directories
                foreach (var folder in Directory.GetDirectories(musicFolder))
                {
                    //Check if this directory contains any directories
                    if (Directory.GetDirectories(folder).Count() > 0)
                    {
                        //if it does then add all the tracks in it to the list of tracks
                        foreach (var SecondLevelfolder in Directory.GetDirectories(folder))
                        {
                            foreach (var file in Directory.GetFiles(SecondLevelfolder))
                            {
                                retriveTracksFromThisFolder(file);
                            }
                        }
                    }
                    //Now add all the tracks from the first folder of the music folder to the list
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        retriveTracksFromThisFolder(file);
                    }
                }
            }
            //Add all the tracks in the music folder root to the list of all tracks
            foreach (var file in Directory.GetFiles(musicFolder))
            {
                retriveTracksFromThisFolder(file);
            }
 
            lstOrderedTracks = lstTracks.OrderBy(x => x.TrackName).ToList();
            //lstOrderedTracks = lstOrderedTracks.OrderBy(x => x.TrackName);
            if (lbxAlbumsTracks.Items.Count == 0)
            {
                foreach (var track in lstOrderedTracks)
                {
                    StackPanel spTrack = new StackPanel();
                    spTrack.Orientation = Orientation.Horizontal;                    
                    spTrack.Margin = new Thickness(0, 0, 0, 15);

                    //Create the album art Holder if this album has an album art
                    StackPanel spArt = new StackPanel();
                    spArt.VerticalAlignment = VerticalAlignment.Center;
                    Image i = new Image();
                    i.VerticalAlignment = VerticalAlignment.Center;
                    
                    if (track.Album.AlbumArt != null)
                    {
                        i = ConvertDrawingImageToWPFImage(track.Album.AlbumArt);                        
                        i.Width = 60;
                        i.Height = 60;
                        spArt.Children.Add(i);
                        spTrack.Children.Add(spArt);                       
                    }
                    else
                    {
                        i.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\NoAlbumArt.png"));
                        i.VerticalAlignment = VerticalAlignment.Center;
                        i.Width = 60;
                        i.Height = 60;
                        spArt.Children.Add(i);                        
                        spTrack.Children.Add(spArt); 
                    }

                    //Create the Track name label
                    StackPanel spTrackDetails = new StackPanel();
                    Label lblTrackName = new Label() { Content = track.TrackName, FontSize = 28, Foreground = new SolidColorBrush(Colors.White) };
                    spTrackDetails.Children.Add(lblTrackName);
                    //Create the Artist name label
                    Label lblArtist = new Label() { Content = track.Artist.ArtistName, FontSize = 18, Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(0, -10, 0, 0) };
                    spTrackDetails.Children.Add(lblArtist);
                    spTrack.Children.Add(spTrackDetails);

                    //Add the stackpanel to the listbox of Tracks
                    lbxAllTracks.Items.Add(spTrack);
                    //lbxAlbumsTracks.Visibility = Visibility.Visible;
                    //scvAlbums.Visibility = Visibility.Hidden;
                }
            }

            //Populate these views now while the user is deciding what to do
            //This saves the delay if we were to build the views when the button is clicked
            BuildAndPopulateAlbumView();
            BuildAndPopulateArtistsView();
        }

        private void retriveTracksFromThisFolder(string file)
        {
            int id = lstTracks.Count;
            FileInfo fi = new FileInfo(file);
            s = new MediaPlayer();
            s.Open(new Uri(file, UriKind.Relative));
            //s.ScrubbingEnabled = true;
            songs.Add(s);

            TagLib.File tagFile = TagLib.File.Create(file);

            System.Drawing.Image AlbumArt = null;
            if (tagFile.Tag.Pictures.Length >= 1)
            {
                var bin = (byte[])(tagFile.Tag.Pictures[0].Data.Data);
                AlbumArt = System.Drawing.Image.FromStream(new MemoryStream(bin)).GetThumbnailImage(100, 100, null, IntPtr.Zero);
            }

            Track track = new Track(
                new Artist(String.IsNullOrEmpty(tagFile.Tag.FirstAlbumArtist) ? fi.Name.Substring(0, 20) : tagFile.Tag.FirstAlbumArtist),
                tagFile.Tag.JoinedPerformers.ToString(),
                new Album((String.IsNullOrEmpty(tagFile.Tag.FirstAlbumArtist) ? fi.Name.Substring(0, 20) : tagFile.Tag.FirstAlbumArtist),
                    ((String.IsNullOrEmpty(tagFile.Tag.Album)?fi.Name.Substring(0,20): tagFile.Tag.Album)), AlbumArt),
                    String.IsNullOrEmpty(tagFile.Tag.Title) ? fi.Name.Substring(0, 20) : tagFile.Tag.Title, id);

            //id++;            
            lstTracks.Add(track);
        }
                      
        private void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                btnPlay.Content = "Pause";
                
                //If the now playing window is visible hide the now playing button
                //otherwise show the now playing button
                if (nowPlaying.Visibility == Visibility.Visible)
                {
                    btnNowPlaying.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnNowPlaying.Visibility = Visibility.Visible;
                }

                BtnBack.Visibility = Visibility.Visible;

                if (!sliderBeingDragged)
                {
                    prbrSong.Value = s.Position.TotalSeconds;
                    sldrTrack.Value = prbrSong.Value;
                }                

                try
                {
                    //Found help with progress bar here : http://robcrocombe.com/2012/06/15/how-to-make-an-audio-progress-bar/
                    //Track progress bar
                    double timeLeft = s.NaturalDuration.TimeSpan.TotalSeconds - s.Position.TotalSeconds;
                    txtsongRunningTime.Text = String.Format(@"{0:mm\:ss}", s.Position);
                    txtsongTimeLeft.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(timeLeft));

                    if (timeLeft <= 1)
                    {
                        Thread.Sleep(900);
                        PlayNextTrack();
                    }
                }
                catch { }

            }
            else //if (!mediaPaused)
            {
                //btnStop.IsEnabled = false;
                btnNowPlaying.Visibility = Visibility.Hidden;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (returnToWindow.Count != 0)
            {
                if (returnToWindow.Count == 1)
                {
                    SetViewsVisibility(returnToWindow[0]);
                }
                else
                {
                    SetViewsVisibility(returnToWindow[1]);
                }

                //Clear selected items from all views
                lbxAllTracks.SelectedIndex = -1;
                lbxAlbumsTracks.SelectedIndex = -1;
                lbxArtistsTracks.SelectedIndex = -1;
                lbxArtists.SelectedIndex = -1;
            }
            //spPlayControls.Visibility = Visibility.Hidden;
            //if (nowPlaying.Visibility == Visibility.Visible && lvSongs.Visibility == Visibility.Hidden)
            //{
            //    nowPlaying.Visibility = Visibility.Hidden;
            //    lvSongs.Visibility = Visibility.Visible;
            //}
            //else if (mediaPlayerIsPlaying || mediaPaused)
            //{
            //    nowPlaying.Visibility = Visibility.Visible;
            //    spPlayControls.Visibility = Visibility.Visible;
            //    lvSongs.Visibility = Visibility.Hidden;
            //}


            //else if (nowPlaying.Visibility == Visibility.Visible && scvAlbums.Visibility == Visibility.Hidden)
            //{
            //    nowPlaying.Visibility = Visibility.Hidden;
            //    lvSongs.Visibility = Visibility.Visible;
            //}
        }

        private void PlaySelectedSong(int songId)
        {
            try
            {
                s.Stop();
                //Quickly reset the progress bar to 0 as soon as the song stops: look better.
                prbrSong.Value = 0;
                sldrTrack.Value = 0;

                if (trackPlaying != null)
                {
                    s = songs[songId];
                    s.Play();
                    mediaPlayerIsPlaying = true;
                    prbrSong.Maximum = s.NaturalDuration.TimeSpan.TotalSeconds;
                    sldrTrack.Maximum = prbrSong.Maximum;
                    btnPlay.IsEnabled = true;
                    btnStop.IsEnabled = true;
                    //lvSongs.Visibility = Visibility.Hidden;
                    //nowPlaying.Visibility = Visibility.Visible;
                    //returnToWindow.Insert(0, MakeVisible.NowPlaying);
                    UpdateNowPlayingPage();                    
                }
            }
            catch
            { }
        }

        private void UpdateNowPlayingPage()
        {
            //Format the text to display in the now playing window
            npSongTitle.Text = trackPlaying.TrackName.Length > 37 ? trackPlaying.TrackName.Substring(0, 34) + "..." : trackPlaying.TrackName;
            npArtistName.Text = trackPlaying.Artist.ArtistName.Length > 50 ? trackPlaying.Artist.ArtistName.Substring(0, 47) + "..." : trackPlaying.Artist.ArtistName;
            npAlbumTitle.Text = trackPlaying.Album.AlbumName.Length > 50 ? trackPlaying.Album.AlbumName.Substring(0, 47) + "..." : trackPlaying.Album.AlbumName;
        }

        #region unused
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //mePlayer.Play();
            //mediaPlayerIsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
           // e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //mePlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //mePlayer.Stop();
            //mediaPlayerIsPlaying = false;
        }
        #endregion

        //private void lvSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //btnPlay.IsEnabled = true;
        //    //trackPlaying = (Track)lvSongs.SelectedValue;
        //    //btnStop.IsEnabled = true;
        //    //PlaySelectedSong(trackPlaying.TrackId);
        //    ////spPlayControls.Visibility = Visibility.Visible;
        //    ////lblNowShowing.Visibility = Visibility.Hidden;
        //    ////spPlayControls.Visibility = Visibility.Visible;
        //    //returnToWindow.Insert(0, MakeVisible.AllSongs);
        //    //SetViewsVisibility(MakeVisible.NowPlaying);
        //}

        private void SetViewsVisibility(MakeVisible view)
        {
            switch (view)
            {
                case MakeVisible.None:
                    {
                        //lvSongs.Visibility = Visibility.Hidden;
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.AllSongs:
                    {
                        //lvSongs.Visibility = Visibility.Visible;
                        lbxAllTracks.Visibility = Visibility.Visible;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.AlbumsGrid:
                    {
                        //lvSongs.Visibility = Visibility.Hidden;
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Visible;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.AlbumTracks:
                    {
                        //lvSongs.Visibility = Visibility.Hidden;
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Visible;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.Artists:
                    {
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Visible;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.ArtistsTracks:
                    {
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Visible;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.NowPlaying:
                    {
                        //lvSongs.Visibility = Visibility.Hidden;
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Visible;
                        spPlayControls.Visibility = Visibility.Visible;
                        break;
                    }                
            }
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            //Was Pause pressed
            if (mediaPlayerIsPlaying)
            {
                s.Pause();
                PlayState(false, true, false, s.Position);
                btnPlay.Content = "Resume";
                btnStop.IsEnabled = true;
            }

            //Was Resume pressed
            else if (mediaPaused)
            {
                s.Position = pausedPosition;
                s.Play();
                mediaPaused = false;
                mediaPlayerIsPlaying = true;
                btnPlay.Content = "Pause";
            }

            //Just play the song
            else
            {
                //trackPlaying = (Track)lvSongs.SelectedValue;
                PlaySelectedSong(trackPlaying.TrackId);
                btnStop.IsEnabled = true;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            PlayState(false, false, true, TimeSpan.Zero);
            s.Stop();
            prbrSong.Value = 0;
            sldrTrack.Value = 0;
            btnPlay.Content = "Play";
        }

        private void PlayState(bool playing, bool paused, bool stopped, TimeSpan position)
        {
            mediaPlayerIsPlaying = playing;
            mediaPaused = paused;
            mediaStopped = stopped;

            if (paused)
            {
                this.pausedPosition = position;
            }
            else
            {
                this.s.Position = position;
            }
        }

        private void sldrTrack_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            s.Position = TimeSpan.FromSeconds(sldrTrack.Value);
            sliderChanging.Stop();
            sliderBeingDragged = false;
        }

        private void SliderMoving(object sender, EventArgs e)
        {
            prbrSong.Value = sldrTrack.Value;
        }

        private void sldrTrack_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sliderChanging.Start();            
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            PlayNextTrack();
        }

        private void PlayNextTrack()
        {
            //Check shuffle
            if (trackPlaying.TrackId + 1 < songs.Count())
            {
                Thread.Sleep(100);
                PlaySelectedSong(trackPlaying.TrackId + 1);
                trackPlaying = lstTracks[trackPlaying.TrackId + 1];
                UpdateNowPlayingPage();
            }
        }

        private void btnBack_Click_1(object sender, RoutedEventArgs e)
        {
            if (trackPlaying.TrackId - 1 >= 0)
            {
                PlaySelectedSong(trackPlaying.TrackId - 1);
                trackPlaying = lstTracks[trackPlaying.TrackId - 1];
                UpdateNowPlayingPage();
            }
        }

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.HomeScreen.Visibility = Visibility.Visible;
            MainWindow.musicPlayer.Visibility = Visibility.Hidden;
            //SetViewsVisibility(MakeVisible.None);
            Canvas.SetZIndex(MainWindow.musicPlayer, 0);
        }                

        private void AlbumClicked(object sender, MouseButtonEventArgs e)
        {
            //Image s = (Image)clickedAlbum.Children[0];
            //Label s1 = (Label)clickedAlbum.Children[1];//Album name
            //Label s2 = (Label)clickedAlbum.Children[2];

            //Get the clicked stackpanel
            //Get the album name
            StackPanel clickedAlbum = (StackPanel)sender;

            //npArtistName.Text = trackPlaying.Artist.ArtistName.Length > 50 ? trackPlaying.Artist.ArtistName.Substring(0, 47) + "..." : trackPlaying.Artist.ArtistName;

            string albumShortName = ((Label)clickedAlbum.Children[1]).Content.ToString();
            //albumName = albumName.Length > 50 ? albumName.Substring(0, 47) + "..." : albumName;
            //t.Album.AlbumName = t.Album.AlbumName.Length > 50 ? t.Album.AlbumName.Substring(0, 47) + "..." : t.Album.AlbumName) == albumName
            lstSelectedTracks = (from t in lstTracks
                                 where t.Album.AlbumShortName == albumShortName
                                 select t).ToList();

            //Add the album art, album name and artist
            StackPanel spHeader = new StackPanel();
            spHeader.Orientation = Orientation.Horizontal;
            Image i = new Image();
            if (lstSelectedTracks[0].Album.AlbumArt != null)
            {
                i = ConvertDrawingImageToWPFImage(lstSelectedTracks[0].Album.AlbumArt);                
            }
            else
            {
                i.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\NoAlbumArt.png"));                
            }
            i.Height = 150;
            i.Width = 150;
            i.Margin = new Thickness(20, 10, 10, 20);
            spHeader.Children.Add(i);

            StackPanel spAlbumTitleAndArtist = new StackPanel();
            Label lblAlbumName = new Label() { Content = lstSelectedTracks[0].Album.AlbumName, FontSize = 24, Foreground = new SolidColorBrush(Colors.White) };
            Label lblAlbumArtist = new Label() { Content = lstSelectedTracks[0].Artist.ArtistName, FontSize = 18, Foreground = new SolidColorBrush(Colors.White) };
            spAlbumTitleAndArtist.VerticalAlignment = VerticalAlignment.Center;
            spAlbumTitleAndArtist.Children.Add(lblAlbumName);
            spAlbumTitleAndArtist.Children.Add(lblAlbumArtist);
            spHeader.Children.Add(spAlbumTitleAndArtist);

            StackPanel spAlbumTrack = new StackPanel();
            //if (lbxAlbumsTracks.Items.Count ==0)
            lbxAlbumsTracks.Items.Clear();
            {
                //StackPanel l = (StackPanel)lbxAlbumsTracks.Items.GetItemAt(0);
                lbxAlbumsTracks.Items.Add(spHeader);

                foreach (var track in lstSelectedTracks)
                {
                    spAlbumTrack = new StackPanel();
                    spAlbumTitleAndArtist.Name = "spAlbumTracks";
                    spAlbumTrack.Margin = new Thickness(0, 0, 0, 15);
                    //Create the Track name label
                    Label lblTrackName = new Label() { Content = track.TrackName, FontSize = 28, Foreground = new SolidColorBrush(Colors.White) };
                    spAlbumTrack.Children.Add(lblTrackName);
                    //Create the Artist name label
                    Label lblArtist = new Label() { Content = track.JoinedArtists, FontSize = 18, Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(0, -10, 0, 0) };
                    spAlbumTrack.Children.Add(lblArtist);
                    //Add the stackpanel to the listbox of Tracks
                    lbxAlbumsTracks.Items.Add(spAlbumTrack);
                    //lbxAlbumsTracks.Visibility = Visibility.Visible;
                    //scvAlbums.Visibility = Visibility.Hidden;
                }
            }
            removeRtwIndexOne = false;
            returnToWindow.Insert(0, MakeVisible.AlbumsGrid);
            SetViewsVisibility(MakeVisible.AlbumTracks);
        }

        private Image ConvertDrawingImageToWPFImage(System.Drawing.Image gdiImg)
        {
            //I copied this convert method directly from:  http://rohitagarwal24.blogspot.ie/2011/04/convert-from-systemdrawingimage-to.html
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();

            //convert System.Drawing.Image to WPF image
            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(gdiImg);
            IntPtr hBitmap = bmp.GetHbitmap();
            System.Windows.Media.ImageSource WpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            img.Source = WpfBitmap;
            img.Width = 500;
            img.Height = 600;
            img.Stretch = System.Windows.Media.Stretch.Fill;
            return img;
        }

        private void BuildAndPopulateAlbumView()//string AlbumName, System.Drawing.Image AlbumArt)
        {
            List<Album> albums = ((from a in lstTracks
                                   select a.Album).ToList()).GroupBy(i => i.AlbumName).Select(group => group.First()).OrderBy(x => x.AlbumName).ToList();

            //Count the albums
            //If theres more than 0
            //start first row
            //Create grid row with 4 columns

            Grid grdAlbums = new Grid();

            for (int i = 0; i < 4; i++)
            {
                ColumnDefinition column = new ColumnDefinition();
                grdAlbums.ColumnDefinitions.Add(column);
            }

            for (int i = 0; i < Math.Round(Convert.ToDouble(albums.Count / 4)) + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(220);
                grdAlbums.RowDefinitions.Add(row);
            }

            //populate the grid with the albums
            int xLocation = 0, yLocation = 0;
            foreach (var a in albums)
            {
                StackPanel sp = new StackPanel();
                sp.MouseDown += AlbumClicked;
                Image i = new Image();
                if (a.AlbumArt != null)
                {
                    i = ConvertDrawingImageToWPFImage(a.AlbumArt);
                }
                else
                {
                    i.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\NoAlbumArt.png"));
                }
                i.Height = 150;
                i.Width = 150;
                sp.Children.Add(i);

                //Create a new lable to hold the Album Name
                Label lblAlbumName = new Label();
                lblAlbumName.Margin = new Thickness(4, 0, 0, 4);
                lblAlbumName.FontSize = 14;
                lblAlbumName.FontWeight = FontWeights.Bold;
                lblAlbumName.HorizontalAlignment = HorizontalAlignment.Center;

                if (a.AlbumName != null)
                {
                    lblAlbumName.Content = a.AlbumShortName;
                    sp.Children.Add(lblAlbumName);
                }
                else
                {
                    //Put in a blank label to fill the space
                    lblAlbumName.Content = "    ";
                    sp.Children.Add(lblAlbumName);
                }

                //Create a new label to hold the ArtistName
                Label lblArtistName = new Label();
                lblArtistName.Margin = new Thickness(4, -15, 0, 0);
                lblArtistName.FontSize = 10;
                lblArtistName.HorizontalAlignment = HorizontalAlignment.Center;
                if (a.Artist.ArtistName != null)
                {
                    lblArtistName.Content = a.Artist.ArtistShortName;
                    sp.Children.Add(lblArtistName);
                }
                else
                {
                    //Put in a blank label to fill the space
                    lblArtistName.Content = "    ";
                    sp.Children.Add(lblArtistName);
                }

                //Add the stackpanel to the grid
                //Set the stackpanel to the correct row and column position
                grdAlbums.Children.Add(sp);
                Grid.SetColumn(sp, xLocation);
                Grid.SetRow(sp, yLocation);
                xLocation++;

                //check if row has been filled
                if (xLocation == 4)
                {
                    xLocation = 0;
                    yLocation++;
                }
            }

            //Add the grid to the Albums View
            scvAlbums.Content = grdAlbums;
        }

        private void BuildAndPopulateArtistsView()
        {
            List<Artist> artists = ((from a in lstTracks
                                     select a.Artist).GroupBy(i => i.ArtistName).Select(group => group.First()).OrderBy(x => x.ArtistName)).ToList();

            lbxArtistsTracks.Items.Clear();
            Label l = new Label() { Content = "All Artists", FontSize = 48, Foreground = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.Black), HorizontalContentAlignment = HorizontalAlignment.Center, Width = lbxArtistsTracks.Width };
            lbxArtists.Items.Add(l);

            lbxArtists.FontSize = 28;
            lbxArtistsTracks.FontSize = 28;
            lbxArtistsTracks.Foreground = new SolidColorBrush(Colors.White);

            foreach (var artist in artists)
            {
                StackPanel sp = new StackPanel();
                sp.Margin = new Thickness(0, 0, 0, 15);
                sp.Children.Add(new Label() { Content = artist.ArtistName, Foreground = new SolidColorBrush(Colors.White) });
                lbxArtists.Items.Add(sp);
            }

        }

        private void txtSongs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //lvSongs.Visibility = Visibility.Visible;
            //scvAlbums.Visibility = Visibility.Hidden;
            returnToWindow.Insert(0, MakeVisible.AllSongs);
            SetViewsVisibility(MakeVisible.AllSongs);
            //spPlayControls.Visibility = Visibility.Visible;
        }

        private void txtAlbums_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //spPlayControls.Visibility = Visibility.Hidden;
            //scvAlbums.Visibility = Visibility.Visible;
            //lvSongs.Visibility = Visibility.Hidden;
            //nowPlaying.Visibility = Visibility.Hidden;            
            returnToWindow.Insert(0, MakeVisible.AlbumsGrid);
            SetViewsVisibility(MakeVisible.AlbumsGrid);
        }

        private void txtArtists_MouseDown(object sender, MouseButtonEventArgs e)
        {
            returnToWindow.Insert(0, MakeVisible.Artists);
            SetViewsVisibility(MakeVisible.Artists);
        }

        private void lbxAlbumsTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StackPanel selectedStackpanel = (StackPanel)((ListBox)sender).SelectedItem;
            try
            {
                string tName = ((Label)selectedStackpanel.Children[0]).Content.ToString();
                string aName = ((Label)selectedStackpanel.Children[1]).Content.ToString();
                var IdOfTrackToPlay = (from t in lstTracks
                                       where t.TrackName == tName  && t.Artist.ArtistName == aName
                                       select t.TrackId).FirstOrDefault();
                trackPlaying = lstTracks[IdOfTrackToPlay];
                PlaySelectedSong(IdOfTrackToPlay);          
                returnToWindow.Insert(1, MakeVisible.AlbumTracks);  
                SetViewsVisibility(MakeVisible.NowPlaying);
            }
            catch { }
        }

        private void lbxAllTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StackPanel selectedStackpanel = (StackPanel)((ListBox)sender).SelectedItem;
            try
            {
                string tName = ((Label)((StackPanel)selectedStackpanel.Children[1]).Children[0]).Content.ToString();
                string aName = ((Label)((StackPanel)selectedStackpanel. Children[1]).Children[1]).Content.ToString();
                var IdOfTrackToPlay = (from t in lstTracks
                                       where t.TrackName == tName && t.Artist.ArtistName == aName
                                       select t.TrackId).FirstOrDefault();
                trackPlaying = lstTracks[IdOfTrackToPlay];

                PlaySelectedSong(IdOfTrackToPlay);
                returnToWindow.Insert(0, MakeVisible.AllSongs);
                SetViewsVisibility(MakeVisible.NowPlaying);
            }
            catch { }
        }

        private void btnNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            SetViewsVisibility(MakeVisible.NowPlaying);
        }

        private void lbxArtists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex != -1 && ((ListBox)sender).SelectedIndex != 0)
            {
                selectedArtistName = ((Label)(((StackPanel)((ListBox)sender).SelectedItem).Children[0])).Content.ToString();

                //Get all the tracks from this artist
                List<string> artistsTracks = (from t in lstTracks
                                              where t.Artist.ArtistName == selectedArtistName
                                              select t.TrackName).ToList();

                lbxArtistsTracks.Items.Clear();
                Label l = new Label() { Content = selectedArtistName, FontSize = 48, Foreground = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.Black), HorizontalContentAlignment = HorizontalAlignment.Center, Width = lbxArtistsTracks.Width };
                lbxArtistsTracks.Items.Add(l);

                //Add all these tracks to the Artists Tracks View
                foreach (var track in artistsTracks)
                {
                    StackPanel sp = new StackPanel();
                    sp.Margin = new Thickness(0, 0, 0, 15);
                    sp.Children.Add(new Label() { Content = track, Foreground = new SolidColorBrush(Colors.White) });
                    lbxArtistsTracks.Items.Add(sp);
                }

                returnToWindow.Insert(0, MakeVisible.Artists);
                SetViewsVisibility(MakeVisible.ArtistsTracks);
            }
        }

        private void lbxArtistsTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex != -1 && ((ListBox)sender).SelectedIndex != 0)
            {
                string tName = ((Label)(((StackPanel)((ListBox)sender).SelectedItem).Children[0])).Content.ToString();
                try
                {
                    var IdOfTrackToPlay = (from t in lstTracks
                                           where t.TrackName == tName && t.Artist.ArtistName == selectedArtistName
                                           select t.TrackId).FirstOrDefault();
                    trackPlaying = lstTracks[IdOfTrackToPlay];

                    PlaySelectedSong(IdOfTrackToPlay);
                    returnToWindow.Insert(1, MakeVisible.ArtistsTracks);
                    SetViewsVisibility(MakeVisible.NowPlaying);
                }
                catch { }
            }
        }
    }
    
}
