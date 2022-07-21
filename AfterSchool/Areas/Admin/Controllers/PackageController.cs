using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using BasicLibrary.DataAccess;
using AfterSchoolLibrary;
using AfterSchool.Models;
using AfterSchoolLibrary.Models;
using System.Data;
using AfterSchoolLibrary.Services;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class PackageController : BaseLoggedInController
    {
        public PackageController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        // GET: Admin/Package
        [HttpGet]
        public ActionResult Index(int? start = null, int? length = null)
        {
            return View(Package.Load(start: start, length: length));
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.Messages = new SystemMessages();
            if (id.IsEmpty())
            {
                ViewBag.Features = new List<PackageFeature>();
                return View(new Package() { Name = "New Package" });
            }
            else
            {
                var package = Package.Load(id.ToInteger());
                ViewBag.Features = package.GetFeatures();
                return View(package);
            }
        }

        [HttpPost]
        public ActionResult Edit(string id, string name, string price, string priceperyear, string isactive, string ispopular, string isafterschool, string description, string isdefaultcustomerpackage)
        {
            SystemMessages msgs = new SystemMessages();
            var package = new Package();
            var features = new List<PackageFeature>();

            if (name.IsEmpty())
                msgs.Add("Package name is required", MessageType.UserError);
                
            if (price.IsEmpty() || !price.IsNumber())
                msgs.Add("Price / Month is required", MessageType.UserError);
            else if (price.ToDecimal() < 0)
                msgs.Add("Price / Month must be greater than or equal to 0", MessageType.UserError);
            
            if (priceperyear.IsEmpty() || !priceperyear.IsNumber())
                msgs.Add("Price / Year is required", MessageType.UserError);
            else if (price.ToDecimal() < 0)
                msgs.Add("Price / Year must be greater than or equal to 0", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction(
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var currentUser = SessionVariables.CurrentUser;
                        
                        if (id.IsEmpty())
                        {
                            package = new Package();
                            package.CreateDate = DateTime.UtcNow;
                            package.CreatedBy = currentUser.ID;
                        }
                        else
                        {
                            package = Package.Load(id.ToInteger(), msgs: amsgs, dc: adc);
                            features = package.GetFeatures(amsgs, adc);
                        }

                        package.Price = price.ToDecimal();
                        package.PricePerYear = priceperyear.ToDecimal();

                        package.LastModifiedDate = DateTime.UtcNow;
                        package.LastModifiedBy = currentUser.ID;

                        package.Name = name;
                        package.IsActive = isactive.IsNotEmpty();
                        package.IsPopular = ispopular.IsNotEmpty();
                        package.IsAfterSchool = isafterschool.IsNotEmpty();
                        package.Description = description;
                        amsgs.AddRange(package.SaveWithStripe(adc));

                        if (isdefaultcustomerpackage.IsBoolean() && isdefaultcustomerpackage.ToBoolean())
                            package.SetAsDefaultCustomerPackage(currentUser, amsgs, adc);
                        
                        if (amsgs.HasNoErrors())
                        {
                            foreach (var type in Enum.GetValues(typeof(FeatureType)).Cast<FeatureType>().Where(x => x != FeatureType.NotSet))
                            {
                                if (Request[type.ToString()].IsNotEmpty())
                                {
                                    var feature = PackageFeature.LoadByPackageIdByFeatureTypeId(package.ID.Value, type, amsgs, adc);
                                    if (feature.IsEmpty())
                                    {
                                        feature = new PackageFeature();
                                        feature.PackageId = package.ID;
                                        feature.CreateDate = DateTime.UtcNow;
                                        feature.CreatedBy = currentUser.ID;
                                        feature.FeatureTypeId = type;
                                    }

                                    feature.Max = Request[type.ToString()].ToDecimal();
                                    feature.LastModifiedBy = currentUser.ID;
                                    feature.LastModifiedDate = DateTime.UtcNow;
                                    amsgs.AddRange(feature.Save(adc));
                                }
                            }
                        }
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        msgs.AddRange(smsgs);

                        features = package.GetFeatures(msgs);
                        msgs.Add("Package Saved", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }

            ViewBag.Features = features;
            ViewBag.Messages = msgs;
            return View(package);
        }

        public ActionResult ResetFeature(int packageId, int featureTypeId, decimal featureValue, string accountTypeIds, bool resetIfAcctValueGreaterThan)
        {
            var msgs = new SystemMessages();
            var package = Package.Load(packageId, msgs);
            var featureType = featureTypeId.ToEnum<FeatureType>();
            var packageFeature = package.GetFeature(featureType, msgs);
            var accountTypeIdList = accountTypeIds.Split(',').Select(x => (AccountType)x.ToInteger()).ToList();
            var currentUser = SessionVariables.CurrentUser;

            if (packageFeature.IsNotEmpty()) {
                packageFeature.Max = featureValue;
                packageFeature.LastModifiedBy = currentUser.ID.Value;
                packageFeature.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(packageFeature.Save());
            }
            else
                packageFeature = PackageFeatureService.AddPackageFeature(packageId, featureTypeId.ToEnum<FeatureType>(), featureValue, currentUser.ID.Value, msgs);

            packageFeature.ResetAllAccountFeaturesToPackageFeature(
                accountTypeIdList, 
                resetIfAcctValueGreaterThan, 
                currentUser, 
                msgs);

            var features = package.GetFeatures(msgs);

            msgs.Add("Package Saved and account features have been reset", MessageType.Success);

            ViewBag.Features = features;
            ViewBag.Messages = msgs;

            return View("edit", package);
        }

        [HttpGet]
        public ActionResult Logs()
        {
            var msgs = new SystemMessages();
            var logDataTable = WebSession.GetLogs(msgs: msgs);
            List<LogViewModel> ListResponse = new List<LogViewModel>();

            if (logDataTable.IsNotEmpty() && logDataTable.Rows.Any())
            {                
                foreach (DataRow row in logDataTable.Rows)
                {
                    if (row.IsNotEmpty())
                    {
                        var response = new LogViewModel();
                        response.IPAddress = row["IPAddress"].IsNotEmpty() ? row["IPAddress"].ToString() : "";
                        response.UrlReferrer = row["UrlReferrer"].IsNotEmpty() ? row["UrlReferrer"].ToString() : "";
                        response.ActionString = row["ActionString"].IsNotEmpty() ? row["ActionString"].ToString() : "";
                        response.ActionStringFormat = row["ActionStringFormat"].IsNotEmpty() ? row["ActionStringFormat"].ToString() : "";

                        ListResponse.Add(response);
                    }
                }
            }

            return View(ListResponse);
        }
    }
}