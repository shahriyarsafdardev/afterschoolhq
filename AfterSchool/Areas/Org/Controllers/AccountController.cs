using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Services;
using Hangfire;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.Org.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        public AccountController() : base(new List<AccountType> { AccountType.School, AccountType.ActivityProvider })
        {

        }

        private bool GetIsFullScreen()
        {
            return Request["isnew"] == "1";
        }

        [HttpGet]
        public ActionResult GetStartedPayments(string id)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = true;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string email, string phone, string uniquename, string isactive, string timezone)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;                                

            if (name.IsEmpty())
                msgs.Add("Organization Name is required", MessageType.UserError);

            a.Name = name;
            a.Phone = phone;
            a.UniqueName = uniquename;
            a.TimeZone = timezone;
            a.Email = email;

            if (msgs.HasNoErrors())
            {
                if (a.UniqueName.IsEmpty() && uniquename.IsEmpty())
                    a.UniqueName = AccountService.GenerateUniqueName(name, a.ID);
                else
                    a.UniqueName = AccountService.GenerateUniqueName(uniquename, a.ID);

                a.LastModifiedDate = DateTime.UtcNow;
                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                msgs.AddRange(a.SaveWithStripe());

                if (msgs.HasNoErrors())
                    msgs.Add("Saved Successfully", MessageType.Success);
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(a);
        }

        [HttpGet]
        public ActionResult Location()
        {
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Messages = new SystemMessages();
            ViewBag.Location = SessionVariables.CurrentAccount.GetDefaultLocation();
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult Location(string id, string location, string locationname, string address1, string address2, string city, string state, string postalcode, string country, string mylocation, string locationtype)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;

            var locationType = LocationType.NotSet;
            var hostAccountId = new int?();

            if (locationtype.IsNumber())
                locationType = (LocationType)locationtype.ToInteger();

            if (mylocation.IsBoolean() && mylocation.ToBoolean())
                hostAccountId = SessionVariables.CurrentAccount.ID;

            if (location.IsNumber())
            {
                var currentLocation = AfterSchoolLibrary.Models.Location.Load(location.ToInteger(), msgs);

                if (currentLocation.IsOwnedByOrIsNotOwnedAtAll(SessionVariables.CurrentAccount))
                {
                    currentLocation.LocationTypeId = locationType;
                    currentLocation.HostAccountId = hostAccountId;
                    currentLocation.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                    currentLocation.LastModifiedDate = DateTime.UtcNow;
                    msgs.AddRange(currentLocation.Save());
                }

                SessionVariables.CurrentAccount.SetDefaultLocation(location.ToInteger(), SessionVariables.LoggedInUser.ID.Value);
            }
            else
            {
                var newLocation = LocationService.CreateNewWithValidation(
                    locationName: locationname,
                    address1: address1,
                    address2: address2,
                    city: city,
                    state: state,
                    postalCode: postalcode,
                    country: "US",
                    locationType: locationType,
                    hostAccountId: hostAccountId,
                    visitorAccount: SessionVariables.CurrentAccount,
                    loggedInUser: SessionVariables.LoggedInUser,
                    msgs: msgs
                    );

                if (msgs.HasNoErrors())
                    SessionVariables.CurrentAccount.SetDefaultLocation(newLocation.ID.Value, SessionVariables.LoggedInUser.ID.Value);
            }            

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Location = SessionVariables.CurrentAccount.GetDefaultLocation();
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(a);
        }     

        [HttpGet]
        public ActionResult PaymentsOptions()
        {
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View();
        }

        [HttpGet]
        public ActionResult CheckSetup()
        {
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetCheckPaymentAccount() ?? new CheckPaymentAccount());
        }

        [HttpPost]
        public ActionResult CheckSetup(string entitytype, string name, string address1, string address2, string city, string state, string postalcode)
        {
            var msgs = new SystemMessages();

            if (!entitytype.IsNumber())
                msgs.Add("Select whether we're sending payments to an Individual or a Company", MessageType.UserError);

            if (name.IsEmpty())
                msgs.Add("Payee Name is required", MessageType.UserError);

            if (address1.IsEmpty())
                msgs.Add("Address 1 is required", MessageType.UserError);

            if (city.IsEmpty())
                msgs.Add("City is required", MessageType.UserError);

            if (state.IsEmpty())
                msgs.Add("State is required", MessageType.UserError);

            if (postalcode.IsEmpty())
                msgs.Add("Postal Code is required", MessageType.UserError);

            if (msgs.HasNoErrors())
                SessionVariables.CurrentAccount.CreateOrUpdateCheckPaymentAccount(
                    entitytype.ToEnum<LegalEntityType>(),
                    name,
                    address1,
                    address2,
                    city,
                    state,
                    postalcode,
                    SessionVariables.LoggedInUser,
                    msgs);

            if (Request["isnew"] == "1")
                return RedirectToAction("paymentsdone", new { isnew = 1 });

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            ViewBag.Messages = msgs;
            return View(SessionVariables.CurrentAccount.GetCheckPaymentAccount() ?? new CheckPaymentAccount());
        }
        
        [HttpGet]
        public ActionResult PaymentsEntity()
        {
            var msgs = new SystemMessages();
            var stripeAccount = SessionVariables.CurrentAccount.GetOrCreateStripeAccount(SessionVariables.LoggedInUser, msgs);

            if (stripeAccount.TOSAcceptanceDate.HasValue)
                stripeAccount.GetLatestStripeAccountDetails(SessionVariables.CurrentAccount, msgs);

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(stripeAccount);
        }

        [HttpPost]
        public ActionResult PaymentsEntity(string entityType, string firstName, string lastName, string address1, string address2, string city, string state, string postalCode, string dateOfBirth, string ssn, string ssnLast4, string legalbusinessname, string businessUrl, string supportphone, string supportemail, string businessPhone, string productDescription, string ein, string tosagree)
        {
            var msgs = new SystemMessages();
            var stripeAccount = SessionVariables.CurrentAccount.GetOrCreateStripeAccount(SessionVariables.LoggedInUser, msgs);
            var startingStatus = stripeAccount.VerificationStatusId;
            var entityTypeId = (LegalEntityType)entityType.ToInteger();

            if (entityTypeId == LegalEntityType.Company)
            {
                if (legalbusinessname.IsEmpty())
                    msgs.Add("Legal Business Name is required", MessageType.UserError);
                if (ein.IsEmpty())
                    msgs.Add("Employer Identification Number is required", MessageType.UserError);
            }
            else
            {
                if (firstName.IsEmpty())
                    msgs.Add("First Name is required", MessageType.UserError);
                if (lastName.IsEmpty())
                    msgs.Add("Last Name is required", MessageType.UserError);
                if (dateOfBirth.IsEmpty() || !dateOfBirth.IsDate())
                    msgs.Add("Date of Birth is required and must be a valid date", MessageType.UserError);
                if (ssnLast4.IsEmpty() || !ssnLast4.IsNumber())
                    msgs.Add("SSN(last 4) is required", MessageType.UserError);
            }

            if (supportemail.IsEmpty())
                msgs.Add("Support Email is required", MessageType.UserError);
            if (supportphone.IsEmpty())
                msgs.Add("Support Phone is required", MessageType.UserError);
            if (address1.IsEmpty())
                msgs.Add("Address 1 is required", MessageType.UserError);
            if (city.IsEmpty())
                msgs.Add("City is required", MessageType.UserError);
            if (state.IsEmpty())
                msgs.Add("State is required", MessageType.UserError);
            if (postalCode.IsEmpty())
                msgs.Add("Postal Code is required", MessageType.UserError);

            if (!tosagree.IsBoolean() || !tosagree.ToBoolean())
                msgs.Add("You must agree to the terms of service", MessageType.UserError);

            if (businessUrl.IsEmpty() && productDescription.IsEmpty())
                msgs.Add("You must enter either a website or a Service Description", MessageType.UserError);

            if (stripeAccount.RequiresFullSSN)
            {
                if (ssn.IsEmpty())
                    msgs.Add("SSN is required", MessageType.UserError);

                if (!ssn.LastChars(4).IsSameAs(ssnLast4))
                    msgs.Add("SSN and Last 4 of SSN must match", MessageType.UserError);
            }

            if (stripeAccount.RequiresDocument)
            {
                if (Request.Files.Count == 0 || Request.Files[0].ContentLength == 0 || Request.Files[0].ContentType != "image/jpeg" && Request.Files[0].ContentType != "image/png")
                    msgs.Add("A valid Driver's License or Passport is required as either a jpg or a png", MessageType.UserError);
                else if (ByteSizeLib.ByteSize.FromBytes(Request.Files[0].ContentLength).MegaBytes > 8)
                    msgs.Add("Your document must be less than 8 MBs", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {                
                stripeAccount.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                stripeAccount.LastModifiedDate = DateTime.UtcNow;
                stripeAccount.LegalEntityAddress1 = address1;
                stripeAccount.LegalEntityAddress2 = address2;
                stripeAccount.LegalEntityCity = city;
                stripeAccount.LegalEntityState = state;
                stripeAccount.LegalEntityPostalCode = postalCode;
                stripeAccount.LegalEntityTypeId = entityTypeId;
                stripeAccount.SupportPhone = supportphone;
                stripeAccount.SupportEmail = supportemail;
                stripeAccount.BusinessUrl = businessUrl;
                stripeAccount.BusinessPhone = businessPhone;
                stripeAccount.ProductDescription = productDescription;

                if (entityTypeId == LegalEntityType.Company)
                {
                    stripeAccount.LegalEntityBusinessName = legalbusinessname;
                    stripeAccount.LegalEntityEIN = ein;
                }
                else
                {
                    stripeAccount.LegalEntityDOB = dateOfBirth.ToDateTime();
                    stripeAccount.LegalEntityFirstName = firstName;
                    stripeAccount.LegalEntityLastName = lastName;
                    stripeAccount.LegalEntitySSNLast4 = ssnLast4;
                }

                if (tosagree.IsBoolean())
                {
                    stripeAccount.TOSAcceptanceDate = DateTime.UtcNow;
                    stripeAccount.TOSAcceptanceIP = Request.UserHostAddress;
                    stripeAccount.TOSAcceptanceUserAgent = Request.UserAgent.MaxChars(400);
                }

                StripeUploadFile uploadFile = null;

                if (stripeAccount.RequiresDocument)
                    uploadFile = new StripeUploadFile(Request.Files[0].InputStream, Request.Files[0].FileName, AfterSchoolLibrary.Billing.StripeFilePurpose.IdentityDocument);

                stripeAccount.SaveAndSendLegalEntityToStripe(ssn, uploadFile, msgs);
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add("Your update information has been submitted.", MessageType.Success);

                if (startingStatus != stripeAccount.VerificationStatusId && stripeAccount.VerificationStatusId == StripeVerificationStatus.Verified)
                    BackgroundJob.Enqueue(() => SessionVariables.CurrentAccount.SendStripeAccountVerified(null));

                if (stripeAccount.LegalEntityTypeId == LegalEntityType.Company && stripeAccount.ChargesEnabled)
                    return RedirectToAction("paymentsbank", "account", new { isnew = Request["isnew"] });
            }
           

            ViewBag.Messages = msgs;
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(stripeAccount);
        }

        [HttpGet]
        public ActionResult PaymentsBank()
        {
            var msgs = new SystemMessages();
            var stripeAccount = SessionVariables.CurrentAccount.GetOrCreateStripeAccount(SessionVariables.LoggedInUser, msgs);
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(stripeAccount);
        }

        [HttpPost]
        public ActionResult PaymentsBank(string routingnumber1, string routingnumber2, string accountnumber1, string accountnumber2)
        {
            var msgs = new SystemMessages();
            var stripeAccount = SessionVariables.CurrentAccount.GetOrCreateStripeAccount(SessionVariables.LoggedInUser, msgs);

            if (routingnumber1.IsEmpty())
                msgs.Add("Routing Number is required", MessageType.UserError);
            if (routingnumber2.IsEmpty())
                msgs.Add("Confirm Routing Number is required", MessageType.UserError);
            if (routingnumber1 != routingnumber2)
                msgs.Add("Routing Numbers must match", MessageType.UserError);

            if (accountnumber1.IsEmpty())
                msgs.Add("Account Number is required", MessageType.UserError);
            if (accountnumber2.IsEmpty())
                msgs.Add("Confirm Account Number is required", MessageType.UserError);
            if (accountnumber1 != accountnumber2)
                msgs.Add("Account Numbers must match", MessageType.UserError);

            if (msgs.HasNoErrors())
                stripeAccount.SaveAndSendBankInfoToStripe(routingnumber1, accountnumber1, msgs);

            if (msgs.HasNoErrors() && Request["isnew"] == "1")
                return RedirectToAction("PaymentsDone");

            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(stripeAccount);
        }

        [HttpGet]
        public ActionResult PaymentsDone(string id)
        {
            var msgs = new SystemMessages();
            var stripeAccount = SessionVariables.CurrentAccount.GetOrCreateStripeAccount(SessionVariables.LoggedInUser, msgs);

            ViewBag.HasActivities = SessionVariables.CurrentAccount.HasActivities(msgs);
            ViewBag.Account = SessionVariables.CurrentAccount;
            ViewBag.Messages = msgs;
            ViewBag.IsFullScreen = GetIsFullScreen();
            return View(stripeAccount);
        }

        [HttpGet]
        public ActionResult Features()
        {
            Account a = SessionVariables.CurrentAccount;
            ViewBag.Messages = new SystemMessages();
            ViewBag.Features = a.GetAccountPackageFeatures();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(a);
        }

        [HttpPost]
        public ActionResult Features(string x)
        {
            var msgs = new SystemMessages();
            var a = SessionVariables.CurrentAccount;
            var accountPackage = a.GetActiveAccountPackage();

            Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) => {
                    foreach (FeatureType type in System.Enum.GetValues(typeof(FeatureType)))
                    {
                        if (Request[type.ToString()].IsNotEmpty())
                        {
                            var accountFeature = AccountPackageFeature.LoadByAccountPackageIdByFeatureTypeId(accountPackage.ID.Value, type, dc: adc);
                            if (accountFeature.IsEmpty())
                            {
                                accountFeature = new AccountPackageFeature();
                                accountFeature.AccountPackageId = accountPackage.ID;
                                accountFeature.CreateDate = DateTime.UtcNow;
                                accountFeature.CreatedBy = SessionVariables.LoggedInUser.ID;
                                accountFeature.FeatureTypeId = type;
                            }

                            accountFeature.Max = Request[type.ToString()].ToDecimal();
                            accountFeature.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                            accountFeature.LastModifiedDate = DateTime.UtcNow;
                            msgs.AddRange(accountFeature.Save(adc));

                            if (accountFeature.FeatureTypeId == FeatureType.TrialDays && accountPackage.TrialStartDate.HasValue)
                                amsgs.AddRange(accountPackage.UpdateTrialDays(accountFeature.Max.Value.ToInteger(), adc));
                        }
                    }
                },
                onSuccess: (SystemMessages smsgs) => {
                    msgs.AddRange(smsgs);
                    msgs.Add("Saved Successfully", MessageType.Success);
                    SessionVariables.AccountPackage = a.GetActiveAccountPackage();
                },
                onFailure: (SystemMessages fmsgs) => {
                    msgs.AddRange(fmsgs);
                }
            );            

            ViewBag.Messages = msgs;
            ViewBag.Features = a.GetAccountPackageFeatures();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(a);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            var members = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult TeamInvite()
        {
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            return View(SessionVariables.CurrentAccount);
        }

        [HttpPost]
        public ActionResult TeamInvite(string usertype, string emails, string message)
        {
            SystemMessages msgs = new SystemMessages();

            if (usertype.IsEmpty())
                msgs.Add("Account type is required", MessageType.UserError);

            if (emails.IsEmpty())
                msgs.Add("Please enter at least one email address", MessageType.UserError);
            else
                emails.Split(',')
                    .Select(x => x.Trim())
                    .Where(email => !email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                    .ToList()
                    .ForEach(email => msgs.Add(string.Format("{0} is not a valid email address", email), MessageType.UserError));

            if (msgs.HasNoErrors())
                emails.Split(',')
                    .Select(x => x.Trim())
                    .ToList()
                    .ForEach(email => 
                        SessionVariables.CurrentAccount.InviteUser(
                            "", 
                            "", 
                            email, 
                            "",
                            Request.Params["message"], 
                            (UserType)usertype.ToInteger(), 
                            SessionVariables.LoggedInUser,
                            msgs)
                     );

            if (msgs.HasNoErrors())
                msgs.Add("All invites have been sent", MessageType.Success);

            var members = SessionVariables.CurrentAccount.GetMembers().Select(x => x.ToWebUser()).ToList();
            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = SessionVariables.AccountPackage;
            ViewBag.Members = members;
            ViewBag.NeedToUpgrade = members.Count >= SessionVariables.AccountPackage.GetFeature(FeatureType.TeamAccounts).GetMaxValue();
            return View("Team", SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult Visitors()
        {
            var msgs = new SystemMessages();

            ViewBag.RequestingVisitors = SessionVariables.CurrentAccount.GetRequestedVisitorAccounts(null, msgs);
            ViewBag.ApprovedVisitors = SessionVariables.CurrentAccount.GetApprovedVisitorAccounts(msgs);
            ViewBag.Messages = msgs;

            return View(SessionVariables.CurrentAccount);
        }

        [HttpGet]
        public ActionResult ApproveVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.Load(id.ToInteger(), msgs);
            msgs.AddRange(accountRelationship.UpdateApprovalStatus(ApprovalStatus.Approved, SessionVariables.LoggedInUser));

            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult DenyVisitor(string id)
        {
            var msgs = new SystemMessages();
            var accountRelationship = AccountRelationship.Load(id.ToInteger(), msgs);
            msgs.AddRange(accountRelationship.UpdateApprovalStatus(ApprovalStatus.Denied, SessionVariables.LoggedInUser));

            return RedirectToAction("visitors");
        }

        [HttpGet]
        public ActionResult Forms()
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetForms());
        }

        [HttpGet]
        public ActionResult FormEdit(string id, string qg)
        {
            var form = Form.Load(id.ToInteger());
            var questionGroups = form.GetQuestionGroups();
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder).ToList();
            var organizationGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Organization)
                .OrderBy(x => x.DisplayOrder).ToList();

            //temporarily disable organization groups
            organizationGroups = new List<FormQuestionGroup>();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);
            questionGroups.AddRange(organizationGroups);

            FormQuestionGroup questionGroup = null;

            if (qg.IsNumber())
            {
                if (qg.ToInteger() == 0)
                    questionGroup = new FormQuestionGroup()
                    {
                        QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                        FormId = form.ID,
                        RegistrantTypeId = RegistrantType.Individual,
                        ID = 0
                    };
                else
                    questionGroup = FormQuestionGroup.Load(qg.ToInteger());
            }
            else
            {
                questionGroup = null;
            }

            ViewBag.Messages = new SystemMessages();
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }

        [HttpPost]
        public ActionResult FormEdit(string id, string copyfrom, string qg, string name, string description, string cmd)
        {
            var msgs = new SystemMessages();
            Form form = null;
            FormQuestionGroup questionGroup = null;

            if (name.IsEmpty() && qg.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);

            if (description.IsEmpty() && qg.IsBlank() && false)
                msgs.Add("Description is required", MessageType.UserError);


            if (id.ToInteger() == 0)
            {
                if (copyfrom.IsNumber())
                    form = Form.Load(copyfrom.ToInteger(), msgs)
                        .Copy(SessionVariables.LoggedInUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, null, msgs);
                else
                    form = SessionVariables.CurrentAccount
                        .GetDefaultForm(msgs)
                        .Copy(SessionVariables.LoggedInUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, null, msgs);
            }
            else
            {
                form = Form.Load(id.ToInteger(), msgs);
            }


            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (name.IsNotBlank()) form.Name = name;
                        if (description.IsNotBlank()) form.Description = description;

                        amsgs.AddRange(form.Save(adc));

                        var questions = new List<FormQuestion>();

                        if (qg.IsNumber())
                        {
                            // create or update question groups
                            if (qg.ToInteger() == 0)
                            {
                                questionGroup = new FormQuestionGroup()
                                {
                                    CreateDate = DateTime.UtcNow,
                                    CreatedBy = SessionVariables.LoggedInUser.ID,
                                    LastModifiedDate = DateTime.UtcNow,
                                    LastModifiedBy = SessionVariables.LoggedInUser.ID,
                                    DisplayOrder = form.GetQuestionGroupCount(amsgs, adc) + 1,
                                    FormId = form.ID,
                                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,                                   
                                    RegistrantTypeId = RegistrantType.Individual,
                                    Name = Request.Params["name-0"]
                                };
                                amsgs.AddRange(questionGroup.Save(adc));
                                questions = new List<FormQuestion>();
                            }
                            else
                            {
                                questionGroup = FormQuestionGroup.Load(qg.ToInteger(), amsgs, adc);
                                questions = questionGroup.GetAllQuestions(amsgs, adc);

                                if (questionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
                                {
                                    questionGroup.Name = Request.Params[$"name-{questionGroup.ID}"];
                                    questionGroup.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                                    questionGroup.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(questionGroup.Save(adc));
                                }
                            }

                            // save existing questions
                            questions.ForEach(question =>
                            {
                                if (!question.IsLocked)
                                {
                                    question.IsActive = Request.Params[$"active-{question.ID}"].IsBoolean();
                                    question.IsRequired = Request.Params[$"required-{question.ID}"].IsBoolean();

                                    if (question.QuestionDataTypeId == QuestionDataType.NotSet)
                                    {
                                        question.Text = Request.Params[$"text-{question.ID}"];
                                        // for Text Block
                                        question.Description = Request.Params[$"description-{question.ID}"];
                                        question.Options = Request.Params[$"options-text-{question.ID}"];
                                        question.Link = Request.Params[$"link-text-{question.ID}"];

                                        question.QuestionTypeId = (QuestionType)Request.Params[$"questiontype-{question.ID}"].ToInteger();

                                    }

                                    question.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                                    question.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(question.Save(adc));
                                }

                            });

                            // save new questions
                            if (Request.Form["newquestioncount"].IsNumber() && Request.Form["newquestioncount"].ToInteger() > 0)
                            {
                                var questionCount = Request.Form["newquestioncount"].ToInteger();

                                for (var i = 0; i < questionCount; i++)
                                {
                                    var question = new FormQuestion
                                    {
                                        FormId = form.ID,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                        CreatedBy = SessionVariables.LoggedInUser.ID,
                                        LastModifiedBy = SessionVariables.LoggedInUser.ID,
                                        DisplayOrder = questionGroup.GetQuestionCount(amsgs, adc) + 1,
                                        FormQuestionGroupId = questionGroup.ID,
                                        IsTemplate = false,
                                        QuestionDataTypeId = QuestionDataType.NotSet,
                                        QuestionTypeId = (QuestionType)Request.Params[string.Format("new-questiontype-{0}", i)].ToInteger(),
                                        Text = Request.Params[string.Format("new-text-{0}", i)],
                                        Options = Request.Params[string.Format("new-options-text-{0}", i)],
                                        Link = Request.Params[string.Format("new-link-text-{0}", i)],
                                        Description = Request.Params[string.Format("new-description-{0}", i)],
                                        IsActive = Request.Params[string.Format("new-active-{0}", i)].IsBoolean(),
                                        IsRequired = Request.Params[string.Format("new-required-{0}", i)].IsBoolean()
                                    };
                                    amsgs.AddRange(question.Save(adc));
                                }
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Form Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            var questionGroups = form.GetQuestionGroups();
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder)
                .ToList();
            var organizationGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Organization)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            //temporarily disable organization groups
            organizationGroups = new List<FormQuestionGroup>();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);
            questionGroups.AddRange(organizationGroups);

            var currentIndex = -1;

            if (questionGroup.IsNotEmpty())
            {
                currentIndex = questionGroups.FindIndex(x => x.ID.Value == questionGroup.ID.Value);
            }

            switch (cmd)
            {
                case "prev":
                    if (currentIndex > 0) questionGroup = questionGroups[currentIndex - 1];
                    break;
                case "next":
                    if (currentIndex < questionGroups.Count - 1) questionGroup = questionGroups[currentIndex + 1];
                    break;
            }

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }


        //[HttpGet]
        //public ActionResult Style()
        //{
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = new SystemMessages();
        //    ViewBag.LandingPage = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.LandingPageText);
        //    ViewBag.RegThankYou = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.RegistrationThankYouEmail);
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult Style(string id, string logourl, string landingpagetext, string regthankyoutext)
        //{
        //    var msgs = new SystemMessages();
        //    var a = SessionVariables.CurrentAccount;

        //    a.LogoUrl = logourl;

        //    var landingPage = a.GetAccountProperty(AccountPropertyType.LandingPageText);
        //    var thankyou = a.GetAccountProperty(AccountPropertyType.RegistrationThankYouEmail);

        //    if (landingPage.IsEmpty())
        //    {
        //        landingPage = new AccountProperty() { Value = landingpagetext };
        //        landingPage.AccountPropertyTypeId = AccountPropertyType.LandingPageText;
        //        landingPage.CreateDate = DateTime.UtcNow;
        //        landingPage.CreatedBy = SessionVariables.LoggedInUser.ID;
        //        landingPage.AccountId = a.ID;
        //    }
        //    else
        //    {
        //        landingPage.Value = landingpagetext;
        //    }

        //    if (thankyou.IsEmpty())
        //    {
        //        thankyou = new AccountProperty() { Value = regthankyoutext };
        //        thankyou.AccountPropertyTypeId = AccountPropertyType.RegistrationThankYouEmail;
        //        thankyou.CreateDate = DateTime.UtcNow;
        //        thankyou.CreatedBy = SessionVariables.LoggedInUser.ID;
        //        thankyou.AccountId = a.ID;
        //    }
        //    else
        //    {
        //        thankyou.Value = regthankyoutext;
        //    }

        //    if (msgs.HasNoErrors())
        //    {
        //        Database.ExecuteTransaction(
        //            action: (SystemMessages amsgs, DatabaseConnection adc) => {
        //                a.LastModifiedDate = DateTime.UtcNow;
        //                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                amsgs.AddRange(a.Save(adc));

        //                if (amsgs.HasNoErrors())
        //                {
        //                    landingPage.LastModifiedDate = DateTime.UtcNow;
        //                    landingPage.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                    amsgs.AddRange(landingPage.Save(adc));
        //                }

        //                if (amsgs.HasNoErrors())
        //                {
        //                    thankyou.LastModifiedDate = DateTime.UtcNow;
        //                    thankyou.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                    amsgs.AddRange(thankyou.Save(adc));
        //                }
        //            },
        //            onSuccess: (SystemMessages smsgs) => {
        //                msgs.AddRange(smsgs);
        //                msgs.Add("Saved Successfully", MessageType.Success);
        //            },
        //            onFailure: (SystemMessages fmsgs) => {
        //                msgs.AddRange(fmsgs);
        //            }
        //        );

        //    }

        //    ViewBag.Messages = msgs;
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.LandingPage = landingPage;
        //    ViewBag.RegThankYou = thankyou;
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult Integrations()
        //{
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = new SystemMessages();
        //    ViewBag.FacebookAdPixel = SessionVariables.CurrentAccount.GetAccountProperty(AccountPropertyType.FacebookAdPixel);
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[ValidateInput(false)]
        //[HttpPost]
        //public ActionResult Integrations(string id, string facebookadpixelscript)
        //{
        //    var msgs = new SystemMessages();
        //    var a = SessionVariables.CurrentAccount;

        //    var facebookAdPixel = a.GetAccountProperty(AccountPropertyType.FacebookAdPixel);

        //    if (facebookAdPixel.IsEmpty())
        //    {
        //        facebookAdPixel = new AccountProperty() { Value = facebookadpixelscript };
        //        facebookAdPixel.AccountPropertyTypeId = AccountPropertyType.FacebookAdPixel;
        //        facebookAdPixel.CreateDate = DateTime.UtcNow;
        //        facebookAdPixel.CreatedBy = SessionVariables.LoggedInUser.ID;
        //        facebookAdPixel.AccountId = a.ID;
        //    }
        //    else
        //    {
        //        facebookAdPixel.Value = facebookadpixelscript;
        //    }

        //    if (msgs.HasNoErrors())
        //    {
        //        Database.ExecuteTransaction(
        //            action: (SystemMessages amsgs, DatabaseConnection adc) => {

        //                a.LastModifiedDate = DateTime.UtcNow;
        //                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                amsgs.AddRange(a.Save(adc));

        //                if (amsgs.HasNoErrors())
        //                {
        //                    facebookAdPixel.LastModifiedDate = DateTime.UtcNow;
        //                    facebookAdPixel.LastModifiedBy = SessionVariables.LoggedInUser.ID;
        //                    amsgs.AddRange(facebookAdPixel.Save(adc));
        //                }
        //            },
        //            onSuccess: (SystemMessages smsgs) => {
        //                msgs.AddRange(smsgs);
        //                msgs.Add("Saved Successfully", MessageType.Success);
        //            },
        //            onFailure: (SystemMessages fmsgs) => {
        //                msgs.AddRange(fmsgs);
        //            }
        //        );

        //    }

        //    ViewBag.Messages = msgs;
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.FacebookAdPixel = facebookAdPixel;
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult Detail()
        //{
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = new SystemMessages();
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[HttpGet]
        //public ActionResult Billing()
        //{
        //    Account a = SessionVariables.CurrentAccount;
        //    ViewBag.BillingHistory = a.GetStripeBillingHistory();
        //    ViewBag.RefundHistory = a.GetStripeRefundHistory();
        //    // ViewBag.Cards = a.GetStripeCards();
        //    ViewBag.Messages = new SystemMessages();
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult NewBilling(string id)
        //{
        //    var msgs = new SystemMessages();

        //    if (SessionVariables.AccountPackage.IsEmpty())
        //    {
        //        var accountPackages = SessionVariables.CurrentAccount.GetAccountPackages(msgs).Where(ap => ap.IsActive);

        //        if (!accountPackages.Any())
        //        {
        //            var defaultPackage = Package.Load(msgs: msgs).First();
        //            SessionVariables.AccountPackage = SessionVariables.CurrentAccount.AddPackage(defaultPackage.ID.Value, BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);
        //        }
        //        else
        //        {
        //            SessionVariables.AccountPackage = accountPackages.Where(ap => ap.IsActive).First();
        //        }

        //        return LoginRedirect();
        //    }

        //    var package = id.IsGuid() ? Package.LoadByUid(id.ToGuid()) : SessionVariables.AccountPackage?.ToPackage();

        //    ViewBag.Package = package;
        //    ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
        //    ViewBag.Messages = new SystemMessages();
        //    return View(SessionVariables.CurrentAccount);
        //}

        //[HttpPost]
        //public ActionResult NewPayment(string selectedpackage, string stripetoken)
        //{
        //    var msgs = new SystemMessages();

        //    Package package = null;
        //    var a = SessionVariables.CurrentAccount;
        //    var isFromTrialAccount = SessionVariables.AccountPackage?.IsDuringTrial ?? false;

        //    Database.ExecuteTransaction(
        //        action: (SystemMessages amsgs, DatabaseConnection adc) => {

        //            var selectedPackageArr = selectedpackage.Split('_');
        //            var billingPeriod = (BillingPeriodType)selectedPackageArr[0].ToInteger();

        //            package = Package.LoadByUid(selectedPackageArr[1].ToGuid(), amsgs, adc);

        //            // amsgs.AddRange(a.AddCreditCard(stripetoken, true, SessionVariables.LoggedInUser, adc));

        //            if (amsgs.HasNoErrors())
        //                a.AddPackage(package.ID.Value, billingPeriod, SessionVariables.LoggedInUser, amsgs, adc);

        //            SessionVariables.AccountPackage = a.GetActiveAccountPackage(amsgs, adc);
        //        },
        //        onSuccess: (SystemMessages smsgs) => {
        //            msgs.AddRange(smsgs);

        //            msgs.Clear(MessageType.Success);
        //            msgs.Add(string.Format("The '{0} Package' has been added to your account", package.Name), MessageType.Success);

        //            if (isFromTrialAccount)
        //                SessionVariables.CurrentAccount.SendWelcomeEmailPurchase();
        //        },
        //        onFailure: (SystemMessages fmsgs) => {
        //            msgs.AddRange(fmsgs);
        //        }
        //    );

        //    ViewBag.Messages = msgs;

        //    if (msgs.HasNoErrors())
        //    {
        //        ViewBag.BillingHistory = a.GetStripeBillingHistory();
        //        ViewBag.RefundHistory = a.GetStripeRefundHistory();
        //        // ViewBag.Cards = a.GetStripeCards();
        //        return View("Billing", a);
        //    }
        //    else
        //    {
        //        ViewBag.Package = package;
        //        ViewBag.Packages = Package.LoadByIsActiveByIsAfterSchool(true, SessionVariables.IsAfterSchool.Value);
        //        return View("NewBilling", a);
        //    }
        //}

        //[HttpPost]
        //public ActionResult AddCard(string id, string stripetoken, string makeprimary)
        //{
        //    SystemMessages msgs = new SystemMessages();

        //    var a = SessionVariables.CurrentAccount;
        //    //msgs.AddRange(a.AddCreditCard(stripetoken, !makeprimary.IsEmpty(), SessionVariables.LoggedInUser));
        //    //ViewBag.Cards = a.GetStripeCards();
        //    ViewBag.Messages = msgs;
        //    return View("Billing", a);
        //}

        //[HttpGet]
        //public ActionResult Packages()
        //{
        //    Account a = SessionVariables.CurrentAccount;
        //    ViewBag.Packages = Package.LoadByIsActive(true);
        //    ViewBag.AccountPackage = a.GetActiveAccountPackage();
        //    ViewBag.Messages = new SystemMessages();
        //    return View(a);
        //}

        //[HttpGet]
        //public ActionResult SelectPackage(string id)
        //{
        //    SystemMessages msgs = new SystemMessages();

        //    Account a = SessionVariables.CurrentAccount;
        //    a.AddPackage(id.ToInteger(), BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);

        //    SessionVariables.AccountPackage = a.GetActiveAccountPackage();

        //    ViewBag.Packages = Package.LoadByIsActive(true);
        //    ViewBag.AccountPackage = SessionVariables.AccountPackage;
        //    ViewBag.Messages = msgs;
        //    return View("Packages", a);
        //}

        //public ActionResult OAuthStripe()
        //{
        //    var msgs = new SystemMessages();

        //    if (Request["error"].IsEmpty())
        //    {
        //        try
        //        {
        //            string scope = Request["scope"];
        //            string code = Request["code"];
        //            string state = Request["state"];

        //            RestClient rc = new RestClient("https://connect.stripe.com/oauth/token");
        //            rc.RequestMethod = RestClientRequestMethod.POST;
        //            rc.Parameters.Add("client_secret", AfterSchoolLibrary.Billing.StripeHelper.SecretKey);
        //            rc.Parameters.Add("code", code);
        //            rc.Parameters.Add("grant_type", "authorization_code");
        //            rc.Parameters.Add("client_id", AfterSchoolLibrary.Billing.StripeHelper.PlatformClientId);
        //            rc.ContentType = RestClientContentTypeEnum.URLEncoded;
        //            string responseFromServer = rc.DoRequest();

        //            dynamic results = responseFromServer.FromJSON();

        //            string token_type = results["token_type"];
        //            string stripe_publishable_key = results["stripe_publishable_key"];
        //            bool livemode = results["livemode"];
        //            string stripe_user_id = results["stripe_user_id"];
        //            string refresh_token = results["refresh_token"];
        //            string access_token = results["access_token"];
        //            scope = results["scope"];

        //            SessionVariables.CurrentAccount.StripePublishableKey = stripe_publishable_key;
        //            SessionVariables.CurrentAccount.StripeAccessToken = access_token;
        //            SessionVariables.CurrentAccount.StripeRefreshToken = refresh_token;
        //            SessionVariables.CurrentAccount.StripeUserId = stripe_user_id;
        //            msgs.AddRange(SessionVariables.CurrentAccount.Save());

        //            if(msgs.HasNoErrors())
        //            {
        //                msgs.Add("Your account has been connected to Stripe", MessageType.Success);
        //            }
        //            else
        //            {
        //                SessionVariables.CurrentAccount.StripePublishableKey = "";
        //                SessionVariables.CurrentAccount.StripeAccessToken = "";
        //                SessionVariables.CurrentAccount.StripeRefreshToken = "";
        //                SessionVariables.CurrentAccount.StripeUserId = "";
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            msgs.Add("An error occurred while connecting your Stripe account.", MessageType.Error, ex);
        //        }
        //    }
        //    else
        //    {
        //        msgs.Add("It appears you have canceled your request to connect your Stripe account.", MessageType.Notice);
        //    }

        //    ViewBag.Messages = msgs;

        //    return View("Payments", SessionVariables.CurrentAccount);
        //}
    }
}