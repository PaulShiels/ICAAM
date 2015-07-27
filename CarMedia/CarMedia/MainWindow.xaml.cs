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
        public static DashCam dashCam = new DashCam();
        public static Radio radio = new Radio();
        public static Phone phone = new Phone();
        public static Internet internet = new Internet();
        private DispatcherTimer timer = new DispatcherTimer();
        public static SerialPort ArduinoPort = new SerialPort();
        public static SerialPort ArduinoCam = new SerialPort();
        public static byte[] ArduinoBuffer = new byte[8];
        public static byte[] ArduinoSensorBuffer = new byte[3];
        public static Grid gauges = new Grid();
        public static byte fanSpeed, tempPos=3, desiredTemp, tempPosRear=5, blowerPosition = 3, resetArduino=0;
        public static byte radioFreq1, radioFreq2, autoTuneOn=0;
        public static List<string> ArduinoOutputs = new List<string>();
        public static Visibility temperatureControlsVisibility = Visibility.Visible;
        public static Visibility volumeControlVisibility = Visibility.Visible;
        private int radioSignalLevel;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);
        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch(Exception e) { }
            //this.RemoveLogicalChild(HomeScreen);
            //this.RemoveVisualChild(HomeScreen);
            //PublishFrameworkElement(HomeScreen, );
            MediaFrame.Children.Add(HomeScreen);
            MediaFrame.Children.Add(musicPlayer);
            MediaFrame.Children.Add(camera);
            MediaFrame.Children.Add(dashCam);
            MediaFrame.Children.Add(radio);
            MediaFrame.Children.Add(internet);
            Canvas.SetZIndex(MainWindow.musicPlayer, 0);
            Canvas.SetZIndex(MainWindow.camera, 0);
            Canvas.SetZIndex(MainWindow.radio, 0);
            Canvas.SetZIndex(MainWindow.dashCam, 0);
            Canvas.SetZIndex(MainWindow.phone, 0);
            Canvas.SetZIndex(MainWindow.internet, 0);
            Canvas.SetZIndex(MainWindow.HomeScreen, 1);
            musicPlayer.Visibility = Visibility.Hidden;
            radio.Visibility = Visibility.Hidden;
            dashCam.Visibility = Visibility.Hidden;
            gauges = grdGauges;
            //MainWindow.camera.startCamera();
        }
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            timer.Tick += timer_Tick;
            //MediaFrame.Width = this.Width - (this.Width * 0.2);
            camera.Visibility = System.Windows.Visibility.Hidden;
            ConnectSerialPort();
            ConnectArduinoCamSerialPort();
            //lblTempInside.Content = (tempPosFont + tempPosRear).ToString();
            timer.Start();
        }

        public void timer_Tick(object sender, EventArgs e)
        {
            //Set the visibility of the temperature controls and volume control
            temperatureControls.Visibility = temperatureControlsVisibility;
            volumeControl.Visibility = volumeControlVisibility;

            if (ArduinoPort.IsOpen)
            {
                //if (ArduinoPort.BytesToRead > 0)
                {
                    //string s = ArduinoPort.ReadByte().ToString();
                    //if (s == "9")
                    {
                        //byte[] data = BitConverter.GetBytes(FanSpeed);
                        ArduinoBuffer[0] = Convert.ToByte(fanSpeed);
                        ArduinoBuffer[1] = Convert.ToByte(tempPos);
                        //ArduinoBuffer[2] = Convert.ToByte(tempPosRear);
                        ArduinoBuffer[2] = Convert.ToByte(blowerPosition);
                        //ArduinoBuffer[3] = Convert.ToByte(radioFreq1);
                        //ArduinoBuffer[4] = Convert.ToByte(radioFreq2);
                        //ArduinoBuffer[5] = Convert.ToByte(autoTuneOn);
                        //ArduinoBuffer[6] = Convert.ToByte(resetArduino);//radio.listenToFrequency);

                        ////ArduinoPort.Write(sb.ToString());
                        //if (ArduinoPort.BytesToWrite>4)
                        try
                        {
                            ArduinoPort.DiscardOutBuffer();
                            ArduinoPort.Write(ArduinoBuffer, 0, 8);
                            #region oldReadBytes
                            //if (ArduinoPort.BytesToRead >= 12)
                            //{                                
                            //    //if (ArduinoPort.BytesToRead > 20)
                            //    //{
                            //    //    ArduinoPort.DiscardInBuffer();
                            //    //}
                            //    //else
                            //    {
                            //        byte[] b = new byte[50];
                            //        ArduinoPort.Read(b, 0, 20);
                            //        lblInsideTemp.Content = System.BitConverter.ToSingle(b, 0);
                            //        HomeScreen.lblOutsideTemp.Content = System.BitConverter.ToSingle(b, 4);
                            //        //lblTempInside.Content  = System.BitConverter.ToSingle(b, 0);
                            //        //lblOutsideTemp.Content = System.BitConverter.ToSingle(b, 4);
                            //        float voltage = System.BitConverter.ToSingle(b, 8);                                    
                            //        voltHand.Angle = (voltage - 8) * (60 - -60) / (16 - 8)+ -60; //map the voltage from a value of to within the range of 8 to 16                                    
                            //        radioSignalLevel = System.BitConverter.ToInt16(b, 12);
                            //        //lblTempInside.Content = voltage; //string.Format("{0} {1}", f, f2);

                            //    }
                            //}
                            #endregion
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }
            else
            {
                ConnectSerialPort();
            }

            if (ArduinoCam.IsOpen)
            {
                if (ArduinoCam.BytesToRead>=19)
                {
                    try
                    {
                        byte[] b = new byte[20];
                        ArduinoCam.Read(b, 0, 19);
                        lblInsideTemp.Content = System.BitConverter.ToSingle(b, 0);
                        HomeScreen.lblOutsideTemp.Content = System.BitConverter.ToSingle(b, 4);
                        //lblTempInside.Content  = System.BitConverter.ToSingle(b, 0);
                        //lblOutsideTemp.Content = System.BitConverter.ToSingle(b, 4);
                        float voltage = System.BitConverter.ToSingle(b, 8);
                        voltHand.Angle = (voltage - 8) * (60 - -60) / (16 - 8) + -60; //map the voltage from a value of to within the range of 8 to 16                                    
                        int reverseEngaged = System.BitConverter.ToInt32(b, 12);
                        camera.ShowHideCamScreen(reverseEngaged);
                        radioSignalLevel = System.BitConverter.ToInt16(b, 16);
                        //lblTempInside.Content = voltage; //string.Format("{0} {1}", f, f2);

                        //ArduinoSensorBuffer[0] = Convert.ToByte('f');
                        ArduinoSensorBuffer[0] = Convert.ToByte(radioFreq1);
                        ArduinoSensorBuffer[1] = Convert.ToByte(radioFreq2);
                        ArduinoSensorBuffer[2] = Convert.ToByte(autoTuneOn);
                        ArduinoCam.DiscardOutBuffer();
                        ArduinoCam.Write(ArduinoSensorBuffer, 0, 3);
                    }
                    catch{ }
                }
                ConnectArduinoCamSerialPort();
                ArduinoCam.DiscardInBuffer();
            }
            
            resetArduino = 0;
            
        }

        private void ConnectSerialPort()
        {
            ArduinoPort.PortName = "COM13";               
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
            catch (Exception e)
            {
                //Console.WriteLine("Unable to connect to Serial Port, Try again?");
                //if (Console.ReadLine() == "y")
                  //  ConnectSerialPort();
            }
        }

        private void ConnectArduinoCamSerialPort()
        {
            if (!ArduinoCam.IsOpen)
            {
                ArduinoCam.PortName = "COM7";
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
                    //Console.WriteLine("Connection Successfull!");
                }
                catch (Exception e)
                {
                    MessageBoxResult mbx = System.Windows.MessageBox.Show("Unable to connect to 'SensorDuino' Serial Port, Try again?");
                    if (mbx == MessageBoxResult.Yes)
                        ConnectArduinoCamSerialPort();
                }
            }
        }

        private void resetArdunoConnection()
        {
            //ArduinoPort.Close();
            //Thread.Sleep(1000);
            //ConnectSerialPort();
        }

        private void btnDecreaseTemp_Click(object sender, RoutedEventArgs e)
        {
            if (tempPos > 0)
            {
                tempPos--;
            }
            //int newTempPos = tempPos - 14;
            //if(newTempPos < 7)
            //{
            //    tempPos = 7;
            //}
            //if (tempPosFont >= 175 && tempPosRear > 5)
            //{
            //    tempPosFont = 175;
            //    tempPosRear -= 10;
            //}
            //else
            //{
            //    tempPosRear = 5;
            //    if (tempPosFont > 20)
            //    {
            //        if (tempPosFont - 10 < 20)
            //            tempPosFont = 20;
            //        else
            //            tempPosFont -= 10;
            //    }
            //}
            //lblTempInside.Content = string.Format("{0} {1}", tempPosFont, tempPosRear);
        }

        private void btnIncreaseTemp_Click(object sender, RoutedEventArgs e)
        {
            if (tempPos < 9)
            {
                tempPos++;
            }
            //int newTempPos = tempPos + 14;
            //if (newTempPos > 75)
            //{
            //    tempPos = 75;
            //}
            //if (tempPosFont < 175)
            //    tempPosFont += 10;
            //else
            //{
            //    tempPosFont = 175;
            //    if (tempPosRear < 115)
            //        if (tempPosRear + 10 > 115)
            //            tempPosRear = 115;
            //        else
            //            tempPosRear += 10;
            //}
            //lblTempInside.Content = string.Format("{0} {1}", tempPosFont, tempPosRear);            
        }

        private void btnIncreaseFanSpeed_Click(object sender, RoutedEventArgs e)
        {
            if (fanSpeed < 4)
            {
                fanSpeed += 1;
                showFanBars(fanSpeed);

                if (fanSpeed == 4)
                {
                    autoTuneOn = 1;
                }
                else { autoTuneOn = 0; }
            }            
        }

        private void btnDecreaseFanSpeed_Click(object sender, RoutedEventArgs e)
        {            
            if (fanSpeed > 0)
            {
                fanSpeed -= 1;
                showFanBars(fanSpeed);
            }
        }

        private void showFanBars(int fanSpeed)
        {
            switch (fanSpeed)
            {
                case 0:
                    imgFanBar4.Opacity = 0.2;
                    imgFanBar3.Opacity = 0.2;
                    imgFanBar2.Opacity = 0.2;
                    imgFanBar1.Opacity = 0.2;
                    break;
                case 1:
                    imgFanBar4.Opacity = 0.2;
                    imgFanBar3.Opacity = 0.2;
                    imgFanBar2.Opacity = 0.2;
                    imgFanBar1.Opacity = 1;
                    break;
                case 2:
                    imgFanBar4.Opacity = 0.2;
                    imgFanBar3.Opacity = 0.2;
                    imgFanBar2.Opacity = 1;
                    imgFanBar1.Opacity = 1;
                    break;
                case 3:
                    imgFanBar4.Opacity = 0.2;
                    imgFanBar3.Opacity = 1;
                    imgFanBar2.Opacity = 1;
                    imgFanBar1.Opacity = 1;
                    break;
                case 4:
                    imgFanBar4.Opacity = 1;
                    imgFanBar3.Opacity = 1;
                    imgFanBar2.Opacity = 1;
                    imgFanBar1.Opacity = 1;
                    break;
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
            //Used this tutorial found here to adjust system volume: http://www.dotnetcurry.com/showarticle.aspx?ID=431
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

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            resetArdunoConnection();
            resetArduino = 1;
        }
    }

    public static class ExtensionMethods
    {
        public static int Map(this int value, int fromSource, int toSource, int fromTarget, int toTarget)
        {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget);
        }
    }
}
