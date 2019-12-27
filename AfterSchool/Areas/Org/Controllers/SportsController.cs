using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using Hangfire;
using System.Collections;
using System.Web.Script.Serialization;

namespace AfterSchool.Areas.Org.Controllers
{
    public class SportsController : BaseLoggedInController
    {
        public SportsController() : base(new List<AccountType>() { AccountType.ActivityProvider, AccountType.School })
        {

        }

        // GET: Org/Sports
        public ActionResult Index(string pn, string rpp, string archive)
        {
            int pageNumber = 1;
            int recordsPerPage = 10;
            bool isArchived = false;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToInteger().ToBoolean();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;
            var hasArchivedGroups = false;

            totalCount = SessionVariables.CurrentAccount.GetLeagueCount(isArchived);
            var leagues = SessionVariables.CurrentAccount.GetLeagues(isArchived, start, length).OrderByDescending(x => x.CreateDate).ToList();

            hasArchivedGroups = SessionVariables.CurrentAccount.GetLeagueCount(true) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "sports";
            ViewBag.Messages = new SystemMessages();
            ViewBag.HasArchivedGroups = hasArchivedGroups;
            ViewBag.ActivityGroupType = ActivityGroupType.League;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(leagues);
        }

        [HttpGet]
        public ActionResult Summary(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var registrations = group.GetRegistrations(msgs: msgs);
            var actions = SessionAction.LoadByActivityGroupId(
                group.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityGroupCreate,
                    SessionActionType.ActivityGroupPublish,
                    SessionActionType.ActivityGroupUnpublish,
                },
                0,
                15,
                msgs);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.Registrations = registrations;
            return View(group);
        }

        [HttpGet]
        public ActionResult Edit()
        {
            return RedirectToAction("EditSetup");
        }

