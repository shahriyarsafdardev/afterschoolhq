using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BasicLibrary;
using AfterSchoolLibrary;
using AfterSchool.Models;
using BasicLibrary.DataAccess;
using Hangfire;

namespace AfterSchool.Areas.Org.Controllers
{
    public class ContactController : BaseLoggedInController
    {
        // GET: Org/Contact
        public ActionResult Index(string pn, string rpp)
        {
            var registrants = new List<Registrant>();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;
            int? selectedList = Request["list"].IsNumber() ? Request["list"].ToInteger() : new int?();
            var query = Request["q"];

            totalCount = SessionVariables.CurrentAccount.SearchContactCount(Request["q"], selectedList);
            registrants = SessionVariables.CurrentAccount.SearchContacts(Request["q"], selectedList, start, length);

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Lists = ContactList.LoadByAccountId(SessionVariables.CurrentAccount.ID.Value).OrderBy(x => x.Name).ToList();
            ViewBag.SelectedList = selectedList;
            ViewBag.Query = query;
            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(registrants);
        }

        private void AddContacts(string step, string importtype, string emaillist, string createlist, string listname, string importtext, string listId)
        {
            var msgs = new SystemMessages();
            var csvMsgs = new SystemMessages();
            ViewBag.Step = 1;
            ViewBag.ContactLists = SessionVariables.CurrentAccount.GetContactLists();

            if (step.IsNumber())
            {
                switch (step.ToInteger())
                {
                    case 2:

                        // validate import
                        if (importtype.ToInteger() == 1)
                        {
                            if (Request.Files.Count == 0 || Request.Files[0].ContentLength == 0 || !Request.Files[0].FileName.Contains(".csv"))
                            {
                                msgs.Add("Please upload a valid CSV file", MessageType.UserError);
                            }
                            else
                            {
                                ViewBag.ImportText = ProcessImport(Request.Files[0], msgs);
                            }
                        }
                        else
                        {
                            if (emaillist.IsEmpty())
                            {
                                msgs.Add("Please enter a list of emails separated by commas", MessageType.UserError);
                            }
                            else if (emaillist.Split(',').Select(x => x.Trim()).Where(x => !x.IsMatch(BasicLibrary.Utilities.EmailRegEx)).Count() > 0)
                            {
                                msgs.Add("Invalid email in list. Please ensure all emails are valid", MessageType.UserError);
                            }
                            else
                            {
                                ViewBag.ImportText = emaillist.Split(',').Select(x => x.Trim()).Distinct().ToStringList(",");
                            }
                        }

                        if (msgs.HasErrors())
                        {
                            ViewBag.Step = 1;
                        }
                        else
                        {
                            ViewBag.Step = 2;
                        }

                        break;
                    case 3:

                        ContactList contactList = null;

                        // do save
                        Database.ExecuteTransaction
                        (
                            action: (SystemMessages amsgs, DatabaseConnection adc) =>
                            {
                                if (createlist.ToBoolean())
                                {
                                    if (listId.IsNumber())
                                    {
                                        contactList = ContactList.Load(listId.ToInteger());
                                    }
                                    else
                                    {
                                        contactList = ContactList.CreateNew(listname, SessionVariables.CurrentAccount.ID.Value, SessionVariables.LoggedInUser.ID.Value, amsgs, dc: adc);
                                    }
                                }

                                if (importtype.ToInteger() == 1)
                                {
                                    var file = importtext.Split('\n');
                                    var headers = file[0];

                                    for (var i = 1; i < file.Count(); i++)
                                    {
                                        string email = null;
                                        string firstname = null;
                                        string lastname = null;

                                        var row = file[i].Split(',').Select(x => x.Trim()).ToList();
                                        var headerList = headers.Split(',').Select(x => x.Trim()).ToList();
                                        if (row.Count == headerList.Count)
                                        {
                                            for (var x = 0; x < headerList.Count; x++)
                                            {
                                                if (emailHeaders.Contains(headerList[x]))
                                                {
                                                    email = row[x];
                                                }
                                                else if (firstNameHeaders.Contains(headerList[x]))
                                                {
                                                    firstname = row[x];
                                                }
                                                else if (lastNameHeaders.Contains(headerList[x]))
                                                {
                                                    lastname = row[x];
                                                }
                                            }

                                            amsgs.AddRange(AddContact(email, firstname, lastname, contactList, adc));
                                        }
                                    }

                                }
                                else
                                {
                                    foreach (var email in importtext.Split(',').Select(x => x.Trim()).Where(x => x.IsNotBlank()).Distinct())
                                    {
                                        amsgs.AddRange(AddContact(email, null, null, contactList, adc));                                       
                                    }
                                }
                            },
                            onSuccess: (SystemMessages smsgs) =>
                            {
                                msgs.AddRange(smsgs);
                                ViewBag.ContactList = contactList;
                            },
                            onFailure: (SystemMessages fmsgs) =>
                            {
                                msgs.AddRange(fmsgs);
                            }
                        );

                        if (msgs.HasErrors())
                        {
                            ViewBag.Step = 2;
                        }
                        else
                        {
                            ViewBag.Step = 3;
                        }
                        break;
                    case 1:
                    default:
                        break;
                }
            }

            ViewBag.Messages = msgs;
        }

