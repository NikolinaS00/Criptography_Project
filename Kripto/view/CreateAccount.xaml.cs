using Kripto.certificates;
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
using System.Xml.Linq;

namespace Kripto.view
{
    public partial class CreateAccount : Window
    {
        string userName;
        string userPassword;
        string _name;
        string _surname;
        public static string currentDirectory = Environment.CurrentDirectory;
        public static string credentialsFile = System.IO.Path.Combine(currentDirectory, @".." + System.IO.Path.DirectorySeparatorChar + ".." + System.IO.Path.DirectorySeparatorChar + "credentials.txt");
        public static string UserFiles = System.IO.Path.Combine(currentDirectory, @".." + System.IO.Path.DirectorySeparatorChar + ".." + System.IO.Path.DirectorySeparatorChar + "documnents");
        public static string credentialsFilePath = System.IO.Path.GetFullPath(credentialsFile);

        public CreateAccount()
        {

            Console.WriteLine("Main Method");
            Certificate.generateRootCA();


            InitializeComponent();
        }

        private void createDirectory(string surname, string name)
        {
            string dirName = name + " " + surname;
            string dir = System.IO.Path.Combine(currentDirectory, @".." + System.IO.Path.DirectorySeparatorChar + ".." + System.IO.Path.DirectorySeparatorChar + "documents" + System.IO.Path.DirectorySeparatorChar + dirName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private bool isUsernameUnique(string usName)
        {
            StreamReader reader = new StreamReader(credentialsFilePath);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line.Split(',')[0].Split(':')[1].Equals(usName))
                {
                    Console.WriteLine("tacnooooooooooo");
                    return false;
                }
            }
            reader.Close();
            return true;
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("kliknuto");

            if (_name != null && _surname != null && userName != null && userPassword != null)
            {
                if (isUsernameUnique(userName))
                {
                    Console.WriteLine(credentialsFilePath);

                    Certificate.createUserCertificate(_name, _surname, userPassword, userName);
                    createDirectory(_surname, _name);
                    using (StreamWriter writer = new StreamWriter(credentialsFilePath, true))
                    {
                        writer.WriteLine("username:" + userName + ",password:" + userPassword);
                        writer.Close(); 
                    }

                    MessageBox.Show("Uspješno ste kreirali korisnički nalog", "Potvrda", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.Yes);
                }
                else
                {
                    MessageBox.Show("Korisničko ime koje ste unijeli nijie jedinstveno. Unesite drugo korisničko ime!", "Greška", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.Yes);
                }
            

            }
            else
            {
                string caption = "Word Processor";
                MessageBox.Show("Niste unijeli sve potrebne podatke", caption, MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.Yes);
            }


        }

        private void name_TextChanged(object sender, TextChangedEventArgs e)
        {
            _name = ((TextBox)sender).Text;
        }

        private void surname_TextChanged(object sender, TextChangedEventArgs e)
        {
            _surname = ((TextBox)sender).Text;
        }

        private void username_TextChanged(object sender, TextChangedEventArgs e)
        {
            userName = ((TextBox)sender).Text;
        }

        private void password_TextChanged(object sender, TextChangedEventArgs e)
        {
            userPassword = ((TextBox)sender).Text;
        }
    }
}
