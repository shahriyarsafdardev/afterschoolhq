using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolAdHoc;

public static partial class CsvReportGenerator
{
    public static void RunForAllActivities(int accountId, DatabaseConnection dc)
    {
        var msgs = new SystemMessages();
        var list = Activity.LoadByAccountId(accountId, null, null, null, false, false, null, null, null, null, msgs, dc);
        var participationObjectIds = list.Where(x => x.IsPublished).Select(x => x.ID!.Value).ToList();

        var account = Account.Load(accountId, msgs, dc);
        if (account is null) throw new Exception("Account not found");
        var byteData = RegistrationService.GetCsvByteArray(account, null, null, participationObjectIds, ParticipationObjectType.Activity,
            RegistrationReportType.DismissalForHostedActivities, null, RegistrationStatus.NotSet, false, msgs, dc);

        AdHocUtilities.SaveReport(byteData);
    }
}