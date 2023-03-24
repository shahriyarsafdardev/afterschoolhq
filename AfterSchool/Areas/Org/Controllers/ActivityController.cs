using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Areas.Org.Assemblers.Activity;
using AfterSchool.Controllers;
using AfterSchool.Models;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Helpers;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using CsvHelper;
using CsvHelper.Configuration;
using Hangfire;
using Email = AfterSchoolLibrary.Helpers.Email;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ActivityController : BaseLoggedInController
    {
        private const string IsNewRequest = "isnew";

        public ActivityController() : base(FormConstants.OrganizationAccountTypes)
        {
        }

        public ActionResult Summary(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var currentUser = SessionVariables.CurrentUser;
            var accountUserType = SessionVariables.AccountUserType;
            var accountPackage = SessionVariables.AccountPackage;
            var result = GatherAndAssembleDataForSummaryResult(activity, currentAccount, currentUser, accountUserType, accountPackage, msgs);
            return View(result);
        }

        private static SummaryResult GatherAndAssembleDataForSummaryResult(Activity activity, Account currentAccount, WebUser currentUser,
            UserType accountUserType,
            AccountPackage accountPackage, SystemMessages msgs)
        {
            var participationObject = activity.GetParticipationObject(msgs);
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var isActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, accountUserType, currentUserIsActivityLeader);
            var schedule = activity.GetSchedule(msgs);
            var totalPaid = activity.GetTotalAmountPaid(msgs);
            var result = new SummaryAssembler().AssembleSummaryResult(activity, participationObject, schedule, totalPaid, isActivityLeader,
                accountUserType, currentAccount, accountPackage, currentUser, msgs);
            return result;
        }

        [HttpGet]
        public ActionResult Index(string group, string pn, string rpp, string archive, string copyerror, string type, string q)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var pageNumber = pn.IsNumber() && pn.ToInteger() > 0 ? pn.ToInteger() : 1;
            var recordsPerPage = rpp.IsNumber() && rpp.ToInteger() > 0 ? rpp.ToInteger() : 10;
            var isArchived = archive.IsBoolean() && archive.ToBoolean();
            var activityType = type.IsNotEmpty() && type.IsNumber() ? type.ToInteger().ToEnum<ActivityType>() : ActivityType.Activity;

            var start = recordsPerPage * (pageNumber - 1);

            var (activities, activityGroup, hasArchivedActivities, totalActivityCount, totalPages) = GetActivitiesForIndexPage(group, q, currentAccount,
                isArchived, start, recordsPerPage, activityType, recordsPerPage, SessionVariables.AccountUserType, SessionVariables.CurrentUser, msgs);

            if (copyerror.IsBoolean() && copyerror.ToBoolean())
                msgs.Add("Error creating copy", MessageType.Error);

            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Groups = currentAccount.GetSessions(msgs);
            ViewBag.Group = activityGroup;
            ViewBag.HasArchivedActivities = hasArchivedActivities;
            ViewBag.ActivityType = activityType;
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalActivityCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            ViewBag.Messages = msgs;

            return View(activities);
        }

        private static (List<Activity> Activities, ActivityGroup ActivityGroup, bool HasArchivedActivities, int TotalActivityCount, int TotalPages)
            GetActivitiesForIndexPage(string group, string q, Account currentAccount, bool isArchived, int start, int length,
                ActivityType activityType, int recordsPerPage, UserType accountUserType, WebUser currentUser, SystemMessages msgs)
        {
            var isActivityGroup = group.IsGuid();
            var activityGroup = isActivityGroup ? currentAccount.GetActivityGroup(group.ToGuid(), msgs) : null;
            var activities = GetActivities(activityGroup, isArchived, start, length, currentAccount, accountUserType, currentUser, q, activityType, msgs);

            if (!activities.Any() && isArchived)
            {
                isArchived = false;
                activities = GetActivities(activityGroup, false, start, length, currentAccount, accountUserType, currentUser, q, activityType, msgs);
            }

            var totalActivityCount = GetActivityCount(q, currentAccount, isArchived, activityType, accountUserType, currentUser, activityGroup, msgs);
            var hasArchivedActivities =
                isArchived || GetActivityCount(q, currentAccount, true, activityType, accountUserType, currentUser, activityGroup, msgs) > 0;

            var totalPages = Math.Ceiling(totalActivityCount.ToDouble() / recordsPerPage).ToInteger();
            var finalActivities = activities.OrderBy(x => x.Name).ToList();
            return (Activities: finalActivities, ActivityGroup: activityGroup, HasArchivedActivities: hasArchivedActivities,
                TotalActivityCount: totalActivityCount, TotalPages: totalPages);
        }

        private static List<Activity> GetActivities(ActivityGroup activityGroup, bool isArchived, int start, int length, Account currentAccount,
            UserType accountUserType, WebUser currentUser, string textToMatch, ActivityType activityType, SystemMessages msgs)
        {
            return activityGroup.IsNotEmpty()
                ? activityGroup.GetActivities(ActivityType.Activity, isArchived, false, start, length, msgs: msgs)
                : currentAccount.GetActivities(accountUserType, currentUser, textToMatch, activityType, isArchived, false,
                    currentAccount.AccountTypeId == AccountType.School, start, length, msgs);
        }

        private static int GetActivityCount(string q, Account currentAccount, bool isArchived, ActivityType activityType, UserType accountUserType,
            WebUser currentUser, ActivityGroup activityGroup, SystemMessages msgs)
        {
            return activityGroup.IsNotEmpty()
                ? activityGroup.GetActivityCount(ActivityType.Activity, isArchived, false, null, msgs)
                : currentAccount.GetActivityCount(accountUserType, currentUser, q, activityType, isArchived, false,
                    currentAccount.AccountTypeId == AccountType.School, msgs);
        }

        [HttpGet]
        public ActionResult FinalizeRegistration(string id, string atype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            activity.FinalizeRegistrations(currentUser, msgs);
            var msg = msgs.HasErrors() ? "Error Finalizing Registration" : "Registration Finalized";

            ViewBag.Messages = msgs;
            return RedirectToAction("registrants", new {id, atype, msg});
        }

        private void RegistrantsViewBag(Activity activity, string atype, string msg, SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var rat = RegistrationAssignmentType.NotSet;

            List<RegistrantActivity> ras = null;

            if (atype.IsNotEmpty())
            {
                rat = (RegistrationAssignmentType) atype.ToInteger();
                if (rat == RegistrationAssignmentType.Invited)
                    ras = activity.GetRegistrantActivities(true, msgs);
            }

            //Review: we will bring all data to provider review but we will disable those in UI so provider will have proper idea.            
            if (ras.IsEmpty())
                ras = activity.GetRegistrantActivities(includeInvited: true, msgs);

            if (rat != RegistrationAssignmentType.NotSet)
                ras = ras.Where(x => x.AssignmentTypeId == rat).OrderBy(x => x.SeatIndex).ToList();

            if (msg.IsNotEmpty())
                msgs.Add(msg, msg.ToLower().Contains("error") ? MessageType.UserError : MessageType.Success);

            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            var currentAccount = SessionVariables.CurrentAccount;
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.Registrants = ras;
            ViewBag.AssignmentType = rat;
            ViewBag.IsReadOnlySeatStatus = activity.GetParticipationObject(msgs).IsExternalToAccount(currentAccount);
            ViewBag.Messages = msgs;
        }

        [HttpGet]
        public ActionResult Registrants(string id, string atype, string msg, string registrationDate)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            if (msg.IsNotEmpty())
                msgs.Add(msg, msg.ToLower().Contains("error") ? MessageType.UserError : MessageType.Success);
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var result = GatherDataForAndAssembleRegistrantsResult(activity, atype, currentAccount, currentUser, msgs, registrationDate);

            if (activity.ActivityTypeId == ActivityType.VolunteerOpportunity)
                return View("VolunteerRegistrants", result);

            return View(result);
        }

        private static RegistrantsResult GatherDataForAndAssembleRegistrantsResult(Activity activity, string assignmentTypeAsString, Account currentAccount,
            WebUser currentUser, SystemMessages msgs, string registrationDate = null)
        {
            var assignmentType = RegistrationAssignmentType.NotSet;
            List<RegistrantActivity> registrantActivities = null;

            if (assignmentTypeAsString.IsNotEmpty())
            {
                assignmentType = (RegistrationAssignmentType) assignmentTypeAsString.ToInteger();
                if (assignmentType == RegistrationAssignmentType.Invited)
                    registrantActivities = activity.GetRegistrantActivities(true, msgs);
            }

            //Review: we will bring all data to provider review but we will disable those in UI so provider will have proper idea.            
            if (registrantActivities.IsEmpty())
                registrantActivities = activity.GetRegistrantActivities(includeInvited: true, msgs);

            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            var accountUserType = SessionVariables.AccountUserType;
            var isReadOnlySeatStatus = activity.GetParticipationObject(msgs).IsExternalToAccount(currentAccount);

            var result = new RegistrantsAssembler().AssembleRegistrantsResult(activity, registrantActivities, currentAccount, currentUser, assignmentType,
                currentAccount.GetActiveAccountPackage(msgs), isReadOnlySeatStatus, activity.GetSchedule(msgs), accountUserType,
                currentUserIsActivityLeader, msgs, registrationDate);
            return result;
        }

        [HttpGet]
        public ActionResult MoveRegistrant(string id, string registrant, string assignmenttype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                activity.ID.Value,
                ParticipationObjectType.Activity,
                registrant.ToInteger(),
                msgs);

            var newAssignmentType = (RegistrationAssignmentType) assignmenttype.ToInteger();

            if (ra.IsNotEmpty())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        ra.ChangeSeatAssignment(
                            newAssignmentType,
                            currentUser,
                            amsgs,
                            adc);
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            RegistrantsViewBag(activity, assignmenttype, null, msgs);
            ViewBag.Messages = msgs;
            return RedirectToAction("registrants", new {id});
        }

        [HttpGet]
        public ActionResult RegistrantSummary(string id, string volunteers)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var getVolunteers = volunteers.IsBoolean() && volunteers.ToBoolean();
            BackgroundJob.Enqueue(() =>
                ActivityService.SendRegistrationSummaryReport(activity.ID.Value,
                    UILabel.ActivityLabel(activity.ActivityTypeId, false), getVolunteers, null)
            );

            msgs.Add("Your report will be sent shortly", MessageType.Success);

            var result = GatherDataForAndAssembleRegistrantsResult(activity, "", currentAccount, currentUser, msgs);
            if (getVolunteers)
                return View("VolunteerRegistrants", result);

            return View("Registrants", result);
        }

        [HttpGet]
        public ActionResult RegistrantDownload(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var af = activity.GetActiveActivityForm(msgs);
            var questions = af.GetForm(msgs).GetAllQuestions(msgs);
            var registrantActivities = activity.GetRegistrantActivities(msgs);
            var table = new List<List<string>>();
            var row = new List<string>();

            row.Add("Date Registered");
            row.Add("Seat Assignment");

            if (activity.CostPerRegistrant > 0)
                row.Add("Amount Paid");

            foreach (var question in questions)
                row.Add(question.QuestionDataTypeId.ToString());

            table.Add(row);

            foreach (var registrantActivity in registrantActivities)
            {
                row = new List<string>();

                row.Add(registrantActivity.CreateDate.ToShortDateTimeString());
                row.Add(registrantActivity.AssignmentTypeId.ToString().SplitByUpperCase());

                if (activity.CostPerRegistrant > 0)
                    row.Add(registrantActivity.GetAmountPaid(msgs).ToCurrency());

                foreach (var question in questions)
                {
                    var answer = question.GetAnswer(registrantActivity.RegistrantId.Value);

                    if (answer.IsNotEmpty())
                        row.Add(answer.Text);
                    else
                        row.Add("");
                }

                table.Add(row);
            }

            var csv = CsvFile.Create(table);

            Response.Clear();
            Response.AddHeader("Content-Type", BasicLibrary.Utilities.GetMimeType("csv"));
            Response.AddHeader("Content-Disposition", "attachment; filename=registrants.csv");

            var fileBytes = Encoding.UTF8.GetBytes(csv);
            Response.AddHeader("Content-Length", fileBytes.Length.ToString());
            Response.BinaryWrite(fileBytes);
            Response.End();

            var result = GatherDataForAndAssembleRegistrantsResult(activity, "", currentAccount, currentUser, msgs);
            return View("Registrants", result);
        }

        [HttpPost]
        public JsonResult WithdrawRegistrant(string participationObjectId, string registrantUId, string type,
            string isWithdrawAndCancelClicked)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var isCancelPayment = true;

            if (currentAccount.IsEmpty())
                return Json(new {success = false, status = "error"}, JsonRequestBehavior.AllowGet);

            if (participationObjectId.IsEmpty() || registrantUId.IsEmpty() || type.IsEmpty())
                return Json(new {success = false, status = "error"}, JsonRequestBehavior.AllowGet);

            if (isWithdrawAndCancelClicked.IsNotEmpty())
                isCancelPayment = isWithdrawAndCancelClicked == "true";

            var registrant = currentAccount.GetRegistrant(registrantUId.ToGuid(), msgs);

            if (registrant.IsNotEmpty())
            {
                registrant.WithdrawRegistration(
                    participationObjectId: participationObjectId.ToInteger(),
                    participationObjectType:
                    (ParticipationObjectType) Enum.Parse(typeof(ParticipationObjectType), type),
                    loggedInUser: SessionVariables.CurrentUser,
                    isCancelPayment: isCancelPayment,
                    isParent: false,
                    account: currentAccount,
                    msgs: msgs);
            }

            if (msgs.HasNoErrors())
            {
                var registrantName = registrant.Name;
                var successMessage = new SystemMessage($"{registrantName} Withdrawn successfully.", MessageType.Success);
                SetTempMessages(new SystemMessages { successMessage });
                return Json(new { success = true, status = "ok" }, JsonRequestBehavior.AllowGet);
            }

            // something went wrong, error occured
            return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) },
                JsonRequestBehavior.AllowGet);
        }

        private List<AttachedCoupon> GetAttachedCoupons(RegistrantActivity registrantActivity, SystemMessages msgs)
        {
            if (msgs.IsEmpty()) msgs = new SystemMessages();
            var masterRegistrant = registrantActivity.GetRegistrant(msgs).GetMasterRegistrant(msgs);
            return masterRegistrant?.GetAccount(msgs).GetAttachedCoupons() ?? new List<AttachedCoupon>();
        }

        [HttpGet]
        public ActionResult PrintRegistrants(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var registrantActivities = activity.GetRegistrantActivities(msgs);
            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            var isActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);

            var results = new PrintRegistrantsResults
            {
                Activity = activity,
                RegistrantActivities = registrantActivities,
                Messages = msgs,
                IsActivityLeader = isActivityLeader,
            };

            return View(results);
        }

        [HttpGet]
        public ActionResult RegistrantDetail(string id)
        {
            if (id.IsEmpty() || id.IsNotGuid())
                return HttpNotFound();

            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var registrantActivity = RegistrantActivityService.GetRegistrantActivity(id.ToGuid(), msgs);

            // cant get activity, if registrantActivity doesnt exist in this context
            if (registrantActivity.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(registrantActivity.ParticipationObjectId.Value, msgs);

            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity, msgs);
            ViewBag.Messages = msgs;

            return View(registrantActivity);
        }

        [HttpPost]
        public ActionResult AddPayment(string id, string datepaid, string amountpaid, string paymenttype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var registrantActivity = RegistrantActivityService.GetRegistrantActivity(id.ToGuid(), msgs);
            var activity = registrantActivity.GetActivity(msgs);

            if (!datepaid.IsDateTime())
                msgs.Add("Date Paid must be a valid date", MessageType.UserError);

            if (!amountpaid.IsNumber() || amountpaid.ToDecimal() < 0)
                msgs.Add("Amount Paid must be a number greater than or equal to 0", MessageType.UserError);

            if (!paymenttype.IsNumber())
                msgs.Add("Please select a valid payment type", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                // the registrant that is only on the provider or school account
                var registrant = registrantActivity.GetRegistrant(msgs);

                // from the parent account, see if the registrant has a parent account
                var masterRegistrant = registrant.GetMasterRegistrant(msgs);

                // master registrant could be empty because a registrant can be created on the backend
                // so we may not have a purchaser account
                var purchaserAccount = masterRegistrant?.GetAccount(msgs);

                AccountService.CreatePurchaseTransactionPayment(
                    checkoutObject: new CheckoutObject
                    {
                        BillingAccount = currentAccount,
                        PayingAccount = purchaserAccount,
                        NumberOfSeats = 1,
                        ParticipationObject = registrantActivity.GetParticipationObject(msgs),
                        Registration = registrantActivity.GetRegistration(msgs),
                        MasterRegistrant = masterRegistrant,
                        PaymentType = (PaymentType)paymenttype.ToInteger(),
                        PaymentSourceType = PaymentSourceType.ManualProvider,
                        OverrideAmount = amountpaid.ToDecimal(),
                        PaymentTypeFeeOverrideAmount = 0,
                    },
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs
                );

                if (msgs.HasNoErrors())
                {
                    msgs.Add("Payment has been added", MessageType.Success);

                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: SessionVariables.CurrentUser,
                        actionType: WebSessionActionType.ManuallyPaid,
                        objectType: ObjectType.Activity,
                        objectId: activity.ID.Value,
                        objectText: " of amount " + amountpaid
                    );
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity, msgs);
            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);

            return View("registrantdetail", registrantActivity);
        }

        [HttpPost]
        public ActionResult AttachCoupon(string id, decimal couponAmount, CouponAmountType discountType,
            DateTime? expirationDate, CouponApplyType couponApplyType, StripeCouponDuration duration,
            int durationInMonths)
        {
            var msgs = new SystemMessages();
            var registrantActivity = RegistrantActivityService.GetRegistrantActivity(id.ToGuid(), msgs);
            var activity = registrantActivity.GetActivity(msgs);

            // check coupon's minimum amount
            if (couponAmount <= 0)
                msgs.Add("Coupon Amount must be a number greater than 0", MessageType.UserError);

            // check coupon's amount percentage 
            if (discountType == CouponAmountType.Percentage)
            {
                couponAmount /= 100;

                if (couponAmount > 1)
                    msgs.Add("Percentage must be a number less than or equal to 100%", MessageType.UserError);
            }
            else
            {
                // by default the maximum amount is the activity's cost
                var maxAmount = activity.CostPerRegistrant.Value;

                // check coupon's amount
                if (couponAmount > maxAmount)
                    msgs.Add($"Coupon Amount must be a number less than or equal to {maxAmount.ToCurrency()}",
                        MessageType.UserError);
            }

            if (couponApplyType == CouponApplyType.SubscriptionPlan)
            {
                if (duration == StripeCouponDuration.Repeating && durationInMonths < 1)
                    msgs.Add("# of Times must be a number greater than 0", MessageType.UserError);
            }

            // check coupon's expiration date
            if (expirationDate.IsEmpty() || expirationDate <= DateTime.UtcNow.Date)
                msgs.Add("Expiration date must be in the future", MessageType.UserError);

            ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity, msgs);
            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(SessionVariables.CurrentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUserIsActivityLeader);
            ViewBag.Messages = msgs;

            return View("RegistrantDetail", registrantActivity);
        }

        [HttpGet]
        public ActionResult PublishDetail(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Account = currentAccount;
            ViewBag.MustRequestToPublish = MustRequestToPublish(activity, msgs);
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity, msgs);
            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpGet]
        public ActionResult RequestToPublish(string id, bool isFromActivityHome = false)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var isNewActivity = Request[IsNewRequest] == "1";

            var locationOwner = activity.GetLocationOwner(msgs);

            if (locationOwner.IsNotEmpty())
            {
                activity.RequestToPublish(locationOwner, SessionVariables.CurrentUser, msgs);
                msgs.Add($"Your request to publish has been sent to {locationOwner.Name}", MessageType.Success);
            }
            else
                msgs.Add("Activity saved successfully", MessageType.Success);

            ViewBag.MustRequestToPublish = MustRequestToPublish(activity, msgs);
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity, msgs);
            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.IsFullScreen = isNewActivity;
            ViewBag.Messages = msgs;
            SetTempMessages(msgs);

            if (isFromActivityHome)
                return Redirect(activity.ShortHomePageUrl);

            return View("publishdetail", activity);
        }

        private void BeforePublish(Activity activity, SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var currentAccount = SessionVariables.CurrentAccount;

            // check if provided by external provider
            var location = activity.GetLocation(msgs);
            var isLoggedInAccountOwnerAccount = currentAccount.ID == location?.HostAccountId;
            var activityIsHostedAtAnotherAccount = location?.HostAccountId != activity.AccountId;

            // activity is created in the activity provider's account
            if (isLoggedInAccountOwnerAccount && activityIsHostedAtAnotherAccount)
            {
                var visitingAccount = activity.GetAccount(msgs);

                if (!visitingAccount.IsApprovedAsVisitorTo(currentAccount, msgs))
                {
                    currentAccount.ApproveAccessToAccount(visitingAccount, SessionVariables.CurrentUser, msgs);

                    if (msgs.HasNoErrors())
                        currentAccount.SendApprovalNotificationToVisitor(visitingAccount, msgs);
                }
            }
        }

        [HttpGet]
        public ActionResult Publish(string id, bool isFromActivityHome = false)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var mustRequestToPublish = MustRequestToPublish(activity, msgs);

            if (mustRequestToPublish)
                msgs.Add("You do not have permissions to publish", MessageType.UserError);

            msgs.AddRange(activity.GetItemsRequiredToPublish(msgs));

            if (msgs.HasNoErrors())
            {
                BeforePublish(activity, msgs);
                activity.Publish(SessionVariables.CurrentUser, msgs);
            }

            if (msgs.HasNoErrors())
            {
                if (!activity.ActivityGroupId.HasValue)
                {
                    //.. send support email for AHQ just to notifiy the team a new profile has been completed
                    var body = $"<p>Account: {currentAccount.Name}<br />" +
                               $"Account Profile Url: {currentAccount.HomePageUrl}<br />" +
                               $"Activity: {activity.Name}<br />" +
                               $"Url: {activity.HomePageUrl}<br />" +
                               $"User: {currentUser.FirstName} {currentUser.LastName}<br />";

                    var registrationDate = activity.GetActiveRegistrationPeriod();
                    if (registrationDate.IsNotEmpty() && registrationDate.StartDateTime.IsNotEmpty())
                        body += $"Registration Start Date: {registrationDate.StartDateTime} <br />";

                    if (activity.HasSchedule)
                        body +=
                            $"Activity Start Date: {activity.GetSchedule(msgs).StartDate.ToShortDateString()} <br />";

                    if (activity.AccountTimeZone.IsNotEmpty())
                        body += $"All times are in {activity.AccountTimeZone} <br />";

                    EmailHelper.SendEmail(
                        toEmail: Email.AppCustomersEmailAddress,
                        subject: $"{currentAccount.Name} has published a new Activity: {activity.Name}",
                        body,
                        isAfterSchool: true);
                }

                // Show Succes Message
                // LOG: Someone Published Activity ActivityName for Account AccountName
                msgs.Add(string.Format("{0} has been published", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Published,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.IsFullScreen = Request[IsNewRequest] == "1";
            ViewBag.MustRequestToPublish = mustRequestToPublish;
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity, msgs);
            ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            ViewBag.Messages = msgs;

            if (isFromActivityHome && msgs.HasNoErrors())
                return Redirect(activity.ShortHomePageUrl);

            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Unpublish(string id, string redirectUrl, bool isFromActivityHome = false)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var mustRequestToPublish = MustRequestToPublish(activity, msgs);

            //if (mustRequestToPublish)
            //    msgs.Add("You do not have permissions to unpublish", MessageType.UserError);

            if (msgs.HasNoErrors())
                activity.Unpublish(currentUser, msgs);

            // Show Success Message
            // LOG: Someon Unpublished Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been unpublished", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Unpublished,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.MustRequestToPublish = mustRequestToPublish;
            ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity, msgs);
            ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            ViewBag.Messages = msgs;

            if (isFromActivityHome)
                return Redirect(activity.ShortHomePageUrl);

            if (redirectUrl.IsNotEmpty())
            {
                SetTempMessages(msgs);
                return Redirect(redirectUrl);
            }

            return View("publishdetail", activity);
        }

        [HttpGet]
        public ActionResult Messages(string id, string pn, string rpp)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var totalMessageCount = activity.GetMessagesCount(msgs);
            var activityMessages = activity.GetMessages(start, length, msgs);
            var totalPages = Math.Ceiling(totalMessageCount.ToDouble() / recordsPerPage).ToInteger();

            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActivityMessages = activityMessages;
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalMessageCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpGet]
        public ActionResult MessageDetail(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var am = currentAccount.GetActivityMessage(id.ToGuid(), msgs);
            var activity = currentAccount.GetActivity(am.ActivityId.Value, msgs);

            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
            ViewBag.Messages = msgs;

            return View(am);
        }

        [HttpGet]
        public ActionResult MessageAdd(string id, string toUid = "", string returnUrl = "")
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            var assignmentTypes = new List<RegistrationAssignmentType>
            {
                RegistrationAssignmentType.Applied, RegistrationAssignmentType.WaitingList,
                RegistrationAssignmentType.Reserved, RegistrationAssignmentType.ReservedPendingParentalConsent,
                RegistrationAssignmentType.ReservedPendingPayment, RegistrationAssignmentType.Invited,
                RegistrationAssignmentType.ReservedPendingBackgroundCheck,
            };

            if (toUid.IsGuid())
            {
                assignmentTypes.Add(RegistrationAssignmentType.Withdrawn);

                ViewBag.Registrants = activity.GetRegistrantActivities(true, msgs)
                    .Where(ra => ra.AssignmentTypeId.In(assignmentTypes) && ra.RegistrantUid == toUid.ToGuid())
                    .Select(ra => ra.ToRegistrant())
                    .OrderBy(x => x.FirstName)
                    .ToList();
                ViewBag.RedirectUrl = returnUrl;
            }
            else
            {
                ViewBag.Registrants = activity.GetRegistrantActivities(true, msgs)
                    .Where(ra => ra.AssignmentTypeId.In(assignmentTypes))
                    .Select(ra => ra.ToRegistrant())
                    .OrderBy(x => x.FirstName)
                    .ToList();
            }

            // If the user does have the messages feature, and the user tries to send a message, validate the user hasn't used up their quota of emails, and/or text messages, if they have exceeded their quota, show the upgrade.
            // If the user has hit or exceeded the email quota then show the upgrade modal
            ViewBag.EmailCount = currentAccount.GetEmailCountForCurrentMessagePeriod(msgs);
            ViewBag.TextCount = currentAccount.GetTextMessageCountForCurrentMessagePeriod(msgs);
            ViewBag.Body = "";
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult MessageAdd(string id, string deliveryMethod, string subject, string body,
            string sendtostaff, string sendtextmessages, string parentsonly)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (subject.IsEmpty())
                msgs.Add("Message Reason is required", MessageType.UserError);
            if (body.IsEmpty())
                msgs.Add("Message Body is required", MessageType.UserError);

            ActivityMessage am = null;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            if (msgs.HasNoErrors())
            {
                var registrantIds = activity.GetRegistrants(true, msgs)
                    .Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
                    .Select(x => x.ID.Value)
                    .ToList();

                //.. email messaging
                if (msgs.HasNoErrors())
                {
                    var willExceedQuota = currentAccount.WillExceedEmailQuota(SessionVariables.AccountPackage,
                        registrantIds.Count, currentUser, msgs);

                    if (!willExceedQuota)
                    {
                        am = activity.AddMessage(
                            subject,
                            body,
                            registrantIds,
                            sendtostaff.IsBoolean() && sendtostaff.ToBoolean(),
                            parentsonly.IsBoolean() && parentsonly.ToBoolean(),
                            currentUser.ID.Value,
                            msgs);

                        BackgroundJob.Enqueue(() =>
                            ActivityMessageService.Send(am.ID.Value, currentUser.ID.Value, null, null));

                        msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
                        ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
                        ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                            currentUserIsActivityLeader);
                        ViewBag.Messages = msgs;
                        return View("messagedetail", am.Reload());
                    }
                }
            }

            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.Registrants = activity.GetRegistrants(msgs);
            ViewBag.Body = body;
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Users = currentAccount.GetUsers(msgs: msgs);
            var currentUserIsActivityLeader = SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
            ViewBag.IsActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpPost]
        public ActionResult TeamSave(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            Activity activity = null;
            Account account = null;
            List<WebUser> users = null;

            Database.ExecuteTransaction
            (
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    activity = currentAccount.GetActivity(id.ToGuid(), amsgs, adc);
                    account = activity.GetAccount(amsgs, adc);
                    users = account.GetUsers(msgs: amsgs, dc: adc);

                    users.ForEach(user =>
                    {
                        var doNotSkip = !(activity.CreatedBy.Value == user.ID.Value || currentUser.ID == user.ID);

                        if (doNotSkip)
                        {
                            var awu = user.GetActivityWebUser(activity, amsgs, adc);

                            if (Request["onteam-" + user.ID.Value.ToString()].IsNotEmpty())
                            {
                                if (awu.IsEmpty())
                                {
                                    awu = new ActivityWebUser();
                                    awu.ActivityId = activity.ID;
                                    awu.WebUserId = user.ID;
                                    awu.CreateDate = DateTime.UtcNow;
                                    awu.CreatedBy = currentUser.ID;
                                }

                                if (Request["isleader-" + user.ID.Value.ToString()].IsNotEmpty())
                                    awu.IsLeader = true;
                                else
                                    awu.IsLeader = false;

                                awu.LastModifiedDate = DateTime.UtcNow;
                                awu.LastModifiedBy = currentUser.ID;
                                amsgs.AddRange(awu.Save(adc));
                            }
                            else if (awu.IsNotEmpty())
                                amsgs.AddRange(awu.Delete(adc));
                        }
                    });
                },
                onSuccess: (SystemMessages smsgs) =>
                {
                    msgs.AddRange(smsgs);
                    msgs.Add("The Activity Team has been updated", MessageType.Success);
                },
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
            );

            ViewBag.Messages = msgs;
            ViewBag.Users = users;
            return View("Team", activity);
        }

        [HttpGet]
        public ActionResult AddRegistration(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var activeForm = activity.GetActiveForm(msgs);
            var questions = activeForm.GetActiveQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantGender:
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                        question.IsRequired = true;
                        break;
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            ViewBag.QuestionViewModels = questions.OrderBy(x => x.FormQuestionGroupDisplayOrder)
                .ThenBy(x => x.DisplayOrder)
                .Select(x => new FormQuestionViewModel(x)).ToList();
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpPost]
        public ActionResult AddRegistration(string id, string parentsig)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var questions = activity.GetActiveForm(msgs).GetActiveQuestions(msgs);

            questions.ForEach(question =>
            {
                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantGrade:
                    case QuestionDataType.RegistrantGender:
                    case QuestionDataType.RegistrantFirstName:
                    case QuestionDataType.RegistrantLastName:
                        question.IsRequired = true;
                        break;
                    default:
                        question.IsRequired = false;
                        break;
                }
            });

            questions.ForEach(question =>
            {
                var answer = Request[question.ToFormQuestionViewModel().Name];

                if (question.IsRequired && answer.IsEmpty())
                    msgs.Add($"{question.Text} is required", MessageType.UserError);

                switch (question.QuestionDataTypeId)
                {
                    case QuestionDataType.RegistrantHomePhone:
                    case QuestionDataType.RegistrantWorkPhone:
                    case QuestionDataType.RegistrantMobilePhone:
                        if (answer.IsNotEmpty() && !answer.IsRegExMatch(BasicLibrary.Utilities.PhoneRegEx))
                            msgs.Add($"{question.Text} must be a valid phone number", MessageType.UserError);
                        break;
                }
            });

            var qvms = questions.OrderBy(question => question.DisplayOrder)
                .Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

            if (msgs.HasNoErrors())
                currentAccount.CreateNewRegistrantAndRegistration(
                    participationObject: activity.GetParticipationObject(msgs),
                    questionsAndAnswers: qvms.Select(qvm => (qvm.Question, qvm.Value)).ToList(),
                    period: activity.GetCurrentRegistrationPeriod(msgs),
                    loggedInUser: SessionVariables.CurrentUser,
                    msgs: msgs);

            ViewBag.Messages = msgs;
            ViewBag.QuestionViewModels = qvms;

            if (msgs.HasNoErrors())
            {
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.ManuallyRegistered,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: "Student for activity " + activity.Name
                );
                return RedirectToAction("registrants", new { id = activity.Uid });
            }

            return View(activity);
        }

        private bool MustRequestToPublish(Activity activity, SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            return !SessionVariables.CurrentUser.CanPublishActivity(SessionVariables.CurrentAccount, activity, msgs);
        }

        private bool IsExternallyProvided(Activity activity, SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var location = activity.GetLocation(msgs);
            var hostAccount = location?.GetHostAccount(msgs);

            if (hostAccount.IsNotEmpty() && hostAccount.OwnerId.HasValue)
                return hostAccount.ID != activity.AccountId;

            return false;
        }

        private bool ShowCopyButtonOnPublishDetail(Activity activity, SystemMessages msgs)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            return activity.AccountId == SessionVariables.CurrentAccount.ID || !IsExternallyProvided(activity, msgs);
        }

        [HttpGet]
        public virtual JsonResult GetCalculatedStartingAndEndDateForSubscription(string activityId,
            string billingPeriodType, string dayOfWeek, string dayOfMonth)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (!IsRequestFromApp() || activityId.IsEmpty() || currentAccount.IsEmpty())
                return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);

            var activity = currentAccount.GetActivity(activityId.ToInteger(), msgs);
            var dates = activity.FinalizeStartAndEndDateForSubscription((BillingPeriodType)billingPeriodType.ToInteger(),
                dayOfWeek.IsNotEmpty() ? dayOfWeek.ToEnum<DayOfWeek>() : DayOfWeek.Sunday,
                dayOfMonth.IsNotEmpty() ? dayOfMonth.ToDateTime() : DateTime.Now.AddDays(1));

            if (dates.paymentStart.IsNotEmpty() && dates.paymentStart.HasValue && dates.paymentEnd.IsNotEmpty() &&
                dates.paymentEnd.HasValue)
                return Json(new
                {
                    success = true,
                    startDate = dates.paymentStart.Value.ToString(),
                    startDateFormatted = dates.paymentStart.Value.ToString("dddd, MMMM d, yyyy"),
                    endDate = dates.paymentEnd.Value.ToString(),
                    endDateFormatted = dates.paymentEnd.Value.ToString("dddd, MMMM d, yyyy"),
                }, JsonRequestBehavior.AllowGet);

            return Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        public class PrintRegistrantsResults
        {
            public Activity Activity { get; set; }
            public List<RegistrantActivity> RegistrantActivities { get; set; }
            public SystemMessages Messages { get; set; }
            public bool IsActivityLeader { get; set; }
        }

        #region Edit Activity

        [HttpGet]
        public ActionResult EditSetup(string id, string group, string type)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            ActivityGroup activityGroup = null;
            Activity activity;
            EditSetupResult result;

            if (id.IsGuid())
            {
                activity = currentAccount.GetActivity(id.ToGuid(), msgs);
                activityGroup = activity.GetActivityGroup(currentAccount.ID.Value, msgs);

                result = GatherDataForAndAssembleEditSetupResult(activity, activityGroup, msgs, currentAccount, "EditSetup", Request[IsNewRequest]);
                return View(result);
            }

            activity = currentAccount.GetDefaultNewActivity();
            activity.RegistrationModeId = RegistrationMode.Register;
            activity.CostFrequencyTypeId = ActivityCostFrequencyType.PerActivity;

            if (type.IsNotEmpty() && type.IsNumber())
                activity.ActivityTypeId = type.ToInteger().ToEnum<ActivityType>();

            if (activity.ActivityTypeId == ActivityType.LearningPod)
                activity.IsInviteOnly = true;

            //this means a new activity is being added to an existing enrollment group
            if (group.IsGuid())
            {
                activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
                activity.RegistrationModeId = activityGroup.RegistrationModeId;
            }

            result = GatherDataForAndAssembleEditSetupResult(activity, activityGroup, msgs, currentAccount, "EditSetup", Request[IsNewRequest]);
            return View(result);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable,
            string registrationperiod, string registrationopendate, string registrationopentime,
            string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode,
            string group, string cmd, string type, string inviteonly, string minseatsrequired,
            string isAdvanceSettingEnabled, string costfrequencytype, string cutoffhours,
            string isContinuousEnrollmentEnabled)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var accountPackage = SessionVariables.AccountPackage;
            var msgs = new SystemMessages();

            if (id.IsEmpty() || !id.IsGuid())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var defaultActivity = currentAccount.GetDefaultNewActivity();

            if (name.IsEmpty())
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} name is required.", MessageType.UserError);
            else if (name.Length > 75)
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} name must not be greater than 75 characters.", MessageType.UserError);

            if (form.IsEmpty() && (isAdvanceSettingEnabled.IsEmpty() || !isAdvanceSettingEnabled.ToBoolean()))
            {
                ActivityForm activeForm;

                if (activity.IsNotEmpty())
                {
                    //.. edit activity
                    activeForm = activity.GetActiveActivityForm(msgs);
                }
                else
                {
                    currentAccount.EnsureDefaultForm(activity, currentUser, msgs);
                    activeForm = currentAccount.GetDefaultActivityForm(activity, msgs);
                }

                form = activeForm.FormId.Value.ToString();
            }

            if (msgs.HasErrors())
            {
                var result = GatherDataForAndAssembleEditSetupResult(activity, null, msgs, currentAccount, "EditSetup", Request[IsNewRequest]);
                return View(result);
            }

            var activityLocationTypeId = (int)(activity.IsNotEmpty() ? activity.ActivityLocationTypeId : defaultActivity.ActivityLocationTypeId);
            var values = new Dictionary<string, object>
            {
                {"name", name.MaxChars(75)},
                {"seatsavailable", seatsavailable},
                {"registrationperiod", registrationperiod},
                {"registrationopendate", registrationopendate},
                {"registrationopentime", registrationopentime},
                {"registrationclosedate", registrationclosedate},
                {"registrationclosetime", registrationclosetime},
                {"timezone", timezone},
                {"form", form},
                {"regmode", regmode.IsNotEmpty() ? regmode : ((int) RegistrationMode.Register).ToString()},
                {"group", group},
                {"type", type},
                {"inviteonly", inviteonly.IsBoolean().ToString()},
                {"minseatsrequired", minseatsrequired},
                {"gradestart", activity.IsNotEmpty() ? activity.GradeLevelStart : defaultActivity.GradeLevelStart},
                {"gradeend", activity.IsNotEmpty() ? activity.GradeLevelEnd : defaultActivity.GradeLevelEnd},
                {
                    "genderrestrictions",
                    activity.IsNotEmpty() && activity.GenderRestriction.IsNotEmpty()
                        ? (int) activity.GenderRestriction
                        : (int) defaultActivity.GenderRestriction
                },
                {"agestart", activity.IsNotEmpty() ? activity.AgeStart : defaultActivity.AgeStart},
                {"ageend", activity.IsNotEmpty() ? activity.AgeEnd : defaultActivity.AgeEnd},
                {"costfrequencytype", costfrequencytype},
                {"cutoffhours", cutoffhours},
                {"isContinuousEnrollmentEnabled", isContinuousEnrollmentEnabled.IsNotEmpty() && isContinuousEnrollmentEnabled.ToBoolean()},
                {"activitylocationtype", activityLocationTypeId},
                {"isparentpricecost", true},
            };

            var isNew = Request[IsNewRequest] == "1";

            if (isNew && activityLocationTypeId == (int)ActivityLocationType.Physical && activity.IsEmpty())
            {
                var location = currentAccount.GetDefaultLocation();

                if (location.IsNotEmpty())
                {
                    values = values.Concat(new Dictionary<string, object>
                    {
                        {"locationname", location.Name},
                        {"locationdetails", location.Details},
                        {"address1", location.Address1},
                        {"streetnumber", location.StreetNumber},
                        {"streetname", location.StreetName},
                        {"address2", location.Address2},
                        {"neighborhood", location.Neighborhood},
                        {"city", location.City},
                        {"county", location.County},
                        {"state", location.State},
                        {"postalcode", location.PostalCode},
                        {"country", location.Country},
                        {"placeid", location.GooglePlaceId},
                        {"latitude", location.Latitude},
                        {"longitude", location.Longitude},
                        {"mylocation", location.HostAccountId == currentAccount.ID},
                    }).ToDictionary(k => k.Key, v => v.Value);
                }
            }

            ActivityGroup activityGroup = null;
            if (activity.IsNotEmpty())
            {
                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
                activityGroup = activity.GetActivityGroup(msgs);

                // Show Success Message
                // LOG: Someone Updated Activity ActivityName for Account AccountName
                if (msgs.HasNoErrors())
                {
                    msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: currentUser,
                        actionType: WebSessionActionType.Updated,
                        objectType: ObjectType.Activity,
                        objectId: activity.ID.Value,
                        objectText: activity.Name,
                        indirectObjectType: ObjectType.Account,
                        indirectObjectId: currentAccount.ID.Value,
                        indirectObjectText: currentAccount.Name
                    );
                }
            }
            else
            {
                //create initial form for account
                if (msgs.HasNoErrors() && !accountPackage.HasFeature(FeatureType.ManageForms, msgs))
                {
                    var defaultForm = currentAccount.GetDefaultForm(activity, msgs);
                    var newForm = defaultForm.Copy(currentUser.ID.Value, currentAccount.ID.Value,
                        defaultForm.FormTypeId, (name + " form").MaxChars(100, "..."), msgs);

                    if (msgs.HasNoErrors())
                    {
                        if (values.Any(m => m.Key == "form"))
                            values["form"] = newForm.ID.Value.ToString();
                        else
                            values.Add("form", newForm.ID.Value.ToString());
                    }
                }

                values.Add("registrationthankyouemail", "Thank you for registering.");
                values.Add("applicationthankyouemail", "Thank you for applying.");

                activity = ActivityService.CreateNew(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    values: values,
                    msgs: msgs
                );

                // Show Success Message
                // LOG: Someone Updated Activity ActivityName for Account AccountName
                if (msgs.HasNoErrors())
                {
                    msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                    SessionVariables.WebSession.LogActivity(
                        account: currentAccount,
                        actor: currentUser,
                        actionType: WebSessionActionType.Created,
                        objectType: ObjectType.Activity,
                        objectId: activity.ID.Value,
                        objectText: activity.Name,
                        indirectObjectType: ObjectType.Account,
                        indirectObjectId: currentAccount.ID.Value,
                        indirectObjectText: currentAccount.Name
                    );
                }
            }

            if (group.IsGuid())
                activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);

            var fullScreenExitUrl = GetFullScreenExitUrl(activity, group);

            if (cmd == "next")
            {
                switch (activity.ActivityTypeId)
                {
                    case ActivityType.LearningPod:
                    case ActivityType.Activity:
                    case ActivityType.VolunteerOpportunity:
                        var editRestrictionsResult = new EditRestrictionsAssembler().AssembleEditRestrictionsResult(activity, msgs, currentAccount, isNew,
                            fullScreenExitUrl);

                        return View("EditRestrictions", editRestrictionsResult);

                    case ActivityType.FieldTrip:
                    case ActivityType.Team:
                        return GoToEditSchedule(activity, activity.ActivityScheduleId?.ToString(), msgs, currentAccount, currentUser);

                    case ActivityType.PermissionSlip:
                        var isActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                            currentUser.IsActivityLeader(activity, msgs));
                        var editDetailsResult = new EditDetailsAssembler().AssembleEditDetailsResult(currentAccount, msgs, activity, fullScreenExitUrl,
                            isActivityLeader, accountPackage, isNew);
                        return View("EditDetails", editDetailsResult);
                }
            }

            var resultDefault =
                GatherDataForAndAssembleEditSetupResult(activity, activityGroup, msgs, currentAccount, "EditSetup", Request[IsNewRequest]);
            return View(resultDefault);
        }

        private ActionResult GoToEditSchedule(Activity activity, string scheduleId, SystemMessages msgs, Account currentAccount, WebUser currentUser)
        {
            var result = GatherDataForAndAssembleEditScheduleResult(activity, scheduleId, msgs, currentAccount, currentUser, Request[IsNewRequest]);
            return View("EditSchedule", result);
        }

        private EditSetupResult GatherDataForAndAssembleEditSetupResult(Activity activity, ActivityGroup activityGroup, SystemMessages msgs,
            Account currentAccount, string activeMenu, string isNew)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var currentUser = SessionVariables.CurrentUser;
            currentAccount.EnsureDefaultForm(activity, currentUser, msgs);
            var activeForm = activity.GetActiveActivityForm(msgs) ?? activityGroup?.GetDefaultActivityForm(msgs) ??
                currentAccount.GetDefaultActivityForm(activity, msgs);
            var periods = currentAccount.GetOpenAndUpcomingRegistrationPeriods(msgs);
            var activePeriod = activity.GetActiveRegistrationPeriod(msgs) ??
                               currentAccount.GetDefaultNewRegistrationPeriod();

            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var editSetupAssembler = new EditSetupAssembler();
            var fullScreenExitUrl = GetFullScreenExitUrl(activity, activityGroup?.Uid.ToString());
            var forms = currentAccount.GetForms(activity.ActivityTypeId, null, msgs).Where(x => !x.IsArchived || activeForm.FormId == x.ID).OrderBy(x => x.Name)
                .ToList();

            return editSetupAssembler.AssembleEditSetupResult(activity, activityGroup, msgs, currentAccount, activeMenu, periods, activePeriod, activeForm,
                SessionVariables.AccountPackage, SessionVariables.AccountUserType, currentUserIsActivityLeader, isNew, forms,
                fullScreenExitUrl);
        }

        public string GetFullScreenExitUrl(Activity activity, string groupUid = "")
        {
            if (groupUid.IsNotEmpty())
                return Url.Action("Activities", "Group", new { id = groupUid });

            if (activity.ActivityGroupId.HasValue)
            {
                // you have to reload after a save to make sure the ActivityGroupUid is there
                if (activity.ActivityGroupUid.IsEmpty())
                    activity = activity.Reload();

                return Url.Action("Activities", "Group", new { id = activity.ActivityGroupUid });
            }

            return Url.Action("index", "activity", new { type = (int)activity.ActivityTypeId });
        }

        [HttpGet]
        public ActionResult EditRestrictions(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var result = new EditRestrictionsAssembler().AssembleEditRestrictionsResult(activity, msgs, currentAccount, Request[IsNewRequest] == "1",
                fullScreenExitUrl);
            return View(result);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string restrictbygrade,
            string agestart, string ageend, string graderestrictions, string strictgenderrestrictions, string mingrade, string maxgrade,
            string genderrestrictions, string requiresparentalconsent, string isStudentAllowed, string cmd)
        {
            var msgs = new SystemMessages();
            Activity activity;
            var accountPackage = SessionVariables.AccountPackage;
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (activity.IsNotEmpty())
            {
                if (!accountPackage.HasFeature(FeatureType.AllowStudentRegistration) &&
                    requiresparentalconsent.IsBlank() && activity.ActivityTypeId != ActivityType.VolunteerOpportunity)
                    requiresparentalconsent = "1";

                if (restrictbygrade.IsNotEmpty() && restrictbygrade.IsBoolean())
                {
                    if (restrictbygrade.ToBoolean())
                    {
                        agestart = "";
                        ageend = "";
                    }
                    else
                    {
                        mingrade = "";
                        maxgrade = "";
                    }
                }

                if (graderestrictions.IsNumber() && genderrestrictions == "0")
                {
                    strictgenderrestrictions = "0";
                }

                var values = new Dictionary<string, object>
                {
                    {"alloworganization", alloworganization.IfBlank("0")},
                    {"agestart", agestart},
                    {"ageend", ageend},
                    {"gradestart", mingrade},
                    {"gradeend", maxgrade},
                    {"genderrestrictions", genderrestrictions},
                    {"requiresparentalconsent", requiresparentalconsent.IfBlank("0")},
                    {"isStudentAllowed", isStudentAllowed.IfBlank("0")},
                    {"strictgenderrestrictions", strictgenderrestrictions},
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }


            if (msgs.HasNoErrors() && cmd == "next")
            {
                var scheduleId = activity.ActivityScheduleId?.ToString();
                return GoToEditSchedule(activity, scheduleId, msgs, currentAccount, currentUser);
            }

            var isNew = Request[IsNewRequest] == "1";
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);

            var editRestrictionsResult =
                new EditRestrictionsAssembler().AssembleEditRestrictionsResult(activity, msgs, currentAccount, isNew, fullScreenExitUrl);

            return View(editRestrictionsResult);
        }

        [HttpGet]
        public ActionResult EditContact(string id)
        {
            var result = GatherDataForAndAssembleEditContactResult(id, SessionVariables.CurrentUser, SessionVariables.CurrentAccount, Request[IsNewRequest]);
            return View(result);
        }

        private EditContactResult GatherDataForAndAssembleEditContactResult(string id, WebUser currentUser, Account currentAccount, string isNew)
        {
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var instructors = currentAccount.GetUsers(msgs: msgs);
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var accountUserType = SessionVariables.AccountUserType;
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var result = new EditContactAssembler().AssembleEditContactResult(currentAccount, msgs, activity, isNew, fullScreenExitUrl, instructors,
                accountUserType, currentUserIsActivityLeader);
            return result;
        }

        [HttpPost]
        public ActionResult EditContact(string id, string leadertype, string contactname, string contactphone,
            string contactemail, string instructor, string instructorfirstname, string instructorlastname,
            string instructoremail, string instructorphone, string cmd, string provider, string providername,
            string providerfirstname, string providerlastname, string provideremail, string providerphone)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var values = new Dictionary<string, object>
            {
                {"leadertype", leadertype.IfBlank("0")},
                {"contactname", contactname},
                {"contactphone", contactphone},
                {"contactemail", contactemail},
                {"instructor", instructor},
                {"instructorfirstname", instructorfirstname},
                {"instructorlastname", instructorlastname},
                {"instructoremail", instructoremail},
                {"instructorphone", instructorphone},
                {"provider", provider},
                {"providername", providername},
                {"providerfirstname", providerfirstname},
                {"providerlastname", providerlastname},
                {"provideremail", provideremail},
                {"providerphone", providerphone},
            };

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var beforeProviderAccountId = activity.ProviderAccountId;

            if (activity.IsNotEmpty())
                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs);

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            if (msgs.HasNoErrors())
            {
                if (beforeProviderAccountId != activity.ProviderAccountId)
                    activity.NotifyProviderOfNewActivity(currentUser, msgs);
                if (cmd == "next")
                {
                    switch (activity.ActivityTypeId)
                    {
                        case ActivityType.Activity:
                        case ActivityType.LearningPod:
                        case ActivityType.VolunteerOpportunity:
                            var editLocationResult = GatherDataForAndAssembleEditLocationResult(id, Request[IsNewRequest], msgs);
                            return View("EditLocation", editLocationResult);
                        default:
                            var editCostResult = GatherDataForAndAssembleEditCostResult(activity, msgs, currentAccount, currentUser, Request[IsNewRequest]);
                            return View("EditCost", editCostResult);
                    }
                }
            }

            var result = GatherDataForAndAssembleEditContactResult(id, currentUser, currentAccount, Request[IsNewRequest]);
            return View(result);
        }

        [HttpGet]
        public ActionResult EditDetails(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var currentUser = SessionVariables.CurrentUser;
            var isActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            var accountPackage = SessionVariables.AccountPackage;
            var isNew = Request[IsNewRequest] == "1";
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var result = new EditDetailsAssembler().AssembleEditDetailsResult(currentAccount, msgs, activity, fullScreenExitUrl, isActivityLeader,
                accountPackage, isNew);
            return View(result);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditDetails(string id, string description, string details, string cmd)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsEmpty() || id.IsNotGuid())
                return HttpNotFound();

            if (details.IsEmpty())
                msgs.Add($"{UILabel.ActivityLabel()} Details is required", MessageType.UserError);

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (activity.IsNotEmpty())
            {
                var values = new Dictionary<string, object>
                {
                    {"details", details},
                    {"description", description},
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name,
                    msgs: msgs);
            }

            if (msgs.HasNoErrors() && cmd == "next")
            {
                var editContactResult = GatherDataForAndAssembleEditContactResult(id, currentUser, currentAccount, Request[IsNewRequest]);
                return View("EditContact", editContactResult);
            }

            var accountPackage = SessionVariables.AccountPackage;
            var isNew = Request[IsNewRequest] == "1";
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var isActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            var result = new EditDetailsAssembler().AssembleEditDetailsResult(currentAccount, msgs, activity, fullScreenExitUrl, isActivityLeader,
                accountPackage, isNew);
            return View(result);
        }

        private EditScheduleResult GatherDataForAndAssembleEditScheduleResult(Activity activity, string scheduleId, SystemMessages msgs, Account currentAccount,
            WebUser currentUser, string isNew)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = currentAccount.GetActivitySchedule(scheduleId.ToInteger(), msgs);
                if (schedule.AccountId != currentAccount.ID)
                    schedule = null;
            }

            if (schedule.IsEmpty())
                schedule = activity.GetSchedule();
            var activeRegistrationPeriod = activity.GetActiveRegistrationPeriod(msgs);
            var schedules = currentAccount.GetActivitySchedules(msgs);
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var accountUserType = SessionVariables.AccountUserType;
            var fullScreenUrl = GetFullScreenExitUrl(activity);

            return new EditScheduleAssembler().AssembleEditScheduleResult(schedule, activity, currentAccount, accountUserType, activeRegistrationPeriod,
                currentUserIsActivityLeader, schedules, isNew, fullScreenUrl, msgs);
        }

        [HttpGet]
        public ActionResult EditSchedule(string id, string schedule)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            return GoToEditSchedule(activity, schedule, msgs, currentAccount, currentUser);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditSchedule(string id, string cmd, string schedule, string scheduledetails,
            string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime,
            string dailyendtime, string isongoing, string hasschedule)
        {
            var msgs = new SystemMessages();
            var inSessionDates = new List<ActivityInstance>();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            var currentAccountPackage = SessionVariables.AccountPackage;
            List<DateTime> outOfSessionDates = null;
            Dictionary<string, object> values = null;

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (activity.IsNotEmpty())
            {
                if (activity.IsContinuousEnrollment && hasschedule == "1")
                    isongoing = "1";

                if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerClass &&
                    (!hasschedule.IsBoolean() || !hasschedule.ToBoolean()))
                    msgs.Add("Schedule is required", MessageType.UserError);

                if (hasschedule.IsBoolean() && hasschedule.ToBoolean())
                {
                    var filteredParams = Request.Params.Filter("schedulerepeat-");
                    var days = ActivityScheduleUI.PrepDaysOfWeek(filteredParams);

                    if (days.Count == 0 && repeatsweekly == "1")
                        msgs.Add("Please select at least one day", MessageType.UserError);

                    if (msgs.HasNoErrors())
                    {
                        if (!isongoing.IsBoolean())
                            inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate,
                                enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));

                        outOfSessionDates =
                            ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

                        inSessionDates.ForEach(instance =>
                        {
                            if (instance.StartDateTime > instance.EndDateTime)
                                msgs.Add(
                                    "Please ensure start dates and end times are earlier than the corresponding end date and end times",
                                    MessageType.UserError);
                        });
                    }

                    if (startdate.IsNotEmpty() && enddate.IsNotEmpty())
                    {
                        DateTime startDate = startdate.ToDateTime();
                        DateTime endDate = enddate.ToDateTime();
                        if (endDate < startDate)
                        {
                            msgs.Add(
                                "Please ensure start dates and start times are earlier than the corresponding end date and end times",
                                MessageType.UserError);
                        }
                    }

                    if (msgs.HasNoErrors())
                    {
                        values = new Dictionary<string, object>
                        {
                            {"scheduledetails", scheduledetails},
                            {"days", days.Select(x => (int) x).ToStrings().ToStringList()},
                            {"repeatsweekly", repeatsweekly.IfBlank("0")},
                            {"startdate", startdate},
                            {"enddate", enddate},
                            {"dailystarttime", dailystarttime},
                            {"dailyendtime", dailyendtime},
                            {"scheduledates", inSessionDates},
                            {"scheduledatesclosed", outOfSessionDates},
                            {"schedule", schedule},
                            {"isongoing", isongoing},
                        };

                        activity = ActivityService.EditExisting(
                            loggedInUserId: currentUser.ID.Value,
                            account: currentAccount,
                            activityId: activity.ID.Value.ToString(),
                            values: values,
                            msgs: msgs
                        );
                    }
                }
                else if (activity.HasSchedule)
                {
                    var scheduleRemoved = activity.GetSchedule(msgs);
                    activity.RemoveActivitySchedule(currentUser, currentAccount, msgs);

                    // Show Success Message
                    msgs.Add($"Schedule removed from {activity.Name}", MessageType.Success);

                    // LOG: Someone Removed Schedule From for Activity AccountName
                    if (msgs.HasNoErrors())
                        SessionVariables.WebSession.LogActivity(
                            account: currentAccount,
                            actor: currentUser,
                            actionType: WebSessionActionType.Removed,
                            objectType: ObjectType.ActivitySchedule,
                            objectId: scheduleRemoved.ID.Value,
                            objectText: scheduleRemoved.Name,
                            indirectObjectType: ObjectType.Activity,
                            indirectObjectId: activity.ID.Value,
                            indirectObjectText: activity.Name
                        );
                }
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ActivitySchedule activitySchedule;

            if (msgs.HasNoErrors())
                activitySchedule = activity.GetSchedule(msgs: msgs);
            else
                activitySchedule = ActivitySchedule.GetNewDefaultSchedule(inSessionDates.Select(item => item.ToDateRange())
                    .ToList());

            if (msgs.HasNoErrors())
            {
                var isNew = Request[IsNewRequest] == "1";
                var fullScreenExitUrl = GetFullScreenExitUrl(activity);

                if (cmd == "next")
                {
                    switch (EditHelpers.GetNextScreenForActivityType(activity.ActivityTypeId, EditHelpers.EditSchedule))
                    {
                        case EditHelpers.EditDetails:
                            var isActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                                currentUser.IsActivityLeader(activity, msgs));
                            var editDetailsResult = new EditDetailsAssembler().AssembleEditDetailsResult(currentAccount, msgs, activity, fullScreenExitUrl,
                                isActivityLeader, currentAccountPackage, isNew);
                            return View("EditDetails", editDetailsResult);
                        case EditHelpers.EditContact:
                            var editContactResult = GatherDataForAndAssembleEditContactResult(id, currentUser, currentAccount, Request[IsNewRequest]);
                            return View("editcontact", editContactResult);
                    }
                }
            }

            return GoToEditSchedule(activity, activitySchedule?.ID?.ToString(), msgs, currentAccount, currentUser);
        }

        [HttpGet]
        public ActionResult EditLocation(string id)
        {
            var result = GatherDataForAndAssembleEditLocationResult(id, Request[IsNewRequest], new SystemMessages());
            return View(result);
        }

        private EditLocationResult GatherDataForAndAssembleEditLocationResult(string id, string isNew, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var currentUser = SessionVariables.CurrentUser;

            var accountUserType = SessionVariables.AccountUserType;
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var accountPackage = SessionVariables.AccountPackage;
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var location = activity.GetLocation() ?? activity.GetAccount().GetAccountProfile()?.GetLocation();
            return new EditLocationAssembler().AssembleEditLocationResult(currentAccount, activity, msgs, isNew, accountUserType,
                currentUserIsActivityLeader, fullScreenExitUrl, location, accountPackage, currentUser);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditLocation(string id, string location, string locationname, string locationdetails,
            string address1, string streetnumber, string streetname, string address2, string neighborhood, string city,
            string county, string state,
            string postalcode, string country, string placeid, decimal latitude, decimal longitude, string mylocation,
            string locationtype, string cmd, string activitylocationtype, string virtuallocation,
            string hidepreciselocation = "0")
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (location.IsEmpty())
                msgs.Add("Location is required", MessageType.UserError);

            if (activity.IsNotEmpty() && msgs.HasNoErrors())
            {
                var values = new Dictionary<string, object>
                {
                    {"location", location},
                    {"locationname", locationname},
                    {"locationdetails", locationdetails},
                    {"address1", address1},
                    {"streetnumber", streetnumber},
                    {"streetname", streetname},
                    {"address2", address2},
                    {"neighborhood", neighborhood},
                    {"city", city},
                    {"county", county},
                    {"state", state},
                    {"postalcode", postalcode},
                    {"country", country},
                    {"placeid", placeid},
                    {"latitude", latitude},
                    {"longitude", longitude},
                    {"mylocation", mylocation},
                    {"locationtype", locationtype},
                    {"activitylocationtype", activitylocationtype},
                    {"virtuallocation", virtuallocation},
                    {"hidepreciselocation", hidepreciselocation},
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }


            var isNew = Request[IsNewRequest];
            if (msgs.HasNoErrors() && cmd == "next")
            {
                switch (EditHelpers.GetNextScreenForActivityType(activity.ActivityTypeId, EditHelpers.EditLocation))
                {
                    case EditHelpers.EditCost:
                        var editCostResult = GatherDataForAndAssembleEditCostResult(activity, msgs, currentAccount, currentUser, isNew);
                        return View("EditCost", editCostResult);

                    default:
                        var fullScreenExitUrl = GetFullScreenExitUrl(activity);
                        var accountUserType = SessionVariables.AccountUserType;
                        var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);

                        var editConfirmationResult = new EditConfirmationAssembler().AssembleEditConfirmationResult(currentAccount, activity, accountUserType,
                            currentUserIsActivityLeader, isNew, fullScreenExitUrl, msgs);
                        return View("EditConfirmation", editConfirmationResult);
                }
            }

            var editLocationResult = GatherDataForAndAssembleEditLocationResult(id, isNew, msgs);
            return View(editLocationResult);
        }

        private EditCostResult GatherDataForAndAssembleEditCostResult(Activity activity, SystemMessages msgs, Account currentAccount, WebUser currentUser,
            string isNew)
        {
            if (msgs.IsEmpty())
                msgs = new SystemMessages();

            var accountPackage = SessionVariables.AccountPackage;
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var paymentPlan = activity.GetPaymentPlan(msgs);
            var subscriptionPlan = activity.GetSubscriptionPlan(msgs);
            var postPayPlan = activity.GetPostPayPlan(msgs);
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var accountUserType = SessionVariables.AccountUserType;
            return new EditCostAssembler().AssembleEditCostResult(activity, currentAccount.ID, accountUserType, currentUserIsActivityLeader, isNew,
                fullScreenExitUrl, msgs, subscriptionPlan, paymentPlan, postPayPlan, accountPackage);
        }

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            HandleTempMessages(msgs);
            var result = GatherDataForAndAssembleEditCostResult(activity, msgs, currentAccount, SessionVariables.CurrentUser, Request[IsNewRequest]);
            return View(result);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string acceptpaypal,
            string acceptcreditcard, string allowtopaylater, string cmd,
            string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod,
            string isongoingsubscription, string subscriptionPlanPeriodNumber, string subscriptionPaymentStartType,
            string subscriptionStartDate, string subscriptionEndDate,
            string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber,
            string paymentplanpaymentstarttype, string splitcostevenly, string regfeedownpayment,
            string haspostpayplan, string postpaycost, string isparentpricepostpay, string isparentpricecost,
            string isparentpricepaymentplan, string isparentpricesubscription, string prepaidperiodspostpay,
            string postpayplanperiod, string cutoffhours)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (id.IsGuid())
            {
                if (!isparentpricecost.IsNumber())
                    isparentpricecost = "0";

                if (!isparentpricepostpay.IsNumber())
                    isparentpricepostpay = "0";

                var values = new Dictionary<string, object>
                {
                    {"cost", cost},
                    {"costdescription", costdescription},
                    {"acceptpaypal", acceptpaypal.IfBlank("0")},
                    {"acceptcreditcard", acceptcreditcard.IfBlank("0")},
                    {"allowtopaylater", allowtopaylater.IfBlank("0")},
                    {"hassubscriptionplan", hassubscriptionplan.IfBlank("0")},
                    {"subscriptionplancost", subscriptionplancost},
                    {"subscriptionplanperiod", subscriptionplanperiod},
                    {"subscriptionPlanPeriodNumber", subscriptionPlanPeriodNumber},
                    {"subscriptionPaymentStartType", subscriptionPaymentStartType},
                    {"subscriptionStartDate", subscriptionStartDate},
                    {"subscriptionEndDate", subscriptionEndDate},
                    {"isongoingsubscription", !activity.HasSchedule},
                    {"haspaymentplan", haspaymentplan.IfBlank("0")},
                    {"paymentplancost", paymentplancost},
                    {"paymentplanperiod", paymentplanperiod},
                    {"paymentplanperiodnumber", paymentplanperiodnumber},
                    {"paymentplanpaymentstarttype", paymentplanpaymentstarttype},
                    {"splitcostevenly", splitcostevenly.IsBoolean().ToString()},
                    {"regfeedownpayment", regfeedownpayment.IsBoolean().ToString()},
                    {"haspostpayplan", haspostpayplan.IfBlank("0")},
                    {"postpaycost", postpaycost},
                    {"isparentpricepostpay", isparentpricepostpay},
                    {"isparentpricecost", isparentpricecost},
                    {"isparentpricepaymentplan", isparentpricepaymentplan},
                    {"isparentpricesubscription", isparentpricesubscription},
                    {"prepaidperiodspostpay", prepaidperiodspostpay},
                    {"postpayplanperiod", postpayplanperiod},
                };
                if (!activity.IsPublished)
                {
                    activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
                }
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            var isNew = Request[IsNewRequest];
            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "next":
                    case "onlynext":
                        var fullScreenExitUrl = GetFullScreenExitUrl(activity);
                        var accountUserType = SessionVariables.AccountUserType;
                        var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);

                        var editConfirmationResult = new EditConfirmationAssembler().AssembleEditConfirmationResult(currentAccount, activity, accountUserType,
                            currentUserIsActivityLeader, isNew, fullScreenExitUrl, msgs);
                        return View("EditConfirmation", editConfirmationResult);
                    case "done":
                        ViewBag.Account = currentAccount;
                        ViewBag.AccountPackage = SessionVariables.AccountPackage;
                        ViewBag.IsFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");
                        ViewBag.FullScreenExitUrl = GetFullScreenExitUrl(activity);
                        ViewBag.Messages = msgs;
                        return RedirectToAction("activities", "group", new { id = activity.ActivityGroupUid });
                    case "publish":
                        ViewBag.Account = currentAccount;
                        ViewBag.AccountPackage = SessionVariables.AccountPackage;
                        ViewBag.IsFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");
                        ViewBag.FullScreenExitUrl = GetFullScreenExitUrl(activity);
                        ViewBag.Messages = msgs;
                        return RedirectToAction("publish", "activity",
                            new { id = activity.Uid, isnew = isNew, source = Request["source"] });
                }
            }

            var result = GatherDataForAndAssembleEditCostResult(activity, msgs, currentAccount, currentUser, isNew);
            return View(result);
        }

        [HttpGet]
        public ActionResult EditConfirmation(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var currentUser = SessionVariables.CurrentUser;
            var isNew = Request[IsNewRequest];
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);
            var accountUserType = SessionVariables.AccountUserType;
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);

            var result = new EditConfirmationAssembler().AssembleEditConfirmationResult(currentAccount, activity, accountUserType, currentUserIsActivityLeader,
                isNew, fullScreenExitUrl, msgs);

            return View(result);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditConfirmation(string id, string registrationthankyouemail,
            string applicationthankyouemail, string cmd)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (id.IsGuid())
            {
                var values = new Dictionary<string, object>
                {
                    {"registrationthankyouemail", registrationthankyouemail},
                    {"applicationthankyouemail", applicationthankyouemail},
                };

                activity = ActivityService.EditExisting(
                    loggedInUserId: currentUser.ID.Value,
                    account: currentAccount,
                    activityId: activity.ID.Value.ToString(),
                    values: values,
                    msgs: msgs
                );
            }

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Saved", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Updated,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            var isNew = Request[IsNewRequest];
            if (msgs.HasNoErrors())
            {
                ViewBag.Account = currentAccount;
                ViewBag.IsFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");
                ViewBag.FullScreenExitUrl = GetFullScreenExitUrl(activity);
                switch (cmd)
                {
                    case "publish":
                        msgs.AddRange(activity.GetItemsRequiredToPublish(msgs));
                        if (msgs.HasErrors())
                        {
                            ViewBag.Messages = msgs;
                            return RedirectToAction("PublishDetail", new { id = activity.Uid.ToString() });
                        }

                        ViewBag.Messages = msgs;
                        return Redirect(activity.ShortHomePageUrl);
                    case "publish-detail":
                        ViewBag.Messages = msgs;
                        return Redirect(activity.ShortHomePageUrl);

                    default:
                        ViewBag.Messages = msgs;
                        return RedirectToAction("requesttopublish",
                            new { id = activity.Uid, isnew = isNew, source = Request["source"] });
                }
            }

            var accountUserType = SessionVariables.AccountUserType;
            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var fullScreenExitUrl = GetFullScreenExitUrl(activity);

            var result = new EditConfirmationAssembler().AssembleEditConfirmationResult(currentAccount, activity, accountUserType, currentUserIsActivityLeader,
                isNew, fullScreenExitUrl, msgs);
            return View(result);
        }

        #endregion

        #region Copy, Delete, Archive, Restore, Pinned

        [HttpPost]
        public ActionResult Copy(string id, string activitynamecopy, string usesameformcopy, string copywithregistrants,
            string type)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activityToCopy = currentAccount.GetActivity(id.ToGuid(), msgs);

            var activity = activityToCopy.CopyWithoutSchedule(
                activitynamecopy,
                true,
                currentUser.ID.Value,
                msgs);

            // Show Success Message
            // LOG: Someone Updated Activity ActivityName for Account AccountName
            // Redirect to New Copied Activity Edit
            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityLabel(activity.ActivityTypeId)} Copied", MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Copied,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );

                ViewBag.Messages = msgs;
                return RedirectToAction("editsetup", new { id = activity.Uid, isnew = 1, source = Request["source"] });
            }

            ViewBag.Messages = msgs;
            return RedirectToAction("index", "activity", new { copyerror = true, type });
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MarkAsDeleted(currentUser.ID.Value));

            // Show Success Message
            // LOG: Someone has Deleted Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Deleted,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Archive(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveToArchive(currentUser.ID.Value));

            // Show Success Message
            // LOG: Someone Archived Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been archived", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Archived,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsNotEmpty())
                msgs.AddRange(activity.MoveFromArchive(currentUser.ID.Value));

            // Show Success Message
            // LOG: Someone Restored Activity ActivityName for Account AccountName
            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been restored", activity.Name), MessageType.Success);

                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Restored,
                    objectType: ObjectType.Activity,
                    objectId: activity.ID.Value,
                    objectText: activity.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "activity");
        }

        [HttpGet]
        public JsonResult PinFeatureActivity(string id)
        {
            var msgs = new SystemMessages();

            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            if (activity.IsEmpty())
                msgs.Add("Activity not found.", MessageType.Error);

            msgs.AddRange(activity.SetIsFeatured(!activity.IsFeatured, (int) currentAccount?.ID, msgs));

            // This will return True if during switching of IsFeatured; we faced no problems.
            if (msgs.HasNoErrors()) 
                return Json(new
                {
                    status = "ok",
                    isFeaturedNow = activity.IsFeatured,
                }, JsonRequestBehavior.AllowGet);

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Attendance

        [HttpGet]
        public ActionResult Attendance(string id, string orderby, string direction, string pn, string rpp, string msg)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            if (msg.IsNotEmpty())
                msgs.Add(msg, msg.Contains("error") || msg.Contains("Error") ? MessageType.UserError : MessageType.Success);

            int pageNumber = 1;
            int recordsPerPage = 10;
            int totalAttendanceCount = 0;
            int totalPages = 0;
            string orderByParam = "Date";
            OrderByDirectionEnum orderbyDirection = OrderByDirectionEnum.Descending;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (orderby.IsNotBlank())
                orderByParam = orderby;
            if (direction.IsNotBlank())
                orderbyDirection = (OrderByDirectionEnum)Enum.Parse(typeof(OrderByDirectionEnum), direction);

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            List<Attendance> attendances = null;

            if (activity.IsNotEmpty())
            {
                attendances =
                    activity.GetCompletedAttendanceWithPaging(orderByParam, orderbyDirection, start, length, msgs);
                totalAttendanceCount = activity.GetTotalAttendanceCount(msgs);

                if (totalAttendanceCount > 0)
                    totalPages = Math.Ceiling(totalAttendanceCount.ToDouble() / recordsPerPage).ToInteger();
            }

            var currentUser = SessionVariables.CurrentUser;
            ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Attendances = attendances;
            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = totalAttendanceCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            ViewBag.Messages = msgs;

            return View(activity);
        }

        [HttpGet]
        public ActionResult TrackAttendance(string id, string att, string actinst)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var attendanceId = att.IsNumber() ? att.ToInteger() : new int?();
            var activityInstanceId = actinst.IsNumber() ? actinst.ToInteger() : new int?();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var attendance = activity.GetOrCreateNewAttendance(attendanceId, activityInstanceId, currentUser, msgs);
            if (attendance.IsEmpty())
                return RedirectToAction("Attendance", new { id, msg = "Error: No classes have taken place yet" });

            var activitySchedule = activity.GetSchedule(msgs);
            var attendanceRegistrants = GetAttendanceRegistrants(attendance, activity, activitySchedule, msgs);

            ViewBag.AttendanceRegistrants = attendanceRegistrants;
            ViewBag.Schedule = activitySchedule;
            ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            ViewBag.RegistrantActivities = activity.GetRegistrantActivities(msgs);
            ViewBag.Messages = msgs;
            return View(attendance);
        }

        private List<AttendanceRegistrant> GetAttendanceRegistrants(Attendance attendance, Activity activity,
            ActivitySchedule schedule, SystemMessages msgs)
        {
            var attendanceRegistrants = attendance.GetAttendanceRegistrants(msgs);

            if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerClass)
            {
                var arsToRemove = new List<AttendanceRegistrant>();
                attendanceRegistrants.ForEach(ar =>
                {
                    var ra = activity.GetRegistrantActivity(ar.RegistrantId.Value, msgs);
                    var instance = schedule.GetInSessionInstanceByDate(attendance.Date.Value, msgs);
                    if (ra.GetInstanceSeats(instance, msgs).IsEmpty())
                        arsToRemove.Add(ar);
                });

                arsToRemove.ForEach(ar => attendanceRegistrants.Remove(ar));
            }

            return attendanceRegistrants;
        }

        [HttpPost]
        public ActionResult TrackAttendance(string id, string att)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var presentStudents = new List<int>();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var attendance = AfterSchoolLibrary.Models.Attendance.Load(att.ToInteger(), msgs);
            var activitySchedule = activity.GetSchedule(msgs);

            attendance.GetAttendanceRegistrants(msgs).ForEach(attendanceRegistrant =>
            {
                if (Request.Params[$"present-{attendanceRegistrant.ID.ToString()}"].IsNumber())
                    presentStudents.Add(attendanceRegistrant.ID.Value);
            });

            if (msgs.HasNoErrors())
                attendance.UpdateAttendance(attendance.ActivityInstanceId.Value, presentStudents,
                    SessionVariables.CurrentUser, msgs);

            ViewBag.AttendanceRegistrants = GetAttendanceRegistrants(attendance, activity, activitySchedule, msgs);
            ViewBag.Schedule = activitySchedule;
            var currentUser = SessionVariables.CurrentUser;
            ViewBag.IsActivityLeader = EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType,
                currentUser.IsActivityLeader(activity, msgs));
            ViewBag.RegistrantActivities = activity.GetRegistrantActivities(msgs);
            ViewBag.Messages = msgs;

            return View(attendance);
        }

        // This returns all unbilled selected Attendances by Registrant
        private Dictionary<RegistrantActivity, List<Attendance>> GetAttendancesToBillPerRegistrantActivity(
            Account currentAccount, List<RegistrantActivity> registrantActivities, List<Attendance> attendances,
            BillingPeriodType billingPeriodType, SystemMessages msgs)
        {
            Dictionary<RegistrantActivity, List<Attendance>> registrantActivityAttendanceCounts =
                new Dictionary<RegistrantActivity, List<Attendance>>();

            registrantActivities.ForEach(registrantActivity =>
            {
                //.. special case: post pay plan with # of Pre-pay
                var activity = registrantActivity.GetActivity(msgs);
                var postPayPlan = activity.GetPostPayPlan(msgs);
                var attendancesToCharge = new List<Attendance>();

                switch (postPayPlan.BillingPeriodTypeId)
                {
                    case BillingPeriodType.Day:

                        attendancesToCharge = attendances.FindAll(attendance =>
                        {
                            var attendanceRegistrant = currentAccount.GetAttendanceRegistrant(activity,
                                attendance.ID.Value, registrantActivity.RegistrantId.Value, msgs);
                            if (attendanceRegistrant.IsNotEmpty() && !attendanceRegistrant.IsAlreadyBilled)
                                return attendanceRegistrant.NumberPresent > 0;

                            return false;
                        }).ToList();

                        registrantActivityAttendanceCounts.Add(registrantActivity, attendancesToCharge);

                        break;

                    case BillingPeriodType.Week:

                        attendances.ForEach(attendance =>
                        {
                            var currentWeekPresent = currentAccount.GetAttendanceRegistrantByDateRange(
                                    activity,
                                    registrantActivity.RegistrantId.Value,
                                    new DateRange(
                                        attendance.Date.Value.FirstDateOfWeek().Date,
                                        attendance.Date.Value.LastDateOfWeek().Date.AddDays(1).AddMilliseconds(-1)
                                    ),
                                    msgs)
                                .Where(item => item.NumberPresent > 0)
                                .OrderBy(item => item.AttendanceDate.Value)
                                .ToList();

                            if (currentWeekPresent.Any() && !currentWeekPresent.Any(item => item.IsAlreadyBilled))
                            {
                                var firstAttendanceOfWeek = currentWeekPresent.First().GetAttendance(msgs);

                                var currentUser = SessionVariables.CurrentUser;
                                var attendanceRegistrant = currentAccount.GetAttendanceRegistrant(activity,
                                    attendance.ID.Value, registrantActivity.RegistrantId.Value, msgs);

                                if (attendanceRegistrant.IsNotEmpty() && firstAttendanceOfWeek.IsNotEmpty() &&
                                    !attendanceRegistrant.IncludeNextAvailablePrePayment(currentUser, msgs) &&
                                    !attendancesToCharge.Any(item => item.ID == firstAttendanceOfWeek.ID))
                                    attendancesToCharge.Add(firstAttendanceOfWeek);
                            }
                        });

                        registrantActivityAttendanceCounts.Add(registrantActivity, attendancesToCharge);
                        break;
                }
            });

            return registrantActivityAttendanceCounts;
        }

        // This returns billed Attendances and the count of Registrants Billed for billing period
        private Dictionary<Attendance, List<int>> GetNumberOfRegistrantsBilledPerAttendance(
            List<Attendance> attendances, BillingPeriodType billingPeriodType, SystemMessages msgs = null)
        {
            var numberOfRegistrantsBilledPerAttendance = new Dictionary<Attendance, List<int>>();

            attendances.ForEach(attendance =>
            {
                var registrantIdsBilled = AttendanceRegistrant.LoadByAttendanceId(attendance.ID.Value, msgs)
                    .Where(ar => ar.IsAlreadyBilled).Select(rb => rb.RegistrantId.Value).Distinct().ToList();

                switch (billingPeriodType)
                {
                    case BillingPeriodType.Day:
                        if (numberOfRegistrantsBilledPerAttendance.ContainsKey(attendance))
                        {
                            registrantIdsBilled.ForEach(rbId =>
                            {
                                if (!numberOfRegistrantsBilledPerAttendance[attendance].Contains(rbId))
                                    numberOfRegistrantsBilledPerAttendance[attendance].Add(rbId);
                            });
                        }
                        else
                            numberOfRegistrantsBilledPerAttendance.Add(attendance, registrantIdsBilled);

                        break;

                    case BillingPeriodType.Week:
                        var attendanceIsInSameWeekAsExisitingAttendanceInList = numberOfRegistrantsBilledPerAttendance
                            .FirstOrDefault(keyPair => keyPair.Key.Date.Value.DatesAreInSameWeek(attendance.Date.Value))
                            .Key;
                        if (attendanceIsInSameWeekAsExisitingAttendanceInList.IsNotEmpty())
                        {
                            registrantIdsBilled.ForEach(rbId =>
                            {
                                if (!numberOfRegistrantsBilledPerAttendance[
                                        attendanceIsInSameWeekAsExisitingAttendanceInList].Contains(rbId))
                                    numberOfRegistrantsBilledPerAttendance[
                                        attendanceIsInSameWeekAsExisitingAttendanceInList].Add(rbId);
                            });
                        }
                        else
                            numberOfRegistrantsBilledPerAttendance.Add(attendance, registrantIdsBilled);

                        break;

                    default:
                        break;
                }
            });

            return numberOfRegistrantsBilledPerAttendance;
        }

        [HttpPost]
        public ActionResult ChargeAttendanceFee()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            // this should return the amount to be charged for each registrant 
            var attendanceIds = Request.Form["AttendanceIds"];
            var activityId = Request.Form["Uid"];

            Activity activity = null;

            if (activityId.IsNotEmpty() && activityId.IsGuid())
                activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

            if (activity.IsNotEmpty() && (attendanceIds.IsNotEmpty() || attendanceIds.IsNotBlank()))
            {
                // get selected attendances
                var attendanceIdsList = attendanceIds.Split(',').Select(p => p.ToInteger()).ToList();
                var attendances = AfterSchoolLibrary.Models.Attendance.LoadByActivityId(activity.ID.Value, msgs);
                var selectedAttendances = attendances.Where(a => attendanceIdsList.Contains(a.ID.Value)).ToList();

                // sum up total cost of all attendances per registrant to display
                var registrantActivities =
                    RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity,
                        msgs);
                var billingPeriodType = activity.GetPostPayPlan(msgs).BillingPeriodTypeId;

                ViewBag.AttdanceIds = attendanceIds;
                ViewBag.BillingPeriodType = billingPeriodType;
                ViewBag.AttendancesToBillRegistrants = GetAttendancesToBillPerRegistrantActivity(currentAccount,
                    registrantActivities, selectedAttendances, billingPeriodType, msgs);
                ViewBag.Messages = msgs;

                return View(activity);
            }

            return RedirectToAction("Attendance");
        }

        [HttpPost]
        public ActionResult ChargeAttendanceFeeDone()
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            // this should make the transaction purchase and activity payment records
            //and send charges for each registrant to stripe
            var attendanceIds = Request.Form["AttendanceIds"];
            var activityId = Request.Form["Uid"];

            Activity activity = null;
            var defaultMsg =
                "An error occurred while processing payments. Please contact support, do not attempt to charge attendance again";
            var msg = defaultMsg;

            if (activityId.IsNotEmpty() && activityId.IsGuid())
                activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

            if (activity.IsNotEmpty() && (attendanceIds.IsNotEmpty() || attendanceIds.IsNotBlank()))
            {
                // get selected attendances
                var attendanceIdsList = attendanceIds.Split(',').Select(p => p.ToInteger()).ToList();
                var attendances = AfterSchoolLibrary.Models.Attendance.LoadByActivityId(activity.ID.Value, msgs);
                var selectedAttendances = attendances.Where(a => attendanceIdsList.Contains(a.ID.Value)).ToList();

                // sum up total cost of all attendances per registrant to display
                var registrantActivities =
                    RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity,
                        msgs);
                var postPayPlan = activity.GetPostPayPlan(msgs);
                var billingPeriodType = postPayPlan?.BillingPeriodTypeId;
                var registrantActivityAttendances = GetAttendancesToBillPerRegistrantActivity(
                    currentAccount,
                    registrantActivities,
                    selectedAttendances,
                    billingPeriodType.HasValue ? billingPeriodType.Value : BillingPeriodType.NotSet,
                    msgs);

                foreach (var registrantActivityAttendance in registrantActivityAttendances)
                {
                    // one person multiple attendance dates
                    var registrantActivity = registrantActivityAttendance.Key;
                    var registration = registrantActivity.GetRegistration(msgs);
                    var costPerAttendance =
                        registration.CostPerClass.IsNotEmpty() ? registration.CostPerClass.Value : 0;

                    if (costPerAttendance > 0)
                    {
                        var masterRegistrant = registration.GetMasterRegistrant(msgs);
                        var raAttendances = registrantActivityAttendance.Value;
                        var masterRegistrantAccount = masterRegistrant.GetAccount(msgs);
                        var description =
                            $"{registrantActivity.GetActivity(msgs).Name.ToProperCase()} attendance Fee times {raAttendances.Count}";
                        var convenienceFeePayer = registration.GetConvenienceFeePayer(msgs);
                        var paymentTypeFeePayer = registration.GetPaymentTypeFeePayer(msgs);
                        var availablePrePayments = registration.GetAvailablePrePayments(activity, msgs);
                        var attendanceDaysToCharge = Math.Max(0, raAttendances.Count - availablePrePayments.Count);
                        CheckoutObject checkoutObject = null;
                        var coupon = CouponService.GetCoupon(registration, msgs);
                        
                        BankAccountPurchaseObject bankAccountPurchaseObject = null;
                        CreditCardPurchaseObject creditCardPurchaseObject = null;

                            if (registration.PaymentTypeId == PaymentType.ACH)
                                bankAccountPurchaseObject = new BankAccountPurchaseObject
                                { StripeSourceId = registration.StripeSourceId };
                            else
                                creditCardPurchaseObject = new CreditCardPurchaseObject
                                { StripeSourceId = registration.StripeSourceId };

                        var overrideAmount = costPerAttendance * attendanceDaysToCharge;

                        if (postPayPlan.IsNotEmpty())
                        {
                            //.. if post pay plan, then we will have coupon value in registration to discount.
                            var couponDiscount = attendanceDaysToCharge * (registration.CouponValue.HasValue
                                ? registration.CouponValue.Value
                                : 0);
                            overrideAmount = (costPerAttendance * attendanceDaysToCharge) - couponDiscount;
                        }

                        checkoutObject = new CheckoutObject
                        {
                            Coupon = coupon,
                            OverrideAmount = overrideAmount,
                            ConvenienceFeeOverrideAmount =
                                registration.ConvenienceFeePaid.IfEmpty(0) * attendanceDaysToCharge,
                            PaymentTypeFeeOverrideAmount =
                                registration.PaymentTypeFeePaid.IfEmpty(0) * attendanceDaysToCharge,
                            PayingAccount = masterRegistrantAccount,
                            BillingAccount = activity.GetAccount(msgs),
                            CreditCardPurchaseObject = creditCardPurchaseObject,
                            BankAccountPurchaseObject = bankAccountPurchaseObject,
                            PaymentType = bankAccountPurchaseObject.IsNotEmpty()
                                ? PaymentType.ACH
                                : PaymentType.CreditCard,
                            PaymentSourceType = PaymentSourceType.AfterSchoolPlatform,
                            ConvenienceFeePayer = convenienceFeePayer,
                            CreditCardFeePayer = paymentTypeFeePayer,
                            MasterRegistrant = masterRegistrant,
                            Description = description,
                            SavePaymentInfo = false,
                            ParticipationObject = activity.GetParticipationObject(msgs),
                            Quantity = attendanceDaysToCharge,
                            Registration = registration,
                            PostPayPlan = postPayPlan,
                        };
 
                            
                            if (msgs.HasNoErrors())
                            raAttendances.ForEach(raAttendance =>
                            {
                                var attendanceRegistrant = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(
                                        raAttendance.ID.Value,
                                        registrantActivity.GetRegistrant(msgs).ID.Value,
                                        msgs)
                                    .FirstOrDefault();
                                
                                if(attendanceRegistrant != null && attendanceRegistrant.NumberPresent > 0)
                                    AccountService.ProcessPayment(
                                        checkoutObject,
                                        currentUser,
                                        true,
                                        attendanceDaysToCharge,
                                        selectedAttendances.FirstOrDefault()?.Date,
                                        registrantActivity.RegistrantName,
                                        msgs);

                                // add next available pre payment if available, if not then continue to set the checkoutobject transaction id
                                if (attendanceRegistrant.IsNotEmpty()
                                    && !attendanceRegistrant.IncludeNextAvailablePrePayment(currentUser, msgs)
                                    && (checkoutObject?.Transaction).IsNotEmpty()
                                    && !attendanceRegistrant.IsAlreadyBilled)
                                {
                                    attendanceRegistrant.TransactionId = checkoutObject.Transaction.ID.Value;

                                    attendanceRegistrant
                                        .GetAttendanceRegistrantsInSameBillingPeriod(postPayPlan.BillingPeriodTypeId,
                                            msgs)
                                        .ForEach(ar =>
                                        {
                                            ar.TransactionId = checkoutObject.Transaction.ID.Value;
                                            msgs.AddRange(ar.Save());
                                        });

                                    msgs.AddRange(attendanceRegistrant.Save());
                                }
                            });

                        if (msgs.HasErrors())
                        {
                            msg =
                                $"An error occured while charging some parents, some parents may have already been processed and you will still receive a summary email if any have. Please contact support and do NOT attempt to charge for attendance again.";
                            break;
                        }
                    }
                }

                if (msgs.HasNoErrors() && msg == defaultMsg)
                {
                    selectedAttendances.ForEach(attendance =>
                    {
                        attendance.IsCharged = true;
                        msgs.AddRange(attendance.Save());
                    });

                    msg = "All accounts have been successfully charged.";
                }

                // Send summary email of charged AttendanceRegistrants to Activity Provider if there were any successful charges
                var registrantsBilledPerAttendance = GetNumberOfRegistrantsBilledPerAttendance(
                    selectedAttendances,
                    billingPeriodType.HasValue ? billingPeriodType.Value : BillingPeriodType.NotSet,
                    msgs);
                var objRegistration = registrantActivities.First().GetRegistration(msgs);
                var costPerClass = objRegistration.CostPerClass.HasValue ? objRegistration.CostPerClass.Value : 0;
                var couponValue = objRegistration.CouponValue.HasValue ? objRegistration.CouponValue.Value : 0;

                if (registrantsBilledPerAttendance.Count > 0)
                    EmailHelper.SendAttendanceChargeConfirmation(registrantsBilledPerAttendance, activity, costPerClass,
                        couponValue, null, SessionVariables.CurrentUser, false, msgs);
            }

            ViewBag.Messages = msgs;
            return RedirectToAction("attendance", new { id = activityId, msg = msg });
        }

        [HttpGet]
        public ActionResult AttendanceReport(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var attendanceRegistrants = AttendanceRegistrant.LoadByActivityId(activity.ID.Value, msgs);

            var model = new AttendanceReportResults()
            {
                AttendanceRegistrants = attendanceRegistrants,
                Messages = msgs,
                Activity = activity,
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult DownloadAttendanceReport(string id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            var attendanceRegistrants = AttendanceRegistrant.LoadByActivityId(activity.ID.Value, msgs);

            var table = new List<List<string>>();
            var row = new List<string>
            {
                "Date",
                "Student First Name",
                "Student Last Name",
                "Status",
            };

            table.Add(row);

            foreach (var attendanceRegistrant in attendanceRegistrants)
            {
                row = new List<string>
                {
                    attendanceRegistrant.AttendanceDate.ToShortDateTimeString(),
                    attendanceRegistrant.RegistrantFirstName,
                    attendanceRegistrant.RegistrantLastName,
                    attendanceRegistrant.NumberPresent.HasValue ? attendanceRegistrant.NumberPresent.Value > 0 ? "Present" : "Absent" : "N/A",
                };
                table.Add(row);
            }

            var csv = CsvFile.Create(table);

            Response.Clear();
            Response.AddHeader("Content-Type", BasicLibrary.Utilities.GetMimeType("csv"));
            Response.AddHeader("Content-Disposition", $"attachment; filename={activity.UniqueName}-attendance.csv");

            var fileBytes = Encoding.UTF8.GetBytes(csv);
            Response.AddHeader("Content-Length", fileBytes.Length.ToString());
            Response.BinaryWrite(fileBytes);
            Response.End();

            var model = new AttendanceReportResults()
            {
                AttendanceRegistrants = attendanceRegistrants,
                Messages = msgs,
                Activity = activity,
            };

            return View("AttendanceReport", model);

        }

        public class AttendanceReportResults
        {
            public AttendanceReportResults() { }

            public List<AttendanceRegistrant> AttendanceRegistrants { get; set; }
            public SystemMessages Messages { get; set; }
            public Activity Activity { get; set; }
        }

        #endregion

        #region Surveys

        [HttpGet]
        public ActionResult Surveys(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = TempData["msgs"].IsNotEmpty() ? (SystemMessages)TempData["msgs"] : new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

            ViewBag.Surveys = SurveyRequest.LoadByActivityId(activity.ID.Value, msgs)
                .OrderByDescending(a => a.CreateDate).ToList();
            ViewBag.Messages = msgs;
            return View(activity);
        }


        [HttpGet]
        public ActionResult NewSurvey(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = TempData["msgs"].IsNotEmpty() ? (SystemMessages)TempData["msgs"] : new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var activityGroup = activity.GetActivityGroup(currentAccount.ID.Value, msgs);
            var activityRegistrants = activity.GetRegistrantActivitiesForSurvey(msgs);
            var studentList = activityRegistrants.Select(m => new SelectListItem
            {
                Text = m.RegistrantName,
                Value = m.RegistrantId.Value.ToString(),
            });

            var isNew = Request[IsNewRequest];

            var currentUser = SessionVariables.CurrentUser;
            currentAccount.EnsureDefaultForm(activity, currentUser, msgs);
            var activeForm = activity.GetActiveActivityForm(msgs) ?? activityGroup?.GetDefaultActivityForm(msgs) ??
                currentAccount.GetDefaultActivityForm(activity, msgs);
            var periods = currentAccount.GetOpenAndUpcomingRegistrationPeriods(msgs);
            var activePeriod = activity.GetActiveRegistrationPeriod(msgs) ??
                               currentAccount.GetDefaultNewRegistrationPeriod();

            var currentUserIsActivityLeader = currentUser.IsActivityLeader(activity, msgs);
            var fullScreenExitUrl = GetFullScreenExitUrl(activity, activityGroup?.Uid.ToString());
            var forms = currentAccount.GetSurveyForms(false, msgs).OrderBy(x => x.Name).ToList();

            periods.Add(activePeriod);
            periods = periods.OrderBy(period => period.StartDateTime.Value).ToList();

            var formsList = forms.OrderBy(x => x.Name).ToList();
            var isActivityLeader =
                EditHelpers.IsActivityLeader(currentAccount.ID, activity.AccountId, SessionVariables.AccountUserType, currentUserIsActivityLeader);
            var isFullScreen = EditHelpers.GetIsFullScreen(activity, isNew == "1");

            ViewBag.Group = activityGroup;
            ViewBag.Account = currentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = formsList;
            ViewBag.Periods = periods;
            ViewBag.IsActivityLeader = isActivityLeader;
            ViewBag.ActivePeriod = activePeriod;
            ViewBag.ActiveForm = activeForm;
            ViewBag.IsFullScreen = isFullScreen;
            ViewBag.Messages = msgs;
            ViewBag.FullScreenExitUrl = fullScreenExitUrl;

            ViewBag.Students = studentList.ToList();
            ViewBag.Messages = msgs;

            return View(activity);
        }


        [HttpPost]
        public ActionResult SaveSurvey(string id, string activityId, string formId, string name, string optionalText,
            string dueDate, string isEveryIndividualSelected, string isAllowMultiple)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;

            var students = ToDictionary(Request.Form.Filter("studentList"));
            var studentList = ConvertToStudentsList(students);

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            if (name.IsEmpty())
                msgs.Add("Survey title is required", MessageType.Error);

            if (!studentList.Any(a => a.Selected) && Request.Form["students"].ToString() == "false")
                msgs.Add("Select Students", MessageType.Error);

            if (msgs.HasNoErrors())
            {
                var surveyRequest = SurveyRequestService.AddSurvey(formId.ToInteger(), activityId.ToInteger(), name,
                    dueDate, optionalText, isAllowMultiple.ToBoolean(), currentAccount.ID.Value, currentUser.ID.Value,
                    msgs);

                if (isEveryIndividualSelected.ToBoolean())
                    CreateSurveys(surveyRequest.ID.Value, studentList.Where(a => a.Selected).ToList(),
                        currentUser.ID.Value, msgs);
                else
                    CreateSurveys(surveyRequest.ID.Value, studentList, currentUser.ID.Value, msgs);

                if (msgs.HasNoErrors())
                {
                    msgs.Add("Survey notifications sent successfully<br/>", MessageType.Success);

                    TempData["msgs"] = msgs;
                    return RedirectToAction("Surveys", new { id, msgs });
                }
                else
                {
                    TempData["msgs"] = msgs;
                    return RedirectToAction("NewSurvey", new { id, msgs });
                }
            }
            else
            {
                TempData["msgs"] = msgs;
                return RedirectToAction("NewSurvey", new { id, msgs });
            }
        }

        private static IDictionary<string, string> ToDictionary(NameValueCollection source)
        {
            return source.Cast<string>().Select(s => new { Key = s, Value = source[s] })
                .ToDictionary(p => p.Key, p => p.Value);
        }

        private static List<SelectListItem> ConvertToStudentsList(IDictionary<string, string> students)
        {
            var studentsList = new List<SelectListItem>();
            for (var i = 0; i < students.Count();)
            {
                studentsList.Add(new SelectListItem
                {
                    Text = students.ElementAt(i + 2).Value,
                    Value = students.ElementAt(i + 1).Value,
                    Selected = students.ElementAt(i).Value.Contains("true"),
                });
                i = i + 3;
            }

            return studentsList;
        }

        private static void CreateSurveys(int surveyRequestId, List<SelectListItem> studentList, int currentUserId,
            SystemMessages msgs, DatabaseConnection dc = null)
        {
            studentList.ForEach(student =>
            {
                var surveyResponse = SurveyResponseService.CreateSurveyFeedback(surveyRequestId,
                    student.Value.ToInteger(), SurveyFeedbackStatus.New, currentUserId, msgs);
                var masterRegistrant = Registrant.Load(student.Value.ToInteger())?.GetMasterRegistrant(msgs, dc);

                if (masterRegistrant.IsNotEmpty() && masterRegistrant.WebUserId.IsEmpty())
                {
                    var inviteCode = RegistrantInviteCodeService.GetInviteCode(
                        surveyResponseId: surveyResponse.ID.Value,
                        registrantId: surveyResponse.RegistrantId.Value,
                        currentUserId: currentUserId,
                        msgs: msgs,
                        dc: dc);

                    BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value,
                        surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, inviteCode.Code,
                        inviteCode.ID.Value, null));
                }
                else
                    BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value,
                        surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, null, 0, null));
            });
        }

        [HttpGet]
        public ActionResult ResendNotification(int id, int surveyId)
        {
            if (id == 0)
            {
                var surveyResponses = SurveyResponse.LoadBySurveyRequestId(surveyId)
                    .Where(a => a.SurveyFeedbackStatusTypeId != SurveyFeedbackStatus.Completed).ToList();
                surveyResponses.ForEach(surveyResponse => { CheckRegistrantInviteCodeAndSendEmail(surveyResponse); });
            }
            else
            {
                var surveyResponse = SurveyResponse.Load(id);
                CheckRegistrantInviteCodeAndSendEmail(surveyResponse);
            }

            return Json(new { msg = "Reminder sent", success = true }, JsonRequestBehavior.AllowGet);
        }

        private void CheckRegistrantInviteCodeAndSendEmail(SurveyResponse surveyResponse)
        {
            var checkIfInviteCodeExists =
                RegistrantInviteCode.CheckBySurveyResponseIdExpiryDate(surveyResponse.ID.Value, DateTime.Now);
            if (checkIfInviteCodeExists.IsNotEmpty())
                BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value,
                    surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value,
                    checkIfInviteCodeExists.Code, checkIfInviteCodeExists.ID.Value, null));
            else
                BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value,
                    surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, null, 0, null));
        }

        [HttpGet]
        public ActionResult ViewSurvey(int id, string activityId)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

            ViewBag.GetSurveyFeedback = SurveyResponse.LoadBySurveyRequestId(id, msgs)
                .OrderByDescending(a => a.CreateDate).ToList();
            ViewBag.Survey = SurveyRequest.Load(id);
            ViewBag.Messages = msgs;

            return View(activity);
        }

        public FileResult ExportCsv(int surveyRequestId)
        {
            var msgs = new SystemMessages();

            var csv = SurveyRequestService.ExportSurveyRequestToCsvText(surveyRequestId, msgs);
            var fileData = Encoding.UTF8.GetBytes(csv);

            return File(fileData, "application/octet-stream", "ExportSurvey.csv");
        }

        [HttpGet]
        public ActionResult ViewSurveyResults(int id, string activityId)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            var getSurveyResponse = SurveyResponse.Load(id, msgs);
            var activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

            var getSurveyResponses = SurveyResponse.LoadBySurveyRequestId(getSurveyResponse.SurveyRequestId.Value)
                .Where(a => a.SurveyFeedbackStatusTypeId == SurveyFeedbackStatus.Completed).ToList();

            var findIndex = getSurveyResponses.FindIndex(a => a.ID == getSurveyResponse.ID);

            if (findIndex >= 0 && (findIndex + 1) < getSurveyResponses.Count)
                ViewBag.NextExists = getSurveyResponses[findIndex + 1].ID;

            if ((findIndex - 1) >= 0)
                ViewBag.PrevExists = getSurveyResponses[findIndex - 1].ID;


            ViewBag.FormQuestionAnswers = FormQuestionAnswer.LoadBySurveyResponseId(getSurveyResponse.ID.Value, msgs)
                .OrderBy(a => a.FormQuestionGroupDisplayOrder).ThenBy(a => a.FormQuestionDisplayOrder).ToList();
            ViewBag.SurveyName = SurveyRequest.Load(getSurveyResponse.SurveyRequestId.Value).Name;
            ViewBag.RegistrantName = Registrant.Load(getSurveyResponse.RegistrantId.Value).Name;
            ViewBag.SurveyFeedback = getSurveyResponse;
            ViewBag.Messages = msgs;

            return View(activity);
        }

        #endregion

        #region Import

        [HttpGet]
        public ActionResult RegistrantImport(string id, string createaccounts)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            ViewBag.Step = 1;
            ViewBag.StepLabel = "Upload";
            ViewBag.Messages = new SystemMessages();
            ViewBag.CreateAccounts = createaccounts.IfBlank("");
            ViewBag.SelectedAssignmentType = RegistrationAssignmentType.Reserved;
            ViewBag.IgnoreRequiredFields = false;
            return View(activity);
        }

        [HttpPost]
        public ActionResult RegistrantImport(string id, HttpPostedFileBase importfile, string csv, string createaccounts, string assignmenttype,
            string ignorerequiredfields,
            string validatephonenumbers, string enabletextmessages)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var participationObject = activity.GetParticipationObject();
            string extension = "";
            var questions = activity.GetActiveForm(msgs)
                .GetAllQuestions(msgs)
                .Where(question => question.IsDeleted == false)
                .OrderBy(x => x.FormQuestionGroupDisplayOrder).ThenBy(x => x.DisplayOrder)
                .ToList();
            bool createParentAccounts = createaccounts.IsBoolean(); // checkboxes are blank if it's not checked
            bool skipRequiredCheck = ignorerequiredfields.IsBoolean();
            bool validatePhoneNumbers = validatephonenumbers.IsBoolean();
            bool enableTextMessaging = enabletextmessages.IsBoolean();
            var selectedAssignmentType = RegistrationAssignmentType.NotSet;

            if (assignmenttype.IsNumber())
                selectedAssignmentType = assignmenttype.ToString().ToEnum<RegistrationAssignmentType>();

            if (csv.IsBlank())
            {
                if (importfile.IsEmpty())
                    msgs.Add("Please upload a file", MessageType.UserError);
                else if (Path.GetExtension(importfile.FileName).NotIn(new string[] { ".csv" }))
                    msgs.Add("Please upload a valid file", MessageType.UserError);
                else
                    extension = Path.GetExtension(importfile.FileName);

                if (msgs.HasNoErrors())
                {
                    ViewBag.Step = 2;
                    ViewBag.StepLabel = "Review";

                    //import
                    var sr = new StreamReader(importfile.InputStream);
                    var headers = new List<string>();
                    var rows = new List<List<string>>();
                    var validatedRows = new List<List<ImportField>>();
                    var reader = new CsvReader(sr);

                    reader.Configuration.Delimiter = ",";
                    reader.Configuration.HasHeaderRecord = true;
                    reader.Configuration.TrimOptions = TrimOptions.Trim;

                    LoadHeadersAndRowsDuringImport(questions, headers, rows, reader);

                    DoRowValidationsDuringImport(questions, headers, createParentAccounts, rows, validatedRows, skipRequiredCheck, validatePhoneNumbers,
                        participationObject, msgs);

                    importfile.InputStream.Position = 0;
                    var sr2 = new StreamReader(importfile.InputStream);
                    var csvString = sr2.ReadToEnd();

                    ViewBag.Headers = headers;
                    ViewBag.Rows = validatedRows;
                    ViewBag.Csv = csvString;
                    ViewBag.CreateAccounts = createaccounts;
                    ViewBag.EnableTextMessages = enableTextMessaging;
                }
            }
            else
            {
                ViewBag.Step = 3;
                ViewBag.StepLabel = "Import Complete";

                //import
                var sr = new StringReader(csv);
                var headers = new List<string>();
                var rows = new List<List<string>>();
                var validatedRows = new List<List<ImportField>>();

                //do import
                var reader = new CsvReader(sr);
                reader.Configuration.Delimiter = ",";
                reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.TrimOptions = TrimOptions.Trim;

                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var parent1Emails = new List<string>();

                        while (reader.Read())
                        {
                            if (headers.Count == 0)
                            {
                                reader.ReadHeader();
                                foreach (var header in reader.Context.HeaderRecord.ToList())
                                {
                                    if (questions.Any(x => header.IsSameAs($"{x.FormQuestionGroupName} - {x.Text}")))
                                        headers.Add(header);
                                }

                                continue;
                            }

                            WebUser parentUser = null;
                            Account parentAccount = null;
                            Registrant parentRegistrant = null;
                            Registrant parent2Registrant = null;
                            Registrant student = null;
                            Registration registration = null;
                            Registrant submittedRecord = null;
                            RegistrationPeriod period = null;
                            ParticipationObject participationObject = null;

                            if (!CreateAndLoadDataDuringImport(activity, headers, questions, reader, createParentAccounts, enableTextMessaging, parent1Emails,
                                    ref parentUser, ref parentAccount,
                                    ref student, ref parentRegistrant, ref parent2Registrant, ref registration, ref submittedRecord, ref participationObject,
                                    ref period,
                                    currentAccount, currentUser, amsgs, adc))
                                continue;

                            SaveAnswersToRegistrantsDuringImport(headers, questions, reader, createParentAccounts, student, parentRegistrant, parent2Registrant,
                                registration, submittedRecord, currentUser, amsgs, adc);

                            CompleteRegistration(createParentAccounts, parentAccount, student, ref registration, ref submittedRecord,
                                period, participationObject, selectedAssignmentType, currentUser, amsgs, adc);
                        }
                    },
                    onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }

            ViewBag.CreateAccounts = createaccounts;
            ViewBag.SelectedAssignmentType = selectedAssignmentType;
            ViewBag.Messages = msgs;
            ViewBag.IgnoreRequiredFields = skipRequiredCheck;
            return View(activity);
        }

        private static void LoadHeadersAndRowsDuringImport(List<FormQuestion> questions, List<string> headers, List<List<string>> rows, CsvReader reader)
        {
            while (reader.Read())
            {
                if (headers.Count == 0)
                {
                    reader.ReadHeader();

                    foreach (var header in reader.Context.HeaderRecord.ToList())
                    {
                        if (questions.Any(x => header.IsSameAs($"{x.FormQuestionGroupName} - {x.Text}")))
                            headers.Add(header);
                    }

                    continue;
                }

                var row = new List<string>();
                for (var i = 0; i < headers.Count; i++)
                    if (questions.Any(x => headers[i].IsSameAs($"{x.FormQuestionGroupName} - {x.Text}")))
                        row.Add(reader.GetField<string>(i));

                if (row.Count > 0)
                    rows.Add(row);
            }
        }

        private void DoRowValidationsDuringImport(List<FormQuestion> questions, List<string> headers, bool createParentAccounts,
            List<List<string>> rows, List<List<ImportField>> validatedRows, bool ignoreRequiredFields, bool validatePhoneNumbers,
            ParticipationObject participationObject, SystemMessages msgs)
        {
            var parent1InformationLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information).First()
                .FormQuestionGroupName;
            var studentInformationLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation).First()
                .FormQuestionGroupName;
            var studentFirstNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).First().Text;
            var studentLastNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.RegistrantLastName).First().Text;
            var parent1EmailLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1Email).First().Text;
            var parentEmailIndex = headers.IndexOf($"{parent1InformationLabel} - {parent1EmailLabel}");
            var parent1PhoneLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1MobilePhone).First().Text;
            var parentPhoneIndex = headers.IndexOf($"{parent1InformationLabel} - {parent1PhoneLabel}");
            var studentFirstNameIndex = headers.IndexOf($"{studentInformationLabel} - {studentFirstNameLabel}");
            var studentLastNameIndex = headers.IndexOf($"{studentInformationLabel} - {studentLastNameLabel}");
            var parent1Emails = new List<string>();

            if (createParentAccounts && parentEmailIndex == -1)
                msgs.Add(
                    $"Parent email is required to create a parent account. Please add '{parent1InformationLabel} - {parent1EmailLabel}' as a column to your import file",
                    MessageType.UserError);

            foreach (var row in rows)
            {
                var validatedRow = new List<ImportField>();
                var studentFirstName = row[studentFirstNameIndex];
                var studentLastName = row[studentLastNameIndex];
                var possibleDuplicate = false;

                if (studentFirstName.IsNotEmpty() && studentLastName.IsNotEmpty() &&
                    RegistrantActivity.ExistsByParticipationObjectByRegistrantName(participationObject.GetId().Value, participationObject.ObjectType,
                        studentFirstName, studentLastName))
                    possibleDuplicate = true;

                for (var i = 0; i < headers.Count; i++)
                {
                    var validatedField = new ImportField();
                    validatedField.Value = row[i];

                    var field = row[i].IfBlank("").Trim();
                    var question = questions.Where(x => headers[i].IsSameAs($"{x.FormQuestionGroupName} - {x.Text}")).FirstOrDefault();

                    if (question.IsEmpty())
                        continue;

                    validatedField.Value = field;

                    if (field.Length > 1000)
                        validatedField.Messages.Add("Must be less than or equal to 1000 characters. Data will be truncated.", MessageType.Warning);

                    if (field.IsBlank() && question.IsRequired)
                        validatedField.Messages.Add("This field is required, but has no value assigned",
                            ignoreRequiredFields ? MessageType.Warning : MessageType.UserError);

                    switch (question.QuestionTypeId)
                    {
                        case QuestionType.Date:

                            if (field.IsNotBlank() && !field.IsDateTime())
                                validatedField.Messages.Add("Must be a valid date", MessageType.UserError);

                            break;

                        case QuestionType.Checkbox:

                            if (field.IsNotBlank() && !field.IsBoolean())
                                validatedField.Messages.Add("Must be one of the following values: 0, 1, T, F, True, False, Y, N, Yes, No",
                                    MessageType.UserError);

                            break;

                        case QuestionType.TextBox:
                        case QuestionType.TextArea:

                            if (field.IsNotBlank() && question.IsPhoneDataType() && !field.IsPhone())
                                validatedField.Messages.Add("Phone must be a valid phone number",
                                    validatePhoneNumbers ? MessageType.UserError : MessageType.Warning);

                            /*if (createParentAccounts && i == parentPhoneIndex && field.IsNotBlank() && WebUser.ExistsByPhone(field, msgs))
                                validatedField.Messages.Add("Parent already has an account, record cannot be imported. Please ask parent to register. This record will be skipped.", MessageType.Warning);*/

                            if ((studentFirstNameIndex == i || studentLastNameIndex == i) && possibleDuplicate)
                                validatedField.Messages.Add("Another student with this name is already registered for this activity", MessageType.Warning);

                            break;

                        case QuestionType.Email:

                            if (field.IsNotBlank() && !field.IsRegExMatch(BasicLibrary.Utilities.EmailRegEx))
                                validatedField.Messages.Add("Must be a valid email address", MessageType.UserError);

                            if (createParentAccounts && i == parentEmailIndex)
                            {
                                /*if (WebUser.ExistsByEmail(field, msgs))
                                    validatedField.Messages.Add("Parent already has an account, record cannot be imported. Please ask parent to register. This record will be skipped.", MessageType.Warning);*/

                                if (field.In(parent1Emails))
                                    validatedField.Messages.Add(
                                        "This email is a duplicate of another parent email in this import. This student record will be attached to the same parent account",
                                        MessageType.Warning);

                                if (!parent1Emails.Contains(field))
                                    parent1Emails.Add(field);
                            }

                            break;

                        case QuestionType.DropDownList:
                        case QuestionType.RadioList:

                            if (field.IsNotBlank())
                            {
                                var options = Utilities.GetKeyValuePairs(question.Options.ToLower()).ToStringDictionary();

                                if (!options.ContainsKey(field.ToLower()) && !options.ContainsValue(field.ToLower()))
                                    validatedField.Messages.Add(
                                        $"Must be one of the following values: {Utilities.GetKeyValuePairs(question.Options).ToStringDictionary().Keys.ToStringList()}",
                                        MessageType.UserError);
                            }

                            break;

                        case QuestionType.CheckboxList:

                            if (field.IsNotBlank())
                            {
                                var options = Utilities.GetKeyValuePairs(question.Options.ToLower()).ToStringDictionary();
                                var fieldvalues = field.ToLower().Split(',').Select(x => x.Trim()).Where(x => x.Length > 0);

                                if (!options.Keys.Any(key => key.In(fieldvalues)) && !options.Values.Any(value => value.In(fieldvalues)))
                                    validatedField.Messages.Add(
                                        $"Must be one of the following values: {Utilities.GetKeyValuePairs(question.Options).ToStringDictionary().Keys.ToStringList()}",
                                        MessageType.UserError);
                            }

                            break;
                    }

                    validatedRow.Add(validatedField);
                }

                validatedRows.Add(validatedRow);
            }
        }

        private void CompleteRegistration(bool createParentAccounts, Account parentAccount, Registrant student, ref Registration registration,
            ref Registrant submittedRecord, RegistrationPeriod period, ParticipationObject participationObject,
            RegistrationAssignmentType selectedAssignmentType, WebUser currentUser, SystemMessages amsgs, DatabaseConnection adc)
        {
            if (createParentAccounts)
            {
                if (registration.IsNotEmpty())
                {
                    registration.ProcessRegistrationAssignment(
                        participationObject: participationObject,
                        masterRegistrant: student,
                        registrant: submittedRecord,
                        seatsRequested: 1,
                        activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                        periodId: period?.ID,
                        amountCharged: participationObject.Cost,
                        loggedInUser: currentUser,
                        msgs: amsgs,
                        dc: adc);
                }
                else
                {
                    RegistrationCommon.SubmitRecord(
                        ref registration,
                        ref submittedRecord,
                        student,
                        participationObject,
                        currentUser,
                        period,
                        "",
                        Request.UserAgent,
                        Request.GetUserHostAddress(),
                        amsgs,
                        adc);

                    AccountService.ProcessRegistrationWithoutPayment(
                        new CheckoutObject
                        {
                            BillingAccount = participationObject.OwnerAccount,
                            PayingAccount = parentAccount,
                            NumberOfSeats = 1,
                            ParticipationObject = participationObject,
                            Registration = registration,
                            MasterRegistrant = student,
                            SubscriptionPlan = null,
                            SelectedDates = new List<DateTime>(),
                        },
                        false,
                        currentUser,
                        amsgs,
                        adc);
                }
            }
            else
            {
                registration.ProcessRegistrationAssignment(
                    participationObject: participationObject,
                    masterRegistrant: null,
                    registrant: student,
                    seatsRequested: 1,
                    activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                    periodId: period?.ID,
                    amountCharged: participationObject.Cost,
                    loggedInUser: currentUser,
                    msgs: amsgs,
                    dc: adc);
            }

            if (selectedAssignmentType != RegistrationAssignmentType.NotSet)
                registration.GetRegistrantActivity(participationObject, amsgs, adc).SetAssignmentType(selectedAssignmentType, currentUser, amsgs, adc);
        }

        private bool CreateAndLoadDataDuringImport(Activity activity, List<string> headers, List<FormQuestion> questions, CsvReader reader,
            bool createParentAccounts,
            bool enableTextMessaging, List<string> parent1Emails,
            ref WebUser parentUser, ref Account parentAccount,
            ref Registrant student, ref Registrant parentRegistrant, ref Registrant parent2Registrant,
            ref Registration registration, ref Registrant submittedRecord, ref ParticipationObject participationObject, ref RegistrationPeriod period,
            Account currentAccount, WebUser currentUser, SystemMessages amsgs, DatabaseConnection adc)
        {
            var parent1InformationLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information).First()
                .FormQuestionGroupName;
            var parent1EmailLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1Email).First().Text;
            var parentEmailIndex = -1;

            if (parent1EmailLabel.IsNotEmpty())
                parentEmailIndex = headers.IndexOf($"{parent1InformationLabel} - {parent1EmailLabel}");

            period = activity.GetCurrentRegistrationPeriod(amsgs, adc);
            participationObject = activity.GetParticipationObject(amsgs, adc);

            if (createParentAccounts && parentEmailIndex > -1)
            {
                if (!CreateNewAccountRegistrantsDuringImport(questions, headers, reader, enableTextMessaging, parentEmailIndex, parent1Emails,
                        participationObject, period, ref registration, ref submittedRecord, ref parentUser, ref parentAccount, ref parentRegistrant,
                        ref parent2Registrant, ref student,
                        currentUser, currentAccount, amsgs, adc))
                    return false;
            }
            else
            {
                student = currentAccount.CreateParentOrStudentRegistrant(
                    registrantOwner: null,
                    canBeAccessedBy: null,
                    registrantType: RegistrantType.Individual,
                    isMaster: false,
                    isParentOrGuardian: false,
                    allowTextMessages: false,
                    schoolLocationId: null,
                    profileStatus: RegistrantProfileStatus.Started,
                    loggedInUser: currentUser,
                    msgs: amsgs,
                    dc: adc);

                // create a registration only if not creating parent accounts
                // registration will be created later
                registration = student.AddRegistration(
                    ownerAccountId: currentAccount.ID.Value,
                    activityGroupId: participationObject.GetAnyRelatedActivityGroupId(),
                    accountActivityGroupId: participationObject.GetAnyRelatedAccountActivityGroupId(),
                    periodId: period?.ID,
                    completionStatus: RegistrationCompletionStatus.RequiresParentSignature,
                    loggedInUserId: currentUser.ID.Value,
                    msgs: amsgs,
                    dc: adc);
            }

            return true;
        }

        private void SaveAnswersToRegistrantsDuringImport(List<string> headers, List<FormQuestion> questions, CsvReader reader, bool createParentAccounts,
            Registrant student, Registrant parentRegistrant, Registrant parent2Registrant, Registration registration, Registrant submittedRecord,
            WebUser currentUser, SystemMessages amsgs, DatabaseConnection adc)
        {

            var saveParent2Answers = parent2Registrant.IsEmpty() || parent2Registrant.HasAnswers(amsgs, adc) || submittedRecord.IsNotEmpty() ? false : true;

            for (var i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                if (header.IsNotBlank())
                {
                    var field = reader.GetField<string>(i);
                    var question = questions.Where(x => headers[i].IsSameAs($"{x.FormQuestionGroupName} - {x.Text}")).FirstOrDefault();

                    if (field.IsNotBlank() && question.IsNotEmpty())
                    {
                        // having a submittedRecord means that the parent already had an account before this import
                        (submittedRecord ?? student).AddOrUpdateAnswer(field, question, registration?.ID, currentUser, amsgs, adc);

                        if (createParentAccounts && question.QuestionDataTypeId != QuestionDataType.NotSet)
                        {
                            if (student.Email.IsBlank())
                            {
                                student.Email = parentRegistrant.Email;
                                amsgs.AddRange(student.Save(adc));
                            }

                            // if parent had an account before this import, don't update it
                            if (submittedRecord.IsEmpty())
                                switch (question.FormQuestionGroupQuestionGroupDataTypeId)
                                {
                                    case QuestionGroupDataType.Parent1Information:
                                        parentRegistrant.MapAndAddAnswer(
                                            questions,
                                            student,
                                            question.QuestionDataTypeId,
                                            RegistrationCommon.Parent1InfoToMainRegistrantMap.Inverse()[question.QuestionDataTypeId],
                                            currentUser,
                                            amsgs,
                                            adc);
                                        break;
                                    case QuestionGroupDataType.Parent1Address:
                                        parentRegistrant.MapAndAddAnswer(
                                            questions,
                                            student,
                                            question.QuestionDataTypeId,
                                            RegistrationCommon.Parent1AddressToMainRegistrantMap.Inverse()[question.QuestionDataTypeId],
                                            currentUser,
                                            amsgs,
                                            adc);
                                        break;
                                    case QuestionGroupDataType.EmergencyContact1:
                                    case QuestionGroupDataType.EmergencyContact2:
                                    case QuestionGroupDataType.EmergencyContact3:
                                        parentRegistrant.AddOrUpdateAnswer(field, question, currentUser, amsgs, adc);
                                        break;
                                }

                            if (saveParent2Answers && parent2Registrant.IsNotEmpty())
                                switch (question.FormQuestionGroupQuestionGroupDataTypeId)
                                {
                                    case QuestionGroupDataType.Parent2Information:
                                        parent2Registrant.MapAndAddAnswer(
                                            questions,
                                            student,
                                            question.QuestionDataTypeId,
                                            RegistrationCommon.Parent2InfoToMainRegistrantMap.Inverse()[question.QuestionDataTypeId],
                                            currentUser,
                                            amsgs,
                                            adc);
                                        break;
                                    case QuestionGroupDataType.Parent2Address:
                                        parent2Registrant.AddOrUpdateAnswer(field, question, currentUser, amsgs, adc);
                                        parent2Registrant.MapAndAddAnswer(
                                            questions,
                                            student,
                                            question.QuestionDataTypeId,
                                            RegistrationCommon.Parent2AddressToMainRegistrantMap.Inverse()[question.QuestionDataTypeId],
                                            currentUser,
                                            amsgs,
                                            adc);
                                        break;
                                }
                        }
                    }
                }
            }
        }

        private bool CreateNewAccountRegistrantsDuringImport(List<FormQuestion> questions, List<string> headers, CsvReader reader, bool enableTextMessaging,
            int parentEmailIndex, List<string> parent1Emails, ParticipationObject participationObject, RegistrationPeriod period, ref Registration registration,
            ref Registrant submittedRecord,
            ref WebUser parentUser, ref Account parentAccount, ref Registrant parentRegistrant, ref Registrant parent2Registrant, ref Registrant student,
            WebUser currentUser, Account currentAccount, SystemMessages amsgs, DatabaseConnection adc)
        {
            var parent1InformationLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information).First()
                .FormQuestionGroupName;
            var parent1PhoneLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1MobilePhone).First().Text;
            var parentPhoneIndex = -1;

            if (parent1PhoneLabel.IsNotEmpty())
                parentPhoneIndex = headers.IndexOf($"{parent1InformationLabel} - {parent1PhoneLabel}");

            var parent2InformationLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.Parent2Information).First()
                .FormQuestionGroupName;
            var parent2AddressLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.Parent2Address).First()
                .FormQuestionGroupName;

            var parent1FirstNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1FirstName).First().Text;
            var parent1LastNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1LastName).First().Text;
            var parent2FirstNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent2FirstName).First().Text;
            var parent2LastNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent2LastName).First().Text;

            var parentFirstNameIndex = headers.IndexOf($"{parent1InformationLabel} - {parent1FirstNameLabel}");
            var parentLastNameIndex = headers.IndexOf($"{parent1InformationLabel} - {parent1LastNameLabel}");
            var parent2FirstNameIndex = headers.IndexOf($"{parent2InformationLabel} - {parent2FirstNameLabel}");
            var parent2LastNameIndex = headers.IndexOf($"{parent2InformationLabel} - {parent2LastNameLabel}");

            var parentFirstName = reader.GetField<string>(parentFirstNameIndex);
            var parentLastName = reader.GetField<string>(parentLastNameIndex);
            var parentEmail = reader.GetField<string>(parentEmailIndex);

            var studentInformationLabel = questions.Where(x => x.FormQuestionGroupQuestionGroupDataTypeId == QuestionGroupDataType.PersonalInformation).First()
                .FormQuestionGroupName;
            var studentFirstNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.RegistrantFirstName).First().Text;
            var studentLastNameLabel = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.RegistrantLastName).First().Text;
            var studentFirstNameIndex = headers.IndexOf($"{studentInformationLabel} - {studentFirstNameLabel}");
            var studentLastNameIndex = headers.IndexOf($"{studentInformationLabel} - {studentLastNameLabel}");
            var studentFirstName = reader.GetField<string>(studentFirstNameIndex);
            var studentLastName = reader.GetField<string>(studentLastNameIndex);

            var parentPhone = "";
            if (parentPhoneIndex > -1)
                parentPhone = reader.GetField<string>(parentPhoneIndex);

            var createNewStudentRecord = true;

            // if parent account wasn't created during this import
            if (!parent1Emails.Contains(parentEmail))
            {
                parentUser = WebUser.LoadByEmail(parentEmail, amsgs, adc);

                if (parentUser.IsEmpty() && parentPhone.IsNotEmpty())
                    parentUser = WebUser.LoadByPhone(parentPhone, amsgs, adc);

                // parent already has an account
                if (parentUser.IsNotEmpty())
                {
                    parentAccount = parentUser.GetRegistrantAccount(amsgs, adc);
                    parentRegistrant = parentUser.GetMasterParentOrStudentRegistrant(amsgs, adc);
                    var registrantsConnectedToAccount = parentUser.GetAllRegistrants(amsgs, adc);

                    // check if student already has a profile, if not then you create one for the parent account
                    // if so then we don't want to edit the existing student profile in the parent account
                    // we want to only submit the answers in the provider account registrant profile
                    if (registrantsConnectedToAccount.Any(x =>
                            x.FirstName.IsSameAs(studentFirstName) && x.LastName.IsSameAs(studentLastName) && !x.IsParentOrGuardian))
                    {
                        createNewStudentRecord = false;
                        student = registrantsConnectedToAccount.First(x =>
                            x.FirstName.IsSameAs(studentFirstName) && x.LastName.IsSameAs(studentLastName) && !x.IsParentOrGuardian);

                        // go ahead and create the submitted record and save it as the student
                        // if the student is not already registered
                        // so that when the answers are saved it isn't saved to the master student record
                        if (!student.IsRegisteredFor(participationObject, null, amsgs, adc))
                            RegistrationCommon.SubmitRecord(
                                ref registration,
                                ref submittedRecord,
                                student,
                                participationObject,
                                currentUser,
                                period,
                                "",
                                Request.UserAgent,
                                Request.GetUserHostAddress(),
                                amsgs,
                                adc);
                        else
                            return false;
                    }

                    // check for second parent
                    var parentRegistrantId = parentRegistrant.ID;
                    if (registrantsConnectedToAccount.Any(x => x.PersonTypeId == RegistrantPersonType.Parent && x.ID != parentRegistrantId))
                        parent2Registrant = registrantsConnectedToAccount.First(x => x.IsParentOrGuardian && x.ID != parentRegistrantId);
                }
                else
                {
                    parentUser = RegistrationCommon.CreateOrUpdateUserWithOptionalPhone(
                        null,
                        parentFirstName,
                        parentLastName,
                        parentEmail,
                        parentPhone.IsPhone() ? parentPhone : "",
                        Guid.NewGuid().ToString().MaxChars(8), //setting temp password
                        enableTextMessaging,
                        amsgs,
                        adc);

                    if (amsgs.HasNoErrors() && parentUser.IsNotEmpty())
                        parentAccount = RegistrationCommon.GetOrCreateParentOrStudentRegistrantAccount(parentUser, amsgs, adc);

                    if (amsgs.HasNoErrors())
                        parentRegistrant = RegistrationCommon.GetOrCreateParentOrGuardianRegistrant(parentAccount, parentUser, enableTextMessaging, amsgs, adc);

                    if (!parent1Emails.Contains(parentEmail))
                        parent1Emails.Add(parentEmail);
                }
            }
            // accounts already created during the import
            else
            {
                parentUser = WebUser.LoadByEmail(parentEmail, amsgs, adc);
                parentAccount = parentUser.GetRegistrantAccount(amsgs, adc);
                parentRegistrant = parentUser.GetMasterParentOrStudentRegistrant(amsgs, adc);

                // if the existing parent2Registrant is the same as the previous imported one, don't create a new profile
                // if a different name (could be a different parent), then create a new parent profile
                var parentRegistrantId = parentRegistrant.ID;
                var parent2Registrants = parentUser.GetAllRegistrants(amsgs, adc).Where(x => x.IsParentOrGuardian && x.ID != parentRegistrantId).ToList();

                if (parent2Registrants.Any())
                {
                    var parent2FirstName = reader.GetField<string>(parent2FirstNameIndex);
                    var parent2LastName = reader.GetField<string>(parent2LastNameIndex);
                    var tempParent2Registrant = parent2Registrants.First();

                    if (tempParent2Registrant.FirstName.IsSameAs(parent2FirstName) && tempParent2Registrant.LastName.IsSameAs(parent2LastName))
                        parent2Registrant = tempParent2Registrant;
                }
            }

            if (parent2Registrant.IsEmpty() && (headers.ContainsLike($"{parent2InformationLabel} - ") || headers.ContainsLike($"{parent2AddressLabel} - ")))
                parent2Registrant = parentAccount.CreateParentOrStudentRegistrant(
                    registrantOwner: null,
                    canBeAccessedBy: parentUser.AsList(),
                    registrantType: RegistrantType.Individual,
                    isMaster: true,
                    isParentOrGuardian: true,
                    allowTextMessages: true,
                    schoolLocationId: null,
                    profileStatus: RegistrantProfileStatus.Started,
                    loggedInUser: currentUser,
                    msgs: amsgs,
                    dc: adc);


            // create student registrant record
            if (createNewStudentRecord)
                student = parentAccount.CreateParentOrStudentRegistrant(
                    registrantOwner: null,
                    canBeAccessedBy: parentUser.AsList(),
                    registrantType: RegistrantType.Individual,
                    isMaster: true,
                    isParentOrGuardian: false,
                    allowTextMessages: false,
                    schoolLocationId: null,
                    profileStatus: RegistrantProfileStatus.Started,
                    loggedInUser: currentUser,
                    msgs: amsgs,
                    dc: adc);

            return true;
        }

        [HttpGet]
        public ActionResult GetImportSample(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
            var questions = activity.GetActiveForm(msgs).GetAllQuestions(msgs).OrderBy(x => x.FormQuestionGroupDisplayOrder).ThenBy(x => x.DisplayOrder);

            var swriter = new StringWriter();
            var csvWriter = new CsvWriter(swriter);
            csvWriter.Configuration.HasHeaderRecord = true;

            foreach (var question in questions)
            {
                csvWriter.WriteField($"{question.FormQuestionGroupName} - {question.Text}");
            }

            csvWriter.NextRecord();

            string csv = swriter.GetStringBuilder().ToString();

            Response.Clear();
            Response.Write(csv);
            Response.AddHeader("Content-Disposition", "attachment; filename=importsample.csv");
            Response.AddHeader("Content-Type", "text/csv");
            Response.AddHeader("Content-Length", Encoding.UTF8.GetByteCount(csv).ToString());
            Response.End();

            return View();
        }

        #endregion
    }
}