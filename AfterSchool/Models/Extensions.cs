using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Collections.Specialized;

namespace AfterSchool.Models
{
    public static class Extensions
    {
        public static string Link(this RegistrantActivity ra)
        {
            return string.Format("<a href=\"/org/activity/summary/{0}\">{1}</a>", ra.ActivityId, ra.ActivityName);
        }
        public static string Link(this Activity a)
        {
            return string.Format("<a href=\"/org/activity/summary/{0}\">{1}</a>", a.ID, a.Name);
        }

        public static FormQuestionViewModel ToFormQuestionViewModel(this FormQuestion question, NameValueCollection values)
        {
            return new FormQuestionViewModel(question, values);
        }

        public static FormQuestionViewModel ToFormQuestionViewModel(this FormQuestion question)
        {
            return new FormQuestionViewModel(question);
        }

        public static AddressViewModel ToAddressViewModel(this AfterSchoolLibrary.StripeAccount stripeAccount)
        {
            return new AddressViewModel()
            {
                Address1 = stripeAccount.LegalEntityAddress1,
                Address2 = stripeAccount.LegalEntityAddress2,
                City = stripeAccount.LegalEntityCity,
                State = stripeAccount.LegalEntityState,
                PostalCode = stripeAccount.LegalEntityPostalCode
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

            return new AddressViewModel()
            {
                Address1 = activity.Address1,
                Address2 = activity.Address2,
                City = activity.City,
                State = activity.State,
                PostalCode = activity.PostalCode
            };
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

            if (activity.GetDays().Count > 0)
            {
                text += " - " + activity.GetDaysOfTheWeek().Select(x => x.ToString().MaxChars(2)).ToStringList(", ");
            }

            if (activity.GetGradeLevels().Count > 0)
            {
                text += " - " + activity.GetGradeLevels().Select(x => x.Abbreviation).ToStringList(", ");
            }

            if (activity.AgeStart.HasValue && activity.AgeEnd.HasValue)
            {
                text += " - Ages " + activity.AgeStart + " to " + activity.AgeEnd;
            }

            return text;
        }

        public static string ToString(this ActivitySchedule schedule)
        {
            return "";
        }

        public static SystemMessages GetItemsRequiredToPublish(this Activity activity)
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
            {
                setupMsgs.Add("Activity Name", MessageType.UserError);
            }
            if (activity.SeatsAvailable.HasValue == false)
            {
                setupMsgs.Add("Seats Available", MessageType.UserError);
            }
            if (!activity.ActivityGroupId.HasValue)
            {
                if (activity.GetActiveRegistrationPeriod().IsEmpty())
                {
                    setupMsgs.Add(UILabel.RegistrationPeriodLabel(), MessageType.UserError);
                }
                if (activity.GetActiveActivityForm().IsEmpty())
                {
                    setupMsgs.Add("Registration Form", MessageType.UserError);
                }
            }
            if (setupMsgs.HasErrors())
            {
                setupMsgs.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editsetup/{0}\" class=\"publish-error-title\">Settings</a>", activity.Uid), MessageType.UserError));
                msgs.AddRange(setupMsgs);
            }

            if (!activity.HasSchedule)
            {
                scheduleMessages.Add("Schedule Meeting Dates/Times", MessageType.UserError);
            }
            if (scheduleMessages.HasErrors())
            {
                scheduleMessages.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editschedule/{0}\" class=\"publish-error-title\">Schedule</a>", activity.Uid), MessageType.UserError));
                msgs.AddRange(scheduleMessages);
            }

            if (BasicLibrary.Utilities.RemoveHTMLAndEntities(activity.Details).IsBlank())
            {
                detailMsgs.Add("Description", MessageType.UserError);
            }
            if (detailMsgs.HasErrors())
            {
                detailMsgs.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editdetails/{0}\" class=\"publish-error-title\">About Your Activity</a>", activity.Uid), MessageType.UserError));
                msgs.AddRange(detailMsgs);
            }

            if (activity.ContactName.IsBlank())
            {
                contactMsgs.Add("Contact Name", MessageType.UserError);
            }
            if (activity.ContactPhone.IsBlank())
            {
                contactMsgs.Add("Contact Phone", MessageType.UserError);
            }
            if (activity.ContactEmail.IsBlank())
            {
                contactMsgs.Add("Contact Email", MessageType.UserError);
            }
            if (contactMsgs.HasErrors())
            {
                contactMsgs.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editcontact/{0}\" class=\"publish-error-title\">Contact Information</a>", activity.Uid), MessageType.UserError));
                msgs.AddRange(contactMsgs);
            }

            if (BasicLibrary.Utilities.RemoveHTMLAndEntities(activity.LocationDetails).IsBlank())
            {
                locationMsgs.Add("Location Details", MessageType.UserError);
            }
            //if (activity.Address1.IsBlank())
            //{
            //    locationMsgs.Add("Street Address", MessageType.UserError);
            //}
            //if (activity.City.IsBlank())
            //{
            //    locationMsgs.Add("City", MessageType.UserError);
            //}
            //if (activity.State.IsBlank())
            //{
            //    locationMsgs.Add("State", MessageType.UserError);
            //}
            //if (activity.PostalCode.IsBlank())
            //{
            //    locationMsgs.Add("Postal Code", MessageType.UserError);
            //}
            if (locationMsgs.HasErrors())
            {
                locationMsgs.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editlocation/{0}\" class=\"publish-error-title\">Location</a>", activity.Uid), MessageType.UserError));
                msgs.AddRange(locationMsgs);
            }

            if (activity.CostPerRegistrant.HasValue && activity.CostPerRegistrant.Value > 0 && !activity.AllowToPayLater && !activity.AcceptCreditCard && !activity.AcceptPayPal)
            {
                paymentMsgs.Add("Payment Information: Either accept credit cards, paypal, or allow to pay later", MessageType.UserError);
            }
            if (paymentMsgs.HasErrors())
            {
                paymentMsgs.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editcost/{0}\" class=\"publish-error-title\">Payment</a>", activity.Uid), MessageType.UserError));
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
                confirmationMsgs.Insert(0, new SystemMessage(string.Format("<a href=\"/org/activity/editconfirmation/{0}\" class=\"publish-error-title\">Confirmation Message</a>", activity.Uid), MessageType.UserError));
                msgs.AddRange(confirmationMsgs);
            }

            if (activity.IsArchived)
            {
                archiveMsgs.Add(string.Format("<a href=\"/org/activity/restore/{1}\">Restore {0}</a>. It is currently archived", activity.Name, activity.Uid), MessageType.Error);
            }
            if (activity.IsGroupArchived)
            {
                archiveMsgs.Add(string.Format("<a href=\"/org/group/restore/{1}\">Restore {0}</a>. It is currently archived", activity.GetActivityGroup().Name, activity.GetActivityGroup().Uid), MessageType.Error);
            }

            if (archiveMsgs.HasErrors())
            {
                archiveMsgs.Insert(0, new SystemMessage(string.Format("<span class=\"publish-error-title\">Archive Errors<span>", activity.Uid), MessageType.UserError));
                msgs.AddRange(archiveMsgs);
            }

            return msgs;
        }

        public static SystemMessages GetItemsRequiredToPublish(this ActivityGroup group)
        {
            var msgs = new SystemMessages();

            if (group.IsArchived)
            {
                msgs.Add(string.Format("<a href=\"/org/group/restore/{1}\">Restore {0}</a>. It is currently archived", group.Name, group.Uid), MessageType.Error);
            }

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