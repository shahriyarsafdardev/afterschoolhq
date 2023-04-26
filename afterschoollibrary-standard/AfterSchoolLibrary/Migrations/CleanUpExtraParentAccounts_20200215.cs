using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Linq;

namespace AfterSchoolLibrary.Migrations
{
    public class CleanUpExtraParentAccounts_20200215
    {
        public static void Do(SystemMessages msgs, DatabaseConnection dc)
        {
            Migration.Run
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var users = WebUser.Load(msgs: amsgs, dc: adc);

                    users.ForEach(user => {

                        // get all registrant awus for the user
                        var awus = AccountWebUser
                                    .LoadByWebUserIdByIsDeleted(user.ID.Value, false, amsgs, adc)
                                    .Where(awu => awu.AccountAccountTypeId == AccountType.Registrant)
                                    .OrderBy(awu => awu.ID)
                                    .ToList();

                        // if there is more than one registrant account for the user then there's a duplicate account
                        if (awus.Count > 1)
                        {
                            var firstMasterRegistrant = user.GetAllRegistrants(amsgs, adc).FirstOrDefault(registrant => registrant.IsMaster && registrant.IsParentOrGuardian);

                            if (firstMasterRegistrant.IsNotEmpty())
                            {
                                var firstAccount = awus.First().GetAccount(amsgs, adc);

                                if (firstMasterRegistrant.AccountId != firstAccount.ID)
                                {
                                    firstMasterRegistrant.AccountId = firstAccount.ID;
                                    amsgs.AddRange(firstMasterRegistrant.Save(adc));
                                }

                                // delete all except for the first record
                                awus.Where(awu => awu.ID != awus.First().ID).ToList().ForEach(awu =>
                                {
                                    var account = awu.GetAccount(amsgs, adc);

                                    // give me all registrants for this account
                                    var allregistrants = Registrant.LoadByAccountIdByIsDeleted(awu.AccountId.Value, false, amsgs, adc)
                                                            .Where(registrant =>
                                                                    registrant.IsMaster &&
                                                                    registrant.ID != firstMasterRegistrant.ID)
                                                            .ToList();

                                    // give me only parent registrants that are duplicates of the first parent registrant
                                    var registrants = allregistrants
                                                        .Where(registrant =>
                                                                    registrant.IsParentOrGuardian &&
                                                                    registrant.FirstName.IsSameAs(firstMasterRegistrant.FirstName))
                                                        .ToList();

                                    // give me all other registrants, parents & children
                                    var otherRegistrants = allregistrants
                                                            .Where(registrant => !registrant.FirstName.IsSameAs(firstMasterRegistrant.FirstName) || !registrant.IsParentOrGuardian)
                                                            .ToList();

                                    // delete all duplicate registrants
                                    if (registrants.Any())
                                    {
                                        var lastCompletedRegistrant = registrants
                                                                        .Where(registrant => registrant.ProfileStatusId == RegistrantProfileStatus.Completed)
                                                                        .OrderBy(registrant => registrant.CreateDate)
                                                                        .LastOrDefault();

                                        if (lastCompletedRegistrant.IsNotEmpty() && firstMasterRegistrant.LastModifiedDate.Value.IsBefore(lastCompletedRegistrant.LastModifiedDate.Value))
                                        {
                                            lastCompletedRegistrant.CopyAnswersTo(firstMasterRegistrant, null, null, user, amsgs, adc);
                                            firstMasterRegistrant.UpdateProfileStatus(RegistrantProfileStatus.Completed, user, amsgs, adc);
                                        }

                                        registrants.ForEach(registrant =>
                                        {
                                        // delete registrant web user connections
                                        var rwu = RegistrantWebUser.LoadByRegistrantIdByWebUserId(registrant.ID.Value, user.ID.Value, amsgs, adc);

                                            if (rwu.IsNotEmpty())
                                                rwu.MarkAsDeleted(amsgs, adc);

                                            registrant.MarkAsDeleted(user, amsgs, adc);
                                        });
                                    }

                                    // move other registrants to original account
                                    if (otherRegistrants.Any())
                                        otherRegistrants.ForEach(registrant =>
                                        {
                                            registrant.AccountId = firstAccount.ID.Value;
                                            amsgs.AddRange(registrant.Save(adc));
                                        });

                                    // deactivate the awu
                                    awu.MarkAsDeleted(user, amsgs, adc);
                                    account.MarkAsDeleted(user, amsgs, adc);

                                });
                            }
                        }
                    });

                },
                uniqueIdenfier: "CleanUpExtraParentAccounts_20200215",
                msgs: msgs,
                dc: dc
            );
        }
    }
}
