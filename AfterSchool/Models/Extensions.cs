using System;
using System.Collections.Specialized;
using System.Linq;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;

namespace AfterSchool.Models
{
    public static class Extensions
    {
        public static FormQuestionViewModel ToFormQuestionViewModel(this FormQuestion question,
            NameValueCollection values)
        {
            return new FormQuestionViewModel(question, values);
        }

        public static FormQuestionViewModel ToFormQuestionViewModel(this FormQuestion question)
        {
            return new FormQuestionViewModel(question);
        }

        public static AddressViewModel ToAddressViewModel(this PaymentsEntityViewModel paymentsEntityViewModel)
        {
            return new AddressViewModel
            {
                Address1 = paymentsEntityViewModel.Address1,
                Address2 = paymentsEntityViewModel.Address2,
                City = paymentsEntityViewModel.City,
                State = paymentsEntityViewModel.State,
                PostalCode = paymentsEntityViewModel.PostalCode,
            };
        }

        public static AddressViewModel ToAddressViewModel(this CheckPaymentAccount checkPaymentAccount)
        {
            return new AddressViewModel
            {
                Address1 = checkPaymentAccount.Address1,
                Address2 = checkPaymentAccount.Address2,
                City = checkPaymentAccount.City,
                State = checkPaymentAccount.State,
                PostalCode = checkPaymentAccount.PostalCode,
            };
        }

        public static AddressViewModel ToAddressViewModel(this Location location)
        {
            return new AddressViewModel
            {
                Address1 = location.Address1,
                Address2 = location.Address2,
                City = location.City,
                State = location.State,
                PostalCode = location.PostalCode,
            };
        }

        public static string ToFullDescription(this Activity activity)
        {
            var text = activity.Name;

            if (activity.GetDaysOfTheWeek().Count > 0)
                text += $" - {activity.GetDaysOfTheWeek().Select(x => x.ToString().MaxChars(2)).ToStringList(", ")}";

            if (activity.GradeLevelStart.HasValue && activity.GradeLevelEnd.HasValue)
                text +=
                    $" - Grades {GradeLevel.Load(activity.GradeLevelStart.Value).Abbreviation} to {GradeLevel.Load(activity.GradeLevelEnd.Value).Abbreviation}";

            if (activity.AgeStart.HasValue && activity.AgeEnd.HasValue)
                text += $" - Ages {activity.AgeStart} to {activity.AgeEnd}";

            return text;
        }

        public static WebUser ToWebUser(this SignUpIndexViewModel signUpIndexViewModel)
        {
            return new WebUser
            {
                FirstName = signUpIndexViewModel.FirstName,
                LastName = signUpIndexViewModel.LastName,
                Email = signUpIndexViewModel.Email,
            };
        }

        public static string ToString(this ActivitySchedule _)
        {
            return "";
        }

        public static SystemMessages GetItemsRequiredToPublish(this Activity activity, SystemMessages msg = null,
            DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var setupMsgs = new SystemMessages();
            var detailMsgs = new SystemMessages();
            var contactMsgs = new SystemMessages();
            var paymentMsgs = new SystemMessages();
            var confirmationMsgs = new SystemMessages();
            var archiveMsgs = new SystemMessages();

            if (activity.Name.IsEmpty())
                setupMsgs.Add("Activity Name", MessageType.UserError);

            if (!activity.SeatsAvailable.HasValue)
                setupMsgs.Add("Seats Available", MessageType.UserError);

            if (!activity.ActivityGroupId.HasValue)
            {
                if (activity.GetActiveRegistrationPeriod().IsEmpty())
                    setupMsgs.Add(UILabel.RegistrationPeriodLabel(), MessageType.UserError);

                if (activity.GetActiveActivityForm().IsEmpty())
                    setupMsgs.Add("Registration Form", MessageType.UserError);
            }

            if (setupMsgs.HasErrors())
            {
                setupMsgs.Insert(0,
                    new SystemMessage(
                        $"<a href=\"/org/activity/editsetup/{activity.Uid}\" class=\"publish-error-title\">Settings</a>",
                        MessageType.UserError));
                msgs.AddRange(setupMsgs);
            }

            if (BasicLibrary.Utilities.RemoveHTMLAndEntities(activity.Details).IsBlank())
                detailMsgs.Add("Description", MessageType.UserError);

            if (detailMsgs.HasErrors())
            {
                detailMsgs.Insert(0,
                    new SystemMessage(
                        $"<a href=\"/org/activity/editdetails/{activity.Uid}\" class=\"publish-error-title\">About Your Activity</a>",
                        MessageType.UserError));
                msgs.AddRange(detailMsgs);
            }

            if (activity.ContactName.IsBlank())
                contactMsgs.Add("Contact Name", MessageType.UserError);

            if (activity.ContactPhone.IsBlank())
                contactMsgs.Add("Contact Phone", MessageType.UserError);

            if (activity.ContactEmail.IsBlank())
                contactMsgs.Add("Contact Email", MessageType.UserError);

            if (contactMsgs.HasErrors())
            {
                contactMsgs.Insert(0,
                    new SystemMessage(
                        $"<a href=\"/org/activity/editcontact/{activity.Uid}\" class=\"publish-error-title\">Contact Information</a>",
                        MessageType.UserError));
                msgs.AddRange(contactMsgs);
            }

            if (paymentMsgs.HasErrors())
            {
                paymentMsgs.Insert(0,
                    new SystemMessage(
                        $"<a href=\"/org/activity/editcost/{activity.Uid}\" class=\"publish-error-title\">Payment</a>",
                        MessageType.UserError));
                msgs.AddRange(paymentMsgs);
            }

            if (!activity.ActivityGroupId.HasValue)
            {
                if (activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail).IsEmpty() ||
                    activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail).Value.IsBlank())
                    confirmationMsgs.Add(AccountPropertyType.ApplicationThankYouEmail.ToString().SplitByUpperCase(),
                        MessageType.UserError);

