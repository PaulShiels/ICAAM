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
        private List<MediaPlayer> songs = new List<MediaPlayer>();
        private MediaPlayer s = new MediaPlayer();
        public static bool mediaPlayerIsPlaying = false, mediaPaused = false, mediaStopped=false;
        private TimeSpan pausedPosition;
        private bool sliderBeingDragged = false;
        private List<Track> lstTracks = new List<Track>();
        private List<Album> lstAlbums = new List<Album>();
        private MediaElement mePlayer = new MediaElement();
        private DispatcherTimer timer = new DispatcherTimer(), sliderChanging = new DispatcherTimer();
        private Track trackPlaying;
        private enum MakeVisible { None, AllSongs, AlbumsGrid, AlbumTracks, Artists, ArtistsTracks, NowPlaying, PlayControls };
        //private MakeVisible viewToMakeVisible = MakeVisible.AllSongs;
        //private enum MusicPlayerReturnToWindow { AllSongs, AlbumsGrid, AlbumTracks, Artists, ArtistsTracks, NowPlaying };
        //private List<MakeVisible> returnToWindow = new List<MakeVisible>();//List is used to keep the history of the last 2 views
        private List<MakeVisible> _returnToWindow = new List<MakeVisible>();
        private bool removeRtwIndexOne = true;
        private List<MakeVisible> returnToWindow {
            get
            {
                if (_returnToWindow.Count > 1 && removeRtwIndexOne && _returnToWindow[0].ToString() == _returnToWindow[1].ToString())
                {
                    {
                        _returnToWindow.RemoveAt(1);
                    }
                }
                
                if (_returnToWindow.Count>3)
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
            lblNowShowing.Content = "TRACKS";
            songName.Width = lvSongs.ActualWidth * 0.7;
            artist.Width = lvSongs.ActualWidth * 0.3;
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
                        
            int id = 0;
            foreach (var file in Directory.GetFiles("C:\\Music\\"))
            {
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

                Track track = new Track(new Artist(tagFile.Tag.JoinedPerformers), new Album(tagFile.Tag.JoinedPerformers, tagFile.Tag.Album, AlbumArt), tagFile.Tag.Title, id);
                                
                id++;
                lstTracks.Add(track);
            }

            lvSongs.ItemsSource = lstTracks;
            buildAndPopulateAlbumView();
        }
                      
        private void timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayerIsPlaying)
            {
                btnPlay.Content = "Pause";
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
                catch
                {

                }

            }
            else if (!mediaPaused)
            {
                //btnStop.IsEnabled = false;
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

        private void lvSongs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnPlay.IsEnabled = true;
            trackPlaying = (Track)lvSongs.SelectedValue;
            btnStop.IsEnabled = true;
            PlaySelectedSong(trackPlaying.TrackId);
            //spPlayControls.Visibility = Visibility.Visible;
            //lblNowShowing.Visibility = Visibility.Hidden;
            //spPlayControls.Visibility = Visibility.Visible;
            returnToWindow.Insert(0, MakeVisible.AllSongs);
            SetViewsVisibility(MakeVisible.NowPlaying);
        }

        private void SetViewsVisibility(MakeVisible view)
        {
            switch (view)
            {
                case MakeVisible.None:
                    {
                        lvSongs.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.AllSongs:
                    {
                        lvSongs.Visibility = Visibility.Visible;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.AlbumsGrid:
                    {
                        lvSongs.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Visible;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.AlbumTracks:
                    {
                        lvSongs.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Visible;
                        nowPlaying.Visibility = Visibility.Hidden;
                        spPlayControls.Visibility = Visibility.Hidden;
                        break;
                    }
                case MakeVisible.Artists:
                    {

                        break;
                    }
                case MakeVisible.ArtistsTracks:
                    {
                        break;
                    }
                case MakeVisible.NowPlaying:
                    {
                        lvSongs.Visibility = Visibility.Hidden;
                        scvAlbums.Visibility = Visibility.Hidden;
                        lbxAlbumsTracks.Visibility = Visibility.Hidden;
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
                trackPlaying = (Track)lvSongs.SelectedValue;
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

        private void buildAndPopulateAlbumView()//string AlbumName, System.Drawing.Image AlbumArt)
        {
            List<Album> albums = ((from a in lstTracks
                         select a.Album).ToList()).GroupBy(i => i.AlbumName).Select(group => group.First()).ToList();
                        
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
            
            for (int i = 0; i < Math.Round(Convert.ToDouble(albums.Count / 4))+1; i++)
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
                if (a.AlbumArt != null)
                {
                    Image i = ConvertDrawingImageToWPFImage(a.AlbumArt);
                    i.Height = 150;
                    i.Width = 150;
                    sp.Children.Add(i);
                }
                else
                {
                    Image i = new Image();
                    i.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\NoAlbumArt.png"));
                    i.Height = 150;
                    i.Width = 150;
                    sp.Children.Add(i);
                }

                //Create a new lable to hold the Album Name
                Label lblAlbumName = new Label();
                lblAlbumName.Margin = new Thickness(4, 0, 0, 4);
                lblAlbumName.FontSize = 14;
                lblAlbumName.FontWeight = FontWeights.Bold;
                lblAlbumName.HorizontalAlignment = HorizontalAlignment.Center;

                if (a.AlbumName != null)
                {
                    //If the album name is longer than 22 characters take the first 19 then add ...
                    //This help it look nicer.
                    if (a.AlbumName.Length > 22)
                    {
                        a.AlbumName = a.AlbumName.Substring(0, 19) + "...";
                    }
                    lblAlbumName.Content = a.AlbumName;
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
                    //If the artist name is longer than 23 characters take the first 20 then add ...
                    //This help it look nicer.
                    if (a.Artist.ArtistName.Length > 23)
                    {
                        a.Artist.ArtistName = a.Artist.ArtistName.Substring(0, 20) + "...";
                    }
                    lblArtistName.Content = a.Artist.ArtistName;
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
                if (xLocation==4)
                {
                    xLocation = 0;
                    yLocation++;                    
                }
            }

            //Add the grid to the UI
            scvAlbums.Content = grdAlbums;
        }

        private void AlbumClicked(object sender, MouseButtonEventArgs e)
        {
            //Image s = (Image)clickedAlbum.Children[0];
            //Label s1 = (Label)clickedAlbum.Children[1];//Album name
            //Label s2 = (Label)clickedAlbum.Children[2];

            //Get the clicked stackpanel
            //Get the album name
            StackPanel clickedAlbum = (StackPanel)sender;

            string albumName = ((Label)clickedAlbum.Children[1]).Content.ToString();
            lstTracks = (from t in lstTracks
                         where t.Album.AlbumName == albumName
                         select t).ToList();

            //Add the album art, album name and artist
            StackPanel spHeader = new StackPanel();
            spHeader.Orientation = Orientation.Horizontal;
            if (lstTracks[0].Album.AlbumArt != null)
            {
                Image i = new Image();
                i = ConvertDrawingImageToWPFImage(lstTracks[0].Album.AlbumArt);
                i.Height = 150;
                i.Width = 150;
                i.Margin = new Thickness(20, 10, 10, 20);
                spHeader.Children.Add(i);
            }
            StackPanel spAlbumTitleAndArtist = new StackPanel();
            Label lblAlbumName = new Label() { Content = albumName, FontSize = 24, Foreground = new SolidColorBrush(Colors.White) };
            Label lblAlbumArtist = new Label() { Content = ((Label)clickedAlbum.Children[2]).Content.ToString(), FontSize = 18, Foreground = new SolidColorBrush(Colors.White) };
            spAlbumTitleAndArtist.VerticalAlignment = VerticalAlignment.Center;
            spAlbumTitleAndArtist.Children.Add(lblAlbumName);
            spAlbumTitleAndArtist.Children.Add(lblAlbumArtist);
            spHeader.Children.Add(spAlbumTitleAndArtist);

            StackPanel spAlbumTrack = new StackPanel();
            if (lbxAlbumsTracks.Items.Count ==0)
            {
                //StackPanel l = (StackPanel)lbxAlbumsTracks.Items.GetItemAt(0);
                lbxAlbumsTracks.Items.Add(spHeader);

                foreach (var track in lstTracks)
                {
                    spAlbumTrack = new StackPanel();
                    spAlbumTitleAndArtist.Name = "spAlbumTracks";
                    spAlbumTrack.Margin = new Thickness(0, 0, 0, 15);
                    //Create the Track name label
                    Label lblTrackName = new Label() { Content = track.TrackName, FontSize = 28, Foreground = new SolidColorBrush(Colors.White) };
                    spAlbumTrack.Children.Add(lblTrackName);
                    //Create the Artist name label
                    Label lblArtist = new Label() { Content = track.Artist.ArtistName, FontSize = 18, Foreground = new SolidColorBrush(Colors.Gray), Margin = new Thickness(0, -10, 0, 0) };
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

        private void lbxAlbumsTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StackPanel selectedStackpanel = (StackPanel)((ListBox)sender).SelectedItem;
            try
            {
                var IdOfTrackToPlay = (from t in lstTracks
                                       where t.TrackName == ((Label)selectedStackpanel.Children[0]).Content.ToString() && t.Artist.ArtistName == ((Label)selectedStackpanel.Children[1]).Content.ToString()
                                       select t.TrackId).FirstOrDefault();
                trackPlaying = lstTracks[IdOfTrackToPlay];
                PlaySelectedSong(IdOfTrackToPlay);
                returnToWindow.Insert(0, MakeVisible.AlbumTracks);
                returnToWindow.Insert(0, MakeVisible.AlbumTracks);
                SetViewsVisibility(MakeVisible.NowPlaying);
            }
            catch { }
        }

        private void lvSongs_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }        
    }

    
}
