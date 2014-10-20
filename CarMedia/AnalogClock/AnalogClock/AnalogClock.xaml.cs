using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AnalogClock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //Create an instance of RotateTransform objects
        private RotateTransform MinHandTr = new RotateTransform();
        private RotateTransform HourHandTr = new RotateTransform();
        private RotateTransform SecHandTr = new RotateTransform();

        //Create an instance of DispatcherTimer
        private DispatcherTimer dT = new DispatcherTimer();
        public MainWindow()
            : base()
           {
            Loaded += Window1_Loaded;
            this.InitializeComponent();
            // Insert code required on object creation
            // below this point.
        }
        public void dispatcher_Tick(object source, EventArgs e)
        {
            MinHandTr.Angle = (DateTime.Now.Minute *6);
           HourHandTr.Angle = (DateTime.Now.Hour * 30) + (DateTime.Now.Minute * 0.5);
           //textBox1.Text = DateTime.Now.ToShortDateString();
            //Minutehand.RenderTransform = MinHandTr;
            //Hourhand.RenderTransform = HourHandTr;

         }
        private void Window1_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            dT.Tick += dispatcher_Tick;
            //Set the interval of the Tick event to 1 sec
            dT.Interval = new TimeSpan(0, 0, 1);
            //Start the DispatcherTimer
            dT.Start();
            //secondHandTransform.Angle = (DateTime.Now.Second * 6);
			
        }
    }
}
