using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class FixAccountPackageIsActive_20210603
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var groupedAccountPackages = AccountPackage.Load(msgs: amsgs, dc: adc)?.GroupBy(m => m.AccountId).OrderBy(m => m.FirstOrDefault().AccountId).ToList();

                    foreach (var accountPackages in groupedAccountPackages)
                    {
                        var lstActiveAccountPackages = accountPackages.Where(m => m.IsActive).OrderByDescending(m => m.CreateDate);

                        foreach (var accountPackageTobeUpdated in lstActiveAccountPackages.Skip(1))
                        {
                            accountPackageTobeUpdated.IsActive = false;
                            accountPackageTobeUpdated.LastModifiedDate = DateTime.Now;
                            amsgs.AddRange(accountPackageTobeUpdated.Save(adc));
                        }
                    }
                },
                uniqueIdenfier: "FixAccountPackageIsActive_20210603",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
