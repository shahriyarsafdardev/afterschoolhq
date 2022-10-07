using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolAdHoc;

public static class GenerateCsvReportForSession
{
    public static void Run(int accountId, RegistrationReportType reportType, string activityGroupUid, DatabaseConnection dc)
    {
        var msgs = new SystemMessages();
        if (activityGroupUid.IsNotGuid())
            throw new Exception("UID not properly formatted");

        var account = Account.Load(accountId, msgs, dc);
        if (account is null)
            throw new Exception("Account not found");
        var group = account.GetActivityGroup(activityGroupUid.ToGuid(), msgs, dc);

        var activityIds = group.GetActivities(null, false, false, null, null, false, msgs, dc).Select(x => x.ID!.Value).ToList();
        var byteData = RegistrationService.GetCsvByteArray(account, null, null, activityIds, ParticipationObjectType.Activity, reportType, null,
            RegistrationStatus.NotSet, false, msgs, dc);

        AdHocUtilities.SaveReport(byteData);
    }
}