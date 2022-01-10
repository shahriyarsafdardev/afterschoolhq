using System.Collections.Generic;
using System.Linq;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using System.Collections.Specialized;
using BasicLibrary.DataAccess;

namespace AfterSchool.Models
{
    public static class Extensions
    {
        public static FormQuestionViewModel ToFormQuestionViewModel(this FormQuestion question, NameValueCollection values)
        {
            return new FormQuestionViewModel(question, values);
        }

        public static FormQuestionViewModel ToFormQuestionViewModel(this FormQuestion question)
        {
            return new FormQuestionViewModel(question);
        }
        
        public static AddressViewModel ToAddressViewModel(this PaymentsEntityViewModel paymentsEntityVM)
        {
            return new AddressViewModel()
            {
                Address1 = paymentsEntityVM.Address1,
                Address2 = paymentsEntityVM.Address2,
                City = paymentsEntityVM.City,
                State = paymentsEntityVM.State,
                PostalCode = paymentsEntityVM.PostalCode
            };
        }

        public static AddressViewModel ToAddressViewModel(this CheckPaymentAccount checkPaymentAccount)
        {
            return new AddressViewModel()
            {
                Address1 = checkPaymentAccount.Address1,
                Address2 = checkPaymentAccount.Address2,
                City = checkPaymentAccount.City,
                State = checkPaymentAccount.State,
                PostalCode = checkPaymentAccount.PostalCode
            };
        }

        public static AddressViewModel ToAddressViewModel(this Activity activity)
        {
            var location = activity.GetLocation();
            if (location.IsNotEmpty())
                return new AddressViewModel()
                {
                    Address1 = location.Address1,
                    Address2 = location.Address2,
                    City = location.City,
                    State = location.State,
                    PostalCode = location.PostalCode
                };

            return new AddressViewModel();
        }

        public static AddressViewModel ToAddressViewModel(this Location location)
        {
            return new AddressViewModel()
            {
                Address1 = location.Address1,
                Address2 = location.Address2,
                City = location.City,
                State = location.State,
                PostalCode = location.PostalCode
            };
        }

        public static AddressViewModel ToAddressViewModel(this Account account)
        {
            return new AddressViewModel()
            {
                Address1 = account.Address1,
                Address2 = account.Address2,
                City = account.City,
                State = account.State,
                PostalCode = account.PostalCode
            };
        }
        public static string ToFullDescription(this Activity activity)
        {
            var text = activity.Name;

            if (activity.GetDaysOfTheWeek().Count > 0)
                text += $" - {activity.GetDaysOfTheWeek().Select(x => x.ToString().MaxChars(2)).ToStringList(", ")}";

            if (activity.GradeLevelStart.HasValue && activity.GradeLevelEnd.HasValue)
                text += $" - Grades {GradeLevel.Load(activity.GradeLevelStart.Value).Abbreviation} to {GradeLevel.Load(activity.GradeLevelEnd.Value).Abbreviation}";

            if (activity.AgeStart.HasValue && activity.AgeEnd.HasValue)
                text += $" - Ages {activity.AgeStart} to {activity.AgeEnd}";

            return text;
        }

        public static WebUser ToWebUser(this SignUpIndexViewModel signUpIndexViewModel)
        {
            return new WebUser()
            {
                FirstName = signUpIndexViewModel.FirstName,
                LastName = signUpIndexViewModel.LastName,
                Email = signUpIndexViewModel.Email,
            };
        }

        public static string ToString(this ActivitySchedule schedule)
        {
            return "";
        }

        public static SystemMessages GetItemsRequiredToPublish(this Activity activity, SystemMessages msg = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var setupMsgs = new SystemMessages();
            var scheduleMessages = new SystemMessages();
            var detailMsgs = new SystemMessages();
            var contactMsgs = new SystemMessages();
            var locationMsgs = new SystemMessages();
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
                setupMsgs.Insert(0, new SystemMessage($"<a href=\"/org/activity/editsetup/{activity.Uid}\" class=\"publish-error-title\">Settings</a>", MessageType.UserError));
                msgs.AddRange(setupMsgs);
            }

           
            //if (activity.HasSubscriptionPlan && !activity.HasSchedule && !activity.GetSchedule(msgs).IsOngoing)
            //    scheduleMessages.Add("Schedule Meeting Dates/Times", MessageType.UserError);

            //if (scheduleMessages.HasErrors())
            //{
            //    scheduleMessages.Insert(0, new SystemMessage($"<a href=\"/org/activity/editschedule/{activity.Uid}\" class=\"publish-error-title\">Schedule</a>", MessageType.UserError));
            //    msgs.AddRange(scheduleMessages);
            //}

            if (BasicLibrary.Utilities.RemoveHTMLAndEntities(activity.Details).IsBlank())
                detailMsgs.Add("Description", MessageType.UserError);

            if (detailMsgs.HasErrors())
            {
                detailMsgs.Insert(0, new SystemMessage($"<a href=\"/org/activity/editdetails/{activity.Uid}\" class=\"publish-error-title\">About Your Activity</a>", MessageType.UserError));
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
                contactMsgs.Insert(0, new SystemMessage($"<a href=\"/org/activity/editcontact/{activity.Uid}\" class=\"publish-error-title\">Contact Information</a>", MessageType.UserError));
                msgs.AddRange(contactMsgs);
            }

            //if (activity.ActivityTypeId.NotIn(new List<ActivityType>() { ActivityType.FieldTrip, ActivityType.PermissionSlip }))
            //{
            //    if (!activity.LocationId.HasValue)
            //        locationMsgs.Add("Activity Location", MessageType.UserError);

