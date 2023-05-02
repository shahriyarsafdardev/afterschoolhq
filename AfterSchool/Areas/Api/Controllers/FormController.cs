using AfterSchool.Areas.Api.Models;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Interop;

namespace AfterSchool.Areas.Api.Controllers
{
    public class FormController : Controller
    {
        [HttpGet]
        public ActionResult getFormData(string formId)
        {
            if (formId.IsEmpty())
                return HttpNotFound();
            var msgs = new SystemMessages();
            try
            {
                var currentUser = SessionVariables.CurrentUser;
                var currentAccount = SessionVariables.CurrentAccount;
                var form = currentAccount.GetForm(formId.ToInteger(), msgs);
                var questions = form.GetAllQuestions(msgs);
                var questionGroups = form.GetQuestionGroups(msgs);
                var formQuestionGroups = new List<FormQuestionGroupDto>();
                var formQuestions = new List<FormQuestionDto>();
                foreach (var questionGroup in questionGroups)
                {
                    formQuestionGroups.Add(FormQuestionGroupDto.byQuestionGroup(questionGroup));
                }
                foreach (var question in questions)
                {
                    formQuestions.Add(FormQuestionDto.byFormQuestion(question));
                }
                var response = new
                {
                    questions = formQuestions,
                    questionGroups = formQuestionGroups
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
    }
}