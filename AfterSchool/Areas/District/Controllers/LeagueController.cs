using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AfterSchool.Models;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary.Models;
using System.Collections.Specialized;
using AfterSchoolLibrary.Services;
using Hangfire;

namespace AfterSchool.Areas.District.Controllers
{   
    public class LeagueController : BaseLoggedInController
    {
        public LeagueController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }

        // GET: District/League
        [HttpGet]
        public ActionResult Index(string pn, string rpp, string archive)
        {
            var msgs = new SystemMessages();
            int pageNumber = 1;
            int recordsPerPage = 10;
            bool isArchived = false;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (archive.IsBoolean())
                isArchived = archive.ToBoolean();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var groups = new List<ActivityGroup>();
            var totalCount = 0;
            var hasArchivedGroups = false;

            totalCount = SessionVariables.CurrentAccount.GetLeagueCount(isArchived, msgs);
            groups = SessionVariables.CurrentAccount.GetLeagues(isArchived, start, length, msgs).OrderByDescending(x => x.CreateDate).ToList();
            hasArchivedGroups = SessionVariables.CurrentAccount.GetLeagueCount(true, msgs) > 0;

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.GroupController = "league";
            ViewBag.Messages = msgs;
            ViewBag.HasArchivedGroups = hasArchivedGroups;
            ViewBag.ActivityGroupType = ActivityGroupType.League;
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(groups);
        }


        [HttpGet]
        public ActionResult Archive(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            if (group.IsNotEmpty())
                msgs.AddRange(group.MoveToArchive(SessionVariables.CurrentUser.ID.Value));

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} has been archived", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "league");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            if (group.IsNotEmpty())
                group.MoveFromArchive(SessionVariables.CurrentUser.ID.Value, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} has been restored", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "league");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            if (group.IsNotEmpty())
                group.SetIsDeleted(SessionVariables.CurrentUser.ID.Value, true, msgs);

            ViewBag.Messages = msgs;

