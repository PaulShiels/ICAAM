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
        //<MediaElement Name="mePlayer" Grid.Row="1" LoadedBehavior="Manual" Stretch="None" />

        public Music()
        {
            InitializeComponent();                      
            
            //items.Add(new Song() { songName = "John Doe", col2 = 42, col3 = "john@doe-family.com" });
            //lvSelectionDetails.ItemsSource = items;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            imgHomeIcon.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\Home_Icon.png"));
            songName.Header = "Name";
            artist.Header = "Artist";
            album.Header = "Album";
            songName.Width = lvSelectionDetails.ActualWidth / 3;
            artist.Width = lvSelectionDetails.ActualWidth / 3;
            album.Width = lvSelectionDetails.ActualWidth / 3.6;
            
            txtsongTimeLeft.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(TimeSpan.Zero.TotalSeconds));
            txtsongRunningTime.Text = String.Format(@"{0:mm\:ss}", TimeSpan.FromSeconds(TimeSpan.Zero.TotalSeconds));

            //Set up buttons
            btnPlay.IsEnabled = false;
            btnStop.IsEnabled = false;
            btnPlay.Content = "Play";
            BtnBack.Visibility = Visibility.Hidden;

            sldrTrack.IsMoveToPointEnabled = true;

            sliderChanging.Interval = TimeSpan.FromMilliseconds(10);
            sliderChanging.Tick += new EventHandler(SliderMoving);
            timer.Interval = TimeSpan.FromMilliseconds(1000); //one second
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
                        
            int id = 0;
            string previousTracksAlbum = "";
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
                                

                //TagLib.File tagFile = TagLib.File.Create(file);
                //Track track = new Track(file, id);
                //track.TrackId = id;
                id++;
                //track.TrackName = tagFile.Tag.Title;
                //track.artist = "PVD";
                //track.album = "Best of cd1";

                //FileInfo f = new FileInfo(song);                

                //sng.songName = f.Name;
                //sng.album = 0;
                //sng.artist = s.NaturalDuration.ToString();
                lstTracks.Add(track);
                
                //Check if this tracks album has the same name as the previous tracks album
                //if (track.AlbumName != previousTracksAlbum)
                {
                    //Let this be the previous track now
                   // previousTracksAlbum = track.AlbumName;
                    //Add the Album details to the album view
                    //populateAlbumView(track.AlbumName, track.AlbumArt);
                }
            }
            buildAndPopulateAlbumView();
            lvSelectionDetails.ItemsSource = lstTracks;
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

        private void lvSelectionDetails_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            trackPlaying = (Track)lvSelectionDetails.SelectedValue;
            btnStop.IsEnabled = true;
            PlaySelectedSong(trackPlaying.TrackId);
            
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (nowPlaying.Visibility == Visibility.Visible && lvSelectionDetails.Visibility == Visibility.Hidden)
            {
                nowPlaying.Visibility = Visibility.Hidden;
                lvSelectionDetails.Visibility = Visibility.Visible;
            }
            else if(mediaPlayerIsPlaying)
            {
                nowPlaying.Visibility = Visibility.Visible;
                lvSelectionDetails.Visibility = Visibility.Hidden;
            }
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

                    lvSelectionDetails.Visibility = Visibility.Hidden;
                    nowPlaying.Visibility = Visibility.Visible;
                    UpdateNowPlayingPage();
                }
            }
            catch
            { }
        }

        private void UpdateNowPlayingPage()
        {
            //Format the text to display in the now playing window
            //npSongTitle.Text = trackPlaying.TrackName.Length > 37 ? trackPlaying.TrackName.Substring(0, 34) + "..." : trackPlaying.TrackName;
            //npArtistName.Text = trackPlaying.ArtistName.Length > 50 ? trackPlaying.ArtistName.Substring(0, 47) + "..." : trackPlaying.ArtistName;
            //npAlbumTitle.Text = trackPlaying.AlbumName.Length > 50 ? trackPlaying.AlbumName.Substring(0, 47) + "..." : trackPlaying.AlbumName;
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

        private void lvSelectionDetails_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnPlay.IsEnabled = true;
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
                trackPlaying = (Track)lvSelectionDetails.SelectedValue;
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
            //Give each an id
            //If theres more than 4
            //Create another row with 4 colums
            //Give each column an id

            Grid grdAlbums = new Grid();
            
            for (int i = 0; i < 4; i++)
            {
                int colId = 0;
                ColumnDefinition column = new ColumnDefinition();
                //column.Name = colId.ToString(); ;
                colId++;
                grdAlbums.ColumnDefinitions.Add(column);
            }
            
            for (int i = 0; i < Math.Round(Convert.ToDouble(albums.Count / 4))+1; i++)
            {
                int rowId = 0;
                RowDefinition row = new RowDefinition();
                //row.Name = row.ToString();
                row.Height = new GridLength(300);
                rowId++;
                grdAlbums.RowDefinitions.Add(row);
            }
            //if(albums.Count % 4 > 0)
            //{                
            //}

            //populate the grid with the albums
            int xLocation = 0, yLocation = 0;
            foreach (var a in albums)
            {
                StackPanel sp = new StackPanel();
                if (a.AlbumArt != null)
                {
                    Image i = ConvertDrawingImageToWPFImage(a.AlbumArt);
                    i.Height = 150;
                    i.Width = 150;
                    sp.Children.Add(i);
                }
                if (a.AlbumName != null)
                {
                    Label l = new Label();
                    l.Content = a.AlbumName;
                    sp.Children.Add(l);
                }
                //lbxAlbums.Items.Add(sp);
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
            lbxAlbums.Children.Add(grdAlbums);
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
            lvSelectionDetails.Visibility = Visibility.Hidden;
        }

        private void txtAlbums_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lvSelectionDetails.Visibility = Visibility.Visible;
        }
    }

    
}
