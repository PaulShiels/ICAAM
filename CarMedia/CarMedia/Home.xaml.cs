using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        System.Timers.Timer timer = new System.Timers.Timer(1000);

        public Home()
        {
            InitializeComponent();

            //////////////////////////////////////////////////////////////////////////////////////
            //Analog Clock found at: http://www.codeproject.com/Articles/29438/Analog-Clock-in-WPF
            DateTime date = DateTime.Now;
            TimeZone time = TimeZone.CurrentTimeZone;
            TimeSpan difference = time.GetUtcOffset(date);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            ///////////////////////////////////////////////////////////////////////////////////////            
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //http://thispointer.spaces.live.com/blog/cns!74930F9313F0A720!252.entry?_c11_blogpart_blogpart=blogview&_c=blogpart#permalink
            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                secondHand.Angle = DateTime.Now.Second * 6;
                minuteHand.Angle = DateTime.Now.Minute * 6;
                hourHand.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5);
            }));
        }

        private void btnRadio_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.radio.Visibility = Visibility.Visible;
            MainWindow.musicPlayer.Visibility = Visibility.Hidden;
            MainWindow.HomeScreen.Visibility = Visibility.Hidden;
        }

        private void btnMusic_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.radio.Visibility = Visibility.Hidden;
            MainWindow.musicPlayer.Visibility = Visibility.Visible;
            MainWindow.HomeScreen.Visibility = Visibility.Hidden;

            if (!Music.mediaPlayerIsPlaying && !Music.mediaPaused && !Music.mediaStopped)
            {
                //Music musicPlayer = new Music();

                //ChangeWindow(musicPlayer);
                //NavigationService.Navigate(MainWindow.musicPlayer);
                //MainWindow.musicPlayer.Visibility = Visibility.Visible;
                Canvas.SetZIndex(MainWindow.musicPlayer, 1);
                //MainWindow.homePage.Visibility = Visibility.Hidden;
                //MainWindow.musicPage.Visibility = Visibility.Visible; 
            }
            else
            {
                Canvas.SetZIndex(MainWindow.musicPlayer, 1);
                //NavigationService.Navigate(MainWindow.musicPage);
                ////MainWindow.homePage.Visibility = Visibility.Hidden;
                //MainWindow.musicPage.Visibility = Visibility.Visible;

                
                {
                    //this.NavigationService.GoBack();
                }
            }
        }

        private void btnCamera_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.musicPlayer.Visibility = Visibility.Hidden;
            MainWindow.camera.Visibility = System.Windows.Visibility.Visible;
            MainWindow.gauges.Visibility = System.Windows.Visibility.Hidden;
            Canvas.SetZIndex(MainWindow.camera, 1);

            if (MainWindow.camera.Arduino.IsOpen)
            {
                MainWindow.camera.timer.Start();
                MainWindow.camera.Arduino.DiscardInBuffer();
            }

        }

        private void btnPhone_Click(object sender, MouseButtonEventArgs e)
        {
            Phone f = new Phone();
        }
          
    }
}
