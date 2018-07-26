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


namespace AfterSchool.Areas.Admin.Controllers
{
    public class UploadController : Controller
    {
        // GET: Admin/Upload
        [HttpGet]
        public ActionResult Index()
        {
           AddContacts("1", null, null, null, null, null, null);
            return View();
        }

        [HttpPost]
        public ActionResult Index(string step, string importtype, string emaillist, string createlist, string listname, string importtext, string listId)
        {
            AddContacts("2", importtype, emaillist, createlist, listname,importtext, listId);
            return View();
        }

        private void AddContacts(string step, string importtype, string emaillist, string createlist, string listname, string importtext, string listId)
        {
            var msgs = new SystemMessages();
            var csvMsgs = new SystemMessages();
            ViewBag.Step = 1;
            ViewBag.ContactLists = SessionVariables.CurrentAccount.GetContactLists();

            if (step == "1")
            {
                ViewBag.ImportText = "im import text";
            }
            else if (Request.Files.Count == 0 || Request.Files[0].ContentLength == 0 || !Request.Files[0].FileName.Contains(".csv"))
            {
                msgs.Add("Please upload a valid CSV file", MessageType.UserError);
            }
            else 
            {
                ViewBag.ImportText = ProcessImport(Request.Files[0], msgs);
            }                      

            ViewBag.Messages = msgs;
        }

        private string[] schoolHeaders { get { return new string[] { "school", "schoolname", "name" }; } }
        private string[] gradeHeaders { get { return new string[] { "grades", "gradelevels" }; } }
        private string[] phoneHeaders { get { return new string[] { "phone", "phonenumber", "phoneno", "telephone"  }; } }
        private string[] addressHeaders { get { return new string[] { "address", "zip" }; } }
        private string[] cityHeaders { get { return new string[] { "city", "town", "cityname" }; } }
        private string[] stateHeaders { get { return new string[] { "state", "statename" }; } }
        private string[] zipHeaders { get { return new string[] { "zip", "zipcode" }; } }
        private string[] websiteHeaders { get { return new string[] { "website", "site", "url", "webpage" }; } }

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
                            if(headerName.Contains(headerNoSpacesEtc) && field.IsNotBlank())
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

            foreach(var headerName in headerList)
            {
                if(headersWithData.Where(x => schoolHeaders.Contains(x)).Count() == 0)
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

                        var field = row[i].ToLower();
                        var headerName = headersWithData[i];

                        validatedField.Value = field;

                        if (field.Length > 150)
                        {
                            validatedField.Messages.Add("Must be less than or equal to 150 characters. Data will be truncated.", MessageType.Warning);
                        }

                        if (phoneHeaders.Contains(headerName))
                        {
                            if (field.IsBlank() || !field.IsMatch(BasicLibrary.Utilities.PhoneRegEx))
                            {
                                hasErrors = true;
                                validatedField.Messages.Add("Must be a valid phone number", MessageType.UserError);
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
    }
}