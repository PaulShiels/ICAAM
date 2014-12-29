using System;
using System.Collections.Generic;
using System.IO.Ports;
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

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public static Home homePage = new Home();
        //public static Music musicPage = new Music();        

        public static Home HomeScreen = new Home();
        public static Music musicPlayer = new Music();
        public static Camera camera = new Camera();
        public static SerialPort ArduinoPort = new SerialPort();

        public MainWindow()
        {
            InitializeComponent();
            //this.RemoveLogicalChild(HomeScreen);
            //this.RemoveVisualChild(HomeScreen);
            //PublishFrameworkElement(HomeScreen, );
            MediaFrame.Children.Add(HomeScreen);
            MediaFrame.Children.Add(musicPlayer);
            MediaFrame.Children.Add(camera);
            Canvas.SetZIndex(MainWindow.musicPlayer, 0);
            Canvas.SetZIndex(MainWindow.camera, 0);
            Canvas.SetZIndex(MainWindow.HomeScreen, 1);
            musicPlayer.Visibility = Visibility.Hidden;
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            //MediaFrame.Width = this.Width - (this.Width * 0.2);
            camera.Visibility = System.Windows.Visibility.Hidden;
            ConnectSerialPort();
        }

        private void ConnectSerialPort()
        {
            ArduinoPort.PortName = "COM7";               
            ArduinoPort.BaudRate = 115200;
            ArduinoPort.Handshake = System.IO.Ports.Handshake.None;
            ArduinoPort.Parity = Parity.None;
            ArduinoPort.DataBits = 8;
            ArduinoPort.StopBits = StopBits.One;
            ArduinoPort.ReadTimeout = 200;
            ArduinoPort.WriteTimeout = 50;
            try
            {
                ArduinoPort.Open();
                Console.WriteLine("Connection Successfull!");
            }
            catch
            {
                Console.WriteLine("Unable to connect to Serial Port, Try again?");
                if (Console.ReadLine() == "y")
                    ConnectSerialPort();
            }
        }
    }
}