            if (msgs.HasNoErrors())
                msgs.Add($"{group.Name} was deleted at {SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).ToLongDateTimeString()}", MessageType.Success);

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "league");
        }

        [ValidateInput(false)]
        [HttpGet]
        public ActionResult Summary(string id)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var registrations = group.GetRegistrations();
            var actions = SessionAction.LoadByActivityGroupId(
                group.ID.Value,
                new List<SessionActionType>() {
                    SessionActionType.ActivityGroupCreate,
                    SessionActionType.ActivityGroupPublish,
                    SessionActionType.ActivityGroupUnpublish,
                },
                0,
                15);
            ViewBag.Messages = new SystemMessages();
            ViewBag.Actions = actions;
            ViewBag.Registrations = registrations;
            return View(group);
        }

        public void PeriodViewBag(ActivityGroup group, SystemMessages msgs)
        {
            var defaultOpenDateTime = string.Format("{0} {1}", SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).ToShortDateString(), "8:00 AM").ToDateTime();
            var defaultCloseDateTime = string.Format("{0} {1}", SessionVariables.CurrentAccount.ConvertToAccountTimeZone(DateTime.UtcNow).AddWeeks(1).ToShortDateString(), "12:00 AM").ToDateTime();

            ViewBag.Periods = RegistrationPeriod.LoadByActivityGroupId(group.ID.Value).OrderBy(x => x.StartDateTime.Value).ToList();
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.DefaultOpenDateTime = defaultOpenDateTime;
            ViewBag.DefaultCloseDateTime = defaultCloseDateTime;
            ViewBag.Messages = msgs;
        }

        [HttpGet]
        public ActionResult Periods(string id)
        {
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            PeriodViewBag(group, new SystemMessages());
            return View(group);
        }

        [HttpPost]
        public ActionResult PeriodEdit(string id, string period, string opendate, string opentime, string closedate, string closetime)
        {
            var msgs = new SystemMessages();
            var account = SessionVariables.CurrentAccount;
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            var regperiod = new RegistrationPeriod();

            if (period.IsInteger())
            {
                regperiod = RegistrationPeriod.Load(period.ToInteger());
            }
            else
            {
                regperiod.CreatedBy = SessionVariables.CurrentUser.ID.Value;
                regperiod.CreateDate = DateTime.UtcNow;
                regperiod.ActivityGroupId = group.ID;
                regperiod.AccountId = SessionVariables.CurrentAccount.ID;
            }

            regperiod.StartDateTime = string.Format("{0} {1}", opendate, opentime).ToDateTime();
            regperiod.EndDateTime = string.Format("{0} {1}", closedate, closetime).ToDateTime();
            regperiod.LastModifiedBy = SessionVariables.CurrentUser.ID.Value;
            regperiod.LastModifiedDate = DateTime.UtcNow;
            msgs.AddRange(regperiod.Save());

            PeriodViewBag(group, msgs);

            return View("periods", group);
        }        

        private ActivityGroup LoadSchoolsViewBag(string id, string pn, string rpp)
        {
            var msgs = new SystemMessages();
            var group = ActivityGroup.LoadByUid(id.ToGuid());

            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int count = AccountActivityGroup.GetCount(
                new System.Collections.Hashtable()
                {
                    { "activitygroupid", group.ID.Value}
                },
                msgs);

            var invitedAccountActivityGroups = AccountActivityGroup.Load(
                new System.Collections.Hashtable()
                {
                    { "activitygroupid", group.ID.Value}
                },
                null,
                null,
                msgs);

            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();
            
            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            ViewBag.InvitedAccountActivityGroups = invitedAccountActivityGroups;
            return group;
        }

        [ValidateInput(false)]
        // GET: District/School
        [HttpGet]
        public ActionResult Schools(string id,string pn, string rpp)
        {
            var group = LoadSchoolsViewBag(id, pn, rpp);
            return View(group);
        }

        [ValidateInput(false)]
        // GET: District/School
        [HttpGet]
        public ActionResult Schedule(string id)
        {
            var league = ActivityGroup.LoadByUid(id.ToGuid());
            LoadEditScheduleViewBag(league, league.ActivityScheduleId.ToString());
            ViewBag.Teams = league.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School.AsList());
            ViewBag.Messages = new SystemMessages();
            
            return View(league);
        }

        // GET: User
        [HttpGet]
        public ActionResult Teams(string id, string pn, string rpp)
        {
            var group = ActivityGroup.LoadByUid(id.ToGuid());
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            int userCount = SessionVariables.CurrentAccount.GetMemberCount();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            var teams = activityGroup.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School.AsList());
    
            var totalPages = Math.Ceiling(userCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = userCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };
            ViewBag.Teams = teams.Where(t => t.DateSubmittedToActivityGroup.IsNotEmpty() && !t.IsDeleted).ToList();
            ViewBag.Activities = group.GetActivities();
            return View(group);
        }

        [HttpGet]
        public ActionResult TeamDetails(string id, string teamid)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var team = Activity.LoadByUid(teamid.ToGuid(), msgs);
            var members = team.GetRegistrantActivities(msgs);

            ViewBag.Members = members;
            ViewBag.Team = team;
            ViewBag.Messages = msgs;
            return View(league);
        }

        [HttpGet]
        public ActionResult Students(string id)
        {
            var msgs = new SystemMessages();
            var league = ActivityGroup.LoadByUid(id.ToGuid(), msgs);
            var teams = league.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School.AsList())
                            .Where(t => t.DateSubmittedToActivityGroup.IsNotEmpty() && !t.IsDeleted)
                            .ToList();
            var members = new List<RegistrantActivity>();
            
            teams.ForEach(team =>
                members.AddRange(team.GetRegistrantActivities(msgs))
            );

            ViewBag.Teams = teams;
            ViewBag.Members = members.OrderBy(member => member.RegistrantLastName).ThenBy(member => member.RegistrantFirstName).ToList();
            ViewBag.Messages = msgs;
            return View(league);
        }

        [ValidateInput(false)]
        [HttpGet]
        public ActionResult Edit(string id)
        {
            var league = new ActivityGroup()
            {
                ActivityGroupTypeId = ActivityGroupType.League,
                RegistrationModeId = RegistrationMode.Register,
            };

            if (id.IsGuid())
                league = ActivityGroup.LoadByUid(id.ToGuid());
            

            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = SessionVariables.CurrentAccount.GetForms(null).Where(x => !x.IsArchived || league.FormId == x.ID).OrderBy(x => x.Name).ToList();
            ViewBag.Grades = GradeLevel.Load();
            LoadEditScheduleViewBag(league, league.ActivityScheduleId.ToString());

            return View(league);
        }

        private void LoadEditScheduleViewBag(ActivityGroup activityGroup, string scheduleId)
        {
            ViewBag.Schedules = SessionVariables.CurrentAccount.GetActivitySchedules();

            ActivitySchedule schedule = null;

            // changing schedule
            if (scheduleId.IsNumber())
            {
                schedule = ActivitySchedule.Load(scheduleId.ToInteger());
                if (schedule.AccountId != SessionVariables.CurrentAccount.ID)
                    schedule = null;
                
            }

            if (schedule.IsEmpty())
                schedule = activityGroup.GetSchedule();

            if (schedule.IsEmpty())
                schedule = ActivitySchedule.GetNewDefaultSchedule();
            
            ViewBag.Schedule = schedule;
            ViewBag.ActivityInstances = schedule.GetInstances();
            ViewBag.ShowScheduleList = SessionVariables.AccountPackage.HasFeature(FeatureType.ManageActivityGroups);

        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Edit(string id, string name, string description, string details, string regmode, string form, string isinviteonly, string genderrestrictions, string mingrade, string maxgrade, string schedule, string scheduledetails, string requiresalldays, string repeatsweekly, string startdate, string enddate, string dailystarttime, string dailyendtime, string isongoing)
        {
            var msgs = new SystemMessages();
            var inSessionDates = new List<ActivityInstance>();
            var outOfSessionDates = new List<DateTime>();
            var currentWebUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            var values = new Dictionary<string, object>()
            {
                { "name", name },
                { "description", description },
                { "details", details },
                { "regmode", ((int)RegistrationMode.Register).ToString() },
                { "form", form },
                { "isinviteonly", 0.ToString() },
                { "genderrestrictions", genderrestrictions },
                { "mingrade", mingrade },
                { "maxgrade", maxgrade }
            };

            var filteredParams = Request.Params.Filter("schedulerepeat-");
            var days = AfterSchool.Models.ActivityScheduleUI.PrepDaysOfWeek(filteredParams);

            if (!isongoing.IsBoolean())
                inSessionDates = ActivityScheduleUI.PrepScheduleDates(repeatsweekly, days, startdate, enddate, dailystarttime, dailyendtime, Request.Params.Filter("meeting-open-"));

            outOfSessionDates = ActivityScheduleUI.PrepScheduleDatesClosed(Request.Params.Filter("meeting-closed-"));

            values.Add("scheduledetails", scheduledetails);
            values.Add("days", days.Select(x => (int)x).ToStrings().ToStringList());
            values.Add("repeatsweekly", repeatsweekly.IfBlank("0"));
            values.Add("startdate", startdate);
            values.Add("enddate", enddate);
            values.Add("dailystarttime", dailystarttime);
            values.Add("dailyendtime", dailyendtime);
            values.Add("scheduledates", inSessionDates);
            values.Add("scheduledatesclosed", outOfSessionDates);
            values.Add("schedule", schedule);
            values.Add("isongoing", isongoing);

            var league = ActivityGroupService.SaveLeague(currentWebUser, currentAccount, id, values, msgs);

            if (msgs.HasNoErrors())
            {
                msgs.Add($"{UILabel.ActivityGroupLabel(league.ActivityGroupTypeId)} Saved", MessageType.Success);

                // LOG: CurrentUser Updated ActivityGroup ActivityGroupName for Account AccountName
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentWebUser,
                    actionType: WebSessionActionType.Updated,
                    objectId: league.ID.Value,
                    objectType: ObjectType.ActivityGroup,
                    objectText: league.Name,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectText: currentAccount.Name);         
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Forms = currentAccount.GetForms(null).Where(x => !x.IsArchived || league.FormId == x.ID).OrderBy(x => x.Name).ToList();
            ViewBag.Grades = GradeLevel.Load();
            LoadEditScheduleViewBag(league, league.ActivityScheduleId.ToString());
            ViewBag.Teams = league.GetLeagueTeams(currentAccount, AccountType.School.AsList());

            return View(league);
        }

        private List<Account> LoadInviteSchoolViewBag(ActivityGroup activityGroup, SystemMessages msgs)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var schools = currentAccount.GetVisitorAccounts(AccountType.School.AsList(), msgs);
            var accountActivityGroups = AccountActivityGroup.LoadByActivityGroupId(activityGroup.ID.Value, msgs);
            var accountIds = accountActivityGroups.Select(a => a.AccountId.Value);
            
            var notInivitedSchools = schools.Where(s => s.ID.Value.NotIn(accountIds)).ToList();

            var invitedSchools = new List<Account>();

            ViewBag.CurrentSchools = invitedSchools;
            ViewBag.SchoolsPerColumn = Math.Ceiling(schools.Count.ToDecimal() / 3M);
            ViewBag.SchoolsToInvite = notInivitedSchools;
            ViewBag.RegistrationWindow = activityGroup.GetCurrentOrNextRegistrationPeriod(msgs);

            return notInivitedSchools;
        }

        [HttpGet]
        public ActionResult InviteSchool(string id)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());

            LoadInviteSchoolViewBag(activityGroup, msgs);

            ViewBag.Messages = msgs;

            return View(activityGroup);
        }

        [HttpPost]
        public ActionResult InviteSchool(string id, string invitemessage, string sendinvite)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            var schoolsToInvite = LoadInviteSchoolViewBag(activityGroup, msgs);
            var invitedSchools = new List<Account>();

            schoolsToInvite.ForEach(school => {
                if (Request[String.Format("school-{0}", school.ID.Value)].IsBoolean())
                    invitedSchools.Add(school);
            });

            AccountService.InviteSchoolsToLeague(invitedSchools, activityGroup, invitemessage, SessionVariables.CurrentUser, msgs);            

            if (msgs.HasNoErrors() && sendinvite.IsBoolean())
            {
                var activityGroupId = activityGroup.ID.Value;
                var senderUserId = SessionVariables.CurrentUser.ID.Value;

                invitedSchools.ForEach(school => {
                    var schoolId = school.ID.Value;

                    BackgroundJob.Enqueue(() =>
                        AccountService.SendLeagueInvite(schoolId, activityGroupId, senderUserId, invitemessage)
                    );
                });

                msgs.Add("Your invites will be sent shortly", MessageType.Success);
            }

            if (msgs.HasNoErrors())
                return RedirectToAction("schools", new { id = activityGroup.Uid });

            ViewBag.Messages = msgs;

            return View(activityGroup);
        }

        private DateTime ParseDateTimeStrings(string date = null, string time = null)
        {
            if (date.IsNotEmpty() && date.IsDate())
            {
                if (time.IsNotEmpty() && time.IsTime())
                    return $"{date} {time}".ToDateTime();
                
                 return $"{date} 12:00 PM".ToDateTime();
            }

            return DateTime.UtcNow.Date.AddHours(12);
        }

        private ActivityGroup LoadEditGameViewBag(string id, string instance = null, string startDate = null, string endDate = null, string startTime = null, string endTime = null, SystemMessages msgs = null)
        {
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            ActivityInstance activityInstance = null;
            List<Activity> currentTeams = null;
            
            var startDateTime = ParseDateTimeStrings(startDate, startTime);
            var endDateTime = ParseDateTimeStrings(endDate, endTime);
            
            if (instance.IsNotEmpty() && instance.IsInteger())
            {
                activityInstance = ActivityInstance.Load(instance.ToInteger());
                currentTeams = activityInstance.GetTeams();
                startDateTime = activityInstance.StartDateTime.Value;
                endDateTime = activityInstance.EndDateTime.Value;
            }

            var location = new Location();

            if (activityInstance?.LocationId.IsNotEmpty() ?? false)
                location = Location.Load(activityInstance.LocationId.Value);

            ViewBag.ActivityInstance = activityInstance;
            ViewBag.Location = location;
            ViewBag.StartDateTime = startDateTime;
            ViewBag.EndDateTime = endDateTime;
            ViewBag.Teams = activityGroup.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School.AsList()).Where(t => t.DateSubmittedToActivityGroup.IsNotEmpty()).ToList();
            ViewBag.CurrentTeams = currentTeams;
            ViewBag.CurrentAccount = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;

            return activityGroup;
        }

        [HttpGet]
        public ActionResult EditGame(string id, string instance, string date, string time)
        {
            var msgs = new SystemMessages();

            var activityGroup = LoadEditGameViewBag(id, instance, date, date, time, time, msgs);

            return View(activityGroup);
        }

        [HttpGet]
        public ActionResult SaveGame(string id, string instance, string date, string startTime, string endTime, string locationId, string locationname, string locationdescription, string address1, string address2, string city, string state, string postalcode, string locationtype, string teams, string cmd, string addlocationishidden)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());
            ActivityInstance activityInstance =  null;
            Location location = null;

            var startDateTime = $"{date} {startTime}".ToDateTime();
            var endDateTime =  $"{date} {endTime}".ToDateTime();
            var dateRange = new DateRange(startDateTime, endDateTime);
            var addLocationIsHidden = addlocationishidden.ToBoolean();

            Database.ExecuteTransaction(
                   action: (SystemMessages amsgs, DatabaseConnection adc) => {

                       // check location before trying to create a new activity instance
                       if (locationId.IsNotEmpty() && locationId.IsInteger())
                           location = Location.Load(locationId.ToInteger(), amsgs, adc);
                       else
                       {
                           List<KeyValuePair<string, string>> requiredFields = new List<KeyValuePair<string, string>>()
                           {
                               new  KeyValuePair<string, string>( "Location Name", locationname),
                               new  KeyValuePair<string, string>( "Address", address1),
                               new  KeyValuePair<string, string>( "City", city),
                               new  KeyValuePair<string, string>( "State", state),
                               new  KeyValuePair<string, string>( "Postal Code", postalcode)
                           };

                           if (addLocationIsHidden)
                               amsgs.Add(string.Format("Location is Required"), MessageType.UserError);
                            else
                           {
                               requiredFields.ForEach((field) =>
                               {
                                   if (field.Value.IsEmpty())
                                       amsgs.Add(string.Format("{0} is required", field.Key), MessageType.UserError);
                               });
                           }
                           
                       }
                       
                       // validate start and end times
                       if (startDateTime > endDateTime)
                           amsgs.Add("Start Time must be earlier than End Time", MessageType.UserError);

                       //validate teams
                       if (teams.IsEmpty())
                           amsgs.Add("No teams were selected for this Game", MessageType.UserError);

                       // create and save new location if needed
                       if (amsgs.HasNoErrors() && location.IsEmpty())
                       {
                            // create a new location
                            location = LocationService.CreateNewWithValidation
                                (
                                    locationname,
                                    address1,
                                    address2,
                                    city,
                                    state,
                                    postalcode,
                                    "US",
                                    (LocationType)locationtype.ToInteger(),
                                    null,
                                    SessionVariables.CurrentAccount,
                                    SessionVariables.CurrentUser,
                                    amsgs,
                                    adc
                                );
                       }

                       // save activityInstance
                       if (amsgs.HasNoErrors())
                       {
                           if (instance.IsNotEmpty())
                               activityInstance = ActivityInstance.Load(instance.ToInteger());
                           else
                               activityInstance = activityGroup.GetSchedule()
                                    .AddInstance(
                                        dateRange, 
                                        InstanceType.InSession, 
                                        SessionVariables.CurrentUser.ID.Value, 
                                        amsgs, 
                                        adc
                                     );
                           
                            activityInstance.StartDateTime = startDateTime;
                            activityInstance.EndDateTime = endDateTime;                       
                            activityInstance.LocationId = location.ID.Value;

                           amsgs.AddRange(activityInstance.Save(adc));
                       }

                       // save matches
                       if (activityInstance.IsNotEmpty())
                       {
                           var teamIds = new List<String>();

                           if (teams.IsNotEmpty())
                               teamIds = teams.Split(',').ToList();
                           
                           var selectedTeams = new List<Activity>();
                           var activityInstanceTeams = activityInstance.GetTeams(amsgs, adc);
                           var matches = activityInstance.GetMatches(amsgs, adc);

                           // matches
                           teamIds.ForEach(teamId => selectedTeams.Add(Activity.Load(teamId.ToInteger(), amsgs, adc)));

                           activityInstanceTeams.ForEach(activity =>
                           {
                               var match = matches.Where(m => m.TeamId == activity.ID).FirstOrDefault();

                               // remove a match from the activityInstance
                               if (!selectedTeams.Contains(activity) && match.IsNotEmpty())
                                  amsgs.AddRange(match.Delete(adc));
                           });
                           
                            selectedTeams.ForEach(selectedTeam => {
                                if (!activityInstanceTeams.Contains(selectedTeam))
                                    MatchService.CreateOrUpdate(
                                        currentUser: SessionVariables.CurrentUser,
                                        teamId: selectedTeam.ID.Value,
                                        activityInstanceId: activityInstance.ID.Value,
                                        match: null,
                                        score: 0,
                                        teamPlace: 0,
                                        msgs: amsgs,
                                        dc: adc
                                        );
                            });                            
                       }                    
                   },
                   onSuccess: (SystemMessages smsgs) => {
                       msgs.AddRange(smsgs);
                   },
                   onFailure: (SystemMessages fmsgs) => {
                       msgs.AddRange(fmsgs);
                   }
               );      

            if (msgs.HasNoErrors())
                msgs.Add("Game Saved", MessageType.Success);

            if (msgs.HasNoErrors())
            {
                switch (cmd)
                {
                    // save
                    case "save":
                        LoadEditGameViewBag(id,activityInstance.ID.Value.ToString(), date, date, startTime, endTime);
                        ViewBag.Messages = msgs;
                        return View("editgame", activityGroup);
                    // save and add, load viewbag with just activitygroupid and add a new activity instance
                    case "add":
                        LoadEditGameViewBag(id, null, date, date, startTime, endTime); 
                        ViewBag.Messages = msgs;
                        return View("editgame", activityGroup);
                }
            }

            LoadEditGameViewBag(id, instance, date, date, startTime, endTime);
            ViewBag.Messages = msgs;
            return View("editgame", activityGroup);
        }

        [HttpPost]
        public ActionResult DeleteGame(string id, string instance)
        {
            var msgs = new SystemMessages();
            var activityGroup = ActivityGroup.LoadByUid(id.ToGuid());

            Database.ExecuteTransaction(
                  action: (SystemMessages amsgs, DatabaseConnection adc) => {

                      var activityInstance = ActivityInstance.Load(instance.ToInteger());
                      List<Match> matches = activityInstance.GetMatches(amsgs, adc);

                      matches.ForEach(match => amsgs.AddRange(match.Delete(adc)));

                      amsgs.AddRange(activityInstance.Delete(adc));

                  },
                  onSuccess: (SystemMessages smsgs) => {
                      msgs.AddRange(smsgs);
                  },
                  onFailure: (SystemMessages fmsgs) => {
                      msgs.AddRange(fmsgs);
                  }
              );

            LoadEditScheduleViewBag(activityGroup, activityGroup.ActivityScheduleId.ToString());
            ViewBag.Teams = activityGroup.GetLeagueTeams(SessionVariables.CurrentAccount, AccountType.School.AsList());
            ViewBag.Messages = msgs;
            return View("schedule", activityGroup);
        }

    }
}