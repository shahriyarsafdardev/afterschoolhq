using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System.Linq;

namespace AfterSchoolLibrary.Services
{
    /// <summary>
    /// AccountActivityGroup is the way to manage invites to an external league or session
    /// </summary>
    public static class AccountActivityGroupService
    {
        public static AccountActivityGroup CreateNew(Account account, ActivityGroup activityGroup, InviteStatus inviteStatus, WebUser loggedInUser, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            var aag = AccountActivityGroup.LoadByAccountIdByActivityGroupId(account.ID.Value, activityGroup.ID.Value, msgs, dc);

            if (aag.IsEmpty())
            {
                aag = new AccountActivityGroup()
                {
                    AccountId = account.ID,
                    ActivityGroupId = activityGroup.ID,
                    CreateDate = DateTime.UtcNow,
                    CreatedBy = loggedInUser?.ID.IfEmpty(0),
                    InviteStatusId = inviteStatus,
                    IsPublished = false,
                    LastModifiedBy = loggedInUser?.ID.IfEmpty(0),
                    LastModifiedDate = DateTime.UtcNow
                };
                msgs.AddRange(aag.Save(dc));
            }
            else
            {
                msgs.Add("Invite already exists", MessageType.Notice);
            }

            return aag;
        }

        public static ActivityGroup GetActivityGroup(this AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return ActivityGroup.Load(aag.ActivityGroupId.Value, msgs, dc);
        }

        public static ParticipationObject ToParticipationObject(this AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return new ParticipationObject(aag, msgs, dc);
        }

        public static RegistrationPeriod GetCurrentOrNextRegistrationPeriod(this AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var period = aag.GetCurrentRegistrationPeriod(msgs, dc);
            if (period.IsEmpty())
                return aag.GetNextRegistrationPeriod(msgs, dc);

            return period;
        }

        public static RegistrationPeriod GetNextRegistrationPeriod(this AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = aag.GetAccount(msgs, dc).ConvertToAccountTimeZone(DateTime.UtcNow);

            return aag.GetRegistrationPeriods(dc: dc)
                .Where(x => now < x.StartDateTime.Value)
                .OrderBy(x => x.StartDateTime.Value)
                .FirstOrDefault();
        }

        public static RegistrationPeriod GetCurrentRegistrationPeriod(this AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = aag.GetAccount(msgs, dc).ConvertToAccountTimeZone(DateTime.UtcNow);

            return aag.GetRegistrationPeriods(dc: dc)
                .Where(x => x.StartDateTime.Value <= now && now <= x.EndDateTime.Value)
                .FirstOrDefault();
        }

        public static Account GetAccount(this AccountActivityGroup aag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return Account.Load(aag.AccountId.Value, msgs, dc);
        }

        public static string GetRegistrationUrl(this AccountActivityGroup aag, bool isAfterSchool, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var districtName = aag.GetActivityGroup(msgs, dc).GetAccount(msgs,dc).UniqueName.ToLower();


            return $"{Utilities.BaseUrl(isAfterSchool)}/{aag.GetAccount(msgs, dc).UniqueName.ToLower()}/league//{aag.UniqueName}";
        

            return "";
        }
    }
}
