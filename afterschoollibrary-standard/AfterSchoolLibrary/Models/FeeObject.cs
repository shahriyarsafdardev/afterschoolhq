using System;
using System.Collections.Generic;
using System.Text;
namespace AfterSchoolLibrary.Models
{
    public class PlanFeeObject
    {
        public FeesPaidByType PaymentTypeFeesPaidBy { get; set; }
        public FeesPaidByType ConvenienceFeesPaidBy { get; set; }
        public decimal ConvenienceFee { get; set; }
        public decimal PaymentTypeFee { get; set; }
        public decimal Amount { get; set; }
        public int? TotalPeriods { get; set; }
    }
}
