using Microsoft.Win32;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;


namespace Kripto.view
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        public MainWindow()
        {
            InitializeComponent();
        }


        private void calculateHash(string filePath) 
        {
            IDigest digest = new Sha256Digest();
            StreamReader reader = new StreamReader(filePath);
            int numberOfSegments = 4; // staviti da bude neka slucajno generisana vrijednost veca od cetiri kao sto je zadato u projektnom zad

            byte[] data = Encoding.UTF8.GetBytes(reader.ReadToEnd());
            byte[][] arrayOfSegments = new byte[numberOfSegments][];

            int sizeOfDataSegment = data.Length / numberOfSegments;
            int sizeOfLastSegment = (data.Length % numberOfSegments) + sizeOfDataSegment;
                

            for(int i  = 0; i < numberOfSegments; i++)
            {
                for(int j = 0; j < sizeOfDataSegment; j++)
                {

                }
            }

            for (int i = 0; i < data.Length; i++)
            {
                Console.WriteLine(data[i]);

            }

            /*            using (FileStream stream = File.OpenRead(filePath))
                        {
                           // IDigest digest = DigestUtilities.GetDigest("SHA-256");

                            byte[] buffer = new byte[8192];
                            int read;

                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                digest.BlockUpdate(buffer, 0, read);
                            }

                            Console.WriteLine("***************");
                            for(int i = 0; i < DigestUtilities.DoFinal(digest).Length; i++)
                            Console.Write(DigestUtilities.DoFinal(digest)[i]);
                        }*/
            reader.Close();
        }



        private void uploadButton_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "Files|*.txt;*.pdf;*.doc";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == true)
            {
                var path = openFileDialog1.FileName;
            }

            string selectedFileName = openFileDialog1.FileName;
            this.calculateHash(selectedFileName);
            Console.WriteLine(selectedFileName);
        }

    }
}


