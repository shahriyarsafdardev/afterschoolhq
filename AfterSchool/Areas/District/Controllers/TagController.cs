using System;
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
            var availableTag = new AvailableTag();

            if (Id.IsEmpty())
            {
                if (tag.IsEmpty())
                    msgs.Add("Tag name should not be empty", MessageType.UserError);

                if (AvailableTagService.IsUniqueTag(tag.Trim(), objectType, currentAccount, msgs))
                    msgs.Add("Tag should be unique", MessageType.UserError);
            }
            else
            {
                availableTag = AvailableTag.Load((int)Id, msgs);
            }

            if (msgs.HasNoErrors())
            {
                if (availableTag.ID.IsEmpty())
                {
                    availableTag.Tag = tag;
                    availableTag.AccountId = (int)currentAccount.ID;
                    availableTag.ObjectType = (ObjectType)objectType;
                    availableTag.IsDeleted = false;
                    availableTag.CreateDate = DateTime.UtcNow;
                    availableTag.CreatedBy = currentAccount.ID;
                    msgs.AddRange(availableTag.Save());
                }
                else
                {
                    availableTag.IsDeleted = true;
            
                    msgs.AddRange(availableTag.Save());
                }
            }
            
            SetTempMessages(msgs);
            
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
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            var availableTags = AvailableTagService.GetByObjectType((int) ObjectType.Activity, currentAccount, msgs);
            var schools = currentAccount.GetVisitorAccounts(new List<AccountType>() {AccountType.School} , msgs);
            var activities = new List<Activity>();
            foreach (var school in schools)
            {
                 var schoolActivities = school.GetAllActivitiesForAccount(msgs);
                 activities.AddRange(schoolActivities);
            }
            
            ViewBag.Messages = msgs;
            ViewBag.AvailableTags = availableTags;
            
            return View(activities);
        }

        public bool AddRemoveAssignedTag(string tag, int objectID, int objectType, string operation)
        {
            var msgs = new SystemMessages();
            var currentAccount = SessionVariables.CurrentAccount;
            
            if (operation == "add")
            {
                var assignedTag = new AssignedTag();
            
                assignedTag.Tag = tag;
                assignedTag.ObjectId = objectID;
                assignedTag.ObjectType = (ObjectType)objectType;
                assignedTag.CreateDate = DateTime.UtcNow;
                assignedTag.CreatedBy = currentAccount.ID;
            
                msgs.AddRange(assignedTag.Save());
            }
            
            if (operation == "remove")
            {
                AssignedTag.Delete(tag, objectID, objectType, msgs);
            }

            return true;
        }
        
    }
}