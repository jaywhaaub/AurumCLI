using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Azure.Security.ConfidentialLedger;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AurumCLI
{
    public class LedgerModel
    {
        private ConfidentialLedgerClient LedgerClient;
        private bool IsLoggedIn = false;

        public string CurrentUserId;
        public const string ConfidentialLedgerUrl = "https://aucoindev.confidential-ledger.azure.com";

        public LedgerModel()
        {
            Initialize();
        }

        public void Initialize()
        {
            Uri identityServiceEndpoint = new("https://identity.confidential-ledger.core.azure.com");
            var identityClient = new ConfidentialLedgerIdentityServiceClient(identityServiceEndpoint);

            // Get the ledger's  TLS certificate for our ledger. 
            string ledgerId = "aucoindev";
            Response response = identityClient.GetLedgerIdentity(ledgerId);

            // extract the ECC PEM value from the response. 
            var eccPem = JsonDocument.Parse(response.Content)
                .RootElement
                .GetProperty("ledgerTlsCertificate")
                .GetString();

            // construct an X509Certificate2 with the ECC PEM value. 
            X509Certificate2 ledgerTlsCert = new(Encoding.UTF8.GetBytes(eccPem));

            // Create a certificate chain rooted with our TLS cert. 
            X509Chain certificateChain = new();
            certificateChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            certificateChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
            certificateChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            certificateChain.ChainPolicy.VerificationTime = DateTime.Now;
            certificateChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
            certificateChain.ChainPolicy.ExtraStore.Add(ledgerTlsCert);

            // Define a validation function to ensure that the ledger certificate is trusted by the ledger identity TLS certificate. 
            bool CertValidationCheck(HttpRequestMessage httpRequestMessage, X509Certificate2 cert, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors)
            {
                bool isChainValid = certificateChain.Build(cert);
                if (!isChainValid) return false;

                var isCertSignedByTheTlsCert = certificateChain.ChainElements.Cast<X509ChainElement>()
                   .Any(x => x.Certificate.Thumbprint == ledgerTlsCert.Thumbprint);
                return isCertSignedByTheTlsCert;
            }

            // Create an HttpClientHandler to use our certValidationCheck function. 
            var httpHandler = new HttpClientHandler {
                ServerCertificateCustomValidationCallback = CertValidationCheck
            };

            // Create the ledger client using a transport that uses our custom ServerCertificateCustomValidationCallback. 
            Uri uri = new (ConfidentialLedgerUrl);
            ConfidentialLedgerClientOptions options = new() { Transport = new HttpClientTransport(httpHandler) };
            AzureCliCredential credential = new ();

            LedgerClient = new ConfidentialLedgerClient(uri, credential, options);
        }

        public bool Login(string username)
        {
            if (LedgerClient == null) return false;
            List<string> aadIDs = LedgerCredentials.GetADDIDs();

            string managedId = username switch
            {
                "jpw0032" => aadIDs[0],
                _ => aadIDs[0]
            };
            var resp = LedgerClient.CreateOrUpdateUser(managedId, RequestContent.Create(new { assignedRole = "Reader" }));

            IsLoggedIn = resp.Status == 200;
            return IsLoggedIn;
        }

        public void Write(string data)
        {
            LedgerMessage msg = new() { contents = data };
            var json = JsonConvert.SerializeObject(msg);
            Response postResponse = LedgerClient.PostLedgerEntry(RequestContent.Create(json));
            postResponse.Headers.TryGetValue(ConfidentialLedgerConstants.TransactionIdHeaderName, out string transactionId);
            Console.WriteLine($"Appended transaction with Id: {transactionId}");
        }

        public BinaryData Read(string transactionId = null)
        {
            return transactionId switch
            {
                not null => LedgerClient.GetLedgerEntry(transactionId).Content,
                _ => LedgerClient.GetCurrentLedgerEntry().Content,
            };
        }
    }
}
