using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class BankAccountPurchaseObject
    {
        public string PlaidPublicToken { get; set; }
        public string PlaidAccountId { get; set; }
        public string StripeSourceId { get; set; }
    }
}
