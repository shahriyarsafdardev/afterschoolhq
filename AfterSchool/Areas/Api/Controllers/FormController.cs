using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using Clever.Models;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using System.Windows.Interop;
using static System.Net.Mime.MediaTypeNames;

namespace AfterSchool.Areas.Api.Controllers
{
    public class FormController : Controller
    {

        [HttpGet]
        public ActionResult GetFormData(int formId)
        {
            if (formId.IsEmpty())
                return HttpNotFound();
            var msgs = new SystemMessages();
            try
            {
                var currentAccount = SessionVariables.CurrentAccount;
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var form = currentAccount.GetForm(formId, msgs);
                //var form = AfterSchoolLibrary.Models.Form.Load(
                //    criterias: new Hashtable{ {"ID", formId} }).FirstOrDefault();

                if (form.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var formSettings = FormSettingsDto.fromForm(form);

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
        public ActionResult GetFormQuestions(int formId, int sectionId)
        {
            
            var msgs = new SystemMessages();
            try
            {
                if (formId.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var currentAccount = SessionVariables.CurrentAccount;

                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var form = currentAccount.GetForm(formId, msgs);
                //var form = AfterSchoolLibrary.Models.Form.Load(
                //    criterias: new Hashtable { { "ID", formId } }
                //    ).First();

                if (form.IsEmpty())
                    throw new Exception("Invalid Form Id");

                if (sectionId.IsEmpty())
                    throw new Exception("Invalid Section Id");

                var sectionQuestionTypeId = (QuestionGroupDataType) sectionId;

                var questions = form.GetAllQuestions(msgs)
                    .Where(
                        question => question.
                        FormQuestionGroupQuestionGroupDataTypeId.Equals(sectionQuestionTypeId)
                    )
                    .OrderBy(
                        question => question.DisplayOrder
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

        [HttpPost]
        public ActionResult SaveFormData(FormSettingsDto formSettings, FormQuestionGroupDto formQuestionGroup, List<FormQuestionDto> formQuestion)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var formId = formSettings.formId;

            if (currentAccount.IsEmpty())
                throw new Exception("Unauthorized Account");

            if (formId.IsEmpty())
                throw new Exception("Invalid Form Id");

            Database.ExecuteTransaction(
            action: (SystemMessages amsgs, DatabaseConnection adc) =>
            {
                var form = currentAccount.GetForm(formId, msgs);

                //var form = AfterSchoolLibrary.Models.Form.Load(
                //    criterias: new Hashtable { { "ID", formId } }

                //    ).First();

                formSettings.UpdateFormIfNotEmpty(form);
                amsgs.AddRange(form.Save(adc));

                if (formQuestionGroup.IsNotEmpty())
                {
                    var questionGroupExists = FormQuestionGroup.Exists(criterias:
                        new Hashtable{
                            {"Name", formQuestionGroup.Name},
                            {"FormId", form.ID},
                            {"IsDeleted", false}
                        },
                        msgs: amsgs,
                        dc: adc);
                    FormQuestionGroup currQuestionGroup;
                    List<FormQuestion> prevQuestions;
                    if (!questionGroupExists)
                    {
                        currQuestionGroup = new FormQuestionGroup()
                        {
                            CreateDate = DateTime.UtcNow,
                            CreatedBy = currentUser.ID,
                            LastModifiedDate = DateTime.UtcNow,
                            LastModifiedBy = currentUser.ID,
                            DisplayOrder = form.GetQuestionGroupCount(amsgs, adc) + 1,
                            FormId = form.ID,
                            QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                            RegistrantTypeId = RegistrantType.Individual,
                            Name = formQuestionGroup.Name,
                            IsDeleted = false,
                            IsActive = true,
                        };
                        amsgs.AddRange(currQuestionGroup.Save(adc));
                        prevQuestions = new List<FormQuestion>();
                    }
                    else
                    {
                        currQuestionGroup = FormQuestionGroup
                                .Load(criterias: new Hashtable{
                                    {"Name", formQuestionGroup.Name},
                                    {"FormId", form.ID},
                                    {"IsDeleted", false}
                                },
                                msgs: amsgs, dc: adc)?.FirstOrDefault();

                        if (currQuestionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
                        {
                            currQuestionGroup.Name = formQuestionGroup.Name;
                            currQuestionGroup.LastModifiedBy = currentUser.ID;
                            currQuestionGroup.LastModifiedDate = DateTime.UtcNow;
                            amsgs.AddRange(currQuestionGroup.Save(adc));
                        }

                        prevQuestions = currQuestionGroup.GetAllQuestions(amsgs, adc);
                        formQuestion.ForEach(newQuestion =>
                        {
                            var prevQuestion = prevQuestions.Where(question => question.ID == newQuestion.ID).First();
                            if (prevQuestion.IsNotEmpty() && !prevQuestion.IsLocked &&
                                    (!prevQuestion.IsActive.Equals(newQuestion.IsActive) ||
                                    prevQuestion.IsRequired.Equals(newQuestion.IsRequired) ||
                                    prevQuestion.IsAnswerDescriptionEnabled.Equals(newQuestion.IsAnswerDescriptionEnabled)))
                            {
                                newQuestion.UpdateQuestionIfNotEmpty(prevQuestion);
                                prevQuestion.LastModifiedBy = currentUser.ID;
                                prevQuestion.LastModifiedDate = DateTime.UtcNow;
                                msgs.AddRange(prevQuestion.Save(adc));
                            }
                            else
                            {
                                var newFormQuestion = new FormQuestion
                                {
                                    FormId = formId,
                                    CreateDate = DateTime.UtcNow,
                                    LastModifiedDate = DateTime.UtcNow,
                                    CreatedBy = currentUser.ID,
                                    LastModifiedBy = currentUser.ID,
                                    DisplayOrder = newQuestion.DisplayOrder,
                                    FormQuestionGroupId = newQuestion.FormQuestionGroupId,
                                    IsTemplate = false,
                                    QuestionDataTypeId = QuestionDataType.NotSet,
                                    QuestionTypeId = newQuestion.QuestionTypeId,
                                    Text = newQuestion.Text,
                                    Options = newQuestion.Options,
                                    Link = newQuestion.Link,
                                    Description = newQuestion.Description,
                                    IsActive = true,
                                    IsDeleted = false,
                                    IsRequired = newQuestion.IsRequired,
                                    IsAnswerDescriptionEnabled = newQuestion.IsAnswerDescriptionEnabled,
                                };
                                msgs.AddRange(newFormQuestion.Save(adc));
                            }
                        });
                    }

                }
            },
            onSuccess: (SystemMessages smsgs) =>
            {
                msgs.AddRange(smsgs);
                msgs.Add("Form Saved", MessageType.Success);
            },
            onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
            onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs));
            return Json(
                    new ApiResponse(msgs, null),
                    JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult CopyFromQuestion(int formId, int sectionId, int questionIdToCopyFrom, string questionName)
        //{
        //    var msgs = new SystemMessages();
        //    var currentAccount = SessionVariables.CurrentAccount;
        //    var currentUser = SessionVariables.CurrentUser;

        //    if (currentAccount.IsEmpty())
        //        throw new Exception("Unauthorized Account.");
        //    if (formId.IsEmpty())
        //        throw new Exception("Invalid form id.");
        //    if (sectionId.IsEmpty())
        //        throw new Exception("Invalid section id.");
        //    if (questionIdToCopyFrom.IsEmpty())
        //        throw new Exception("Invalid question id.");
        //    if (questionName.IsEmpty())
        //        throw new Exception("Question Name is required.");

        //    Database.ExecuteTransaction(
        //        action: (SystemMessages amsgs, DatabaseConnection adc) =>
        //        {
        //            var form = currentAccount.GetForm(formId, msgs);

        //            if (form.IsEmpty())
        //                throw new Exception("Invalid form id.");

        //            var questionToCopyFrom = FormQuestion
        //                            .Load(criterias: new Hashtable{
        //                            {"FormId", form.ID},
        //                            {"FormQuestionGroupId", sectionId},
        //                            {"QuestionTypeId", questionIdToCopyFrom}
        //                            },
        //                            msgs: msgs)?.First();

        //            if(questionToCopyFrom.IsEmpty())
        //                throw new Exception("Invalid 'Section Id' or 'Question Id'.");

        //            var newFormQuestion = new FormQuestion
        //            {
        //                FormId = form.ID,
        //                CreateDate = DateTime.UtcNow,
        //                LastModifiedDate = DateTime.UtcNow,
        //                CreatedBy = currentUser.ID,
        //                LastModifiedBy = currentUser.ID,
        //                DisplayOrder = questionToCopyFrom.DisplayOrder + 1,
        //                FormQuestionGroupId = questionToCopyFrom.FormQuestionGroupId,
        //                IsTemplate = false,
        //                QuestionDataTypeId = QuestionDataType.NotSet,
        //                QuestionTypeId = questionToCopyFrom.QuestionTypeId,
        //                Text = questionName,
        //                Options = questionToCopyFrom.Options,
        //                Link = questionToCopyFrom.Link,
        //                Description = questionToCopyFrom.Description,
        //                IsActive = true,
        //                IsDeleted = false,
        //                IsRequired = questionToCopyFrom.IsRequired,
        //                IsAnswerDescriptionEnabled = questionToCopyFrom.IsAnswerDescriptionEnabled,
        //            };

        //            var prevFormQuestions = FormQuestion
        //                        .Load(criterias: new Hashtable{
        //                            {"FormId", form.ID},
        //                            {"FormQuestionGroupId", sectionId},
        //                        },
        //                        msgs: msgs);

        //            if(prevFormQuestions.IsEmpty())
        //                throw new Exception("Invalid 'Section Id'.");

        //            prevFormQuestions.ForEach(question =>
        //            {
        //                if (question.DisplayOrder > questionToCopyFrom.DisplayOrder)
        //                {
        //                    question.DisplayOrder++;
        //                    msgs.AddRange(question.Save());
        //                }
        //            });

        //            msgs.AddRange(newFormQuestion.Save());

        //        },
        //        onSuccess: (SystemMessages smsgs) =>
        //        {
        //            msgs.AddRange(smsgs);
        //            msgs.Add("Form Saved", MessageType.Success);
        //        },
        //        onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
        //        onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs));
        //    if(msgs.HasNoErrors())
        //        return GetFormQuestions(formId,sectionId);
        //    return Json(
        //            new ApiResponse(msgs, null),
        //            JsonRequestBehavior.AllowGet);
        //}
    }
}