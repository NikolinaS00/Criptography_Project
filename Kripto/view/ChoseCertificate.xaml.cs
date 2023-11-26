using Kripto.certificates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Path = System.IO.Path;

namespace Kripto.view
{
    /// <summary>
    /// Interaction logic for ChoseCertificate.xaml
    /// </summary>
    public partial class ChoseCertificate : Window
    {
        
        public List<String> listOfCerts = new List<String>();
        public ChoseCertificate()
        {
          
            InitializeComponent();
            buttonNext.IsEnabled = false;
            this.getAllCertificates();

        }
        private void getAllCertificates() {

            string path = Certificate.userCertFolder;
            string[] folders = Directory.GetDirectories(path);
            foreach (string file in folders)
            {
                Console.WriteLine(file);
                listOfCertificates.Items.Add($"{System.IO.Directory.GetFiles(file, "*.cer")[0]}");
            }
           

        }

        private void ListViewItem_GotFocus(object sender, RoutedEventArgs e)
        {
            const char separator = ':';
            Console.WriteLine(sender.ToString().Split(separator)[2]);

            if (Certificate.isCertificateRevoked(sender.ToString().Split(separator)[2]))
            {
                buttonNext.IsEnabled = false;
                string caption = "Nevalidan sertifikat";
                MessageBoxButton button = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;
               MessageBoxResult res =    MessageBox.Show("Izabrani sertifikat je suspendovan, ukoliko zelite da vratite serttifikat kliknite \"YES\"", caption, button, icon);
                if(res == MessageBoxResult.Yes)
                {
                    if (Certificate.ifCertificateIsValid(sender.ToString().Split(separator)[2]))
                    {
                        LogIn.isCertificateRevoked = true;
                        LogIn win = new LogIn();
                        LogIn.certificatePath = sender.ToString().Split(separator)[2];
                        win.Show();
                    }
                    else
                    {
                        buttonNext.IsEnabled = false;
                       
                        MessageBox.Show("Izabrani sertifikat nije validan", "Nevalidan sertifikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    
                }
               

            }
            else if (!Certificate.ifCertificateIsValid(sender.ToString().Split(separator)[2]))
            {
                buttonNext.IsEnabled = false;
                string caption = "Nevalidan sertifikat";
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Warning;
                MessageBoxResult res = MessageBox.Show("Izabrani sertifikat nije validan", caption, button, icon);
            }
            else
            {
                Console.WriteLine("+++++++++++" + sender.ToString().Split(separator)[2]);
                LogIn.certificatePath = sender.ToString().Split(separator)[2];
                buttonNext.IsEnabled = true;
            }
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            // Certificate.revokeCertificate();
            this.Hide();
            LogIn win = new LogIn();
            win.Show();
        }
    }
}
