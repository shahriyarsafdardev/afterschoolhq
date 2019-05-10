using AfterSchoolLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AfterSchool.Models
{
    public class BankInfoViewModel
    {
        public string RoutingNumber1 { get; set; }
        public string RoutingNumber2 { get; set; }
        public string AccountNumber1 { get; set; }
        public string AccountNumber2 { get; set; }
        public string BankName { get; set; }
        public string AccountHoldersName { get; set; }
        public BankAccountType? BankAccountTypeId { get; set; }
        public string BankCountry { get; set; }

        public static BankInfoViewModel FromBankPaymentAccount(BankPaymentAccount bankPaymentAccount)
        {
            return new BankInfoViewModel()
            {
                RoutingNumber1 = bankPaymentAccount.RoutingNumber,
                RoutingNumber2 = bankPaymentAccount.RoutingNumber,
                AccountHoldersName = bankPaymentAccount.AccountHoldersName,
                BankAccountTypeId = bankPaymentAccount.BankAccountTypeId,
                BankCountry = bankPaymentAccount.BankCountry,
                BankName = bankPaymentAccount.BankName
            };
        }
    }
}