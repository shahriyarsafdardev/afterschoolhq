using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Clever.Models;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using System.Windows.Interop;

namespace AfterSchool.Areas.Api.Controllers
{
    public class FormController : Controller
    {

        [HttpGet]
        public ActionResult GetFormData(string formId)
        {
            if (formId.IsEmpty())
                return HttpNotFound();
            var msgs = new SystemMessages();
            try
            {
                var currentAccount = SessionVariables.CurrentAccount;

                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var form = currentAccount.GetForm(formId.ToInteger(), msgs);
                var formSettings = FormSettingsDto.fromForm(form);

                if (form.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var questionGroups = form.GetQuestionGroups(msgs)
                    .Where(
                        questionGroup => questionGroup.
                        RegistrantTypeId == RegistrantType.Individual)
                    .OrderBy(
                        questionGroup => questionGroup
                        .DisplayOrder)
                    .ToList();
                var formQuestionGroups = FormQuestionGroupDto.GetByQuestionGroupList(questionGroups);

                var response = new
                {
                    formQuestionGroup = formQuestionGroups,
                    form = formSettings
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetFormQuestions(string formId, string sectionId, string sectionTitle)
        {
            if (formId.IsEmpty())
                return HttpNotFound();
            var msgs = new SystemMessages();
            try
            {
                var currentAccount = SessionVariables.CurrentAccount;

                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var form = currentAccount.GetForm(formId.ToInteger(), msgs);

                if (form.IsEmpty())
                    throw new Exception("Invalid Form Id");

                if (sectionId.IsEmpty())
                    throw new Exception("Invalid Section Id");

                var sectionQuestionTypeId = (QuestionGroupDataType)sectionId.ToInteger();

                var questions = form.GetAllQuestions(msgs)
                    .Where(
                        question=>question.
                        FormQuestionGroupQuestionGroupDataTypeId.Equals(sectionQuestionTypeId)
                        &&
                        question.FormQuestionGroupName.Equals(sectionTitle)
                    )
                    .OrderBy(
                        question=>question.DisplayOrder
                    )
                    .ToList();
                var formQuestions = FormQuestionDto.GetByFormQuestionList(questions);

                var response = new
                {
                    formQuestion = formQuestions,
                };

                return Json(
                    new ApiResponse(msgs, response),
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.ApiError);

                return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpGet]
        //public ActionResult SaveFormData(string formId, List<FormQuestionGroupDto> formQuestionGroup, List<FormQuestionDto> formQuestion)
        //{
        //    var msgs = new SystemMessages();
        //    var currentAccount = SessionVariables.CurrentAccount;
        //    var currentUser = SessionVariables.CurrentUser;

        //    if (currentAccount.IsEmpty())
        //        throw new Exception("Unauthorized Account");

        //    if (formId.IsEmpty())
        //        throw new Exception("Invalid Form Id");

        //    var form = currentAccount.GetForm(formId.ToInteger(), msgs);
        //    var formSettings = FormSettingsDto.fromForm(form);


        //    Database.ExecuteTransaction(
        //    action: (SystemMessages amsgs, DatabaseConnection adc) =>
        //    {
        //        formSettings.alterForm(form);
        //        amsgs.AddRange(form.Save(adc));
        //        formQuestionGroup.ForEach(questionGroup =>
        //        {
        //            var questionGroupExists = FormQuestionGroup.Exists(criterias:
        //                new Hashtable{
        //                    {"Name", questionGroup.Name},
        //                    {"FormId", form.ID},
        //                    {"IsDeleted", false}
        //                },
        //                msgs: amsgs,
        //                dc: adc);
        //            FormQuestionGroup currQuestionGroup;
        //            List<FormQuestion> prevQuestions;
        //            if (!questionGroupExists)
        //            {
        //                currQuestionGroup = new FormQuestionGroup()
        //                {
        //                    CreateDate = DateTime.UtcNow,
        //                    CreatedBy = currentUser.ID,
        //                    LastModifiedDate = DateTime.UtcNow,
        //                    LastModifiedBy = currentUser.ID,
        //                    DisplayOrder = form.GetQuestionGroupCount(amsgs, adc) + 1,
        //                    FormId = form.ID,
        //                    QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
        //                    RegistrantTypeId = RegistrantType.Individual,
        //                    Name = questionGroup.Name,
        //                    IsDeleted = false,
        //                    IsActive = true,
        //                };
        //                amsgs.AddRange(currQuestionGroup.Save(adc));
        //                prevQuestions = new List<FormQuestion>();
        //            }
        //            else
        //            {
        //                currQuestionGroup = FormQuestionGroup
        //                        .Load(criterias: new Hashtable{
        //                            {"Name", questionGroup.Name},
        //                            {"FormId", form.ID},
        //                            {"IsDeleted", false}
        //                        },
        //                        msgs: amsgs, dc: adc)?.FirstOrDefault();
        //                if (currQuestionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
        //                {
        //                    currQuestionGroup.Name = Request.Params[$"name-{currQuestionGroup.ID}"];
        //                    currQuestionGroup.LastModifiedBy = currentUser.ID;
        //                    currQuestionGroup.LastModifiedDate = DateTime.UtcNow;
        //                    amsgs.AddRange(currQuestionGroup.Save(adc));
        //                }
        //                prevQuestions = currQuestionGroup.GetAllQuestions(amsgs, adc);
        //            }
        //            var currQuestions = formQuestion.Where(question=>
        //                                                    question.FormQuestionGroupQuestionGroupDataTypeId.Equals(questionGroup.QuestionGroupDataTypeId)
        //                                                    &&
        //                                                    question.GroupTitle.Equals(questionGroup.Name));

        //        });
        //        if (qg.IsNumber())
        //        {
        //            // create or update question groups
        //            if (qg.ToInteger() == 0)
        //            {
        //            }
        //            else
        //            {
        //                questionGroup = FormQuestionGroup.Load(qg.ToInteger(), amsgs, adc);
        //                questions = questionGroup.GetAllQuestions(amsgs, adc);

        //                if (questionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
        //                {
        //                    questionGroup.Name = Request.Params[$"name-{questionGroup.ID}"];
        //                    questionGroup.LastModifiedBy = currentUser.ID;
        //                    questionGroup.LastModifiedDate = DateTime.UtcNow;
        //                    amsgs.AddRange(questionGroup.Save(adc));
        //                }
        //            }

        //            // save existing questions
        //            questions.ForEach(question =>
        //            {
        //                var text = Request.Params[$"text-{question.ID}"];
        //                var desc = Request.Params[$"description-{question.ID}"];
        //                var options = Request.Params[$"options-text-{question.ID}"];
        //                var link = Request.Params[$"link-text-{question.ID}"];
        //                var isActive = Request.Params[$"active-{question.ID}"].IsBoolean();
        //                var isRequired = Request.Params[$"required-{question.ID}"].IsBoolean();
        //                var isAnswerDescriptionEnabled = Request.Params[$"answerDescription-{question.ID}"].IsBoolean();

        //                if (text != null && text.Length > 400)
        //                    amsgs.Add("Question should not exceed 400 characters", MessageType.UserError);

        //                if (desc != null && desc.Length > 4000)
        //                    amsgs.Add("Description should not exceed 4,000 characters", MessageType.UserError);


        //                // this extra conditions are to prevent null entry in db from refresh-post calls
        //                if (!question.IsLocked &&
        //                    (text.IsNotEmpty() || desc.IsNotEmpty() || options.IsNotEmpty() ||
        //                     link.IsNotEmpty() ||
        //                     question.IsActive != isActive || question.IsRequired != isRequired ||
        //                     question.IsAnswerDescriptionEnabled != isAnswerDescriptionEnabled))
        //                {
        //                    question.IsActive = isActive;
        //                    question.IsRequired = isRequired;
        //                    question.IsAnswerDescriptionEnabled = isAnswerDescriptionEnabled;

        //                    if (question.QuestionDataTypeId == QuestionDataType.NotSet)
        //                    {
        //                        question.Text = text;
        //                        question.Description = desc;
        //                        question.Options = options;
        //                        question.Link = link;
        //                        question.DisplayOrder =
        //                            Request.Params[$"displayOrderId-{question.ID}"].ToInteger();
        //                        question.QuestionTypeId =
        //                            (QuestionType) Request.Params[$"questiontype-{question.ID}"].ToInteger();
        //                    }

        //                    question.LastModifiedBy = currentUser.ID;
        //                    question.LastModifiedDate = DateTime.UtcNow;
        //                    amsgs.AddRange(question.Save(adc));
        //                }
        //            });

        //            if (Request.Form["newquestionsIds"].Length > 0)
        //            {
        //                SaveNewQuestions(form.ID, currentUser.ID, questionGroup.ID, amsgs, adc);
        //            }
        //        }
        //    },
        //            onSuccess: (SystemMessages smsgs) =>
        //            {
        //                msgs.AddRange(smsgs);
        //                msgs.Add("Form Saved", MessageType.Success);
        //            },
        //            onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
        //            onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs)
        //        );

        //    return Json();
        //}
    }
}