using System.Collections.Generic;

namespace AG.WebHelpers.Security
{
    public interface ITrustedCertificateIssuersContainer
    {
        HashSet<string> TrustedCertificateIssuers { get; set; }
    }
}
