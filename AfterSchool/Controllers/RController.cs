using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using AfterSchool.Assemblers;
using AfterSchool.Models;
using AfterSchoolLibrary;
using AfterSchoolLibrary.FeatureFlag;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using Utilities = AfterSchoolLibrary.Helpers.Utilities;

namespace AfterSchool.Controllers
{
    public class RController : BaseController
    {
        private readonly IFeatureFlagRepository featureFlagRepository;

        public RController(IFeatureFlagRepository featureFlagRepository)
        {
            this.featureFlagRepository = featureFlagRepository;
        }

        // GET: R
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///     Used to know where to direct someone if there is an error or lack of information
        ///     the question is where did they start and how do we get them back there?
        /// </summary>
        /// <param name="account"></param>
        private void SetRegistrationStartPage(Account account, bool forceReset = false)
        {
            if (SessionVariables.RegistrationStartPageAccount.IsEmpty() || forceReset)
                SessionVariables.RegistrationStartPageAccount = account;
        }

        private void RestartRegistrationSession(bool clearCart = true)
        {
            SessionVariables.CurrentRegistrant = null;

            if (clearCart)
                SessionVariables.Cart = null;
        }

        [HttpGet]
        public ActionResult Account(string accountname, string dir)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            try
            {
                if (account.IsEmpty() || account.IsDeleted)
                    return HttpNotFound();

                var profile = account.GetAccountProfile(msgs);
                var ah = Utilities.GetNewAmazonHelper();
                var regStartPageAccount = account;

                if (dir.IsNotEmpty())
                {
                    var directoryAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(dir, msgs);
                    if (directoryAccount.IsNotEmpty() && directoryAccount.AccountTypeId.In(new List<AccountType>
                            {AccountType.Directory, AccountType.SchoolDistrict}))
                    {
                        regStartPageAccount = directoryAccount;

                        // LOG: Someone Viewed Account AccountName from Directory DirectoryAccountName
                        // pass in the SessionVariables.WebSession object instead of trying to pass the ID
                        SessionVariables.WebSession.LogActivity(
                            Request,
                            currentUser,
                            directoryAccount,
                            WebSessionActionType.Viewed,
                            ObjectType.DirectoryAccount,
                            objectId: account.ID,
                            objectText: account.Name,
                            indirectObjectId: directoryAccount.ID,
                            indirectObjectText: directoryAccount.Name
                        );
                    }
                }

                if (SessionVariables.RegistrationStartPageAccount.IsEmpty() ||
                    SessionVariables.RegistrationStartPageAccount.AccountTypeId != AccountType.Directory)
                {
                    RestartRegistrationSession();
                    SetRegistrationStartPage(regStartPageAccount, true);
                }

                if (account.AccountTypeId.In(new List<AccountType> {AccountType.Directory, AccountType.SchoolDistrict}))
                {
                    var neighborhoodList = Location.LoadNeighborhoodsByHostAccount(account.ID.Value, msgs).ToList();

                    ViewBag.Messages = msgs;
                    ViewBag.Neighborhoods = neighborhoodList;
                    ViewBag.Grades = GradeLevel.Load();
                    ViewBag.AccountUniqueName = account.UniqueName;
                    ViewBag.AccountName = account.Name;
                    ViewBag.AccountId = account.ID.ToString();

                    if (profile.IsNotEmpty())
                    {
                        ViewBag.PrimaryColor = profile.PrimaryColor;

                        if (profile.Logo.IsNotBlank())
                            ViewBag.LogoUrl = ah.GetFileLink(profile.Logo);

                        if (profile.HeaderImage.IsNotBlank())
                            ViewBag.HeaderImageUrl = ah.GetFileLink(profile.HeaderImage);

                        if (profile.Website.IsNotBlank())
                            ViewBag.Website = profile.Website;
                    }

                    // LOG: Someone Viewed Directory Account DirectoryName
                    SessionVariables.WebSession.LogActivity(
                        Request,
                        currentUser,
                        currentAccount,
                        WebSessionActionType.Viewed,
                        ObjectType.DirectoryAccount,
                        objectId: account.ID,
                        objectText: account.Name
                    );

                    return View("search");
                }

                if (dir.IsEmpty())
                    // LOG: Somone Viewed Account AccountName
                    SessionVariables.WebSession.LogActivity(
                        Request,
                        currentUser,
                        currentAccount,
                        objectId: account.ID,
                        objectText: account.Name,
                        actionType: WebSessionActionType.Viewed,
                        objectType: ObjectType.Account
                    );

                if (profile.IsEmpty())
                    return Redirect(account.BrowseUrl);

                var photos = profile.GetPhotos(msgs);

                photos.ForEach(photo => photo.Value = ah.GetFileLink(photo.Value));

                ViewBag.AccountProfile = profile;
                ViewBag.Location = profile.GetLocation(msgs) ?? account.GetDefaultLocation(msgs);
                ViewBag.Awards = profile.GetAwards(msgs);
                ViewBag.Certifications = profile.GetCertifications(msgs);
                ViewBag.Photos = photos;
                ViewBag.GradeLevelStart = profile.GetGradeLevelStart(msgs);
                ViewBag.GradeLevelEnd = profile.GetGradeLevelEnd(msgs);
                ViewBag.AgeStart = profile.GetAgeStart();
                ViewBag.AgeEnd = profile.GetAgeEnd();
                ViewBag.GenderRestriction = profile.PrintGenderRestrictions();
                ViewBag.RestrictByGrade = profile.RestrictByGrade();
                ViewBag.Messages = msgs;
                ViewBag.ParticipationObjects = GetParticipationObjects(account, msgs)
                    .Where(po =>
                        po.RegistrationPeriod.IsOpen(account, msgs) || po.RegistrationPeriod.WillOpen(account, msgs))
                    .ToList();
                ViewBag.Locations = account.GetAllAvailableLocations(msgs);
                ViewBag.AccountProperties = account.GetAccountProperties(
                    new List<AccountPropertyType>
                    {
                        AccountPropertyType.AcademicSubjects,
                        AccountPropertyType.Classifications,
                        AccountPropertyType.ServiceOffered,
                        AccountPropertyType.SoftSkills,
                        AccountPropertyType.TeachingMethods,
                        AccountPropertyType.TimeOffered,
                        AccountPropertyType.SpecialNeedsOffering,
                        AccountPropertyType.PricingOption,
                    }, msgs);
            }
            catch (Exception ex)
            {
                msgs.Add(accountname, MessageType.Notice);
                msgs.Add(new SystemMessage {Exception = ex, Text = ex.Message, MessageType = MessageType.Error});
                Utilities.SystemErrorLog(msgs);
            }

            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");

