using Microsoft.Win32;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Kripto.view
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string currentDirectory = Environment.CurrentDirectory;
        public static string documentName;
        public static X509Certificate loggedUserCertificate;
        public static string userKey = Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar + "userCerts" + Path.DirectorySeparatorChar));
        public static string nameAndSurname = LogIn.loggedUserCertificate.SubjectDN.ToString();
        public static string userKeyPath = Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar + "userCerts" + Path.DirectorySeparatorChar + nameAndSurname.Split('=')[1].Split(' ')[0] + "_" + nameAndSurname.Split('=')[1].Split(' ')[1] + Path.DirectorySeparatorChar + "Key.pfx"));
        public static string[] folders;
        static X509Certificate2 certificate = new X509Certificate2(userKeyPath, LogIn.loggedUserPassword, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        static RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
        static AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(rsa);
        public MainWindow()
        {

            InitializeComponent();
            getAllUserFiles();
        }


        private ArrayList splitFileIntoSegments(string filePath)
        {

            StreamReader reader = new StreamReader(filePath);
            int numberOfSegments = 5; // staviti da bude neka slucajno generisana vrijednost veca od cetiri kao sto je zadato u projektnom zad

            byte[] data = Encoding.UTF8.GetBytes(reader.ReadToEnd());


            int sizeOfDataSegment = data.Length / numberOfSegments;
            int sizeOfLastSegment = (data.Length % numberOfSegments) + sizeOfDataSegment;
            ArrayList segments = new ArrayList();

            for (int i = 0; i < data.Length; i++)
            {
                Console.Write("-" + data[i]);


            }
            Console.WriteLine("===========================");
            for (int i = 0; i < numberOfSegments; i++)
            {
                int endOfCounting = i == (numberOfSegments - 1) ? sizeOfLastSegment : sizeOfDataSegment;
                byte[] segment = new byte[endOfCounting];
                for (int j = 0; j < endOfCounting; j++)
                {
                    int counter = endOfCounting == sizeOfLastSegment ? (data.Length - sizeOfLastSegment) + j : endOfCounting * i + j;
                    if (counter < data.Length || counter != data.Length)
                    {
                        Console.Write("-" + data[counter]);
                        segment[j] = data[counter];


                    }
                    else break;

                }
                segments.Add(segment);
                string nameAndSurname = LogIn.loggedUserCertificate.SubjectDN.ToString();
                //Upisivanje enkodovanog binanro procitanog fajla u novi fajl
                string dir1 = System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "documents" + Path.DirectorySeparatorChar + nameAndSurname.Split('=')[1].Split(' ')[0] + " " + nameAndSurname.Split('=')[1].Split(' ')[1] + Path.DirectorySeparatorChar + documentName);
                if (!Directory.Exists(dir1))
                {
                    Directory.CreateDirectory(dir1);
                }
                string dir2 = System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "documents" + Path.DirectorySeparatorChar + nameAndSurname.Split('=')[1].Split(' ')[0] + " " + nameAndSurname.Split('=')[1].Split(' ')[1] + Path.DirectorySeparatorChar + documentName + Path.DirectorySeparatorChar + documentName + i);
                if (!Directory.Exists(dir2))
                {
                    Directory.CreateDirectory(dir2);
                }
                File.WriteAllText(Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "documents" + Path.DirectorySeparatorChar + nameAndSurname.Split('=')[1].Split(' ')[0] + " " + nameAndSurname.Split('=')[1].Split(' ')[1] + Path.DirectorySeparatorChar + documentName + Path.DirectorySeparatorChar + documentName + i + Path.DirectorySeparatorChar + "Content.txt")), Convert.ToBase64String(Encrypt(segment)));
            }

            Console.WriteLine("=====================");

            foreach (byte[] item in segments)
            {
                for (int i = 0; i < item.Length; i++)
                {
                    Console.Write("-" + item[i]);
                }

            }
            reader.Close();

            return segments;



        }

        static byte[] Encrypt(byte[] data)
        {
            RsaEngine engine = new RsaEngine();
            Pkcs1Encoding enc = new Pkcs1Encoding(engine);
            enc.Init(true, keyPair.Private);
            return enc.ProcessBlock(data, 0, data.Length);
        }

        static byte[] Decrypt(byte[] encryptedData)
        {
            
                
            RsaEngine engine = new RsaEngine();
            Pkcs1Encoding enc = new Pkcs1Encoding(engine);
            enc.Init(false, keyPair.Public);
            return enc.ProcessBlock(encryptedData, 0, encryptedData.Length);
            
        }

        private byte[] calculateHash(byte[] segment)
        {
            IDigest digest = new Sha256Digest();
            Console.WriteLine("================");


            for (int i = 0; i < segment.Length; i += 5)
            {
                int len = Math.Min(5, segment.Length - i);
                digest.BlockUpdate(segment, i, len);

            }
            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash;
        }

        private void digitalSigning(string filePath)
        {


            Console.WriteLine(userKeyPath);

            ISigner signer = SignerUtilities.GetSigner("SHA-256withRSA");
            signer.Init(true, keyPair.Private);

            int i = 0;
            foreach (byte[] item in splitFileIntoSegments(filePath))
            {
                var hash = calculateHash(item);
                signer.BlockUpdate(hash, 0, hash.Length);
                byte[] signature = signer.GenerateSignature();
                //Upisivanje digitalnog potpisa u fajl
                File.WriteAllBytes(Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "documents" + Path.DirectorySeparatorChar + nameAndSurname.Split('=')[1].Split(' ')[0] + " " + nameAndSurname.Split('=')[1].Split(' ')[1] + Path.DirectorySeparatorChar + documentName + Path.DirectorySeparatorChar + documentName + i++ + Path.DirectorySeparatorChar + "Signature")), signature);

            }

        }

        private bool verifySignature(string signaturePath, byte[] documentSegment)
        {

            byte[] signature = File.ReadAllBytes(signaturePath);
            byte[] documentHash = calculateHash(documentSegment);
            ISigner verifier = SignerUtilities.GetSigner("SHA-256withRSA");
            verifier.Init(false, keyPair.Public);

            verifier.BlockUpdate(documentHash, 0, documentHash.Length);
            return verifier.VerifySignature(signature);

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
            documentName = Path.GetFileName(selectedFileName).Split('.')[0];
            this.digitalSigning(selectedFileName);  //privremeno zakomentarisano zbog testiranja dekriptovanja
            Console.WriteLine(Path.GetFileName(selectedFileName));

        }

        private void getAllUserFiles()
        {
            string path = Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "documents" + Path.DirectorySeparatorChar + nameAndSurname.Split('=')[1].Split(' ')[0] + " " + nameAndSurname.Split('=')[1].Split(' ')[1]));
            string[] foldersss = Directory.GetDirectories(path);
            if(foldersss.Length > 0)
            {
                foreach (string file in foldersss)
                {
                    Console.WriteLine("allfiles " + file);
                    listOfUserFolders.Items.Add(file);
                }
            }
           
        }

        private void listOfUserFolders_GotFocus(object sender, RoutedEventArgs e)
        {


            Console.WriteLine("**************" + sender.ToString().Split(':')[2]);


            string folderPath = System.IO.Path.GetFullPath(sender.ToString().Split(':')[2]);
            Console.WriteLine(folderPath);
            folders = Directory.GetDirectories(folderPath);
            decryptFileContent(folders);
            
        }

        private string decryptFileContent(string[] folderPath)
        {

            string folderName = Path.GetFileName(folderPath[0]);
           // StreamWriter sw = new StreamWriter(Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "downloads" + Path.DirectorySeparatorChar   + folderName + ".txt")));
            bool writeIntoFile = true;
            Console.WriteLine($"{folderName}");
            string fileContentString = "";
            foreach (var folder in folderPath)
            {
                string[] files = Directory.GetFiles(folder, "*.txt");
                string[] Signaturefiles = Directory.GetFiles(folder,"Signature*");
                int i = 0;
                
                foreach (var file in files)
                {
                    string pathss = Path.GetFullPath(file);
                    StreamReader reader = new StreamReader(pathss);
                    string content = reader.ReadToEnd();
                  byte[] decryptedBytes = new byte[1];

                    try
                    {
                        decryptedBytes = Decrypt(Base64.Decode(content));
                    }catch(Exception ex) { }


                        if (verifySignature(Signaturefiles[i++], decryptedBytes))
                        {
                             fileContentString += Encoding.UTF8.GetString(decryptedBytes);
                           
                            Console.WriteLine("decrypt " + BitConverter.ToString(decryptedBytes));
                        }
                        else
                        {
                        writeIntoFile = false;
                        MessageBox.Show("Nije moguce preuzeti izmjenjeni dokument", "Upozorenje", MessageBoxButton.OK, MessageBoxImage.Error);
                       
                        break;
                    }
                    
                  
                    

                }
              
            }
            if (writeIntoFile)
            {
                StreamWriter sw = new StreamWriter(Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "downloads" + Path.DirectorySeparatorChar + folderName + ".txt")));
                sw.Write(fileContentString);
                sw.Close();
            }
            

            return "string";
        }

    }
}


