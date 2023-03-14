using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using AfterSchoolLibrary.Services;
using BasicLibrary;
using BasicLibrary.DataAccess;
using BCrypt.Net;

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
            var availableTags = AvailableTag.LoadByAccountIdByObjectType(currentAccount.ID.Value ,objectType, msgs);
            
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

                if (AvailableTag.ExistsByTag(tag.Trim(), objectType, currentAccount.ID.Value, msgs))
                    msgs.Add("Tag should be unique", MessageType.UserError);
            }
            else
            {
                availableTag = AvailableTag.Load(Id.Value, msgs);
            }

            if (msgs.HasNoErrors())
            {
                if (availableTag.ID.IsEmpty())
                {
                    availableTag.Tag = tag;
                    availableTag.AccountId = currentAccount.ID.Value;
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
            var availableTags = AvailableTag.LoadByAccountIdByObjectType(currentAccount.ID.Value ,(int) ObjectType.Account, msgs);
            
            ViewBag.Messages = msgs;
            ViewBag.AvailableTags = availableTags;
            
            return View(schools);
        }
        
        public ActionResult Activity(string pn, string rpp, int dateOrderType = 0, SystemMessages msgs = null)
        {
            var pageNumber = 1;
            var recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
                pageNumber = pn.ToInteger();

            if (rpp.IsNumber() && rpp.ToInteger() > 0)
                recordsPerPage = rpp.ToInteger();

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;

            var currentAccount = SessionVariables.CurrentAccount;
            var availableTags = AvailableTag.LoadByAccountIdByObjectType(currentAccount.ID.Value ,(int) ObjectType.Activity, msgs);
            //var schools = currentAccount.GetVisitorAccounts(new List<AccountType>() {AccountType.School} , msgs);
            var count = AfterSchoolLibrary.Models.Activity.GetCountByDistrict(currentAccount, msgs);
            var allActivities  = AfterSchoolLibrary.Models.Activity.LoadByDistrict(currentAccount, dateOrderType, start, length, msgs);
            /*List<Activity> allActivities = new List<Activity>();

            if (dateOrderType == 0)
            {
                allActivities = AfterSchoolLibrary.Models.Activity.LoadByDistrict(currentAccount, dateOrderType, start, length, msgs);
                ViewBag.SelectedDateOrderType = dateOrderType;
            }

            if (dateOrderType == 1)
            {
                 allActivities = AfterSchoolLibrary.Models.Activity.LoadByAccountIds(schools, start, length, OrderByDirectionEnum.Ascending, msgs);
                ViewBag.SelectedDateOrderType = dateOrderType;
            }*/

            var totalPages = Math.Ceiling(count.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Messages = msgs;
            ViewBag.AvailableTags = availableTags;
            ViewBag.SelectedDateOrderType = dateOrderType;

            ViewBag.PagingModel = new PagingModel
            {
                TotalCount = count,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages,
            };

            return View(allActivities);
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
                var assignedTag = AssignedTag.LoadByObjectIdByObjectTypeByTag(objectID, objectType,tag, msgs);
                msgs.AddRange(assignedTag.Delete());
            }

            return true;
        }
        
    }
}