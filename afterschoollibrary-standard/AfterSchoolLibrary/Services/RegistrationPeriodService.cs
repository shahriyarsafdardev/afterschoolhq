using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrationPeriodService
    {
        public static bool IsOpen(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (period.IsNotEmpty())
            {
                var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
                return now.IsOnOrAfter(period.StartDateTime.Value) && now.IsOnOrBefore(period.EndDateTime.Value);
            }

            return false;
        }

        public static bool IsClosed(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (period.IsNotEmpty())
                return !period.IsOpen(ownerAccount, msgs, dc);

            return true;
        }

        public static bool IsRecentlyClosed(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (period.IsNotEmpty())
            {
                var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
                return now.IsOnOrBefore(period.EndDateTime.Value.AddWeeks(8));
            }

            return true;
        }

        public static bool WillOpen(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (period.IsNotEmpty())
            {
                var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
                return now.IsBefore(period.StartDateTime.Value);
            }

            return false;
        }

        public static TimeSpan WillOpenIn(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
            return period.StartDateTime.Value - now;
        }

        public static TimeSpan WillCloseIn(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
            return period.EndDateTime.Value - now;
        }

        public static string GetStatus(this RegistrationPeriod period, DatabaseConnection dc = null)
        {
            if (period.WillOpen(dc: dc))
                return "Not Open";
            else if (period.IsOpen(dc: dc))
                return "Open";
            else if (period.IsClosed(dc: dc))
                return "Closed";

            return "N/A";
        }

        public static SystemMessages AddActivity(this RegistrationPeriod registrationPeriod, Activity activity, bool isActive, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var item = ActivityRegistrationPeriod.LoadByRegistrationPeriodIdByActivityId(registrationPeriod.ID.Value, activity.ID.Value, dc: dc).FirstOrDefault();

            if ((item.IsEmpty() || (item.IsNotEmpty() && !item.IsActive)) && isActive)
            {
                activity.GetActivityRegistrationPeriods(msgs, dc).ForEach(period => {
                    period.IsActive = false;
                    period.LastModifiedDate = DateTime.UtcNow;
                    period.LastModifiedBy = loggedInUserId;
                    msgs.AddRange(period.Save(dc: dc));
                });
            }

            //if record doesn't already exist
            if (item.IsEmpty())
            {
                var activityperiod = new ActivityRegistrationPeriod
                {
                    RegistrationPeriodId = registrationPeriod.ID,
                    ActivityId = activity.ID,
                    IsActive = isActive,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    CreatedBy = loggedInUserId,
                    LastModifiedBy = loggedInUserId
                };
                msgs.AddRange(activityperiod.Save(dc: dc));
            }
            else
            {
                item.IsActive = isActive;
                msgs.AddRange(item.Save(dc: dc));
            }

            return msgs;
        }

        public static Account GetAccount(this RegistrationPeriod registrationPeriod, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (registrationPeriod.AccountId.HasValue)
                return Account.Load(registrationPeriod.AccountId.Value, msgs, dc);

            var arps = registrationPeriod.GetActivityRegistrationPeriods(msgs, dc).Where(x => x.IsActive).ToList();
            if (arps.Any())
                return arps.First().GetActivity(msgs, dc).GetAccount(msgs, dc);

            return null;
        }

        public static bool IsFuture(this RegistrationPeriod period, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (period.IsNotEmpty())
            {
                return period.StartDateTime.Value >= DateTime.Now;
            }

            return false;
        }

        public static void CreateOrUpdate(WebUser currentUser, Account account, ActivityGroup group, string period, string opendate, string closedate, string opentime, string closetime, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            Extensions.ValidateStartAndEndDateWithTime(opendate, opentime, closedate, closetime, msgs);

            if (msgs.HasNoErrors())
            {
                var regperiod = new RegistrationPeriod();
                if (period.IsInteger())
                    regperiod = account.GetRegistrationPeriod(period.ToInteger(), msgs);
                else
                {
                    regperiod.CreatedBy = currentUser.ID.Value;
                    regperiod.CreateDate = DateTime.UtcNow;
                    regperiod.ActivityGroupId = group.ID;
                    regperiod.AccountId = account.ID;
                }

                regperiod.StartDateTime = string.Format("{0} {1}", opendate, opentime).ToDateTime();
                regperiod.EndDateTime = string.Format("{0} {1}", closedate, closetime).ToDateTime();
                regperiod.LastModifiedBy = currentUser.ID.Value;
                regperiod.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(regperiod.Save());
            }
        }
    }
}
