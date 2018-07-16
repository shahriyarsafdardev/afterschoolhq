using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Linq;
using System.Collections.Specialized;
using AfterSchool.Models;
using System.Collections.Generic;

namespace AfterSchool.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        [HttpGet]
        public ActionResult GetStarted()
        {
            if (SessionVariables.LoggedInUser.IsNotEmpty() && SessionVariables.CurrentAccount.AccountTypeId == AccountType.Registrant)
                return RedirectToAction("select");

            var msgs = new SystemMessages();
            var administratorAccount = Account.LoadByAccountTypeId(AccountType.AfterSchool, msgs).First();
            var form = administratorAccount.GetDefaultForm(msgs);
            var parentQuestionGroup = form.GetQuestionGroups(msgs).Where(group => group.QuestionGroupDataTypeId == QuestionGroupDataType.Parent1Information).First();
            var questions = parentQuestionGroup.GetAllQuestions(msgs);

            ViewBag.Messages = msgs;
            ViewBag.Cart = SessionVariables.Cart;

            return View(questions.Select(question => new FormQuestionViewModel(question)).ToList());
        }

        [HttpPost]
        public ActionResult GetStarted(string formdata)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Select()
        {

            return View();
        }
    }
}