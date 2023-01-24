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

            var msgs = new SystemMessages();
            HandleTempMessages(msgs);
            
            var availableTags = AvailableTagService.GetByObjectType(objectType, msgs);
            
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
            return View();
        }
        
        public ActionResult Activity()
        {
            return View();
        }
    }
}