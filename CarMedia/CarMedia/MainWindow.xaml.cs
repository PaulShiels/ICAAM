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

        public MainWindow()
        {
            InitializeComponent();
            //this.RemoveLogicalChild(HomeScreen);
            //this.RemoveVisualChild(HomeScreen);
            //PublishFrameworkElement(HomeScreen, );
            MediaFrame.Children.Add(HomeScreen);
            MediaFrame.Children.Add(musicPlayer);
            Canvas.SetZIndex(MainWindow.musicPlayer, 0);
            Canvas.SetZIndex(MainWindow.HomeScreen, 1);
            //musicPlayer.Visibility = Visibility.Hidden;
            //MediaFrame.Source = new Uri("Home.xaml", UriKind.Relative);
            
            //Home homeScreen = new Home();
            //MediaFrame.Children.Add(homeScreen);

            //MediaFrame.Source = new Uri("Home.xaml", UriKind.Relative);
        }


        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
