using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantActivityGroupService
    {
        public static string GetInviteUrl(this RegistrantActivityGroup rag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            var url = rag.GetGroup(msgs, dc).BrowseUrl;
            url += (url.Contains("?") ? "&" : "?") + $"invite={rag.Uid.ToString()}";
            return url;
        }

        public static string GetDeclineUrl(this RegistrantActivityGroup rag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            return rag.GetInviteUrl(msgs, dc) + "&decline=1";
        }

        public static ActivityGroup GetGroup(this RegistrantActivityGroup rag, SystemMessages msgs = null, DatabaseConnection dc = null)
        {
            if (rag.ActivityGroupId.HasValue)
                return ActivityGroup.Load(rag.ActivityGroupId.Value, msgs, dc);

            return null;
        }

        public static void DeclineInvite(this RegistrantActivityGroup rag, int loggedInUserId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    // TODO: Fix
                    rag.NumberOfSeatsDeclined = rag.NumberOfInvitationSeats;
                    rag.NumberOfInvitationSeats = 0;
                    rag.LastModifiedBy = loggedInUserId;
                    rag.LastModifiedDate = DateTime.UtcNow;
                    amsgs.AddRange(rag.Save(adc));

                    RegistrantActivity.LoadByRegistrantId(rag.RegistrantId.Value, amsgs, adc)
                        .ForEach(ra => {
                            ra.ChangeSeatAssignment(
                                RegistrationAssignmentType.Declined,
                                new WebUser() { ID = loggedInUserId },
                                amsgs,
                                adc);
                        });
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    var body = $"<p>Hello,</p><p>{rag.RegistrantName.IfEmpty(rag.RegistrantEmail)} has opted to decline the invitation to register for {rag.ActivityGroupName}</p>{Helpers.Email.GetAppSignature(true, "declined_invite_notice")}";
                    var account = rag.GetGroup(msgs, dc).GetAccount(msgs, dc);

                    SendGridHelper.SendMail(
                        fromEmail: Helpers.Email.SendGridNoReplyEmailAddress(),
                        toEmail: new SendGrid.Helpers.Mail.EmailAddress(account.Email, account.Name),
                        subject: string.Format("An invitation was declined for {0}", rag.ActivityGroupName),
                        body: body,
                        emailTemplateId: SendGridHelper.GetGenericTemplateId(account.IsAfterSchool)
                    );

                    msgs.Add("Your invitation has been declined", MessageType.Success);

                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
                dc: dc
            );
        }
    }
}
