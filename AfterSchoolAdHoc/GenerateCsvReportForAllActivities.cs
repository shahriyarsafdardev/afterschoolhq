using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using File = System.IO.File;

namespace AfterSchoolAdHoc;

public static class GenerateCsvReportForAllActivities
{
    public static void Run(DatabaseConnection dc)
    {
        const int accountId = 1;

        var msgs = new SystemMessages();
        var list = Activity.LoadByAccountId(accountId, null, null, null, false, false, null, null, null, null, msgs, dc);
        var participationObjectIds = list.Where(x => x.IsPublished).Select(x => x.ID!.Value).ToList();

        var account = Account.Load(accountId, msgs, dc);
        if (account is null) throw new Exception("Account not found");
        var byteData = RegistrationService.GetCsvByteArray(account, null, null, participationObjectIds, ParticipationObjectType.Activity,
            RegistrationReportType.Dismissal, null, RegistrationStatus.NotSet, false, msgs, dc);
        File.WriteAllBytes($"report-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.csv", byteData);
        Console.WriteLine(@"Report successfully saved");
    }
}