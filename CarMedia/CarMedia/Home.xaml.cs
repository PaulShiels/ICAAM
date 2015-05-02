using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home
    {
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        
        //http://stackoverflow.com/questions/12019524/get-active-window-of-net-application
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

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
            lblDate.Content = DateTime.Now.ToShortDateString();
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
            MainWindow.temperatureControlsVisibility = Visibility.Hidden;
            MainWindow.volumeControlVisibility = Visibility.Hidden;
            Canvas.SetZIndex(MainWindow.camera, 1);
            MainWindow.camera.startCamera();
        }

        private void btnPhone_Click(object sender, MouseButtonEventArgs e)
        {
            openApplication(316, 187);
        }

        private void btnGps_Click(object sender, RoutedEventArgs e)
        {
            openApplication(390, 269);
        }

        private void btnInternet_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow.radio.Visibility = Visibility.Hidden;
            //MainWindow.phone.Visibility = Visibility.Hidden;
            //MainWindow.musicPlayer.Visibility = Visibility.Hidden;
            //MainWindow.HomeScreen.Visibility = Visibility.Hidden;
            //MainWindow.internet.Visibility = Visibility.Visible;
            MainWindow.internet.openBrowser();
        }

        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //int x = System.Windows.Forms.Control.MousePosition.X;
            //int y = System.Windows.Forms.Control.MousePosition.Y;

            //// sample code
            //MessageBox.Show(x.ToString() + "  " + y.ToString());
        }

        public static void clickLeftMouseButton(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            SetCursorPos(1300, 0);
        }

        private void openApplication(int coOrdX, int CoOrdY)
        {
            try
            {
                Process[] pname = Process.GetProcessesByName("ProjectMyScreenApp");
                if (pname.Length == 0)
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo(@"ProjectMyScreenApp\ProjectMyScreenApp.exe");
                    System.Diagnostics.Process.Start(procInfo);
                    Thread.Sleep(2000);
                    System.Windows.Forms.SendKeys.SendWait("{ESC}");
                    Thread.Sleep(2000);
                    clickLeftMouseButton(coOrdX, CoOrdY);
                    System.Windows.Forms.SendKeys.SendWait("E");
                }
                else
                {
                    foreach (var process in Process.GetProcessesByName("ProjectMyScreenApp.exe"))
                    {
                        process.Dispose();//.Kill();
                    }
                    openApplication(coOrdX, CoOrdY);
                }
            }
            catch { }
        }
        
          
    }
}