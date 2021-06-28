using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Specialized;
using AfterSchool.Models;
using System.Collections.Generic;
using AfterSchoolLibrary.Services;
using BasicLibrary.DataAccess;
using System;
using Hangfire;
using AfterSchoolLibrary.Models;

namespace AfterSchool.Areas.Admin.Controllers
{
    public class UploadController : BaseLoggedInController
    {
        public UploadController() : base(new List<AccountType> { AccountType.AfterSchool })
        {

        }

        // GET: Admin/Upload
        [HttpGet]
        public ActionResult Index()
        {
            AddContacts("1", null, null, null, null, null, null);
            ViewBag.Step = 1;
            ViewBag.ShowSubmit = false;
            return View();
        }

        [HttpPost]
        public ActionResult Index(string step, string importtype, string emaillist, string createlist, string listname, string importtext, string listId)
        {
            AddContacts(step, importtype, emaillist, createlist, listname, importtext, listId);
            ViewBag.Step = step.ToInteger();
            ViewBag.ShowSubmit = ViewBag.Messages.HasNoErrors();
            ViewBag.ImportFile = Request.Files[0];

            return View("LocationsSubmitted");
        }

        private void AddContacts(string step, string importtype, string emaillist, string createlist, string listname, string importtext, string listId)
        {
            var msgs = new SystemMessages();
            var csvMsgs = new SystemMessages();

            ViewBag.ContactLists = SessionVariables.CurrentAccount.GetContactLists();

            if (step == "1")
            {
                ViewBag.ImportText = "";
            }
            else if ((Request.Files.Count == 0 || Request.Files[0].ContentLength == 0 || !Request.Files[0].FileName.Contains(".csv")))
            {
                msgs.Add("Please upload a valid CSV file", MessageType.UserError);
            }
            else
            {
                var inviteMessage = Request["invite-message"];
                ViewBag.ImportText = ProcessImport(Request.Files[0], msgs);
                if (msgs.HasNoErrors())
                {
                    msgs.AddRange(AddLocations(ViewBag.Rows, ViewBag.Headers, inviteMessage, msgs));
                }
            }

            ViewBag.Messages = msgs;
        }

        private string[] schoolHeaders { get { return new string[] { "school", "schoolname", "name" }; } }
        private string[] gradeHeaders { get { return new string[] { "grades", "gradelevels" }; } }
        private string[] phoneHeaders { get { return new string[] { "phone", "phonenumber", "phoneno", "telephone" }; } }
        private string[] addressHeaders { get { return new string[] { "address", "addr", "streetaddress", "address1" }; } }
        private string[] aptSuiteHeaders { get { return new string[] { "address2", "apartmentno", "suite", "apt/suite" }; } }
        private string[] cityHeaders { get { return new string[] { "city", "town", "cityname" }; } }
        private string[] stateHeaders { get { return new string[] { "state", "statename" }; } }
        private string[] zipHeaders { get { return new string[] { "zip", "zipcode" }; } }
        private string[] websiteHeaders { get { return new string[] { "website", "site", "url", "webpage" }; } }
        private string[] firstNameHeaders { get { return new string[] { "firstname", "fname", "first" }; } }
        private string[] lastNameHeaders { get { return new string[] { "lastname", "lname", "last" }; } }
        private string[] emailHeaders { get { return new string[] { "email", "emailaddress" }; } }
        private string[] minGradeHeaders { get { return new string[] { "mingrade", "lowestgrade" }; } }
        private string[] maxGradeHeaders { get { return new string[] { "maxgrade", "highestgrade" }; } }
        private string[] accountIdHeaders { get { return new string[] { "accountid" }; } }


        private void AddHeaders(List<string[]> list)
        {
            list.Add(schoolHeaders);
            list.Add(gradeHeaders);
            list.Add(phoneHeaders);
            list.Add(addressHeaders);
            list.Add(cityHeaders);
            list.Add(stateHeaders);
            list.Add(zipHeaders);
            list.Add(websiteHeaders);
            list.Add(aptSuiteHeaders);
            list.Add(firstNameHeaders);
            list.Add(lastNameHeaders);
            list.Add(emailHeaders);
            list.Add(minGradeHeaders);
            list.Add(maxGradeHeaders);
            list.Add(accountIdHeaders);
        }

