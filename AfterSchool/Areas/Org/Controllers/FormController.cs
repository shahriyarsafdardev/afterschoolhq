using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Areas.Org.Controllers
{
    public class FormController : BaseLoggedInController
    {
        public FormController() : base(AccountService.OrganizationAccountTypes)
        {

        }

        // GET: Org/Form
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Messages = new SystemMessages();
            return View(SessionVariables.CurrentAccount.GetForms());
        }

        [HttpGet]
        public ActionResult Edit(string id, string qg)
        {
            var form = Form.Load(id.ToInteger());
            var questionGroups = form.GetQuestionGroups();
            var individualGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual && x.GetQuestionCount() > 0)
                .OrderBy(x => x.DisplayOrder).ToList();
            var organizationGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Organization && x.GetQuestionCount() > 0)
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
        public ActionResult Edit(string id, string copyfrom, string qg, string name, string description, string cmd)
        {
            var msgs = new SystemMessages();
            Form form = null;
            FormQuestionGroup questionGroup = null;

            if (name.IsEmpty() && qg.IsBlank())
                msgs.Add("Name is required", MessageType.UserError);

            if (id.ToInteger() == 0)
            {
                if (copyfrom.IsNumber())
                    form = Form.Load(copyfrom.ToInteger(), msgs)
                        .Copy(SessionVariables.CurrentUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, FormType.NotSet, null, msgs);
                else
                    form = SessionVariables.CurrentAccount
                        .GetDefaultForm(msgs)
                        .Copy(SessionVariables.CurrentUser.ID.Value, SessionVariables.CurrentAccount.ID.Value, FormType.NotSet, null, msgs);
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
                                    CreatedBy = SessionVariables.CurrentUser.ID,
                                    LastModifiedDate = DateTime.UtcNow,
                                    LastModifiedBy = SessionVariables.CurrentUser.ID,
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
                                    questionGroup.LastModifiedBy = SessionVariables.CurrentUser.ID;
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

                                    question.LastModifiedBy = SessionVariables.CurrentUser.ID;
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
                                        CreatedBy = SessionVariables.CurrentUser.ID,
                                        LastModifiedBy = SessionVariables.CurrentUser.ID,
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
                .Where(x => x.RegistrantTypeId == RegistrantType.Individual && x.GetQuestionCount() > 0)
                .OrderBy(x => x.DisplayOrder)
                .ToList();
            var organizationGroups = questionGroups
                .Where(x => x.RegistrantTypeId == RegistrantType.Organization && x.GetQuestionCount() > 0)
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
                    if(currentIndex > 0) questionGroup = questionGroups[currentIndex - 1];
                    break;
                case "next":
                    if(currentIndex < questionGroups.Count - 1) questionGroup = questionGroups[currentIndex + 1];
                    break;
            }

            ViewBag.Messages = msgs;
            ViewBag.QuestionGroups = questionGroups;
            ViewBag.IndividualGroups = individualGroups;
            ViewBag.OrganizationGroups = organizationGroups;
            ViewBag.QuestionGroup = questionGroup;
            return View(form);
        }
    }
}