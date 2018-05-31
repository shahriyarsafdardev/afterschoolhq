using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class AccountController : BaseLoggedInController
    {
        // GET: Account
        [HttpGet]
        public ActionResult Index(int? start = null, int? length = null)
        {
            return View(Account.Load(
                criterias: new System.Collections.Hashtable() { { "IsAdminAccount", false } }, 
                start: start, 
                length: length));
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();
            if (id.IsEmpty())
            {
                return View(new Account() { Name = "New Account" });
            }
            else
            {
                Account a = Account.Load(id.ToInteger());
                ViewBag.AccountPackage = a.GetActiveAccountPackage();
                return View(a);
            }
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string phone, string address1, string address2, string city, string state, string postalcode, string country, string isactive)
        {
            SystemMessages msgs = new SystemMessages();
            Account a = null;

            if (name.IsEmpty())
            {
                msgs.Add("Organization name is required", MessageType.UserError);
            }
            if (address1.IsEmpty())
            {
                msgs.Add("Street Address is required", MessageType.UserError);
            }
            if (city.IsEmpty())
            {
                msgs.Add("City is required", MessageType.UserError);
            }
            if (state.IsEmpty())
            {
                msgs.Add("State is required", MessageType.UserError);
            }
            if (postalcode.IsEmpty())
            {
                msgs.Add("Zip Code is required", MessageType.UserError);
            }

            if (msgs.HasNoErrors())
            {
                if (id.IsEmpty())
                {
                    a = new Account();
                    a.CreateDate = DateTime.UtcNow;
                    a.CreatedBy = SessionVariables.LoggedInUser.ID;
                }
                else
                {
                    a = Account.Load(id.ToInteger());
                }

                a.LastModifiedDate = DateTime.UtcNow;
                a.LastModifiedBy = SessionVariables.LoggedInUser.ID;

                a.Name = name;
                a.Phone = phone;
                a.Address1 = address1;
                a.Address2 = address2;
                a.City = city;
                a.State = state;
                a.PostalCode = postalcode;
                a.UniqueName = AccountService.GenerateUniqueName(name, a.ID);
                a.IsAdminAccount = false;
                a.Country = country;
                a.IsActive = !isactive.IsEmpty();
                msgs.AddRange(a.SaveWithStripe());
                    
                if(msgs.HasNoErrors())
                {
                    msgs.Add("Saved Successfully", MessageType.Success);
                }
            }

            ViewBag.Messages = msgs;
            ViewBag.AccountPackage = a.GetActiveAccountPackage();
            return View(a);
        }

        [HttpGet]
        public ActionResult Detail(string id)
        {            
            Account a = Account.Load(id.ToInteger());
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = a.GetActiveAccountPackage();
            return View(a);
        }

        [HttpGet]
        public ActionResult Billing(string id)
        {
            Account a = Account.Load(id.ToInteger());
            ViewBag.BillingHistory = a.GetStripeBillingHistory();
            ViewBag.RefundHistory = a.GetStripeRefundHistory();
            ViewBag.Cards = a.GetStripeCards();
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpPost]
        public ActionResult AddCard(string id, string name, string number, string cvc, string expmonth, string expyear, string makeprimary)
        {
            SystemMessages msgs = new SystemMessages();

            Account a = Account.Load(id.ToInteger());
            msgs.AddRange(a.AddCreditCard(name, number, expmonth, expyear, cvc, !makeprimary.IsEmpty()));
            ViewBag.Cards = a.GetStripeCards();
            ViewBag.Messages = msgs;
            return View("Billing", a);
        }

        [HttpGet]
        public ActionResult Packages(string id)
        {
            Account a = Account.Load(id.ToInteger());
            ViewBag.Packages = Package.LoadByIsActive(true);
            ViewBag.AccountPackage = a.GetActiveAccountPackage();
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpGet]
        public ActionResult SelectPackage(string id, string package)
        {
            SystemMessages msgs = new SystemMessages();

            Account a = Account.Load(id.ToInteger());
            a.AddPackage(package.ToInteger(), BillingPeriodType.Month, SessionVariables.LoggedInUser, msgs);
            ViewBag.Packages = Package.LoadByIsActive(true);
            ViewBag.AccountPackage = a.GetActiveAccountPackage();
            ViewBag.Messages = msgs;
            return View("Packages", a);
        }

        [HttpGet]
        public ActionResult Stripe(string id)
        {
            Account a = Account.Load(id.ToInteger());
            ViewBag.Messages = new SystemMessages();
            return View(a);
        }

        [HttpGet]
        public ActionResult Team(string id)
        {
            Account a = Account.Load(id.ToInteger());
            ViewBag.Messages = new SystemMessages();
            ViewBag.AccountPackage = a.GetActiveAccountPackage();
            ViewBag.Members = a.GetMembers().Select(x => x.ToWebUser()).ToList();
            return View(a);
        }
    }
}