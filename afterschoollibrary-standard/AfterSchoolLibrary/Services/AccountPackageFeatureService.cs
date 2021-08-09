using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class AccountPackageFeatureService
    {
        public static AccountPackage GetAccountPackage(this AccountPackageFeature accountPackageFeature, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (accountPackageFeature.IsNotEmpty())
                return AccountPackage.Load(accountPackageFeature.AccountPackageId.Value, msgs, dc);

            return null;
        }
        public static Account GetAccount(this AccountPackageFeature accountPackageFeature, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (accountPackageFeature.IsNotEmpty())
                return accountPackageFeature.GetAccountPackage(msgs, dc)?.GetAccount(msgs, dc);

            return null;
        }
    }
}
