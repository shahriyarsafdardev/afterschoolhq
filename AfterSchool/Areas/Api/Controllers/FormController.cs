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
using System.Windows.Interop;

namespace AfterSchool.Areas.Api.Controllers
{
    public class FormController : Controller
    {

        [HttpGet]
        public ActionResult GetFormData(int FormId)
        {
            if (FormId.IsEmpty())
                return HttpNotFound();
            var msgs = new SystemMessages();
            try
            {
                var CurrentAccount = SessionVariables.CurrentAccount;
                if (CurrentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var RequestedForm = CurrentAccount.GetForm(FormId, msgs);

                if (RequestedForm.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var FormSettings = FormSettingsDto.MapToDto(RequestedForm);

                var QuestionGroups = RequestedForm.GetQuestionGroups(msgs)
                    .Where(
                        QuestionGroup =>
                        QuestionGroup.RegistrantTypeId == RegistrantType.Individual &&
                        !QuestionGroup.IsDeleted
                        )
                    .OrderBy(
                        QuestionGroup => QuestionGroup
                        .DisplayOrder
                        )
                    .ToList();

                var FormQuestionGroups = FormQuestionGroupDto.MapToDtoList(QuestionGroups);

                var Response = new
                {
                    FormQuestionGroup = FormQuestionGroups,
                    Form = FormSettings
                };

                return Json(
                    new ApiResponse(msgs, Response),
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
        public ActionResult GetFormQuestions(int FormId, int SectionId)
        {

            var msgs = new SystemMessages();
            try
            {
                if (FormId.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var CurrentAccount = SessionVariables.CurrentAccount;

                if (CurrentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                var RequestedForm = CurrentAccount.GetForm(FormId, msgs);

                if (RequestedForm.IsEmpty())
                    throw new Exception("Invalid Form Id");

                if (SectionId.IsEmpty())
                    throw new Exception("Invalid Section Id");

                var QuestionGroup = RequestedForm.GetQuestionGroups()
                    .Where(QuestionGroup =>
                    QuestionGroup.ID
                    .Equals(SectionId))
                    .FirstOrDefault();
                if (QuestionGroup.IsEmpty())
                    throw new Exception("Invalid Section Id");

                var RequestedQuestions = RequestedForm.GetAllQuestions(msgs)
                    .Where(
                        Question =>
                        Question.FormQuestionGroupId.Equals(SectionId) &&
                        !Question.IsDeleted
                    )
                    .OrderBy(
                        Question => Question.DisplayOrder
                    )
                    .ToList();

                var QuestionsToSend = FormQuestionDto.MapToDtoList(RequestedQuestions);

                var response = new
                {
                    FormQuestion = QuestionsToSend,
                    SectionName = QuestionGroup.Name
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
        public ActionResult SaveFormData(FormSettingsDto FormSettings, FormQuestionGroupDto FormQuestionGroup, List<FormQuestionDto> FormQuestion)
        {
  
            var msgs = new SystemMessages();
            var CurrentAccount = SessionVariables.CurrentAccount;
            var CurrentUser = SessionVariables.CurrentUser;
            var FormId = FormSettings.FormId;
            try
            {

                if (CurrentAccount.IsEmpty())
                    throw new Exception("Unauthorized Account");

                if (FormId.IsEmpty())
                    throw new Exception("Invalid Form Id");

                var RequestedForm = CurrentAccount.GetForm(FormId??0, msgs);

                if (RequestedForm.IsEmpty())
                    throw new Exception("Invalid Form Id");

                Database.ExecuteTransaction(
                action: (SystemMessages amsgs, DatabaseConnection adc) =>
                {

                    FormSettings.UpdateIfNotNull(RequestedForm);
                    amsgs.AddRange(RequestedForm.Save(adc));

                    if (FormQuestionGroup.IsNotNull())
                    {
                        var QuestionGroupExists = AfterSchoolLibrary.Models.FormQuestionGroup.Exists(criterias:
                            new Hashtable{
                            {"Name", FormQuestionGroup.Name},
                            {"FormId", RequestedForm.ID},
                            {"IsDeleted", false}
                            },
                            msgs: amsgs,
                            dc: adc);

                        FormQuestionGroup CurrentQuestionGroup;
                        List<FormQuestion> PrevQuestions;

                        if (!QuestionGroupExists)
                        {

                            CurrentQuestionGroup = new FormQuestionGroup()
                            {
                                CreateDate = DateTime.UtcNow,
                                CreatedBy = CurrentUser.ID,
                                LastModifiedDate = DateTime.UtcNow,
                                LastModifiedBy = CurrentUser.ID,
                                DisplayOrder = RequestedForm.GetQuestionGroupCount(amsgs, adc) + 1,
                                FormId = RequestedForm.ID,
                                QuestionGroupDataTypeId = QuestionGroupDataType.NotSet,
                                RegistrantTypeId = RegistrantType.Individual,
                                Name = FormQuestionGroup.Name,
                                IsDeleted = false,
                                IsActive = true,
                            };

                            amsgs.AddRange(CurrentQuestionGroup.Save(adc));
                            PrevQuestions = new List<FormQuestion>();

                        }
                        else
                        {

                            CurrentQuestionGroup = AfterSchoolLibrary.Models.FormQuestionGroup
                                    .Load(criterias: new Hashtable{
                                    {"Name", FormQuestionGroup.Name},
                                    {"FormId", RequestedForm.ID},
                                    {"IsDeleted", false}
                                    },
                                    msgs: amsgs, dc: adc)?.FirstOrDefault();

                            if (CurrentQuestionGroup.IsEmpty())
                                throw new Exception("Invalid Section Name");

                            if (CurrentQuestionGroup.QuestionGroupDataTypeId == QuestionGroupDataType.NotSet)
                            {
                                CurrentQuestionGroup.Name = FormQuestionGroup.Name;
                                CurrentQuestionGroup.LastModifiedBy = CurrentUser.ID;
                                CurrentQuestionGroup.LastModifiedDate = DateTime.UtcNow;
                                amsgs.AddRange(CurrentQuestionGroup.Save(adc));
                            }

                            PrevQuestions = CurrentQuestionGroup.GetAllQuestions(amsgs, adc).Where(Question=>!Question.IsDeleted).ToList();
                            FormQuestion.ForEach(NewQuestion =>
                            {
                                var ExistingQuestion = PrevQuestions.Where(question => question.ID == NewQuestion.ID).FirstOrDefault();

                                if (ExistingQuestion.IsNotEmpty())
                                {
                                    NewQuestion.UpdateQuestionIfNotEmpty(ExistingQuestion);
                                    ExistingQuestion.LastModifiedBy = CurrentUser.ID;
                                    ExistingQuestion.LastModifiedDate = DateTime.UtcNow;
                                    msgs.AddRange(ExistingQuestion.Save(adc));
                                }
                                else if (ExistingQuestion.IsEmpty())
                                {

                                    var NewFormQuestion = new FormQuestion()
                                    {
                                        FormId = FormId,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                        CreatedBy = CurrentUser.ID,
                                        LastModifiedBy = CurrentUser.ID,
                                        DisplayOrder = NewQuestion.DisplayOrder,
                                        FormQuestionGroupId = NewQuestion.FormQuestionGroupId,
                                        IsTemplate = false,
                                        QuestionDataTypeId = QuestionDataType.NotSet,
                                        QuestionTypeId = NewQuestion.QuestionTypeId,
                                        Text = NewQuestion.Text,
                                        Options = NewQuestion.Options,
                                        Link = NewQuestion.Link,
                                        Description = NewQuestion.Description,
                                        IsActive = true,
                                        IsDeleted = false,
                                        IsRequired = NewQuestion.IsRequired,
                                        IsAnswerDescriptionEnabled = NewQuestion.IsAnswerDescriptionEnabled,
                                    };
                                    msgs.AddRange(NewFormQuestion.Save(adc));
                                }

                                var PrevSettingsList = FormQuestionSettings.LoadByQuestionIds(NewQuestion.ID, msgs,adc);

                                if (PrevSettingsList.Count > 0)
                                    PrevSettingsList.ForEach(Setting => msgs.AddRange(Setting.Delete(adc)));

                                NewQuestion.QuestionSettings.ForEach(Setting =>
                                {
                                    var questionSetting = new FormQuestionSettings()
                                    {
                                        QuestionId = NewQuestion.ID,
                                        Audience = Setting.Audience,
                                        Show = Setting.Show,
                                        Required = Setting.Required,
                                        CreateDate = DateTime.UtcNow,
                                        LastModifiedDate = DateTime.UtcNow,
                                        CreatedBy = CurrentUser.ID,
                                        LastModifiedBy = CurrentUser.ID,
                                    };
                                    msgs.AddRange(questionSetting.Save(adc));
                                });

                            });
                        }

                    }
                },
                onSuccess: (SystemMessages smsgs) => AfterSchoolLibrary.Helpers.Utilities.SystemErrorLog(smsgs),
                onFailure: (SystemMessages fmsgs) => msgs.AddRange(fmsgs),
                onException: (SystemMessages emsgs) => msgs.AddRange(emsgs));
                
            }catch(Exception ex)
            {
                msgs.Add(ex.Message, MessageType.Error);   
            }
            return Json(
                        new ApiResponse(msgs, null),
                        JsonRequestBehavior.AllowGet);
        }
    }
}