        [HttpGet]
        public ActionResult EditSetup(string id)
        {
            var msgs = new SystemMessages();
            ActivityGroup league = null;

            if (id.IsGuid())
            {
                league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            }
            else
            {
                league = SessionVariables.CurrentAccount.GetDefaultNewLeague();
                league.RegistrationModeId = RegistrationMode.Register;
            }

            LoadEditSetupViewBag(league, msgs);
            ViewBag.ActiveMenu = "EditSetup";

            return View(league);
        }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "seatsavailable", seatsavailable },
                { "waitinglistseats", waitinglistseats },
                { "registrationperiod", registrationperiod },
                { "registrationopendate", registrationopendate },
                { "registrationopentime", registrationopentime },
                { "registrationclosedate", registrationclosedate },
                { "registrationclosetime", registrationclosetime },
                { "timezone", timezone },
                { "form", form },
                { "regmode", regmode },
            };
            var league = SaveLeague(id, values, msgs);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "next":
                        ViewBag.ActiveMenu = "EditRestrictions";
                        return View("EditRestrictions", league);
                }
            }

            LoadEditSetupViewBag(league, msgs);
            ViewBag.ActiveMenu = "EditSetup";
            return View(league);
        }

        private void LoadEditSetupViewBag(ActivityGroup league, SystemMessages msgs)
        {
            var activeForm = league?.GetForm(msgs) ?? SessionVariables.CurrentAccount.GetDefaultForm(msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms(msgs).OrderBy(x => x.Name).ToList();
            ViewBag.ActiveForm = activeForm;
            ViewBag.IsFullScreen = GetIsFullScreen(league);
        }

        private bool GetIsFullScreen(ActivityGroup league)
        {
            ViewBag.FullScreenExitUrl = Url.Action("index", "sports");

            return Request["isnew"] == "1" && !league.IsPublished;
        }

        [HttpGet]
        public ActionResult EditRestrictions(string id)
        {
            var league = ActivityGroup.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditRestrictions";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditRestrictions(string id, string alloworganization, string agerestrictions, string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade, string genderrestrictions, string requiresparentalconsent, string cmd)
        {
            var msgs = new SystemMessages();
            var gradeIds = new List<int>();
            var grades = GradeLevel.Load();

            grades.ForEach(grade => {
                if (Request["grade-" + grade.ID.ToString()].IsNotEmpty())
                    gradeIds.Add(grade.ID.Value);
            });

            var values = new Dictionary<string, object>()
            {
                { "alloworganization", alloworganization.IfBlank("0")},
                { "agestart", agerestrictions.IsNumber() ? agestart : "-1"},
                { "ageend", agerestrictions.IsNumber() ? ageend : "-1"},
                { "gradestart", graderestrictions.IsNumber() ? mingrade : "-1"},
                { "gradeend", graderestrictions.IsNumber() ? maxgrade : "-1"},
                { "genderrestrictions", genderrestrictions},
                { "gradeIds", gradeIds.ToStrings().ToStringList() },
                { "requiresparentalconsent", requiresparentalconsent.IfBlank("0") }
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditSetup";
                        LoadEditSetupViewBag(league, msgs);
                        return View("EditSetup", league);
                    case "next":
                        ViewBag.ActiveMenu = "EditDetails";
                        return View("EditDetails", league);
                }
            }

            ViewBag.IsFullScreen = GetIsFullScreen(league);
            ViewBag.ActiveMenu = "EditRestrictions";
            return View(league);
        }

        [HttpGet]
        public ActionResult EditContact(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Instructors = SessionVariables.CurrentAccount.GetUsers(msgs: msgs);
            ViewBag.ActiveMenu = "EditContact";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditContact(string id, string contactname, string contactphone, string contactemail, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "contactname", contactname},
                { "contactphone", contactphone},
                { "contactemail", contactemail},
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditDetails";
                        return View("EditDetails", league);
                    case "next":
                        ViewBag.ActiveMenu = "EditCost";
                        return View("EditCost", league);
                }
            }

            ViewBag.ActiveMenu = "EditCost";
            return View(league);
        }

        [HttpGet]
        public ActionResult EditDetails(string id)
        {
            var league = ActivityGroup.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditDetails";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditDetails(string id, string description, string details, string cmd)
        {
            var msgs = new SystemMessages();

            if (details.IsEmpty())
                msgs.Add($"{UILabel.ActivityGroupLabel(ActivityGroupType.League)} Details is required", MessageType.UserError);

            
            var values = new Dictionary<string, object>()
            {
                { "details", details},
                { "description", description}
            };

            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditRestrictions";
                        return View("EditRestrictions", league);
                    case "next":
                        ViewBag.ActiveMenu = "EditContact";
                        return View("EditContact", league);
                }
            }

            ViewBag.ActiveMenu = "EditDetails";
            return View(league);
        }

        private void LoadEditCostViewBag(ActivityGroup league, SystemMessages msgs)
        {
            var canProcessPayments = SessionVariables.CurrentAccount.CanProcessPayments(msgs);

            if (!canProcessPayments)
                league.AllowToPayLater = true;

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.CanProcessPayments = canProcessPayments;
            ViewBag.PaymentPlan = league.GetPaymentPlan();
            ViewBag.ActiveMenu = "EditCost";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
        }

        [HttpGet]
        public ActionResult EditCost(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            LoadEditCostViewBag(league, msgs);
            return View(league);
        }

        [HttpPost]
        public ActionResult EditCost(string id, string cost, string costdescription, string allowtopaylater, string cmd, string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "cost", cost},
                { "costdescription", costdescription},
                { "allowtopaylater", allowtopaylater.IfBlank("0")},
                { "haspaymentplan", haspaymentplan.IfBlank("0") },
                { "paymentplancost", paymentplancost },
                { "paymentplanperiod", paymentplanperiod },
                { "paymentplanperiodnumber", paymentplanperiodnumber }
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        ViewBag.ActiveMenu = "EditContact";
                        return View("EditContact", league);
                    case "next":
                        ViewBag.ActiveMenu = "EditConfirmation";
                        return View("EditConfirmation", league);
                    case "publish":
                        return RedirectToAction("publish", "sports", new { id = league.Uid, isnew = Request["isnew"] });
                }
            }

            LoadEditCostViewBag(league, msgs);

            return View(league);
        }

        [HttpGet]
        public ActionResult EditConfirmation(string id)
        {
            var league = ActivityGroup.LoadByUid(id.ToGuid());

            ViewBag.Messages = new SystemMessages();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.ActiveMenu = "EditConfirmation";
            ViewBag.IsFullScreen = GetIsFullScreen(league);
            return View(league);
        }

        private ActivityGroup SaveLeague(string id, Dictionary<string, object> values, SystemMessages msgs)
        {
            var league = ActivityGroupService.SaveLeague(
                loggedInUser: SessionVariables.CurrentUser,
                account: SessionVariables.CurrentAccount,
                activityGroupUid: id,
                values: values,
                msgs: msgs
            );

            if (msgs.HasNoErrors())
                msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);

            return league;
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditConfirmation(string id, string registrationthankyouemail, string applicationthankyouemail, string cmd)
        {
            var msgs = new SystemMessages();
            var values = new Dictionary<string, object>()
            {
                { "registrationthankyouemail", registrationthankyouemail },
                { "applicationthankyouemail", applicationthankyouemail }
            };
            var league = SaveLeague(id, values, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen(league);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    case "prev":
                        if (SessionVariables.CurrentAccount.CanProcessPayments())
                        {
                            LoadEditCostViewBag(league, msgs);
                            return View("EditCost", league);
                        }
                        else
                        {
                            ViewBag.ActiveMenu = "EditContact";
                            return View("EditContact", league);
                        }
                    case "publish":
                        return RedirectToAction("publish", new { id = league.Uid, isnew = Request["isnew"] });
                }
            }

            ViewBag.ActiveMenu = "EditConfirmation";
            return View(league);
        }
    }
}