            return View(account);
        }

        private List<ParticipationObject> GetParticipationObjects(Account account, SystemMessages msgs)
        {
            var participationObjects = new List<ParticipationObject>();
            participationObjects.AddRange(account.GetPublishedParticipationObjects(msgs));

            return participationObjects
                .Where(po => (!po.IsActivity() || !po.GetActivity().ActivityGroupId.HasValue) && !po.IsInviteOnly)
                .OrderBy(po => po.RegistrationPeriod.IsClosed(account, msgs))
                .ThenBy(po => po.RegistrationPeriod.WillOpen(account, msgs))
                .ThenBy(po => po.RegistrationPeriod.IsOpen(account, msgs))
                .ToList();
        }

        private List<ParticipationObject> GetBrowseActivitiesParticipationObjects(Account account, SystemMessages msgs)
        {
            var participationObjects = new List<ParticipationObject>();
            participationObjects.AddRange(account.GetPublishedParticipationObjects(msgs));

            return participationObjects
                .Where(ShouldDisplayOnBrowseActivities)
                .OrderBy(po => po.RegistrationPeriod.IsClosed(account, msgs))
                .ThenBy(po => po.RegistrationPeriod.WillOpen(account, msgs))
                .ThenBy(po => po.RegistrationPeriod.IsOpen(account, msgs))
                .ToList();
        }

        private static bool ShouldDisplayOnBrowseActivities(ParticipationObject po)
        {
            return (!po.IsActivity() || (!po.GetActivity().ActivityGroupId.HasValue && po.GetActivity().ActivityTypeId != ActivityType.VolunteerOpportunity))
                   && !po.IsInviteOnly;
        }

