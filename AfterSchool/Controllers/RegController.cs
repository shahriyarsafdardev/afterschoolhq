using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;
using BasicLibrary.DataAccess;
using System.Collections.Specialized;
using Hangfire;

namespace AfterSchool.Controllers
{
    public class RegController : BaseController
    {
        // GET: Reg
        [HttpGet]
        public ActionResult New(string id, string preview, string day, string group, string profile, string login)
        {
            ActivityForm af = null;
            Account account = null;
            Activity activity = null;
            Activity parentActivity = null;
            ActivityGroup activityGroup = null;
            var msgs = new SystemMessages();

            ViewBag.Preview = false;

            if (group.IsGuid())
            {
                activityGroup = ActivityGroup.LoadByUid(group.ToGuid());
            }

            if (login == "error")
            {
                msgs.Add("Invalid Username and/or Password", MessageType.UserError);
            }

            if (id.IsGuid())
            {
                var dow = DayOfWeekASL.NotSet;
                activity = ActivityForm.LoadByUid(id.ToGuid()).ToActivity(true);
                if (dow.IsNotEmpty())
                {
                    if(DayOfWeekASL.TryParse(day, out dow) == false)
                    {
                        dow = DayOfWeekASL.NotSet;
                    }
                }

                if(dow == DayOfWeekASL.NotSet && activity.GetDays().Count > 0)
                {
                    dow = activity.GetDaysOfTheWeek().First();
                }

                SessionVariables.Cart = Cart.GetNewCart();
                SessionVariables.Cart.AddCartItem(activity.ID.Value, dow, null, null);

                id = "";
            }

            if (id.IsGuid())
            {
                if (preview.IsGuid())
                {
                    account = Account.LoadByUid(preview.ToGuid());
                    if (!account.IsEmpty())
                    {
                        af = ActivityForm.LoadByUid(id.ToGuid());
                        if (af.AccountId != account.ID)
                        {
                            af = null;
                        }
                        else
                        {
                            ViewBag.Preview = true;
                        }
                    }

                }
                else
                {
                    af = ActivityForm.LoadByUidByIsPublished(id.ToGuid(), true);
                    if (af.IsNotEmpty())
                    {
                        account = Account.Load(af.AccountId.Value);
                    }
                }

                if (af.IsNotEmpty())
                {
                    activity = af.ToActivity(true);
                    parentActivity = activity;

                    if (activity.IsChildActivity)
                    {
                        parentActivity = activity.GetParentActivity();
                    }
                }

                if (account.IsNotEmpty() && activity.IsNotEmpty())
                {
                    UpdateSession(SessionActionType.RegistrationFormView, account.ID, activity.ID);
                }

                ViewBag.Account = account;
                ViewBag.Activity = activity;
                ViewBag.ParentActivity = parentActivity;
                ViewBag.RegistrationPeriod = activity.GetActiveRegistrationPeriod();
                ViewBag.Messages = msgs;
                return View(af);
            }
            else if (SessionVariables.Cart.IsNotEmpty())
            {
                var items = SessionVariables.Cart.GetCartItems();

                if (account.IsEmpty())
                {
                    account = Activity.Load(items.First().ActivityId.Value).GetAccount();
                }

                LoadQuestions(group: activityGroup);

                ViewBag.Preview = preview.IsGuid();
                ViewBag.Messages = msgs;
                ViewBag.CartItems = items;
                ViewBag.Group = activityGroup;
                ViewBag.AdminMode = SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.LoggedInUser.HasAccessTo(account);

                ViewBag.FieldValues = Request.Params.Filter("question-").Count > 0 ? Request.Params : GetFieldValuesFromSession((List<FormQuestion>)ViewBag.Questions);

                if (profile.IsGuid())
                {
                    LoadAnswersFromProfile(profile.ToGuid());
                }

                return View("Multiple", account);
            }
            else if (id.IsNotBlank() && id.ToLower().Contains("a;"))
            {
                Account a = Account.LoadByUid(id.Split(';')[1].ToGuid());
                Response.Redirect(a.HomePageUrl);
            }

            return View();
        }

        [HttpGet]
        public ActionResult Add(string id)
        {
            var msgs = new SystemMessages();
            var reg = id;
            var registration = Registration.LoadByUid(reg.ToGuid());
            var newcart = false;

            if (registration.IsNotEmpty())
            {
                if (SessionVariables.Cart.IsEmpty() || SessionVariables.Cart.RegistrationId != registration.ID)
                {
                    SessionVariables.Cart = Cart.GetNewCart(registration.ID, msgs);
                    newcart = true;
                }

                var ras = registration.GetRegistrantActivities();
                if (newcart)
                {
                    foreach (var ra in ras)
                    {
                        SessionVariables.Cart.AddCartItem(ra, msgs);
                    }
                }
                              
                return RedirectToAction("org", new { id = registration.GetAccount().UniqueName, reg = registration.Uid.ToString() });
            }

            return View();
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            var msgs = new SystemMessages();
            var reg = id;
            var registration = Registration.LoadByUid(reg.ToGuid());
            var newcart = false;

            if (registration.IsNotEmpty())
            {
                if (SessionVariables.Cart.IsEmpty() || SessionVariables.Cart.RegistrationId != registration.ID)
                {
                    SessionVariables.Cart = Cart.GetNewCart(registration.ID, msgs);
                    newcart = true;
                }

                var ras = registration.GetRegistrantActivities();
                if (newcart)
                {
                    foreach (var ra in ras)
                    {
                        SessionVariables.Cart.AddCartItem(ra, msgs);
                    }
                }

                LoadFieldValues(registration, ras);
                LoadQuestions(ras, registration.GetActivityGroup());

                ViewBag.Preview = false;
                ViewBag.Messages = msgs;
                ViewBag.RegistrationUid = registration.Uid.ToString();
                ViewBag.Registration = registration;
                ViewBag.CartItems = SessionVariables.Cart.GetCartItems();
                ViewBag.AdminMode = SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.LoggedInUser.HasAccessTo(registration.GetAccount());                

                return View("Multiple", registration.GetAccount());
            }

            return View();
        }

        [HttpPost]
        public ActionResult LoadProfile(string id, string group, string profile, string activity)
        {
            if (activity.IsGuid())
            {
                var act = Activity.LoadByUid(activity.ToGuid());
                var account = act.GetAccount();
                return RedirectToAction("org", "reg", new { id = account.UniqueName, activity = act.Uid, profile = profile });
            }
            else
            {
                return RedirectToAction("new", "reg", new { id = id, group = group, profile = profile });
            }
        }

