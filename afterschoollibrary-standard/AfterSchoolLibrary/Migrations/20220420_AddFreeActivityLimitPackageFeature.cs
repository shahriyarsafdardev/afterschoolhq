using System;
using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;
using AfterSchoolLibrary.Services;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220420_AddFreeActivityLimitPackageFeature
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var packages = Package.Load(msgs: msgs, dc: dc);
                    packages.ForEach(package =>
                    {
                        var maxValue = package.Price > 0 ? Int32.MaxValue : 0;
                        
                        PackageFeatureService.AddPackageFeature(
                            package.ID.Value,
                            FeatureType.FreeActivityLimit,
                            maxValue,
                            0,
                            msgs, dc);
                    });

                    var accountPackages = AccountPackage.Load(msgs: msgs, dc: dc);
                    accountPackages.ForEach(accountPackage =>
                    {
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.FreeActivityLimit, Int32.MaxValue, 0, msgs, dc);
                    });
                    
                },
                uniqueIdenfier: "Migration_20220420_AddFreeActivityLimitPackageFeature",
                msgs: msgs,
                dc: dc
            );
        }
    }
}