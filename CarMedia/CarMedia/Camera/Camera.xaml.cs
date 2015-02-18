using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
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
    /// Interaction logic for Camera.xaml
    /// </summary>
    public partial class Camera : UserControl
    {
        public DispatcherTimer timer = new DispatcherTimer();
        public SerialPort Arduino;
        private string new_Dis, old_Dis;
        WPFCSharpWebCam.WebCam webcam = new WPFCSharpWebCam.WebCam();
        

        public Camera()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            imgHomeIcon.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\Home_Icon.png"));
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick+= new EventHandler(timer_Tick);
            Arduino = MainWindow.ArduinoPort;
            webcam.InitializeWebCam(ref imgVideo);
            webcam.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {

            Arduino.DiscardInBuffer();
            Thread.Sleep(400);
            if (Arduino.IsOpen)// && MainWindow.camera.Visibility==Visibility.Visible)
            {                
                //int bufSize = Arduino.ReadBufferSize;
                //if (bufSize > 0)
                //{
                //    old_Dis = new_Dis;
                //    txtDistance.Content = Arduino.ReadExisting();                    
                //}
            }
        }

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.HomeScreen.Visibility = Visibility.Visible;
            MainWindow.camera.Visibility = Visibility.Hidden;
            MainWindow.gauges.Visibility = System.Windows.Visibility.Visible;
            Canvas.SetZIndex(MainWindow.camera, 0);
        }

    }
}