        private SystemMessages AddContact(Registrant contact, ContactList contactList, DatabaseConnection dc = null)
        {
            return contactList.AddContact(contact.ID.Value, SessionVariables.LoggedInUser.ID.Value, dc: dc);
        }
        private SystemMessages AddContact(string email, string firstname, string lastname, ContactList contactList, DatabaseConnection dc = null)
        {
            var msgs = new SystemMessages();

            if(firstname.IsBlank() && lastname.IsBlank() && email.IsBlank())
            {
                msgs.Add("Invalid contact row", MessageType.Error);
                return msgs;
            }

            Registrant contact = null;

            if (firstname.IsNotEmpty() && lastname.IsNotEmpty())
            {
                contact = Registrant.LoadByNameByEmail(SessionVariables.CurrentAccount.ID.Value, firstname.Trim(), lastname.Trim(), email.Trim(), dc: dc);
            }
            else
            {
                contact = Registrant.LoadByEmailOnly(SessionVariables.CurrentAccount.ID.Value, email.Trim(), dc: dc);
            }

            if (contact.IsEmpty())
            {
                contact = new Registrant();
                contact.AccountId = SessionVariables.CurrentAccount.ID;
                contact.CreateDate = contact.LastModifiedDate = DateTime.UtcNow;
                contact.CreatedBy = contact.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                contact.IsContactOnly = true;
                contact.RegistrantTypeId = RegistrantType.Individual;
                contact.Email = email;
                contact.FirstName = firstname;
                contact.LastName = lastname;
                msgs.AddRange(contact.Save(dc: dc));
            }

            if (msgs.HasNoErrors() && contactList.IsNotEmpty())
            {
                AddContact(contact, contactList, dc: dc);                
            }
            return msgs;
        }

        private string[] emailHeaders { get { return new string[] { "email", "emailaddr", "emailaddress" }; } }
        private string[] firstNameHeaders { get { return new string[] { "first", "fname", "firstname" }; } }
        private string[] lastNameHeaders { get { return new string[] { "last", "lname", "lastname" }; } }

