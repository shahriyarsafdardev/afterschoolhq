using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ServicesController : BaseLoggedInController
    {
        public ServicesController() : base(AccountService.OrganizationAccountTypes)
        {
        }

        [HttpGet]
        public ActionResult CalculateFinalPrice(string cost, string activityUid, string takenOutOfCost)
        {
            var msgs = new SystemMessages();

            if (!cost.IsNumber() || cost.ToDecimal() <= 0)
                msgs.Add("Total cost must be a number greater than 0", MessageType.UserError);

            if (activityUid.IsEmpty())
                msgs.Add("Invalid activity", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var currentAccount = SessionVariables.CurrentAccount;
                var activity = Activity.LoadByUidByAccountId(activityUid.ToGuid(), currentAccount.ID.Value, msgs);
                var calcObject = new FeeCalculationObject(
                    cost.ToDecimal(),
                    currentAccount,
                    activity,
                    takenOutOfCost.IsBoolean() ? takenOutOfCost.ToBoolean() : new bool?());
                var priceToParent = calcObject.GetCostWithConvenienceFee();
                var amountToProvider = calcObject.GetAmountToProvider();

                return Json(new
                {
                    status = "ok",
                    priceToParent = priceToParent,
                    amountToProvider = amountToProvider,
                    messages = msgs,
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdatePublishPermission(int arId, bool isChecked)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            var ar = AccountRelationship.Load(arId, msgs);

            if (ar.IsEmpty())
                msgs.Add("Invalid request", MessageType.UserError);

            if (msgs.HasNoErrors() && ar.HostAccountId != currentAccount.ID)
                msgs.Add("Invalid request", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                ar.UpdateRequiresPublishPermission(isChecked, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                        messages = msgs,
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateShowAllPrograms(int arId, bool isChecked)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            var ar = AccountRelationship.Load(arId, msgs);

            if (ar.IsEmpty())
                msgs.Add("Invalid request", MessageType.UserError);

            if (msgs.HasNoErrors() && ar.HostAccountId != currentAccount.ID)
                msgs.Add("Invalid request", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                ar.UpdateShowAllPrograms(isChecked, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new
                    {
                        status = "ok",
                        messages = msgs,
                    }, JsonRequestBehavior.AllowGet);
            }

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult InviteTreasurer(string firstname, string lastname, string email)
        {
            var msgs = new SystemMessages();

            if (firstname.IsEmpty())
                msgs.Add("First Name is required", MessageType.UserError);

            if (lastname.IsEmpty())
                msgs.Add("Last Name is required", MessageType.UserError);

            if (!email.IsEmail())
                msgs.Add("Email is required", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var emailText =
                    $"{SessionVariables.CurrentUser.FirstName} has invited you to setup the payment account for {SessionVariables.CurrentAccount.Name} in {UILabel.ProductName()}. This will enable us to send registration money to your account.";

                SessionVariables.CurrentAccount.InviteUser(firstname, lastname, email, null, emailText,
                    UserType.Treasurer, SessionVariables.CurrentUser, msgs);

                if (msgs.HasNoErrors())
                    return Json(new {status = "ok"}, JsonRequestBehavior.AllowGet);
            }

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MoveRegistrantAssignment(string id, string registrant, string assignmenttype)
        {
            var msgs = new SystemMessages();
            var activity = Activity.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value, msgs);
            var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                activity.ID.Value,
                ParticipationObjectType.Activity,
                registrant.ToInteger(), msgs);
            var newAssignmentType = (RegistrationAssignmentType) assignmenttype.ToInteger();

            if (ra.IsNotEmpty())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var reg = ra.ToRegistrant();
                        reg.AddOrUpdateParticipationObject(
                            ra.RegistrationId.Value,
                            activity.ID.Value,
                            ParticipationObjectType.Activity,
                            newAssignmentType,
                            ra.RegistrationPeriodId,
                            ra.ActivityGroupId,
                            null,
                            SessionVariables.CurrentUser.ID.Value,
                            amsgs,
                            adc);

                        // TODO: may change assignment to pending reserved if the user is unpaid, and then auto-send the invoice
                        // For now won't do that.
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        WriteOutput(new {status = "ok"}, ResponseType.JSON);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                        HandleErrors(msgs, ResponseType.JSON);
                    }
                );
            }
            else
            {
                msgs.Add("Invalid data", MessageType.UserError);
            }

            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult SendPaymentReminder(string id, string registrant)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (id.IsGuid())
            {
                var activity = Activity.LoadByUid(id.ToGuid(), msgs);
                if (activity.IsNotEmpty())
                {
                    var ra = RegistrantActivity.LoadByParticipationObjectByRegistrant(
                        activity.ID.Value,
                        ParticipationObjectType.Activity,
                        registrant.ToInteger(), msgs);
                    var assignmentType = ra.AssignmentTypeId;
                    var reservedTypes = new RegistrationAssignmentType[]
                        {RegistrationAssignmentType.Reserved, RegistrationAssignmentType.ReservedPendingPayment};

                    if (ra.IsNotEmpty())
                    {
                        if (ra.AssignmentTypeId.NotIn(reservedTypes))
                        {
                            var reg = ra.ToRegistrant();

                            assignmentType = RegistrationAssignmentType.ReservedPendingPayment;

                            reg.AddOrUpdateParticipationObject(
                                ra.RegistrationId.Value,
                                activity.ID.Value,
                                ParticipationObjectType.Activity,
                                assignmentType,
                                ra.RegistrationPeriodId,
                                ra.ActivityGroupId,
                                null,
                                SessionVariables.CurrentUser.ID.Value,
                                msgs);
                        }

                        msgs.AddRange(ra.SendPaymentReminder());

                        WriteOutput(new {status = "ok", AssignmentType = (int) assignmentType}, ResponseType.JSON);
                    }
                    else
                    {
                        msgs.Add("Invalid data", MessageType.UserError);
                    }
                }
                else
                {
                    msgs.Add("Invalid data", MessageType.UserError);
                }
            }
            else
            {
                msgs.Add("Invalid data", MessageType.UserError);
            }

            if (msgs.HasErrors())
                HandleErrors(msgs, ResponseType.JSON);

            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult GetScheduleActivities(string id)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (SessionVariables.CurrentAccount.IsNotEmpty() && id.IsInteger())
            {
                var schedule =
                    ActivitySchedule.LoadByIdByAccountId(id.ToInteger(), SessionVariables.CurrentAccount.ID.Value,
                        msgs);

                if (schedule.IsEmpty())
                    msgs.Add("Invalid data", MessageType.ApiUsageError);
                else
                {
                    var activities = schedule.GetActivities();
                    WriteOutput(new {status = "ok", activities = activities.Select(x => new {x.ID, x.Name})},
                        ResponseType.JSON);
                }
            }
            else
                msgs.Add("Invalid data", MessageType.ApiUsageError);

            if (msgs.HasErrors())
                HandleErrors(msgs, ResponseType.JSON);

            Response.End();

            return View();
        }

        [HttpPost]
        public ActionResult SetScheduleActivities(string id)
        {
            var msgs = new SystemMessages();
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            Response.Clear();

            if (currentUser.IsEmpty() || !currentUser.ID.HasValue)
                msgs.Add("User is not logged in.", MessageType.UserError);

            if (currentAccount.IsEmpty() || !currentAccount.ID.HasValue)
                msgs.Add("Account is empty.", MessageType.UserError);

            if (msgs.HasNoErrors() && id.IsInteger())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var schedule =
                            ActivitySchedule.LoadByIdByAccountId(id.ToInteger(), currentAccount.ID.Value, amsgs, adc);
                        var currentActivities = schedule.GetActivities(amsgs, adc);
                        var activities = schedule.GetActivityGroup(amsgs, adc).GetActivities(amsgs, adc);

                        // remove
                        foreach (var item in currentActivities)
                            if (!Request[$"activity-{item.ID}"].IsNumber())
                                item.UpdateActivitySchedule(null, currentUser.ID.Value, amsgs, adc);

                        // add
                        foreach (var item in activities)
                            if (Request[$"activity-{item.ID}"].IsNumber())
                                item.UpdateActivitySchedule(schedule.ID, currentUser.ID.Value, amsgs, adc);
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        WriteOutput(new {status = "ok"}, ResponseType.JSON);
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => Utilities.SystemErrorLog(emsgs)
                );
            }
            else
                msgs.Add("Invalid data", MessageType.ApiUsageError);

            if (msgs.HasErrors())
                HandleErrors(msgs, ResponseType.JSON);

            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult UpdateSchedule(string id, string schedule)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (id.IsGuid() && schedule.IsInteger())
            {
                var activity =
                    Activity.LoadByUidByAccountId(id.ToGuid(), SessionVariables.CurrentAccount.ID.Value, msgs);
                var sched = ActivitySchedule.LoadByIdByAccountId(schedule.ToInteger(),
                    SessionVariables.CurrentAccount.ID.Value, msgs);

                if (activity.AccountId != SessionVariables.CurrentAccount.ID ||
                    sched.AccountId != SessionVariables.CurrentAccount.ID)
                    msgs.Add("Invalid data", MessageType.ApiUsageError);

                if (msgs.HasNoErrors())
                    activity.UpdateActivitySchedule(sched.ID, SessionVariables.CurrentUser.ID.Value, msgs);
            }
            else
                msgs.Add("Invalid data", MessageType.ApiUsageError);

            if (msgs.HasNoErrors())
                WriteOutput(new {status = "ok"}, ResponseType.JSON);
            else
                HandleErrors(msgs, ResponseType.JSON);

            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult AddContactToList(string list, string contact)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (!list.IsInteger() || !contact.IsGuid())
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasNoErrors())
            {
                var contactList = ContactList.Load(list.ToInteger());
                var registrant = Registrant.LoadByUid(contact.ToGuid());

                if (contactList.AccountId == SessionVariables.CurrentAccount.ID.Value)
                {
                    msgs.AddRange(contactList.AddContact(registrant.ID.Value, SessionVariables.CurrentUser.ID.Value));
                    if (msgs.HasNoErrors())
                    {
                        WriteOutput(
                            new {status = "ok", contactName = registrant.DisplayName, listName = contactList.Name},
                            ResponseType.JSON);
                    }
                }
                else
                {
                    msgs.Add("Invalid data", MessageType.ApiUsageError);
                }
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }

            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult RemoveFromContactList(string list, string contact)
        {
            var msgs = new SystemMessages();
            Response.Clear();

            if (!list.IsInteger() || !contact.IsGuid())
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasNoErrors())
            {
                var contactList = ContactList.Load(list.ToInteger());
                var registrant = Registrant.LoadByUid(contact.ToGuid());

                if (contactList.AccountId == SessionVariables.CurrentAccount.ID.Value)
                {
                    msgs.AddRange(contactList.RemoveContact(registrant.ID.Value,
                        SessionVariables.CurrentUser.ID.Value));
                    if (msgs.HasNoErrors())
                    {
                        WriteOutput(
                            new {status = "ok", contactName = registrant.DisplayName, listName = contactList.Name},
                            ResponseType.JSON);
                    }
                }
                else
                {
                    msgs.Add("Invalid data", MessageType.ApiUsageError);
                }
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }

            Response.End();

            return View();
        }

        [HttpGet]
        public ActionResult DeleteContact(string id)
        {
            Response.Clear();
            var msgs = new SystemMessages();
            if (id.IsGuid())
            {
                Registrant contact = null;

                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        contact = Registrant.LoadByUid(id.ToGuid(), dc: adc);
                        if (contact.IsNotEmpty() && contact.AccountId == SessionVariables.CurrentAccount.ID &&
                            contact.IsContactOnly)
                        {
                            var rags = RegistrantActivityGroup.LoadByRegistrantId(contact.ID.Value, dc: adc);
                            foreach (var rag in rags)
                            {
                                amsgs.AddRange(rag.Delete(adc));
                            }

                            var ras = RegistrantActivity.LoadByRegistrantId(contact.ID.Value, dc: adc);
                            foreach (var ra in ras)
                            {
                                amsgs.AddRange(ra.Delete(adc));
                            }

                            amsgs.AddRange(contact.Delete(adc));
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        WriteOutput(new {status = "ok", contactName = contact.DisplayName}, ResponseType.JSON);
                    },
                    onFailure: (SystemMessages fmsgs) => { msgs.AddRange(fmsgs); }
                );
            }
            else
            {
                msgs.Add("Invalid data", MessageType.ApiUsageError);
            }

            if (msgs.HasErrors())
            {
                HandleErrors(msgs, ResponseType.JSON);
            }

            Response.End();
            return View();
        }

        // GET: Org/Services
        public ActionResult GetAnswerOptions(string id)
        {
            var question = FormQuestion.Load(id.ToInteger());
            var list = new List<string>();

            if (question.IsNotEmpty() && question.Options.IsNotEmpty())
            {
                list = question.Options.Split(',').ToList();
            }

            Response.Clear();
            WriteOutput(new {status = "ok", answers = list}, ResponseType.JSON);
            Response.End();

            return View();
        }

        public ActionResult GetActivityDays(string id)
        {
            var activity = Activity.LoadByUid(id.ToGuid());
            Response.Clear();
            WriteOutput(
                new
                {
                    status = "ok",
                    days = activity.GetDaysOfTheWeek().Select(x => new {Value = (int) x, Text = x.ToString()}),
                }, ResponseType.JSON);
            Response.End();

            return View();
        }

        //public ActionResult GetRegistrationMaxChoiceNumbersForDay(string id, string day)
        //{
        //    var ras = RegistrantActivity.LoadByRegistrationId(id.ToInteger());
        //    var dow = (DayOfWeekASL)day.ToInteger();

        //    Response.Clear();
        //    WriteOutput(new { status = "ok", MaxChoices = ras.Where(x => x.Day == dow).Max(x => x.PreferenceNumber.Value) }, ResponseType.JSON);
        //    Response.End();

        //    return View();
        //}

        public ActionResult SendVerificationEmail(string redirectUrl)
        {
            var msgs = SessionVariables.CurrentUser.SendVerificationEmail(redirectUrl,
                SessionVariables.IsAfterSchool.Value);

            Response.Clear();
            WriteOutput(new {status = "ok", messages = msgs}, ResponseType.JSON);
            Response.End();

            return View();
        }

        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            var msgs = new SystemMessages();
            var allowedFiles = new string[] {".png", ".jpg", ".jpeg"};

            if (file.IsEmpty() || file.ContentLength == 0 ||
                !file.FileName.ToLower().Contains(allowedFiles) ||
                file.ContentLength > Utilities.ImageUploadMaxBytes)
                msgs.Add("Please upload a valid jpg or png file, must be less than 4MB", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                var account = SessionVariables.CurrentAccount;
                var filename = file.FileName;
                var stream = file.InputStream;
                var ah = Utilities.GetNewAmazonHelper();
                var amazonLocation = $"{account.UniqueName}/files/{filename}";

                msgs.AddRange(ah.AddFile(stream, amazonLocation, true));

                if (msgs.HasNoErrors())
                {
                    var url = ah.GetFileLink(amazonLocation, null, true);

                    return Json(new
                    {
                        status = "ok",
                        url,
                    }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new {status = "error", messages = msgs}, JsonRequestBehavior.AllowGet);
        }
    }
}