        [HttpPost]
        public ActionResult Login(string id, string group, string username, string password, string activity)
        {
            var msgs = new SystemMessages();

            if (username.IsEmpty())
            {
                msgs.Add("Username is required", MessageType.UserError);
            }
            if (password.IsEmpty())
            {
                msgs.Add("Password is required", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                WebUser user = null;

                if (username.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                {
                    user = WebUser.LoginByEmail(username, password);
                }
                else
                {
                    user = WebUser.LoginByUsername(username, password);
                }
                if (user.IsEmpty())
                {
                    msgs.Add("Invalid username and/or password", MessageType.UserError);
                }
                else
                {
                    msgs.AddRange(LoadSessionInfo(user, !Request.Params["remember"].IsEmpty()));
                }
            }

            if (msgs.HasNoErrors())
            {
                if (activity.IsGuid())
                {
                    var act = Activity.LoadByUid(activity.ToGuid());
                    var account = act.GetAccount();
                    return RedirectToAction("org", "reg", new { id = account.UniqueName, activity = act.Uid});
                }
                else
                {
                    return RedirectToAction("new", "reg", new { id = id, group = group });
                }
            }
            else
            {
                return RedirectToAction("new", "reg", new { id = id, group = group, login = "error" });
            }
        }

        [HttpGet]
        public ActionResult Logout(string id, string group, string activity)
        {
            SessionVariables.LoggedInUser = null;
            if (activity.IsGuid())
            {
                var act = Activity.LoadByUid(activity.ToGuid());
                var account = act.GetAccount();
                return RedirectToAction("org", "reg", new { id = account.UniqueName, activity = act.Uid });
            }
            else
            {
                return RedirectToAction("new", "reg", new { id = id, group = group });
            }
        }

        [HttpPost]
        public ActionResult ModifyActivities(string id, string reg, string preview, string day, string group, string profile)
        {
            var msgs = new SystemMessages();
            ActivityGroup activityGroup = null;
            Registration registration = null;
            var registrantTypeId = RegistrantType.Individual;
            var cartItems = SessionVariables.Cart.GetCartItems();
            var activities = cartItems.Select(x => x.GetActivity()).ToList();
            var costPerRegistrant = activities.Sum(x => x.CostPerRegistrant);
            var acceptCreditCard = activities.Where(x => x.AcceptCreditCard).Count() > 0;

            if (group.IsGuid())
            {
                activityGroup = ActivityGroup.LoadByUid(group.ToGuid());
            }
            else if (reg.IsGuid())
            {
                registration = Registration.LoadByUid(reg.ToGuid());
                activityGroup = registration.GetActivityGroup();
            }

            var questions = GetQuestions(activityGroup, cartItems);
            var questionAnswers = ValidateForm(questions, registrantTypeId, costPerRegistrant, acceptCreditCard, msgs, false);

            SessionVariables.Cart.QuestionAnswers = questionAnswers;

            return RedirectToAction("org", new {id = activities.First().GetAccount().UniqueName, reg = reg, group = group, profile = profile });
        }

        [HttpPost]
        public ActionResult SaveMultiple(string id, string paymenttype, string ccname, string ccnumber, string cccvc, string ccexpmonth, string ccexpyear, string reg, string group, string activity, string profile, string copy, string password)
        {
            var msgs = new SystemMessages();

            Registrant registrant = null;
            Registration registration = null;

            var cartItems = SessionVariables.Cart.GetCartItems();

            var registrantTypeId = RegistrantType.Individual;
            var activities = cartItems.Select(x => x.GetActivity()).ToList();
            var costPerRegistrant = activities.Sum(x => x.CostPerRegistrant);
            var acceptPayments = activities.Where(x => x.CanAcceptPayments).Any();
            var acceptCreditCard = activities.Where(x => x.AcceptCreditCard).Any();
            var acceptPayPal = activities.Where(x => x.AcceptPayPal).Any();
            var allowToPayLater = activities.Where(x => x.AllowToPayLater).Any();
            var account = activities.First().GetAccount();
            var isAdminMode = SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.LoggedInUser.HasAccessTo(account);
            var ras = new List<RegistrantActivity>();
            var addOnly = reg.IsGuid() && SessionVariables.LoggedInUser.IsEmpty();

            ActivityGroup activityGroup = null;
            var groupId = new int?();
            var activityId = new int?();

            if (group.IsGuid())
            {
                activityGroup = ActivityGroup.LoadByUid(group.ToGuid());
            }
            else if (reg.IsGuid())
            {
                registration = Registration.LoadByUid(reg.ToGuid());
                activityGroup = registration.GetActivityGroup();
            }

            if (activityGroup.IsNotEmpty())
                groupId = activityGroup.ID;

            if (cartItems.Count > 0)
            {
                var questions = GetQuestions(activityGroup, cartItems);  
                var questionAnswers = ValidateForm(questions, registrantTypeId, costPerRegistrant, acceptCreditCard, msgs);

                // need to check for conflicting registrations, date conflicts
                // TODO: need to only worry about this if the user is not running a lottery!!
                // may need to create RegistrationMode.Lottery
                /*
                if (activityGroup?.RegistrationModeId == RegistrationMode.Apply && 
                    DateRange.HasOverlappingDates(cartItems.Select(x => x.GetActivity().GetSchedule().GetInstances().Select(y => y.GetDateRange()))))
                {
                    // msgs.Add(string.Format("You have selected {0} with overlapping dates, please check your registration to ensure", UILabel.ActivityLabel(true).ToLower()), MessageType.Error);
                }                
                */

                if (cartItems.Count == 1)
                    activityId = cartItems.First().ActivityId;

                if (msgs.HasNoErrors())
                {
                    Database.ExecuteTransaction
                    (
                        action: (SystemMessages amsgs, DatabaseConnection adc) =>
                        {
                            var existingAnswers = new List<FormQuestionAnswer>();
                            var loggedInUserId = 0;

                            if (SessionVariables.LoggedInUser.IsNotEmpty() && !isAdminMode)
                                loggedInUserId = SessionVariables.LoggedInUser.ID.Value;

                            // is existing registration
                            if (reg.IsGuid())
                            {
                                registration = Registration.LoadByUid(reg.ToGuid(), adc);
                                registrant = registration.GetRegistrant(adc);
                                existingAnswers = registration.GetAnswers(adc);
                                activityGroup = registration.GetActivityGroup(adc);
                            }
                            else
                            {
                                registrant = account.AddRegistrant(loggedInUserId, registrantTypeId, false, amsgs, adc);
                                registration = registrant.AddRegistration(loggedInUserId, account.ID, groupId, amsgs, adc);
                            }

                            // save answers to the current registrant
                            amsgs.AddRange(AddAnswers(questions, questionAnswers, existingAnswers, registrant, registration, adc));

                            // check for profile copy or usage here
                            // also can set the user id, etc
                            msgs.AddRange(SaveRegistrantProfileInfo(registrant, profile, ref copy, isAdminMode, adc));

                            // this is an existing registration then cancel registrations for class the enrollee is leaving
                            if (reg.IsGuid())
                                amsgs.AddRange(RemoveActivities(registration, cartItems, adc));

                            // need to attach the registration to a specific reg period for stats info
                            RegistrationPeriod regperiod = null;
                            var regPeriodId = new int?();
                            var registrationMode = RegistrationMode.Register;

                            // get activity group details, like current reg period and registration mode
                            if (activityGroup.IsNotEmpty())
                            {
                                regperiod = activityGroup.GetCurrentRegistrationPeriod(adc);

                                if (regperiod.IsNotEmpty())
                                    regPeriodId = regperiod.ID;

                                registrationMode = activityGroup.RegistrationModeId;
                            }

                            // add each activity from the cart
                            cartItems.ForEach(item =>
                            {
                                amsgs.AddRange(ProcessCartItem(item, registrant, activityGroup, ref registration, ref registrationMode, regperiod, ref regPeriodId, cartItems, ras, reg, account, groupId, adc));
                            });

                            // set period
                            if (regPeriodId.HasValue)
                            {
                                registration.RegistrationPeriodId = regPeriodId;
                                amsgs.AddRange(registration.Save(adc));
                            }

                            // process payment for all at once
                            if (amsgs.HasNoErrors() && costPerRegistrant > 0 && acceptCreditCard && paymenttype == PaymentType.CreditCard.ToString())
                                amsgs.AddRange(registration.ProcessStripePayment(registrant, costPerRegistrant.Value, 1, ccname, ccnumber, cccvc, ccexpmonth, ccexpyear, adc));

                            SessionVariables.Cart.RegistrationId = registration.ID;
                            amsgs.AddRange(SessionVariables.Cart.Save(adc));

                            amsgs.AddRange(registration.FindAndSetFirstRegistration(registrant.FirstName, registrant.LastName, registrant.Email, adc));
                            amsgs.AddRange(registrant.FindAndSetFirstRegistrant(registrant.FirstName, registrant.LastName, registrant.Email, adc));

                            // update registration last updated
                            amsgs.AddRange(registration.WasUpdated(adc));

                            // create a user account for this registrant if the user has a newly created a password
                            // otherwise do not create any accounts automatically
                            if (registrant.ParentRegistrantId.IsEmpty() && password.IsNotBlank())
                                CreateRegistrantAccount(registrant, password, registrantTypeId, account, questions, copy, loggedInUserId, isAdminMode, amsgs, adc);

                        },
                        onSuccess: (SystemMessages smsgs) =>
                        {
                            msgs.AddRange(smsgs);

                            // send registrant confirmation email 
                            //      if the registration is free or 
                            //      they have paid or 
                            //      if they are paying in person, 
                            //      paypal payments will skip over this
                            // also don't send if editing a registration or adding a new registration from the admin side
                            if ((costPerRegistrant == 0 || registration.DatePaid.HasValue || paymenttype == "InPerson") && isAdminMode == false)
                                BackgroundJob.Enqueue(() => Registration.SendConfirmation(registration.ID.Value, true, null));

                            // save actions for tracking and stats
                            var actionType = SessionActionType.RegistrationCompletedView;

                            if (reg.IsNotEmpty())
                                actionType = SessionActionType.RegistrationUpdated;

                            UpdateSession(actionType, account.ID, activityId, SessionVariables.LoggedInUser?.ID, registrant.ID, registration.ID, null, groupId);
                        },
                        onFailure: (SystemMessages fmsgs) =>
                        {
                            msgs.AddRange(fmsgs);
                        }
                    );                    
                }
            }

            LoadQuestions(group: activityGroup);

            ViewBag.CartItems = SessionVariables.Cart.GetCartItems();
            ViewBag.Preview = false;
            ViewBag.RegistrationUid = reg;
            ViewBag.Messages = msgs;
            ViewBag.AdminMode = isAdminMode;

            if (msgs.HasNoErrors())
            {
                if (reg.IsGuid() && !addOnly)
                {
                    msgs.Add("Registration Updated", MessageType.Success);
                    LoadFieldValues(registration);
                    ViewBag.Registration = registration;
                    return View("Multiple", account);
                }
                else
                { 
                    if (paymenttype != PaymentType.PayPal.ToString())
                    {
                        SessionVariables.Cart = null;
                        return RedirectToAction("thankyou", "reg", new { id = registration.Uid.ToString() });
                    }
                    else
                    {
                        return View("paypal");
                    }
                }
            }
            else
            {
                if (activity.IsGuid())
                {
                    LoadSingleActivityViewBag(account, cartItems.First().GetActivity(), msgs, false);
                    return View("New", cartItems.First().GetActivity());
                }
                else
                {
                    ViewBag.FieldValues = Request.Params;
                    return View("Multiple", account);
                }
            }
        }

        private SystemMessages ProcessCartItem(CartItem item, Registrant registrant, ActivityGroup group, ref Registration registration, ref RegistrationMode registrationMode, RegistrationPeriod regperiod, ref int? regPeriodId, List<CartItem> cartItems, List<RegistrantActivity> ras, string reg, Account account, int? groupId, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            var numberOfSeatsRequested = 1;
            var numberOfSeatsLeft = 0;
            var numberOfWaitingListSeatsLeft = 0;
            var totalSeatsLeft = 0;
            var act = item.GetActivity(dc);

            //check if already registered for this activity
            if (registrant.ParentRegistrantId.HasValue && RegistrantActivity.LoadByActivityIdByRegistrantId(act.ID.Value, registrant.ParentRegistrantId.Value, dc: dc).IsNotEmpty())
            {
                msgs.Add(string.Format("You're already registered for {0}", act.Name), MessageType.UserError);
            }

            // registration mode is always set by the activityGroup if it exists
            if (group.IsEmpty())
            {
                registrationMode = act.RegistrationModeId;
                regperiod = act.GetActiveRegistrationPeriod(dc);
                if (regperiod.RegistrationModeId != RegistrationMode.NotSet)
                {
                    registrationMode = regperiod.RegistrationModeId;
                }
                regPeriodId = regperiod.ID;
            }

            if (registrationMode == RegistrationMode.Register)
            {
                msgs.AddRange(SetSeatAvailable(item, act, numberOfSeatsRequested, out numberOfSeatsLeft, out numberOfWaitingListSeatsLeft, out totalSeatsLeft, dc: dc));
            }

            if (msgs.HasNoErrors())
            {
                var choiceNumber = GetChoiceNumber(act, cartItems, item);

                var registrantSeats = numberOfSeatsRequested;
                var registrantWaitingListSeats = 0;
                var registrantApplicationSeats = 0;
                var registrantInvitationSeats = 0;
                var registrantDeclinedSeats = 0;

                UpdateRegistrantSeats(act, registrationMode, numberOfSeatsRequested, numberOfSeatsLeft, ref registrantSeats, ref registrantWaitingListSeats, ref registrantApplicationSeats, ref registrantInvitationSeats, ref registrantDeclinedSeats);

                msgs.AddRange(CheckOverlappingDates(registrationMode, dc: dc));

                // adds new registrant activity or updates existing activity, assumes a registrant and one activity is a unique record
                ras.Add(registrant.AddOrUpdateActivity(registration.ID.Value, act.ID.Value, item.Day, registrantSeats, registrantWaitingListSeats, registrantApplicationSeats, registrantInvitationSeats, registrantDeclinedSeats, choiceNumber, regPeriodId, groupId, 0, msgs, dc: dc));
            }

            if (msgs.HasNoErrors())
            {
                // checks before it adds a new one
                msgs.AddRange(registration.AddActivity(act, 0, dc: dc));
            }

            if (msgs.HasNoErrors())
            {
                var actionType = SessionActionType.ActivityRegistrationCompleted;
                if (reg.IsGuid())
                {
                    actionType = SessionActionType.ActivityRegistrationUpdated;
                }
                UpdateSession(actionType, account.ID, act.ID, null, registrant.ID, registration.ID, regPeriodId, null, dc: dc);
            }

            return msgs;
        }

        [HttpGet]
        public ActionResult ThankYou(string id)
        {
            Registration reg = null;
            Account a = null;
            List<RegistrantActivity> activities = null;
            Registrant registrant = null;

            if (id.IsGuid())
            {
                SessionVariables.Cart = null;
                reg = Registration.LoadByUid(id.ToGuid());
                a = reg.GetAccount();
                activities = reg.GetRegistrantActivities().OrderBy(x => x.PreferenceNumber).OrderBy(x => (int)x.Day).ToList();
                registrant = reg.GetRegistrant();
            }

            ViewBag.Account = a;
            ViewBag.Activities = activities;
            ViewBag.Registrant = registrant;
            return View(reg);
        }

        [HttpGet]
        public ActionResult Thanks(string id)
        {
            RegistrantActivity ra = null;
            ActivityForm af = null;
            Account a = null;

            if (id.IsGuid())
            {
                ra = RegistrantActivity.LoadByUid(id.ToGuid());
                af = ActivityForm.LoadByActivityIdByIsActive(ra.ActivityId.Value, true);
                a = Account.Load(af.AccountId.Value);
            }

            ViewBag.RegistrantActivity = ra;
            ViewBag.Account = a;
            return View(af);
        }

        [HttpGet]
        public ActionResult PayPal(string id)
        {
            var ra = RegistrantActivity.LoadByUid(id.ToGuid());
            var af = ActivityForm.LoadByActivityIdByIsActive(ra.ActivityId.Value, true);
            var a = Account.Load(af.AccountId.Value);
            ViewBag.Messages = new SystemMessages();
            ViewBag.RegistrantActivity = ra;
            ViewBag.Account = a;
            return View(af);
        }

        [HttpGet]
        public ActionResult PayPalThanks(string id)
        {
            var msgs = new SystemMessages();

            RegistrantActivity ra = null;
            ActivityForm af = null;
            Account a = null;

            if (id.IsGuid())
            {
                ra = RegistrantActivity.LoadByUid(id.ToGuid());
                af = ActivityForm.LoadByActivityIdByIsActive(ra.ActivityId.Value, true);
                a = Account.Load(af.AccountId.Value);
            }

            ViewBag.Messages = msgs;
            ViewBag.RegistrantActivity = ra;
            ViewBag.Account = a;
            if (msgs.HasNoErrors())
            {
                return View("thanks", af);
            }
            else
            {
                return View(af);
            }
        }

        [HttpGet]
        public ActionResult PayPalCancel(string id)
        {
            var msgs = new SystemMessages();

            RegistrantActivity ra = null;
            ActivityForm af = null;
            Account a = null;

            if (id.IsGuid())
            {
                ra = RegistrantActivity.LoadByUid(id.ToGuid());
                af = ActivityForm.LoadByActivityIdByIsActive(ra.ActivityId.Value, true);
                a = Account.Load(af.AccountId.Value);
                msgs.Add("It appears you cancelled your registration. If this is a mistake, please try again.", MessageType.Notice);
            }

            ViewBag.Messages = msgs;
            ViewBag.RegistrantActivity = ra;
            ViewBag.Account = a;
            return View("paypal", af);
        }

        [HttpPost]
        public ActionResult PayPalIPN(string id, string mc_gross, string payment_status, string txn_id)
        {
            Response.Clear();
            Response.StatusCode = 200;
            Response.End();
            Response.Flush();

            var msgs = new SystemMessages();
            RegistrantActivity ra = null;

            if (!id.IsGuid())
            {
                msgs.Add("Invalid Identifier", MessageType.Error);
            }
            else
            {
                ra = RegistrantActivity.LoadByUid(id.ToGuid());
                if (ra.IsEmpty())
                {
                    msgs.Add("Invalid Registrant Activity Record", MessageType.Error);
                }
            }
            if (mc_gross.IsEmpty() || !mc_gross.IsNumber())
            {
                msgs.Add("Invalid Payment Amount", MessageType.Error);
            }
            if (payment_status.IsEmpty() || payment_status.ToLower() != "completed")
            {
                msgs.Add("Invalid Payment Status", MessageType.Error);
            }

            if (msgs.HasNoErrors())
            {
                var activity = ra.ToActivity(true);
                var af = activity.GetActiveActivityForm();

                var rc = new RestClient();
                rc.Url = "https://www.paypal.com/cgi-bin/webscr";
                rc.RequestMethod = RestClientRequestMethod.POST;
                rc.ContentType = RestClientContentTypeEnum.URLEncoded;
                rc.Parameters.Add("cmd", "_notify-validate");

                foreach (string key in Request.Form.Keys)
                {
                    rc.Parameters.Add(key, Request.Form[key]);
                }

                var responseFromServer = rc.DoRequest();

                if (responseFromServer.ToLower().Contains("verified"))
                {
                    if ((activity.CostPerRegistrant.Value * ra.TotalSeats) == Request["mc_gross"].ToDecimal())
                    {
                        var seatsLeft = activity.GetSeatsRemaining();

                        //if there aren't enough seats available then only move some of the seats off the waiting list, otherwise move everyone from the waiting list to the main list
                        if (ra.NumberOfWaitingListSeats.Value > 0)
                        {
                            if (ra.NumberOfWaitingListSeats.Value > seatsLeft)
                            {
                                ra.NumberOfSeats = seatsLeft;
                                ra.NumberOfWaitingListSeats = ra.NumberOfWaitingListSeats.Value - seatsLeft;
                            }
                            else
                            {
                                ra.NumberOfSeats = ra.NumberOfWaitingListSeats + ra.NumberOfSeats.Value;
                                ra.NumberOfWaitingListSeats = 0;
                            }
                        }

                        ra.PayPalTransactionId = txn_id;
                        ra.DatePaid = DateTime.UtcNow;
                        ra.AmountPaid = Request["mc_gross"].ToDecimal();
                        ra.PaymentTypeId = PaymentType.PayPal;
                        msgs.AddRange(ra.Save());

                        if (msgs.HasNoErrors())
                            BackgroundJob.Enqueue(() => Registration.SendConfirmation(ra.GetRegistration(null).ID.Value, true, null));
                    }
                    else
                        //notify the user that full payment was not received and they must pay in full
                        BackgroundJob.Enqueue(() => Registrant.SendIncompletePayPalPaymentEmail(ra.ToRegistrant().ID.Value, af));

                }
                else
                    //notify the user the payment was declined or not received from paypal
                    BackgroundJob.Enqueue(() => Registrant.SendPayPalPaymentError(ra.ToRegistrant().ID.Value, af));

            }

            //send error to admin

            foreach (string key in Request.Params.Keys)
                msgs.Add(new SystemMessage(key + ": " + Request.Params[key], MessageType.Notice));

            SendGridHelper.SendMail(
                fromEmail: AfterSchoolLibrary.Email.AppEmailAddress,
                toEmail: AfterSchoolLibrary.Email.AppAdminEmailAddress,
                subject: "EnrollHQ PayPal Errors",
                body: msgs.RenderMessageList(MessageOutputType.Html)
            );


            return View();
        }

        [HttpGet]
        public ActionResult Org(string id, string activity, string reg, string group, string preview, string profile, string invite, string declined, string reset)
        {
            var activities = new List<Activity>();
            Account account = null;
            ActivityGroup activityGroup = null;
            var groups = new List<ActivityGroup>();
            var msgs = new SystemMessages();
            bool adminMode = false;

            if (reset.IsBoolean() && reset.ToBoolean())
            {
                SessionVariables.Cart = null;
                return RedirectToAction("org", new { id, activity, reg, group, preview, profile, invite, declined });
            }

            //go to account page
            if (id.IsNotBlank())
            {
                account = Account.LoadByUniqueName(id);
                if (account.IsNotEmpty())
                {
                    adminMode = SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.LoggedInUser.HasAccessTo(account);

                    ViewBag.UseCart = false;

                    if (reg.IsGuid())
                    {
                        var registration = Registration.LoadByUid(reg.ToGuid());
                        activityGroup = registration.GetActivityGroup();
                        activities = activityGroup.GetActivities(false, false, null, null, false).Where(x => x.IsPubliclyViewable).ToList();
                        ViewBag.Registration = registration;

                        if (activityGroup.IsNotEmpty()) ViewBag.UseCart = true;
                    }
                    else if (group.IsGuid())
                    {
                        ViewBag.UseCart = true;
                        activityGroup = ActivityGroup.LoadByUidByIsPublished(group.ToGuid(), true);
                        if (activityGroup.IsNotEmpty())
                        {
                            if (activityGroup.IsInviteOnly && !adminMode)
                            {
                                if (invite.IsGuid())
                                {
                                    if (declined == "1")
                                        return RedirectToAction("declineinvite", "reg", new { id = id, invite = invite });
                                    else
                                    {
                                        var rag = RegistrantActivityGroup.LoadByUid(invite.ToGuid());
                                        if (rag.IsNotEmpty())
                                            activities = rag.GetActivities();
                                    }
                                }
                                else
                                    msgs.Add("Error loading invite", MessageType.UserError);
                            }
                            else
                                activities = activityGroup.GetActivities(false, false, null, null, false)
                                    .Where(x => x.IsPubliclyViewable)
                                    .OrderBy(x => x.Name)
                                    .ToList();
                        }
                        else
                            activities = account.GetPublishedActivities();
                    }
                    else if (activity.IsGuid())
                    {
                        var act = Activity.LoadByUid(activity.ToGuid());

                        SessionVariables.Cart = Cart.GetNewCart();
                        SessionVariables.Cart.AddCartItem(act.ID.Value, DayOfWeekASL.NotSet, null, null);

                        LoadSingleActivityViewBag(account, act, new SystemMessages(), preview.IsBoolean() ? preview.ToBoolean() : false);

                        if (profile.IsGuid())
                            LoadAnswersFromProfile(profile.ToGuid());

                        return View("new", act);
                    }
                    else
                    {
                        groups = account.GetPublishedActivityGroups()
                            .Where(x => x.IsPubliclyViewable && (!x.IsInviteOnly || adminMode))
                            .Where(x => x.IsOpenNow() || x.WillOpen())
                            .ToList();

                        if (!groups.Any())
                            activities = account.GetPublishedActivities().ToList();
                        else
                            activities = account.GetPublishedActivities()
                                .Where(x => !x.ActivityGroupId.HasValue)
                                .ToList();
                    }
                }
            }

            if (SessionVariables.Cart.IsNotEmpty() && SessionVariables.Cart.RegistrationId.HasValue && reg.IsEmpty())
                SessionVariables.Cart = Cart.GetNewCart();

            if (account.IsNotEmpty())
                UpdateSession(SessionActionType.OrganizationHomeView, account.ID);

            ViewBag.AdminMode = adminMode;
            ViewBag.Activities = activities
                .OrderByDescending(x => x.GetSeatsTaken())
                .ThenBy(x => x.GetSeatsRemaining())
                .ThenBy(x => x.Name)
                .ToList();
            ViewBag.Groups = groups;
            ViewBag.Group = activityGroup;
            return View(account);
        }

        private void LoadAnswersFromProfile(Guid profile)
        {
            var registrant = Registrant.LoadByUid(profile);
            var answers = registrant.GetAnswers();

            // map questions to answers
            var questions = (List<FormQuestion>)ViewBag.Questions;
            foreach (var answer in answers)
            {
                FormQuestion question = null;
                if (answer.QuestionDataTypeId != QuestionDataType.NotSet)
                {
                    question = questions.Find(x => x.QuestionDataTypeId == answer.QuestionDataTypeId);
                }
                else
                {
                    question = questions.Find(x =>
                                    x.QuestionDataTypeId == answer.QuestionDataTypeId &&
                                    x.FormQuestionGroupId == answer.FormQuestionFormQuestionGroupId &&
                                    x.Text == answer.FormQuestionText);
                }

                if (question.IsNotEmpty())
                {
                    answer.SetProperyValue("FormQuestionId", question.ID);
                    answer.SetProperyValue("FormQuestionQuestionTypeId", question.QuestionTypeId);
                }
            }

            ViewBag.FieldValues = GetFieldValuesFromAnswers(answers.Where(x => x.FormQuestionId.HasValue).ToList());
        }

        public ActionResult DeclineInvite(string id, string invite)
        {
            var account = Account.LoadByUniqueName(id);
            var rag = RegistrantActivityGroup.LoadByUid(invite.ToGuid());
            ViewBag.Messages = rag.DeclineInvite(account, 0);
            return View(account);
        }
        
        private NameValueCollection GetFieldValuesFromSession(List<FormQuestion> questions)
        {
            var fieldValues = new NameValueCollection();
            if (SessionVariables.Cart.IsNotEmpty())
            {
                if (SessionVariables.Cart.QuestionAnswers.IsNotEmpty())
                {
                    foreach (var key in SessionVariables.Cart.QuestionAnswers.Keys)
                    {
                        fieldValues.Add(
                            string.Format(
                                "question-{0}",
                                key
                            ),
                            SessionVariables.Cart.QuestionAnswers[key]
                        );
                    }
                }
                else
                {
                    if (SessionVariables.Cart.GradeLevel.HasValue && questions.Where(x => x.QuestionDataTypeId == QuestionDataType.RegistrantGrade).Any())
                    {
                        fieldValues.Add(
                            string.Format(
                                "question-{0}",
                                questions.Find(x => x.QuestionDataTypeId == QuestionDataType.RegistrantGrade).ID
                            ),
                            GradeLevel.Load(SessionVariables.Cart.GradeLevel.Value).Name
                        );
                    }
                    if (SessionVariables.Cart.Gender.HasValue && questions.Where(x => x.QuestionDataTypeId == QuestionDataType.RegistrantGender).Any())
                    {
                        fieldValues.Add(
                            string.Format(
                                "question-{0}",
                                questions.Find(x => x.QuestionDataTypeId == QuestionDataType.RegistrantGender).ID
                            ),
                            SessionVariables.Cart.Gender.ToString()
                        );
                    }
                }
            }
            return fieldValues;
        }

        private NameValueCollection GetFieldValuesFromAnswers(List<FormQuestionAnswer> answers)
        {
            var fieldValues = new NameValueCollection();
            foreach (var answer in answers)
            {
                if (Request["copy"] == "1")
                {
                    switch (answer.QuestionDataTypeId)
                    {
                        case QuestionDataType.RegistrantFirstName:
                        case QuestionDataType.RegistrantDateOfBirth:
                        case QuestionDataType.RegistrantAllergies:
                        case QuestionDataType.RegistrantImmunizations:
                        case QuestionDataType.RegistrantMedicalConditions:
                            continue;
                        case QuestionDataType.RegistrantGrade:
                            if (SessionVariables.Cart.GradeLevel.HasValue)
                            {
                                fieldValues.Add(
                                    string.Format(
                                        "question-{0}",
                                        answer.FormQuestionId
                                    ),
                                    GradeLevel.Load(SessionVariables.Cart.GradeLevel.Value).Name
                                );
                            }
                            continue;
                        case QuestionDataType.RegistrantGender:
                            if (SessionVariables.Cart.Gender.HasValue)
                            {
                                fieldValues.Add(
                                    string.Format(
                                        "question-{0}",
                                        answer.FormQuestionId
                                    ),
                                    SessionVariables.Cart.Gender.ToString()
                                );
                            }
                            continue;
                    }
                }

                fieldValues.Add(string.Format("question-{0}", answer.FormQuestionId.ToString()), answer.Text);

                if (answer.FormQuestionQuestionTypeId == QuestionType.RadioMatrix)
                {
                    var q = answer.ToFormQuestion(true);
                    var cols = q.Options.Split(';')[0].Split(',');
                    var rows = q.Options.Split(';')[1].Split(',');

                    foreach (var row in rows.Select(x => x.Trim()))
                    {
                        foreach (var col in cols.Select(x => x.Trim()))
                        {
                            var radioname = string.Format("question-{0}-{1}", q.ID, col); // name + "-" + col;
                            if (answer.Text.Contains(string.Format("{0}={1}", col, row)))
                            {
                                fieldValues.Add(radioname, row);
                            }
                        }
                    }
                }
            }
            return fieldValues;
        }

        private void LoadFieldValues(Registration registration, List<RegistrantActivity> ras = null)
        {
            var fieldValues = GetFieldValuesFromAnswers(registration.GetAnswers());

            if (ras.IsEmpty())
            {
                ras = registration.GetRegistrantActivities();
            }

            var maxCount = ras.Max(x => x.PreferenceNumber.Value);
            foreach (var ra in ras)
            {
                fieldValues.Add(string.Format("preference-{0}", ra.PreferenceNumber.Value), ra.ActivityId.Value.ToString());
            }

            ViewBag.FieldValues = fieldValues;
        }

        private void LoadQuestions(List<RegistrantActivity> ras = null, ActivityGroup group = null)
        {
            var questions = new List<FormQuestion>();
            var finalQuestions = new List<FormQuestion>();

            if (group.IsEmpty())
            {
                if (ras.IsNotEmpty())
                    ras.ForEach(ra => questions.AddRange(ra.ToActivity().GetActiveActivityForm().GetActiveQuestions()));
                else
                    SessionVariables.Cart.GetCartItems().ForEach(cartItem => questions.AddRange(Activity.Load(cartItem.ActivityId.Value).GetActiveActivityForm().GetActiveQuestions()));
            }
            else
            {
                questions.AddRange(group.GetForm().GetActiveQuestions());
            }

            finalQuestions.AddRange(questions.Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet).DistinctBy(x => x.QuestionDataTypeId));
            finalQuestions.AddRange(questions.Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet).DistinctBy(x => x.ID.Value));

            var questionGroups = new List<FormQuestionGroup>();
            var finalQuestionGroups = new List<FormQuestionGroup>();
            var groupIds = finalQuestions.Select(x => x.FormQuestionGroupId.Value).Distinct().ToList();
            groupIds.ForEach(groupId => questionGroups.Add(FormQuestionGroup.Load(groupId)));
            finalQuestionGroups.AddRange(questionGroups.Where(x => x.QuestionGroupDataTypeId != QuestionGroupDataType.NotSet).DistinctBy(x => x.QuestionGroupDataTypeId));
            finalQuestionGroups.AddRange(questionGroups.Where(x => x.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet).DistinctBy(x => x.ID.Value));

            ViewBag.Questions = finalQuestions;
            ViewBag.QuestionGroups = finalQuestionGroups;
        }