        private string ProcessImport(HttpPostedFileBase importfile, SystemMessages msgs)
        {
            var sr = new System.IO.StreamReader(importfile.InputStream);
            var extension = System.IO.Path.GetExtension(importfile.FileName);

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

                        if ((
                                emailHeaders.Contains(headerNoSpacesEtc) || 
                                firstNameHeaders.Contains(headerNoSpacesEtc) || 
                                lastNameHeaders.Contains(headerNoSpacesEtc)
                            ) && 
                            field.IsNotBlank())
                        {
                            row.Add(field);
                            if (!headersWithData.Contains(headerNoSpacesEtc))
                            {
                                headersWithData.Add(headerNoSpacesEtc);
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
            else if (headersWithData.Where(x => emailHeaders.Contains(x)).Count() == 0)
            {
                msgs.Add("Please add an email address to each record", MessageType.UserError);
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

                        var field = row[i].ToLower();
                        var headerName = headersWithData[i];

                        validatedField.Value = field;

                        if (field.Length > 150)
                        {
                            validatedField.Messages.Add("Must be less than or equal to 150 characters. Data will be truncated.", MessageType.Warning);
                        }

                        if (emailHeaders.Contains(headerName))
                        {
                            if (field.IsBlank() || !field.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                            {
                                hasErrors = true;
                                validatedField.Messages.Add("Must be a valid email address", MessageType.UserError);
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

            //ViewBag.Headers = headersWithData;
            ViewBag.Rows = validatedRows;
            //ViewBag.Csv = csvString.ToString();

            if (hasErrors)
            {
                msgs.Add("You have errors in your CSV file", MessageType.UserError);
            }

            return csvString.ToString();
        }

        [HttpGet]
        public ActionResult Add()
        {
            AddContacts("1", null, null, null, null, null, null);
            return View();
        }

        [HttpPost]
        public ActionResult Add(string step, string importtype, string emaillist, string createlist, string listname, string importtext, string contactlist)
        {
            AddContacts(step, importtype, emaillist, createlist, listname, importtext, contactlist);
            return View();
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            Registrant registrant = null;
            if (id.IsGuid())
            {
                registrant = Registrant.LoadByUid(id.ToGuid());
            }
            ViewBag.Messages = new SystemMessages();
            return View(registrant);
        }

        [HttpPost]
        public ActionResult Edit(string id, string firstname, string lastname, string email)
        {
            Registrant registrant = null;
            var msgs = new SystemMessages();

            if (id.IsGuid())
            {
                if (email.IsEmpty() || !email.IsMatch(BasicLibrary.Utilities.EmailRegEx))
                {
                    msgs.Add("Email is invalid", MessageType.UserError);
                }

                registrant = Registrant.LoadByUid(id.ToGuid());
                registrant.FirstName = firstname;
                registrant.LastName = lastname;
                registrant.Email = email;
                registrant.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                registrant.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(registrant.Save());
            }

            ViewBag.Messages = msgs;

            return View(registrant);
        }

        [HttpGet]
        public ActionResult Lists(string pn, string rpp)
        {
            var lists = new List<ContactList>();
            int pageNumber = 1;
            int recordsPerPage = 10;

            if (pn.IsNumber() && pn.ToInteger() > 0)
            {
                pageNumber = pn.ToInteger();
            }
            if (rpp.IsNumber() && rpp.ToInteger() > 0)
            {
                recordsPerPage = rpp.ToInteger();
            }

            var start = recordsPerPage * (pageNumber - 1);
            var length = recordsPerPage;
            var totalCount = 0;

            totalCount = SessionVariables.CurrentAccount.GetContactListCount();
            lists = SessionVariables.CurrentAccount.GetContactLists(start, length);

            var totalPages = Math.Ceiling(totalCount.ToDouble() / recordsPerPage).ToInteger();

            ViewBag.Groups = SessionVariables.CurrentAccount.GetSessions(false, null, null);
            ViewBag.Messages = new SystemMessages();
            ViewBag.PagingModel = new PagingModel()
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                RecordsPerPage = recordsPerPage,
                TotalPages = totalPages
            };

            return View(lists);
        }

        [HttpGet]
        public ActionResult DeleteList(string id)
        {
            var msgs = new SystemMessages();
            if (id.IsNumber())
            {
                var list = ContactList.Load(id.ToInteger());
                if(list.IsNotEmpty() && list.AccountId == SessionVariables.CurrentAccount.ID)
                {
                    msgs.AddRange(list.Delete());
                }
                else
                {
                    msgs.Add("Invalid request", MessageType.UserError);
                }
            }

            return RedirectToAction("lists");
         
        }
        
        [HttpPost]
        public ActionResult ListEdit(string id, string list, string listname)
        {
            var msgs = new SystemMessages();
            var contactList = new ContactList();

            if (list.IsInteger())
            {
                contactList = ContactList.Load(list.ToInteger());
                contactList.Name = listname;
                contactList.LastModifiedBy = SessionVariables.LoggedInUser.ID;
                contactList.LastModifiedDate = DateTime.UtcNow;
                msgs.AddRange(contactList.Save());
            }
            else
            {
                contactList = ContactList.CreateNew(listname, SessionVariables.CurrentAccount.ID.Value, SessionVariables.LoggedInUser.ID.Value, msgs);
            }

            return RedirectToAction("lists");
        }

        [HttpGet]
        public ActionResult InviteTo(string id, string type, string list)
        {
            InviteToViewBag(id, type, list);
            return View();
        }

        private void InviteToViewBag(string id, string type, string list)
        {
            var returnUrl = "";
            var activities = new List<Activity>();

            switch (type)
            {
                case "period":
                    var period = RegistrationPeriod.LoadByUid(id.ToGuid());
                    returnUrl = Url.Action("periods", "group", new { id = period.GetGroup().Uid });

                    if (period.ActivityGroupId.HasValue)
                    {
                        activities.AddRange(period.GetGroup().GetActivities(false, false, null, null));
                    }

                    break;
                case "groupfromlists":
                    var group = ActivityGroup.LoadByUid(id.ToGuid());
                    returnUrl = Url.Action("lists", "contact");
                    activities.AddRange(group.GetActivities(false, false, null, null));

                    break;
            }

            ViewBag.ReturnUrl = returnUrl;
            ViewBag.SelectedList = list.IsNumber() ? list.ToInteger() : new int?();
            ViewBag.Id = id;
            ViewBag.Type = type;
            ViewBag.Activities = activities;
            ViewBag.Messages = new SystemMessages();
            ViewBag.Lists = SessionVariables.CurrentAccount.GetContactLists();
            ViewBag.Done = false;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult InviteTo(string id, string type, string list, string invitemessage)
        {
            var msgs = new SystemMessages();
            ActivityGroup group = null;
            RegistrationPeriod period = null;

            switch (type)
            {
                case "period":

                    period = RegistrationPeriod.LoadByUid(id.ToGuid());
                    group = period.GetGroup();

                    msgs.AddRange(InviteContactList(list, invitemessage, period, group));

                    ViewBag.ReturnUrl = Url.Action("periods", "group", new { id = group.Uid });

                    break;

                case "groupfromlists":
                    
                    group = ActivityGroup.LoadByUid(id.ToGuid());
                    period = group.GetCurrentRegistrationPeriod();

                    msgs.AddRange(InviteContactList(list, invitemessage, period, group));

                    ViewBag.ReturnUrl = Url.Action("lists", "contact");
                    break;
            }

            if (msgs.HasNoErrors())
            {
                ViewBag.Messages = msgs;
                return View("InviteToDone");
            }
            else
            {
                InviteToViewBag(id, type, list);
                return View();
            }
        }

        private SystemMessages InviteContactList(string list, string invitemessage, RegistrationPeriod period, ActivityGroup group)
        {
            var msgs = new SystemMessages();
            var contactList = ContactList.Load(list.ToInteger());
            List<Registrant> contacts = null;

            if (contactList.AccountId != SessionVariables.CurrentAccount.ID)
                msgs.Add("Invalid contact list", MessageType.UserError);

            if (msgs.HasNoErrors())
            {
                Database.ExecuteTransaction
                (
                    action: (SystemMessages amsgs, DatabaseConnection adc) =>
                    {
                        var activityIds = group.GetActivities(false, false, null, null, false, adc)
                            .Where(x => Request["activity-" + x.ID.Value.ToString()].IsNotBlank())
                            .Select(x => x.ID.Value)
                            .ToList();

                        contacts = contactList.GetContacts(adc);

                        amsgs.AddRange(group.InviteContacts(contacts, period, activityIds, SessionVariables.LoggedInUser.ID.Value, adc));
                    },
                    onSuccess: (SystemMessages smsgs) =>
                    {
                        // get session information because it isn't available in a background task
                        var account = SessionVariables.CurrentAccount;
                        var loggedInUser = SessionVariables.LoggedInUser;

                        BackgroundJob.Enqueue(() =>
                            ActivityGroup.EmailInvitedContacts(
                                group.ID.Value,
                                period.ID.Value,
                                contacts.Select(x => x.ID.Value).ToList(),
                                invitemessage,
                                group.HomePageUrl,
                                loggedInUser.ID.Value,
                                null)
                        );

                        msgs.Add("Your invites will be sent shortly", MessageType.Success);
                    },
                    onFailure: (SystemMessages fmsgs) =>
                    {
                        msgs.AddRange(fmsgs);
                    }
                );
            }
            return msgs;
        }
    }
}