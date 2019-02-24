using System;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;

namespace AfterSchoolLibrary.Services
{
    public static class PasswordResetTokenService
    {
        public static SystemMessages Use(this PasswordResetToken passwordResetToken, DatabaseConnection dc = null)
        {
            return PasswordResetTokenService.UseToken(passwordResetToken.Uid, dc: dc);
        }

        public static SystemMessages UseToken(Guid uid, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var token = PasswordResetToken.LoadValidByUid(uid, dc: adc);
                    token.Used = true;
                    token.LastModifiedBy = token.WebUserId;
                    token.LastModifiedDate = DateTime.UtcNow;
                    amsgs.AddRange(token.Save(adc));

                    amsgs.AddRange(token.GetWebUser(adc).MarkAsVerified(adc));
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                },
                onFailure: (SystemMessages fmsgs) =>
                {
                    msgs.AddRange(fmsgs);
                },
                dc: dc
            );

            return msgs;
        }
    }
}
