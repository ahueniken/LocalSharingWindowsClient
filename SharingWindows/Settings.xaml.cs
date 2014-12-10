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

namespace SharingWindows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
        }



        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();

        }

        private void authButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkCalls.Login(passwordBox.Password);
            Properties.Settings.Default.Save();
        }

        private void stopSharingButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["shouldBroadcast"] = false;
            Properties.Settings.Default.Save();
        }

        private void restartSharingButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["shouldBroadcast"] = true;
            Properties.Settings.Default.Save();
        }

    }
}
