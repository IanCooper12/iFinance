using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Group13iFinanceFix.Models
{
    public class FinanceTransactionViewModel
    {
        public FinanceTransaction Transaction { get; set; }
        public List<TransactionLine> TransactionLines { get; set; } = new List<TransactionLine>();

    }
}