        private SystemMessages CheckOverlappingDates(RegistrationMode registrationMode, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();
            if (registrationMode == RegistrationMode.Register)
            {
                // if an enrollee already has successfully registered for another activity on that day, 
                // then they need to be added to the waiting list for this activity on this day
                // Should work for day not set also
                /*
                if (ras.Where(x => x.Day == item.Day && x.NumberOfSeats >= 1).Count() > 0 && registrantSeats > 0)
                {
                    registrantWaitingListSeats = registrantSeats;
                    registrantSeats = 0;
                }
                */
                /*
                // find matching registrations based on name and email
                // check for conflicts in the schedule
                // get only active schedules
                // probably put items that are in conflict into waiting list
                var matches = registration.FindRelatedRegistrations(registrant.FirstName, registrant.LastName, registrant.Email, adc).Where(x => x.ID != registration.ID).ToList();
                var activities1 = new List<Activity>();
                var schedules = new List<ActivitySchedule>();
                var instances = new List<ActivityInstance>();

                foreach (var match in matches)
                {
                    activities1.AddRange(match.GetRegistrantActivities(adc).Select(x => x.ToActivity()));
                }
                schedules.AddRange(activities.Select(x => x.GetSchedule(dc: adc)));
                foreach(var schedule in schedules)
                {
                    instances.AddRange(schedule.GetInstances(dc: adc));
                }

                DateRange.HasOverlappingDates();
                */
            }
            return msgs;
        }

