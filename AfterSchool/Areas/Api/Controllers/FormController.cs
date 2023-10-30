using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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

                var requestedForm = currentAccount.GetForm(formId, msgs);
                if (requestedForm.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var formSettings = FormSettingsDto.MapToDto(requestedForm);
                var questionGroups = requestedForm.GetQuestionGroups(msgs)
                    .Where(
                        questionGroup =>
                            questionGroup.RegistrantTypeId == RegistrantType.Individual &&
                        !questionGroup.IsDeleted
                        )
                    .OrderBy(
                        questionGroup => questionGroup
                        .DisplayOrder
                        )
                    .ToList();
                var formQuestionGroups = FormQuestionGroupDto.MapToDtoList(questionGroups);
                var response = new
                {
                    FormQuestionGroup = formQuestionGroups,
                    Form = formSettings,
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

                var requestedForm = currentAccount.GetForm(formId, msgs);
                if (requestedForm.IsEmpty())
                    throw new Exception("Invalid Form Id");
                if (sectionId.IsEmpty())
                    throw new Exception("Invalid Section Id");

                var questionGroup = requestedForm.GetQuestionGroups(msgs).FirstOrDefault(questionGroup => questionGroup.ID.Equals(sectionId));
                if (questionGroup.IsEmpty())
                    throw new Exception("Invalid Section Id");

                var requestedQuestions = requestedForm.GetAllQuestions(msgs)
                    .Where(
                        question =>
                            question.FormQuestionGroupId.Equals(sectionId) &&
                        !question.IsDeleted
                    )
                    .OrderBy(
                        question => question.DisplayOrder
                    )
                    .ToList();
                var questionsToSend = FormQuestionDto.MapToDtoList(requestedQuestions, requestedForm.FormTypeId);
                questionsToSend = FormQuestionDto.validateByDisplayOrder(questionsToSend);
                var response = new
                {
                    FormQuestion = questionsToSend,
                    SectionName = questionGroup.Name,
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
        public ActionResult SaveFormData(FormSettingsDto formSettings, FormQuestionGroupDto formQuestionGroup, List<FormQuestionDto> formQuestion, bool? saveQuestion)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            var formId = formSettings.FormId;
            if (formQuestion.IsEmpty())
                formQuestion = new List<FormQuestionDto>();
            try
            {
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                if (formId.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var requestedForm = currentAccount.GetForm(formId ?? 0, msgs);
                if (requestedForm.IsEmpty())
                    throw new Exception("Invalid Form Id");

                Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    formSettings.UpdateIfNotNull(requestedForm);
                    amsgs.AddRange(requestedForm.Save(adc));
                    if (formQuestionGroup.IsNotNull())
                    {
                        var questionGroupExists = FormQuestionGroup.Exists(criterias:
                            new Hashtable{
                                {"ID", formQuestionGroup.Id },
                                {"FormId", requestedForm.ID},
                                {"IsDeleted", false},
                            },
                            msgs: amsgs,
                            dc: adc);
                        FormQuestionGroup currentQuestionGroup;
                        List<FormQuestion> prevQuestions;
                        if (questionGroupExists)
                        {
                            currentQuestionGroup = FormQuestionGroup
                                    .Load(criterias: new Hashtable{
                                        {"ID", formQuestionGroup.Id },
                                        {"FormId", requestedForm.ID},
                                        {"IsDeleted", false},
                                    },
                                    msgs: amsgs, dc: adc)?.FirstOrDefault();

                            if (currentQuestionGroup.IsEmpty())
                                throw new Exception("Invalid Section id");
                            
                            if (currentQuestionGroup.Name.IsEmpty())
                                throw new Exception("Invalid Section Name");

                            if (
                                (!currentQuestionGroup.Name.Equals(formQuestionGroup.Name) || (formQuestionGroup.IsDeleted ?? true)) &&
                                 (
                                    currentQuestionGroup.QuestionGroupDataTypeId.Equals(QuestionGroupDataType.NotSet) ||
                                    currentQuestionGroup.QuestionGroupDataTypeId.Equals(QuestionGroupDataType.Custom)
                                 )
                                )
                            {
                                var previousQuestionGroupName = currentQuestionGroup.Name;
                                currentQuestionGroup.Name = formQuestionGroup.Name;
                                currentQuestionGroup.IsDeleted = formQuestionGroup.IsDeleted ?? false;
                                currentQuestionGroup.LastModifiedBy = currentUser.ID;
                                currentQuestionGroup.LastModifiedDate = DateTime.UtcNow;
                                amsgs.AddRange(currentQuestionGroup.Save(adc));
                                if (amsgs.HasNoErrors())
                                {
                                    SessionVariables.WebSession.LogActivity(
                                        account: currentAccount,
                                        actor: currentUser,
                                        actionType: WebSessionActionType.Updated,
                                        objectType: ObjectType.FormQuestionGroup,
                                        objectId: currentQuestionGroup.ID.Value,
                                        objectText: $"From {previousQuestionGroupName} To {formQuestionGroup.Name}",
                                        indirectObjectType: ObjectType.Form,
                                        indirectObjectId: requestedForm.ID.Value,
                                        indirectObjectText: requestedForm.Name
                                    );
                                }
                            }
                            prevQuestions = currentQuestionGroup.GetAllQuestions(amsgs, adc).Where(question => !question.IsDeleted).ToList();
                            formQuestion.ForEach(newQuestion =>
                            {
                                if(newQuestion.IsDeleted)
                                    SessionVariables.WebSession.LogActivity(
                                            account: currentAccount,
                                            actor: currentUser,
                                            actionType: WebSessionActionType.Deleted,
                                            objectType: ObjectType.FormQuestion,
                                            objectId: newQuestion.ID,
                                            objectText: $"Question Name: {newQuestion.Text}",
                                            indirectObjectType: ObjectType.FormQuestionGroup,
                                            indirectObjectId: currentQuestionGroup.ID.Value,
                                            indirectObjectText: formQuestionGroup.Name
                                        );
                                var existingQuestion = prevQuestions.FirstOrDefault(question => question.ID == newQuestion.ID);
                                if (existingQuestion.IsNotEmpty())
                                {
                                    if (
                                        !existingQuestion.IsLocked &&
                                        (
                                            newQuestion.Text.IsNotEmpty() ||
                                            newQuestion.Description.IsNotEmpty() ||
                                            newQuestion.Options.IsNotEmpty() ||
                                            newQuestion.Link.IsNotEmpty() ||
                                            existingQuestion.IsActive != newQuestion.IsActive ||
                                            existingQuestion.IsRequired != newQuestion.IsRequired ||
                                            existingQuestion.IsAnswerDescriptionEnabled != newQuestion.IsAnswerDescriptionEnabled
                                        )
                                        )
                                        newQuestion.UpdateDefaultFields(existingQuestion);

                                    if (existingQuestion.QuestionDataTypeId == QuestionDataType.NotSet)
                                        newQuestion.UpdateNonDefaultFields(existingQuestion);

                                    existingQuestion.LastModifiedBy = currentUser.ID;
                                    existingQuestion.LastModifiedDate = DateTime.UtcNow;
                                    if (existingQuestion.QuestionTypeId == QuestionType.TextBlock)
                                    {
                                        existingQuestion.Description = existingQuestion.Text;
                                        existingQuestion.Text = null;
                                    }
                                    amsgs.AddRange(existingQuestion.Save(adc));
                                }
                                else if (existingQuestion.IsEmpty())
                                {
                                    var newFormQuestion = new FormQuestion()
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
                                    if (newQuestion.QuestionTypeId == QuestionType.TextBlock)
                                    {
                                        newFormQuestion.Text = null;
                                        newFormQuestion.Description = newQuestion.Text;
                                    }
                                    amsgs.AddRange(newFormQuestion.Save(adc));
                                    if (amsgs.HasNoErrors())
                                    {
                                        SessionVariables.WebSession.LogActivity(
                                            account: currentAccount,
                                            actor: currentUser,
                                            actionType: WebSessionActionType.Added,
                                            objectType: ObjectType.FormQuestion,
                                            objectId: newFormQuestion.ID,
                                            objectText:$"Question Name: {newQuestion.Text}",
                                            indirectObjectType: ObjectType.FormQuestionGroup,
                                            indirectObjectId: currentQuestionGroup.ID.Value,
                                            indirectObjectText: formQuestionGroup.Name
                                        );
                                    }
                                    newQuestion.ID = newFormQuestion.ID??0;
                                }
                                var prevSettingsList = FormQuestionSettings.LoadByQuestionIds(newQuestion.ID, amsgs, adc);
                                var newSettingsList = new List<FormQuestionSettings>();
                                if (prevSettingsList.Count > 0)
                                    newQuestion.QuestionSettings.ForEach(setting =>
                                    {
                                        if (!newQuestion.IsLocked)
                                        {
                                            var questionSetting = new FormQuestionSettings()
                                            {
                                                QuestionId = newQuestion.ID,
                                                Audience = setting.Audience,
                                                Show = setting.Show,
                                                Required = setting.Required,
                                                CreateDate = DateTime.UtcNow,
                                                LastModifiedDate = DateTime.UtcNow,
                                                CreatedBy = currentUser.ID,
                                                LastModifiedBy = currentUser.ID,
                                            };
                                            newSettingsList.Add(questionSetting);
                                        }
                                    });
                                if (prevSettingsList.Count > 0)
                                {
                                    if (newSettingsList.IsNotEmpty() && newSettingsList.Any())
                                    {
                                        prevSettingsList
                                            .Where(previousQuestionSetting => prevSettingsList.Any(newQuestionSetting => newQuestionSetting.Audience == previousQuestionSetting.Audience))
                                            .ToList()
                                            .ForEach(previousQuestionSetting =>
                                            {
                                                var newQuestionSetting = newSettingsList.First(x => x.Audience == previousQuestionSetting.Audience);
                                                if (previousQuestionSetting.Show != newQuestionSetting.Show)
                                                    SessionVariables.WebSession.LogActivity(
                                                            account: currentAccount,
                                                            actor: currentUser,
                                                            actionType: WebSessionActionType.Updated,
                                                            objectType: ObjectType.FormQuestion,
                                                            objectId: newQuestion.ID,
                                                            objectText: $" Having ID: {newQuestion.ID} -> Show: {newQuestionSetting.Show} FOR Audience: {newQuestionSetting.Audience}",
                                                            indirectObjectType: ObjectType.FormQuestionGroup,
                                                            indirectObjectId: currentQuestionGroup.ID.Value,
                                                            indirectObjectText: formQuestionGroup.Name
                                                        ); 
                                                if (previousQuestionSetting.Required != newQuestionSetting.Required)
                                                    SessionVariables.WebSession.LogActivity(
                                                            account: currentAccount,
                                                            actor: currentUser,
                                                            actionType: WebSessionActionType.Updated,
                                                            objectType: ObjectType.FormQuestion,
                                                            objectId: newQuestion.ID,
                                                            objectText: $" Having ID: {newQuestion.ID} -> Required: {newQuestionSetting.Required} FOR Audience: {newQuestionSetting.Audience}",
                                                            indirectObjectType: ObjectType.FormQuestionGroup,
                                                            indirectObjectId: currentQuestionGroup.ID.Value,
                                                            indirectObjectText: formQuestionGroup.Name
                                                        );
                                            });
                                    }
                                }
                                prevSettingsList.ForEach(setting => amsgs.AddRange(setting.Delete(adc)));
                                newSettingsList.ForEach(setting =>
                                {
                                    amsgs.AddRange(setting.Save(adc));
                                });
                            });
                        }
                        else
                            throw new Exception("Section does not exist");
                    }
                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs));
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error);
            }
            if (saveQuestion ?? false)
                return GetFormQuestions(formId ?? 0,formQuestionGroup.Id);
            
            return Json(
                        new ApiResponse(msgs, null),
                        JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveNewSection(FormQuestionGroupDto formQuestionGroup)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var currentUser = SessionVariables.CurrentUser;
            FormQuestionGroupDto newSection = null;
            try
            {
                if (currentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");
                if (formQuestionGroup.IsEmpty())
                    throw new Exception("Invalid Question Group");
                if (formQuestionGroup.FormId.IsEmpty())
                    throw new Exception("Invalid Form Id");
                
                Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {
                    var requestedForm = currentAccount.GetForm(formQuestionGroup.FormId ?? 0, amsgs, adc);
                    if (requestedForm.IsEmpty())
                        throw new Exception("Invalid Form Id");
                    if (formQuestionGroup.IsNotNull() && formQuestionGroup.IsValid())
                    {
                        var questionGroupExists = FormQuestionGroup.Exists(criterias:
                            new Hashtable{
                                {"Name", formQuestionGroup.Name},
                                {"FormId", requestedForm.ID},
                                {"IsDeleted", false},
                            },
                            msgs: amsgs,
                            dc: adc);
                        if (!questionGroupExists)
                        {
                            var newQuestionGroup = new FormQuestionGroup()
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = currentUser.ID,
                                LastModifiedDate = DateTime.UtcNow,
                                LastModifiedBy = currentUser.ID,
                                DisplayOrder = requestedForm.GetQuestionGroupCount(amsgs, adc) + 1,
                                FormId = requestedForm.ID,
                                QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                                RegistrantTypeId = RegistrantType.Individual,
                                Name = formQuestionGroup.Name,
                                IsDeleted = false,
                                IsActive = true,
                            };
                            amsgs.AddRange(newQuestionGroup.Save(adc));
                            if (amsgs.HasNoErrors())
                            {
                                newSection = FormQuestionGroupDto.MapToDto(newQuestionGroup);
                                SessionVariables.WebSession.LogActivity(
                                    account: currentAccount,
                                    actor: currentUser,
                                    actionType: WebSessionActionType.Added,
                                    objectType: ObjectType.FormQuestionGroup,
                                    objectId: newQuestionGroup.ID.Value,
                                    objectText: formQuestionGroup.Name,
                                    indirectObjectType: ObjectType.Form,
                                    indirectObjectId: requestedForm.ID.Value,
                                    indirectObjectText: requestedForm.Name
                                );
                            }
                                
                        }
                        else
                            throw new Exception("Section with same name already exists");
                    }
                    else
                    {
                        if (formQuestionGroup.IsNull())
                            throw new Exception("Section is empty");
                        else
                            throw new Exception("Section is not valid");
                    }

                },
                onSuccess: (SystemMessages smsgs) => msgs.AddRange(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(emsgs));
            }
            catch (Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error);
            }
            return Json(
                        new ApiResponse(msgs, newSection),
                        JsonRequestBehavior.AllowGet);
        }
    }
}