            //    if (locationMsgs.HasErrors())
            //    {
            //        locationMsgs.Insert(0, new SystemMessage($"<a href=\"/org/activity/editlocation/{activity.Uid}\" class=\"publish-error-title\">Location</a>", MessageType.UserError));
            //        msgs.AddRange(locationMsgs);
            //    }
            //}

            if (paymentMsgs.HasErrors())
            {
                paymentMsgs.Insert(0, new SystemMessage($"<a href=\"/org/activity/editcost/{activity.Uid}\" class=\"publish-error-title\">Payment</a>", MessageType.UserError));
                msgs.AddRange(paymentMsgs);
            }

            if (!activity.ActivityGroupId.HasValue)
            {
                if (activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail).IsEmpty() ||
                    activity.GetActivityProperty(AccountPropertyType.ApplicationThankYouEmail).Value.IsBlank())
                {
                    confirmationMsgs.Add(AccountPropertyType.ApplicationThankYouEmail.ToString().SplitByUpperCase(), MessageType.UserError);
                }

                if (activity.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail).IsEmpty() ||
                    activity.GetActivityProperty(AccountPropertyType.RegistrationThankYouEmail).Value.IsBlank())
                {
                    confirmationMsgs.Add(AccountPropertyType.RegistrationThankYouEmail.ToString().SplitByUpperCase(), MessageType.UserError);
                }
            }

            if (confirmationMsgs.HasErrors())
            {
                confirmationMsgs.Insert(0, new SystemMessage($"<a href=\"/org/activity/editconfirmation/{activity.Uid}\" class=\"publish-error-title\">Confirmation Message</a>", MessageType.UserError));
                msgs.AddRange(confirmationMsgs);
            }

            if (activity.IsArchived)
                archiveMsgs.Add($"<a href=\"/org/activity/restore/{activity.Uid}\">Restore {activity.Name}</a>. It is currently archived", MessageType.Error);

            if (activity.IsGroupArchived)
                archiveMsgs.Add($"<a href=\"/org/group/restore/{activity.GetActivityGroup().Uid}\">Restore {activity.GetActivityGroup().Name}</a>. It is currently archived", MessageType.Error);

            msgs.AddRange(activity.IsPaymentMethodAddedForExternalAccount(SessionVariables.CurrentAccount, msg, dc));

            if (archiveMsgs.HasErrors())
            {
                archiveMsgs.Insert(0, new SystemMessage($"<span class=\"publish-error-title\">Archive Errors<span>", MessageType.UserError));
                msgs.AddRange(archiveMsgs);
            }

            return msgs;
        }

        public static SystemMessages IsPaymentMethodAddedForExternalAccount(this Activity activity, Account account, SystemMessages msg = null, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if (!activity.GetParticipationObject(msg, dc).IsRegistrationFree && activity.CostPerRegistrant > 0 && !account.CanReceivePaymentDeposits(msg, dc) && !activity.GetParticipationObject(msg, dc).IsExternalToAccount(account))
            {
                var addPaymentMsgs = new SystemMessages();
                addPaymentMsgs.Add($"<a href=\"/org/account/paymentsoptions\">Add Payment Method</a>. To accept payments", MessageType.Error);
                msgs.AddRange(addPaymentMsgs);
            }

            return msgs;
        }

        public static SystemMessages GetItemsRequiredToPublish(this ActivityGroup group)
        {
            var msgs = new SystemMessages();

            if (group.IsArchived)
                msgs.Add($"<a href=\"/org/group/restore/{group.Uid}\">Restore {group.Name}</a>. It is currently archived", MessageType.Error);

            if (!group.GetRegistrationPeriods().Any())
                msgs.Add($"Please <a href=\"/org/group/periods/{group.Uid}\">add a Sign Up Period</a>", MessageType.Error);

            if (group.GetActivities().Where(x => !x.ActivityScheduleId.HasValue).Any())
                msgs.Add($"Please ensure all of your {UILabel.ActivityLabel(ActivityType.Activity, true).ToLower()} <a href=\"/org/group/schedules/{group.Uid}\">have schedules</a>", MessageType.Error);

            return msgs;
        }

        public static SystemMessages GetItemsRequiredToPublish(this AccountActivityGroup aag)
        {
            var msgs = new SystemMessages();

            if (!aag.HasRegistrationPeriods())
                msgs.Add($"<a href=\"/org/league/periods/{aag.ActivityGroupUid}\">Create at least one {UILabel.RegistrationPeriodLabel()}</a> in order to publish", MessageType.UserError);

            if (aag.FormId.IsEmpty())
                msgs.Add($"<a href=\"/org/league/edit/{aag.ActivityGroupUid}\">Select a registration form</a> in order to publish", MessageType.UserError);
            if (aag.ContactName.IsEmpty())
                msgs.Add($"<a href=\"/org/league/edit/{aag.ActivityGroupUid}\">Contact Name</a> is required in order to publish", MessageType.UserError);
            if (aag.ContactPhone.IsEmpty())
                msgs.Add($"<a href=\"/org/league/edit/{aag.ActivityGroupUid}\">Contact Phone</a> is required in order to publish", MessageType.UserError);
            if (aag.ContactEmail.IsEmpty())
                msgs.Add($"<a href=\"/org/league/edit/{aag.ActivityGroupUid}\">Contact Email</a> is required in order to publish", MessageType.UserError);

            return msgs;
        }

        public static string ToShortString(this List<GradeLevel> grades)
        {
            if (grades.Count > 1)
            {
                return grades.OrderBy(x => x.ID.Value).First().Abbreviation + " - " + grades.OrderBy(x => x.ID.Value).Last().Abbreviation + " Grade";
            }
            else
            {
                return grades.First().Abbreviation + " Grade";
            }
        }
    }
}