        private SystemMessages SaveRegistrantProfileInfo(Registrant registrant, string profile, ref string copy, bool isAdminMode, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            Registrant masterRegistrant = null;

            // if loading from a profile
            // if not a copy of an existing profile, then mark say where you got the profile information from originally
            if (profile.IsGuid() && copy.IsBlank())
            {
                masterRegistrant = Registrant.LoadByUid(profile.ToGuid(), dc: dc);

                // sometimes users use a profile but they're really trying to copy.
                // so if it's not a match to the master registrant then don't connect it
                // which basically means they selected a profile but then changed the name
                if (registrant.IsRelated(masterRegistrant))
                {
                    registrant.ParentRegistrantId = masterRegistrant.ID;
                }
                else
                {
                    copy = "1";
                }
            }

            // set who owns the profile
            if (SessionVariables.LoggedInUser.IsNotEmpty())
            {
                // don't want to associate a user who is a master login making edits
                if (!isAdminMode)
                {
                    registrant.WebUserId = SessionVariables.LoggedInUser.ID;
                }
                else if (masterRegistrant.IsNotEmpty())
                {
                    // if there is a matching master record then go ahead and set the owner to the master record owner
                    registrant.WebUserId = masterRegistrant.WebUserId;
                }
            }

            msgs.AddRange(registrant.Save(dc));

            return msgs;
        }

