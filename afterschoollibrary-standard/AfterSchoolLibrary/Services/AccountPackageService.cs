using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AccountPackageService
    {
        public static Account GetAccount(this AccountPackage accountPackage, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (accountPackage.IsNotEmpty())
                return Account.Load(accountPackage.AccountId.Value, msgs, dc);

            return null;
        }
    }
}