        private List<ParticipationObject> GetVolunteerOpportunityParticipationObjects(Account account, SystemMessages msgs)
        {
            var participationObjects = new List<ParticipationObject>();
            participationObjects.AddRange(account.GetPublishedParticipationObjects(msgs));

            return participationObjects
                .Where(po => po.IsActivity() && po.GetActivity().ActivityTypeId == ActivityType.VolunteerOpportunity && !po.IsInviteOnly)
                .OrderBy(po => po.RegistrationPeriod.IsClosed(account, msgs))
                .ThenBy(po => po.RegistrationPeriod.WillOpen(account, msgs))
                .ThenBy(po => po.RegistrationPeriod.IsOpen(account, msgs))
                .ToList();
        }
        
        [HttpGet]
        public ActionResult Browse(string accountname)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            if (account.IsNotEmpty())
            {
                // LOG: Someone Browsed Account AccountName's Programs
                SessionVariables.WebSession.LogActivity(
                    account,
                    SessionVariables.CurrentUser,
                    WebSessionActionType.BrowsedAllActivities,
                    ObjectType.Account,
                    objectId: account.ID,
                    objectText: account.Name
                );
            }
            else
            {
                if (CookieVariables.DemoUniqueOrgName.IsNotEmpty() && accountname != CookieVariables.DemoUniqueOrgName)
                    return HttpNotFound();
            }

            RestartRegistrationSession();
            SetRegistrationStartPage(account, true);