        private string ProcessImport(HttpPostedFileBase importfile, SystemMessages msgs)
        {
            var sr = new System.IO.StreamReader(importfile.InputStream);
            var extension = System.IO.Path.GetExtension(importfile.FileName);

            List<string[]> headerList = new List<string[]>();
            AddHeaders(headerList);

            List<string> headers = new List<string>();
            var headersWithData = new List<string>();
            var rows = new List<List<string>>();
            var validatedRows = new List<List<ImportField>>();

            if (extension == ".csv")
            {
                CsvHelper.CsvReader reader = new CsvHelper.CsvReader(sr);
                reader.Configuration.Delimiter = ",";
                reader.Configuration.HasHeaderRecord = true;
                reader.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
                reader.Read();
                reader.ReadHeader();
                while (reader.Read())
                {
                    if (headers.Count == 0)
                    {
                        headers = reader.Context.HeaderRecord.ToList();
                    }

                    var row = new List<string>();
                    for (var i = 0; i < headers.Count; i++)
                    {
                        var header = headers[i];
                        var field = reader.GetField<string>(i);
                        var headerNoSpacesEtc = header.Replace("-", "").Replace(" ", "").Replace("_", "").ToLower();

                        foreach (var headerName in headerList)
                        {
                            if (headerName.Contains(headerNoSpacesEtc))
                            {
                                row.Add(field);
                                if (!headersWithData.Contains(headerNoSpacesEtc))
                                {
                                    headersWithData.Add(headerNoSpacesEtc);
                                }
                            }
                        }
                    }

                    rows.Add(row);
                }
            }
            var hasErrors = false;

            if (headersWithData.Count == 0)
            {
                msgs.Add("You do not have any valid data in this CSV", MessageType.UserError);
            }
            else if (headersWithData.Where(x => schoolHeaders.Contains(x)).Count() == 0)
            {
                msgs.Add("Please add an school name to each record", MessageType.UserError);
            }

            foreach (var headerName in headerList)
            {
                if (headersWithData.Where(x => schoolHeaders.Contains(x)).Count() == 0)
                {
                    msgs.Add(String.Format("Please add {0} value to each record", headerName[0]), MessageType.UserError);
                }
            }


            if (msgs.HasNoErrors())
            {
                foreach (var row in rows)
                {
                    var validatedRow = new List<ImportField>();

                    for (var i = 0; i < headersWithData.Count; i++)
                    {
                        var validatedField = new ImportField();
                        validatedField.Value = row[i];

                        var field = row[i];
                        var headerName = headersWithData[i];

                        validatedField.Value = field;

                        if (field.Length > 150)
                        {
                            validatedField.Messages.Add("Must be less than or equal to 150 characters. Data will be truncated.", MessageType.Warning);
                        }

                        if (phoneHeaders.Contains(headerName))
                        {
                            if (field != "" && !field.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                            {
                                hasErrors = true;
                                msgs.Add(String.Format("In csv, line: {1} \n {0} is not a valid phone number", field, rows.IndexOf(row)), MessageType.UserError);
                            }
                        }
                        if (emailHeaders.Contains(headerName))
                        {
                            if (!field.IsBlank() && !field.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                            {
                                hasErrors = true;
                                msgs.Add(String.Format("In csv, line: {1} \n {0} is not a valid email", field, rows.IndexOf(row)), MessageType.UserError);
                            }
                        }

                        validatedRow.Add(validatedField);
                    }
                    validatedRows.Add(validatedRow);
                }
            }

            var csvString = new System.Text.StringBuilder();
            foreach (var header in headersWithData)
            {
                csvString.Append(header);
                csvString.Append(",");
            }
            csvString.Append("\n");

            foreach (var row in rows)
            {
                foreach (var field in row)
                {
                    csvString.Append(field);
                    csvString.Append(",");
                }
                csvString.Append("\n");
            }

            ViewBag.Headers = headersWithData;
            ViewBag.Rows = validatedRows;
            //ViewBag.Csv = csvString.ToString();

            if (hasErrors)
            {
                msgs.Add("You have errors in your CSV file", MessageType.UserError);
            }

            return csvString.ToString();
        }


        [HttpPost]
        public ActionResult SubmitLocations()
        {
            var msgs = new SystemMessages();
            var inviteMessage = Request["invite-message"];
            ProcessImport(Request.Files[0], msgs);
            msgs.AddRange(AddLocations(ViewBag.Rows, ViewBag.headers, inviteMessage, msgs));
            ViewBag.Messages = msgs;
            return View("LocationsSubmitted");
        }

        private int GetHeaderPostion(List<string> headersWithData, string[] headerToCheckFor)
        {
            for (int i = 0; i < headersWithData.Count; i++)
            {
                if (headerToCheckFor.Contains(headersWithData[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private SystemMessages AddLocations(List<List<ImportField>> rows, List<string> headersWithData, string inviteMessage, SystemMessages msgs)
        {
            //List<string[]> headerList = new List<string[]>();
            //AddHeaders(headerList);

            var schoolIndex = GetHeaderPostion(headersWithData, schoolHeaders);
            var addressIndex = GetHeaderPostion(headersWithData, addressHeaders);
            var address2Index = GetHeaderPostion(headersWithData, aptSuiteHeaders);
            var cityIndex = GetHeaderPostion(headersWithData, cityHeaders);
            var stateIndex = GetHeaderPostion(headersWithData, stateHeaders);
            var postalCodeIndex = GetHeaderPostion(headersWithData, zipHeaders);
            var phoneIndex = GetHeaderPostion(headersWithData, phoneHeaders);
            var firstNameIndex = GetHeaderPostion(headersWithData, firstNameHeaders);
            var lastNameIndex = GetHeaderPostion(headersWithData, lastNameHeaders);
            var emailIndex = GetHeaderPostion(headersWithData, emailHeaders);
            var accountIdIndex = GetHeaderPostion(headersWithData, accountIdHeaders);
            var minGradeIndex = GetHeaderPostion(headersWithData, minGradeHeaders);
            var maxGradeIndex = GetHeaderPostion(headersWithData, maxGradeHeaders);
            Account districtAccount = null;
            var allGradeLevels = GradeLevel.Load();

            if (AfterSchoolLibrary.Helpers.Utilities.IsProduction)
                districtAccount = Account.Load(84);
            else if (AfterSchoolLibrary.Helpers.Utilities.IsQA)
                districtAccount = Account.Load(193);
            // else
            //     districtAccount = Account.Load(80);

            var users = new List<WebUser>();
            var accounts = new List<Account>();

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
               (
                   action: (SystemMessages amsgs, DatabaseConnection adc) =>
                   {
                       foreach (var row in rows)
                       {
                           WebUser user = null;
                           Account account = null;
                           Account locationHostAccount = null;

                           var accountIdFromCSV = row.ElementAt(accountIdIndex).Value.IfEmpty("").Trim();
                           var schoolName = row.ElementAt(schoolIndex).Value.IfEmpty("").Trim();
                           var schoolPhone = row.ElementAt(phoneIndex).Value.IfEmpty("").Trim();
                           var address = row.ElementAt(addressIndex).Value.IfEmpty("").Trim();
                           var address2 = "";
                           if (address2Index > -1)
                           {
                               address2 = row.ElementAt(address2Index).Value.IfEmpty("").Trim();
                           }
                           var city = row.ElementAt(cityIndex).Value.IfEmpty("").Trim();
                           var state = row.ElementAt(stateIndex).Value.IfEmpty("").Trim();
                           var postalCode = row.ElementAt(postalCodeIndex).Value.IfEmpty("").Trim();
                           var country = "US";
                           var locationType = LocationType.School;
                           var firstname = row.ElementAt(firstNameIndex).Value.IfEmpty("").Trim();
                           var lastname = row.ElementAt(lastNameIndex).Value.IfEmpty("").Trim();
                           var email = row.ElementAt(emailIndex).Value.IfEmpty("").Trim();
                           var minGrade = row.ElementAt(minGradeIndex).Value.IfEmpty("").Trim();
                           var maxGrade = row.ElementAt(maxGradeIndex).Value.IfEmpty("").Trim();

                           if (accountIdFromCSV.IfEmpty("").Trim().Length > 0)
                           {
                               account = Account.Load(accountIdFromCSV.ToInteger(), amsgs, adc);
                           }

                           if (firstname.Length > 0)
                           {
                               if (account.IsEmpty())
                               {
                                   account = Account.Load(
                                       criterias: new System.Collections.Hashtable
                                       {
                                           { "Name", schoolName },
                                           { "Phone", schoolPhone }
                                       },
                                       msgs: amsgs,
                                       dc: adc
                                       ).FirstOrDefault();
                               }

                               if (user.IsEmpty())
                               {
                                   user = WebUser.LoadByEmail(email, amsgs, adc);
                               }

                               if (account.IsNotEmpty())
                               {
                                   if (user.IsEmpty())
                                   {
                                       user = WebUserService.InviteNew(
                                           firstname,
                                           lastname,
                                           email,
                                           new WebUser { ID = 0 },
                                           amsgs,
                                           adc);
                                   }

                                   account.AddUser(user, UserType.Owner, new WebUser(), amsgs, adc);
                               }
                               else
                               {
                                   if (user.IsEmpty())
                                   {
                                       user = WebUserService.InviteNew(
                                            firstName: firstname,
                                            lastName: lastname,
                                            email: email,
                                            loggedInUser: SessionVariables.CurrentUser,
                                            msgs: amsgs,
                                            dc: adc
                                        );
                                   }

                                   account = user.AddNewAccount(
                                        orgName: schoolName,
                                        phone: schoolPhone,
                                        isActive: true,
                                        accountType: AccountType.School,
                                        packageId: Package.Load(msgs: amsgs, dc: adc).First().ID.Value,
                                        packageBillingPeriod: BillingPeriodType.Month,
                                        timeZone: null,
                                        isAfterSchool: SessionVariables.IsAfterSchool.Value,
                                        loggedInUser: SessionVariables.CurrentUser,
                                        msgs: amsgs,
                                        dc: adc
                                    );
                               }


                               // load grade levels
                               var minGradeLevel = allGradeLevels.Where(grade =>
                                    (minGrade.Length == 1 &&
                                    grade.Name.ToLower().First() == minGrade.ToLower().First()) ||
                                    (minGrade.Length == 2 &&
                                    grade.Name.ToLower().MaxChars(2) == minGrade.ToLower().MaxChars(2))
                                ).FirstOrDefault();

                               var maxGradeLevel = allGradeLevels.Where(grade =>
                                    (maxGrade.Length == 1 &&
                                    grade.Name.ToLower().First() == maxGrade.ToLower().First()) ||
                                    (maxGrade.Length == 2 &&
                                    grade.Name.ToLower().MaxChars(2) == maxGrade.ToLower().MaxChars(2))
                                ).FirstOrDefault();

                               if (minGradeLevel.IsNotEmpty())
                                   account.MinGradeLevelOffered = minGradeLevel.ID;
                               if (maxGradeLevel.IsNotEmpty())
                                   account.MaxGradeLevelOffered = maxGradeLevel.ID;

                               amsgs.AddRange(account.Save(adc));

                               locationHostAccount = account;

                               // Adding the school to the district account so district can see details
                               amsgs.AddRange(
                                   districtAccount.AddVisitorAccount(
                                       account,
                                       HostPermissionType.Edit,
                                       ApprovalStatus.Approved,
                                       SessionVariables.CurrentUser,
                                       adc
                                   )
                               );

                               users.Add(user);
                               accounts.Add(account);

                           }
                           else
                           {
                               locationHostAccount = null;
                           }

                           if (address != "" && accountIdFromCSV.Length == 0)
                           {
                               LocationService.CreateNewWithValidation(
                               schoolName,
                               address,
                               address2,
                               city,
                               state,
                               postalCode,
                               country,
                               locationType,
                               locationHostAccount?.ID,
                               null,
                               SessionVariables.CurrentUser,
                               amsgs,
                               adc);
                           }
                       }

                   },
                   onSuccess: (SystemMessages smsgs) =>
                   {
                       msgs.AddRange(smsgs);

                       // var numbersAndWords = numbers.Zip(words, (n, w) => new { Number = n, Word = w });
                       var usersAndAccounts = users.Zip(accounts, (u, a) => new { user = u, account = a });
                       foreach (var userAndAccount in usersAndAccounts)
                       {
                           // msgs.AddRange(userAndAccount.account.SendInvitedEmail(userAndAccount.user, SessionVariables.CurrentUser, inviteMessage));
                       }
                   },
                   onFailure: (SystemMessages fmsgs) =>
                   {
                       msgs.AddRange(fmsgs);
                   }
               );
            }

            return msgs;
        }

        [HttpGet]
        public ActionResult LocationsSubmitted(SystemMessages msgs)
        {

            return View();
        }
    }


}