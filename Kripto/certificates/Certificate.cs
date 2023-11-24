using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Operators;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using Org.BouncyCastle.X509;
using System.Security.Cryptography.X509Certificates;
using Kripto.view;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Asn1;
using System.Runtime.ConstrainedExecution;
using Org.BouncyCastle.OpenSsl;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace Kripto.certificates
{
    internal class Certificate
    {
        public static string currentDirectory = Environment.CurrentDirectory;
        // public static string rootCaPath = Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto";
        public static string rootCaPath = System.IO.Path.Combine( currentDirectory , @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar +"rootCA.cer");
        public static string rootCAKey = System.IO.Path.Combine( currentDirectory , @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar +"rootCAKey.pfx");
        public static string userCertFolder = System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar + "userCerts" );
        public static string path = Path.GetFullPath(rootCaPath);
        public static string pathCRL = Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar + "CRL.crl"));
        public static string pathRootKey = Path.GetFullPath(System.IO.Path.Combine(currentDirectory, @".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "cripto" + Path.DirectorySeparatorChar + "rootCAKey.pfx"));
        public static X509Certificate CAcertificate;

        public static X509Certificate2 generateRootCA()
        {
            
            if (File.Exists(path))
            {
                Console.WriteLine("postoji");
                return null;
            }
            else
            {
                Console.WriteLine("ne postojiiiii");
                RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
                keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
                AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();

                X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
                X509Name subjectDN = new X509Name($"CN=CA Tijelo");
                BigInteger serialNumber = BigInteger.ProbablePrime(120, new Random());

                certGenerator.SetSerialNumber(serialNumber);
                certGenerator.SetSubjectDN(subjectDN);
                certGenerator.SetIssuerDN(subjectDN);
                certGenerator.SetNotBefore(DateTime.UtcNow.Date);
                certGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(1));
                certGenerator.SetPublicKey(keyPair.Public);
                certGenerator.AddExtension(X509Extensions.BasicConstraints.Id, true, new BasicConstraints(true));
                var authorityKeyIdentifier = new AuthorityKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public), new GeneralNames(new GeneralName(subjectDN)), serialNumber);
                certGenerator.AddExtension(
                    X509Extensions.AuthorityKeyIdentifier.Id, false, authorityKeyIdentifier);
                var subjectKeyIdentifier =
                new SubjectKeyIdentifier(
                SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public));
                certGenerator.AddExtension(
                    X509Extensions.SubjectKeyIdentifier.Id, false, subjectKeyIdentifier);
                var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private);
                CAcertificate = certGenerator.Generate(signatureFactory);
                CAcertificate.GetBasicConstraints();
                byte[] certData = CAcertificate.GetEncoded();

                File.WriteAllBytes(rootCaPath, certData);
              
                using (FileStream keyFileStream = File.Create(rootCAKey))
                {
                    var builder = new Pkcs12StoreBuilder();
                    builder.SetUseDerEncoding(true);
                    Pkcs12Store store = builder.Build();
                    store.SetKeyEntry("CA Tijelo", new AsymmetricKeyEntry(keyPair.Private), new X509CertificateEntry[] { new X509CertificateEntry(CAcertificate) });

                    store.Save(keyFileStream, "sigurnost".ToCharArray(), new SecureRandom());
                }

                X509Certificate2 certificate2 = new X509Certificate2(rootCaPath, "sigurnost", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

                if (File.Exists(pathCRL))
                {
                    Console.WriteLine("crl lista ne postoji");
                }
                else
                {

                    X509V2CrlGenerator crlGen = new X509V2CrlGenerator();
                    crlGen.SetIssuerDN(CAcertificate.IssuerDN);
                    crlGen.SetThisUpdate(DateTime.Now);
                    crlGen.SetNextUpdate(DateTime.Now.AddYears(1));
                  
                    var randomGenerator = new CryptoApiRandomGenerator();
                    var random = new SecureRandom(randomGenerator);
                    string signatureAlgorithm = "SHA256WITHRSA";
                    ISignatureFactory sf = new Asn1SignatureFactory(signatureAlgorithm,keyPair.Private, random);
                    X509Crl crlTemp = crlGen.Generate(sf);
                    System.IO.File.WriteAllBytes(pathCRL, crlTemp.GetEncoded());

                }
                return certificate2;
            }
        }

     
        public static void createUserCertificate(string name, string surname,string password, string username)
        {

            Stream caCert = new FileStream(path,FileMode.Open);
            X509Certificate signedX509CaCert = new X509CertificateParser().ReadCertificate(caCert);
            X509CertificateEntry certCaEntry = new X509CertificateEntry(signedX509CaCert);

            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
                keyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
                AsymmetricCipherKeyPair keyPair = keyPairGenerator.GenerateKeyPair();
                X509V3CertificateGenerator certGenerator = new X509V3CertificateGenerator();
                X509Name subjectDN = new X509Name($"CN=" + name + " " + surname);
                BigInteger serialNumber = BigInteger.ProbablePrime(120, new Random());

                certGenerator.SetSerialNumber(serialNumber);
                certGenerator.SetSubjectDN(subjectDN);
                certGenerator.SetIssuerDN(certCaEntry.Certificate.IssuerDN);
                certGenerator.SetNotBefore(DateTime.UtcNow.Date);
                certGenerator.SetNotAfter(DateTime.UtcNow.Date.AddMonths(1)); // DateTime.UtcNow.Date.AddMonths(6) vrattiti na ovo poslije jer je tako zadano po projektnom zadatku
            certGenerator.SetPublicKey(keyPair.Public);
                certGenerator.AddExtension(X509Extensions.BasicConstraints.Id, true, new BasicConstraints(false));
            var keyUsage = new KeyUsage(KeyUsage.KeyCertSign);
            certGenerator.AddExtension(X509Extensions.KeyUsage, false, keyUsage.ToAsn1Object());
            var authorityKeyIdentifier = new AuthorityKeyIdentifier(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public), new GeneralNames(new GeneralName(subjectDN)), serialNumber);
            certGenerator.AddExtension(
                X509Extensions.AuthorityKeyIdentifier.Id, false, authorityKeyIdentifier);
            var subjectKeyIdentifier =
            new SubjectKeyIdentifier(
            SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public));
            certGenerator.AddExtension(
                X509Extensions.SubjectKeyIdentifier.Id, false, subjectKeyIdentifier);
            var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private);
                X509Certificate certificate = certGenerator.Generate(signatureFactory);
                certificate.GetBasicConstraints();
                byte[] certData = certificate.GetEncoded();

            Directory.CreateDirectory(userCertFolder+ Path.DirectorySeparatorChar + name + "_" + surname);
            File.WriteAllBytes(userCertFolder+ Path.DirectorySeparatorChar + name + "_" + surname + Path.DirectorySeparatorChar + name+"Certificate.cer", certData);

                using (FileStream keyFileStream = File.Create(userCertFolder + Path.DirectorySeparatorChar + name + "_" + surname + Path.DirectorySeparatorChar + "Key.pfx"))
                {
                    var builder = new Pkcs12StoreBuilder();
                    builder.SetUseDerEncoding(true);
                    Pkcs12Store pkcsstore = builder.Build();
                    pkcsstore.SetKeyEntry("CA Tijelo", new AsymmetricKeyEntry(keyPair.Private), new X509CertificateEntry[] { new X509CertificateEntry(certificate) });

                    pkcsstore.Save(keyFileStream, password.ToCharArray(), new SecureRandom());
                }
            
           
        }


        public static bool ifCertificateIsValid(string certificatePath)
        {
            Stream userCert = new FileStream(certificatePath, FileMode.Open);
            X509Certificate signedX509CaCert = new X509CertificateParser().ReadCertificate(userCert);
            X509CertificateEntry certUserEntry = new X509CertificateEntry(signedX509CaCert);
            if (certUserEntry.Certificate.IsValidNow)
            {
                LogIn.loggedUserCertificate = certUserEntry.Certificate;
                Console.WriteLine("validaan");
                userCert.Close();
                return true;
          
            }
            Console.WriteLine("nije validaan");
            userCert.Close();
            return false;
        }

        public static  bool isCertificateRevoked(string certificatePath)
        {
            Console.WriteLine("------------" + certificatePath);
            Stream userCert = new FileStream(certificatePath, FileMode.Open);
            X509Certificate signedX509CaCert = new X509CertificateParser().ReadCertificate(userCert);
            X509CertificateEntry certUserEntry = new X509CertificateEntry(signedX509CaCert);
            var crlFile = File.ReadAllBytes(pathCRL);
            X509Crl crl = new X509CrlParser().ReadCrl(crlFile);
            var revokedCertifikates = crl.GetRevokedCertificates();

            foreach (var cert in revokedCertifikates)
            {
                if (certUserEntry.Certificate.SerialNumber.Equals(cert.SerialNumber))
                {
                    return true;
                }                 
            }
            return false;
        }

        public static void revokeCertificate(string certificatePath) 
        {

            X509Certificate2 certificate = new X509Certificate2(pathRootKey, "sigurnost", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
            AsymmetricCipherKeyPair keyPair = DotNetUtilities.GetRsaKeyPair(rsa);
        
            var crlFile = File.ReadAllBytes(pathCRL);
            X509Crl crl = new X509CrlParser().ReadCrl(crlFile);
            Stream userCert = new FileStream(certificatePath, FileMode.Open);
            var cert = new X509CertificateParser().ReadCertificate(userCert);
            var revokedSerialNumber = cert.SerialNumber;
            var revokedDate = DateTime.Now;
            X509V2CrlGenerator gen = new X509V2CrlGenerator(crl);
            gen.AddCrlEntry(revokedSerialNumber, revokedDate, CrlReason.PrivilegeWithdrawn);
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            string signatureAlgorithm = "SHA256WITHRSA";
            ISignatureFactory sf = new Asn1SignatureFactory(signatureAlgorithm, ((AsymmetricCipherKeyPair)keyPair).Private, random);
            X509Crl crlTemp = gen.Generate(sf);
            var updatedCrlFile = crlTemp.GetEncoded();
            File.WriteAllBytes(pathCRL, updatedCrlFile);


        }
    }
}
