using System;
using System.Collections.Generic;
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
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;

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
        public static Radio radio = new Radio();
        private DispatcherTimer timer = new DispatcherTimer();
        private int tickcount = 0;
        public static SerialPort ArduinoPort = new SerialPort();
        public static byte[] ArduinoBuffer = new byte[4];
        public static Grid gauges = new Grid();
        public static byte fanSpeed, desiredTemperature=24, blowerPosition = 3;
        public static List<string> ArduinoOutputs = new List<string>();

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        public MainWindow()
        {
            InitializeComponent();
            //this.RemoveLogicalChild(HomeScreen);
            //this.RemoveVisualChild(HomeScreen);
            //PublishFrameworkElement(HomeScreen, );
            MediaFrame.Children.Add(HomeScreen);
            MediaFrame.Children.Add(musicPlayer);
            MediaFrame.Children.Add(camera);
            MediaFrame.Children.Add(radio);
            Canvas.SetZIndex(MainWindow.musicPlayer, 0);
            Canvas.SetZIndex(MainWindow.camera, 0);
            Canvas.SetZIndex(MainWindow.radio, 0);
            Canvas.SetZIndex(MainWindow.HomeScreen, 1);
            musicPlayer.Visibility = Visibility.Hidden;
            radio.Visibility = Visibility.Hidden;
            gauges = grdGauges;
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 1);
            timer.Tick += timer_Tick;
            //MediaFrame.Width = this.Width - (this.Width * 0.2);
            camera.Visibility = System.Windows.Visibility.Hidden;
            ConnectSerialPort();
            lblTempInside.Content = "20";
            timer.Start();
        }

        public void timer_Tick(object sender, EventArgs e)
        {
            //if (tickcount > 10)
            {
                tickcount = 0;
                if (ArduinoPort.IsOpen)
                {
                    //byte[] data = BitConverter.GetBytes(FanSpeed);
                    ArduinoBuffer[0] = Convert.ToByte(fanSpeed);
                    ArduinoBuffer[1] = Convert.ToByte(desiredTemperature);
                    ArduinoBuffer[2] = Convert.ToByte(blowerPosition);
                    ArduinoBuffer[3] = Convert.ToByte(14);//radio.listenToFrequency);
                    //ArduinoPort.Write(sb.ToString());
                    //if (ArduinoPort.BytesToWrite>4)
                    try
                    {
                        ArduinoPort.Write(ArduinoBuffer, 0, 4);
                    }
                    catch (Exception ex)
                    { }
                }
            }
            if (tickcount >50)
            {
                tickcount = 0; 
            }
            tickcount++;
        }

        private void ConnectSerialPort()
        {
            ArduinoPort.PortName = "COM5";               
            ArduinoPort.BaudRate = 9600;
            ArduinoPort.Handshake = System.IO.Ports.Handshake.None;
            ArduinoPort.Parity = Parity.None;
            ArduinoPort.DataBits = 8;
            ArduinoPort.StopBits = StopBits.One;
            ArduinoPort.ReadTimeout = 2000;
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

        private void btnDecreaseTemp_Click(object sender, RoutedEventArgs e)
        {
            desiredTemperature--;
            lblTempInside.Content = desiredTemperature.ToString();
        }

        private void btnIncreaseTemp_Click(object sender, RoutedEventArgs e)
        {
            desiredTemperature++;
            lblTempInside.Content = desiredTemperature.ToString();
        }

        private void btnIncreaseFanSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (fanSpeed < 4)
            {
                fanSpeed += 1;
            }            
        }

        private void btnDecreaseFanSpeed_Click(object sender, RoutedEventArgs e)
        {            
            if (fanSpeed > 0)
            {
                fanSpeed -= 1;
            }
        }

        private void btnBlowerWindscreen_Click(object sender, RoutedEventArgs e)
        {
            blowerPosition = 3;
        }

        private void btnBlowerFace_Click(object sender, RoutedEventArgs e)
        {
            blowerPosition = 0;
        }

        private void btnBlowerFaceDown_Click(object sender, RoutedEventArgs e)
        {
            blowerPosition = 1;
        }

        private void btnBlowerDown_Click(object sender, RoutedEventArgs e)
        {
            blowerPosition = 2;
        }

        private void btnIncreaseVolume_Click(object sender, RoutedEventArgs e)
        {
            SendMessageW(new WindowInteropHelper(this).Handle, WM_APPCOMMAND, new WindowInteropHelper(this).Handle,
                (IntPtr)APPCOMMAND_VOLUME_UP);
        }

        private void btnDecreaseVolume_Click(object sender, RoutedEventArgs e)
        {
            SendMessageW(new WindowInteropHelper(this).Handle, WM_APPCOMMAND, new WindowInteropHelper(this).Handle,
                (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        private void btnDecreaseMute_Click(object sender, RoutedEventArgs e)
        {
            SendMessageW(new WindowInteropHelper(this).Handle, WM_APPCOMMAND, new WindowInteropHelper(this).Handle,
                (IntPtr)APPCOMMAND_VOLUME_MUTE);
        }
    }
}
