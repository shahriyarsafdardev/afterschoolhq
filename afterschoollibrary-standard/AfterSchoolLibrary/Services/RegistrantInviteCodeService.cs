using AfterSchoolLibrary.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AfterSchoolLibrary.Services
{
    public static class RegistrantInviteCodeService
    {
        public static RegistrantInviteCode Create(int surveyResponseId, int registrantId, int userId, SystemMessages msgs, DatabaseConnection dc = null)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();

            RegistrantInviteCode registrantInviteCode = null;

            Database.ExecuteTransaction
            (
               action: (SystemMessages amsgs, DatabaseConnection adc) =>
               {
                   registrantInviteCode = new RegistrantInviteCode()
                   {
                       RegistrantId = registrantId,
                       SurveyResponseId = surveyResponseId,
                       CreatedBy = userId,
                       CreateDate = DateTime.Now,
                       LastModifiedDate = DateTime.Now,
                       LastModifiedBy = userId,
                       Uid = Guid.NewGuid(),
                       IsDeleted = false,
                       ExpiryDate = DateTime.Now.AddDays(7),
                       Code = GenerateRandomCode()
                   };

                   msgs.AddRange(registrantInviteCode.Save(dc));
               },
               onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
               onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
               onException: (SystemMessages emsgs) => Helpers.Utilities.SystemErrorLog(emsgs),
               dc: dc
            );

            return registrantInviteCode;
        }

        private static string GenerateRandomCode()
        {
            var condition = true;
            var newCode = "";
            while (condition)
            {
                newCode = Guid.NewGuid().ToString().Substring(0, 6);

                var checkifCodeExists = RegistrantInviteCode.LoadByIsDeletedByCode(false, newCode);

                if (!checkifCodeExists.IsNotEmpty())
                    condition = false;
            }

            return newCode;
        }

    }
}
