using NativeWifi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
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

namespace SharingWindows
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainPage()
        {
            InitializeComponent();
            Uri uri = new Uri("http://local-experiences.herokuapp.com");
            
            this.myWebBrowser.Navigate(uri);

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            
            myWebBrowser.Navigated += new NavigatedEventHandler(wbMain_Navigated);
            this.Loaded += (a, b) =>
            {
                worker.RunWorkerAsync();
                settingsButton.Content = "Settings";
            };
        }

        void wbMain_Navigated(object sender, NavigationEventArgs e)
        {
            SetSilent(this.myWebBrowser, true);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            getCurrentWindow();
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
         
        public void getCurrentWindow()
        {
            int chars = 256;
            StringBuilder buff = new StringBuilder(chars);
            while (true)
            {
                // Obtain the handle of the active window.
                IntPtr handle = GetForegroundWindow();

                Process currentProcess = GetActiveProcess();
                if (currentProcess != null) { 
                    //Console.WriteLine(currentProcess.MainWindowTitle);
                    //Console.WriteLine(currentProcess.ProcessName);
                    NetworkCalls.ShareApp(currentProcess.ProcessName, currentProcess.MainWindowTitle);
                }

                Thread.Sleep(4000);
            }
        }

        private static Process GetActiveProcess()
        {
            IntPtr hwnd = GetForegroundWindow();
            return hwnd != null ? GetProcessByHandle(hwnd) : null;
        }

        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch { return null; }
        }


        public static void SetSilent(WebBrowser browser, bool silent)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            // get an IWebBrowser2 from the document
            IOleServiceProvider sp = browser.Document as IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType().InvokeMember("Silent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null, webBrowser, new object[] { silent });
                }
            }
        }


        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IOleServiceProvider
        {
            [PreserveSig]
            int QueryService([In] ref Guid guidService, [In] ref Guid riid, [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(new Uri("Settings.xaml", UriKind.Relative));
        }
    }

}
