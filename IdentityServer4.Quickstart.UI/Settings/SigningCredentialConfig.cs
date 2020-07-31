using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Quickstart.UI.Settings
{
    public class SigningCredentialConfig
    {
        public string SigningCredentialType { get; set; }
        public string CertName { get; set; }
        public string ClientId { get; set; }
        public string Domain { get; set; }
        public string CertPassword { get; set; }
    }
}
