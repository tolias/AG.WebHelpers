using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using AG.AssemblyInfo;
using AG.FormAdditions;
using AG.WebHelpers.Security;

namespace AG.WebHelpers.UI.Security
{
    public class InvalidCertificatesHandler
    {
        private object _syncObj = new object();
        public ITrustedCertificateIssuersContainer trustedCertificateIssuersContainer;

        public InvalidCertificatesHandler(ITrustedCertificateIssuersContainer trustedCertificateIssuersContainer)
        {
            this.trustedCertificateIssuersContainer = trustedCertificateIssuersContainer;
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
        }

        private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            string certificateIssuer = certificate.Issuer;
            lock (_syncObj)
            {
                if (trustedCertificateIssuersContainer.TrustedCertificateIssuers != null && trustedCertificateIssuersContainer.TrustedCertificateIssuers.Contains(certificateIssuer))
                    return true;

                var request = sender as HttpWebRequest;
                string server = request == null ? null : " \"" + request.Address.Host + "\"";
                string msg = string.Format("{0} cannot verify the identity of the server{1}, due to a certificate problem. the server could be trying to trick you. Would you like to continue to the server?"
                    + "\r\n\r\nCertificate: {2}"
                    + "\r\n\rSSL error: {3}", ProgramInfo.Name, server, certificate, sslPolicyErrors);

                var showCertificateButton = new Button()
                {
                    Text = "Show certificate",
                    Width = 100
                };
                showCertificateButton.Click += (s, e) =>
                {
                    var x509Certificate2 = new X509Certificate2(certificate);
                    X509Certificate2UI.DisplayCertificate(x509Certificate2);
                };

                string btnContinueAnyWay = "Continue Anyway";
                string btnAddToExclusionInThisSession = "Add to exclusion in this session";
                string btnCancel = "Cancel";
                var pressedButton = AdvancedMessageBox.Show(msg, "Invalid certificate", showCertificateButton, MessageBoxIcon.Warning, btnContinueAnyWay, btnAddToExclusionInThisSession, btnCancel);

                if (pressedButton == btnContinueAnyWay)
                {
                    return true;
                }
                else if (pressedButton == btnAddToExclusionInThisSession)
                {
                    var trustedCertificateIssuers = trustedCertificateIssuersContainer.TrustedCertificateIssuers;
                    if (trustedCertificateIssuers == null)
                    {
                        trustedCertificateIssuers = new HashSet<string>();
                    }
                    trustedCertificateIssuers.Add(certificateIssuer);
                    trustedCertificateIssuersContainer.TrustedCertificateIssuers = trustedCertificateIssuers;
                    return true;
                }
            }
            return false;
        }
    }
}
