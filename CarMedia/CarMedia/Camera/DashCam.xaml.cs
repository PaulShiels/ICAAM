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
using System.Windows.Shapes;
using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Video.VFW;
using AForge;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for DashCam.xaml
    /// </summary>
    public partial class DashCam : System.Windows.Controls.UserControl
    {
        VideoCaptureDevice cam;
        Bitmap frame;
        FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
        AVIWriter aviWriter;
        bool recording = false;
        bool paused = false;
        DispatcherTimer timer = new DispatcherTimer();
        String folder = Directory.GetCurrentDirectory() + @"\Recordings\";
        String file;
        int tickCount;

        public DashCam()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            imgHomeIcon.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\Home_Icon.png"));
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += timer_Tick;
            aviWriter = new AVIWriter();
            aviWriter.FrameRate = 5;//1000 / 100;
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnSave.IsEnabled = true;
        }

        private void startWebcam(String deviceMoniker)
        {
            if (cam != null && cam.IsRunning)
            {
                stopWebcam();
            }
            cam = new VideoCaptureDevice(deviceMoniker);
            cam.NewFrame += new NewFrameEventHandler(cam_NewFrame);
            cam.Start();
            timer.Start();
        }

        void cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            frame = (Bitmap)eventArgs.Frame.Clone();
        }

        private void stopWebcam()
        {
            cam.SignalToStop();
            timer.Stop();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (frame == null) return;
            Bitmap cannedFrame = frame;
            BitmapImage b = ToWpfBitmap(cannedFrame);
            pictureBox1.Source = b;
            if (recording)
            {
                //Flash the red recording icon
                if (tickCount > 10)
                {
                    MainWindow.HomeScreen.recordingIcon.Fill = new SolidColorBrush(System.Windows.Media.Colors.Red);
                    lblRecording.Visibility = Visibility.Visible;
                    tickCount = 0;
                }
                else if (tickCount > 5)
                {
                    MainWindow.HomeScreen.recordingIcon.Fill = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
                    lblRecording.Visibility = Visibility.Hidden;
                }

                if (!paused)
                {
                    try
                    {
                        aviWriter.AddFrame(cannedFrame);
                    }
                    catch
                    {
                        aviWriter.Close();
                    }
                }
            }
            tickCount++;
        }

        public static BitmapImage ToWpfBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        private void Button_Start(object sender, RoutedEventArgs e)
        {
            try
            {
                startWebcam(videoDevices[1].MonikerString);
                aviWriter.Close();
                string fileName = String.Format("{0}.{1}.{2}.{3}.{4}.{5}.avi", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                FileInfo fi = new FileInfo(folder + fileName);
                if (!fi.Exists)
                {
                    File.Create(folder + fileName).Close();
                }

                aviWriter.Codec = "wmv3";
                aviWriter.Open(folder + fileName, 640, 480);
                recording = true;
                paused = false;
                btnStart.IsEnabled = false;
                btnStart.Opacity = 0.2;
                btnStop.IsEnabled = true;
                btnStop.Opacity = 1;
                btnSave.IsEnabled = false;
                btnSave.Opacity = 0.2;
            }
            catch
            {
                System.Windows.MessageBox.Show("Unable to Record!");
            }
        }

        private void Button_Stop(object sender, RoutedEventArgs e)
        {
            aviWriter.Close();
            recording = false;
            btnStart.IsEnabled = true;
            btnStart.Opacity = 1;
            btnStop.IsEnabled = false;
            btnStop.Opacity = 0.2;
            btnSave.IsEnabled = true;
            btnSave.Opacity = 1;
            lblRecording.Visibility = Visibility.Collapsed;
            MainWindow.HomeScreen.recordingIcon.Fill = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        private void ButtonSaveLocation(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                folder = fbd.SelectedPath + @"\";
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
