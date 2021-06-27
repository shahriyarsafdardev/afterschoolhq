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
                    var groupedAccountPackages = AccountPackage.Load(
                        new Hashtable() { { "IsActive", 1 } }, 
                        msgs: amsgs, 
                        dc: adc)?
                    .GroupBy(m => m.AccountId)
                    .Where(m => m.Count() > 1)
                    .OrderBy(m => m.FirstOrDefault().AccountId)
                    .ToList();

                    foreach (var accountPackages in groupedAccountPackages)
                    {
                        //skip is needed as we want to make sure that only lastly added package remains active, and all other will be inactive
                        foreach (var accountPackageTobeUpdated in accountPackages.OrderByDescending(m => m.CreateDate).Skip(1))
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
