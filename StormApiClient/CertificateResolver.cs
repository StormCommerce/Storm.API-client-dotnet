using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Web.Configuration;
using Enferno.Public.Logging;

namespace Enferno.StormApiClient
{
    public interface ICertificateResolver
    {      
        X509Certificate2 GetCertificate();
    }

    /// <summary>
    /// Specify API.CertFile and API.CertPwd in AppSettings to load certificate from file.
    /// API.CertFile should be relative to the application, i.e App_Data\MyCertFile.pfx. (as Content and Copy Always)
    /// </summary>
    public class CertificateResolver : ICertificateResolver
    {
        protected string File { get; set; }
        protected string Password { get; set; }

        private static readonly object syncRoot = new object();
        private static readonly Dictionary<string, X509Certificate2> certificates = new Dictionary<string, X509Certificate2>();

        public CertificateResolver()
        {
            File = WebConfigurationManager.AppSettings["API.CertFile"];
            Password = WebConfigurationManager.AppSettings["API.CertPwd"];
        }

        public CertificateResolver(string file, string password)
        {
            File = file;
            Password = password;
        }

        public X509Certificate2 GetCertificate()
        {
            if (string.IsNullOrWhiteSpace(File))
            {
                Log.LogEntry.Categories(AccessClient.LogCategory).Categories(CategoryFlags.Debug).Message("CertificateResolver.GetCertificate: Filename missing.").WriteWarning();
                return null;
            }

            if (certificates.ContainsKey(File)) return certificates[File];
            lock (syncRoot)
            {
                if (certificates.ContainsKey(File)) return certificates[File];

                var fullFileName = Path.IsPathRooted(File) ? File : Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, File);
                var certificate = new X509Certificate2(fullFileName, Password, X509KeyStorageFlags.MachineKeySet);
                certificates.Add(File, certificate);

                Log.LogEntry.Categories(AccessClient.LogCategory).Categories(CategoryFlags.Debug).Message("CertificateResolver.GetCertificate: Loaded certificate.")
                    .Property("Filename", fullFileName)
                    .Property("Certificate", certificate.ToString())
                    .WriteInformation();

                return certificate;
            }
        }
    }
}
