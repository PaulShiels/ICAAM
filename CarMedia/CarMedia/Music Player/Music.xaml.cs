using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
        private List<Playlist> lstPlaylists = new List<Playlist>();
        private int playlistButtonHeldCounter = 0;
        private string selectedArtistName; //This is needed to ensure the correct track is select from the list of artists tracks
        private Playlist selectedPlaylist;
        private Window newPlaylistWindow = new Window();
        private string nameOfNewPlaylistBeingAdded;
        private MediaElement mePlayer = new MediaElement();
        private DispatcherTimer timer = new DispatcherTimer(), sliderChanging = new DispatcherTimer();
        private Track trackPlaying;
        private enum MakeVisible { None, AllSongs, AlbumsGrid, AlbumTracks, Artists, ArtistsTracks, Playlists, PlaylistsTracks, NowPlaying, PlayControls };
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
            
            txtsongTimeLeft.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(TimeSpan.Zero.TotalSeconds));
            txtsongRunningTime.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(TimeSpan.Zero.TotalSeconds));

            //Set up buttons
            btnPlay.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnPlay.Content = "Play";

            sldrTrack.IsMoveToPointEnabled = true;

            sliderChanging.Interval = TimeSpan.FromMilliseconds(10);
            sliderChanging.Tick += new EventHandler(SliderMoving);
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

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
                                FileInfo fi = new FileInfo(file);
                                    if (fi.Extension == ".mp3" || fi.Extension == ".wav" || fi.Extension == ".m4a" || fi.Extension == ".mv3" || fi.Extension == ".wma" || fi.Extension == ".aac" || fi.Extension == ".m4p" || fi.Extension == ".m4b" || fi.Extension == ".wm")
                                    retriveTracksFromThisFolder(file);
                            }
                        }
                    }
                    //Now add all the tracks from the first folder of the music folder to the list
                    foreach (var file in Directory.GetFiles(folder))
                    {
                        FileInfo fi = new FileInfo(file);
                        if (fi.Extension == ".mp3" || fi.Extension == ".wav" || fi.Extension == ".m4a" || fi.Extension == ".mv3" || fi.Extension == ".wma" || fi.Extension == ".aac" || fi.Extension == ".m4p" || fi.Extension == ".m4b" || fi.Extension == ".wm")
                            retriveTracksFromThisFolder(file);
                    }
                }
            }
            //Add all the tracks in the music folder root to the list of all tracks
            foreach (var file in Directory.GetFiles(musicFolder))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Extension == ".mp3" || fi.Extension == ".wav" || fi.Extension == ".m4a" || fi.Extension == ".mv3" || fi.Extension == ".wma" || fi.Extension == ".aac" || fi.Extension == ".m4p" || fi.Extension == ".m4b" || fi.Extension == ".wm")
                retriveTracksFromThisFolder(file);
            }
 
            //Populate these views now while the user is deciding what to do
            //This saves the delay if we were to build the views when the button is clicked
            BuildAndPopulateTracksView(lstTracks);
            BuildAndPopulateAlbumView();
            BuildAndPopulateArtistsView();
            BuildAndPopulatePlaylistsView();
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

            if(MainWindow.musicPlayer.Visibility==Visibility.Visible)
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (playlistButtonHeldCounter >= 1)
                {
                    ctxMenu.IsOpen = true;
                    playlistButtonHeldCounter = 0;
                }
                else
                {
                    ctxMenu.IsOpen = false;
                    playlistButtonHeldCounter++;
                }
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
                lbxPlaylists.SelectedIndex = -1;
                lbxPlaylistsTracks.SelectedIndex = -1;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.Playlists:
                    {
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        lbxPlaylists.Visibility = Visibility.Visible;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.PlaylistsTracks:
                    {
                        lbxAllTracks.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        lbxArtists.Visibility = Visibility.Hidden;
                        lbxArtistsTracks.Visibility = Visibility.Hidden;
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Visible;
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
                        lbxPlaylists.Visibility = Visibility.Hidden;
                        lbxPlaylistsTracks.Visibility = Visibility.Hidden;
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

        private void BuildAndPopulateTracksView(List<Track> tracks)
        {
            tracks = tracks.OrderBy(x => x.TrackName).ToList();
            //lstOrderedTracks = lstOrderedTracks.OrderBy(x => x.TrackName);            
            if (lbxAllTracks!=null && lbxAllTracks.Items.Count == 0)
            {
                foreach (var track in tracks)
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



            //lstOrderedTracks = lstTracks.OrderBy(x => x.TrackName).ToList();
            ////lstOrderedTracks = lstOrderedTracks.OrderBy(x => x.TrackName);
            //if (lbxAlbumsTracks.Items.Count == 0)
            //{
            //    foreach (var track in lstOrderedTracks)
            //    {
            //        StackPanel spTrack = new StackPanel();
            //        spTrack.Orientation = Orientation.Horizontal;
            //        spTrack.Margin = new Thickness(0, 0, 0, 15);

            //        //Create the album art Holder if this album has an album art
            //        StackPanel spArt = new StackPanel();
            //        spArt.VerticalAlignment = VerticalAlignment.Center;
            //        Image i = new Image();
            //        i.VerticalAlignment = VerticalAlignment.Center;

            //        if (track.Album.AlbumArt != null)
            //        {
            //            i = ConvertDrawingImageToWPFImage(track.Album.AlbumArt);
            //            i.Width = 60;
            //            i.Height = 60;
            //            spArt.Children.Add(i);
            //            spTrack.Children.Add(spArt);
            //        }
            //        else
            //        {
            //            i.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\NoAlbumArt.png"));
            //            i.VerticalAlignment = VerticalAlignment.Center;
            //            i.Width = 60;
            //            i.Height = 60;
            //            spArt.Children.Add(i);
            //            spTrack.Children.Add(spArt);
            //        }

            //        //Create the Track name label
            //        StackPanel spTrackDetails = new StackPanel();
            //        Label lblTrackName = new Label() { Content = track.TrackName, FontSize = 28, Foreground = new SolidColorBrush(Colors.White) };
            //        spTrackDetails.Children.Add(lblTrackName);
            //        //Create the Artist name label
            //        Label lblArtist = new Label() { Content = track.Artist.ArtistName, FontSize = 18, Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(0, -10, 0, 0) };
            //        spTrackDetails.Children.Add(lblArtist);
            //        spTrack.Children.Add(spTrackDetails);

            //        //Add the stackpanel to the listbox of Tracks
            //        lbxAllTracks.Items.Add(spTrack);
            //        //lbxAlbumsTracks.Visibility = Visibility.Visible;
            //        //scvAlbums.Visibility = Visibility.Hidden;
            //    }
            //}
        }

        private void BuildAndPopulateAlbumView()
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

        private void BuildAndPopulatePlaylistsView()
        {
            //Playlist p1 = new Playlist("Playlist One", lstTracks.GetRange(0, 5));
            //Playlist p2 = new Playlist("Playlist Two", lstTracks.GetRange(10, 5));
            //Playlist p3 = new Playlist("Playlist Three", lstTracks.GetRange(15, 6));
            //lstPlaylists.Add(p1);
            //lstPlaylists.Add(p2);
            //lstPlaylists.Add(p3);

            DeserializePlaylists(ref lstPlaylists);
            lbxPlaylists.Items.Clear();
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal, Background = new SolidColorBrush(Colors.Black), Width=lbxArtistsTracks.Width};
            sp.Children.Add(new Button() { Content = "+", FontSize = 80, VerticalContentAlignment=VerticalAlignment.Center, Padding=new Thickness(0,-50,0,-30), Margin=new Thickness(10), BorderBrush = new SolidColorBrush(Colors.Transparent), Background = new SolidColorBrush(Colors.Black), FontWeight = FontWeights.ExtraBold, Foreground = new SolidColorBrush(Colors.White)});
            sp.Children.Add(new Label() { Content = "Playlists", FontSize = 48, Foreground = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.Black), Margin = new Thickness(200,0,0,0), });            
            ((Button)sp.Children[0]).Click+=BtnAddNewPlaylist_Click;
            lbxPlaylists.Items.Add(sp);

            foreach (var playlist in lstPlaylists)
            {
                lbxPlaylists.Items.Add(new Label() { Content = playlist.PlaylistName, Margin = new Thickness(0, 0, 0, 15), Foreground = new SolidColorBrush(Colors.White) });
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

        private void btnPlaylists_Click(object sender, RoutedEventArgs e)
        {
            returnToWindow.Insert(0, MakeVisible.Playlists);
            SetViewsVisibility(MakeVisible.Playlists);
        }

        private void btnNowPlaying_Click(object sender, RoutedEventArgs e)
        {
            SetViewsVisibility(MakeVisible.NowPlaying);
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

        private void lbxPlaylists_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (ctxMenu.IsOpen == false && ((ListBox)sender).SelectedIndex != -1 && ((ListBox)sender).SelectedIndex != 0 && lbxPlaylists.Items.Contains(selectedPlaylist))
            //{
            //   // string selectedPlaylistName = ((ListBox)sender).SelectedItem.ToString();

            //   // //Get all the tracks from this playlist
            //   //selectedPlaylist = (from p in lstPlaylists
            //   //                 where p.PlaylistName == selectedPlaylistName
            //   //                 select p).FirstOrDefault();

            //    lstSelectedTracks.Clear();
            //    lbxPlaylistsTracks.Items.Clear();
            //    Label l = new Label() { Content = selectedPlaylist.PlaylistName, FontSize = 48, Foreground = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.Black), HorizontalContentAlignment = HorizontalAlignment.Center, Width = lbxArtistsTracks.Width };
            //    lbxPlaylistsTracks.Items.Add(l);

            //    //Add all these tracks to the Artists Tracks View
            //    foreach (var track in selectedPlaylist.PlaylistTracks)
            //    {
            //        StackPanel sp = new StackPanel();
            //        sp.Margin = new Thickness(0, 0, 0, 15);
            //        sp.Children.Add(new Label() { Content = track.TrackName, Foreground = new SolidColorBrush(Colors.White) });
            //        lbxPlaylistsTracks.Items.Add(sp);
            //        lstSelectedTracks.Add(track);
            //    }

            //    returnToWindow.Insert(0, MakeVisible.Playlists);
            //    SetViewsVisibility(MakeVisible.PlaylistsTracks);
            //}
            if (((ListBox)sender).SelectedIndex != -1 && ((ListBox)sender).SelectedIndex != 0)
            {
                returnToWindow.Insert(0, MakeVisible.Playlists);
                SetViewsVisibility(MakeVisible.PlaylistsTracks);
            }
        } 
        
        private void lbxPlaylistsTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex != -1 && ((ListBox)sender).SelectedIndex != 0)
            {
                string tName = ((Label)(((StackPanel)((ListBox)sender).SelectedItem).Children[0])).Content.ToString();
                try
                {
                    var IdOfTrackToPlay = (from t in lstSelectedTracks
                                           where t.TrackName == tName
                                           select t.TrackId).FirstOrDefault();
                    trackPlaying = lstTracks[IdOfTrackToPlay];

                    PlaySelectedSong(IdOfTrackToPlay);
                    returnToWindow.Insert(1, MakeVisible.PlaylistsTracks);
                    SetViewsVisibility(MakeVisible.NowPlaying);
                }
                catch { }
            }
        }

        private void CtxMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this playlist?", "Delete Playlist?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                lbxPlaylists.Items.RemoveAt(lstPlaylists.IndexOf(selectedPlaylist)+1);
                lstPlaylists.Remove(selectedPlaylist);
                SerializePlaylists(lstPlaylists);
            }
        }

        private void lbxPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex != -1 && ((ListBox)sender).SelectedIndex != 0)
            {
                string selectedPlaylistName = ((Label)((ListBox)sender).SelectedItem).Content.ToString();

                //Get all the tracks from this playlist
                selectedPlaylist = (from p in lstPlaylists
                                    where p.PlaylistName == selectedPlaylistName
                                    select p).FirstOrDefault();

                lstSelectedTracks.Clear();
                lbxPlaylistsTracks.Items.Clear();
                Label l = new Label() { Content = selectedPlaylist.PlaylistName, FontSize = 48, Foreground = new SolidColorBrush(Colors.White), Background = new SolidColorBrush(Colors.Black), HorizontalContentAlignment = HorizontalAlignment.Center, Width = lbxArtistsTracks.Width };
                lbxPlaylistsTracks.Items.Add(l);

                //Add all these tracks to the Artists Tracks View
                foreach (var track in selectedPlaylist.PlaylistTracks)
                {
                    StackPanel sp = new StackPanel();
                    sp.Margin = new Thickness(0, 0, 0, 15);
                    sp.Children.Add(new Label() { Content = track.TrackName, Foreground = new SolidColorBrush(Colors.White) });
                    lbxPlaylistsTracks.Items.Add(sp);
                    lstSelectedTracks.Add(track);
                }
            }
        }

        private void BtnAddNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!newPlaylistWindow.IsActive)
            {
                newPlaylistWindow = new Window() { Width = 350, Height = 330, AllowsTransparency = true, Background = new SolidColorBrush(Colors.Transparent), BorderBrush = new SolidColorBrush(Colors.Transparent), WindowStyle = WindowStyle.None };
                Canvas.SetLeft(newPlaylistWindow, 350);
                Canvas.SetTop(newPlaylistWindow, 60);
                StackPanel spNewPlaylist = new StackPanel() { Opacity = 0.9, Background = new SolidColorBrush(Colors.Black) };
                StackPanel spAddTo = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment=HorizontalAlignment.Right };
                spAddTo.Children.Add(new Label() { Content = "Add New Playlist", Margin = new Thickness(0, 0, 0, 20), FontSize = 28, Foreground = new SolidColorBrush(Colors.White), HorizontalAlignment = HorizontalAlignment.Center });
                Button btnClose = new Button() { Content = "x", Foreground = new SolidColorBrush(Colors.Red), FontSize = 38, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(50, -15, 0, 45) };
                btnClose.Click += btnClose_Click;
                spAddTo.Children.Add(btnClose);
                spNewPlaylist.Children.Add(spAddTo);
                StackPanel spGetPlaylistName = new StackPanel();
                spGetPlaylistName.Children.Add(new Label() { Content = "New Playlist Name: ", FontSize = 24, Foreground = new SolidColorBrush(Colors.White), Margin = new Thickness(20, 0, 0, 0) });
                TextBox tbxNewPlaylistName = new TextBox() { Width = 300, MaxLength = 25, FontSize = 24, Margin = new Thickness(25, 0, 0, 20), HorizontalAlignment = HorizontalAlignment.Left, Background = new SolidColorBrush(Colors.DarkGray) };
                spGetPlaylistName.Children.Add(tbxNewPlaylistName);
                spNewPlaylist.Children.Add(spGetPlaylistName);
                tbxNewPlaylistName.TextChanged += tbxNewPlaylistName_TextChanged;
                Button btnAddNewPlaylistToPlaylists = new Button() { Content = "Add Playlist", Foreground = new SolidColorBrush(Colors.White), FontSize = 24, Margin = new Thickness(20), MaxWidth = 200, Background = new SolidColorBrush(Colors.DarkGray) };
                btnAddNewPlaylistToPlaylists.Click += BtnAddNewPlaylistToPlaylists_Click;
                spNewPlaylist.Children.Add(btnAddNewPlaylistToPlaylists);
                newPlaylistWindow.Content = spNewPlaylist;
                lbxPlaylists.Opacity = 0.4;
                dpButtonsColumn.Opacity = 0.4;
                tbxNewPlaylistName.Focus();
            }
            newPlaylistWindow.Show();

            //Found the next 4 lines at: http://stackoverflow.com/questions/15554786/how-to-use-windows-on-screen-keyboard-in-c-sharp-winforms
            Process onScreenKeyboardProc = new Process();
            string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
            string onScreenKeyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");
            onScreenKeyboardProc = System.Diagnostics.Process.Start(onScreenKeyboardPath);
        }

        private void tbxNewPlaylistName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Check the text being entered against the list of avaiable playlists and show an error message if the name already exists
            nameOfNewPlaylistBeingAdded = ((TextBox)sender).Text.ToString();
            foreach (var playlist in lstPlaylists)
            {
                if (playlist.PlaylistName == nameOfNewPlaylistBeingAdded)
                {
                    ((Button)((StackPanel)newPlaylistWindow.Content).Children[2]).Visibility = Visibility.Hidden;//Hide the add button
                    if ((((StackPanel)newPlaylistWindow.Content).Children).Count < 4)//Check if the label already exists
                        ((StackPanel)newPlaylistWindow.Content).Children.Add(new Label() { Content = "Playlist Already Exists", HorizontalAlignment = HorizontalAlignment.Center, FontSize = 18, Foreground = new SolidColorBrush(Colors.Red) });//Add the error label
                    ((Label)((StackPanel)newPlaylistWindow.Content).Children[3]).Visibility = Visibility.Visible;//Show the error label
                    break;
                }
                else
                {
                    ((Button)((StackPanel)newPlaylistWindow.Content).Children[2]).Visibility = Visibility.Visible;//Show the Add button
                    if ((((StackPanel)newPlaylistWindow.Content).Children).Count>3)
                    ((Label)((StackPanel)newPlaylistWindow.Content).Children[3]).Visibility = Visibility.Hidden;//Hide the error label
                }
            }
        }

        private void BtnAddNewPlaylistToPlaylists_Click(object sender, RoutedEventArgs e)
        {
            //Found the next 5 lines at: http://stackoverflow.com/questions/15554786/how-to-use-windows-on-screen-keyboard-in-c-sharp-winforms
            //Kill all on screen keyboards
            Process[] oskProcessArray = Process.GetProcessesByName("TabTip");
            foreach (Process onscreenProcess in oskProcessArray)
            {
                onscreenProcess.Kill();
            }

            newPlaylistWindow.Close();
            lbxPlaylists.Opacity = 0.7;
            spPlayControls.Opacity = 0.7;
            dpButtonsColumn.Opacity = 0.7;
            lstPlaylists.Add(new Playlist(nameOfNewPlaylistBeingAdded, new List<Track>()));
            lbxPlaylists.Items.Add(new Label() { Content = nameOfNewPlaylistBeingAdded, Margin = new Thickness(0, 0, 0, 15), Foreground = new SolidColorBrush(Colors.White) });
            SerializePlaylists(lstPlaylists);
        }

        private void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (!newPlaylistWindow.IsActive)
            {
                newPlaylistWindow = new Window() { Width = 360, Height = 400, AllowsTransparency = true, Background = new SolidColorBrush(Colors.Transparent), BorderBrush = new SolidColorBrush(Colors.Transparent), WindowStyle = WindowStyle.None };
                Canvas.SetLeft(newPlaylistWindow, 350);
                Canvas.SetTop(newPlaylistWindow, 60);
                ListBox lbxPlaylistList = new ListBox() { Width = 320, Height = 400, Opacity = 0.9, Background = new SolidColorBrush(Colors.Black) };
                ScrollViewer.SetVerticalScrollBarVisibility(lbxPlaylistList, ScrollBarVisibility.Hidden);
                ScrollViewer.SetHorizontalScrollBarVisibility(lbxPlaylistList, ScrollBarVisibility.Hidden);
                StackPanel spAddTo = new StackPanel() { Orientation = Orientation.Horizontal };
                spAddTo.Children.Add(new Label() { Content = "Add Track To:", Margin = new Thickness(0, 0, 100, 5), FontSize = 28, Foreground = new SolidColorBrush(Colors.White) });
                Button btnClose = new Button() { Content = "x", Foreground = new SolidColorBrush(Colors.Red), FontSize = 38, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, -15, 0, 45) };
                btnClose.Click+=btnClose_Click;
                spAddTo.Children.Add(btnClose);
                lbxPlaylistList.Items.Add(spAddTo);
                lbxPlaylists.Opacity = 0.3;
                spPlayControls.Opacity = 0.3;
                dpButtonsColumn.Opacity = 0.3;

                foreach (var playlist in lstPlaylists)
                {
                    Label l = new Label() { Content = playlist.PlaylistName, Margin = new Thickness(0, 0, 0, 15), FontSize = 28, Foreground = new SolidColorBrush(Colors.White), Opacity = 0.7 };
                    l.MouseUp += ModalPlaylist_MouseUp;
                    lbxPlaylistList.Items.Add(l);
                }
                newPlaylistWindow.Content = lbxPlaylistList;
            }
            newPlaylistWindow.Show();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            lbxPlaylists.Opacity = 0.7;
            dpButtonsColumn.Opacity = 0.7;
            newPlaylistWindow.Close();

            //Found the next 5 lines at: http://stackoverflow.com/questions/15554786/how-to-use-windows-on-screen-keyboard-in-c-sharp-winforms
            //Kill all on screen keyboards
            Process[] oskProcessArray = Process.GetProcessesByName("TabTip");
            foreach (Process onscreenProcess in oskProcessArray)
            {
                onscreenProcess.Kill();
            }
        }

        private void ModalPlaylist_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string selectedPlaylistName = ((Label)sender).Content.ToString();

            Playlist playlist = (from p in lstPlaylists
                                 where p.PlaylistName == selectedPlaylistName
                                 select p).FirstOrDefault();
            foreach (var plst in lstPlaylists)
            {
                if (plst.PlaylistName == playlist.PlaylistName && !plst.PlaylistTracks.Contains(trackPlaying))
                {
                    plst.PlaylistTracks.Add(trackPlaying);
                    SerializePlaylists(lstPlaylists);
                }
            }
            lbxPlaylists.Opacity = 0.7;
            dpButtonsColumn.Opacity = 0.7;
            spPlayControls.Opacity = 0.7;
            newPlaylistWindow.Close();
        }

        private static void SerializePlaylists(List<Playlist> playlists)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (Stream str = new FileStream("Playlists.dat", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                bf.Serialize(str, playlists);
            }
        }

        private static void DeserializePlaylists(ref List<Playlist> lstPlaylists)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (Stream str = File.OpenRead("Playlists.dat"))
            {
                lstPlaylists = (List<Playlist>)bf.Deserialize(str);
            }
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tbx = (TextBox)sender;
            tbx.Text = string.Empty;
            tbx.GotFocus -= txtSearch_GotFocus;
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string textboxText = ((TextBox)sender).Text;
            if(textboxText.Length>0)
            {
                List<Track> lstMatchingTracks = new List<Track>();
                foreach (var track in lstTracks)
                {
                    if(track.TrackName.ToLower().Contains(textboxText.ToLower()))
                    {
                        lstMatchingTracks.Add(track);
                    }
                }
                if(lbxAllTracks!=null)
                lbxAllTracks.Items.Clear();
                BuildAndPopulateTracksView(lstMatchingTracks);
            }
            else
            {
                BuildAndPopulateTracksView(lstTracks);
            }
        }
    }    
}
