using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CarMedia
{
    /// <summary>
    /// Interaction logic for Phone.xaml
    /// </summary>
    public partial class Phone : Window
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public Phone()
        {
            this.InitializeComponent();
            
        }

        private void Button_Click(object sender, RoutedEventArgs ex)
        {
            
        }

        //[DllImport("user32.dll")]
        //static extern IntPtr GetActiveWindow();
        //public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
        public void moveWindow()
        {
            try
            {                
                //var procInfo = new ProcessStartInfo(@"C:\Users\Paul\Documents\Visual Studio 2013\Projects\openApplicationInsideWpf\openApplicationInsideWpf\bin\Release\ProjectMyScreenAssistant.exe");
                var procInfo = new ProcessStartInfo(@"ProjectMyScreenApp\ProjectMyScreenApp.exe");
                System.Diagnostics.Process.Start(procInfo);
                Thread.Sleep(2000);
                System.Windows.Forms.SendKeys.SendWait("{ESC}");
                Thread.Sleep(3000);
                clickLeftMouseButton(390, 269);
                System.Windows.Forms.SendKeys.SendWait("{LEFT}");
                Thread.Sleep(500);
                System.Windows.Forms.SendKeys.SendWait("E");
                Thread.Sleep(500);
                System.Windows.Forms.SendKeys.SendWait("B");                
            }
            catch (Exception e)
            {

            }
        //    const short SWP_NOMOVE = 0X2;
        //    const short SWP_NOSIZE = 1;
        //    const short SWP_NOZORDER = 0X4;
        //    const int SWP_SHOWWINDOW = 0x0040;

        //    try
        //    {
        //        ProcessStartInfo p = new ProcessStartInfo(@"notepad.exe");//ProjectMyScreenApp\ProjectMyScreenApp.exe");
        //        Process.Start(p);

        //        IntPtr handle = GetActiveWindow();
        //        //WindowInteropHelper winHelp = new WindowInteropHelper(GetActiveWindow());
        //        if (handle != IntPtr.Zero)
        //        {
        //            SetWindowPos(handle, 0, 0, 0, 0, 0, SWP_NOZORDER | SWP_NOSIZE | SWP_SHOWWINDOW);
        //        }
        //    }
        //    catch { }
            
        }

        public static void clickLeftMouseButton(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        
        
        public static void MoveWindow()
        {
            

            
            //WindowInteropHelper winHelp = new WindowInteropHelper(MainWindow.phone);


            //var windowRec = winHelp.Handle;
            //// When I move a window to a different monitor it subtracts 16 from the Width and 38 from the Height, Not sure if this is on my system or others.
            //SetWindowPos(windowHandler, (IntPtr)SpecialWindowHandles.HWND_TOP, Screen.AllScreens[monitor].WorkingArea.Left,
            //     Screen.AllScreens[monitor].WorkingArea.Top, windowRec.Size.Width + 16, windowRec.Size.Height + 38,
            //     SetWindowPosFlags.SWP_SHOWWINDOW);
        }
        
    }
}
