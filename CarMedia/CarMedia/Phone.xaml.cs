using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for Phone.xaml
    /// </summary>
    public partial class Phone : UserControl
    {
        public Phone()
        {
            this.InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs ex)
        {
            try
            {                
                var procInfo = new ProcessStartInfo(@"C:\Users\Paul\Documents\Visual Studio 2013\Projects\openApplicationInsideWpf\openApplicationInsideWpf\bin\Release\ProjectMyScreenAssistant.exe");
                System.Diagnostics.Process.Start(procInfo);
            }
            catch(Exception e)
            {

            }
        }
    }
}
