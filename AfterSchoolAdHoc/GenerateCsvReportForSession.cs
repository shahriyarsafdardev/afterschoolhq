using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using File = System.IO.File;

namespace AfterSchoolAdHoc;

public static class GenerateCsvReportForSession
{
    public static void Run(DatabaseConnection dc)
    {
        const int accountId = 1;
        const RegistrationReportType reportType = RegistrationReportType.Detailed;
        const string activityGroupUid = "00000000-0000-0000-0000-000000000000";

        var msgs = new SystemMessages();
        if (activityGroupUid.IsNotGuid())
            throw new Exception("UID not properly formatted");

        var account = Account.Load(accountId, msgs, dc);
        if (account is null)
            throw new Exception("Account not found");
        var group = account.GetActivityGroup(activityGroupUid.ToGuid(), msgs, dc);

        var activityIds = group.GetActivities(null, false, false, null, null, false, msgs, dc).Select(x => x.ID.Value).ToList();
        var byteData = RegistrationService.GetCsvByteArray(account, activityIds, reportType, msgs, dc);

        File.WriteAllBytes($"report-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv", byteData);
        Console.WriteLine(@"Report successfully saved");
    }
}