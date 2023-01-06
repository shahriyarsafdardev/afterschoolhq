using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AfterSchool.Models;
using AfterSchoolLibrary.Enums;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Hangfire;

namespace AfterSchool.Areas.Org.Controllers
{
    public class FormController : BaseLoggedInController
    {
        public FormController() : base(FormConstants.OrganizationAccountTypes)
        {
        }

        // GET: Org/Form
        [HttpGet]
        public ActionResult Index(string fromSurvey, string pn, string rpp, string q, string type, bool archived = false)
        {
            if (!FeatureFlag.CheckFlagIsEnabled("forms-reporting"))
                return RedirectToAction("forms", "account", new { fromSurvey, pn, rpp, q, type });

            var msgs = new SystemMessages();

            var pageNumber = 1;
            var recordsPerPage = 10;
            var currentAccount = SessionVariables.CurrentAccount;
            var formType = FormType.NotSet;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            if (type.IsNotEmpty() && type.IsNumber())
                formType = type.ToInteger().ToEnum<FormType>();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var forms = currentAccount.GetForms(archived, formType, q, start, length, msgs);
            var formCount = currentAccount.GetFormCount(archived, formType, q, msgs);
            var totalPages = Math.Ceiling(formCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = formCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };
            ViewBag.Messages = msgs;
            ViewBag.FromSurvey = fromSurvey;
            ViewBag.Archived = archived;
            return View(forms);
        }

        [HttpGet]
        public ActionResult Edit(string id, string qg)
        {
            if (!FeatureFlag.CheckFlagIsEnabled("forms-reporting"))
                return RedirectToAction("formedit", "account", new { id, qg });

            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            if (id.IsEmpty())
                return HttpNotFound();

            var form = currentAccount.GetForm(id.ToInteger(), msgs);
            var questionGroups = form.GetQuestionGroups(msgs);
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder).ToList();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);

            FormQuestionGroup questionGroup;