        private static void CreateRegistrantAccount(Registrant registrant, string password, RegistrantType registrantTypeId, Account account, List<FormQuestion> questions, string copy, int loggedInUserId, bool isAdminMode, SystemMessages msgs, DatabaseConnection dc)
        {
            var isParentRegistration = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1FirstName).Any();
            var userFirstName = registrant.FirstName;
            var userLastName = registrant.LastName;
            var userEmail = registrant.Email;

            // first create master copy of registrant (if this is a first time registrant)
            // have to create a master copy so that the user can edit their own profile when logged in without editing existing registration information 
            // need to create a master registrant if there is a logged in user no matter what
            var masterRegistrant = registrant.CreateMasterCopy(questions, loggedInUserId, msgs, dc: dc);

            // don't create any extra records if this is a copy of an existing record that has 
            // already went through this process of creating parents, a user account, etc
            // don't create an account if the user is logged in
            if (loggedInUserId == 0 && copy.IsBlank())
            {
                if (isParentRegistration)
                {
                    // get parent info
                    userFirstName = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1FirstName)
                        .First()
                        .GetAnswer(registrant.ID.Value, dc: dc)
                        .Text;

                    if (questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1LastName).Any())
                        userLastName = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1LastName)
                            .First()
                            .GetAnswer(registrant.ID.Value, dc: dc)
                            .Text;