            var result = GatherAndAssembleDataForBrowseResult(accountname, account, false, msgs);
            return View(result);
        }
        
        [HttpGet]
        public ActionResult BrowseVolunteerOpportunities(string accountname)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            if (account.IsNotEmpty())
            {
                // LOG: Someone Browsed Account AccountName's Programs
                SessionVariables.WebSession.LogActivity(
                    account,
                    SessionVariables.CurrentUser,
                    WebSessionActionType.BrowsedAllActivities,
                    ObjectType.Account,
                    objectId: account.ID,
                    objectText: account.Name
                );
            }
            else
            {
                if (CookieVariables.DemoUniqueOrgName.IsNotEmpty() && accountname != CookieVariables.DemoUniqueOrgName)
                    return HttpNotFound();
            }

            RestartRegistrationSession();
            SetRegistrationStartPage(account, true);

            var result = GatherAndAssembleDataForBrowseResult(accountname, account, true, msgs);
            return View(result);
        }

        private BrowseResult GatherAndAssembleDataForBrowseResult(string accountName, Account account, bool browseForVolunteerOpportunities, SystemMessages msgs)
        {
            List<ParticipationObject> participationObjects;
            var locations = new List<Location>();
            var grades = GradeLevel.Load(msgs: msgs);
            var featuredProviders = new List<Account>();

            List<ParticipationObject> registrationOpened;
            List<ParticipationObject> registrationWillOpen;
            List<ParticipationObject> registrationClosed;

            if (account.IsNotEmpty())
            {
                participationObjects = browseForVolunteerOpportunities
                    ? GetVolunteerOpportunityParticipationObjects(account, msgs)
                    : GetBrowseActivitiesParticipationObjects(account, msgs);

                locations.AddRange(account.GetAllAvailableLocations(msgs));
                locations.AddRange(participationObjects.Where(po => po.Location.IsNotEmpty())
                    .Select(po => po.Location));

                registrationOpened = participationObjects.Where(po => po.RegistrationPeriod.IsOpen(account, msgs)).ToList();
                registrationWillOpen = participationObjects.Where(po => po.RegistrationPeriod.WillOpen(account, msgs)).ToList();
                registrationClosed = participationObjects.Where(po => po.NotIn(registrationWillOpen) && po.RegistrationPeriod.IsClosed(account, msgs)).ToList();

                featuredProviders = account.GetApprovedVisitorAccounts(msgs);
            }
            else
            {
                string accountUniqueName;
                (accountName, accountUniqueName) = GetDemoAccountDetails(accountName);

                participationObjects = new List<ParticipationObject>();
                ParticipationObject.GenerateDemoObjects(ref account, participationObjects, locations, accountName, accountUniqueName, grades);
                participationObjects = participationObjects.Take(11).ToList();

                registrationOpened = participationObjects;
                registrationWillOpen = new List<ParticipationObject>();
                registrationClosed = new List<ParticipationObject>();
            }

            var accountProfile = account.GetAccountProfile(msgs);
            var cleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");

            return new BrowseAssembler().AssembleBrowseResult(account, accountProfile, cleverSsoEnabled, featuredProviders, grades, locations,
                registrationClosed, registrationOpened, registrationWillOpen, null, msgs);
        }

        [HttpGet]
        public ActionResult Contact(string accountname)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);


            if (account.IsEmpty())
                return HttpNotFound();

            if (!account.ContactFormId.HasValue)
                return Redirect(account.HomePageUrl);

            return RedirectToAction("contact", "account", new {area = "", regaccount = account.Uid});
        }

        public ActionResult Pay(string accountname)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            if (currentUser.IsEmpty())
                return Redirect(account.HomePageUrl);

            if (!currentUser.IsRegistered)
                return Redirect(Url.Action("confirm", "signup",
                    new {id = currentUser.Uid, returnurl = account.PayUrl}));

            if (currentAccount.AccountTypeId != AccountType.Registrant)
            {
                //check if the user has a parent account
                var registrantAccount = currentUser.GetRegistrantAccount(msgs);

                // if no parent account, then create one real quick
                if (registrantAccount.IsEmpty())
                    registrantAccount =
                        currentUser.AddNewRegistrantAccount(UserType.Owner, true, true, currentUser, msgs);

                var masterRegistrant = currentUser.GetMasterRegistrant(msgs);

                if (masterRegistrant.IsNotEmpty())
                    registrantAccount = masterRegistrant.GetAccount(msgs);

                if (registrantAccount.ID != currentAccount.ID)
                    LoadSessionInfo(
                        currentUser,
                        registrantAccount,
                        false,
                        msgs);

                return Redirect(account.PayUrl);
            }

            ViewBag.Sources = currentAccount.GetStripeSources(msgs);
            ViewBag.BankAccounts = currentAccount.GetStripeBankAccounts(msgs);
            ViewBag.AccountProfile = account.GetAccountProfile(msgs);
            ViewBag.Messages = msgs;
            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");

            return View(account);
        }

        [HttpGet]
        public ActionResult Activity(string accountname, string activitytype, string activityname, string preview)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            ParticipationObject participationObject = null;
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var canPublishActivity = false;
            var accountProfile = account?.GetAccountProfile(msgs);
            var currentUser = SessionVariables.CurrentUser;
            var currentAccount = SessionVariables.CurrentAccount;
            ParticipationObject session = null;

            if (account.IsNotEmpty())
            {
                if (account.IsDeleted)
                    return HttpNotFound();

                var activity =
                    AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value,
                        msgs);
                if (!activity.IsPublished && (currentUser.IsEmpty() ||
                                              (currentUser.IsNotEmpty() &&
                                               !currentUser.HasAccessToActivity(activity, msgs))))
                    return Redirect(account.HomePageUrl);

                if (activity.IsEmpty())
                    return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

                participationObject = activity.GetParticipationObject(msgs);

                if (activity.ActivityGroupId.HasValue)
                {
                    session = activity.GetActivityGroup(msgs)?.GetParticipationObject(msgs);
                    AddNoCacheHeaders();
                }


                if (currentUser.IsNotEmpty() && currentAccount.IsNotEmpty())
                    canPublishActivity = currentUser.CanPublishActivity(currentAccount, activity, msgs);
            }
            else
            {
                return HttpNotFound();
            }

            RestartRegistrationSession(session.IsEmpty());
            SetRegistrationStartPage(account);

            // LOG: Someone Viewed ParticipationObjectType ParticipationObjectName by Account AccountName
            SessionVariables.WebSession.LogActivity(
                account,
                SessionVariables.CurrentUser,
                WebSessionActionType.Viewed,
                participationObject.GetWebSessionObjectType(),
                objectId: participationObject.ObjectID.Value,
                objectText: participationObject.Name,
                indirectObjectType: ObjectType.Account,
                indirectObjectId: participationObject.OwnerAccount.ID.Value,
                indirectObjectText: participationObject.OwnerAccount.Name
            );

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.CanPublishActivity = canPublishActivity;
            ViewBag.Session = session;
            ViewBag.AccountProfile = accountProfile;
            ViewBag.CleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");

            return View(participationObject);
        }

        [HttpGet]
        public ActionResult SelectDates(string accountname, string activitytype, string activityname, string reg)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var accountProfile = account?.GetAccountProfile(msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var activity =
                AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            if (IsStudentSelectingDatesForVolunteerOpportunity(activity, msgs))
            {
                msgs.Add("Only parents or guardians can register for volunteer opportunities", MessageType.UserError);
                SetTempMessage(msgs);
                return Redirect(activity.GetHomePageUrl());
            }
                

            var participationObject = activity.GetParticipationObject(msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.CanPublishActivity = false;
            ViewBag.CanRegisterFor = true;
            ViewBag.AccountProfile = accountProfile;

            return View("ActivitySelectDates", participationObject);
        }

        private static bool IsStudentSelectingDatesForVolunteerOpportunity(Activity activity, SystemMessages msgs)
        {
            return activity.ActivityTypeId == ActivityType.VolunteerOpportunity &&
                   (!SessionVariables.CurrentUser?.GetMasterRegistrant(msgs).IsParentOrGuardian ?? false);
        }

        [HttpPost]
        public ActionResult SelectDates(string accountname, string activitytype, string activityname, string reg,
            string post)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var activity =
                AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var participationObject = activity.GetParticipationObject(msgs);
            var dateFields = Request.Params.Filter("date-");

            var selectedDates = dateFields.Cast<string>().Select(x => dateFields[x]).Select(x => x.ToDateTime())
                .ToList();

            if (selectedDates.Count == 0)
            {
                msgs.Add("Please select at least one date to continue.", MessageType.UserError);
                
                ViewBag.Account = account;
                ViewBag.CanPublishActivity = false;
                ViewBag.CanRegisterFor = true;
                ViewBag.AccountProfile = account.GetAccountProfile(msgs);
                ViewBag.Messages = msgs;
                return View("ActivitySelectDates", participationObject);
            }

            return RedirectToRegistration(account, participationObject, reg, selectedDates, msgs);
        }

        [HttpGet]
        public ActionResult ActivitySession(string accountname, string sessionname, string invite, string decline)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            if (session.IsEmpty())
                return HttpNotFound();

            var participationObject = session.GetParticipationObject();

            LoadSessionInvite(session, invite, decline, msgs);

            SetRegistrationStartPage(account);

            var canRegisterFor = !session.IsInviteOnly || session.IsInvited(SessionVariables.SessionInvite, msgs);

            if (canRegisterFor)
                return Redirect(session.BrowseUrl);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.CanRegisterFor = canRegisterFor;
            ViewBag.AccountProfile = account.GetAccountProfile(msgs);
            ViewBag.CanPublishActivity = false;

            // LOG: Somone Viewed Session SessionName by Account AccountName
            SessionVariables.WebSession.LogActivity(
                account,
                SessionVariables.CurrentUser,
                WebSessionActionType.Viewed,
                participationObject.GetWebSessionObjectType(),
                objectId: participationObject.ObjectID.Value,
                objectText: sessionname,
                indirectObjectType: ObjectType.Account,
                indirectObjectId: participationObject.OwnerAccount.ID.Value,
                indirectObjectText: participationObject.OwnerAccount.Name
            );

            return View("activity", participationObject);
        }

        [HttpGet]
        public ActionResult League(string accountname, string districtname, string leaguename)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            RestartRegistrationSession();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            ParticipationObject participationObject = null;

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            if (districtname.IsNotEmpty())
            {
                var districtAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(districtname, msgs);

                if (districtAccount.IsEmpty())
                    return HttpNotFound();

                var league = ActivityGroup.LoadByAccountIdByUniqueName(districtAccount.ID.Value, leaguename, msgs);

                if (league.IsEmpty())
                    return HttpNotFound();

                var aag = league.GetAccountActivityGroup(account, msgs);
                participationObject = aag.GetParticipationObject(msgs);
            }
            else
            {
                var league = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, leaguename, msgs);

                if (league.IsEmpty())
                    return HttpNotFound();

                participationObject = league.GetParticipationObject(msgs);
            }

            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.AccountProfile = account.GetAccountProfile(msgs);
            ViewBag.CanPublishActivity = false;

            // LOG: Somone Viewed League LeagueName by Account AccountName
            SessionVariables.WebSession.LogActivity(
                account,
                SessionVariables.CurrentUser,
                WebSessionActionType.Viewed,
                participationObject.GetWebSessionObjectType(),
                objectId: participationObject.ObjectID.Value,
                objectText: leaguename,
                indirectObjectType: ObjectType.Account,
                indirectObjectId: participationObject.OwnerAccount.ID.Value,
                indirectObjectText: participationObject.OwnerAccount.Name
            );

            return View("activity", participationObject);
        }

        [HttpGet]
        public ActionResult Register(string accountname, string activitytype, string activityname, string reg)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var activity =
                AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var participationObject = activity.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, reg, null, msgs);
        }

        [HttpGet]
        public ActionResult Interested(string accountname, string activitytype, string activityname, string reg)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var activity =
                AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var participationObject = activity.GetParticipationObject(msgs);

            return RedirectToInterested(account, participationObject, msgs);
        }

        [HttpGet]
        public ActionResult RegisterSession(string accountname, string sessionname, string reg)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            if (session.IsEmpty())
                return HttpNotFound();

            var participationObject = session.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, reg, null, msgs);
        }

        [HttpGet]
        public ActionResult BrowseSession(string accountname, string sessionname, string invite, string decline)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            var featuredProviders = new List<Account>();
            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);
            if (session.IsEmpty())
                return HttpNotFound();

            LoadSessionInvite(session, invite, decline, msgs);

            var canRegisterFor = !session.IsInviteOnly || (SessionVariables.SessionInvite.IsGuid() &&
                                                           session.IsInvited(SessionVariables.SessionInvite.ToGuid(), msgs));

            if (!canRegisterFor)
                return Redirect(session.HomePageUrl);

            AddNoCacheHeaders();

            var result = GatherAndAssembleDataForBrowseSessionResult(session, msgs, account, featuredProviders);
            return View("Browse", result);
        }

        private BrowseResult GatherAndAssembleDataForBrowseSessionResult(ActivityGroup session, SystemMessages msgs, Account account, List<Account> featuredProviders)
        {
            var activities = session.IsInviteOnly
                ? session.GetInvitedToActivities(SessionVariables.SessionInvite.ToGuid(), msgs)
                : session.GetActivities(msgs);
            var participationObjects = activities.Select(activity => activity.GetParticipationObject(msgs)).ToList();
            var opened = new List<ParticipationObject>();
            var willOpen = new List<ParticipationObject>();
            var closed = new List<ParticipationObject>();

            if (session.IsOpenNow(msgs))
                opened = participationObjects;
            else if (session.WillOpen(msgs))
                willOpen = participationObjects;
            else
                closed = participationObjects;

            var grades = GradeLevel.Load(msgs: msgs);
            var locations = account.GetAllAvailableLocations(msgs);
            var accountProfile = account.GetAccountProfile(msgs);
            var cleverSsoEnabled = featureFlagRepository.IsEnabled("clever-sso");

            var result = new BrowseAssembler().AssembleBrowseResult(account, accountProfile, cleverSsoEnabled, featuredProviders, grades, locations,
                closed, opened, willOpen, session.GetParticipationObject(msgs), msgs);
            return result;
        }

        [HttpGet]
        public ActionResult RegisterLeague(string accountname, string activitygrouptype, string externalaccountname,
            string group, string reg)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            ParticipationObject participationObject = null;

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            if (externalaccountname.IsNotEmpty())
            {
                var externalAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(externalaccountname, msgs);

                if (externalAccount.IsEmpty())
                    return HttpNotFound();

                var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(externalAccount.ID.Value, group, msgs);

                if (activityGroup.IsEmpty())
                    return HttpNotFound();

                var aag = activityGroup.GetAccountActivityGroup(account, msgs);
                participationObject = aag.GetParticipationObject(msgs);
            }
            else
            {
                var activityGroup = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, group, msgs);

                if (activityGroup.IsEmpty())
                    return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

                participationObject = activityGroup.GetParticipationObject(msgs);
            }

            return RedirectToRegistration(account, participationObject, reg, null, msgs);
        }

        private ActionResult RedirectToRegistration(Account account, ParticipationObject participationObject,
            string reg, List<DateTime> selectedDates, SystemMessages msgs)
        {
            SetRegistrationStartPage(account);

            if (participationObject.IsEmpty())
                return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

            if (!participationObject.IsSession())
                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);

            if (SessionVariables.Cart.IsEmpty())
                return Redirect(participationObject.GetHomePageUrl());

            SessionVariables.Cart.AddCartItem(participationObject, selectedDates, msgs);

            if (reg.IsGuid())
                SessionVariables.CurrentRegistrant = Registrant.LoadByUid(reg.ToGuid(), msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
            {
                if (participationObject.ActivityType == ActivityType.VolunteerOpportunity)
                {
                    return RedirectToAction("getstarted", "volunteerRegistration", new {area = "", regaccount = account.Uid});
                }
                return RedirectToAction("getstarted", "account", new {area = "", regaccount = account.Uid});
            }

            var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant();

            if (masterRegistrant.IsParentOrGuardian)
            {
                // LOG: Somone Began Registration for ParticipationObjectType ParticipationObjectName by Account AccountName
                SessionVariables.WebSession.LogActivity(
                    account,
                    SessionVariables.CurrentUser,
                    WebSessionActionType.BeganRegistration,
                    participationObject.GetWebSessionObjectType(),
                    objectId: participationObject.ObjectID.Value,
                    objectText: participationObject.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: participationObject.OwnerAccount.ID.Value,
                    indirectObjectText: participationObject.OwnerAccount.Name
                );

                return RedirectToAction("register", "account", new {area = "", regaccount = account.Uid});
            }

            return RedirectToAction("registerstudent", "account", new {area = "", regaccount = account.Uid});
        }

        private ActionResult RedirectToInterested(Account account, ParticipationObject participationObject,
            SystemMessages msgs)
        {
            SetRegistrationStartPage(account);

            if (participationObject.IsEmpty())
                return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

            if (!participationObject.IsSession())
                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);

            if (SessionVariables.Cart.IsEmpty())
                return Redirect(participationObject.GetHomePageUrl());

            SessionVariables.Cart.AddCartItem(participationObject, null, msgs);

            return RedirectToAction("interested", "account", new {area = "", regaccount = account.Uid});
        }

        private void LoadSessionInvite(ActivityGroup session, string invite, string decline, SystemMessages msgs)
        {
            if (!invite.IsGuid())
                return;
            
            SessionVariables.SessionInvite = invite;

            if (decline.IsBoolean() && decline.ToBoolean())
                session.DeclineInvite(SessionVariables.SessionInvite.ToGuid(), msgs);
        }

        private (string AccountName, string AccountUniqueName) GetDemoAccountDetails(string accountName)
        {
            var accountUniqueName = "";

            if (CookieVariables.DemoOrgName.IsNotEmpty())
            {
                accountName = CookieVariables.DemoOrgName;
                accountUniqueName = CookieVariables.DemoUniqueOrgName;
            }
            else
            {
                var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx,
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);
                accountName = accountName.SplitByUpperCase();
                accountUniqueName = reg.Replace(accountName.GetMaxWords(25), "");
            }

            return (accountName, accountUniqueName);
        }

        private void AddNoCacheHeaders()
        {
            Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
            Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
            Response.AppendHeader("Expires", "0"); // Proxies.
        }

        private ActionResult RedirectToProfile(string uniqueName, string msg, MessageType msgsType)
        {
            var msgs = new SystemMessages();
            msgs.Add(msg, msgsType);

            return Redirect($"/{uniqueName}");
        }
    }
}