using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AurumCLI
{
    public class LedgerTransactionModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private string _transactionId;

        public string TransactionId
        {
            get => _transactionId;
            set { 
                _transactionId = value;
                OnPropertyChanged();
            }
        }

        public LedgerTransactionModel()
        {

        }
    }
}
