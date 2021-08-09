using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Linq;
using System.Collections.Generic;

namespace AfterSchoolLibrary.Services
{
    public static class PackageFeatureService
    {
        public static PackageFeature AddPackageFeature(int? PackageId, FeatureType featureTypeId, decimal maxValue, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            var packageFeature = new PackageFeature();

            packageFeature.PackageId = PackageId;
            packageFeature.FeatureTypeId = featureTypeId;
            packageFeature.CreateDate = DateTime.UtcNow;
            packageFeature.CreatedBy = loggedInUser?.ID.IfEmpty(0);
            packageFeature.Max = maxValue;
            packageFeature.LastModifiedDate = DateTime.UtcNow;
            packageFeature.LastModifiedBy = loggedInUser?.ID.IfEmpty(0);

            msgs.AddRange(packageFeature.Save(dc: dc));
            return packageFeature;
        }

        public static void EnsureAllAccountsHavePackageFeature(this PackageFeature packageFeature, List<AccountType> accountTypes, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var accountCount = Account.GetSearchCount(null, AccountService.ProviderAccountTypes.ToList(), false, amsgs, adc);
                    var totalProcessed = 0;
                    var pageSize = 50;
                    var start = 0;

                    while(totalProcessed <= accountCount)
                    {
                        var accounts = Account.Search(null, AccountService.ProviderAccountTypes.ToList(), false, start, pageSize, amsgs, adc);

                        accounts.ForEach(account => packageFeature.EnsureAccountHasPackageFeature(account, accountTypes, loggedInUser, amsgs, adc));

                        start += pageSize;
                        totalProcessed = start + 1;
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => msgs.AddRange(emsgs),
                dc: dc
            );

        }

        public static void EnsureAccountHasPackageFeature(this PackageFeature packageFeature, Account account, List<AccountType> accountTypes, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (account.IsEmpty())
                return;

            if (account.AccountTypeId.NotIn(accountTypes))
                return;

            if (!account.IsActiveAccountPackage(packageFeature.PackageId, msgs, dc))
                return;

            if (!account.HasAccountPackageFeature(packageFeature.FeatureTypeId, msgs, dc))
                account.AddOrUpdatePackageFeature(packageFeature.FeatureTypeId, packageFeature.Max.Value, loggedInUser, msgs, dc);
        }

        public static void ResetAllAccountFeaturesToPackageFeature(this PackageFeature packageFeature, List<AccountType> accountTypes, bool resetIfAcctValueIsGreaterThan, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var accountFeatures = AccountPackageFeature.LoadByFeatureTypeId(packageFeature.FeatureTypeId, amsgs, adc);

                    packageFeature.EnsureAllAccountsHavePackageFeature(accountTypes, loggedInUser, amsgs, adc);

                    accountFeatures.ForEach(accountFeature => {
                        packageFeature.ResetAccountFeatureToPackageFeature(
                            accountFeature.GetAccount(amsgs, adc),
                            accountTypes,
                            resetIfAcctValueIsGreaterThan, 
                            loggedInUser, 
                            amsgs, 
                            adc);
                    });

                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => msgs.AddRange(emsgs),
                dc: dc
            );
        }

        public static void ResetAccountFeatureToPackageFeature(this PackageFeature packageFeature, Account account, List<AccountType> accountTypes, bool resetIfAcctValueIsGreaterThan, WebUser loggedInUser, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (account.IsEmpty())
                return;

            if (account.AccountTypeId.NotIn(accountTypes))
                return;

            if (!account.IsActiveAccountPackage(packageFeature.PackageId, msgs, dc))
                return;

            packageFeature.EnsureAccountHasPackageFeature(account, accountTypes, loggedInUser, msgs, dc);

            var apf = account.GetAccountPackageFeature(packageFeature.FeatureTypeId, msgs, dc);

            if (resetIfAcctValueIsGreaterThan || apf.Max < packageFeature.Max)
                account.AddOrUpdatePackageFeature(
                    packageFeature.FeatureTypeId,
                    packageFeature.Max.Value,
                    loggedInUser,
                    msgs,
                    dc);
        }
    }
}