                    if (questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1Email).Any())
                        userEmail = questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent1Email)
                            .First()
                            .GetAnswer(registrant.ID.Value, dc: dc)
                            .Text;
                }

                // create a user account for the registrant or registrant parent
                var user = masterRegistrant.MatchCreateOrUpdateUser(userFirstName, userLastName, userEmail, password, SessionVariables.IsAfterSchool.Value, isAdminMode, msgs, dc: dc);

                if (user.IsEmpty())
                    return;

                //add registrant to the main new user account;
                registrant.WebUserId = user.ID;
                msgs.AddRange(registrant.Save(dc));

                // set the new user as the logged in user, so if they go to register again it will already be loaded
                if (password.IsNotBlank())
                    SessionVariables.LoggedInUser = user;

                // need to also create a parent profile as well since we have some info
                // skip parent profiles for now, because no real way to know who the parent is 
                // need to add relationship table between registrants
                if (isParentRegistration && false) 
                {
                    var allQuestionTypes = Enum.GetValues(typeof(QuestionDataType)).Cast<QuestionDataType>().Where(x => x != QuestionDataType.NotSet);
                    var parent1QuestionTypes = allQuestionTypes.Where(x => x.ToString().Contains("Parent1")).ToList();

                    // check for existing parent1 info

                    // what if parent already exists
                    var parent1 = user.CreateRegistrant(registrantTypeId, true, msgs, dc: dc);

                    // set default email answer on parent, can be overwritten if there is a parent email
                    parent1.AddAnswer(userEmail, QuestionDataType.RegistrantEmail, loggedInUserId, msgs, dc: dc);

                    foreach (var qt in parent1QuestionTypes)
                    {
                        if (questions.Where(x => x.QuestionDataTypeId == qt).Any())
                        {
                            var parentQuestion = questions.Where(x => x.QuestionDataTypeId == qt).First();
                            var answerText = parentQuestion.GetAnswer(registrant.ID.Value, dc: dc).Text;

                            // translate answer from parent field on registrant to registrant field on new parent registrant
                            var parentQt = (QuestionDataType)Enum.Parse(typeof(QuestionDataType), qt.ToString().Replace("Parent1", "Registrant"));
                            parent1.AddAnswer(answerText, parentQt, loggedInUserId, msgs, dc: dc);
                        }
                    }

                    // check if parent 2 info is there
                    var parent2QuestionTypes = allQuestionTypes.Where(x => x.ToString().Contains("Parent2")).ToList();

                    if (questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent2FirstName).Any() &&
                        questions.Where(x => x.QuestionDataTypeId == QuestionDataType.Parent2FirstName).First()
                        .GetAnswer(registrant.ID.Value, dc: dc).Text.IsNotBlank())
                    {
                        var parent2 = user.CreateRegistrant(registrantTypeId, true, msgs, dc: dc);

                        // set default email answer on parent, can be overwritten if there is a parent email
                        parent2.AddAnswer(userEmail, QuestionDataType.RegistrantEmail, loggedInUserId, msgs, dc: dc);

                        foreach (var qt in parent2QuestionTypes)
                        {
                            if (questions.Where(x => x.QuestionDataTypeId == qt).Any())
                            {
                                var parentQuestion = questions.Where(x => x.QuestionDataTypeId == qt).First();
                                var answerText = parentQuestion.GetAnswer(registrant.ID.Value, dc: dc).Text;

                                var parentQt = (QuestionDataType)Enum.Parse(typeof(QuestionDataType), qt.ToString().Replace("Parent2", "Registrant"));
                                var answer = parent2.AddAnswer(answerText, parentQt, loggedInUserId, msgs, dc: dc);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateRegistrantSeats(Activity activity, RegistrationMode registrationMode, int numberOfSeatsRequested, int numberOfSeatsLeft, ref int registrantSeats, ref int registrantWaitingListSeats, ref int registrantApplicationSeats, ref int registrantInvitationSeats, ref int registrantDeclinedSeats)
        {
            // if they're using paypal we can't confirm until we receive a confirmed payment or paying later, put them on the waiting list
            if (Request.Params["paymenttype"] == PaymentType.PayPal.ToString())
            {
                registrantSeats = 0;
                registrantWaitingListSeats = numberOfSeatsRequested;
            }
            // if they're paying later, but there's an option to pay now, then put them on the waiting list
            else if ((activity.AcceptCreditCard || activity.AcceptPayPal) && Request.Params["paymenttype"] == "InPerson")
            {
                registrantSeats = 0;
                registrantWaitingListSeats = numberOfSeatsRequested;
            }
            else if (numberOfSeatsRequested > numberOfSeatsLeft)
            {
                registrantSeats = numberOfSeatsLeft;
                registrantWaitingListSeats = numberOfSeatsRequested - numberOfSeatsLeft;
            }

            if (registrationMode == RegistrationMode.Apply)
            {
                registrantSeats = 0;
                registrantWaitingListSeats = 0;
                registrantApplicationSeats = numberOfSeatsRequested;
            }
        }

        private SystemMessages SetSeatAvailable(CartItem item, Activity act, int numberOfSeatsRequested, out int numberOfSeatsLeft, out int numberOfWaitingListSeatsLeft, out int totalSeatsLeft, DatabaseConnection dc)
        {
            var msgs = new SystemMessages();

            numberOfSeatsLeft = act.GetSeatsRemaining(dc);
            numberOfWaitingListSeatsLeft = act.GetWaitingListSeatsRemaining(dc);
            totalSeatsLeft = numberOfSeatsLeft + numberOfWaitingListSeatsLeft;

            if (numberOfSeatsRequested > totalSeatsLeft)
            {
                msgs.Add(string.Format("There are not enough seats or waiting list seats available for {0}", act.Name), MessageType.UserError);
            }

            return msgs;
        }

        private List<FormQuestion> GetQuestions(ActivityGroup activityGroup, List<CartItem> items)
        {
            var questions = new List<FormQuestion>();
            var finalquestions = new List<FormQuestion>();
            foreach (var cartItem in items)
            {
                var act = Activity.Load(cartItem.ActivityId.Value);
                if (activityGroup.IsEmpty())
                {
                    questions.AddRange(act.GetActiveActivityForm().GetActiveQuestions());
                }
            }

            if (activityGroup.IsNotEmpty())
            {
                questions.AddRange(activityGroup.GetForm().GetActiveQuestions());
            }

            finalquestions.AddRange(questions.Where(x => x.FormQuestionGroupRegistrantTypeId == RegistrantType.Individual && x.QuestionDataTypeId != QuestionDataType.NotSet).DistinctBy(x => x.QuestionDataTypeId));
            finalquestions.AddRange(questions.Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet));
            questions = finalquestions;

            return questions;
        }

        private Dictionary<int, string> ValidateForm(List<FormQuestion> questions, RegistrantType registrantTypeId, decimal? costPerRegistrant, bool acceptCreditCard, SystemMessages msgs, bool checkRequiredFields = true)
        {
            if(msgs.IsNotEmpty()) msgs = new SystemMessages();

            var alreadyCheckedIds = new List<int>();
            var questionAnswers = new Dictionary<int, string>();

            foreach (var question in questions)
            {
                if (question.FormQuestionGroupRegistrantTypeId == registrantTypeId)
                {
                    if (!alreadyCheckedIds.Contains(question.ID.Value))
                    {
                        var answerText = Request.Params["question-" + question.ID.ToString()];
                        alreadyCheckedIds.Add(question.ID.Value);

                        if (question.QuestionDataTypeId != QuestionDataType.NotSet && answerText.IsEmpty())
                        {
                            var similarQuestions = questions.Where(x => x.QuestionDataTypeId == question.QuestionDataTypeId);
                            foreach (var similarQuestion in similarQuestions)
                            {
                                answerText = Request.Params["question-" + question.ID.ToString()];
                                alreadyCheckedIds.Add(similarQuestion.ID.Value);
                                if (answerText.IsNotEmpty())
                                {
                                    break;
                                }
                            }
                        }

                        if (question.IsRequired && answerText.IsEmpty())
                        {
                            msgs.Add(question.Text + " is required", MessageType.UserError);
                        }
                        else if (question.QuestionDataTypeId == QuestionDataType.RegistrantEmail && !answerText.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                        {
                            msgs.Add(question.Text + " must be a valid email", MessageType.UserError);
                        }

                        questionAnswers.Add(question.ID.Value, answerText);
                    }
                }
            }

            if (costPerRegistrant > 0 && acceptCreditCard && Request.Params["paymenttype"] == PaymentType.CreditCard.ToString())
            {
                if (Request.Params["ccname"].IsEmpty())
                {
                    msgs.Add("Name on Card is required", MessageType.UserError);
                }
                if (Request.Params["ccnumber"].IsEmpty())
                {
                    msgs.Add("Card Number is required", MessageType.UserError);
                }
                if (Request.Params["cccvc"].IsEmpty())
                {
                    msgs.Add("CVC is required", MessageType.UserError);
                }
                if (Request.Params["ccexpmonth"].IsEmpty())
                {
                    msgs.Add("Expiration Month is required", MessageType.UserError);
                }
                if (Request.Params["ccexpyear"].IsEmpty())
                {
                    msgs.Add("Expiration Year on Card is required", MessageType.UserError);
                }
            }

            if (msgs.HasErrors())
            {
                var tempmsgs = new SystemMessages();

                if (msgs.HasMessageContainingText("required"))
                {
                    msgs.Add("Please fill in all required fields", MessageType.Error);
                }

                foreach (SystemMessage msg in msgs)
                {
                    if (!msg.Text.ToLower().Contains("required"))
                    {
                        tempmsgs.Add(msg);
                    }
                }

                msgs = tempmsgs;
            }

            return questionAnswers;
        }

        private SystemMessages AddAnswers(List<FormQuestion> questions, Dictionary<int, string> questionAnswers, List<FormQuestionAnswer> existingAnswers, Registrant registrant, Registration registration, BasicLibrary.DataAccess.DatabaseConnection dc)
        {
            var msgs = new SystemMessages();

            foreach (var keypair in questionAnswers)
            {
                var answerText = keypair.Value;
                var question = questions.Where(x => x.ID.Value == keypair.Key).First();
                if (existingAnswers.Count > 0)
                {
                    var answer = existingAnswers.Where(x => x.FormQuestionId == question.ID).FirstOrDefault();
                    if (answer.IsNotEmpty())
                    {
                        msgs.AddRange(answer.UpdateAnswer(
                            answerText.IfEmpty("").Trim(),
                            registrant,
                            SessionVariables.LoggedInUser?.ID,
                            dc));
                    }
                }
                else
                {
                    registration.AddAnswer(answerText.IfEmpty("").Trim(), question, registrant, 0, msgs, dc: dc);
                }
            }

            return msgs;
        }

        private SystemMessages RemoveActivities(Registration registration, List<CartItem> items, BasicLibrary.DataAccess.DatabaseConnection dc)
        {
            var msgs = new SystemMessages();
            //do clean up if removing activities from an existing registration
            //NEED TO READJUST SEAT INDEXES
            
            var registrantactivities = registration.GetRegistrantActivities(dc);
            var rasToDelete = registrantactivities.Where(x => items.Select(y => y.ActivityId).Contains(x.ActivityId.Value) == false);
            foreach (var ra in rasToDelete)
            {
                msgs.AddRange(ra.CancelRegistration(dc));
            }
            return msgs;
        }

        /*
        private int GetMaxChoiceCount(List<CartItem> items)
        {
            var choiceMaxCount = 0;
            var days = Enum.GetValues(typeof(DayOfWeekASL)).Cast<DayOfWeekASL>().Where(x => x != DayOfWeekASL.NotSet);
            foreach (var day in days)
            {
                if (items.Where(x => x.Day == day).Count() > choiceMaxCount)
                {
                    choiceMaxCount = items.Where(x => x.Day == day).Count();
                }
            }
            return choiceMaxCount;
        }
        */

        private int GetChoiceNumber(Activity act, List<CartItem> items, CartItem item)
        {
            var choiceNumber = 1;
            if (items.Count() > 1)
            {
                for (var i = 1; i <= items.Count(); i++)
                {
                    var postedChoice = Request["preference-" + i.ToString()];
                    if (postedChoice == act.ID.ToString())
                    {
                        choiceNumber = i;
                    }
                }
            }
            return choiceNumber;
        }

        private void LoadSingleActivityViewBag(Account account, Activity activity, SystemMessages msgs, bool preview)
        {
            ViewBag.Account = account;
            ViewBag.Activity = activity;
            ViewBag.RegistrationPeriod = activity.GetActiveRegistrationPeriod();
            ViewBag.Messages = new SystemMessages();
            ViewBag.Preview = preview;
            ViewBag.FieldValues = Request.Params;
            ViewBag.AdminMode = SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.LoggedInUser.HasAccessTo(account);
            LoadQuestions();
        }
    }
}