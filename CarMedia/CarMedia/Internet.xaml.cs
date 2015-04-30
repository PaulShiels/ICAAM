using System;
using System.Collections.Generic;
using System.IO;
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
using WpfAppControl;

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for Internet.xaml
    /// </summary>
    public partial class Internet : UserControl
    {
        public Internet()
        {
            InitializeComponent();
            imgHomeIcon.Source = new BitmapImage(new Uri(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).ToString()).ToString() + "\\Images\\Home_Icon.png"));
        }

        private void HomeBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.HomeScreen.Visibility = Visibility.Visible;
            MainWindow.radio.Visibility = Visibility.Hidden;
            MainWindow.musicPlayer.Visibility = Visibility.Hidden;
            MainWindow.internet.Visibility = Visibility.Hidden;
            ////SetViewsVisibility(MakeVisible.None);
            //Canvas.SetZIndex(MainWindow.musicPlayer, 0);
        }
    }
}
