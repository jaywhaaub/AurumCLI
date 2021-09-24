using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AurumCLI
{
    public class LedgerCredentials
    {
        //TODO: Get AAD IDs from Azure Key Vault

        public List<string> GetADDIDs() => File.ReadAllLines(@"C:\MS-Blockchain-AAD-IDs.txt").ToList();
    }
}
