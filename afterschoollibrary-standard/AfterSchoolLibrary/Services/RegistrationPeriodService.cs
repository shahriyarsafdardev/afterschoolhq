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
            var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
            return period.StartDateTime.Value <= now && period.EndDateTime.Value >= now;
        }

        public static bool IsClosed(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return !period.IsOpen(ownerAccount, msgs, dc);
            //var now = this.GetAccount(msgs, dc).ConvertToAccountTimeZone(DateTime.UtcNow);
            //return this.EndDateTime.Value < now;
        }

        public static bool WillOpen(this RegistrationPeriod period, Account ownerAccount = null, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var now = (ownerAccount ?? period.GetAccount(msgs, dc)).ConvertToAccountTimeZone(DateTime.UtcNow);
            return period.StartDateTime.Value > now;
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

        public static SystemMessages NotifyRegistrants(int id, string emailmessage, List<int> registrationIds = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var regPeriod = RegistrationPeriod.Load(id, dc: dc);
            var account = regPeriod.GetAccount(dc: dc);
            var registrations = new List<Registration>();

            if (registrationIds.IsEmpty())
                registrations = regPeriod.GetGroup(dc: dc).GetRegistrations(dc: dc);
            else
                registrations = Registration.LoadList(registrationIds, dc: dc);

            if (registrations.Any())
            {
                var subject = "";
                var body = "";

                if (regPeriod.IsOpen())
                {
                    subject = $"Registration for {account.Name} is open again";
                    body = "<p>{0},</p>{3}<p>Please click the following link to add to your registration: {1}</p>{2}";
                }
                else if (regPeriod.WillOpen())
                {
                    subject = $"Registration for {account.Name} will open again soon";
                    body = "<p>{0},</p>{3}<p>Registration will open " + regPeriod.StartDateTime.ToShortDateString() + ". Please click the following link to add to your registration: {1}</p>{2}";
                }

                foreach (var reg in registrations)
                {
                    SendGridHelper.SendMail(
                        fromEmail: new SendGrid.Helpers.Mail.EmailAddress(
                            Helpers.Email.AppNoReplyEmailAddress,
                            $"{account.Name} via {Helpers.Utilities.AppName(account.IsAfterSchool)}"),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(reg.RegistrantEmail, reg.Name),
                        subject: subject,
                        body: string.Format(
                            body,
                            reg.RegistrantFirstName,
                            reg.AddToRegistrationUrl,
                            Helpers.Email.GetAppSignature(account.IsAfterSchool, "invite_enrollees"),
                            emailmessage),
                        replyTo: new SendGrid.Helpers.Mail.EmailAddress(account.Email, account.Name),
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );
                }
            }

            return msgs;
        }

        public static SystemMessages AddActivity(this RegistrationPeriod registrationPeriod, Activity activity, bool isActive, int loggedInUserId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var item = ActivityRegistrationPeriod.LoadByRegistrationPeriodIdByActivityId(registrationPeriod.ID.Value, activity.ID.Value, dc: dc);

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
    }
}
