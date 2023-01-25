using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;

namespace AfterSchool.Areas.District.Controllers
{
    public class TagController : BaseLoggedInController
    {
        
        public TagController() : base(new List<AccountType> { AccountType.SchoolDistrict })
        {

        }
        // GET
        public ActionResult Index(int objectType = (int)ObjectType.Account)
        {
            var currentAccount = SessionVariables.CurrentAccount;
            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            
            var availableTags = AvailableTagService.GetByObjectType(objectType, currentAccount, msgs);
            
            ViewBag.Messages = msgs;
            ViewBag.ObjectType = objectType;
            
            return View(availableTags);
        }

        public ActionResult AddRemoveTag(string tag , int objectType , int? Id)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;

            if (Id.IsEmpty())
            {
                if(tag.IsEmpty())
                    msgs.Add("Tag name should not be empty", MessageType.UserError);
            
                if(AvailableTagService.UniqueTag(tag.Trim(), objectType, msgs))
                    msgs.Add("Tag should be unique", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                var availableTag = AvailableTagService.GetById(Id, msgs);

                if (availableTag.IsEmpty())
                    AvailableTagService.Save(objectType, tag, currentAccount, msgs);
                else
                    AvailableTagService.Delete(availableTag, msgs);
            }
            
            HandleTempMessages(msgs);
            
            return RedirectToAction("Index" , new {objectType});
        }
        
        public ActionResult School()
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var schools = currentAccount.GetVisitorAccounts(new List<AccountType>() {AccountType.School} , msgs);
            var availableTags = AvailableTagService.GetByObjectType((int) ObjectType.Account, currentAccount, msgs);

            ViewBag.Messages = msgs;
            ViewBag.AvailableTags = availableTags;
            
            return View(schools);
        }
        
        public ActionResult Activity()
        {
            return View();
        }

        public bool AddRemoveAssignedTag(string tag, int objectID, int objectType, string operation)
        {
            var msgs = new SystemMessages();
            
            if (operation == "add")
            {
                msgs.AddRange(AssignedTagService.Save(tag, objectID, objectType, msgs));
            }
            
            if (operation == "remove")
            {
                AssignedTagService.Remove(tag, objectID, objectType, msgs);
            }

            return true;
        }
        
    }
}