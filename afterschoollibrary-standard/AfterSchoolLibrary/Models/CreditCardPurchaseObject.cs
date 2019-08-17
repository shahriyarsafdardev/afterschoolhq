using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Models
{
    public class CreditCardPurchaseObject
    {
        public string StripeToken { get; set; }
        public string StripeSourceId { get; set; }
    }
}