            if (qg.IsNumber())
            {
                if (qg.ToInteger() == 0)
                    questionGroup = new FormQuestionGroup
                    {
                        QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                        FormId = form.ID,
                        RegistrantTypeId = RegistrantType.Individual,
                        ID = 0,
                    };
                else
                    questionGroup = FormQuestionGroup.Load(qg.ToInteger(), msgs);
            }
            else
                questionGroup = null;

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }

        [HttpPost]
        public ActionResult Edit(string id, string copyfrom, string qg, string name, string description, string cmd,
            string formtype)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (id.IsEmpty() && copyfrom.IsEmpty())
                return HttpNotFound();

            Form form = null;
            FormQuestionGroup questionGroup = null;
            FormType formTypeId = (FormType) formtype.ToInteger();

            if (name.IsEmpty() && qg.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);

            if (id.ToInteger() == 0)
            {
                if (copyfrom.IsNumber())
                {
                    var formToCopy = currentAccount.GetForm(copyfrom.ToInteger(), msgs);

                    if (formToCopy.IsNotEmpty())
                        form = formToCopy.Copy(currentUser.ID.Value, currentAccount.ID.Value, formToCopy.FormTypeId, null, msgs);
                }
                else
                    form = currentAccount.GetOrCreateDefaultForm(formTypeId, currentUser, msgs)
                                            .Copy(currentUser.ID.Value, currentAccount.ID.Value,
                                                formTypeId, null, msgs);
            }
            else
                form = currentAccount.GetForm(id.ToInteger(), msgs);

            var questions = new List<FormQuestion>();
            var questionGroups = new List<FormQuestionGroup>();
            var individualGroups = new List<FormQuestionGroup>();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        if (name.IsNotBlank()) form.Name = name;
                        form.Description = description;
                        if (formtype.IsNotBlank()) form.FormTypeId = (FormType) formtype.ToInteger();

                        amsgs.AddRange(form.Save(adc));

                        if (qg.IsNumber())
                        {
                            // create or update question groups
                            if (qg.ToInteger() == 0)
                            {
                                var sectionName = Request.Params["name-0"];
                                var questionGroupExists = FormQuestionGroup.Exists(
                                    criterias: new Hashtable
                                        {{"Name", sectionName}, {"FormId", form.ID}, {"IsDeleted", false}},
                                    msgs: amsgs,
                                    dc: adc);

                                if (!questionGroupExists)
                                {
                                    questionGroup = new FormQuestionGroup
                                    {
                                        CreateDate = DateTime.UtcNow,
                                        CreatedBy = currentUser.ID,
                                        LastModifiedDate = DateTime.UtcNow,
                                        LastModifiedBy = currentUser.ID,
                                        DisplayOrder = form.GetQuestionGroupCount(amsgs, adc) + 1,
                                        FormId = form.ID,
                                        QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                                        RegistrantTypeId = RegistrantType.Individual,
                                        Name = sectionName,
                                        IsDeleted = false,
                                        IsActive = true,
                                    };
                                    amsgs.AddRange(questionGroup.Save(adc));
                                    questions = new List<FormQuestion>();
                                }
                                else
                                {
                                    questionGroup = FormQuestionGroup
                                        .Load(
                                            criterias: new Hashtable
                                                {{"Name", sectionName}, {"FormId", form.ID}, {"IsDeleted", false}},
                                            msgs: amsgs, dc: adc)?.FirstOrDefault();
                                }
                            }
                            else
                            {
                                questionGroup = FormQuestionGroup.Load(qg.ToInteger(), amsgs, adc);
                                questions = questionGroup.GetAllQuestions(amsgs, adc);

                                if (questionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
                                {
                                    questionGroup.Name = Request.Params[$"name-{questionGroup.ID}"];
                                    questionGroup.LastModifiedBy = currentUser.ID;
                                    questionGroup.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(questionGroup.Save(adc));
                                }
                            }

                            // save existing questions
                            questions.ForEach(question =>
                            {
                                var text = Request.Params[$"text-{question.ID}"];
                                var desc = Request.Params[$"description-{question.ID}"];
                                var options = Request.Params[$"options-text-{question.ID}"];
                                var link = Request.Params[$"link-text-{question.ID}"];
                                var isActive = Request.Params[$"active-{question.ID}"].IsBoolean();
                                var isRequired = Request.Params[$"required-{question.ID}"].IsBoolean();
                                var isAnswerDescriptionEnabled =
                                    Request.Params[$"answerDescription-{question.ID}"].IsBoolean();

                                // this extra conditions are to prevent null entry in db from refresh-post calls
                                if (!question.IsLocked &&
                                    (text.IsNotEmpty() || desc.IsNotEmpty() || options.IsNotEmpty() ||
                                     link.IsNotEmpty() ||
                                     question.IsActive != isActive || question.IsRequired != isRequired ||
                                     question.IsAnswerDescriptionEnabled != isAnswerDescriptionEnabled))
                                {
                                    question.IsActive = isActive;
                                    question.IsRequired = isRequired;
                                    question.IsAnswerDescriptionEnabled = isAnswerDescriptionEnabled;

                                    if (question.QuestionDataTypeId == QuestionDataType.NotSet)
                                    {
                                        question.Text = text;
                                        question.Description = desc;
                                        question.Options = options;
                                        question.Link = link;
                                        question.DisplayOrder =
                                            Request.Params[$"displayOrderId-{question.ID}"].ToInteger();
                                        question.QuestionTypeId =
                                            (QuestionType) Request.Params[$"questiontype-{question.ID}"].ToInteger();
                                    }

                                    question.LastModifiedBy = currentUser.ID;
                                    question.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(question.Save(adc));
                                }
                            });

                            if (Request.Form["newquestionsIds"].Length > 0)
                            {
                                SaveNewQuestions(form.ID, currentUser.ID, questionGroup.ID, amsgs, adc);
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);
                        msgs.Add("Form Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                    onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
                );
            }

            questionGroups = form.GetQuestionGroups(msgs);
            individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

            //make sure only question groups with a setting of individual or organization are added to the final count
            questionGroups.Clear();
            questionGroups.AddRange(individualGroups);

            var currentIndex = -1;

            if (questionGroup.IsNotEmpty())
                currentIndex = questionGroups.FindIndex(x => x.ID.Value == questionGroup.ID.Value);

            switch (cmd)
            {
                case "prev":
                    if (currentIndex > 0) questionGroup = questionGroups[currentIndex - 1];
                    break;
                case "next":
                    if (currentIndex < questionGroups.Count - 1) questionGroup = questionGroups[currentIndex + 1];
                    break;
            }

            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.QuestionGroup = questionGroup;
            ViewBag.Messages = msgs;
            return View(form);
        }

        [HttpGet]
        public ActionResult Report(int? form, string activities, bool collapseRecords = false, RegistrantPersonType personType = RegistrantPersonType.NotSet)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            Form formObj = null;
            var selectedActivities = new List<Activity>();
            var activityOptions = currentAccount.GetActivities(SessionVariables.AccountUserType, currentUser, null, null, null, false, null, null, null, msgs);
            var questions = new List<FormQuestion>();
            var forms = new List<Form>();
            var formType = personType == RegistrantPersonType.Student ? FormType.StudentForm : FormType.VolunteerForm;
            var showCollapsedRecordOption = true;

            if (form.HasValue)
            {
                formObj = currentAccount.GetForm(form.Value, msgs);
                questions = formObj.GetAllQuestions(msgs)
                    .Where(x => !x.IsDeleted && x.QuestionTypeId != QuestionType.TextBlock)
                    .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                    .ThenBy(x => x.DisplayOrder)
                    .ToList();

                if (formObj.FormTypeId == FormType.Survey)
                    showCollapsedRecordOption = false;
            }

            if (activities.IsNotBlank())
            {
                var activityIds = activities.Split(',').ToIntegerArray().ToList();
                selectedActivities = currentAccount.GetActivities(activityIds, msgs);
                selectedActivities.ForEach(x => forms.Add(x.GetActiveForm(msgs)));                
            }

            if (!forms.Any())
                forms = currentAccount.GetForms(null, msgs).Where(x => x.FormTypeId == formType).ToList();

            if (!questions.Any())
            {
                var allQuestions = new List<FormQuestion>();

                forms.ForEach(x => allQuestions.AddRange(x.GetAllQuestions(msgs).Where(x => !x.IsDeleted && x.QuestionTypeId != QuestionType.TextBlock)));

                questions.AddRange(allQuestions.Where(x => x.QuestionDataTypeId == QuestionDataType.NotSet));
                questions.AddRange(allQuestions.Where(x => x.QuestionDataTypeId != QuestionDataType.NotSet).DistinctBy(x => x.QuestionDataTypeId));

                questions = questions
                    .OrderBy(x => x.FormQuestionGroupDisplayOrder)
                    .ThenBy(x => x.DisplayOrder)
                    .ToList();
            }

            var reportResults = new ReportResults()
            {
                Questions = questions,
                Messages = msgs,
                Form = formObj,
                FormId = form,
                SelectedActivities = selectedActivities,
                ActivityOptions = activityOptions.OrderBy(x => x.Name).ToList(),
                CollapseRecords = collapseRecords && showCollapsedRecordOption,
                PersonType = personType,
                ShowCollapseRecordOption = showCollapsedRecordOption,
            };

            return View(reportResults);
        }

        [HttpPost]
        public ActionResult GetReportPage(int? form, int start, int length, string sdate, string edate, string search, string activities, 
            string searchquestions, bool collapseRecords = false, RegistrantPersonType personType = RegistrantPersonType.NotSet)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();

            var formId = new int?();
            var startDate = new DateTime?();
            var endDate = new DateTime?();
            var activityIds = new List<int>();
            var searchQuestionIds = new List<int>();
            var searchQuestionTypeIds = new List<QuestionDataType>();

            if (sdate.IsDateTime())
                startDate = sdate.ToDateTime();

            if (edate.IsDateTime())
                endDate = edate.ToDateTime().AddDays(1).AddMilliseconds(-1);

            if (activities.IsNotBlank())
                activityIds = activities.Split(',').ToIntegerArray().ToList();

            if (form.HasValue)
                formId = currentAccount.GetForm(form.Value, msgs)?.ID;

            if (searchquestions.IsNotBlank())
            {
                searchQuestionIds = searchquestions.Split(',').Select(x => x.Split('|').First().ToInteger()).ToList();
                searchQuestionTypeIds = searchquestions.Split(',').Select(x => x.Split('|').Last().ToInteger().ToEnum<QuestionDataType>()).ToList();
            }

            var registrants = Registrant.LoadByAnswered(currentAccount.ID.Value, formId, activityIds, search, startDate, endDate, start, length, 
                collapseRecords, searchQuestionIds, searchQuestionTypeIds, personType, msgs);
            var totalRegistrantCount = Registrant.GetCountByAnswered(currentAccount.ID.Value, formId, activityIds, search, startDate, endDate, 
                collapseRecords, searchQuestionIds, searchQuestionTypeIds, personType, msgs);
            var answers = new List<FormQuestionAnswer>();
            var registrantActivities = new Dictionary<int, string>();

            if (registrants.Any())
            {
                var registrantsWithAnswers = new List<Registrant>();
                registrantsWithAnswers.AddRange(registrants);

                if (collapseRecords)
                    registrants.ForEach(registrant => registrantsWithAnswers.AddRange(registrant.GetRelatedRegistrants(msgs)));

                answers = FormQuestionAnswer.LoadByRegistrantIds(registrantsWithAnswers.Select(x => x.ID.Value), msgs)
                            .Where(x => !x.Text.IsBlank())
                            .OrderByDescending(x => x.CreateDate.Value)
                            .ToList();
               
                // preload the registrants activities
                // only show activities that are in the filter
                registrants.ForEach(registrant => 
                {
                    var ras = registrant.GetRegistrantActivities(msgs);

                    if (collapseRecords)
                        registrant.GetRelatedRegistrants(msgs)
                            .ForEach(relatedRegistrant => ras.AddRange(relatedRegistrant.GetRegistrantActivities(msgs)));

                    if (activityIds.Any())
                        ras = ras.Where(ra => ra.ParticipationObjectTypeId == ParticipationObjectType.Activity && ra.ParticipationObjectId.Value.In(activityIds)).ToList();

                    registrantActivities.Add(
                        registrant.ID.Value,
                        ras.Select(ra => ra.GetParticipationObject(msgs).Name).ToStringList(", ")
                    );
                });

            }

            if (msgs.HasNoErrors())
                return Json(new
                {
                    status = "ok",
                    registrants = registrants.Select(registrant => 
                                new 
                                { 
                                    ID = registrant.ID.Value, 
                                    FirstRegistrantId = registrant.FirstRegistrantId.IfEmpty(""), 
                                    CreateDate = registrant.CreateDate.ToShortDateTimeString(),
                                    Activities = registrantActivities[registrant.ID.Value],
                                    RecordId = registrant.FirstRegistrantId.IfEmpty(registrant.ID.Value).ToString().PadLeft(9, '0'),
                                    registrant.FirstName,
                                    registrant.LastName,
                                    registrant.Email,
                                }),
                    answers = answers.Select(answer =>
                                new 
                                { 
                                    ID = answer.ID.Value, 
                                    Text = answer.GetAnswerText().IfEmpty(""), 
                                    QuestionId = answer.FormQuestionId.Value, 
                                    QuestionDataTypeId = (int)answer.QuestionDataTypeId, 
                                    RegistrantId = answer.RegistrantId.Value,
                                    FirstRegistrantId = answer.RegistrantFirstRegistrantId
                                }),
                    registrantCount = totalRegistrantCount,
                    messages = msgs,
                }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EmailReport(int? form, string sdate, string edate, string search, string hidden, string activities, 
            string searchquestions, bool collapseRecords = false, RegistrantPersonType personType = RegistrantPersonType.NotSet)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);

            var formId = new int?();
            var activityIds = new List<int>();
            var startDate = new DateTime?();
            var endDate = new DateTime?();
            var searchQuestionIds = new List<int>();
            var searchQuestionTypeIds = new List<QuestionDataType>();

            if (sdate.IsDateTime())
                startDate = sdate.ToDateTime();

            if (edate.IsDateTime())
                endDate = edate.ToDateTime().AddDays(1).AddMilliseconds(-1);

            if (form.HasValue)
                formId = currentAccount.GetForm(form.Value, msgs)?.ID;

            if (activities.IsNotBlank())
                activityIds = activities.Split(',').ToIntegerArray().ToList();

            if (searchquestions.IsNotBlank())
            {
                searchQuestionIds = searchquestions.Split(',').Select(x => x.Split('|').First().ToInteger()).ToList();
                searchQuestionTypeIds = searchquestions.Split(',').Select(x => x.Split('|').Last().ToInteger().ToEnum<QuestionDataType>()).ToList();
            }

            BackgroundJob.Enqueue(() =>
                FormService.EmailDataReport(
                    currentAccount.ID.Value,
                    formId,
                    activityIds,
                    search, 
                    startDate, 
                    endDate, 
                    hidden.IsNotEmpty() ? hidden.Split(',').Select(x => x.ToInteger()).ToArray() : new int[] { },
                    searchQuestionIds, 
                    searchQuestionTypeIds,
                    collapseRecords,
                    personType,
                    currentUser.ID.Value)
            );

            if (msgs.HasNoErrors())
                return Json(new
                {
                    status = "ok",
                    messages = msgs,
                    email = currentUser.Email,
                }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                status = "error",
                messages = msgs
            }, JsonRequestBehavior.AllowGet);

        }        

        private void SaveNewQuestions(int? formId, int? currentUserId, int? formQuestionGroupId, SystemMessages msgs, DatabaseConnection dc)
        {
            var questionIds = Request.Form["newquestionsIds"];
            var questionsList = questionIds.Split(',');
            foreach (var question in questionsList)
            {
                var text = Request.Params[$"new-text-{question}"];

                var formQuestion = new FormQuestion
                {
                    FormId = formId,
                    CreateDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    CreatedBy = currentUserId,
                    LastModifiedBy = currentUserId,
                    DisplayOrder = Request.Params[$"new-sort-{question}"].ToInteger(),
                    FormQuestionGroupId = formQuestionGroupId,
                    IsTemplate = false,
                    QuestionDataTypeId = QuestionDataType.NotSet,
                    QuestionTypeId = (QuestionType) Request.Params[$"new-questiontype-{question}"].ToInteger(),
                    Text = text,
                    Options = Request.Params[$"new-options-text-{question}"],
                    Link = Request.Params[$"new-link-text-{question}"],
                    Description = Request.Params[$"new-description-{question}"],
                    IsActive = true,
                    IsDeleted = false,
                    IsRequired = Request.Params[$"new-required-{question}"].IsBoolean(),
                    IsAnswerDescriptionEnabled = Request.Params[$"new-description-enabled-{question}"].IsBoolean(),
                };
                msgs.AddRange(formQuestion.Save(dc));
            }
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var questionToDelete = FormQuestion.Load(id.ToInteger(), msgs);
            questionToDelete.IsDeleted = true;
            msgs.AddRange(questionToDelete.Save());

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted", questionToDelete.Text), MessageType.Success);
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Deleted,
                    objectType: ObjectType.FormQuestion,
                    objectId: id.ToInteger(),
                    objectText: questionToDelete.Text,
                    indirectObjectType: ObjectType.FormQuestion,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            SetTempMessages(msgs);

            return RedirectToAction("edit",
                new { id = questionToDelete.FormId, qg = questionToDelete.FormQuestionGroupId });
        }

        [HttpGet]
        public ActionResult DeleteSection(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();

            if (currentAccount.IsEmpty())
                return HttpNotFound();

            var sectionToDelete = FormQuestionGroup.Load(id: id.ToInteger(), msgs: msgs);
            sectionToDelete.IsDeleted = true;
            msgs.AddRange(sectionToDelete.Save());

            //lets inactive all the questions for this section, so later when retrieving questions, we don't have to check for removed sections
            var questionsInThisSection =
                FormQuestion.LoadByFormQuestionGroupId(FormQuestionGroupId: sectionToDelete.ID.Value, msgs: msgs);
            foreach (var item in questionsInThisSection)
            {
                item.IsDeleted = true;
                msgs.AddRange(item.Save());
            }

            if (msgs.HasNoErrors())
            {
                msgs.Add(string.Format("{0} has been deleted with all it's questions.", sectionToDelete.Name),
                    MessageType.Success);
                SessionVariables.WebSession.LogActivity(
                    account: currentAccount,
                    actor: currentUser,
                    actionType: WebSessionActionType.Deleted,
                    objectType: ObjectType.FormQuestionGroup,
                    objectId: id.ToInteger(),
                    objectText: sectionToDelete.Name,
                    indirectObjectType: ObjectType.FormQuestionGroup,
                    indirectObjectId: currentAccount.ID.Value,
                    indirectObjectText: currentAccount.Name
                );
            }

            ViewBag.Messages = msgs;
            SetTempMessages(msgs);

            return RedirectToAction("edit", new { id = sectionToDelete.FormId, qg = "" });
        }


        [HttpGet]
        public ActionResult Archive(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var form = currentAccount.GetForm(id.ToInteger(), msgs);

            if (form.IsNotEmpty())
                msgs.AddRange(form.MoveToArchive(currentUser.ID.Value));

            // Show Success Message
            if (msgs.HasNoErrors())
                msgs.Add($"{form.Name} has been archived", MessageType.Success);

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "form");
        }

        [HttpGet]
        public ActionResult Restore(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var form = currentAccount.GetForm(id.ToInteger(), msgs);

            if (form.IsNotEmpty())
                msgs.AddRange(form.MoveFromArchive(currentUser.ID.Value));

            // Show Success Message
            if (msgs.HasNoErrors())
                msgs.Add($"{form.Name} has been restored", MessageType.Success);

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "form");
        }

        [HttpGet]
        public ActionResult SetAsDefault(string id)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var msgs = new SystemMessages();
            var form = currentAccount.GetForm(id.ToInteger(), msgs);

            if (form.IsNotEmpty())
                msgs.AddRange(currentAccount.SetDefaultForm(form, currentUser));

            // Show Success Message
            if (msgs.HasNoErrors())
                msgs.Add($"{form.Name} has been set as the default", MessageType.Success);

            ViewBag.Messages = msgs;

            if (Request.UrlReferrer.IsNotEmpty())
                return Redirect(Request.UrlReferrer.ToString());

            return RedirectToAction("index", "form");
        }
    }
}