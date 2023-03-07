using AfterSchoolLibrary.Migrations;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolLibrary.Helpers
{
    public class DatabaseInit
    {
        public static SystemMessages InitDatabase(DatabaseCredentials databaseCredentials, string adminEmail = null,
            string adminPassword = null,
            string adminPhone = null,
            bool skipTableCreation = false, string accountName = "First Group")
        {
            var msgs = new SystemMessages();

            if (!skipTableCreation)
            {
                var dc2 = new DatabaseConnection(databaseCredentials.ConnectionString);

                msgs.AddRange(Database.CreateTables(dc2));
                msgs.AddRange(Database.AlterTables(dc2));
                msgs.AddRange(Database.UpdateData(dc2));
                msgs.AddRange(Database.CreateIndexes(dc2));
                dc2 = null;
            }

            msgs.Clear();

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    if (WebUser.GetCount() == 0)
                        amsgs.AddRange(AccountService.CreateAfterSchoolAccount(adminEmail, adminPassword, adminPhone,
                            accountName, adc));
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs)
            );

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    amsgs.AddRange(AdditionalData(adc));

                    Database.RunMigrations(amsgs, adc);
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs)
            );

            return msgs;
        }

        public static SystemMessages AdditionalData(DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            msgs.AddRange(GradeLevel.CreateDefaultGradeLevels(dc));

            FormService.AddNewAfterSchoolDefaultStudentForm(msgs, dc);

            var packageCount = Package.GetCount(null, msgs, dc);
            if (packageCount == 0)
            {
                var package = PackageService.CreateDefaultPackage(null, msgs, dc);
                msgs.AddRange(AccountService.CreateSampleAccount(AccountType.School, package.ID.Value, dc));
            }

            return msgs;
        }
    }
}