                if (activity.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail).IsEmpty() ||
                    activity.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail).Value.IsBlank())
                    confirmationMsgs.Add(AccountPropertyType.RegistrationThankYouEmail.ToString().SplitByUpperCase(),
                        MessageType.UserError);
            }

            if (confirmationMsgs.HasErrors())
            {
                confirmationMsgs.Insert(0,
                    new SystemMessage(
                        $"<a href=\"/org/activity/editconfirmation/{activity.Uid}\" class=\"publish-error-title\">Confirmation Message</a>",
                        MessageType.UserError));
                msgs.AddRange(confirmationMsgs);
            }

            if (activity.IsArchived)
                archiveMsgs.Add(
                    $"<a href=\"/org/activity/restore/{activity.Uid}\">Restore {activity.Name}</a>. It is currently archived",
                    MessageType.Error);

            if (activity.IsGroupArchived)
                archiveMsgs.Add(
                    $"<a href=\"/org/group/restore/{activity.GetActivityGroup().Uid}\">Restore {activity.GetActivityGroup().Name}</a>. It is currently archived",
                    MessageType.Error);

            var account = activity.GetAccount(msgs, dc);

            if (activity.CostPerRegistrant <= 0 && !account.CanPublishFreeActivity(msgs, dc))
            {
                var freeActivityLimitPackage =
                    PackageService.GetPackageWithFeatureLowestPrice(FeatureType.FreeActivityLimit, SessionVariables.AccountPackage);
                var editCostLink =
                    $"<a href=\"/org/activity/editcost/{activity.Uid}\">update pricing and payment for this activity</a>";
                var upgradePackageLink =
                    $"<a href=\"#\" onclick=\"setFeatureIdAndLoadUpgradePage('{freeActivityLimitPackage.ID}')\">upgrade your account for more free activities</a>";
               
                msgs.Add(
                    $"<p class=\"no-bullet\">You've reached your limit on free activities, please take one of the following actions:</p><ul>" +
                    $"<li>{editCostLink}</li><li>{upgradePackageLink}</li>" +
                    $"  </ul>",
                    MessageType.UserError);
            }

            msgs.AddRange(activity.IsPaymentMethodAddedForExternalAccount(SessionVariables.CurrentAccount, msg, dc));

            if (activity.ActivityTypeId == ActivityType.VolunteerOpportunity && !activity.HasSchedule)
            {
                msgs.Add(new SystemMessage
                (
                    $"<a href=\"/org/activity/editschedule/{activity.Uid}\">Add a Schedule</a>",
                    MessageType.Error
                ));
            }
            
            if (archiveMsgs.HasErrors())
            {
                archiveMsgs.Insert(0,
                    new SystemMessage("<span class=\"publish-error-title\">Archive Errors<span>",
                        MessageType.UserError));
                msgs.AddRange(archiveMsgs);
            }

            return msgs;
        }

        public static SystemMessages IsPaymentMethodAddedForExternalAccount(this Activity activity, Account account,
            SystemMessages msg = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (!activity.GetParticipationObject(msg, dc).IsRegistrationFree && activity.CostPerRegistrant > 0 &&
                !account.CanReceivePaymentDeposits(msg, dc) &&
                !activity.GetParticipationObject(msg, dc).IsExternalToAccount(account))
            {
                var addPaymentMsgs = new SystemMessages
                {
                    {
                        "<a href=\"/org/account/paymentsoptions\">Add Payment Method</a>. To accept payments",
                        MessageType.Error
                    },
                };
                msgs.AddRange(addPaymentMsgs);
            }

            return msgs;
        }

        public static SystemMessages GetItemsRequiredToPublish(this ActivityGroup group)
        {
            var msgs = new SystemMessages();

            if (group.IsArchived)
                msgs.Add(
                    $"<a href=\"/org/group/restore/{group.Uid}\">Restore {group.Name}</a>. It is currently archived",
                    MessageType.Error);

            if (!group.GetRegistrationPeriods().Any())
                msgs.Add($"Please <a href=\"/org/group/periods/{group.Uid}\">add a Sign Up Period</a>",
                    MessageType.Error);

            if (group.GetActivities().Any(x => !x.ActivityScheduleId.HasValue))
                msgs.Add(
                    $"Please ensure all of your {UILabel.ActivityLabel(ActivityType.Activity, true).ToLower()} <a href=\"/org/group/schedules/{group.Uid}\">have schedules</a>",
                    MessageType.Error);

            return msgs;
        }
    }
}