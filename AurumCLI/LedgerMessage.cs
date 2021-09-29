using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AurumCLI
{
    [DataContract]
    public class LedgerMessage
    {
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Match case-sensitive syntax")]
        public string contents { get; set; }
    }
}
