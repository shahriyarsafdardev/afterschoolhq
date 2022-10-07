using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolAdHoc;

public static class CsvReportGenerator
{
    public static void Run(int accountId, RegistrationReportType? reportType, string? commaSeparatedActivityGroupIds, DatabaseConnection dc)
    {
        var msgs = new SystemMessages();
        byte[] byteData;
        if (reportType is not null && commaSeparatedActivityGroupIds is not null)
        {
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

            byteData = RegistrationService.GetCsvByteArray(account, null, null, activityIds, ParticipationObjectType.Activity, reportType.Value, null,
                RegistrationStatus.NotSet, false, msgs, dc);
        }
        else
        {
            var list = Activity.LoadByAccountId(accountId, null, null, null, false, false, null, null, null, null, msgs, dc);
            var participationObjectIds = list.Where(x => x.IsPublished).Select(x => x.ID!.Value).ToList();

            var account = Account.Load(accountId, msgs, dc);
            if (account is null) throw new Exception("Account not found");
            byteData = RegistrationService.GetCsvByteArray(account, null, null, participationObjectIds, ParticipationObjectType.Activity,
                RegistrationReportType.DismissalForHostedActivities, null, RegistrationStatus.NotSet, false, msgs, dc);
        }

        AdHocUtilities.SaveReport(byteData);
    }
}