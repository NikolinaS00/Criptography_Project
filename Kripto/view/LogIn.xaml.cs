using Org.BouncyCastle.X509;
using System;
using System.IO;
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
using Path = System.IO.Path;
using Kripto.certificates;

namespace Kripto.view
{
    
    public partial class LogIn : Window
    {
        public static X509Certificate loggedUserCertificate;
        public static string loggedUserPassword;
        public static bool loginAvailable = false;
        public string _username;
        public string _password;
        public static string currentDirectory = Environment.CurrentDirectory;
        public static string CredentialsPath = System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "credentials.txt");
        public static int logInCounter = 0;
        public static string certificatePath;
        public LogIn()
        {
            InitializeComponent();

        }



        private void addCertificate_Click(object sender, RoutedEventArgs e)
        {
            ChoseCertificate win2 = new ChoseCertificate();
            win2.Show();
        }

        private void logInBtn_Click(object sender, RoutedEventArgs e)
        {

            if (logInCounter < 3)
            {

                if (areCredentialsValid(userName.Text, password.Text))
                {
                    loggedUserPassword = password.Text;
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                else
                {
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    MessageBox.Show("Uneseni kredencijali nisu ispravni, pokušajte ponovo!", "Greška", button, icon, MessageBoxResult.Yes);
                }

            }
            else
            {
                MessageBoxButton button = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                MessageBox.Show("Tri puta ste pogrešno unijeli kredencijale. Vaš sertifikat je suspendovan!", "Greška", button, icon, MessageBoxResult.Yes);
            }

            logInCounter++;
            if (logInCounter == 3)
            {
                if (LogIn.certificatePath != null)
                {

                    Certificate.revokeCertificate(LogIn.certificatePath);
                    Console.WriteLine("revokovan");
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Error;
                    MessageBox.Show("Tri puta ste pogrešno unijeli kredencijale. Vaš sertifikat će biti suspendovan!", "Greška", button, icon, MessageBoxResult.Yes);

                }

            }
        }

        private bool areCredentialsValid(string usName, string pass)
        {

            StreamReader reader = new StreamReader(CredentialsPath);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Split(',')[0].Split(':')[1].Equals(usName) && line.Split(',')[1].Split(':')[1].Equals(pass))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
