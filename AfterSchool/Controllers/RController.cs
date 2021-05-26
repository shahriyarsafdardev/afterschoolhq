using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchoolLibrary.Services;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace AfterSchool.Controllers
{
    public class RController : BaseController
    {
        // GET: R
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Used to know where to direct someone if there is an error or lack of information
        /// the question is where did they start and how do we get them back there?
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
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);

            try
            {
                if (account.IsEmpty() || account.IsDeleted)
                    return HttpNotFound();

                var profile = account.GetAccountProfile(msgs);
                var ah = AfterSchoolLibrary.Helpers.Utilities.GetNewAmazonHelper();
                var regStartPageAccount = account;

                if (dir.IsNotEmpty())
                {
                    var directoryAccount = AfterSchoolLibrary.Models.Account.LoadByUniqueName(dir, msgs);
                    if (directoryAccount.IsNotEmpty() && directoryAccount.AccountTypeId.In(new List<AccountType>() { AccountType.Directory, AccountType.SchoolDistrict }))
                    {
                        regStartPageAccount = directoryAccount;

                        // LOG: Someone Viewed Account AccountName from Directory DirectoryAccountName
                        // pass in the SessionVariables.WebSession object instead of trying to pass the ID
                        WebSessionUtilities.LogActivity(
                            request: Request,
                            webSession: SessionVariables.WebSession,
                            actor: SessionVariables.CurrentUser,
                            actionType: WebSessionActionType.Viewed,
                            objectType: ObjectType.DirectoryAccount,
                            objectId: account.ID,
                            objectText: account.Name,
                            indirectObjectId: directoryAccount.ID,
                            indirectObjectText: directoryAccount.Name
                        );
                    }
                }

                if (SessionVariables.RegistrationStartPageAccount.IsEmpty() || SessionVariables.RegistrationStartPageAccount.AccountTypeId != AccountType.Directory)
                {
                    RestartRegistrationSession();
                    SetRegistrationStartPage(regStartPageAccount, true);
                }

                if (account.AccountTypeId.In(new List<AccountType>() { AccountType.Directory, AccountType.SchoolDistrict }))
                {
                    var neighborhoodList = Location.LoadNeighborhoodsByHostAccount(hostAccountId: account.ID.Value, msgs: msgs).ToList();

                    ViewBag.Messages = msgs;
                    ViewBag.Neighborhoods = neighborhoodList;
                    ViewBag.Grades = GradeLevel.Load();
                    ViewBag.AccountUniqueName = account.UniqueName;
                    ViewBag.AccountId = account.ID.ToString();

                    if (profile.IsNotEmpty())
                    {
                        ViewBag.PrimaryColor = profile.PrimaryColor;

                        if (profile.Logo.IsNotBlank())
                            ViewBag.LogoUrl = ah.GetFileLink(profile.Logo, true, null, true);

                        if (profile.HeaderImage.IsNotBlank())
                            ViewBag.HeaderImageUrl = ah.GetFileLink(profile.HeaderImage, true, null, true);

                        if (profile.Website.IsNotBlank())
                            ViewBag.Website = profile.Website;
                    }

                    // LOG: Someone Viewed Directory Account DirectoryName
                    WebSessionUtilities.LogActivity(
                        request: Request,
                        webSession: SessionVariables.WebSession,
                        actor: SessionVariables.CurrentUser,
                        actionType: WebSessionActionType.Viewed,
                        objectType: ObjectType.DirectoryAccount,
                        objectId: account.ID,
                        objectText: account.Name
                    );

                    return View("search");
                }

                if (dir.IsEmpty())
                {
                    // LOG: Somone Viewed Account AccountName
                    WebSessionUtilities.LogActivity(
                       request: Request,
                       webSession: SessionVariables.WebSession,
                       actor: SessionVariables.CurrentUser,
                       objectId: account.ID,
                       objectText: account.Name,
                       actionType: WebSessionActionType.Viewed,
                       objectType: ObjectType.Account
                    );

                }

                if (profile.IsEmpty())
                    return Redirect(account.BrowseUrl);

                var photos = profile.GetPhotos(msgs);

                photos.ForEach(photo => photo.Value = ah.GetFileLink(photo.Value, true, null, true));

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
                                                .Where(po => po.RegistrationPeriod.IsOpen(account, msgs) || po.RegistrationPeriod.WillOpen(account, msgs))
                                                .ToList();
                ViewBag.Locations = account.GetAllAvailableLocations(msgs);
                ViewBag.AccountProperties = account.GetAccountProperties(
                                                new List<AccountPropertyType>()
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
                msgs.Add(new SystemMessage() { Exception = ex, Text = ex.Message, MessageType = MessageType.Error });
                AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(msgs);
            }

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


        [HttpGet]
        public ActionResult Browse(string accountname)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname);

            if (account.IsEmpty() || account.IsDeleted)
                return HttpNotFound();

            RestartRegistrationSession();
            SetRegistrationStartPage(account, true);

            var participationObjects = new List<ParticipationObject>();
            var locations = new List<Location>();
            var grades = GradeLevel.Load(msgs: msgs);

            List<ParticipationObject> regOpened = null;
            List<ParticipationObject> regWillOpen = null;
            List<ParticipationObject> regClosed = null;

            if (account.IsNotEmpty())
            {
                locations.AddRange(account.GetAllAvailableLocations(msgs));

                participationObjects = GetParticipationObjects(account, msgs);

                regOpened = participationObjects.Where(po => po.RegistrationPeriod.IsOpen(account, msgs)).ToList();
                regWillOpen = participationObjects.Where(po => po.RegistrationPeriod.WillOpen(account, msgs)).ToList();
                regClosed = participationObjects.Where(po => po.NotIn(regWillOpen) && po.RegistrationPeriod.IsClosed(account, msgs)).ToList();

                // LOG: Someone Browsed Account AccountName's Programs
                WebSessionUtilities.LogActivity(
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actor: SessionVariables.CurrentUser,
                    actionType: WebSessionActionType.BrowsedAllActivities,
                    objectType: ObjectType.Account,
                    objectId: account.ID,
                    objectText: account.Name
                 );
            }
            else
            {
                if (CookieVariables.DemoUniqueOrgName.IsNotEmpty() && accountname != CookieVariables.DemoUniqueOrgName)
                    return HttpNotFound();

                var accountUniqueName = "";
                (accountname, accountUniqueName) = GetDemoAccountDetails(accountname);

                ParticipationObject.GenerateDemoObjects(ref account, participationObjects, locations, accountname, accountUniqueName, grades);
                participationObjects = participationObjects.Take(11).ToList();

                regOpened = participationObjects;
                regWillOpen = new List<ParticipationObject>();
                regClosed = new List<ParticipationObject>();
            }

            ViewBag.RegistrationOpened = regOpened;
            ViewBag.RegistrationWillOpen = regWillOpen;
            ViewBag.RegistrationClosed = regClosed;

            ViewBag.Grades = grades;
            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Locations = locations;
            ViewBag.ParticipationObjects = participationObjects;
            ViewBag.AccountProfile = account.GetAccountProfile(msgs);

            return View(account);
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

            return RedirectToAction("contact", "account", new { area = "", regaccount = account.Uid });
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
                return Redirect(Url.Action("confirm", "signup", new { id = currentUser.Uid, returnurl = account.PayUrl }));

            if (currentAccount.AccountTypeId != AccountType.Registrant)
            {
                //check if the user has a parent account
                var registrantAccount = currentUser.GetRegistrantAccount(msgs);

                // if no parent account, then create one real quick
                if (registrantAccount.IsEmpty())
                    registrantAccount = currentUser.AddNewRegistrantAccount(UserType.Owner, true, true, currentUser, msgs);

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

            return View(account);
        }

        [HttpGet]
        public ActionResult Activity(string accountname, string activitytype, string activityname, string preview)
        {
            if (isUnsupportedBrowser())
                return RedirectUnsupportedBrowser();

            ParticipationObject participationObject = null;
            var msgs = new SystemMessages();
            var account = AfterSchoolLibrary.Models.Account.LoadByUniqueName(accountname, msgs);
            var canPublishActivity = false;
            var accountProfile = account?.GetAccountProfile(msgs);
            ParticipationObject session = null;

            if (account.IsNotEmpty())
            {
                if (account.IsDeleted)
                    return HttpNotFound();

                var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

                if (activity.IsEmpty())
                    return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

                participationObject = activity.GetParticipationObject(msgs);

                if (activity.ActivityGroupId.HasValue)
                {
                    session = activity.GetActivityGroup(msgs)?.GetParticipationObject(msgs);
                    AddNoCacheHeaders();
                }

                if (!activity.IsPublished &&
                    activity.GetLocation().IsNotEmpty() &&
                    activity.GetLocation().HostAccountId.IsNotEmpty() &&
                    activity.GetLocation().HostAccountId != activity.AccountId)
                {
                    var accountOwnerId = activity.GetLocation().GetHostAccount().OwnerId;

                    if (SessionVariables.CurrentUser.ID == accountOwnerId)
                        canPublishActivity = true;
                }
            }
            else
                return HttpNotFound();


            RestartRegistrationSession(clearCart: session.IsEmpty());
            SetRegistrationStartPage(account);

            ViewBag.Messages = msgs;
            ViewBag.Account = account;
            ViewBag.CanPublishActivity = canPublishActivity;
            ViewBag.CanRegisterFor = true;
            ViewBag.Session = session;
            ViewBag.AccountProfile = accountProfile;

            // LOG: Someone Viewed ParticipationObjectType ParticipationObjectName by Account AccountName
            WebSessionUtilities.LogActivity(
                webSessionId: SessionVariables.WebSession.ID.Value,
                actor: SessionVariables.CurrentUser,
                actionType: WebSessionActionType.Viewed,
                objectType: participationObject.GetWebSessionObjectType(),
                objectId: participationObject.ObjectID.Value,
                objectText: participationObject.Name,
                indirectObjectType: ObjectType.Account,
                indirectObjectId: participationObject.OwnerAccount.ID.Value,
                indirectObjectText: participationObject.OwnerAccount.Name
            );

            return View(participationObject);
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

            // LOG: Somone Viewed Session SessionName by Account AccountName
            WebSessionUtilities.LogActivity(
                actor: SessionVariables.CurrentUser,
                webSessionId: SessionVariables.WebSession.ID.Value,
                actionType: WebSessionActionType.Viewed,
                objectType: participationObject.GetWebSessionObjectType(),
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

            // LOG: Somone Viewed League LeagueName by Account AccountName
            WebSessionUtilities.LogActivity(
                actor: SessionVariables.CurrentUser,
                webSessionId: SessionVariables.WebSession.ID.Value,
                actionType: WebSessionActionType.Viewed,
                objectType: participationObject.GetWebSessionObjectType(),
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

            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

            if (activity.IsEmpty())
                return HttpNotFound();

            var participationObject = activity.GetParticipationObject(msgs);

            return RedirectToRegistration(account, participationObject, reg, msgs);
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

            var activity = AfterSchoolLibrary.Models.Activity.LoadByUniqueNameByAccountId(activityname, account.ID.Value, msgs);

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

            return RedirectToRegistration(account, participationObject, reg, msgs);
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

            var session = ActivityGroup.LoadByAccountIdByUniqueName(account.ID.Value, sessionname, msgs);

            if (session.IsEmpty())
                return HttpNotFound();

            LoadSessionInvite(session, invite, decline, msgs);

            var canRegisterFor = !session.IsInviteOnly || (SessionVariables.SessionInvite.IsGuid() && session.IsInvited(SessionVariables.SessionInvite.ToGuid(), msgs));

            if (!canRegisterFor)
                return Redirect(session.HomePageUrl);

            AddNoCacheHeaders();

            List<Activity> activities = null;

            if (session.IsInviteOnly)
                activities = session.GetInvitedToActivities(SessionVariables.SessionInvite.ToGuid(), msgs);
            else
                activities = session.GetActivities(msgs);

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

            ViewBag.Session = session.GetParticipationObject(msgs);
            ViewBag.Grades = GradeLevel.Load(msgs: msgs);
            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;
            ViewBag.Locations = account.GetAllAvailableLocations(msgs);
            ViewBag.ParticipationObjects = participationObjects;
            ViewBag.RegistrationOpened = opened;
            ViewBag.RegistrationClosed = closed;
            ViewBag.RegistrationWillOpen = willOpen;
            ViewBag.AccountProfile = account.GetAccountProfile(msgs);

            return View("browse", account);
        }

        [HttpGet]
        public ActionResult RegisterLeague(string accountname, string activitygrouptype, string externalaccountname, string group, string reg)
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

            return RedirectToRegistration(account, participationObject, reg, msgs);
        }

        private ActionResult RedirectToRegistration(Account account, ParticipationObject participationObject, string reg, SystemMessages msgs)
        {
            SetRegistrationStartPage(account);

            if (participationObject.IsEmpty())
                return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

            if (!participationObject.IsSession())
                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);

            if (SessionVariables.Cart.IsEmpty())
                return Redirect(participationObject.GetHomePageUrl());

            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            if (reg.IsGuid())
                SessionVariables.CurrentRegistrant = Registrant.LoadByUid(reg.ToGuid(), msgs);

            if (SessionVariables.CurrentRegistrant.IsEmpty())
                return RedirectToAction("getstarted", "account", new { area = "", regaccount = account.Uid });

            var masterRegistrant = SessionVariables.CurrentUser.GetMasterRegistrant();

            if (masterRegistrant.IsParentOrGuardian)
            {
                // LOG: Somone Began Registration for ParticipationObjectType ParticipationObjectName by Account AccountName
                WebSessionUtilities.LogActivity(
                    actor: SessionVariables.CurrentUser,
                    webSessionId: SessionVariables.WebSession.ID.Value,
                    actionType: WebSessionActionType.BeganRegistration,
                    objectType: participationObject.GetWebSessionObjectType(),
                    objectId: participationObject.ObjectID.Value,
                    objectText: participationObject.Name,
                    indirectObjectType: ObjectType.Account,
                    indirectObjectId: participationObject.OwnerAccount.ID.Value,
                    indirectObjectText: participationObject.OwnerAccount.Name
                 );

                return RedirectToAction("register", "account", new { area = "", regaccount = account.Uid });

            }

            return RedirectToAction("registerstudent", "account", new { area = "", regaccount = account.Uid });
        }
        private ActionResult RedirectToInterested(Account account, ParticipationObject participationObject, SystemMessages msgs)
        {
            SetRegistrationStartPage(account);

            if (participationObject.IsEmpty())
                return RedirectToProfile(account.UniqueName, "Activity not found", MessageType.Error);

            if (!participationObject.IsSession())
                SessionVariables.Cart = Cart.GetNewCart(loggedInUser: SessionVariables.CurrentUser, msgs: msgs);

            if (SessionVariables.Cart.IsEmpty())
                return Redirect(participationObject.GetHomePageUrl());

            SessionVariables.Cart.AddCartItem(participationObject, msgs);

            return RedirectToAction("interested", "account", new { area = "", regaccount = account.Uid });
        }

        private void LoadSessionInvite(ActivityGroup session, string invite, string decline, SystemMessages msgs)
        {
            if (invite.IsGuid())
            {
                SessionVariables.SessionInvite = invite;

                if (decline.IsBoolean() && decline.ToBoolean())
                    session.DeclineInvite(SessionVariables.SessionInvite.ToGuid(), msgs);
            }
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
                var reg = new Regex(BasicLibrary.Utilities.UrlFriendlyRegEx, RegexOptions.IgnoreCase | RegexOptions.Singleline);
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