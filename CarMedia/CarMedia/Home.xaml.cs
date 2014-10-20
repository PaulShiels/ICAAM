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
        public Home()
        {
            InitializeComponent();
        }

        private void btnRadio_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void btnMusic_Click(object sender, RoutedEventArgs e)
        {
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
          
    }
}
