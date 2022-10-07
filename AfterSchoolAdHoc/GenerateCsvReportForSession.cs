using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolAdHoc;

public static class GenerateCsvReportForSession
{
    public static void Run(int accountId, RegistrationReportType reportType, string activityGroupUid, DatabaseConnection dc)
    {
        var commaSeparatedActivityGroupIds = activityGroupUid;
        var msgs = new SystemMessages();

        var account = Account.Load(accountId, msgs, dc);
        if (account is null)
            throw new Exception("Account not found");

        var activityIds = new List<int>();
        if (commaSeparatedActivityGroupIds.IsNotEmpty())
        {
            var groupIds = commaSeparatedActivityGroupIds.Split(',').Select(x => x.ToInteger()).ToList();
            var activityGroups = account.GetActivityGroups(groupIds, msgs, dc);
            activityGroups.ForEach(group =>
                activityIds.AddRange(group.GetActivities(null, false, false, null, null, false, msgs, dc).Select(y => y.ID!.Value).ToList()));
        }

        var byteData = RegistrationService.GetCsvByteArray(account, null, null, activityIds, ParticipationObjectType.Activity, reportType, null,
            RegistrationStatus.NotSet, false, msgs, dc);

        AdHocUtilities.SaveReport(byteData);
    }
}