using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class Migration_20220606_VolunteerManagement
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    using (var connection = new SqlConnection(adc.ConnectionString))
                    {
                        connection.Execute(@"insert into FeatureFlag (FlagName, IsEnabled) values ('volunteer-management', 0)");
                    }
                    
                    var packages = Package.Load(msgs: msgs, dc: dc);
                    packages.ForEach(package =>
                    {
                        PackageFeatureService.AddPackageFeature(package.ID.Value, FeatureType.ManageVolunteerOpportunities, 0, 0, msgs, dc);
                    });

                    var accountPackages = AccountPackage.Load(msgs: msgs, dc: dc);
                    accountPackages.ForEach(accountPackage =>
                    {
                        accountPackage.AddOrUpdatePackageFeature(FeatureType.ManageVolunteerOpportunities, 0, 0, msgs, dc);
                    });
                },
                uniqueIdenfier: "Migration_20220606_VolunteerManagement",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
