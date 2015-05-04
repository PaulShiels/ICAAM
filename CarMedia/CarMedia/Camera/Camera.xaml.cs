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
        public SerialPort ArduinoCam = new SerialPort();
        private string new_Dis, old_Dis;
        WPFCSharpWebCam.WebCam webcam = new WPFCSharpWebCam.WebCam();
        

        public Camera()
        {
            InitializeComponent();
            //ConnectArduinoCamSerialPort();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            imgHomeIcon.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\Home_Icon.png"));
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick+= new EventHandler(timer_Tick);
            //ArduinoCam = MainWindow.ArduinoPort;
            webcam.InitializeWebCam(ref imgVideo);
            webcam.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            //ArduinoCam.DiscardInBuffer();
            //Thread.Sleep(400);
            ArduinoCam.DiscardOutBuffer();   
            if (ArduinoCam.IsOpen)// && MainWindow.camera.Visibility==Visibility.Visible)
            {
                int bufSize = ArduinoCam.ReadBufferSize;
                if (ArduinoCam.BytesToRead >= 8)
                {
                    old_Dis = new_Dis;
                    //string S = ArduinoCam.ReadExisting();
                    byte[] b = new byte[8];
                    ArduinoCam.Read(b, 0, 8);
                    int dis = System.BitConverter.ToInt32(b, 0);
                    int reverseEngaged = System.BitConverter.ToInt32(b, 4);
                    txtDistance.Content = dis;
                    ShowHideCamScreen(reverseEngaged);

                }
            }
            else
                ConnectArduinoCamSerialPort();
        }

        private void ConnectArduinoCamSerialPort()
        {
            if (!ArduinoCam.IsOpen)
            {
                ArduinoCam.PortName = "COM8";
                ArduinoCam.BaudRate = 9600;
                ArduinoCam.Handshake = System.IO.Ports.Handshake.None;
                ArduinoCam.Parity = Parity.None;
                ArduinoCam.DataBits = 8;
                ArduinoCam.StopBits = StopBits.One;
                ArduinoCam.ReadTimeout = 2000;
                ArduinoCam.WriteTimeout = 50;
                try
                {
                    ArduinoCam.Open();
                    Console.WriteLine("Connection Successfull!");
                }
                catch (Exception e)
                {
                    MessageBoxResult mbx = MessageBox.Show("Unable to connect to Serial Port, Try again?");
                    if (mbx == MessageBoxResult.Yes)
                        ConnectArduinoCamSerialPort();
                }
            }
        }

        public void startCamera()
        {
            ConnectArduinoCamSerialPort();

            if (MainWindow.camera.ArduinoCam.IsOpen)
            {
                MainWindow.camera.timer.Start();
                MainWindow.camera.ArduinoCam.DiscardInBuffer();
            }
        }

        private void ShowHideCamScreen(int isInReverse)
        {
            if (isInReverse == 1)
            {
                MainWindow.musicPlayer.Visibility = Visibility.Hidden;
                MainWindow.camera.Visibility = System.Windows.Visibility.Visible;
                MainWindow.gauges.Visibility = System.Windows.Visibility.Hidden;
                MainWindow.temperatureControlsVisibility = Visibility.Hidden;
                MainWindow.volumeControlVisibility = Visibility.Hidden;
                Canvas.SetZIndex(MainWindow.camera, 1);
                MainWindow.camera.startCamera();
            }
            else
            {
                MainWindow.HomeScreen.Visibility = Visibility.Visible;
                MainWindow.radio.Visibility = Visibility.Hidden;
                MainWindow.camera.Visibility = Visibility.Hidden;
                MainWindow.musicPlayer.Visibility = Visibility.Hidden;
                MainWindow.gauges.Visibility = System.Windows.Visibility.Visible;
                MainWindow.temperatureControlsVisibility = Visibility.Visible;
                MainWindow.volumeControlVisibility = Visibility.Visible;
                Canvas.SetZIndex(MainWindow.camera, 0);
            }
        }        

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.HomeScreen.Visibility = Visibility.Visible;
            MainWindow.radio.Visibility = Visibility.Hidden;
            MainWindow.camera.Visibility = Visibility.Hidden;
            MainWindow.dashCam.Visibility = System.Windows.Visibility.Hidden;
            MainWindow.musicPlayer.Visibility = Visibility.Hidden;
            MainWindow.gauges.Visibility = System.Windows.Visibility.Visible;
            MainWindow.temperatureControlsVisibility = Visibility.Visible;
            MainWindow.volumeControlVisibility = Visibility.Visible;
            Canvas.SetZIndex(MainWindow.camera, 0);
        }

    }
}
