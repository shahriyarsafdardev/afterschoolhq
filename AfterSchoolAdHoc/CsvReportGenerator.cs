using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchoolAdHoc;

public static class CsvReportGenerator
{
    public static void Run(int accountId, RegistrationReportType reportType, string? commaSeparatedActivityGroupIds, DatabaseConnection dc)
    {
        var msgs = new SystemMessages();
        var account = Account.Load(accountId, msgs, dc);
        if (account is null)
            throw new Exception("Account not found");
        var byteData = commaSeparatedActivityGroupIds is not null && commaSeparatedActivityGroupIds.IsNotEmpty()
            ? GetReportByteDataForSessionReport(account, reportType, commaSeparatedActivityGroupIds, msgs, dc)
            : GetReportByteDataForReport(account, reportType, msgs, dc);

        AdHocUtilities.SaveReport(byteData);
    }

    private static byte[] GetReportByteDataForSessionReport(Account account, RegistrationReportType reportType, string commaSeparatedActivityGroupIds,
        SystemMessages msgs, DatabaseConnection dc)
    {
        var activityIds = new List<int>();
        var groupIds = commaSeparatedActivityGroupIds.Split(',').Select(x => x.ToInteger()).ToList();
        var activityGroups = account.GetActivityGroups(groupIds, msgs, dc);
        activityGroups.ForEach(group =>
            activityIds.AddRange(group.GetActivities(null, false, false, null, null, false, msgs, dc).Select(y => y.ID!.Value).ToList()));

        return RegistrationService.GetCsvByteArray(account, null, null, activityIds, ParticipationObjectType.Activity, reportType, null,
            RegistrationStatus.NotSet, false, msgs, dc);
    }

    private static byte[] GetReportByteDataForReport(Account account, RegistrationReportType reportType, SystemMessages msgs, DatabaseConnection dc)
    {
        var list = Activity.LoadByAccountId(account.ID!.Value, null, null, null, false, false, null, null, null, null, msgs, dc);
        var participationObjectIds = list.Where(x => x.IsPublished).Select(x => x.ID!.Value).ToList();

        return RegistrationService.GetCsvByteArray(account, null, null, participationObjectIds, ParticipationObjectType.Activity,
            reportType, null, RegistrationStatus.NotSet, false, msgs, dc);
    }
}