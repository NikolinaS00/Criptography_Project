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

namespace Kripto.view
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void addCertificate_Click(object sender, RoutedEventArgs e)
        {
            ChoseCertificate win2 = new ChoseCertificate();
            win2.Show();
        }

        private void createAccountButton_Click(object sender, RoutedEventArgs e)
        {
            CreateAccount win = new CreateAccount();
            win.Show();
        }
    }
}
