using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using Hangfire;
using AfterSchoolLibrary.Billing;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Helpers;
using System.IO;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ActivityController : BaseLoggedInController
    {
    public ActivityController() : base(AccountService.ProviderAccountTypes.ToList())
    {
    }

    public ActionResult Summary(string id)
    {
        var currentAccount = SessionVariables.CurrentAccount;
        var msgs = new SystemMessages();
        var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

        if (activity.IsEmpty())
            return HttpNotFound();

        var actions = WebSessionAction.LoadByAccountIdActivityId(
            activity.ID.Value,
            currentAccount.ID.Value,
            0,
            15,
            msgs);

		ViewBag.ParticipationObject = activity.GetParticipationObject(msgs);
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.Actions = actions;
		ViewBag.Schedule = activity.GetSchedule(msgs);
		ViewBag.TotalPaid = activity.GetTotalAmountPaid(msgs);
		ViewBag.Messages = msgs;

		return View(activity);
    }

        [HttpGet]
        public ActionResult Index(string group, string pn, string rpp, string archive, string copyerror, string type, string q)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var pageNumber = 1;
            var recordsPerPage = 10;
            var isArchived = false;
            var activityType = ActivityType.Activity;

		if (pn.IsNumber() && pn.ToInteger() > 0)
			pageNumber = pn.ToInteger();

		if (rpp.IsNumber() && rpp.ToInteger() > 0)
			recordsPerPage = rpp.ToInteger();

		if (archive.IsBoolean())
			isArchived = archive.ToBoolean();

		if (type.IsNotEmpty() && type.IsNumber())
			activityType = type.ToInteger().ToEnum<ActivityType>();

		var start = recordsPerPage * (pageNumber - 1);
		var length = recordsPerPage;

		List<Activity> activities = null;
		var totalActivityCount = 0;
		var hasArchivedActivities = false;

		ActivityGroup activityGroup = null;
		if (group.IsGuid())
		{
			activityGroup = currentAccount.GetActivityGroup(group.ToGuid(), msgs);
			totalActivityCount = activityGroup.GetActivityCount(ActivityType.Activity, isArchived, false, null, msgs);
			activities = activityGroup.GetActivities(ActivityType.Activity, isArchived, false, start, length, msgs: msgs);
			hasArchivedActivities = activityGroup.GetActivityCount(ActivityType.Activity, true, false, null, msgs) > 0;
		}
		else
		{
			var getUngroupActivitiesOnly = currentAccount.AccountTypeId == AccountType.School;

                totalActivityCount = currentAccount.GetActivityCount(
                    SessionVariables.AccountUserType,
                    SessionVariables.CurrentUser,
                    q,
                    activityType,
                    isArchived,
                    false,
                    getUngroupActivitiesOnly,
                    msgs);

                activities = currentAccount.GetActivities(
                    SessionVariables.AccountUserType,
                    SessionVariables.CurrentUser,
                    q,
                    activityType,
                    isArchived,
                    false,
                    getUngroupActivitiesOnly,
                    start,
                    length,
                    msgs);

                hasArchivedActivities = currentAccount.GetActivityCount(
                    SessionVariables.AccountUserType,
                    SessionVariables.CurrentUser,
                    q,
                    activityType,
                    true,
                    false,
                    getUngroupActivitiesOnly,
                    msgs) > 0;
            }

		var totalPages = Math.Ceiling(totalActivityCount.ToDouble() / recordsPerPage).ToInteger();

		var finalActivities = activities
			.OrderBy(x => x.Name)
			.ToList();

		if (!finalActivities.Any() && archive == "1")
			return RedirectToAction("index");

		if (copyerror.IsBoolean() && copyerror.ToBoolean())
			msgs.Add("Error creating copy", MessageType.Error);

		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.Groups = currentAccount.GetSessions(msgs);
		ViewBag.Group = activityGroup;
		ViewBag.NeedToUpgrade = totalActivityCount >= SessionVariables.AccountPackage.GetFeature(FeatureType.Activities, msgs).GetMaxValue();
		ViewBag.HasArchivedActivities = hasArchivedActivities;
		ViewBag.ActivityType = activityType;
		ViewBag.PagingModel = new PagingModel()
		{
			TotalCount = totalActivityCount,
			PageNumber = pageNumber,
			RecordsPerPage = recordsPerPage,
			TotalPages = totalPages
		};
		ViewBag.Messages = msgs;

		return View(finalActivities);
    }

    #region Edit Activity
    [HttpGet]
    public ActionResult EditSetup(string id, string group, string type)
    {
      var currentAccount = SessionVariables.CurrentAccount;
      var msgs = new SystemMessages();
      ActivityGroup activityGroup = null;
      Activity activity;

      if (id.IsGuid())
      {
        activity = currentAccount.GetActivity(id.ToGuid(), msgs);
        activityGroup = activity.GetActivityGroup(currentAccount.ID.Value, msgs);
      }
      else
      {
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
      }

		LoadEditSetupViewBag(activity, activityGroup, msgs);

		// we should default to have ContinuousEnrollment off
		if (id.IsEmpty())
			activity.IsContinuousEnrollment = false;

		ViewBag.ActiveMenu = "EditSetup";

		return View(activity);
    }

        [HttpPost]
        public ActionResult EditSetup(string id, string name, string seatsavailable, string waitinglistseats, string registrationperiod, string registrationopendate, string registrationopentime, string registrationclosedate, string registrationclosetime, string timezone, string form, string regmode, string group, string cmd, string type, string inviteonly, string minseatsrequired, string isAdvanceSettingEnabled, string costfrequencytype, string cutoffhours, string isContinuousEnrollmentEnabled)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

		Activity activity;
		Activity defaultActivity;
		ActivityGroup activityGroup = null;
		ActivityForm activeForm;

		if (id.IsEmpty() || !id.IsGuid())
			return HttpNotFound();

		activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		defaultActivity = currentAccount.GetDefaultNewActivity();

		if (name.IsEmpty())
			msgs.Add("Activity name is required.", MessageType.UserError);
		else if (name.Length > 75)
			msgs.Add("Activity name must not be greater than 75 characters.", MessageType.UserError);

		if (form.IsEmpty() && (isAdvanceSettingEnabled.IsEmpty() || !isAdvanceSettingEnabled.ToBoolean()))
		{
			if (activity.IsNotEmpty())
			//.. edit activity
			activeForm = activity.GetActiveActivityForm(msgs);
			else
			//.. create activity
			activeForm = currentAccount.GetDefaultActivityForm(msgs);

			if (activeForm.IsEmpty())
			{
			var defaultForm = currentAccount.CreateDefaultForm(currentUser, msgs);
			currentAccount.SetDefaultForm(defaultForm, currentUser);

			activeForm = currentAccount.GetDefaultActivityForm(msgs);
			}

			form = activeForm.FormId.Value.ToString();
		}

            if (msgs.HasNoErrors())
            {
                var values = new Dictionary<string, object>()
                {
                    { "name", name.MaxChars(75) },
                    { "seatsavailable", seatsavailable },
                    { "waitinglistseats", waitinglistseats },
                    { "registrationperiod", registrationperiod },
                    { "registrationopendate", registrationopendate },
                    { "registrationopentime", registrationopentime },
                    { "registrationclosedate", registrationclosedate },
                    { "registrationclosetime", registrationclosetime },
                    { "timezone", timezone },
                    { "form", form },
                    { "regmode", regmode.IsNotEmpty() ? regmode : ((int)RegistrationMode.Register).ToString() },
                    { "group", group },
                    { "type", type },
                    { "inviteonly", inviteonly.IsBoolean().ToString() },
                    { "minseatsrequired", minseatsrequired },
                    { "gradestart", activity.IsNotEmpty()  ? activity.GradeLevelStart : defaultActivity.GradeLevelStart },
                    { "gradeend", activity.IsNotEmpty() ? activity.GradeLevelEnd :defaultActivity.GradeLevelEnd },
                    { "genderrestrictions", activity.IsNotEmpty() && activity.GenderRestriction.IsNotEmpty() ? (int)activity.GenderRestriction :(int)defaultActivity.GenderRestriction },
                    { "agestart", activity.IsNotEmpty() ? activity.AgeStart : defaultActivity.AgeStart },
                    { "ageend",activity.IsNotEmpty() ? activity.AgeEnd : defaultActivity.AgeEnd },
                    { "costfrequencytype", costfrequencytype },
                    { "cutoffhours", cutoffhours },
                    { "isContinuousEnrollmentEnabled", isContinuousEnrollmentEnabled.IsNotEmpty() && isContinuousEnrollmentEnabled.ToBoolean() },
                };

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
			if (msgs.HasNoErrors() && !SessionVariables.AccountPackage.HasFeature(FeatureType.ManageForms, msgs))
			{
				var defaultForm = currentAccount.GetDefaultForm(msgs);
				var newForm = defaultForm.Copy(currentUser.ID.Value, currentAccount.ID.Value, FormType.Registration, (name + " form").MaxChars(100, "..."), msgs);
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
			values.Add("interestthankyouemail", "Thank you for your interest. We will be in touch with you soon to follow up.");

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

			switch (cmd)
			{
			case "next":
				switch (activity.ActivityTypeId)
				{
				case ActivityType.LearningPod:
				case ActivityType.Activity:
					LoadEditSetupViewBag(activity, activityGroup, msgs);
					ViewBag.ActiveMenu = "EditRestrictions";
					return View("EditRestrictions", activity);

				case ActivityType.FieldTrip:
				case ActivityType.Team:
					LoadEditScheduleViewBag(activity, activity.ActivityScheduleId?.ToString(), msgs);
					ViewBag.ActiveMenu = "EditSchedule";
					return View("editschedule", activity);

				case ActivityType.PermissionSlip:
					LoadEditSetupViewBag(activity, activityGroup, msgs);
					ViewBag.ActiveMenu = "EditDetails";
					return View("EditDetails", activity);
				}
				break;
			}
		}

		LoadEditSetupViewBag(activity, activityGroup, msgs);
		ViewBag.ActiveMenu = "EditSetup";

		return View(activity);
    }

    private void LoadEditSetupViewBag(Activity activity, ActivityGroup activityGroup, SystemMessages msgs)
    {
		var currentAccount = SessionVariables.CurrentAccount;

		if (msgs.IsEmpty())
			msgs = new SystemMessages();

		var activeForm = activity.GetActiveActivityForm(msgs) ?? activityGroup?.GetDefaultActivityForm(msgs) ?? currentAccount.GetDefaultActivityForm(msgs);
		var periods = currentAccount.GetOpenAndUpcomingRegistrationPeriods(msgs);
		var activePeriod = activity.GetActiveRegistrationPeriod(msgs) ?? currentAccount.GetDefaultNewRegistrationPeriod();

		periods.Add(activePeriod);
		periods = periods.OrderBy(period => period.StartDateTime.Value).ToList();

		ViewBag.Group = activityGroup;
		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.Forms = currentAccount.GetForms(msgs).OrderBy(x => x.Name).ToList();
		ViewBag.Periods = periods;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.ActivePeriod = activePeriod;
		ViewBag.ActiveForm = activeForm;
		ViewBag.IsFullScreen = GetIsFullScreen(activity, activityGroup?.Uid.ToString());
		ViewBag.Messages = msgs;
    }

    private bool GetIsFullScreen(Activity activity, string groupUid = "")
    {
		ViewBag.FullScreenExitUrl = Url.Action("index", "activity", new { type = (int)activity.ActivityTypeId });

		if (groupUid.IsNotEmpty())
			ViewBag.FullScreenExitUrl = Url.Action("activities", "group", new { id = groupUid });
		else if (activity.ActivityGroupId.HasValue)
			ViewBag.FullScreenExitUrl = Url.Action("activities", "group", new { id = activity.ActivityGroupUid });

		return Request["isnew"] == "1" && !activity.IsPublished;
    }

    [HttpGet]
    public ActionResult EditRestrictions(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		ViewBag.Messages = msgs;
		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.ActiveMenu = "EditRestrictions";
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		return View(activity);
    }

    [HttpPost]
    public ActionResult EditRestrictions(string id, string alloworganization, string restrictbygrade, string agestart, string ageend, string graderestrictions, string mingrade, string maxgrade, string genderrestrictions, string requiresparentalconsent, string cmd)
    {
		var msgs = new SystemMessages();
		Activity activity = null;
		var accountPackage = SessionVariables.AccountPackage;
		var currentUser = SessionVariables.CurrentUser;
		var currentAccount = SessionVariables.CurrentAccount;

		activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		if (activity.IsNotEmpty())
		{
			if (!accountPackage.HasFeature(FeatureType.AllowStudentRegistration) && requiresparentalconsent.IsBlank())
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

			var values = new Dictionary<string, object>()
					{
						{ "alloworganization", alloworganization.IfBlank("0")},
						{ "agestart", agestart },
						{ "ageend", ageend },
						{ "gradestart", mingrade },
						{ "gradeend", maxgrade },
						{ "genderrestrictions", genderrestrictions},
						{ "requiresparentalconsent", requiresparentalconsent.IfBlank("0") }
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

		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = accountPackage;

		if (msgs.HasNoErrors())
		{
			switch (cmd)
			{
			case "prev":
				ViewBag.ActiveMenu = "EditSetup";
				LoadEditSetupViewBag(activity, activity.GetActivityGroup(msgs), msgs);
				return View("EditSetup", activity);
			case "next":
				ViewBag.ActiveMenu = "EditSchedule";
				var scheduleId = activity.ActivityScheduleId?.ToString();
				LoadEditScheduleViewBag(activity, scheduleId, msgs);
				return View("EditSchedule", activity);
			}
		}

		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.ActiveMenu = "EditRestrictions";
		ViewBag.Messages = msgs;

		return View(activity);
    }

    [HttpGet]
    public ActionResult EditContact(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
		ViewBag.ActiveMenu = "EditContact";
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.Messages = msgs;

		return View(activity);
    }

    [HttpPost]
    public ActionResult EditContact(string id, string leadertype, string contactname, string contactphone, string contactemail, string instructor, string instructorfirstname, string instructorlastname, string instructoremail, string instructorphone, string cmd, string provider, string providername, string providerfirstname, string providerlastname, string provideremail, string providerphone)
    {
		var msgs = new SystemMessages();
		var currentAccount = SessionVariables.CurrentAccount;
		var currentUser = SessionVariables.CurrentUser;
		var values = new Dictionary<string, object>() {
					{ "leadertype", leadertype.IfBlank("0") },
					{ "contactname", contactname},
					{ "contactphone", contactphone},
					{ "contactemail", contactemail},
					{ "instructor", instructor },
					{ "instructorfirstname", instructorfirstname },
					{ "instructorlastname", instructorlastname },
					{ "instructoremail", instructoremail },
					{ "instructorphone", instructorphone },
					{ "provider", provider },
					{ "providername", providername },
					{ "providerfirstname", providerfirstname },
					{ "providerlastname", providerlastname },
					{ "provideremail", provideremail },
					{ "providerphone", providerphone }
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

		ViewBag.Messages = msgs;
		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
		ViewBag.IsFullScreen = GetIsFullScreen(activity);

		if (msgs.HasNoErrors())
		{
			if (beforeProviderAccountId != activity.ProviderAccountId)
			ActivityService.NotifyProviderOfNewActivity(activity, currentUser, msgs);

			switch (cmd)
			{
			case "prev":
				ViewBag.ActiveMenu = "EditDetails";
				return View("EditDetails", activity);
			case "next":
				if (activity.ActivityTypeId.In(new ActivityType[] { ActivityType.Activity, ActivityType.LearningPod }))
				{
				ViewBag.ActiveMenu = "EditLocation";
				return View("EditLocation", activity);
				}
				else
				{
				LoadEditCostViewBag(activity, msgs);
				ViewBag.ActiveMenu = "EditCost";
				return View("editcost", activity);
				}
			}
		}

		ViewBag.ActiveMenu = "EditContact";
		return View(activity);
    }

    [HttpGet]
    public ActionResult EditDetails(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		ViewBag.Account = currentAccount;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.ActiveMenu = "EditDetails";
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.Messages = msgs;

		return View(activity);
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
			var values = new Dictionary<string, object>()
					{
						{ "details", details},
						{ "description", description}
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

		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.IsFullScreen = GetIsFullScreen(activity);

		if (msgs.HasNoErrors())
		{
			switch (cmd)
			{
			case "prev":
				ViewBag.ActiveMenu = "EditSchedule";
				var scheduleId = activity.ActivityScheduleId?.ToString();
				LoadEditScheduleViewBag(activity, scheduleId, msgs);
				ViewBag.Messages = msgs;
				return View("EditSchedule", activity);
			case "next":
				ViewBag.ActiveMenu = "EditContact";
				ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
				ViewBag.Messages = msgs;
				return View("EditContact", activity);
			}
		}

		ViewBag.ActiveMenu = "EditDetails";
		ViewBag.Messages = msgs;
		return View(activity);
    }

    private void LoadEditScheduleViewBag(Activity activity, string scheduleId, SystemMessages msgs)
    {
		var currentAccount = SessionVariables.CurrentAccount;

		if (msgs.IsEmpty())
			msgs = new SystemMessages();

		ViewBag.Account = currentAccount;
		ViewBag.ActivityId = activity.Uid.ToString();
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.ActiveMenu = "EditSchedule";
		ViewBag.Schedules = currentAccount.GetActivitySchedules(msgs);
		ViewBag.IsFullScreen = GetIsFullScreen(activity);

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

		if (schedule.IsEmpty())
			schedule = ActivitySchedule.GetNewDefaultSchedule(activity, msgs);

		ViewBag.Schedule = schedule;
		ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups, msgs);
		ViewBag.Messages = msgs;
    }

    [HttpGet]
    public ActionResult EditSchedule(string id, string schedule)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		LoadEditScheduleViewBag(activity, schedule, msgs);
		return View(activity);
    }

    [HttpPost]
    [ValidateInput(false)]
    public ActionResult EditSchedule(string id, string cmd, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing, string hasschedule)
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
			if (activity.CostFrequencyTypeId == ActivityCostFrequencyType.PerClass && (!hasschedule.IsBoolean() || !hasschedule.ToBoolean()))
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
				inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));

				outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

				inSessionDates.ForEach(instance =>
				{
				if (instance.StartDateTime > instance.EndDateTime)
					msgs.Add("Please ensure start dates and end times are earlier than the corresponding end date and end times", MessageType.UserError);
				});
			}

			if(startdate.IsNotEmpty() && enddate.IsNotEmpty())
			{
				DateTime startDate = startdate.ToDateTime();
				DateTime endDate = enddate.ToDateTime();
				if (endDate < startDate)
				{
					msgs.Add("Please ensure start dates and start times are earlier than the corresponding end date and end times", MessageType.UserError);
				}
			}

			if (msgs.HasNoErrors())
			{
				values = new Dictionary<string, object>()
						{
							{ "scheduledetails", scheduledetails},
							{ "days", days.Select(x => (int)x).ToStrings().ToStringList() },
							{ "repeatsweekly", repeatsweekly.IfBlank("0") },
							{ "startdate", startdate },
							{ "enddate", enddate },
							{ "dailystarttime", dailystarttime },
							{ "dailyendtime", dailyendtime },
							{ "scheduledates", inSessionDates },
							{ "scheduledatesclosed", outOfSessionDates },
							{ "schedule", schedule },
							{ "isongoing", isongoing }
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

		ActivitySchedule sched = null;

		if (msgs.HasNoErrors())
			sched = activity.GetSchedule(msgs: msgs);
		else
			sched = ActivitySchedule.GetNewDefaultSchedule(inSessionDates.Select(item => item.ToDateRange()).ToList());

		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = currentAccountPackage;
		ViewBag.Schedule = sched;
		ViewBag.Schedules = currentAccount.GetActivitySchedules(msgs);
		ViewBag.ShowScheduleList = currentAccountPackage.HasFeature(FeatureType.ManageActivityGroups, msgs);
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.Messages = msgs;

		if (msgs.HasNoErrors())
		{
			switch (cmd)
			{
			case "prev":
				ViewBag.ActiveMenu = "EditRestrictions";
				return View("EditRestrictions", activity);
			case "next":
				if (activity.ActivityTypeId != ActivityType.Team)
				{
				ViewBag.ActiveMenu = "EditDetails";
				return View("EditDetails", activity);
				}
				else
				{
				ViewBag.ActiveMenu = "EditContact";
				ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
				return View("editcontact", activity);
				}
			}
		}

		if (ViewBag.Schedule == null)
			ViewBag.Schedule = ActivitySchedule.GetNewDefaultSchedule(inSessionDates.Select(item => item.ToDateRange()).ToList());

            ViewBag.Messages = msgs;
            ViewBag.ActiveMenu = "EditSchedule";
            ViewBag.hasSchedule = hasschedule;
            return View(activity);
        }

    [HttpGet]
    public ActionResult EditLocation(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		ViewBag.Account = currentAccount;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.ActiveMenu = "EditLocation";
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.Messages = msgs;

		return View(activity);
    }

    [ValidateInput(false)]
    [HttpPost]
    public ActionResult EditLocation(string id, string location, string locationname, string locationdetails, string address1, string streetnumber, string streetname, string address2, string neighborhood, string city, string county, string state,
        string postalcode, string country, string placeid, decimal latitude, decimal longitude, string mylocation, string locationtype, string cmd, string activitylocationtype, string virtuallocation, string hidepreciselocation = "0")
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var currentUser = SessionVariables.CurrentUser;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		if (location.IsEmpty())
			msgs.Add("Location is required", MessageType.UserError);

		if (activity.IsNotEmpty() && msgs.HasNoErrors())
		{
			var values = new Dictionary<string, object>()
					{
						{ "location", location},
						{ "locationname", locationname},
						{ "locationdetails", locationdetails},
						{ "address1", address1},
						{ "streetnumber", streetnumber},
						{ "streetname", streetname},
						{ "address2", address2},
						{ "neighborhood", neighborhood},
						{ "city", city},
						{ "county", county},
						{ "state", state},
						{ "postalcode", postalcode},
						{ "country", country},
						{ "placeid", placeid},
						{ "latitude", latitude },
						{ "longitude", longitude },
						{ "mylocation", mylocation },
						{ "locationtype", locationtype },
						{ "activitylocationtype", activitylocationtype },
						{ "virtuallocation", virtuallocation },
						{ "hidepreciselocation", hidepreciselocation }
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

		ViewBag.Messages = msgs;
		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.IsFullScreen = GetIsFullScreen(activity);

		if (msgs.HasNoErrors())
		{
			switch (cmd)
			{
			case "prev":
				ViewBag.ActiveMenu = "EditContact";
				ViewBag.Instructors = currentAccount.GetUsers(msgs: msgs);
				return View("EditContact", activity);
			case "next":
				if (SessionVariables.AccountPackage.HasFeature(FeatureType.AcceptPayments))
				{
				LoadEditCostViewBag(activity, msgs);
				return View("EditCost", activity);
				}
				else
				{
				ViewBag.ActiveMenu = "EditConfirmation";
				return View("EditConfirmation", activity);
				}
			}
		}

		ViewBag.ActiveMenu = "EditLocation";
		return View(activity);
		}

		private void LoadEditCostViewBag(Activity activity, SystemMessages msgs)
		{
		if (msgs.IsEmpty())
			msgs = new SystemMessages();

		var currentAccount = SessionVariables.CurrentAccount;
		var canProcessPayments = currentAccount.CanProcessPayments(msgs);


		ViewBag.Account = currentAccount;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.CanProcessPayments = canProcessPayments;
		ViewBag.PaymentPlan = activity.GetPaymentPlan(msgs);
		ViewBag.SubscriptionPlan = activity.GetSubscriptionPlan(msgs);
		ViewBag.PostPayPlan = activity.GetPostPayPlan(msgs);
		ViewBag.ActiveMenu = "EditCost";
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.hasSchedule = activity.HasSchedule;
		ViewBag.Messages = msgs;
    }

    [HttpGet]
    public ActionResult EditCost(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		LoadEditCostViewBag(activity, msgs);
		return View(activity);
    }

    [HttpPost]
    public ActionResult EditCost(string id, string cost, string costdescription, string acceptpaypal, string acceptcreditcard, string allowtopaylater, string cmd,
        string hassubscriptionplan, string subscriptionplancost, string subscriptionplanperiod, string isongoingsubscription, string subscriptionPlanPeriodNumber, string subscriptionPaymentStartType, string subscriptionStartDate, string subscriptionEndDate,
        string haspaymentplan, string paymentplancost, string paymentplanperiod, string paymentplanperiodnumber, string paymentplanpaymentstarttype, string splitcostevenly, string regfeedownpayment,
        string haspostpayplan, string postpaycost, string isparentpricepostpay, string isparentpricecost, string isparentpricepaymentplan, string isparentpricesubscription, string prepaidperiodspostpay, string postpayplanperiod, string cutoffhours)
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

			if (!cost.IsNumber())
			cost = "0";

			var values = new Dictionary<string, object>()
					{
						{ "cost", cost},
						{ "costdescription", costdescription},
						{ "acceptpaypal", acceptpaypal.IfBlank("0")},
						{ "acceptcreditcard", acceptcreditcard.IfBlank("0")},
						{ "allowtopaylater", allowtopaylater.IfBlank("0")},
						{ "hassubscriptionplan", hassubscriptionplan.IfBlank("0") },
						{ "subscriptionplancost", subscriptionplancost },
						{ "subscriptionplanperiod", subscriptionplanperiod },
						{ "subscriptionPlanPeriodNumber", subscriptionPlanPeriodNumber },
						{ "subscriptionPaymentStartType", subscriptionPaymentStartType },
						{ "subscriptionStartDate", subscriptionStartDate },
						{ "subscriptionEndDate", subscriptionEndDate },
						{ "isongoingsubscription", !activity.HasSchedule },
						{ "haspaymentplan", haspaymentplan.IfBlank("0") },
						{ "paymentplancost", paymentplancost },
						{ "paymentplanperiod", paymentplanperiod },
						{ "paymentplanperiodnumber", paymentplanperiodnumber },
						{ "paymentplanpaymentstarttype", paymentplanpaymentstarttype },
						{ "splitcostevenly", splitcostevenly.IsBoolean().ToString() },
						{ "regfeedownpayment", regfeedownpayment.IsBoolean().ToString() },
						{ "haspostpayplan", haspostpayplan.IfBlank("0") },
						{ "postpaycost", postpaycost },
						{ "isparentpricepostpay", isparentpricepostpay },
						{ "isparentpricecost", isparentpricecost },
						{ "isparentpricepaymentplan", isparentpricepaymentplan},
						{ "isparentpricesubscription", isparentpricesubscription },
						{ "prepaidperiodspostpay", prepaidperiodspostpay },
						{ "postpayplanperiod", postpayplanperiod },
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

		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.Messages = msgs;

		if (msgs.HasNoErrors())
		{
			switch (cmd)
			{
			case "prev":
				ViewBag.ActiveMenu = "EditLocation";
				return View("EditLocation", activity);
			case "next":
				ViewBag.ActiveMenu = "EditConfirmation";
				return View("EditConfirmation", activity);
			case "done":
				return RedirectToAction("activities", "group", new { id = activity.ActivityGroupUid });
			case "publish":
				return RedirectToAction("publish", "activity", new { id = activity.Uid, isnew = Request["isnew"], source = Request["source"] });
			}
		}

		LoadEditCostViewBag(activity, msgs);
		return View(activity);
		}

		[HttpGet]
		public ActionResult EditConfirmation(string id)
		{
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		ViewBag.Account = currentAccount;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.ActiveMenu = "EditConfirmation";
		ViewBag.IsFullScreen = GetIsFullScreen(activity);
		ViewBag.Messages = msgs;

		return View(activity);
    }

    [ValidateInput(false)]
    [HttpPost]
    public ActionResult EditConfirmation(string id, string registrationthankyouemail, string applicationthankyouemail, string interestthankyouemail, string cmd)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var currentUser = SessionVariables.CurrentUser;
		var msgs = new SystemMessages();

		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		if (id.IsGuid())
		{
			var values = new Dictionary<string, object>()
					{
						{ "registrationthankyouemail", registrationthankyouemail },
						{ "interestthankyouemail", interestthankyouemail },
						{ "applicationthankyouemail", applicationthankyouemail }
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

		ViewBag.Account = currentAccount;
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.IsFullScreen = GetIsFullScreen(activity);

		if (msgs.HasNoErrors())
		{
			switch (cmd)
			{
			case "prev":
				if (activity.CanAcceptPayments(msgs))
				{
				LoadEditCostViewBag(activity, msgs);
				return View("EditCost", activity);
				}
				else
				{
				ViewBag.Messages = msgs;
				ViewBag.ActiveMenu = "EditLocation";
				return View("EditLocation", activity);
				}
			case "publish":
				ViewBag.Messages = msgs;
				return Redirect(activity.ShortHomePageUrl);

			default:
				ViewBag.Messages = msgs;
				return RedirectToAction("requesttopublish", new { id = activity.Uid, isnew = Request["isnew"], source = Request["source"] });
			}
		}

		ViewBag.Messages = msgs;
		ViewBag.ActiveMenu = "EditConfirmation";
		return View(activity);
    }

    #endregion

    #region Copy, Delete, Archive, Restore
    [HttpPost]
    public ActionResult Copy(string id, string activitynamecopy, string usesameformcopy, string copywithregistrants, string type)
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
			attendances = activity.GetCompletedAttendanceWithPaging(orderByParam, orderbyDirection, start, length, msgs);
			totalAttendanceCount = activity.GetTotalAttendanceCount(msgs);

			if (totalAttendanceCount > 0)
			totalPages = Math.Ceiling(totalAttendanceCount.ToDouble() / recordsPerPage).ToInteger();
		}

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.Attendances = attendances;
		ViewBag.PagingModel = new PagingModel()
		{
			TotalCount = totalAttendanceCount,
			PageNumber = pageNumber,
			RecordsPerPage = recordsPerPage,
			TotalPages = totalPages
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
		var activitySchedule = activity.GetSchedule(msgs);

		if (attendance.IsEmpty())
			return RedirectToAction("Attendance", new { id, msg = "Error: No classes have taken place yet" });

		var attendanceRegistrants = GetAttendanceRegistrants(attendance, activity, activitySchedule, msgs);

		ViewBag.AttendanceRegistrants = attendanceRegistrants;
		ViewBag.Schedule = activitySchedule;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.RegistrantActivities = activity.GetRegistrantActivities(msgs);
		ViewBag.Messages = msgs;
		return View(attendance);
    }

    private List<AttendanceRegistrant> GetAttendanceRegistrants(Attendance attendance, Activity activity, ActivitySchedule schedule, SystemMessages msgs)
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
			attendance.UpdateAttendance(attendance.ActivityInstanceId.Value, presentStudents, SessionVariables.CurrentUser, msgs);

		ViewBag.AttendanceRegistrants = GetAttendanceRegistrants(attendance, activity, activitySchedule, msgs);
		ViewBag.Schedule = activitySchedule;
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.RegistrantActivities = activity.GetRegistrantActivities(msgs);
		ViewBag.Messages = msgs;

		return View(attendance);
    }

    // This returns all unbilled selected Attendances by Registrant
    private Dictionary<RegistrantActivity, List<Attendance>> GetAttendancesToBillPerRegistrantActivity(Account currentAccount, List<RegistrantActivity> registrantActivities, List<Attendance> attendances, BillingPeriodType billingPeriodType, SystemMessages msgs)
    {
		Dictionary<RegistrantActivity, List<Attendance>> registrantActivityAttendanceCounts = new Dictionary<RegistrantActivity, List<Attendance>>();

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
						var attendanceRegistrant = currentAccount.GetAttendanceRegistrant(activity, attendance.ID.Value, registrantActivity.RegistrantId.Value, msgs);
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
						var attendanceRegistrant = currentAccount.GetAttendanceRegistrant(activity, attendance.ID.Value, registrantActivity.RegistrantId.Value, msgs);

						if (attendanceRegistrant.IsNotEmpty() && firstAttendanceOfWeek.IsNotEmpty() && !attendanceRegistrant.IncludeNextAvailablePrePayment(currentUser, msgs) && !attendancesToCharge.Any(item => item.ID == firstAttendanceOfWeek.ID))
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
    private Dictionary<Attendance, List<int>> GetNumberOfRegistrantsBilledPerAttendance(List<Attendance> attendances, BillingPeriodType billingPeriodType, SystemMessages msgs = null)
    {
		var numberOfRegistrantsBilledPerAttendance = new Dictionary<Attendance, List<int>>();

		attendances.ForEach(attendance =>
		{
			var registrantIdsBilled = AttendanceRegistrant.LoadByAttendanceId(attendance.ID.Value, msgs).Where(ar => ar.IsAlreadyBilled).Select(rb => rb.RegistrantId.Value).Distinct().ToList();

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
				var attendanceIsInSameWeekAsExisitingAttendanceInList = numberOfRegistrantsBilledPerAttendance.FirstOrDefault(keyPair => keyPair.Key.Date.Value.DatesAreInSameWeek(attendance.Date.Value)).Key;
				if (attendanceIsInSameWeekAsExisitingAttendanceInList.IsNotEmpty())
				{
				registrantIdsBilled.ForEach(rbId =>
					{
					if (!numberOfRegistrantsBilledPerAttendance[attendanceIsInSameWeekAsExisitingAttendanceInList].Contains(rbId))
						numberOfRegistrantsBilledPerAttendance[attendanceIsInSameWeekAsExisitingAttendanceInList].Add(rbId);
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
			var registrantActivities = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs);
			var billingPeriodType = activity.GetPostPayPlan(msgs).BillingPeriodTypeId;

			ViewBag.AttdanceIds = attendanceIds;
			ViewBag.BillingPeriodType = billingPeriodType;
			ViewBag.AttendancesToBillRegistrants = GetAttendancesToBillPerRegistrantActivity(currentAccount, registrantActivities, selectedAttendances, billingPeriodType, msgs);
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
		var defaultMsg = "An error occurred while processing payments. Please contact support, do not attempt to charge attendance again";
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
			var registrantActivities = RegistrantActivity.LoadByParticipationObject(activity.ID.Value, ParticipationObjectType.Activity, msgs);
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
			var costPerAttendance = registration.CostPerClass.IsNotEmpty() ? registration.CostPerClass.Value : 0;

			if (costPerAttendance > 0)
			{
				var masterRegistrant = registration.GetMasterRegistrant(msgs);
				var raAttendances = registrantActivityAttendance.Value;
				var masterRegistrantAccount = masterRegistrant.GetAccount(msgs);
				var description = $"{registrantActivity.GetActivity(msgs).Name.ToProperCase()} attendance Fee times {raAttendances.Count}";
				var convenienceFeePayer = registration.GetConvenienceFeePayer(msgs);
				var paymentTypeFeePayer = registration.GetPaymentTypeFeePayer(msgs);
				var availablePrePayments = registration.GetAvailablePrePayments(activity, msgs);
				var attendanceDaysToCharge = Math.Max(0, raAttendances.Count - availablePrePayments.Count);
				CheckoutObject checkoutObject = null;
				var coupon = CouponService.GetCoupon(registration, msgs);

				if (attendanceDaysToCharge > 0)
				{
				BankAccountPurchaseObject bankAccountPurchaseObject = null;
				CreditCardPurchaseObject creditCardPurchaseObject = null;

				if (registration.PaymentTypeId == PaymentType.ACH)
					bankAccountPurchaseObject = new BankAccountPurchaseObject { StripeSourceId = registration.StripeSourceId };
				else
					creditCardPurchaseObject = new CreditCardPurchaseObject { StripeSourceId = registration.StripeSourceId };

				var overrideAmount = costPerAttendance * attendanceDaysToCharge;

				if (postPayPlan.IsNotEmpty())
				{
					//.. if post pay plan, then we will have coupon value in registration to discount.
					var couponDiscount = attendanceDaysToCharge * (registration.CouponValue.HasValue ? registration.CouponValue.Value : 0);
					overrideAmount = (costPerAttendance * attendanceDaysToCharge) - couponDiscount;
				}

				checkoutObject = new CheckoutObject()
				{
					Coupon = coupon,
					OverrideAmount = overrideAmount,
					ConvenienceFeeOverrideAmount = registration.ConvenienceFeePaid.IfEmpty(0) * attendanceDaysToCharge,
					PaymentTypeFeeOverrideAmount = registration.PaymentTypeFeePaid.IfEmpty(0) * attendanceDaysToCharge,
					PayingAccount = masterRegistrantAccount,
					BillingAccount = activity.GetAccount(msgs),
					CreditCardPurchaseObject = creditCardPurchaseObject,
					BankAccountPurchaseObject = bankAccountPurchaseObject,
					PaymentType = bankAccountPurchaseObject.IsNotEmpty() ? PaymentType.ACH : PaymentType.CreditCard,
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
					AccountService.ProcessPayment(
						checkoutObject,
						isAttendanceCharge: true,
						loggedInUser: currentUser,
						msgs: msgs);
				}

				if (msgs.HasNoErrors())
				raAttendances.ForEach(raAttendance =>
				{
					var attendanceRegistrant = AttendanceRegistrant.LoadByAttendanceIdByRegistrantId(
																raAttendance.ID.Value,
																registrantActivity.GetRegistrant(msgs).ID.Value,
																msgs)
															.FirstOrDefault();

					// add next available pre payment if available, if not then continue to set the checkoutobject transaction id
					if (attendanceRegistrant.IsNotEmpty()
						&& !attendanceRegistrant.IncludeNextAvailablePrePayment(currentUser, msgs)
						&& (checkoutObject?.Transaction).IsNotEmpty()
						&& !attendanceRegistrant.IsAlreadyBilled)
					{
					attendanceRegistrant.TransactionId = checkoutObject.Transaction.ID.Value;

					attendanceRegistrant
										.GetAttendanceRegistrantsInSameBillingPeriod(postPayPlan.BillingPeriodTypeId, msgs)
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
				msg = $"An error occured while charging some parents, some parents may have already been processed and you will still receive a summary email if any have. Please contact support and do NOT attempt to charge for attendance again.";
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
			EmailHelper.SendAttendanceChargeConfirmation(registrantsBilledPerAttendance, activity, costPerClass, couponValue, null, SessionVariables.CurrentUser, false, msgs);
		}

		ViewBag.Messages = msgs;
		return RedirectToAction("attendance", new { id = activityId, msg = msg });
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

		ViewBag.Surveys = SurveyRequest.LoadByActivityId(activity.ID.Value, msgs).OrderByDescending(a => a.CreateDate).ToList();
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

		LoadEditSetupViewBag(activity, activityGroup, msgs);

		var activityRegistrants = activity.GetRegistrantActivitiesForSurvey(msgs);
		var studentList = activityRegistrants.Select(m => new SelectListItem()
		{
			Text = m.RegistrantName,
			Value = m.RegistrantId.Value.ToString()
		});

		ViewBag.Students = studentList.ToList();
		ViewBag.Messages = msgs;

		return View(activity);
    }


    [HttpPost]
    public ActionResult SaveSurvey(string id, string activityId, string formId, string name, string optionalText, string dueDate, string isEveryIndividualSelected, string isAllowMultiple)
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
			var surveyRequest = SurveyRequestService.AddSurvey(formId.ToInteger(), activityId.ToInteger(), name, dueDate, optionalText, isAllowMultiple.ToBoolean(), currentAccount.ID.Value, currentUser.ID.Value, msgs);

			if (isEveryIndividualSelected.ToBoolean())
			CreateSurveys(surveyRequest.ID.Value, studentList.Where(a => a.Selected).ToList(), currentUser.ID.Value, msgs);
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

    private static IDictionary<string, string> ToDictionary(System.Collections.Specialized.NameValueCollection source)
    {
		return source.Cast<string>().Select(s => new { Key = s, Value = source[s] }).ToDictionary(p => p.Key, p => p.Value);
    }

    private static List<SelectListItem> ConvertToStudentsList(IDictionary<string, string> students)
    {
		var studentsList = new List<SelectListItem>();
		for (var i = 0; i < students.Count();)
		{
			studentsList.Add(new SelectListItem { Text = students.ElementAt(i + 2).Value, Value = students.ElementAt(i + 1).Value, Selected = students.ElementAt(i).Value.Contains("true") });
			i = i + 3;
		}
		return studentsList;
    }

    private static void CreateSurveys(int surveyRequestId, List<SelectListItem> studentList, int currentUserId, SystemMessages msgs, DatabaseConnection dc = null)
    {
		studentList.ForEach(student =>
		{
			var surveyResponse = SurveyResponseService.CreateSurveyFeedback(surveyRequestId, student.Value.ToInteger(), SurveyFeedbackStatus.New, currentUserId, msgs);
			var masterRegistrant = Registrant.Load(student.Value.ToInteger())?.GetMasterRegistrant(msgs, dc);

			if (masterRegistrant.IsNotEmpty() && masterRegistrant.WebUserId.IsEmpty())
			{
			var inviteCode = RegistrantInviteCodeService.GetInviteCode(surveyResponseId: surveyResponse.ID.Value,
				registrantId: surveyResponse.RegistrantId.Value,
				currentUserId: currentUserId,
				msgs: msgs,
				dc: dc);

			BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value, surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, inviteCode.Code, inviteCode.ID.Value, null));
			}
			else
			BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value, surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, null, 0, null));
		});
    }
    [HttpGet]
    public ActionResult ResendNotification(int id, int surveyId)
    {
		if (id == 0)
		{
			var surveyResponses = SurveyResponse.LoadBySurveyRequestId(surveyId).Where(a => a.SurveyFeedbackStatusTypeId != SurveyFeedbackStatus.Completed).ToList();
			surveyResponses.ForEach(surveyResponse =>
			{
			CheckRegistrantInviteCodeAndSendEmail(surveyResponse);
			});
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
		var checkIfInviteCodeExists = RegistrantInviteCode.CheckBySurveyResponseIdExpiryDate(surveyResponse.ID.Value, DateTime.Now);
		if (checkIfInviteCodeExists.IsNotEmpty())
			BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value, surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, checkIfInviteCodeExists.Code, checkIfInviteCodeExists.ID.Value, null));
		else
			BackgroundJob.Enqueue(() => SurveyResponseService.NotifyStudents(surveyResponse.ID.Value, surveyResponse.RegistrantId.Value, surveyResponse.SurveyRequestId.Value, null, 0, null));
    }

    [HttpGet]
    public ActionResult ViewSurvey(int id, string activityId)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();

		if (currentAccount.IsEmpty())
			return HttpNotFound();

		var activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

		ViewBag.GetSurveyFeedback = SurveyResponse.LoadBySurveyRequestId(id, msgs).OrderByDescending(a => a.CreateDate).ToList();
		ViewBag.Survey = SurveyRequest.Load(id);
		ViewBag.Messages = msgs;

		return View(activity);
    }

    public FileResult ExportCSV(int id)
    {
		var msgs = new SystemMessages();
		var currentAccount = SessionVariables.CurrentAccount;

		var fileLink = SurveyRequestService.ExportSurveyRequestToCSV(id, currentAccount.ID.Value, "ExportCSV", msgs);

		if (!Directory.Exists(Server.MapPath("~/Uploads")))
			Directory.CreateDirectory(Server.MapPath("~/Uploads"));

		var filePath = Server.MapPath("~/Uploads/" + Guid.NewGuid() + "ExportSurvey.csv");

		var client = new System.Net.WebClient();
		client.DownloadFile(fileLink, filePath);
		byte[] filedata = System.IO.File.ReadAllBytes(filePath);
		client.Dispose();

		BackgroundJob.Enqueue(() => SurveyRequestService.DeleteCSVFile(filePath));
		return File(filedata, "application/octet-stream", "ExportSurvey.csv");
    }
    [HttpGet]
    public ActionResult ViewSurveyResults(int id, string activityId)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();

		var getSurveyResponse = SurveyResponse.Load(id, msgs);
		var activity = currentAccount.GetActivity(activityId.ToGuid(), msgs);

		var getSurveyResponses = SurveyResponse.LoadBySurveyRequestId(getSurveyResponse.SurveyRequestId.Value).Where(a => a.SurveyFeedbackStatusTypeId == SurveyFeedbackStatus.Completed).ToList();

		var findIndex = getSurveyResponses.FindIndex(a => a.ID == getSurveyResponse.ID);

		if (findIndex >= 0 && (findIndex + 1) < getSurveyResponses.Count)
			ViewBag.NextExists = getSurveyResponses[findIndex + 1].ID;

		if ((findIndex - 1) >= 0)
			ViewBag.PrevExists = getSurveyResponses[findIndex - 1].ID;


		ViewBag.FormQuestionAnswers = FormQuestionAnswer.LoadBySurveyResponseId(getSurveyResponse.ID.Value, msgs).OrderBy(a => a.FormQuestionGroupDisplayOrder).ThenBy(a => a.FormQuestionDisplayOrder).ToList();
		ViewBag.SurveyName = SurveyRequest.Load(getSurveyResponse.SurveyRequestId.Value).Name;
		ViewBag.RegistrantName = Registrant.Load(getSurveyResponse.RegistrantId.Value).Name;
		ViewBag.SurveyFeedback = getSurveyResponse;
		ViewBag.Messages = msgs;

		return View(activity);
    }

    #endregion

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
		return RedirectToAction("registrants", new { id, atype, msg });
    }

    private void RegistrantsViewBag(Activity activity, string atype, string msg, SystemMessages msgs)
    {
		if (msgs.IsEmpty())
			msgs = new SystemMessages();

		var rat = RegistrationAssignmentType.NotSet;

		List<RegistrantActivity> ras = null;

		if (atype.IsNotEmpty())
		{
			rat = (RegistrationAssignmentType)atype.ToInteger();
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

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.Registrants = ras;
		ViewBag.AssignmentType = rat;
		ViewBag.IsReadOnlySeatStatus = activity.GetParticipationObject(msgs).IsExternalToAccount(SessionVariables.CurrentAccount);
		ViewBag.Messages = msgs;
    }

    [HttpGet]
    public ActionResult Registrants(string id, string atype, string msg)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		HandleTempMessages(msgs);
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		RegistrantsViewBag(activity, atype, msg, msgs);
		return View(activity);
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

		var newAssignmentType = (RegistrationAssignmentType)assignmenttype.ToInteger();

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
				onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
			);
		}

		RegistrantsViewBag(activity, assignmenttype, null, msgs);
		ViewBag.Messages = msgs;
		return RedirectToAction("registrants", new { id });
    }

    [HttpGet]
    public ActionResult RegistrantSummary(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		BackgroundJob.Enqueue(() =>
			ActivityService.SendRegistrationSummaryReport(activity.ID.Value, UILabel.ActivityLabel(activity.ActivityTypeId, false), null)
		);

		msgs.Add("Your report will be sent shortly", MessageType.Success);

		RegistrantsViewBag(activity, null, null, msgs);
		ViewBag.Messages = msgs;
		return View("Registrants", activity);
    }

    [HttpGet]
    public ActionResult RegistrantDownload(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
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

		string csv = CsvFile.Create(table);

		Response.Clear();
		Response.AddHeader("Content-Type", BasicLibrary.Utilities.GetMimeType("csv"));
		Response.AddHeader("Content-Disposition", "attachment; filename=registrants.csv");

		byte[] filebytes = System.Text.UTF8Encoding.UTF8.GetBytes(csv);
		Response.AddHeader("Content-Length", filebytes.Length.ToString());
		Response.BinaryWrite(filebytes);
		Response.End();

		RegistrantsViewBag(activity, null, null, msgs);
		ViewBag.Messages = msgs;

		return View("Registrants", activity);
    }

        [HttpPost]
        public JsonResult WithdrawRegistrant(string participationObjectId, string registrantUId, string type, string isWithdrawAndCancelClicked)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            var isCancelPayment = true;

			if (currentAccount.IsEmpty())
				return Json(new { success = false, status = "error" }, JsonRequestBehavior.AllowGet);

			if (participationObjectId.IsEmpty() || registrantUId.IsEmpty() || type.IsEmpty())
				return Json(new { success = false, status = "error" }, JsonRequestBehavior.AllowGet);

			if (isWithdrawAndCancelClicked.IsNotEmpty())
				isCancelPayment = isWithdrawAndCancelClicked == "true";

            var registrant = currentAccount.GetRegistrant(registrantUId.ToGuid(), msgs);

            if (registrant.IsNotEmpty())
            {
                registrant.WithdrawRegistration(
                    participationObjectId: participationObjectId.ToInteger(),
                    participationObjectType: (ParticipationObjectType)Enum.Parse(typeof(ParticipationObjectType), type),
                    loggedInUser: SessionVariables.CurrentUser,
                    isCancelPayment: isCancelPayment,
                    isParent: false,
                    account: currentAccount,
                    msgs: msgs);
            }

		if (msgs.HasNoErrors())
			return Json(new { success = true, status = "ok" }, JsonRequestBehavior.AllowGet);

		//.. something went wrong, error occured
		return Json(new { success = false, status = "error", data = msgs.ToList().Select(msg => new { msg.Text }) }, JsonRequestBehavior.AllowGet);
    }

    private List<AttachedCoupon> GetAttachedCoupons(RegistrantActivity registrantActivity, SystemMessages msgs)
    {
		if (msgs.IsEmpty()) msgs = new SystemMessages();
		var masterRegistrant = registrantActivity.GetRegistrant(msgs).GetMasterRegistrant(msgs);
		return masterRegistrant?.GetAccount(msgs).GetAttachedCoupons() ?? new List<AttachedCoupon>();
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

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
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
				checkoutObject: new CheckoutObject()
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
			msgs.Add("Payment has been added", MessageType.Success);
		}

		ViewBag.Messages = msgs;
		ViewBag.AttachedCoupons = GetAttachedCoupons(registrantActivity, msgs);
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);

		return View("registrantdetail", registrantActivity);
    }

    [HttpPost]
    public ActionResult AttachCoupon(string id, decimal couponAmount, CouponAmountType discountType, DateTime? expirationDate, CouponApplyType couponApplyType, StripeCouponDuration duration, int durationInMonths)
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
			msgs.Add($"Coupon Amount must be a number less than or equal to {maxAmount.ToCurrency()}", MessageType.UserError);
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
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
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
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.Messages = msgs;

		return View(activity);
    }

    [HttpGet]
    public ActionResult RequestToPublish(string id, bool isFromActivityHome = false)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		var isNewActivity = Request["isnew"] == "1";

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
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.IsFullScreen = isNewActivity;
		ViewBag.Messages = msgs;
		SetTempMessage(msgs);

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
				body += $"Activity Start Date: {activity.GetSchedule(msgs).StartDate.ToShortDateString()} <br />";

			if (activity.AccountTimeZone.IsNotEmpty())
				body += $"All times are in {activity.AccountTimeZone} <br />";

			EmailHelper.SendEmail(
				toEmail: AfterSchoolLibrary.Helpers.Email.AppCustomersEmailAddress,
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

		var user = currentAccount.GetAccountOwner(msgs);
		if (user.IsNotEmpty() && currentAccount.IsNotEmpty())
			SendGridHelper.AddOrUpdateContact(currentAccount, user, msgs);

		ViewBag.IsFullScreen = Request["isnew"] == "1";
		ViewBag.MustRequestToPublish = mustRequestToPublish;
		ViewBag.ShowCopyButton = ShowCopyButtonOnPublishDetail(activity, msgs);
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.Messages = msgs;

		if (isFromActivityHome)
			return Redirect(activity.ShortHomePageUrl);

		return View("publishdetail", activity);
    }

    [HttpGet]
    public ActionResult Unpublish(string id, bool isFromActivityHome = false)
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
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.Messages = msgs;

		if (isFromActivityHome)
			return Redirect(activity.ShortHomePageUrl);

		return View("publishdetail", activity);
    }

    [HttpGet]
    public ActionResult Form(string id, string qg)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		var activityForm = activity.GetActiveActivityForm(msgs);

		var questionGroups = activityForm.GetForm(msgs).GetQuestionGroups(msgs);
		var individualGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Individual).ToList();
		var organizationGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Organization).ToList();

		FormQuestionGroup questionGroup = null;

		if (qg.IsNumber())
			questionGroup = FormQuestionGroup.Load(qg.ToInteger(), msgs);
		else
			questionGroup = questionGroups.First();

		ViewBag.QuestionGroups = questionGroups;
		ViewBag.IndividualGroups = individualGroups;
		ViewBag.OrganizationGroups = organizationGroups;
		ViewBag.QuestionGroup = questionGroup;
		ViewBag.ActivityForm = activityForm;
		ViewBag.Messages = msgs;

		return View(activity);
    }

    [HttpPost]
    public ActionResult Form(string id, string qg, string name, string description, string cmd)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();

		if (name.IsEmpty() && qg.IsBlank())
			msgs.Add("Name is required", MessageType.UserError);
		if (description.IsEmpty() && qg.IsBlank())
			msgs.Add("Description is required", MessageType.UserError);

		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);
		var activityForm = activity.GetActiveActivityForm(msgs);
		FormQuestionGroup questionGroup = null;

		if (msgs.HasNoErrors())
		{
			Database.ExecuteTransaction(
				action: (SystemMessages amsgs, DatabaseConnection adc) =>
				{
				if (name.IsNotBlank()) activityForm.Name = name;
				if (description.IsNotBlank()) activityForm.Description = description;

				amsgs.AddRange(activityForm.Save(adc));

				List<FormQuestion> questions = new List<FormQuestion>();

				if (qg.IsNumber())
				{
					questions = FormQuestion.LoadByFormQuestionGroupId(qg.ToInteger(), amsgs, dc: adc);
					questionGroup = FormQuestionGroup.Load(qg.ToInteger(), amsgs, dc: adc);
				}

				if (questionGroup.IsNotEmpty())
				{
					questionGroup.Name = Request.Params[string.Format("name-{0}", questionGroup.ID)];
					questionGroup.LastModifiedBy = SessionVariables.CurrentUser.ID;
					questionGroup.LastModifiedDate = DateTime.UtcNow;
					amsgs.AddRange(questionGroup.Save(adc));
				}

				foreach (var question in questions)
				{
					if (!question.IsLocked)
					{
					question.Text = Request.Params[string.Format("text-{0}", question.ID)];

					question.IsActive = false;
					if (Request.Params[string.Format("active-{0}", question.ID)].IsBoolean())
						question.IsActive = true;

					question.IsRequired = false;
					if (Request.Params[string.Format("required-{0}", question.ID)].IsBoolean())
						question.IsRequired = true;

					amsgs.AddRange(question.Save(adc));
					}
				}
				},
				onSuccess: (SystemMessages smsgs) =>
				{
				msgs.AddRange(smsgs);
				msgs.Add("Registration Form Saved", MessageType.Success);
				},
				onFailure: (SystemMessages fmsgs) =>
				{
				msgs.AddRange(fmsgs);
				}
			);
		}

		var questionGroups = activityForm.GetForm(msgs).GetQuestionGroups(msgs);
		var individualGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Individual).ToList();
		var organizationGroups = questionGroups.Where(x => x.RegistrantTypeId == RegistrantType.Organization).ToList();
		var currentIndex = questionGroups.FindIndex(x => x.ID.Value == questionGroup.ID.Value);
		switch (cmd)
		{
			case "prev":
			questionGroup = questionGroups[currentIndex - 1];
			break;
			case "next":
			questionGroup = questionGroups[currentIndex + 1];
			break;
		}

		ViewBag.QuestionGroups = questionGroups;
		ViewBag.IndividualGroups = individualGroups;
		ViewBag.OrganizationGroups = organizationGroups;
		ViewBag.QuestionGroup = questionGroup;
		ViewBag.ActivityForm = activityForm;
		ViewBag.Messages = msgs;

		return View(activity);
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

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.AccountPackage = SessionVariables.AccountPackage;
		ViewBag.ActivityMessages = activityMessages;
		ViewBag.PagingModel = new PagingModel()
		{
			TotalCount = totalMessageCount,
			PageNumber = pageNumber,
			RecordsPerPage = recordsPerPage,
			TotalPages = totalPages
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

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
		ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
		ViewBag.Messages = msgs;

		return View(am);
    }

    [HttpGet]
    public ActionResult MessageAdd(string id)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();
		var activity = currentAccount.GetActivity(id.ToGuid(), msgs);

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);

		var assignmentTypes = new RegistrationAssignmentType[] { RegistrationAssignmentType.Applied, RegistrationAssignmentType.WaitingList, RegistrationAssignmentType.Reserved, RegistrationAssignmentType.ReservedPendingParentalConsent, RegistrationAssignmentType.ReservedPendingPayment, RegistrationAssignmentType.Invited };

		ViewBag.Registrants = activity.GetRegistrantActivities(true, msgs)
								.Where(ra => ra.AssignmentTypeId.In(assignmentTypes))
								.Select(ra => ra.ToRegistrant())
								.OrderBy(x => x.FirstName)
								.ToList();

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
    public ActionResult MessageAdd(string id, string deliveryMethod, string subject, string body, string sendtostaff, string sendtextmessages)
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

		if (msgs.HasNoErrors())
		{
			var registrantIds = activity.GetRegistrants(true, msgs)
									.Where(x => Request[$"registrant-{x.ID.ToString()}"].IsNotBlank())
									.Select(x => x.ID.Value)
									.ToList();

			//.. email messaging
			if (msgs.HasNoErrors())
			{
			var willExceedQuota = currentAccount.WillExceedEmailQuota(SessionVariables.AccountPackage, registrantIds.Count, currentUser, msgs);

			if (!willExceedQuota)
			{
				am = activity.AddMessage(
					subject,
					body,
					registrantIds,
					sendtostaff.IsBoolean() && sendtostaff.ToBoolean(),
					currentUser.ID.Value,
					msgs);

				BackgroundJob.Enqueue(() => ActivityMessageService.Send(am.ID.Value, currentUser.ID.Value, null, null));

				msgs.Add("Your message will been sent to the selected registrants", MessageType.Success);
				ViewBag.Registrants = am.GetActivityMessageRegistrants(msgs);
				ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
				ViewBag.Messages = msgs;
				return View("messagedetail", am.Reload());
			}
			}
		}

		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
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
		ViewBag.IsActivityLeader = IsActivityLeader(activity, msgs);
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
			onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
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

		ViewBag.QuestionViewModels = questions.OrderBy(question => question.DisplayOrder).Select(question => new FormQuestionViewModel(question)).ToList();
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

		var qvms = questions.OrderBy(question => question.DisplayOrder).Select(question => question.ToFormQuestionViewModel(Request.Params)).ToList();

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
			return RedirectToAction("registrants", new { id = activity.Uid });

		return View(activity);
    }

    private bool IsActivityLeader(Activity activity, SystemMessages msgs)
    {
		if (msgs.IsEmpty())
			msgs = new SystemMessages();

		if (SessionVariables.CurrentAccount.ID == activity.AccountId)
		{
			if (SessionVariables.AccountUserType.In(AccountService.AdminUserTypes))
			return true;

			return SessionVariables.CurrentUser.IsActivityLeader(activity, msgs);
		}

		return false;
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
    public virtual JsonResult GetCalculatedStartingAndEndDateForSubscription(string activityId, string billingPeriodType, string dayOfWeek, string dayOfMonth)
    {
		var currentAccount = SessionVariables.CurrentAccount;
		var msgs = new SystemMessages();

		if (!IsRequestFromApp() || activityId.IsEmpty() || currentAccount.IsEmpty())
			return Json(new { status = "error", messages = msgs }, JsonRequestBehavior.AllowGet);

		var activity = currentAccount.GetActivity(activityId.ToInteger(), msgs);
		var dates = ActivityService.FinalizeStartAndEndDateForSubscription(activity,
			(BillingPeriodType)billingPeriodType.ToInteger(),
			dayOfWeek.IsNotEmpty() ? dayOfWeek.ToEnum<DayOfWeek>() : DayOfWeek.Sunday,
			dayOfMonth.IsNotEmpty() ? dayOfMonth.ToDateTime() : DateTime.Now.AddDays(1));

		if (dates.paymentStart.IsNotEmpty() && dates.paymentStart.HasValue && dates.paymentEnd.IsNotEmpty() && dates.paymentEnd.HasValue)
			return Json(new
			{
			success = true,
			startDate = dates.paymentStart.Value.ToString(),
			startDateFormated = dates.paymentStart.Value.ToString("dddd, MMMM d, yyyy"),
			endDate = dates.paymentEnd.Value.ToString(),
			endDateFormated = dates.paymentEnd.Value.ToString("dddd, MMMM d, yyyy"),
			}, JsonRequestBehavior.AllowGet);

		return Json(new { success = false }, JsonRequestBehavior.AllowGet);
		}
	}
}