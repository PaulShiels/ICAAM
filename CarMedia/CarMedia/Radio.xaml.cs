using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for Radio.xaml
    /// </summary>
    public partial class Radio : UserControl
    {
        private Stopwatch stopwatch = new Stopwatch();
        public string listenToFrequency;

        public Radio()
        {
            InitializeComponent();
        }

        private void sldrRadioFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lblRadioFrequency.Content = Math.Round(sldrRadioFrequency.Value,2);
            string frequency = lblRadioFrequency.Content.ToString();
            if (frequency.Contains('.'))
            {
                string s = frequency.Substring(frequency.IndexOf('.'),frequency.Length-(frequency.IndexOf('.')));
                if (s.Length<3)
                {
                    lblRadioFrequency.Content = frequency + "0";
                }
            }
            else
            {
                lblRadioFrequency.Content = lblRadioFrequency.Content.ToString() + ".00";
            }
            listenToFrequency = lblRadioFrequency.Content.ToString();
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            sldrRadioFrequency.Value += 0.1;
        }

        private void Image_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            sldrRadioFrequency.Value -= 0.1;
        }

        private void spPresetFeq_MouseDown(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Start();
        }

        private void spPresetFreqMouseUp(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Stop();            
            if (stopwatch.ElapsedMilliseconds > 2000)
            {
                string frequency = sldrRadioFrequency.Value.ToString();
                if (frequency.Contains('.'))
                {
                    string s = frequency.Substring(frequency.IndexOf('.'), frequency.Length - (frequency.IndexOf('.')));
                    if (s.Length < 3)
                    {
                        ((Label)(((StackPanel)((StackPanel)sender).Children[0]).Children[1])).Content = frequency + "0";
                    }
                }
                else
                {
                    ((Label)(((StackPanel)((StackPanel)sender).Children[0]).Children[1])).Content = frequency + ".00";
                }
            }
            else
            {
                double freqFromButton = Convert.ToDouble(((Label)(((StackPanel)((StackPanel)sender).Children[0]).Children[1])).Content.ToString());
                sldrRadioFrequency.Value = freqFromButton;
            }
            stopwatch.Reset();
            
        }
        
    }
}
